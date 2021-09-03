using Sandbox;
using System;

namespace ZPS_Viral
{
	[Library( "zpsviral_mp5", Title = "MP-5" )]
	[Hammer.EditorModel( "models/weapons/mp5/w_mp5.vmdl" )]
	partial class MP5 : WeaponBase
	{
		public override string ViewModelPath => "models/weapons/mp5/v_mp5.vmdl";

		public override AmmoType AmmoType => AmmoType.Pistol;
		public override float PrimaryRate => 14.0f;
		public override int ClipSize => 30;
		public override float ReloadTime => 3f;
		public override int Bucket => 2;
		public override int BulletsRemaining => ClipSize;
		
		public override float Weight => 2.89f;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/mp5/w_mp5.vmdl" );
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
			PlaySound( "mp5_fire" );

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
				ViewModelEntity?.SetAnimBool( "reload_empty", true );
				PlaySound( "mp5_reloadempty" );
			}
			else
			{
				ViewModelEntity?.SetAnimBool( "reload", true );
				PlaySound( "mp5_reload" );
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

			ViewModelEntity?.SetAnimBool( "fire", true );
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetParam( "holdtype", 2 );
			anim.SetParam( "aimat_weight", 1.0f );
		}
		
		public override void DryFire()
		{
			ViewModelEntity?.SetAnimBool( "dryfire", true );
			PlaySound( "mp5_dryfire" );
		}
	}
}
