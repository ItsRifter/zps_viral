using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;


namespace ZPS_Viral
{
	public class Ammo : Panel
	{
		public Label Weapon;
		public Label PistolAmmo;
		public Label BuckshotAmmo;
		public Label RifleAmmo;
		public Label MagnumAmmo;

		private string ammoType;
		
		public Ammo()
		{
			Weapon = Add.Label( "", "weapon" );
			PistolAmmo = Add.Label( "", "pistol" );
			BuckshotAmmo = Add.Label( "", "buckshot" );
			RifleAmmo = Add.Label( "", "rifle" );
			MagnumAmmo = Add.Label( "", "magnum" );
			
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			if ( pawn is ZPSVPlayer player )
				ammoType = player.AmmoTypeToDrop;
		}

		public override void Tick()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			if ( pawn is ZPSVPlayer player )
			{

				var weapon = player.ActiveChild as WeaponBase;
				SetClass( "active", true  );

				if ( player.CurTeam == ZPSVPlayer.TeamType.Undead )
				{
					PistolAmmo.Text = "";
					BuckshotAmmo.Text = "";
					RifleAmmo.Text = "";
					MagnumAmmo.Text = "";
				}
				
				if ( weapon == null )
				{
					Weapon.Text = "";
					return;
				}
				
				if ( weapon.IsMelee )
					Weapon.Text = "";
				
				PistolAmmo.Text = "9MM: " +  player.AmmoCount( AmmoType.Pistol );
                BuckshotAmmo.Text = "Shells: " + player.AmmoCount(AmmoType.Buckshot);
                RifleAmmo.Text = "Rifle: " + player.AmmoCount(AmmoType.Rifle);
				MagnumAmmo.Text = "Magnum: " + player.AmmoCount( AmmoType.Magnum );
				
				Weapon.Text = $"{weapon.AmmoClip}";

			}
		}
	}
}
