using System;
using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZPS_Viral.Entities
{
	[Library( "zpsviral_setmode", Description = "Sets the mode [Survival/Objective]" )]
	[Hammer.Solid]
	[Hammer.AutoApplyMaterial( "materials/tools/toolstrigger.vmat" )]
	public partial class AutoRun : Entity
	{
		[Flags]
		public enum Flags
		{
			IsSurvival = 1,
			IsObjective = 2,
		}
		
		[Property( "mapspawn", Title = "Start map on this mode - DO NOT SET BOTH ON", FGDType = "flags" )]
		public Flags SpawnSettings { get; set; } = Flags.IsSurvival;

		public override void Spawn()
		{
			if ( SpawnSettings == Flags.IsSurvival )
				ZPSVGame.MapMode = ZPSVGame.Mode.Survival;
			else
				ZPSVGame.MapMode = ZPSVGame.Mode.Objective;
			
			Delete();
		}
	}
}
