using Sandbox;

namespace ZPS_Viral
{
	[Library( "zpsviral_claws", Title = "Claws" )]
	partial class Claws : WeaponBase
	{
		public override float PrimaryRate => 2f;
		public override float SecondaryRate => 0.0f;
		public override float ReloadTime => 0.0f;
		public override int ClipSize => 99;

		public override bool IsDroppable => false;
		public override int Bucket => 1;

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
			anim.SetParam( "holdtype", 0 );
			anim.SetParam( "aimat_weight", 1.0f );
		}

		private bool MeleeAttack()
		{
			var forward = Owner.EyeRot.Forward;
			forward = forward.Normal;

			bool hit = false;

			foreach ( var tr in TraceBullet( Owner.EyePos, Owner.EyePos + forward * 80, 20.0f ) )
			{
				if ( !tr.Entity.IsValid() ) continue;

				tr.Surface.DoBulletImpact( tr );

				hit = true;

				if ( !IsServer ) continue;

				using ( Prediction.Off() )
				{
					var damageInfo = DamageInfo.FromBullet( tr.EndPos, forward * 100, 20 )
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

			ViewModelEntity?.SetAnimBool( "attack", true );
		}

		[ClientRpc]
		private void OnMeleeHit()
		{
			Host.AssertClient();

			if ( IsLocalPawn )
			{
				_ = new Sandbox.ScreenShake.Perlin( 1.0f, 1.0f, 3.0f );
			}

			ViewModelEntity?.SetAnimBool( "attack_hit", true );
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
