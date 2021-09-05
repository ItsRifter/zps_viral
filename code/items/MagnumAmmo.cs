using Sandbox;
namespace ZPS_Viral
{
	[Library( "zpsviral_ammo_magnum", Title = "Magnum Ammo Box" )]

	[Hammer.EntityTool( "Magnum Ammo Box", "Ammo", "Magnum ammo box" )]
	[Hammer.EditorModel( "models/ammo/magnum_ammo.vmdl" )]
	partial class MagnumAmmo : ItemBase
	{
		public override string WorldModelPath => "models/ammo/magnum_ammo.vmdl";
		
		public override AmmoType ammoType => AmmoType.Magnum; 
		public override int RemainingAmmo { get; set; } = 6;
		public override void Spawn()
		{
			base.Spawn();

			SetModel( WorldModelPath );
		}
	}
}
