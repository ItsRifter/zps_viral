using Sandbox;
namespace ZPS_Viral
{
	[Library( "zpsviral_ammo_9mm", Title = "9mm Ammo Box" )]

	[Hammer.EntityTool( "9mm Ammo Box", "Ammo", "Small 9mm ammo box" )]
	[Hammer.EditorModel( "models/ammo/pistol_ammo.vmdl" )]
	partial class PistolAmmo : ItemBase
	{
		public override string WorldModelPath => "models/ammo/pistol_ammo.vmdl";
		public override int RemainingAmmo { get; set; } = 12;
		public override void Spawn()
		{
			base.Spawn();

			SetModel( WorldModelPath );
		}

		public override void OnCarryStart( Entity carrier )
		{
			base.OnCarryStart( carrier );
			
			if(carrier is ZPSVPlayer ply)
				ply.GiveAmmo( AmmoType.Pistol, RemainingAmmo );
		}
	}
}
