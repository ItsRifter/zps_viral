using System.Collections.Generic;
using System.Threading.Tasks;
using Sandbox;
using static ZPS2.ZPS2Player;

namespace ZPS_Viral.Entities
{
	/// <summary>
	/// A simple trigger volume that teleports entities that touch it.
	/// </summary>
	[Library( "trigger_teamselection", Description = "Sets the player to the team of their choosing upon contact." )]
	[Hammer.Solid]
	[Hammer.AutoApplyMaterial( "materials/tools/toolstrigger.vmat" )]
	public partial class TriggerTeams : BaseTrigger
	{

		[Property( "Teleport Destination 01") ]
		public string TargetEntity1 { get; set; }

		[Property( "Teleport Destination 02" )]
		public string TargetEntity2 { get; set; }

		[Property( "Teleport Destination 03" )]
		public string TargetEntity3 { get; set; }

		[Property( "Teleport Destination 04" )]
		public string TargetEntity4 { get; set; }

		[Property( "Teleport Destination 05" )]
		public string TargetEntity5 { get; set; }

		[Property( "Teleport Destination 06" )]
		public string TargetEntity6 { get; set; }

		protected Output OnTriggered { get; set; }

		[Property( Title = "Team to join" )]
		public TeamType SelectedTeam { get; set; } = TeamType.Spectator;

		public override void OnTouchStart( Entity other )
		{
			if ( !Enabled ) return;

			List<Entity> destinations = new List<Entity>();

			//Really bad hardcoded method, should change this to how logic_cases work - garry pls add
			if( TargetEntity1 != null)
				destinations.Add(Entity.FindByName( TargetEntity1 ) );

			if ( TargetEntity2 != null )
				destinations.Add( Entity.FindByName( TargetEntity2 ) );

			if ( TargetEntity3 != null )
				destinations.Add( Entity.FindByName( TargetEntity3 ) );

			if ( TargetEntity4 != null )
				destinations.Add( Entity.FindByName( TargetEntity4 ) );

			if ( TargetEntity5 != null )
				destinations.Add( Entity.FindByName( TargetEntity5 ) );

			if ( TargetEntity6 != null )
				destinations.Add( Entity.FindByName( TargetEntity6 ) );


			if ( destinations != null )
			{
				// Fire the output, before actual teleportation so map IO can do things like disable a trigger_teleport we are teleporting this entity into
				OnTriggered.Fire( other );

				other.Transform = destinations[Rand.Int(0, destinations.Count - 1)].Transform;

				if(other is ZPS2Player)
				{
					var player = other as ZPS2Player;
					player.SwapTeam( SelectedTeam );

					if ( ZPSVGame.CurState == ZPSVGame.RoundState.Idle || ZPSVGame.CurState == ZPSVGame.RoundState.Start )
					{
						player.Camera = null;
						player.Camera = new FreezeCamera();
					}
				}
			}
		}
	}
}
