﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using ZPS_Viral.Entities;

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

			Dress();

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
			this.Inventory.DeleteContents();
			if ( this.CurTeam == TeamType.Undead )
				Inventory.Add( new Claws(), true );
			else
			{
				int RandFirearm = Rand.Int( 1, 1 );
				if ( RandFirearm == 1 )
				{
					AddWeaponToList( new USP(), true );
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

			if(this.CurTeam == TeamType.Survivor)
				Controller = new SurvivorWalkController();

			else if(this.CurTeam == TeamType.Undead)
				//Controller = new ZombieWalkController -TODO

			Animator = new StandardPlayerAnimator();

			ClearAmmo();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			EnableAllCollisions = true;

			base.Respawn();

			if(this.CurTeam == TeamType.Spectator)
			{
				Camera = new SpectateCamera();
				EnableAllCollisions = false;
				RenderAlpha = 0;
				Event.Run( "noclip", Local.Client );
			} else
			{
				Camera = new FirstPersonCamera();
				EnableAllCollisions = true;
				RenderAlpha = 255;
				Dress();
			}

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
			GiveWeapons();
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
			base.Simulate( cl );
			SimulateActiveChild( cl, ActiveChild );

			bool toggle = Input.Pressed( InputButton.Flashlight );

			if ( flashlight.IsValid() )
			{
				if ( this.CurTeam != TeamType.Survivor )
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
				var weapon = Inventory.GetSlot( Inventory.GetActiveSlot() );

				if ( weapon == null )
					return;

				var dropped = Inventory.DropActive();

				if ( dropped != null )
				{
					dropped.PhysicsGroup.ApplyImpulse( Velocity + EyeRot.Forward * 500.0f + Vector3.Up * 100.0f, true );
					dropped.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100.0f, true );

					timeSinceDropped = 0;
					SwitchToBestWeapon();
				}
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
				this.isInfected = false;
				return;
			}

			if ( this.CurTeam == TeamType.Infected || this.isInfected )
			{
				if (InfectionTime > 0f )
				{
						InfectionTime -= 0.01f;
				}

			}

			if ( this.CurTeam == TeamType.Survivor && InfectionTime < 13f && this.phaseInfection1 == false )
			{
				Sound.FromScreen( "infected" );
				phaseInfection1 = true;
				SwapTeam( TeamType.Infected );
			}

			if ( this.CurTeam == TeamType.Infected && InfectionTime < 4.5f && this.phaseInfection2 == false )
			{
				Sound.FromScreen( "turning" );
				phaseInfection2 = true;
			}

			if ( this.CurTeam == TeamType.Infected && InfectionTime <= 0f )
			{
				this.SwapTeam( TeamType.Undead );
				GiveWeapons();
				phaseInfection1 = false;
				phaseInfection2 = false;
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

			ZPSVGame.CheckRoundStatus();
		}

		[ClientRpc]
		public void SetTeamOnClient( TeamType targetTeam )
		{
			this.CurTeam = targetTeam;
		}

		public override void TakeDamage( DamageInfo info )
		{
			var attacker = info.Attacker as ZPSVPlayer;

			if ( ZPSVGame.CurState != ZPSVGame.RoundState.Active )
				return;

			if ( attacker.IsValid() && attacker.CurTeam == this.CurTeam )
				return;

			if( this.CurTeam == TeamType.Survivor && attacker.IsValid() && (attacker.CurTeam == TeamType.Undead && attacker.CurZombieType == ZombieType.Carrier) )
			{
				if ( Rand.Int( 0, 100 ) >= ZPSVGame.InfectionChance )
					this.InfectPlayer();
			}

			if ( this.CurTeam == TeamType.Undead && this.CurZombieType == ZombieType.Carrier )
				PlaySound( "carrier_pain" );	
			else if ( this.CurTeam == TeamType.Undead && this.CurZombieType == ZombieType.Standard )
				//TODO Standard Zombie pain sounds
				PlaySound( "carrier_pain" );


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

			EnableAllCollisions = false;
			/*
			List<Entity> holdingWeapons = GetAllWeapons();

			foreach ( var weapon in  )
			{
				String WhatToDrop = "";
				bool HasSpawned = false;
				switch ( weapon )
				{
					case USP: WhatToDrop = "USP";
						break;
					case Remington: WhatToDrop = "Remington";
						break;
				}

				Log.Info( WhatToDrop );

				if ( WhatToDrop.Equals( "USP" ) && !HasSpawned )
				{
					var USP = new USP();
					USP.Position = this.Position;
					HasSpawned = true;

				} else if ( WhatToDrop.Equals( "Reminton" ) && !HasSpawned )
				{
					var Shotgun = new Remington();
					Shotgun.Position = this.Position;
					HasSpawned = true;
				}
				
			}
			*/

			BecomeRagdollOnClient( Velocity, lastDamage.Flags, lastDamage.Position, lastDamage.Force, GetHitboxBone( lastDamage.HitboxIndex ) );

			if ( this.CurTeam != TeamType.Undead )
			{
				SwapTeam( TeamType.Undead );
				ZPSVGame.ZombieLives++;

			} else if (this.CurTeam == TeamType.Undead)
			{
				if ( ZPSVGame.ZombieLives > 0 )
					ZPSVGame.ZombieLives--;
				else
					SwapTeam( TeamType.Spectator );

				if ( this.CurZombieType == ZombieType.Carrier )
					PlaySound( "carrier_death" );

			}

			Inventory.DeleteContents();
			ZPSVGame.CheckRoundStatus();
			Camera = new SpectateRagdollCamera();
			EnableDrawing = false;
		}
	}
}
