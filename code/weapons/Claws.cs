using Sandbox;

namespace ZPS_Viral
{
	[Library( "zpsviral_claws", Title = "Claws" )]
	partial class Claws : WeaponBase
	{
		public override string ViewModelPath => "models/weapons/claws/carrier_claws.vmdl";
		public override float PrimaryRate => 2f;
		public override float SecondaryRate => 0.0f;
		public override float ReloadTime => 0.0f;
		public override int ClipSize => 99;

		public override bool IsDroppable => false;
		public override int Bucket => 1;
	
		public override int BaseDamage => 20;
		public override bool IsMelee => true;	
		public override void Spawn()
		{
			base.Spawn();

			AmmoClip = 99;
		}

		public override bool CanCarry( Entity carrier )
		{
			return true;
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetAnimParameter( "holdtype", 0 );
			anim.SetAnimParameter( "aimat_weight", 1.0f );
		}

		private bool MeleeAttack()
		{
			var forward = Owner.EyeRotation.Forward;
			forward = forward.Normal;

			bool hit = false;

			foreach ( var tr in TraceBullet( Owner.EyePosition, Owner.EyePosition + forward * 80, 20.0f ) )
			{
				if ( !tr.Entity.IsValid() ) continue;

				tr.Surface.DoBulletImpact( tr );

				hit = true;

				if ( !IsServer ) continue;

				using ( Prediction.Off() )
				{
					var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100, BaseDamage )
						.UsingTraceResult( tr )
						.WithAttacker( Owner )
						.WithWeapon( this );

					tr.Entity.TakeDamage( damageInfo );
				}
			}

			return hit;
		}

		[ClientRpc]
		private void OnMeleeMiss()
		{
			Host.AssertClient();

			if ( IsLocalPawn )
			{
				_ = new Sandbox.ScreenShake.Perlin();
			}

			ViewModelEntity?.SetAnimParameter( "fire", true );
		}

		[ClientRpc]
		private void OnMeleeHit()
		{
			Host.AssertClient();

			if ( IsLocalPawn )
			{
				_ = new Sandbox.ScreenShake.Perlin( 1.0f, 1.0f, 3.0f );
			}

			ViewModelEntity?.SetAnimParameter( "fire", true );
		}

		public override void AttackPrimary()
		{
			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;

			if ( MeleeAttack() )
			{
				OnMeleeHit();
				PlaySound("z_hit");
			}
			else
			{
				OnMeleeMiss();
				PlaySound( "z_miss" );
			}
		}
	}
}
