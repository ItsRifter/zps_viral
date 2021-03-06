using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace ZPS_Viral
{
	public partial class WeaponBase : BaseWeapon
	{
		public virtual AmmoType AmmoType => AmmoType.Pistol;

		public virtual string WorldModelPath { get; set; }
		public virtual int ClipSize => 16;
		public virtual float ReloadTime => 3.0f;
		public virtual int Bucket => 0;
		public virtual int BucketWeight => 100;

		public virtual int CheckIndex => 0;
		
		public virtual int WeightSlots => 0;
		
		public virtual bool IsDroppable => true;

		public virtual bool IsMelee => false;

		public virtual int BulletsRemaining => 0;

		[Net, Predicted]
		public int AmmoClip { get; set; }

		[Net, Predicted]
		public TimeSince TimeSinceReload { get; set; }

		[Net, Predicted]
		public bool IsReloading { get; set; }

		[Net, Predicted]
		public TimeSince TimeSinceDeployed { get; set; }

		[Net, Predicted]
		public TimeSince TimeSinceDropped { get; set; }

		public PickupTrigger PickupTrigger { get; protected set; }

		public virtual int BaseDamage => 0;
		
		public override void Spawn()
		{
			base.Spawn();
			
			SetInteractsAs( CollisionLayer.Hitbox );
			
		}

		public int AvailableAmmo()
		{
			var owner = Owner as ZPSVPlayer;
			if ( owner == null ) return 0;
			return owner.AmmoCount( AmmoType );
		}

		public override bool CanCarry( Entity carrier )
		{
			if ( TimeSinceDropped < 0.5f ) {
				return false;
			}
			
			if(carrier is ZPSVPlayer player)
			{
				if ( player.CurTeam == ZPSVPlayer.TeamType.Undead || player.CurTeam == ZPSVPlayer.TeamType.Infected )
					return false;
			}

			return true;
		}

		public override void ActiveStart( Entity ent )
		{
			base.ActiveStart( ent );

			TimeSinceDeployed = 0;
			TimeSinceDropped = 0;
		}

		public override void Reload()
		{
			if ( IsReloading )
				return;

			if ( AmmoClip >= ClipSize )
				return;

			TimeSinceReload = 0;

			if ( Owner is ZPSVPlayer player )
			{
				if ( player.AmmoCount( AmmoType ) <= 0 )
					return;

				StartReloadEffects();
			}

			IsReloading = true;

			(Owner as AnimEntity).SetAnimParameter( "b_reload", true );

			StartReloadEffects();
		}

		public override void Simulate( Client owner )
		{
			if ( TimeSinceDeployed < 0.6f )
				return;

			if ( !IsReloading )
			{
				base.Simulate( owner );
			}

			if ( IsReloading && TimeSinceReload > ReloadTime )
			{
				OnReloadFinish();
			}
		}

		public virtual void OnReloadFinish()
		{
			IsReloading = false;

			if ( Owner is ZPSVPlayer player )
			{
				player.RecheckAmmoWeight(ClipSize, AmmoType);
				
				var ammo = player.TakeAmmo( AmmoType, ClipSize - AmmoClip );
				if ( ammo == 0 )
					return;

				AmmoClip += ammo;
			}
		}

		[ClientRpc]
		public virtual void StartReloadEffects()
		{
			ViewModelEntity?.SetAnimParameter( "reload", true );

			// TODO - player third person model reload
		}

		public override void AttackPrimary()
		{
			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;

			//
			// Tell the clients to play the shoot effects
			//
			ShootEffects();

			//
			// ShootBullet is coded in a way where we can have bullets pass through shit
			// or bounce off shit, in which case it'll return multiple results
			//
			foreach ( var tr in TraceBullet( Owner.EyePosition, Owner.EyePosition + Owner.EyeRotation.Forward * 5000 ) )
			{
				tr.Surface.DoBulletImpact( tr );

				if ( !IsServer ) continue;
				if ( !tr.Entity.IsValid() ) continue;

				//
				// We turn predictiuon off for this, so aany exploding effects don't get culled etc
				//
				using ( Prediction.Off() )
				{
					var damage = DamageInfo.FromBullet( tr.EndPosition, Owner.EyeRotation.Forward * 100, 15 )
						.UsingTraceResult( tr )
						.WithAttacker( Owner )
						.WithWeapon( this );

					tr.Entity.TakeDamage( damage );
				}
			}
		}

		[ClientRpc]
		protected virtual void ShootEffects()
		{
			Host.AssertClient();

			Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

			if ( IsLocalPawn )
			{
				new Sandbox.ScreenShake.Perlin();
			}

			ViewModelEntity?.SetAnimParameter( "fire", true );
		}

		/// <summary>
		/// Shoot a single bullet
		/// </summary>
		public virtual void ShootBullet( float spread, float force, float damage, float bulletSize )
		{
			var forward = Owner.EyeRotation.Forward;
			forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
			forward = forward.Normal;

			//
			// ShootBullet is coded in a way where we can have bullets pass through shit
			// or bounce off shit, in which case it'll return multiple results
			//
			foreach ( var tr in TraceBullet( Owner.EyePosition, Owner.EyePosition + forward * 5000, bulletSize ) )
			{
				tr.Surface.DoBulletImpact( tr );

				if ( !IsServer ) continue;
				if ( !tr.Entity.IsValid() ) continue;

				//
				// We turn predictiuon off for this, so any exploding effects don't get culled etc
				//
				using ( Prediction.Off() )
				{
					var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100 * force, damage )
						.UsingTraceResult( tr )
						.WithAttacker( Owner )
						.WithWeapon( this );

					tr.Entity.TakeDamage( damageInfo );
				}
			}
		}

		public bool TakeAmmo( int amount )
		{
			if ( AmmoClip < amount )
				return false;

			AmmoClip -= amount;
			return true;
		}

		[ClientRpc]
		public virtual void DryFire()
		{
			// CLICK
		}

		public override void CreateViewModel()
		{
			Host.AssertClient();

			if ( string.IsNullOrEmpty( ViewModelPath ) )
				return;

			ViewModelEntity = new ViewModel();
			ViewModelEntity.Position = Position;
			ViewModelEntity.Owner = Owner;
			ViewModelEntity.EnableViewmodelRendering = true;
			ViewModelEntity.SetModel( ViewModelPath );
		}

		public override void CreateHudElements()
		{
			if ( Local.Hud == null ) return;
		}

		public bool IsUsable()
		{
			if ( AmmoClip > 0 ) return true;
			return AvailableAmmo() > 0;
		}

		
		public override void OnCarryStart( Entity carrier )
		{
			base.OnCarryStart( carrier );
		}

		public override void OnCarryDrop( Entity dropper )
		{
			base.OnCarryDrop( dropper );

			TimeSinceDropped = 0;

			if ( PickupTrigger.IsValid() )
			{
				PickupTrigger.EnableTouch = true;
			}
		}

		public virtual void OnHolster()
		{

		}

	}
}
