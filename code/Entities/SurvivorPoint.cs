using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sandbox;

namespace ZPS2.Entities
{
	/// <summary>
	/// This entity defines the sppawn point of the player in first person shooter gamemodes.
	/// </summary>
	[Library( "info_survivor_start" )]
	[Hammer.EditorModel( "models/editor/playerstart.vmdl" )]
	[Hammer.EntityTool( "Survivor Spawnpoint", "Player", "Defines a point where a survivor can spawn" )]

	public class SurvivorPoint : Entity
	{

	}
}
