using Sandbox;
namespace ZPS2
{
	[Library( "zps2_ammo_9mm", Title = "9mm Ammo Box" )]

	[Hammer.EntityTool( "9mm Ammo Box", "Ammo", "Small 9mm ammo box" )]
	[Hammer.EditorModel( "models/pistol_ammobox.vmdl" )]
	partial class PistolAmmo : ItemBase
	{
		public override Type ItemType => Type.Ammo;
		public virtual string WorldModelPath => "models/pistol_ammobox.vmdl";

		public override string PickupSound => "smg_pickup";

		public override void Spawn()
		{
			base.Spawn();

			SetModel( WorldModelPath );
		}

		public override void OnCarryStart( Entity carrier )
		{
			base.OnCarryStart( carrier );

			if ( PickupTrigger.IsValid() )
			{
				PickupTrigger.EnableTouch = false;

				var ply = carrier as ZPS2Player;
				ply.GiveAmmo( AmmoType.Pistol, 12 );
			}
		}
	}
}
