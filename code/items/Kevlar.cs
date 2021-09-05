using Sandbox;
namespace ZPS_Viral
{
	[Library( "zpsviral_misc_kevlar", Title = "Rifle Ammo Box" )]

	[Hammer.EntityTool( "Kevlar Vest", "Health", "Kevlar Vest" )]
	[Hammer.EditorModel( "models/misc/kevlar.vmdl" )]
	partial class Kevlar : ItemBase
	{
		public override string WorldModelPath => "models/misc/kevlar.vmdl";

		public override int ArmorPoints => 50;
		public override int RemainingAmmo { get; set; } = 0;
		
		public override string PickupSound => "kevlar_equip";
		
		public override void Spawn()
		{
			base.Spawn();

			SetModel( WorldModelPath );
		}

		public override void OnCarryStart( Entity carrier )
		{
			if ( carrier is ZPSVPlayer ply )
			{
				ply.GiveAmmo( ammoType, RemainingAmmo );
				
				int check = ArmorPoints + ply.ArmorPoints;

				if ( ply.ArmorPoints >= 100 )
					return;
				
				if ( check > 100 )
				{
					check -= ply.ArmorPoints;
					ArmorPoints -= check;
					ply.ArmorPoints += check;
				}

				ply.ArmorPoints += ArmorPoints;
			}
			
			base.OnCarryStart( carrier );
		}
	}
}
