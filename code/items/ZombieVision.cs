using Sandbox;

namespace ZPS_Viral
{
	partial class ZombieVision : WeaponBase
	{
		protected virtual Vector3 LightOffset => Vector3.Up * 5;

		public SpotLightEntity viewLight;

		[Net, Local, Predicted] public bool LightEnabled { get; set; } = true;

		public TimeSince timeSinceLightToggled;

		public override void CreateViewModel()
		{
			base.CreateViewModel();

			viewLight = CreateLight();
			viewLight.SetParent( ViewModelEntity, null, new Transform( LightOffset ) );
			viewLight.EnableViewmodelRendering = true;
			viewLight.Enabled = LightEnabled;
		}

		private SpotLightEntity CreateLight()
		{
			var light = new SpotLightEntity
			{
				Enabled = true,
				DynamicShadows = false,
				Range = 512,
				Falloff = 0f,
				LinearAttenuation = 0.0f,
				QuadraticAttenuation = 1.0f,
				Brightness = 1,
				Color = Color.Red,
				InnerConeAngle = 20,
				OuterConeAngle = 40,
				FogStength = 0f,
				Owner = Owner,
			};
			
			return light;
		}
	}
}
