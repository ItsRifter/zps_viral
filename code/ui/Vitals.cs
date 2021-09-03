
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace ZPS_Viral
{
	public class Vitals : Panel
	{
		public Label Health;
		public Panel Team;

		public Vitals()
		{
			Health = Add.Label( "100", "health" );
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
						AddClass( "survivor" );
						
						if(HasClass("undead"))
							RemoveClass( "undead" );
					}
				}
				
				if(k.Equals("Infected") && ply.InfectionTime <= 13f && ply.phaseInfection1) 
				{
					if(!HasClass("infected"))
					{
						RemoveClass( "survivor" );
						AddClass( "infected" );
					}
				}

				if(k.Equals("Undead"))
				{
					if ( !HasClass( "undead" ) )
					{
						if(HasClass("survivor"))
							RemoveClass( "survivor" );
						
						if(HasClass("infected"))
							RemoveClass( "infected" );

						AddClass( "undead" );
					}
					
					
				}
				if ( k.Equals( "Unassigned" ) )
				{
					if(HasClass("survivor"))
						RemoveClass( "survivor" );

					if ( HasClass( "infected" ) )
						RemoveClass( "infected" );
					
					if(HasClass("undead"))
						RemoveClass( "undead" );
				}
			}

			Health.Text = $"{player.Health.CeilToInt()}";
		}
	}
}
