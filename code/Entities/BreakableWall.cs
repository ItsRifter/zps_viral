using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sandbox;

namespace ZPS_Viral.Entities
{
	[Library( "zpsviral_breakable", Description = "Acts the same as a func_breakable" )]
	[Hammer.Model]
	[Hammer.SupportsSolid]
	[Hammer.RenderFields]
	partial class BreakableWall : AnimEntity
	{
		[Property( "Health until break" )] 
		public float HealthUntilBreak { get; set; }

		protected Output OnBroken { get; set; }

		public override void Spawn()
		{
			base.Spawn();
			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
			
		}

		public override void TakeDamage( DamageInfo info )
		{
			HealthUntilBreak -= info.Damage;

			if ( HealthUntilBreak <= 0 )
			{
				_ = OnBroken.Fire( this );
				OnKilled();
			}
		}
	}
}
