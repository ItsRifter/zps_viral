
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace ZPS_Viral
{
	public class Scoreboard : Sandbox.UI.Scoreboard<ScoreboardEntry>
	{

		public Scoreboard()
		{
			StyleSheet.Load( "/ui/Scoreboard.scss" );
		}

		protected override void AddHeader()
		{
			Header = Add.Panel( "header" );
			Header.Add.Label( "Player", "name" );
			Header.Add.Label( "Kills", "kills" );
			Header.Add.Label( "Deaths", "deaths" );
			Header.Add.Label( "Ping", "ping" );
		}
	}

	public class ScoreboardEntry : Sandbox.UI.ScoreboardEntry
	{
		public Label Fps;

		public ScoreboardEntry()
		{
			Fps = Add.Label( "", "fps" );
		}

		public override void UpdateFrom( PlayerScore.Entry entry )
		{
			base.UpdateFrom( entry );

			Fps.Text = entry.Get<int>( "fps", 0 ).ToString();
		}
	}
}
