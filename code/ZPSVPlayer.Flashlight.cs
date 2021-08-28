using Sandbox;
using System;


namespace ZPS_Viral
{
	partial class ZPSVPlayer
	{
		public static bool AllowFlashlight { get; set; } = false;

		private Flashlight flashlight;

		private void EnableFlashlight( bool shouldEnable )
		{
			if ( !AllowFlashlight )
				return;

			flashlight.worldLight.SetParent( this.Owner, "head", new Transform( Vector3.Forward * 25 ) );
			flashlight.worldLight.Enabled = shouldEnable;
		}
	}
}
