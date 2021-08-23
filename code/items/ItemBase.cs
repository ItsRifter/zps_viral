using Sandbox;
using System;

namespace ZPS2
{
	partial class ItemBase : BaseCarriable, IRespawnableEntity
	{
		public virtual Type ItemType => Type.Health;

		public virtual string WorldModelPath => "weapons/rust_pistol/rust_pistol.vmdl";

		public virtual string PickupSound => "shotgun_pickup";

		public PickupTrigger PickupTrigger { get; protected set; }

		public enum Type
		{
			Health,
			Armor,
			Weapon,
			Ammo,
			Other
		}

		public override void Spawn()
		{
			base.Spawn();

			SetModel( WorldModelPath );

			PickupTrigger = new PickupTrigger();
			PickupTrigger.Parent = this;
			PickupTrigger.Position = Position;
		}

		public override void OnCarryStart( Entity carrier )
		{
			base.OnCarryStart( carrier );

			if ( PickupTrigger.IsValid() )
			{
				PickupTrigger.EnableTouch = false;
				PlaySound( PickupSound );
			}
		}
	}
}
