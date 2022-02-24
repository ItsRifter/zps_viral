using Sandbox;

namespace ZPS_Viral
{
	[Library( "zpsviral_revolver", Title = "Revolver" )]
	[Hammer.EditorModel( "models/weapons/revolver/w_revolver.vmdl" )]
	partial class Revolver : WeaponBase
	{
		public override string ViewModelPath => "models/weapons/revolver/v_revolver.vmdl";
		
		public override string WorldModelPath => "models/weapons/revolver/w_revolver.vmdl";
		public override AmmoType AmmoType => AmmoType.Magnum;
		public override float PrimaryRate => 1.35f;
		public override float SecondaryRate => 1.0f;
		public override float ReloadTime => 3.75f;
		public override int ClipSize => 6;
		public override int BulletsRemaining => ClipSize;
		public override int Bucket => 1;
		public override int CheckIndex => 40;
		public override int WeightSlots => 2;
		public override int BaseDamage => 75;
		
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
		
		[ClientRpc]
		public override void StartReloadEffects()
		{
			ViewModelEntity?.SetAnimParameter( "reload", true );
			PlaySound( "revolver_reload" );
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
			PlaySound( "revolver_fire" );

			ShootBullet( 0.05f, 1.5f, BaseDamage, 2.0f );

		}

		public override void DryFire()
		{
			PlaySound( "revolver_dryfire" );
		}
	}
}
