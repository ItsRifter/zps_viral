using Sandbox;
namespace ZPS_Viral
{
	[Library( "zpsviral_ammo_buckshot", Title = "Buckshot Ammo Box" )]

	[Hammer.EntityTool( "Buckshot Ammo Box", "Ammo", "Buckshot ammo box" )]
	[Hammer.EditorModel( "models/ammo/shotgun_ammo.vmdl" )]
	partial class ShotgunAmmo : ItemBase
	{
		public override string WorldModelPath => "models/ammo/shotgun_ammo.vmdl";
		public override int RemainingAmmo { get; set; } = 8;
		public override void Spawn()
		{
			base.Spawn();

			SetModel( WorldModelPath );
		}

		public override void OnCarryStart( Entity carrier )
		{
			base.OnCarryStart( carrier );
			
			if(carrier is ZPSVPlayer ply)
				ply.GiveAmmo( AmmoType.Buckshot, RemainingAmmo );
		}
	}
}
