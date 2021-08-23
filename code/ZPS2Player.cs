using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using ZPS2.Entities;

namespace ZPS2
{
	public partial class ZPS2Player : Player
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

		private static float InfectionTime = 25f;

		private DamageInfo lastDamage;

		private bool isInfected = false;
		private bool phaseInfection1 = false;
		private bool phaseInfection2 = false;

		[Net]
		public TeamType CurTeam { get; set; }

		[Net]
		public ZombieType CurZombieType { get; set; }

		public ICamera LastCamera { get; set; }

		public ZPS2Player()
		{
			Inventory = new Inventory( this );
		}


		public void InitialSpawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			Controller = new SurvivorWalkController();
			Animator = new StandardPlayerAnimator();
			Camera = new FirstPersonCamera();

			Dress();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			base.Respawn();
			flashlight = new Flashlight();
			AllowFlashlight = true;

			SwapTeam( TeamType.Unassigned );
		}

		public void GiveWeapons()
		{
			this.Inventory.DeleteContents();
			if ( this.CurTeam == TeamType.Undead )
				Inventory.Add( new Claws(), true );
		}

		public override void Respawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			if(this.CurTeam == TeamType.Survivor)
				Controller = new SurvivorWalkController();

			else if(this.CurTeam == TeamType.Undead)
				//Controller = new ZombieWalkController -TODO

			Animator = new StandardPlayerAnimator();
			
			Dress();
			ClearAmmo();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			base.Respawn();

			if(this.CurTeam == TeamType.Spectator)
			{
				Camera = new SpectateRagdollCamera();
			} else
			{
				Camera = new FirstPersonCamera();
			}
			
			GiveWeapons();

			if( this.CurZombieType == ZombieType.Carrier )
			{
				this.RenderColor = new Color32( 255, 140, 140 );
			}

			//TODO: Make zombie points
			List<Vector3> vectorSpawns = new List<Vector3>();
			foreach(var ZombiePoints in Entity.All.OfType<SurvivorPoint>())
			{
				vectorSpawns.Add( ZombiePoints.Position );
			}

			this.Position = vectorSpawns[Rand.Int(0, vectorSpawns.Count - 1)];	
		}

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );
			SimulateActiveChild( cl, ActiveChild );

			bool toggle = Input.Pressed( InputButton.Flashlight );

			if ( flashlight.IsValid() )
			{
				if ( flashlight.timeSinceLightToggled > 0.1f && toggle )
				{
					flashlight.LightEnabled = !flashlight.LightEnabled;

					EnableFlashlight( flashlight.LightEnabled );

					flashlight.timeSinceLightToggled = 0;
				}
			}
		}

		[Event("server.tick")]
		public void InfectionThink()
		{
			if ( this.CurTeam == TeamType.Infected || isInfected )
			{
				if ( InfectionTime > 0f )
				{
					InfectionTime -= 0.01f;
				}

			}

			if ( this.CurTeam == TeamType.Survivor && InfectionTime < 13f && phaseInfection1 == false )
			{
				PlaySound( "infected" );
				phaseInfection1 = true;
				SwapTeam( TeamType.Infected );
			}

			if ( this.CurTeam == TeamType.Infected && InfectionTime < 4.5f && phaseInfection2 == false )
			{
				PlaySound( "turning" );
				phaseInfection2 = true;
			}

			if ( this.CurTeam == TeamType.Infected && InfectionTime <= 0f )
			{
				this.SwapTeam( TeamType.Undead );
				GiveWeapons();
			}
		}


		public void SwapTeam( TeamType targetTeam )
		{
			this.CurTeam = targetTeam;

			if ( targetTeam == TeamType.Undead )
			{
				this.RenderColor = new Color32( 125, 75, 75 ); 
			}
			else
			{
				this.RenderColor = new Color32( 255, 255, 255 );
			}

			using ( Prediction.Off() )
			{
				SetTeamOnClient( To.Single( this ), targetTeam );
			}

			ZPS2Game.CheckRoundStatus();
		}

		[ClientRpc]
		public void SetTeamOnClient( TeamType targetTeam )
		{
			this.CurTeam = targetTeam;
		}

		public override void TakeDamage( DamageInfo info )
		{
			var attacker = info.Attacker as ZPS2Player;

			if ( ZPS2Game.CurState != ZPS2Game.RoundState.Active )
				return;

			if ( attacker.IsValid() && attacker.CurTeam == this.CurTeam )
				return;

			if( attacker.IsValid() && (attacker.CurTeam == TeamType.Undead && attacker.CurZombieType == ZombieType.Carrier) )
			{
				if ( Rand.Int( 0, 100 ) <= ZPS2Game.InfectionChance )
					InfectPlayer();
			}

			base.TakeDamage( info );

		}

		[Event( "InfectHuman")]
		public void InfectPlayer()
		{
			if ( this.CurTeam != TeamType.Survivor )
				return;

			this.isInfected = true;
		}

		public override void OnKilled()
		{
			base.OnKilled();

			Inventory.DeleteContents();

			BecomeRagdollOnClient( Velocity, lastDamage.Flags, lastDamage.Position, lastDamage.Force, GetHitboxBone( lastDamage.HitboxIndex ) );

			if ( this.CurTeam != TeamType.Undead )
			{
				SwapTeam( TeamType.Undead );
				ZPS2Game.ZombieLives++;
			} else if (this.CurTeam == TeamType.Undead)
			{
				if ( ZPS2Game.ZombieLives > 0 )
					ZPS2Game.ZombieLives--;
				else
					SwapTeam( TeamType.Spectator );
			}

			ZPS2Game.CheckRoundStatus();
			Camera = new SpectateRagdollCamera();
			EnableDrawing = false;
		}
	}
}
