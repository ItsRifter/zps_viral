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
			if ( !IsServer )
				return;
			
			if ( !AllowFlashlight )
				return;

			using ( Prediction.Off() )
			{ 
				PlaySound("flashlight_toggle");
			}
		
			flashlight.worldLight.SetParent( Owner, "head", new Transform( Vector3.Forward * 25 ) );
			flashlight.worldLight.Enabled = shouldEnable;
		}

		public bool IsFlashlightOn()
		{
			return flashlight.worldLight.Enabled;
		}
	}
}
