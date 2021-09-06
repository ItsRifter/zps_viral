using Sandbox;
using System;

namespace ZPS_Viral
{
	partial class ZPSVPlayer
	{
		private ZombieVision vision;

		private void EnableVision( bool shouldEnable )
		{
			if ( !IsServer )
				return;
			
			if ( !AllowFlashlight )
				return;

			using ( Prediction.Off() )
			{ 
				PlaySound("flashlight_toggle");
			}

			if ( vision.viewLight == null )
			{
				vision.CreateViewModel();
			}
			
			vision.viewLight.SetParent( Owner, "head", new Transform( Vector3.Up * 5 ) );
			vision.viewLight.Enabled = shouldEnable;
		}

		public bool IsVisionOn()
		{
			return vision.viewLight.Enabled;
		}
	}
}
