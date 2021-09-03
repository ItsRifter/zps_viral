using Sandbox;
namespace ZPS_Viral
{
	[Library( "zpsviral_ammo_buckshot", Title = "Buckshot Ammo Box" )]

	[Hammer.EntityTool( "Buckshot Ammo Box", "Ammo", "Buckshot ammo box" )]
	[Hammer.EditorModel( "models/ammo/shotgun_ammo.vmdl" )]
	partial class ShotgunAmmo : ItemBase
	{
		public string WorldModelPath => "models/ammo/shotgun_ammo.vmdl";
		
		public override AmmoType ammoType => AmmoType.Buckshot; 
		public override int RemainingAmmo { get; set; } = 6;
		public override void Spawn()
		{
			base.Spawn();

			SetModel( WorldModelPath );
		}
	}
}
