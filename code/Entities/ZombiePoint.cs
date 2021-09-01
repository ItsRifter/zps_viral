using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sandbox;

namespace ZPS_Viral.Entities
{
	/// <summary>
	/// This entity defines the sppawn point of the player in first person shooter gamemodes.
	/// </summary>
	[Library( "info_zombie_start" )]
	[Hammer.EditorModel( "models/editor/playerstart.vmdl" )]
	[Hammer.EntityTool( "Zombie Spawnpoint", "Player", "Defines a point where a zombie can spawn" )]

	public class ZombiePoint : Entity
	{

	}
}
