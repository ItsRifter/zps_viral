using Sandbox;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using ZPS_Viral.Entities;
using Trace = Sandbox.Trace;

namespace ZPS_Viral
{
	public partial class ZPSVPlayer : Player
	{

		public enum TeamType
		{
			Unassigned,
			Survivor,
			Infected,
			Undead,
			Spectator
		}

		public enum ZombieType
		{
			Standard,
			Carrier
		}

		private TimeSince timeSinceDropped;
		private TimeSince timeSinceDied;
		private TimeSince timeForPanic;
		private TimeSince timeSinceLastHit;
		private TimeSince timeTillNextRegen;

		public bool IsPanicked;
		
		private DamageInfo lastDamage;

		public int ArmorPoints { get; set; }

		public float InfectionTime { get; set; } = 25f;

		public bool phaseInfection1 { get; set; }
		private bool phaseInfection2;

		public float FeedBar = 5;
		public bool BerserkMode;
		
		[Net]
		public TeamType CurTeam { get; set; }

		[Net]
		public ZombieType CurZombieType { get; set; }

		private int RegenAmount = 5;
		
		public int CurWeightSlots;
		private int CarryAllowance = 4;

		public float CurAmmoWeight;
		private float CarryAmmoAllowance = 30f;

		public string AmmoTypeToDrop = "pistol";

		private List<WeaponBase> curWeapons;
		private List<ItemBase> curAmmo;
		public ZPSVPlayer()
		{
			Inventory = new Inventory( this );
			curWeapons = new List<WeaponBase>();
			curAmmo = new List<ItemBase>();
		}


		public void InitialSpawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			Controller = new SurvivorWalkController();
			Animator = new StandardPlayerAnimator();
			CameraMode = new FirstPersonCamera();

			//Dress();
			ClearAmmo();
			
			CurWeightSlots = 0;
			CurAmmoWeight = 0;
			RenderColor = new Color(255, 255, 255, 1);
			
			timeForPanic = 0;
			
			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			EnableAllCollisions = true;

			base.Respawn();
			flashlight = new Flashlight();

			SwapTeam( TeamType.Unassigned );
		}

		public void GiveWeapons()
		{
			curAmmo.Clear();
			curWeapons.Clear();
			
			Inventory.DeleteContents();
			if ( CurTeam == TeamType.Undead )
			{
				Inventory.Add( new Claws(), true );
			} else {
				int RandMelee = Rand.Int( 1, 2 );
				if ( RandMelee == 1 )
				{
					var axe = new Axe();
					CurWeightSlots += axe.WeightSlots;
					curWeapons.Add( axe );
					Inventory.Add( axe );
				} else if ( RandMelee == 2 )
				{
					var machete = new Machete();
					CurWeightSlots += machete.WeightSlots;	
					curWeapons.Add( machete );
					Inventory.Add( machete );
				}
				
				int RandFirearm = Rand.Int( 2, 2 );
				if ( RandFirearm == 1 )
				{
					var usp = new USP();
					CurWeightSlots += usp.WeightSlots;
					curWeapons.Add( usp );
					Inventory.Add( usp, true );
				} else if ( RandFirearm == 2 )
				{
					var glock17 = new Glock17();
					CurWeightSlots += glock17.WeightSlots;	
					curWeapons.Add( glock17 );
					Inventory.Add( glock17, true );
				}
				
				var pistolammo = new PistolAmmo();
				pistolammo.shouldBePickedUpFirst = true;
				pistolammo.Position = Position;
				
				using ( Prediction.Off() )
				{
					UpdateAmmoClient( To.Single( this ), CurAmmoWeight );
				}

				timeForPanic = 0;
				
				flashlight = new Flashlight();
				flashlight.LightEnabled = true;
			}
		}

