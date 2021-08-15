using Sandbox;
using System;
using System.Linq;

namespace ZPS2
{
	public partial class ZPS2Player : Player
	{
		public enum TeamType
		{
			Unassigned,
			Survivor,
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

			Controller = new WalkController();
			Animator = new StandardPlayerAnimator();
			Camera = new FirstPersonCamera();

			Dress();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			base.Respawn();

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

			Controller = new WalkController();
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
		}

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );
			SimulateActiveChild( cl, ActiveChild );
		}

		public void SwapTeam( TeamType targetTeam )
		{
			this.CurTeam = targetTeam;

			if ( targetTeam == TeamType.Undead )
			{
				this.RenderColor = new Color32( 86, 227, 54 );
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

		[ClientRpc]
		public void PlaySoundClient( String soundPath)
		{
			Sound.FromScreen( soundPath );
		}

		public override void TakeDamage( DamageInfo info )
		{
			var attacker = info.Attacker as ZPS2Player;

			if ( ZPS2Game.CurState != ZPS2Game.RoundState.Active )
				return;

			if ( attacker.IsValid() && attacker.CurTeam == this.CurTeam )
				return;

			base.TakeDamage( info );

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
