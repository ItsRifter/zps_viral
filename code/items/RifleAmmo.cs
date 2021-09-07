using Sandbox;
namespace ZPS_Viral
{
	[Library( "zpsviral_ammo_rifle", Title = "Rifle Ammo Box" )]

	[Hammer.EntityTool( "Rifle Ammo Box", "Ammo", "Rifle ammo box" )]
	[Hammer.EditorModel( "models/ammo/rifle_ammo.vmdl" )]
	partial class RifleAmmo : ItemBase
	{
		public override string WorldModelPath => "models/ammo/rifle_ammo.vmdl";
		
		public override AmmoType ammoType => AmmoType.Rifle; 
		public override int RemainingAmmo { get; set; } = 30;
		
		public override float Weight => 6f;
		public override void Spawn()
		{
			base.Spawn();

			SetModel( WorldModelPath );
		}
	}
}
