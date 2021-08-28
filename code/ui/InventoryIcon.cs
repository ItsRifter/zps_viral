
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace ZPS_Viral
{
	class InventoryIcon : Panel
	{
		public WeaponBase Weapon;
		public Panel Icon;

		public InventoryIcon( WeaponBase weapon )
		{
			Weapon = weapon;
			Icon = Add.Panel( "icon" );
			AddClass( weapon.ClassInfo.Name );
		}

		internal void TickSelection( WeaponBase selectedWeapon )
		{
			SetClass( "active", selectedWeapon == Weapon );
			SetClass( "empty", !Weapon?.IsUsable() ?? true );
		}

		public override void Tick()
		{
			base.Tick();

			if ( !Weapon.IsValid() || Weapon.Owner != Local.Pawn )
				Delete();
		}
	}
}
