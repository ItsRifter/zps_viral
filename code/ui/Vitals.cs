
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace ZPS_Viral
{
	public class Vitals : Panel
	{
		public Label Health;
		public Label Armor;
		public Panel ArmorIcon;
		public Panel Icon;
		public Panel Team;

		public Vitals()
		{
			Health = Add.Label( "100", "health" );
			Armor = Add.Label( "0", "armor" );
			Icon = Add.Panel( "team" );
			ArmorIcon = Add.Panel( "armoricon" );
		}

		public override void Tick()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			if ( pawn is ZPSVPlayer player ) 
			{
				var k = player.CurTeam.ToString();

				if ( k.Equals( "Unassigned" ) )
				{
					Health.Text = "";
					Armor.Text = "";
					ArmorIcon.SetClass( "armoricon", false );
					return;
				}
				
				if( k.Equals( "Survivor"))
				{
					if ( !HasClass( "survivor" ) )
					{
						Icon.SetClass( "survivor", true );
						AddClass( "survivor" );
						
						if ( HasClass( "undead" ) )
						{
							RemoveClass( "undead" );
							Icon.SetClass( "undead", false );
						}
					}
				}
				
				if(k.Equals("Infected") && player.phaseInfection1) 
				{
					if(!HasClass("infected"))
					{
						RemoveClass( "survivor" );
						AddClass( "infected" );
						Icon.SetClass( "infected", true );
						Icon.SetClass( "survivor", false);
					}
				}

				if(k.Equals("Undead"))
				{
					if ( !HasClass( "undead" ) )
					{
						if ( HasClass( "survivor" ) )
						{
							RemoveClass( "survivor" );
							Icon.SetClass( "survivor", false );
						}

						if ( HasClass( "infected" ) )
						{
							RemoveClass( "infected" );
							Icon.SetClass( "infected", false );
						}
						
						Icon.SetClass( "undead", true );
						AddClass( "undead" );
					}
					
					
				}
				if ( k.Equals( "Unassigned" ) )
				{
					if ( HasClass( "survivor" ) )
					{
						RemoveClass( "survivor" );
						Icon.SetClass( "survivor", false );
					}

					if ( HasClass( "infected" ) )
					{
						RemoveClass( "infected" );
						Icon.SetClass( "infected", false );
					}

					if ( HasClass( "undead" ) )
					{
						RemoveClass( "undead" );
						Icon.SetClass( "undead", false );
					}
				}
				
				
				ArmorIcon.SetClass( "armoricon", player.ArmorPoints >= 1 );
				
				Armor.Text = $"{player.ArmorPoints}";
				
				if ( player.ArmorPoints <= 0 )
				{
					Armor.Text = "";
				}
				
				
				Health.Text = $"{player.Health.CeilToInt()}";
			}
			
			
		}
	}
}
