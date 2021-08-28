using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace ZPS_Viral
{
	class Lives : Panel
	{
		public Label LiveText;
		private ZPSVGame LiveChecker;


		public Lives()
		{
			StyleSheet.Load( "/ui/Lives.scss" );
			LiveText = Add.Label( "0", "lives" );
		}

		[Event( "client.tick" )]
		public void Tick()
		{
			var player = Local.Pawn;
			if ( player == null ) return;

			if ( player is ZPSVPlayer ply )
			{
				var k = ply.CurTeam.ToString();
			}
			
		}
	}
}
