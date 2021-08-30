using Sandbox;

namespace ZPS_Viral
{
	[Library( "zpsviral_remington", Title = "Remington" )]
	[Hammer.EditorModel( "models/weapons/remington.vmdl" )]
	partial class Remington : WeaponBase
	{
		public override string ViewModelPath => "models/weapons/v_remington.vmdl";
		public override float PrimaryRate => 1;
		public override AmmoType AmmoType => AmmoType.Buckshot;
		public override int ClipSize => 6;
		public override float ReloadTime => 0.5f;
		public override int Bucket => 2;
		public override int BulletsRemaining => ClipSize;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/remington.vmdl" );

			AmmoClip = BulletsRemaining;
		}

		public override bool CanPrimaryAttack()
		{
			return base.CanPrimaryAttack() && Input.Pressed( InputButton.Attack1 );
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
			PlaySound( "remington_fire" );

			//
			// Shoot the bullets
			//
			for ( int i = 0; i < 10; i++ )
			{
				ShootBullet( 0.15f, 0.3f, 9.0f, 3.0f );
			}
		}

		public override void DryFire()
		{
			PlaySound( "remington_dryfire" );
		}

		[ClientRpc]
		protected override void ShootEffects()
		{
			Host.AssertClient();

			Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
			Particles.Create( "particles/pistol_ejectbrass.vpcf", EffectEntity, "ejection_point" );

			ViewModelEntity?.SetAnimBool( "fire", true );

			if ( IsLocalPawn )
			{
				new Sandbox.ScreenShake.Perlin( 1.0f, 1.5f, 2.0f );
			}
		}

		public override void OnReloadFinish()
		{
			IsReloading = false;

			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;

			if ( AmmoClip >= ClipSize )
				return;

			if ( Owner is ZPSVPlayer player )
			{
				var ammo = player.TakeAmmo( AmmoType, 1 );
				if ( ammo == 0 )
					return;

				AmmoClip += ammo;

				if ( AmmoClip < ClipSize )
				{
					Reload();
				}
				else
				{
					FinishReload();
				}
			}
		}

		[ClientRpc]
		protected virtual void FinishReload()
		{
			ViewModelEntity?.SetAnimBool( "reload_finished", true );
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetParam( "holdtype", 2 );
			anim.SetParam( "aimat_weight", 1.0f );
		}
	}
}
