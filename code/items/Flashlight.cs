using Sandbox;

namespace ZPS2
{
	[Library( "zps2_flashlight", Title = "Flashlight" )]
	partial class Flashlight : ItemBase
	{
		protected virtual Vector3 LightOffset => Vector3.Forward * 15;

		public SpotLightEntity worldLight;
		public SpotLightEntity viewLight;

		[Net, Local, Predicted]
		public bool LightEnabled { get; set; } = true;

		public TimeSince timeSinceLightToggled;

		public override void Spawn()
		{
			base.Spawn();

			worldLight = CreateLight();
			worldLight.EnableHideInFirstPerson = true;
			worldLight.Enabled = false;
		}

		public SpotLightEntity CreateLight()
		{
			var light = new SpotLightEntity
			{
				Enabled = true,
				DynamicShadows = false,
				Range = 512,
				Falloff = 1.0f,
				LinearAttenuation = 0.0f,
				QuadraticAttenuation = 1.0f,
				Brightness = 2,
				Color = Color.White,
				InnerConeAngle = 15,
				OuterConeAngle = 30,
				FogStength = 0.0f,
				Owner = Owner,
			};

			return light;
		}

		private void Activate()
		{
			if ( worldLight.IsValid() )
			{
				worldLight.Enabled = LightEnabled;
			}
		}

		private void Deactivate()
		{
			if ( worldLight.IsValid() )
			{
				worldLight.Enabled = false;
			}
		}

		public override void ActiveStart( Entity ent )
		{
			base.ActiveStart( ent );

			if ( IsServer )
			{
				Activate();
			}
		}

		public override void ActiveEnd( Entity ent, bool dropped )
		{
			base.ActiveEnd( ent, dropped );

			if ( IsServer )
			{
				if ( dropped )
				{
					Activate();
				}
				else
				{
					Deactivate();
				}
			}
		}
	}
}
