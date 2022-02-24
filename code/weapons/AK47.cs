using Sandbox;
using System;

namespace ZPS_Viral
{
	[Library( "zpsviral_ak47", Title = "AK-47" )]
	[Hammer.EditorModel( "models/weapons/ak47/w_ak47.vmdl" )]
	partial class AK47 : WeaponBase
	{
		public override string ViewModelPath => "models/weapons/ak47/v_ak47.vmdl";

		public override string WorldModelPath => "models/weapons/ak47/w_ak47.vmdl";

		public override AmmoType AmmoType => AmmoType.Rifle;
		public override float PrimaryRate => 12.0f;
		public override int ClipSize => 30;
		public override float ReloadTime => 3.65f;
		public override int Bucket => 3;
		public override int BulletsRemaining => ClipSize;
		public override int CheckIndex => 31;
		public override int WeightSlots => 2;

		public override int BaseDamage => 55;
		
		public override void Spawn()
		{
			base.Spawn();

			SetModel( WorldModelPath );
			AmmoClip = BulletsRemaining;
		}

		public override void AttackPrimary()
		{
			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;

			if ( !TakeAmmo( 1 ) )
			{
				DryFire();
				return;
			}

			(Owner as AnimEntity).SetAnimParameter( "b_attack", true );

			//
			// Tell the clients to play the shoot effects
			//
			ShootEffects();
			PlaySound( "ak47_fire" );

			//
			// Shoot the bullets
			//
			ShootBullet( 0.1f, 1.5f, 22f, 3.0f );

		}
		
		[ClientRpc]
		public override void StartReloadEffects()
		{
			if ( AmmoClip <= 0 )
			{
				ViewModelEntity?.SetAnimParameter( "reload_empty", true );
				PlaySound( "ak47_reload_empty" );
			}
			else
			{
				ViewModelEntity?.SetAnimParameter( "reload", true );
				PlaySound( "ak47_reload" );
			}
		}

		[ClientRpc]
		protected override void ShootEffects()
		{
			Host.AssertClient();

			Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
			Particles.Create( "particles/pistol_ejectbrass.vpcf", EffectEntity, "ejection_point" );

			if ( Owner == Local.Pawn )
			{
				new Sandbox.ScreenShake.Perlin( 0.5f, 3.0f, 0.5f, 0.5f );
			}

			ViewModelEntity?.SetAnimParameter( "fire" + Rand.Int(1, 3), true );
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetAnimParameter( "holdtype", 2 );
			anim.SetAnimParameter( "aimat_weight", 1.0f );
		}
		
		public override void DryFire()
		{
			ViewModelEntity?.SetAnimParameter( "dry_fire", true );
			PlaySound( "ak47_dryfire" );
		}
	}
}
