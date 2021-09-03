using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The main inventory panel, top left of the screen.
/// </summary>
namespace ZPS_Viral
{

	public class InventoryBar : Panel
	{
		List<InventoryColumn> columns = new();
		List<WeaponBase> Weapons = new();

		public bool IsOpen;
		WeaponBase SelectedWeapon;

		public InventoryBar()
		{
			StyleSheet.Load( "/ui/InventoryBar.scss" );

			for ( int i = 0; i < 6; i++ )
			{
				var icon = new InventoryColumn( i, this );
				columns.Add( icon );
			}
		}

		public override void Tick()
		{
			base.Tick();

			SetClass( "active", true );

			var player = Local.Pawn as Player;
			if ( player == null ) return;

			Weapons.Clear();
			Weapons.AddRange( player.Children.Select( x => x as WeaponBase )
				.Where( x => x.IsValid() ) );

			foreach ( var weapon in Weapons )
			{
				columns[weapon.Bucket].UpdateWeapon( weapon );
			}
		}

		/// <summary>
		/// IClientInput implementation, calls during the client input build.
		/// You can both read and write to input, to affect what happens down the line.
		/// </summary>
		[Event.BuildInput]
		public void ProcessClientInput( InputBuilder input )
		{
			bool wantOpen = IsOpen;

			// If we're not open, maybe this input has something that will 
			// make us want to start being open?
			wantOpen = wantOpen || input.MouseWheel != 0;

			if ( Weapons.Count == 0 )
			{
				IsOpen = false;
				return;
			}

			// We're not open, but we want to be
			if ( IsOpen != wantOpen )
			{
				SelectedWeapon = Local.Pawn.ActiveChild as WeaponBase;
				IsOpen = true;
			}

			// Not open fuck it off
			if ( !IsOpen ) return;
			
			// get our current index
			int SelectedIndex = Weapons.IndexOf( SelectedWeapon );

			// forward if mouse wheel was pressed
			SelectedIndex -= input.MouseWheel;
			SelectedIndex = SelectedIndex.UnsignedMod( Weapons.Count );

			SelectedWeapon = Weapons[SelectedIndex];
			input.ActiveChild = SelectedWeapon;
			
			for ( int i = 0; i < 6; i++ )
			{
				columns[i].TickSelection( SelectedWeapon );
			}

			input.MouseWheel = 0;
		}

		int NextInBucket()
		{
			Assert.NotNull( SelectedWeapon );

			WeaponBase first = null;
			WeaponBase prev = null;
			foreach ( var weapon in Weapons.Where( x => x.Bucket == SelectedWeapon.Bucket )
				.OrderBy( x => x.BucketWeight ) )
			{
				if ( first == null ) first = weapon;
				if ( prev == SelectedWeapon ) return Weapons.IndexOf( weapon );
				prev = weapon;
			}

			return Weapons.IndexOf( first );
		}
	}
}
