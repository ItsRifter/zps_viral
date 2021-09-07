using Sandbox;
using System;

namespace ZPS_Viral
{
	partial class ItemBase : BaseCarriable, IRespawnableEntity
	{
		public virtual AmmoType ammoType => AmmoType.Pistol; 
		
		public virtual string WorldModelPath { get; set; }
		
		public virtual string PickupSound => "ammo_pickup";

		public virtual int ArmorPoints { get; set; } = 0;
		
		public virtual int RemainingAmmo { get; set; } = 1;

		public virtual float Weight => 0f;

		public PickupTrigger PickupTrigger { get; protected set; }

		public override void Spawn()
		{
			base.Spawn();
		}
		
		public override bool CanCarry( Entity carrier )
		{
			if(carrier is ZPSVPlayer player)
			{
				if ( player.CurTeam == ZPSVPlayer.TeamType.Undead || player.CurTeam == ZPSVPlayer.TeamType.Infected )
					return false;
			}

			return true;
		}
		
		public override void OnCarryStart( Entity carrier )
		{
			base.OnCarryStart( carrier );
			
			using ( Prediction.Off() )
			{
				PlaySound( PickupSound );
			}

			if ( carrier is ZPSVPlayer ply )
			{
				ply.GiveAmmo( ammoType, RemainingAmmo );
			}
		}
	}
}
