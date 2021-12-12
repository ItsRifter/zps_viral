using Sandbox;

namespace ZPS_Viral
{
	[Library( "zpsviral_remington", Title = "Remington" )]
	[Hammer.EditorModel( "models/weapons/remington/w_remington.vmdl" )]
	partial class Remington : WeaponBase
	{
		public override string ViewModelPath => "models/weapons/remington/v_remington.vmdl";

		public override string WorldModelPath => "models/weapons/remington/w_remington.vmdl";
		public override float PrimaryRate => 1f;
		public override AmmoType AmmoType => AmmoType.Buckshot;
		public override int ClipSize => 6;
		public override float ReloadTime => 0.5f;
		public override int Bucket => 2;
		public override int BulletsRemaining => ClipSize;
		public override int  WeightSlots => 2;
		
		public override int BaseDamage => 15;
		public override int CheckIndex => 20;
		
		private bool _shouldPump;
		
		public override void Spawn()
		{
			base.Spawn();

			SetModel( WorldModelPath );

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
			PlaySound( "870_fire" );

			//
			// Shoot the bullets
			//
			for ( int i = 0; i < 8; i++ )
			{
				ShootBullet( 0.15f, 0.3f, BaseDamage, 3.0f );
			}
		}

		public override void DryFire()
		{
			PlaySound( "870_dryfire" );
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
		
		[ClientRpc]
		public override void StartReloadEffects()
		{
			PlaySound( "870_reload" );
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
				{
					FinishReload();
					return;
				}
				
				if ( AmmoClip == 0)
					_shouldPump = true;
				
				AmmoClip += ammo;
				
				if ( AmmoClip < ClipSize )
				{
					player.RecheckAmmoWeight( 1, AmmoType );
					Reload();
				}
				else
				{
					player.RecheckAmmoWeight( 1, AmmoType );
					FinishReload();
				}
			}
		}

		[ClientRpc]
		protected virtual void FinishReload()
		{
			if ( _shouldPump )
			{
				PlaySound( "870_pump" );
				ViewModelEntity?.SetAnimBool( "pump", true );
				_shouldPump = false;
			}
		}
		
		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetParam( "holdtype", 2 );
			anim.SetParam( "aimat_weight", 1.0f );
		}
		
	}
}