		public override void StartTouch( Entity other )
		{
			if ( other is PistolAmmo ammo )
			{
				if ( ammo.shouldBePickedUpFirst )
				{
					CurAmmoWeight += ammo.Weight;
					curAmmo.Add( ammo );
					
					using ( Prediction.Off() )
					{
						SetAmmoClient( To.Single( this ), CurAmmoWeight );
					}
					
					base.StartTouch( other );
				}
			}
				
		}


		public override void Respawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			CameraMode = new FirstPersonCamera();
			Controller = new ZombieWalkController();
			Animator = new StandardPlayerAnimator();

			ClearAmmo();
			
			CurAmmoWeight = 0f;
			
			using ( Prediction.Off() )
			{
				UpdateAmmoClient( To.Single( this ), CurAmmoWeight );
			}
			
			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			EnableAllCollisions = true;

			base.Respawn();

			if(CurTeam == TeamType.Spectator)
			{
				EnableAllCollisions = false;
				RenderColor = new Color( 255, 255, 255, 0 );

			} else
			{
				EnableAllCollisions = true;
				RenderColor = new Color( 255, 255, 255, 1 );
			}

			if ( CurTeam == TeamType.Undead )
			{
				//vision = new ZombieVision();
				//EnableVision( true );
				
				Health = 200;
				
				if( CurZombieType == ZombieType.Carrier )
                {
                	RenderColor = new Color( 255, 140, 140 );
                    Health = 250;
                }
    
                List<Vector3> vectorSpawns = new List<Vector3>();
                foreach(var ZombiePoints in All.OfType<ZombiePoint>())
                {
                	vectorSpawns.Add( ZombiePoints.Position );
                }
                Position = vectorSpawns[Rand.Int(0, vectorSpawns.Count - 1)];
                GiveWeapons();
			}
		}

		public void SwitchToBestWeapon()
		{
			var best = Children.Select( x => x as WeaponBase )
				.Where( x => x.IsValid() && x.IsUsable() )
				.OrderBy( x => x.BucketWeight )
				.FirstOrDefault();

			if ( best == null ) return;

			ActiveChild = best;
			
		}
		
		public override void Simulate( Client cl )
		{
			SimulateActiveChild( cl, ActiveChild );
			
			if (ZPSVGame.CurState != ZPSVGame.RoundState.Active && CurTeam != TeamType.Unassigned)
				return;
			
			var controller = GetActiveController();
			controller?.Simulate( cl, this, GetActiveAnimator() );
			
			
			
			if ( timeForPanic >= 7 && IsPanicked )
			{
				IsPanicked = false;
				
				using ( Prediction.Off() )
					SetPanicMode( To.Single( this ), false );
				
				
				Controller = new SurvivorWalkController();
			}
			
			TickPlayerUse();

			if ( LifeState == LifeState.Dead )
			{
				if(timeSinceDied > 6 && IsServer && ZPSVGame.CurState == ZPSVGame.RoundState.Active)
					Respawn();
			}
			
			bool toggle = Input.Pressed( InputButton.Flashlight );

			if ( flashlight.IsValid() )
			{
				if ( CurTeam != TeamType.Survivor )
					return;
				
				if ( flashlight.timeSinceLightToggled > 0.1f && toggle )
				{
					
					flashlight.LightEnabled = !flashlight.LightEnabled;

					EnableFlashlight( flashlight.LightEnabled );

					flashlight.timeSinceLightToggled = 0;
					
				}
			} else if ( vision.IsValid() )
			{
				if ( CurTeam != TeamType.Undead )
					return;
				
				if ( vision.timeSinceLightToggled > 0.1f && toggle )
				{
					
					vision.LightEnabled = !vision.LightEnabled;

					EnableVision( vision.LightEnabled );

					vision.timeSinceLightToggled = 0;
					
				}
			}
			

			if ( Input.Pressed( InputButton.Drop ) && IsServer )
			{
				var weapon = Inventory.GetSlot( Inventory.GetActiveSlot() ) as WeaponBase;

				if ( weapon == null || weapon.IsDroppable == false )
					return;
				
				CurWeightSlots -= weapon.WeightSlots;
				curWeapons.Remove( weapon );
				
				var dropped = Inventory.DropActive();

				if ( dropped != null )
				{
					dropped.PhysicsGroup.ApplyImpulse( Velocity + EyeRotation.Forward * 250.0f + Vector3.Up * 100.0f, true );
					dropped.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100.0f, true );

					timeSinceDropped = 0;
					SwitchToBestWeapon();
				}
			} 
			
			if ( Input.Pressed( InputButton.Use ) && IsServer && CurTeam == TeamType.Survivor )
			{
				var tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 92 )
					.UseHitboxes( )
					.Ignore( this )
					.Size( 7)
					.EntitiesOnly()
					.Run();

				if ( !tr.Entity.IsValid() )
					return;
				
				if ( tr.Entity is WeaponBase weapon )
				{
					int checkWeight = CurWeightSlots + weapon.WeightSlots;
					
					if ( checkWeight > CarryAllowance )
						return;

					bool hasWeapon = false;


					for ( int i = 0; i < curWeapons.Count; i++ )
					{
						if ( curWeapons[i].CheckIndex == weapon.CheckIndex)
							hasWeapon = true;
					}
					
					if ( hasWeapon )
						return;

					curWeapons.Add( weapon );
					Inventory.Add( weapon );
					
					CurWeightSlots += weapon.WeightSlots;
					
					if(Inventory.Count() <= 1)
						SwitchToBestWeapon();
						
					using ( Prediction.Off() )
					{
						PlaySound( "weapon_pickup" );
					}
					
				}
				
				if ( tr.Entity is ItemBase item )
				{
					
					float checkWeight = CurAmmoWeight + item.Weight;

					if ( checkWeight >= CarryAmmoAllowance )
						return;
					
					item.OnCarryStart( this );
					
					curAmmo.Add( item );
					CurAmmoWeight += item.Weight;
					
					using ( Prediction.Off() )
					{
						UpdateAmmoClient( To.Single( this ), item.Weight );
					}
				}
			}

			if ( Input.Pressed( InputButton.Zoom ) && IsServer )
			{
				if ( IsClient )
					return;
				
				if ( timeSinceDropped < 0.5f ) 
					return;

				DropAmmoType();
			}

			if ( Input.Pressed( InputButton.View ) && IsServer )
			{
				
				if(AmmoTypeToDrop == "pistol")
					AmmoTypeToDrop = "buckshot";	
				else if ( AmmoTypeToDrop == "buckshot" )
					AmmoTypeToDrop = "rifle";
				else if( AmmoTypeToDrop == "rifle")
					AmmoTypeToDrop = "magnum";
				else if( AmmoTypeToDrop == "magnum")
					AmmoTypeToDrop = "pistol";

				using ( Prediction.Off() )
				{
					SetDropOnClient( To.Single( this ), AmmoTypeToDrop );
				}
			}
			
			if ( Input.Pressed( InputButton.Run ) && CurTeam == TeamType.Undead && FeedBar > 0f )
			{
				FeedBar -= 0.5f;
			}
			
			if ( Input.Pressed( InputButton.Slot0 ) && timeForPanic >= 60 && CurTeam == TeamType.Survivor)
				StartPanic();
			
			if ( Input.ActiveChild != null )
			{
				ActiveChild = Input.ActiveChild;
			}
		}

		public void StartPanic()
		{
			IsPanicked = true;
			
			using ( Prediction.Off() )
			{
				SetPanicMode( To.Single( this ), true );
				
				PlaySound( "panic" );
				Sound.FromScreen( "heartbeat" );
			}

			foreach ( var drop in curWeapons )
			{
				CurWeightSlots -= drop.WeightSlots;
				Inventory.Drop( drop );
			}
			
			curWeapons.Clear();
			
			Controller = new PanicWalkController();
			
			timeForPanic = 0;
		}

		[ClientRpc]
		private void SetPanicMode( bool isPanic )
		{
			IsPanicked = isPanic;
		}
		
		
		public void RecheckAmmoWeight(int removeAmmo, AmmoType ammoType)
		{
			if ( !IsServer )
				return;

			int remainToRemove = removeAmmo;
			
			int pistolCount = 0;
			int shotgunCount = 0;
			int rifleCount = 0;
			int magnumCount = 0;

			List<PistolAmmo> pistol = new List<PistolAmmo>();
			List<RifleAmmo> rifle = new List<RifleAmmo>();
			
			foreach ( var ammo in curAmmo )
			{
				Log.Info(ammo);
				Log.Info(ammo.ToString().Contains("PistolAmmo"));
				
				if ( ammo.ToString().Contains("PistolAmmo") )
				{
					pistolCount++;
					pistol.Add( new PistolAmmo() );
				}
				
				if( ammo.ToString().Contains("ShotgunAmmo") )
					shotgunCount++;
				
				if( ammo.ToString().Contains("RifleAmmo") )
				{
					rifleCount++;
					rifle.Add( new RifleAmmo() );
				}
				if( ammo.ToString().Contains("MagnumAmmo") )
					magnumCount++;
			}

			if ( ammoType == AmmoType.Pistol )
			{
				foreach ( var curPistols in pistol )
				{
					if ( remainToRemove < 1 )
						break;
					
					remainToRemove -= curAmmo[pistolCount].RemainingAmmo; 
					curAmmo[pistolCount].RemainingAmmo -= remainToRemove;
				}
				
				
			}
				
			else if ( ammoType == AmmoType.Buckshot )
			{
				remainToRemove -= curAmmo[shotgunCount].RemainingAmmo;
				curAmmo[shotgunCount].RemainingAmmo -= remainToRemove;
			}
			else if ( ammoType == AmmoType.Rifle )
			{
				remainToRemove -= rifle[rifleCount].RemainingAmmo;
				rifle[rifleCount].RemainingAmmo -= remainToRemove;
			}
			else if ( ammoType == AmmoType.Magnum )
			{
				remainToRemove -= curAmmo[magnumCount].RemainingAmmo;
				curAmmo[magnumCount].RemainingAmmo -= remainToRemove;
			}
			
			if ( ammoType == AmmoType.Pistol && curAmmo[pistolCount].RemainingAmmo <= 0 )
			{
				CurAmmoWeight -= curAmmo[pistolCount].Weight;
                
                using ( Prediction.Off() )
                {
                    RemoveAmmoClient( To.Single( this ), curAmmo[pistolCount].Weight );
                }
                
                curAmmo.RemoveAt(pistolCount);
                pistolCount--;

                //Because of the way 9mm ammo works with mp5
                //This will run if remaining ammo to remove is still greater than 0
                while ( remainToRemove > 1 )
                {
	                remainToRemove -= curAmmo[pistolCount].RemainingAmmo;
	                CurAmmoWeight -= curAmmo[pistolCount].Weight;
                
	                using ( Prediction.Off() )
	                {
		                RemoveAmmoClient( To.Single( this ), curAmmo[pistolCount].Weight );
	                }
                
	                curAmmo.RemoveAt(pistolCount);
	                pistolCount--;
                }
			}
			
			if ( ammoType == AmmoType.Buckshot && curAmmo[shotgunCount].RemainingAmmo <= 0 )
			{
				CurAmmoWeight -= curAmmo[shotgunCount].Weight;

				using ( Prediction.Off() )
				{
					RemoveAmmoClient( To.Single( this ), curAmmo[shotgunCount].Weight );
				}	
				
				curAmmo.RemoveAt(shotgunCount);
			}
			
			if ( ammoType == AmmoType.Rifle && curAmmo[rifleCount].RemainingAmmo <= 0 )
			{
				
				CurAmmoWeight -= curAmmo[rifleCount].Weight;
                
				using ( Prediction.Off() )
				{
					RemoveAmmoClient( To.Single( this ), curAmmo[rifleCount].Weight );
				}
                
				curAmmo.RemoveAt(rifleCount);
			}
			
			if ( ammoType == AmmoType.Magnum && curAmmo[magnumCount].RemainingAmmo <= 0 )
			{
				CurAmmoWeight -= curAmmo[magnumCount].Weight;
				
				
				using ( Prediction.Off() )
				{
					RemoveAmmoClient( To.Single( this ), curAmmo[magnumCount].Weight );
				}
				
				curAmmo.RemoveAt(magnumCount);
			}
			
			
		}
		
		[ClientRpc]
		public void UpdateArmorClient(int amount)
		{
			ArmorPoints += amount;
		}

		[ClientRpc]
		public void UpdateAmmoClient(float amount)
		{
			CurAmmoWeight += amount;
		}
		
		[ClientRpc]
		public void RemoveAmmoClient(float amount)
		{
			CurAmmoWeight -= amount;
		}
		
		[ClientRpc]
		public void SetAmmoClient(float amount)
		{
			CurAmmoWeight = amount;
		}
		
		
		[ClientRpc]
		public void SetDropOnClient(string ammoType)
		{
			AmmoTypeToDrop = ammoType;
		}
		
		
		[Event( "server.tick" )]
		public void Regeneration()
		{
			if ( CurTeam != TeamType.Undead )
				return;

			if ( LifeState == LifeState.Dead )
				return;

			bool isFullHP = false;
			
			if ( timeSinceLastHit >= 15)
			{
				if(timeTillNextRegen <= 4)
					return;

				if ( CurZombieType == ZombieType.Carrier )
				{
					if ( Health > 250 )
					{
						Health = 250;
						isFullHP = true;
					}
				}
				else
				{
					if ( Health > 200 )
					{
                    	Health = 200;
                        isFullHP = true;
                    }
				}

				if ( !isFullHP )
				{
					Health += RegenAmount;
					timeTillNextRegen = 0;
				}
			}
		}
		
		
		public void DropAmmoType()
		{
			if ( AmmoTypeToDrop == "pistol" && AmmoCount( AmmoType.Pistol ) >= 12 )
			{
				var pistolAmmo = new PistolAmmo();

				pistolAmmo.Position = Position + new Vector3( 0, 0, 85 );
				
				pistolAmmo.PhysicsGroup.ApplyImpulse( EyeRotation.Forward * 250.0f + Vector3.Up * 100.0f, true );
				pistolAmmo.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100.0f, true );

				
				CurAmmoWeight -= pistolAmmo.Weight;
				curAmmo.Remove( pistolAmmo );
				
				using ( Prediction.Off() )
				{
					UpdateAmmoClient( To.Single( this ), -pistolAmmo.Weight );
				}

				TakeAmmo( AmmoType.Pistol, 15 );
				timeSinceDropped = 0;
					
			} else if ( AmmoTypeToDrop == "buckshot" && AmmoCount( AmmoType.Buckshot ) >= 6 )
			{
				var shotgunAmmo = new ShotgunAmmo();

				shotgunAmmo.Position = Position + new Vector3( 0, 0, 85 );

				shotgunAmmo.PhysicsGroup.ApplyImpulse( EyeRotation.Forward * 250.0f + Vector3.Up * 100.0f, true );
				shotgunAmmo.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100.0f, true );

				CurAmmoWeight -= shotgunAmmo.Weight;
				curAmmo.Remove( shotgunAmmo );
				using ( Prediction.Off() )
				{
					UpdateAmmoClient( To.Single( this ), -shotgunAmmo.Weight );
				}

				TakeAmmo( AmmoType.Buckshot, 6 );
				timeSinceDropped = 0;
					
			} else if ( AmmoTypeToDrop == "rifle" && AmmoCount( AmmoType.Rifle ) >= 30 )
			{
				var rifleAmmo = new RifleAmmo();

				rifleAmmo.Position = Position + new Vector3( 0, 0, 85 );

				rifleAmmo.PhysicsGroup.ApplyImpulse( EyeRotation.Forward * 250.0f + Vector3.Up * 100.0f, true );
				rifleAmmo.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100.0f, true );

				CurAmmoWeight -= rifleAmmo.Weight;
				curAmmo.Remove( rifleAmmo );
				
				using ( Prediction.Off() )
				{
					UpdateAmmoClient( To.Single( this ), -rifleAmmo.Weight );
				}
				
				TakeAmmo( AmmoType.Rifle, 30 );
				timeSinceDropped = 0;
				
			} else if ( AmmoTypeToDrop == "magnum" && AmmoCount( AmmoType.Magnum ) >= 6 )
			{
				var magnumAmmo = new MagnumAmmo();

				magnumAmmo.Position = Position + new Vector3( 0, 0, 85 );

				magnumAmmo.PhysicsGroup.ApplyImpulse( EyeRotation.Forward * 250.0f + Vector3.Up * 100.0f, true );
				magnumAmmo.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100.0f, true );

				CurAmmoWeight -= magnumAmmo.Weight;
				curAmmo.Remove( magnumAmmo );
				
				using ( Prediction.Off() )
				{
					UpdateAmmoClient( To.Single( this ), -magnumAmmo.Weight );
				}
				
				TakeAmmo( AmmoType.Magnum, 6 );
				timeSinceDropped = 0;
			}
		}

		[Event("server.tick")]
		public void InfectionThink()
		{
			if(ZPSVGame.CurState != ZPSVGame.RoundState.Active)
			{
				InfectionTime = 25f;
				return;
			}

			if ( CurTeam == TeamType.Infected )
			{
				if (InfectionTime > 0f )
				{
					InfectionTime -= 0.01f;
				}
			}

			if ( CurTeam == TeamType.Infected && InfectionTime < 13f && phaseInfection1 == false )
			{
				using ( Prediction.Off() )
				{
					Sound.FromScreen( "infected" );
				}
				
				phaseInfection1 = true;
			}

			if ( CurTeam == TeamType.Infected && InfectionTime < 3.9f && phaseInfection2 == false )
			{
				using ( Prediction.Off() )
				{
					Sound.FromScreen( "turning" );
				}
				phaseInfection2 = true;
			}

			if ( CurTeam == TeamType.Infected && InfectionTime <= 0f )
			{
				SwapTeam( TeamType.Undead );
				EnableFlashlight( false );
				GiveWeapons();
				phaseInfection1 = false;
				phaseInfection2 = false;
				DropEverything();
			}
		}



		public void SwapTeam( TeamType targetTeam )
		{
			CurTeam = targetTeam;

			if ( targetTeam == TeamType.Undead )
			{
				RenderColor = new Color( 125, 75, 75 ); 
			}
			else
			{
				RenderColor = new Color( 255, 255, 255 );
			}

			using ( Prediction.Off() )
			{
				SetTeamOnClient( To.Single(this), targetTeam );
			}

			ZPSVGame.CheckRoundStatus();
		}

		[ClientRpc]
		public void SetTeamOnClient( TeamType targetTeam )
		{
			CurTeam = targetTeam;
		}

		public float ScaleDamageHitBox(DamageInfo info)
		{
			float scale = 1;
			
			//Head
			if ( info.HitboxIndex == 5 )
				scale = 2f;
			
			//Chest
			if ( info.HitboxIndex == 3 )
				scale = 1.25f;

			//Arms
			if ( info.HitboxIndex == 7 || info.HitboxIndex == 11 )
				scale = 0.95f;
			
			//Legs
			if ( info.HitboxIndex == 14 || info.HitboxIndex == 17 )
				scale = 0.75f;
			
			//Feet
			if ( info.HitboxIndex == 15 || info.HitboxIndex == 18 || info.HitboxIndex == 19 )
				scale = 0.5f;
			
			return info.Damage * scale;
		}
		

		public override void TakeDamage( DamageInfo info )
		{
			var attacker = info.Attacker as ZPSVPlayer;
			
			if(CurTeam == TeamType.Undead)
				info.Damage = ScaleDamageHitBox(info);

			if ( ZPSVGame.CurState != ZPSVGame.RoundState.Active )
				return;

			if ( attacker.IsValid() && attacker.CurTeam == CurTeam )
				return;
			
			if( CurTeam == TeamType.Survivor && attacker.IsValid() && attacker.CurTeam == TeamType.Undead && attacker.CurZombieType == ZombieType.Carrier )
			{
				if ( Rand.Int( 0, 100 ) >= ZPSVGame.InfectionChance )
					InfectPlayer();
			}
			
			if ( ArmorPoints > 0 )
			{
				int reduction = ArmorPoints - 20;
				
				ArmorPoints -= reduction;
				
				if ( ArmorPoints <= 0 )
				{
					ArmorPoints = 0;
				}
			}
			
			if ( CurTeam == TeamType.Undead && CurZombieType == ZombieType.Carrier )
			{
				PlaySound( "carrier_pain" );	
				timeSinceLastHit = 0;
			}	
			else if ( CurTeam == TeamType.Undead && CurZombieType == ZombieType.Standard )
			{
				PlaySound( "carrier_pain" );
				timeSinceLastHit = 0;
			}
			
			base.TakeDamage( info );

		}
		
		public void InfectPlayer()
		{
			if ( CurTeam != TeamType.Survivor )
				return;

			SwapTeam( TeamType.Infected );
		}

		public void DropEverything()
		{
			foreach ( var drop in curWeapons )
			{
				if ( drop.IsDroppable == false )
					return;

				//Melee
				if ( drop.ToString() == "Machete" )
				{
					var dropEnt = new Machete();
					dropEnt.Position = Position;
				}
				
				if ( drop.ToString() == "Axe" )
				{
					var dropEnt = new Axe();
					dropEnt.Position = Position;
				}
				
				//Pistols
				if ( drop.ToString() == "USP" )
				{
					var dropEnt = new USP();
					dropEnt.Position = Position;
				}

				if ( drop.ToString() == "Glock17" )
				{
					var dropEnt = new Glock17();
					dropEnt.Position = Position;
				}
				
				if ( drop.ToString() == "Revolver" )
				{
					var dropEnt = new Revolver();
					dropEnt.Position = Position;
				}

				//Shotguns
				if ( drop.ToString() == "Remington" )
				{
					var dropEnt = new Remington();
					dropEnt.Position = Position;
				}

				//Assault Rifles
				if ( drop.ToString() == "AK47" )
				{
					var dropEnt = new AK47();
					dropEnt.Position = Position;
				}
			}
		}

		public override void OnKilled()
		{
			//base.OnKilled();
			
			Game.Current?.OnKilled( this );

			EnableFlashlight( false );
			
			timeSinceDied = 0;
			LifeState = LifeState.Dead;

			StopUsing();
			
			EnableAllCollisions = false;

			BecomeRagdollOnClient( Velocity, lastDamage.Flags, lastDamage.Position, lastDamage.Force, GetHitboxBone( lastDamage.HitboxIndex ) );

			if ( CurTeam != TeamType.Undead )
			{
				SwapTeam( TeamType.Undead );
				ZPSVGame.ZombieLives++;

			} else if (CurTeam == TeamType.Undead)
			{
				if ( ZPSVGame.ZombieLives > 0 )
					ZPSVGame.ZombieLives--;
				else
					SwapTeam( TeamType.Spectator );

				if ( CurZombieType == ZombieType.Carrier )
					PlaySound( "carrier_death" );

			}

			DropEverything();
			
			Inventory.DeleteContents();
			ZPSVGame.CheckRoundStatus();
			CameraMode = new SpectateRagdollCamera();
			EnableDrawing = false;
		}
	}
}
