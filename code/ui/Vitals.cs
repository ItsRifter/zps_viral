
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace ZPS_Viral
{
	public class Vitals : Panel
	{
		public Label Health;
		public Panel Icon;
		public Panel Team;

		public Vitals()
		{
			Health = Add.Label( "100", "health" );
			Icon = Add.Panel( "team" );
		}

		public override void Tick()
		{
			var player = Local.Pawn;
			if ( player == null ) return;

			if ( player is ZPSVPlayer ply ) {

				var k = ply.CurTeam.ToString();

				if ( k.Equals( "Unassigned" ) )
				{
					Health.Text = "";
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
				
				if(k.Equals("Infected") && ply.InfectionTime <= 13f && ply.phaseInfection1) 
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
			}

			Health.Text = $"{player.Health.CeilToInt()}";
		}
	}
}
