using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;


namespace ZPS_Viral
{
	public class Ammo : Panel
	{
		public Label Weapon;
		public Label Inventory;

		public Ammo()
		{
			Weapon = Add.Label( "", "weapon" );
			Inventory = Add.Label( "", "inventory" );
		}

		public override void Tick()
		{
			var player = Local.Pawn;
			if ( player == null ) return;

			var weapon = player.ActiveChild as WeaponBase;
			

			SetClass( "active", weapon != null );
		
			if ( weapon == null )
			{
				Weapon.Text = "";
				Inventory.Text = "";
				return;
			}

			var inv = weapon.AvailableAmmo();
			Inventory.SetClass( "active", inv >= 0 );

			if ( weapon.ToString() == "zpsviral_claws" )
			{
				Weapon.Text = "";
				Inventory.Text = "";
				return;
			}

			Weapon.Text = $"{weapon.AmmoClip}";
			Inventory.Text = $" / {inv}";	
		}
	}
}
