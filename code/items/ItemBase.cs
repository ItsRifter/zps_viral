using Sandbox;
using System;

namespace ZPS_Viral
{
	partial class ItemBase : BaseCarriable, IRespawnableEntity
	{
		
		public virtual string WorldModelPath => "models/ammo/pistol_ammo.vmdl";

		public virtual string PickupSound => "ammo_pickup";

		public virtual int RemainingAmmo { get; set; } = 1;

		public PickupTrigger PickupTrigger { get; protected set; }

		public override void Spawn()
		{
			base.Spawn();
			
			SetInteractsAs( CollisionLayer.Hitbox );
			
			SetModel( WorldModelPath );
		}

		public override void OnCarryStart( Entity carrier )
		{
			base.OnCarryStart( carrier );
			
			PlaySound( PickupSound );
		}
	}
}
