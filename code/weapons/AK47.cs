using Sandbox;
using System;

namespace ZPS_Viral
{
	[Library( "zpsviral_ak47", Title = "AK-47" )]
	[Hammer.EditorModel( "models/weapons/ak47/w_ak47.vmdl" )]
	partial class AK47 : WeaponBase
	{
		public override string ViewModelPath => "models/weapons/ak47/v_ak47.vmdl";

		public override AmmoType AmmoType => AmmoType.Rifle;
		public override float PrimaryRate => 12.0f;
		public override float SecondaryRate => 1.0f;
		public override int ClipSize => 30;
		public override float ReloadTime => 4.0f;
		public override int Bucket => 2;
		public override int BulletsRemaining => ClipSize;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/ak47/w_ak47.vmdl" );
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

			(Owner as AnimEntity).SetAnimBool( "b_attack", true );

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

		public override void StartReloadEffects()
		{
			if ( AmmoClip <= 0 )
			{
				ViewModelEntity?.SetAnimBool( "reload_empty", true );
				PlaySound( "ak47_reload_empty" );
			}
			else
			{
				ViewModelEntity?.SetAnimBool( "reload", true );
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

			ViewModelEntity?.SetAnimBool( "fire" + Rand.Int(1, 3), true );
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetParam( "holdtype", 2 );
			anim.SetParam( "aimat_weight", 1.0f );
		}
	}
}
