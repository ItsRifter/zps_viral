using Sandbox;
using System;

namespace ZPS_Viral
{
	[Library( "zpsviral_mp5", Title = "MP-5" )]
	[Hammer.EditorModel( "models/weapons/mp5/w_mp5.vmdl" )]
	partial class MP5 : WeaponBase
	{
		public override string ViewModelPath => "models/weapons/mp5/v_mp5.vmdl";

		public override string WorldModelPath => "models/weapons/mp5/w_mp5.vmdl";
		public override AmmoType AmmoType => AmmoType.Pistol;
		public override float PrimaryRate => 14.0f;
		public override int ClipSize => 30;
		public override float ReloadTime => 3f;
		public override int Bucket => 2;
		public override int BulletsRemaining => ClipSize;
		
		public override int BaseDamage => 45;
		public override int CheckIndex => 30;
		public override int  WeightSlots => 2;

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
				ViewModelEntity?.SetAnimParameter( "reload_empty", true );
				PlaySound( "mp5_reloadempty" );
			}
			else
			{
				ViewModelEntity?.SetAnimParameter( "reload", true );
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

			ViewModelEntity?.SetAnimParameter( "fire", true );
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetAnimParameter( "holdtype", 2 );
			anim.SetAnimParameter( "aimat_weight", 1.0f );
		}
		
		public override void DryFire()
		{
			ViewModelEntity?.SetAnimParameter( "dryfire", true );
			PlaySound( "mp5_dryfire" );
		}
	}
}
