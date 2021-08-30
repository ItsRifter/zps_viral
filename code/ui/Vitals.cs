
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace ZPS_Viral
{
	public class Vitals : Panel
	{
		public Label Health;
		public Label Team;
		public Panel Flashlight;

		public Vitals()
		{
			StyleSheet.Load( "/ui/Vitals.scss" );
			Health = Add.Label( "100", "health" );
			Team = Add.Label( "", "team" );
			Flashlight = Add.Panel( "flashlight" );
		}

		[Event( "client.tick" )]
		public void Tick()
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
					}
				}

				
				if(k.Equals("Infected"))
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
				}
			}

			Health.Text = $"{player.Health.CeilToInt()}";
			Health.SetClass( "danger", player.Health < 40.0f );
		}
	}
}
