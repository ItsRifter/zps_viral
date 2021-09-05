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

		public string ammoType;
		
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
					return;
				}
				
				if ( weapon == null )
				{
					Weapon.Text = "";
					return;
				}
				
				Weapon.Text = $"{weapon.AmmoClip}";
				
				if ( weapon.IsMelee )
					Weapon.Text = "";
				
				PistolAmmo.Text = "9MM: " +  player.AmmoCount( AmmoType.Pistol );
                BuckshotAmmo.Text = "Shells: " + player.AmmoCount(AmmoType.Buckshot);
                RifleAmmo.Text = "Rifle: " + player.AmmoCount(AmmoType.Rifle);
				MagnumAmmo.Text = "Magnum: " + player.AmmoCount( AmmoType.Magnum );

				if ( player.AmmoTypeToDrop == "pistol" )
				{
					PistolAmmo.SetClass( "isSelected", true );
				}
				else
				{
					PistolAmmo.SetClass( "isSelected", false );
				}
				
				if ( player.AmmoTypeToDrop == "buckshot" )
				{
					BuckshotAmmo.SetClass( "isSelected", true );
				}
				else
				{
					BuckshotAmmo.SetClass( "isSelected", false );
				}
				
				if ( player.AmmoTypeToDrop == "rifle" )
				{
					RifleAmmo.SetClass( "isSelected", true );
				}
				else
				{
					RifleAmmo.SetClass( "isSelected", false );
				}
				
				if ( player.AmmoTypeToDrop == "magnum" )
				{
					MagnumAmmo.SetClass( "isSelected", true );
				}
				else
				{
					MagnumAmmo.SetClass( "isSelected", false );
				}

				

			}
		}
	}
}
