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
		private TimeSince timeSinceJumpReleased;
		
		private DamageInfo lastDamage;

		[Net]
		public float InfectionTime { get; set; } = 25f;

		[Net]
		public bool phaseInfection1 { get; set; }
		private bool phaseInfection2;

		public float FeedBar = 5f;
		public bool BerserkMode = false;
		
		[Net]
		public TeamType CurTeam { get; set; }

		[Net]
		public ZombieType CurZombieType { get; set; }

		public ICamera LastCamera { get; set; }

		private TimeSince timeSinceDied;
		
		public ZPSVPlayer()
		{
			Inventory = new Inventory( this );
		}


		public void InitialSpawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			Controller = new SurvivorWalkController();
			Animator = new StandardPlayerAnimator();
			Camera = new FirstPersonCamera();

			//Dress();

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
				int RandFirearm = Rand.Int( 1, 2 );
				switch(RandFirearm)
				{
					case 1: AddWeaponToList( new USP(), true );
						break;
					case 2: AddWeaponToList( new Glock17(), true );
						break;
				}
			}
		}


		public void AddWeaponToList(Entity weapon, bool force)
		{
			Inventory.Add( weapon, force );
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
				
				List<Vector3> spectateSpawns = new List<Vector3>();
				foreach(var ZombiePoints in Entity.All.OfType<SurvivorPoint>())
				{
					spectateSpawns.Add( ZombiePoints.Position );
				}

				Position = spectateSpawns[Rand.Int(0, spectateSpawns.Count - 1)];

			} else
			{
				EnableAllCollisions = true;
				RenderAlpha = 255;
			}

			if ( CurTeam == TeamType.Undead )
			{
				if( CurZombieType == ZombieType.Carrier )
                {
                	RenderColor = new Color32( 255, 140, 140 );
                }
    
                //TODO: Make zombie points
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
				.OrderByDescending( x => x.BucketWeight )
				.FirstOrDefault();

			if ( best == null ) return;

			ActiveChild = best;
		}

		public override void Simulate( Client cl )
		{
			//base.Simulate( cl );
			
			var controller = GetActiveController();
			controller?.Simulate( cl, this, GetActiveAnimator() );
			
			SimulateActiveChild( cl, ActiveChild );

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

			if ( Input.Pressed( InputButton.Drop ) )
			{
				var weapon = Inventory.GetSlot( Inventory.GetActiveSlot() ) as WeaponBase;

				if ( weapon == null || weapon.IsDroppable == false )
					return;

				var dropped = Inventory.DropActive();

				if ( dropped != null )
				{
					dropped.PhysicsGroup.ApplyImpulse( Velocity + EyeRot.Forward * 250.0f + Vector3.Up * 100.0f, true );
					dropped.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100.0f, true );

					timeSinceDropped = 0;
					SwitchToBestWeapon();
				}
			} 
			
			if ( Input.Pressed( InputButton.Use ) )
			{
				var tr = Trace.Ray( EyePos, EyePos + EyeRot.Forward * 92 )
					.UseHitboxes( )
					.Ignore( this )
					.Size( 7)
					.EntitiesOnly()
					.Run();
				
				if ( tr.Entity is WeaponBase weapon && IsServer )
				{
					AddWeaponToList( weapon, false );
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

			if( CurTeam == TeamType.Survivor && attacker.IsValid() && (attacker.CurTeam == TeamType.Undead && attacker.CurZombieType == ZombieType.Carrier) )
			{
				if ( Rand.Int( 0, 100 ) >= ZPSVGame.InfectionChance )
					InfectPlayer();
			}

			if ( CurTeam == TeamType.Undead && CurZombieType == ZombieType.Carrier )
				PlaySound( "carrier_pain" );	
			else if ( CurTeam == TeamType.Undead && CurZombieType == ZombieType.Standard )
				//TODO Standard Zombie pain sounds
				PlaySound( "carrier_pain" );


			base.TakeDamage( info );

		}

		[Event( "InfectHuman")]
		public void InfectPlayer()
		{
			if ( CurTeam != TeamType.Survivor )
				return;

			CurTeam = TeamType.Infected;
		}

		public override void OnKilled()
		{
			//base.OnKilled();
			
			Game.Current?.OnKilled( this );
			
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
