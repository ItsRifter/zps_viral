using Sandbox;
using System;

namespace ZPS_Viral
{
	[Library( "zpsviral_machete", Title = "Axe" )]
	[Hammer.EditorModel( "models/weapons/machete/w_machete.vmdl" )]
	partial class Machete : WeaponBase
	{
		public override string ViewModelPath => "models/weapons/machete/v_machete.vmdl";
		public override int ClipSize => -1;
		public override float PrimaryRate => 1.75f;
		public override float SecondaryRate => 0.5f;
		public override float ReloadTime => 4.0f;
		public override int Bucket => 0;
		public override int CheckIndex => 1;
		public override int WeightSlots => 1;
		public override bool IsMelee => true;

		private int MeleeDistance = 70;
		
		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/machete/w_machete.vmdl" );
		}
		
		private bool MeleeAttack()
		{
			var forward = Owner.EyeRot.Forward;
			forward = forward.Normal;

			bool hit = false;

			foreach ( var tr in TraceBullet( Owner.EyePos, Owner.EyePos + forward * MeleeDistance, 20.0f ) )
			{
				if ( !tr.Entity.IsValid() ) continue;

				tr.Surface.DoBulletImpact( tr );

				hit = true;

				if ( !IsServer ) continue;

				using ( Prediction.Off() )
				{
					var damageInfo = DamageInfo.FromBullet( tr.EndPos, forward * MeleeDistance, 25 )
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

			ViewModelEntity?.SetAnimBool( "miss" + Rand.Int( 1, 2 ), true );
		}

		[ClientRpc]
		private void OnMeleeHit()
		{
			Host.AssertClient();

			if ( IsLocalPawn )
			{
				_ = new Sandbox.ScreenShake.Perlin( 1.0f, 1.0f, 3.0f );
			}

			ViewModelEntity?.SetAnimBool( "fire" + Rand.Int( 1, 2 ), true );
		}

		public override void AttackPrimary()
		{
			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;

			if ( MeleeAttack() )
			{
				OnMeleeHit();
				PlaySound("machete_hit");
			}
			else
			{
				OnMeleeMiss();
				PlaySound( "z_miss" );
			}
		}
		
		[ClientRpc]
		protected override void ShootEffects()
		{
			Host.AssertClient();

			ViewModelEntity?.SetAnimBool( "fire" + Rand.Int( 1, 2 ), true );
		}

		
		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetParam( "holdtype", 4 ); // TODO this is shit
			anim.SetParam( "aimat_weight", 1.0f );
		}
	}
}
