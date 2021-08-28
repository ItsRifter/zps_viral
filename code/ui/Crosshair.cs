
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace ZPS_Viral
{
	public class Crosshair : Panel
	{
		int fireCounter;

		public Crosshair()
		{
			StyleSheet.Load( "/ui/Crosshair.scss" );

			for ( int i = 0; i < 5; i++ )
			{
				var p = Add.Panel( "element" );
				p.AddClass( $"el{i}" );
			}
		}

		public override void Tick()
		{
			base.Tick();

			SetClass( "fire", fireCounter > 0 );

			if ( fireCounter > 0 )
				fireCounter--;
		}
	}
}
