using Sandbox;

namespace ZPS_Viral
{
	[Library( "zpsviral_usp", Title = "USP" )]
	[Hammer.EditorModel( "models/weapons/usp/w_usp.vmdl" )]
	partial class USP : WeaponBase
	{
		public override string ViewModelPath => "models/weapons/usp/v_usp.vmdl";

		public override string WorldModelPath => "models/weapons/usp/w_usp.vmdl";
		public override float PrimaryRate => 15.0f;
		public override float SecondaryRate => 1.0f;
		public override float ReloadTime => 3.0f;
		public override int ClipSize => 14;
		public override int BulletsRemaining => ClipSize;
		public override int Bucket => 1;

		public override int CheckIndex => 10;
		public override int WeightSlots => 1;
		public override int BaseDamage => 30;
		
		public override void Spawn()
		{
			base.Spawn();

			SetModel( WorldModelPath );
			AmmoClip = BulletsRemaining;
		}

		[ClientRpc]
		public override void StartReloadEffects()
		{
			if ( AmmoClip <= 0 )
			{
				ViewModelEntity?.SetAnimParameter( "reload_empty", true );
				PlaySound( "usp_reloadempty" );
			}
			else
			{
				ViewModelEntity?.SetAnimParameter( "reload", true );
				PlaySound( "usp_reload" );
			}
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

			ShootEffects();
			PlaySound( "usp_fire" );

			ShootBullet( 0.05f, 1.5f, BaseDamage, 3.0f );

		}

		public override void DryFire()
		{
			PlaySound( "usp_dryfire" );
		}
	}
}
