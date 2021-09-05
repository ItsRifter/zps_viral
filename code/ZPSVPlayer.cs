﻿using Sandbox;
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
		private TimeSince timeSinceJumpReleased;
		private DamageInfo lastDamage;

		public int ArmorPoints { get; set; }

		public float InfectionTime { get; set; } = 25f;

		public bool phaseInfection1 { get; set; }
		private bool phaseInfection2;

		public float FeedBar = 5;
		public bool BerserkMode = false;
		
		[Net]
		public TeamType CurTeam { get; set; }

		[Net]
		public ZombieType CurZombieType { get; set; }

		private TimeSince timeSinceDied;
		private TimeSince timeSinceLastHit;
		private TimeSince timeTillNextRegen;

		private int RegenAmount = 5;
		
		public int CurWeightSlots;
		private int CarryAllowance = 4;

		public string AmmoTypeToDrop = "pistol";

		private List<WeaponBase> curWeapons;
		
		public ZPSVPlayer()
		{
			Inventory = new Inventory( this );
			curWeapons = new List<WeaponBase>();
		}


		public void InitialSpawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			Controller = new SurvivorWalkController();
			Animator = new StandardPlayerAnimator();
			Camera = new FirstPersonCamera();

			//Dress();
			ClearAmmo();
			curWeapons.Clear();
			
			CurWeightSlots = 0;
			RenderAlpha = 255;
			
			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			EnableAllCollisions = true;

			base.Respawn();
			flashlight = new Flashlight();
			AllowFlashlight = true;

			SwapTeam( TeamType.Unassigned );
		}

		public void GiveWeapons()
		{
			Inventory.DeleteContents();
			if ( CurTeam == TeamType.Undead )
				Inventory.Add( new Claws(), true );
			else
			{
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
				
				int RandFirearm = Rand.Int( 1, 2 );
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
				
				GiveAmmo(AmmoType.Pistol, 12);
				//CurWeightSlots += 1.25f;
			}
		}

		public override void StartTouch( Entity other )
		{
		}


		public override void Respawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			Camera = new FirstPersonCamera();
			
			if(CurTeam == TeamType.Survivor)
				Controller = new SurvivorWalkController();

			else if ( CurTeam == TeamType.Undead )
				Controller = new ZombieWalkController();
			
			Animator = new StandardPlayerAnimator();

			ClearAmmo();
			curWeapons.Clear();
			
			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			EnableAllCollisions = true;

			base.Respawn();

			if(CurTeam == TeamType.Spectator)
			{
				EnableAllCollisions = false;
				RenderAlpha = 0;

			} else
			{
				EnableAllCollisions = true;
				RenderAlpha = 255;
			}

			if ( CurTeam == TeamType.Undead )
			{
				Health = 200;
				
				if( CurZombieType == ZombieType.Carrier )
                {
                	RenderColor = new Color32( 255, 140, 140 );
                    Health = 250;
                }
    
                List<Vector3> vectorSpawns = new List<Vector3>();
                foreach(var ZombiePoints in Entity.All.OfType<ZombiePoint>())
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
			//base.Simulate( cl );
			SimulateActiveChild( cl, ActiveChild );

			if ( (ZPSVGame.CurState == ZPSVGame.RoundState.Idle || ZPSVGame.CurState == ZPSVGame.RoundState.Start) &&
			     (CurTeam == TeamType.Survivor || CurTeam == TeamType.Undead || CurTeam == TeamType.Infected && phaseInfection1 ) )
					return;
			
			TickPlayerUse();
			
			var controller = GetActiveController();
			controller?.Simulate( cl, this, GetActiveAnimator() );
			
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
					dropped.PhysicsGroup.ApplyImpulse( Velocity + EyeRot.Forward * 250.0f + Vector3.Up * 100.0f, true );
					dropped.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100.0f, true );

					timeSinceDropped = 0;
					SwitchToBestWeapon();
				}
			} 
			
			if ( Input.Pressed( InputButton.Use ) && IsServer )
			{
				var tr = Trace.Ray( EyePos, EyePos + EyeRot.Forward * 92 )
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
					
					item.OnCarryStart( this );
					Log.Info(ArmorPoints);
					//CurWeight += item.Weight;
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
			
			if ( Input.ActiveChild != null )
			{
				ActiveChild = Input.ActiveChild;
			}
		}

		[ClientRpc]
		public void UpdateArmorClient(int amount)
		{
			ArmorPoints += amount;
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
			
			if ( timeSinceLastHit >= 15)
			{
				if(timeTillNextRegen <= 4)
					return;

				if ( CurZombieType == ZombieType.Carrier )
				{
					if ( Health > 250 )
					{
						Health = 250;
						return;
					}
				}
				else
				{
					if ( Health > 200 )
                    {
                    	Health = 200;
                    	return;
                    }
				}
				
				Health += RegenAmount;
				
				timeTillNextRegen = 0;
			}
		}
		
		
		public void DropAmmoType()
		{
			if ( AmmoTypeToDrop == "pistol" && AmmoCount( AmmoType.Pistol ) >= 12 )
			{
				var pistolAmmo = new PistolAmmo();

				pistolAmmo.Position = Position + new Vector3( 0, 0, 85 );
				
				pistolAmmo.PhysicsGroup.ApplyImpulse( EyeRot.Forward * 250.0f + Vector3.Up * 100.0f, true );
				pistolAmmo.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100.0f, true );
					
				//CurWeight -= pistolAmmo.Weight;
					
				TakeAmmo( AmmoType.Pistol, 12 );
				timeSinceDropped = 0;
					
			} else if ( AmmoTypeToDrop == "buckshot" && AmmoCount( AmmoType.Buckshot ) >= 6 )
			{
				var shotgunAmmo = new ShotgunAmmo();

				shotgunAmmo.Position = Position + new Vector3( 0, 0, 85 );

				shotgunAmmo.PhysicsGroup.ApplyImpulse( EyeRot.Forward * 250.0f + Vector3.Up * 100.0f, true );
				shotgunAmmo.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100.0f, true );

				//CurWeight -= shotgunAmmo.Weight;

				TakeAmmo( AmmoType.Buckshot, 6 );
				timeSinceDropped = 0;
					
			} else if ( AmmoTypeToDrop == "rifle" && AmmoCount( AmmoType.Rifle ) >= 30 )
			{
				var rifleAmmo = new RifleAmmo();

				rifleAmmo.Position = Position + new Vector3( 0, 0, 85 );

				rifleAmmo.PhysicsGroup.ApplyImpulse( EyeRot.Forward * 250.0f + Vector3.Up * 100.0f, true );
				rifleAmmo.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100.0f, true );

				//CurWeight -= rifleAmmo.Weight;

				TakeAmmo( AmmoType.Rifle, 30 );
				timeSinceDropped = 0;
				
			} else if ( AmmoTypeToDrop == "magnum" && AmmoCount( AmmoType.Magnum ) >= 6 )
			{
				var magnumAmmo = new MagnumAmmo();

				magnumAmmo.Position = Position + new Vector3( 0, 0, 85 );

				magnumAmmo.PhysicsGroup.ApplyImpulse( EyeRot.Forward * 250.0f + Vector3.Up * 100.0f, true );
				magnumAmmo.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100.0f, true );

				//CurWeight -= rifleAmmo.Weight;

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
				Sound.FromScreen( "infected" );
				phaseInfection1 = true;

			}

			if ( CurTeam == TeamType.Infected && InfectionTime < 3.9f && phaseInfection2 == false )
			{
				Sound.FromScreen( "turning" );
				phaseInfection2 = true;
			}

			if ( CurTeam == TeamType.Infected && InfectionTime <= 0f )
			{
				SwapTeam( TeamType.Undead );
				EnableFlashlight( false );
				GiveWeapons();
				phaseInfection1 = false;
				phaseInfection2 = false;
			}
		}



		public void SwapTeam( TeamType targetTeam )
		{
			CurTeam = targetTeam;

			if ( targetTeam == TeamType.Undead )
			{
				RenderColor = new Color32( 125, 75, 75 ); 
			}
			else
			{
				RenderColor = new Color32( 255, 255, 255 );
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

		public override void TakeDamage( DamageInfo info )
		{
			var attacker = info.Attacker as ZPSVPlayer;

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

		public override void OnKilled()
		{
			//base.OnKilled();
			
			Game.Current?.OnKilled( this );

			flashlight.LightEnabled = false;
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

			Inventory.DeleteContents();
			ZPSVGame.CheckRoundStatus();
			Camera = new SpectateRagdollCamera();
			EnableDrawing = false;
		}
	}
}
