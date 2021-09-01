using Sandbox;
using System;

namespace ZPS_Viral
{
	[Library( "zpsviral_axe", Title = "Hatchet" )]
	[Hammer.EditorModel( "models/weapons/axe/rust_boneknife.vmdl" )]
	partial class Axe : WeaponBase
	{
		public override string ViewModelPath => "models/weapons/axe/v_rust_boneknife.vmdl";
		public override int ClipSize => -1;
		public override float PrimaryRate => 1.0f;
		public override float SecondaryRate => 0.5f;
		public override float ReloadTime => 4.0f;
		public override int Bucket => 0;

		public override bool IsMelee => true;

		private int MeleeDistance = 25;
		
		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/axe/rust_boneknife.vmdl" );
		}

		public virtual void MeleeStrike( float damage, float force )
		{
			var forward = Owner.EyeRot.Forward;
			forward = forward.Normal;

			foreach ( var tr in TraceBullet( Owner.EyePos, Owner.EyePos + forward * MeleeDistance, 10f ) )
			{
				if ( !tr.Entity.IsValid() ) continue;

				tr.Surface.DoBulletImpact( tr );

				if ( !IsServer ) continue;

				using ( Prediction.Off() )
				{
					var damageInfo = DamageInfo.FromBullet( tr.EndPos, forward * 100 * force, damage )
						.UsingTraceResult( tr )
						.WithAttacker( Owner )
						.WithWeapon( this );

					tr.Entity.TakeDamage( damageInfo );
				}
			}
		}

		public override void AttackPrimary()
		{
			//if (!CanPrimaryAttack()) return;

			PlaySound( "rust_boneknife.attack" );
			MeleeStrike( 30, 1.5f );

			(Owner as AnimEntity).SetAnimBool( "b_attack", true );
			ShootEffects();
		}
		
		[ClientRpc]
		protected override void ShootEffects()
		{
			Host.AssertClient();

			ViewModelEntity?.SetAnimBool( "fire", true );
			CrosshairPanel?.CreateEvent( "fire" );
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetParam( "holdtype", 4 ); // TODO this is shit
			anim.SetParam( "aimat_weight", 1.0f );
		}
	}
}
