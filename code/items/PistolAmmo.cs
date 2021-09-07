using Sandbox;
namespace ZPS_Viral
{
	[Library( "zpsviral_ammo_9mm", Title = "9mm Ammo Box" )]

	[Hammer.EntityTool( "9mm Ammo Box", "Ammo", "Small 9mm ammo box" )]
	[Hammer.EditorModel( "models/ammo/pistol_ammo.vmdl" )]
	partial class PistolAmmo : ItemBase
	{
		public override string WorldModelPath => "models/ammo/pistol_ammo.vmdl";
		
		public override AmmoType ammoType => AmmoType.Pistol;
		public override int RemainingAmmo { get; set; } = 15;
		public override float Weight => 3f;

		public bool shouldBePickedUpFirst = false;
		
		public override void Spawn()
		{
			base.Spawn();

			SetModel( WorldModelPath );
		}
	}
}
