using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sandbox;
using static ZPS_Viral.ZPSVPlayer;

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
		
		[Property( "Teleport Destination 07" )]
		public string TargetEntity7 { get; set; }
		
		[Property( "Teleport Destination 08" )]
		public string TargetEntity8 { get; set; }
		
		protected Output OnTriggered { get; set; }

		[Property( Title = "Team to join" )]
		public TeamType SelectedTeam { get; set; } = TeamType.Spectator;

		private List<Entity> _destinations;
		
		public override void OnTouchStart( Entity other )
		{
			if ( !Enabled ) return;

			_destinations = new List<Entity>();

			//Really bad hardcoded method, should change this to how logic_cases work - garry pls add
			if( TargetEntity1 != null)
				_destinations.Add(FindByName( TargetEntity1 ) );

			if ( TargetEntity2 != null )
				_destinations.Add( FindByName( TargetEntity2 ) );

			if ( TargetEntity3 != null )
				_destinations.Add( FindByName( TargetEntity3 ) );

			if ( TargetEntity4 != null )
				_destinations.Add( FindByName( TargetEntity4 ) );

			if ( TargetEntity5 != null )
				_destinations.Add( FindByName( TargetEntity5 ) );

			if ( TargetEntity6 != null )
				_destinations.Add( FindByName( TargetEntity6 ) );
			
			if ( TargetEntity7 != null )
				_destinations.Add( FindByName( TargetEntity7 ) );
			
			if ( TargetEntity8 != null )
				_destinations.Add( FindByName( TargetEntity8 ) );


			// Fire the output, before actual teleportation so map IO can do things like disable a trigger_teleport we are teleporting this entity into
			OnTriggered.Fire( other );
				
			if(other is ZPSVPlayer player )
			{
				int random = Rand.Int( 0, _destinations.Count - 1 );
					
				player.Position = _destinations[random].Position;
				player.SwapTeam( SelectedTeam );
			}
		}
	}
}
