using Sandbox;

namespace ZPS_Viral
{
	[Library( "zpsviral_glock17", Title = "Glock 17" )]
	[Hammer.EditorModel( "models/weapons/glock17/w_glock17.vmdl" )]
	partial class Glock17 : WeaponBase
	{
		public override string ViewModelPath => "models/weapons/glock17/v_glock17.vmdl";
		
		public override string WorldModelPath => "models/weapons/glock17/w_glock17.vmdl";
		public override float PrimaryRate => 15.0f;
		public override float SecondaryRate => 1.0f;
		public override float ReloadTime => 3.0f;
		public override int ClipSize => 18;
		public override int BulletsRemaining => ClipSize;
		public override int Bucket => 1;
		public override int CheckIndex => 11;
		public override int WeightSlots => 1;
		
		public override int BaseDamage => 40;
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
			
			if(AmmoClip <= 0)
			{
				ViewModelEntity?.SetAnimParameter( "reload_empty", true );
				PlaySound( "glock17_reloadempty" );
			}
			else
			{
				ViewModelEntity?.SetAnimParameter( "reload", true );
				PlaySound( "glock17_reload" );
			}
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
			PlaySound( "glock17_fire" );

			ShootBullet( 0.05f, 1.5f, 15.0f, 3.0f );

		}

		public override void DryFire()
		{
			PlaySound( "glock17_dryfire" );
		}
	}
}
