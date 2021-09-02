using Sandbox;
namespace ZPS_Viral
{
	[Library( "zpsviral_ammo_rifle", Title = "Rifle Ammo Box" )]

	[Hammer.EntityTool( "Rifle Ammo Box", "Ammo", "Rifle ammo box" )]
	[Hammer.EditorModel( "models/ammo/rifle_ammo.vmdl" )]
	partial class RifleAmmo : ItemBase
	{
		public override string WorldModelPath => "models/ammo/rifle_ammo.vmdl";
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
				ply.GiveAmmo( AmmoType.Rifle, RemainingAmmo );
		}
	}
}
