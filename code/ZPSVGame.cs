using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ZPS_Viral.Entities;

namespace ZPS_Viral
{
	[Library( "Zombie Panic Viral" )]
	public partial class ZPSVGame : Sandbox.Game
	{
		[Net]
		private static float TimeCurLeft { get; set; }

		[Net]
		public static RoundState CurState { get; set; } = RoundState.Idle;

		private int NextThink;

		private static bool DebugMode = false;

		public static float InfectionChance = 80f;

		[Net]
		public static int ZombieLives { get; set; } = 4;

		private MusicPlayer MusicPlayer;
		
		/*Round statuses:
			Idle = Not enough players are in the game
			Start = Initial setup
			Active = Currently playing the round
			Post = End round
		*/
		public enum RoundState
		{
			Idle,
			Start,
			Active,
			Post
		}
		
		public static List<Vector3> spectateSpawnsPos;
		public static List<Rotation> spectateSpawnsRot;
		
		public ZPSVGame()
		{
			if ( IsServer )
			{
				new ZPSViralHud();
				MusicPlayer = new MusicPlayer();
			}
		}
		[Event("StartGame")]
		public void BeginGame()
		{
			Sound.FromScreen( "round_begin" );

			if(DebugMode)
			{
				CurState = RoundState.Active;
				StartActiveGame();
				GiveRandomSurvivorWeapons();
				return;
			}

			CurState = RoundState.Start;
			TimeCurLeft = 10f;
		}

		public void StartActiveGame()
		{
			foreach ( var p in Entity.All.OfType<ZPSVPlayer>() )
			{
				p.Camera = null;
				p.Camera = new FirstPersonCamera();
			}

			if(!MusicPlayer.IsPlaying)
				MusicPlayer.PlayRandomMusic();
		}

		public void StopGame()
		{
			CurState = RoundState.Idle;
		}

		public void RestartGame()
		{
			ResetAllPlayers();
			ZombieLives = 4;
			CurState = RoundState.Idle;
		}

		[Event.Tick]
		public void UpdateTime()
		{
			if ( Host.IsClient )
				return;

			if ( NextThink <= 0f )
			{
				TimeCurLeft -= 1;
				NextThink = 60;
			}
			else
				NextThink--;

			if ( TimeCurLeft <= 0f && CurState == RoundState.Start )
			{
				TransformRandomHuman();
				CurState = RoundState.Active;
				StartActiveGame();
				return;
			}

			if ( TimeCurLeft <= 0f && CurState == RoundState.Post )
			{
				RestartGame();
			}
		}

		//Turn a human player into a carrier zombie
		public static void TransformRandomHuman()
		{
			var zombCheck = GetZombies();
			
			if( zombCheck.Count < 1)
			{
				var randHuman = GetSurvivors();
				int randomInt = Rand.Int( 0, randHuman.Count - 1 );

				while ( randHuman[randomInt] == null )
				{
					randomInt = Rand.Int( 0, randHuman.Count - 1 );
				}

				randHuman[randomInt].SwapTeam( ZPSVPlayer.TeamType.Undead );
				randHuman[randomInt].CurZombieType = ZPSVPlayer.ZombieType.Carrier;
				randHuman[randomInt].Respawn();
			} else
			{
				zombCheck[Rand.Int(0, zombCheck.Count - 1)].CurZombieType = ZPSVPlayer.ZombieType.Carrier;
				foreach ( var zombie in zombCheck )
				{
					zombie.Respawn();
				}
			}

			GiveRandomSurvivorWeapons();
			CurState = RoundState.Active;
		}

		public static void GiveRandomSurvivorWeapons()
		{
			var survivors = GetSurvivors();

			foreach(var human in survivors)
			{
				human.GiveWeapons();
			}
		}

		public void ResetAllPlayers()
		{
			foreach ( var player in Client.All )
			{
				if ( player.Pawn is ZPSVPlayer ply )
				{
					if ( ply.CurTeam == ZPSVPlayer.TeamType.Unassigned )
						break;

					ply.SwapTeam( ZPSVPlayer.TeamType.Unassigned );
					ply.Inventory.DeleteContents();
					ply.InitialSpawn();
				}
			}
		}

		public static List<ZPSVPlayer> GetSurvivors()
		{
			var curHumans = new List<ZPSVPlayer>();

			foreach ( var p in Entity.All.OfType<ZPSVPlayer>() )
			{
				if ( p.CurTeam == ZPSVPlayer.TeamType.Survivor )
					curHumans.Add( p );
			}

			return curHumans;
		}

		public static List<ZPSVPlayer> GetInfected()
		{
			var curInfected = new List<ZPSVPlayer>();

			foreach ( var p in Entity.All.OfType<ZPSVPlayer>() )
			{
				if ( p.CurTeam == ZPSVPlayer.TeamType.Infected )
					curInfected.Add( p );
			}

			return curInfected;
		}

		public static List<ZPSVPlayer> GetZombies()
		{
			var curZombies = new List<ZPSVPlayer>();

			foreach ( var p in Entity.All.OfType<ZPSVPlayer>() )
			{
				if ( p.CurTeam == ZPSVPlayer.TeamType.Undead )
					curZombies.Add( p );
			}

			return curZombies;
		}

		public static void CheckRoundStatus()
		{
			var humans = GetSurvivors();
			var infected = GetInfected();
			var zombies = GetZombies();
			
			if(DebugMode)
			{
				if ( humans.Count >= 1 || humans.Count >= 1 && zombies.Count >= 1 )
					Event.Run( "StartGame" );
			}
			
			//Start round checks
			if ( CurState == RoundState.Idle )
			{
				if ( humans.Count >= 2 || humans.Count >= 1 && zombies.Count >= 1 ) 
					Event.Run( "StartGame" );
			}
			
			//End round checks
			if ( CurState == RoundState.Active )
			{
				if ( humans.Count <= 0 && infected.Count <= 0 )
                	Event.Run( "evnt_endgame", false );
                else if (zombies.Count <= 0 && ZombieLives <= 0 && infected.Count <= 0 )
                	Event.Run( "evnt_endgame", true );
			}
		}

		[Event( "evnt_endgame")]
		public void EndGame(bool isHumanWin)
		{
			CurState = RoundState.Post;

			String filePath;

			if ( isHumanWin )
				filePath = "round_end_human";
			else
				filePath = "round_end_zombie";

			Sound.FromScreen( filePath );

			TimeCurLeft = 10f;
		}

		[ServerCmd("endstate")]
		public static void endstate( bool isHumanWin )
		{
			String filePath;
			if ( isHumanWin )
				filePath = "round_end_human";
			else
				filePath = "round_end_zombie";

			foreach(var p in Entity.All.OfType<ZPSVPlayer>())
			{
				Sound.FromScreen( filePath );
			}
		}

		[ServerCmd( "zpsviral_debug" )]
		public static void SwitchDebugMode( bool turnOn )
		{
			DebugMode = turnOn;
		}

		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			var player = new ZPSVPlayer();
			client.Pawn = player;

			player.InitialSpawn();
		}
		public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
		{
			base.ClientDisconnect( cl, reason );

			CheckRoundStatus();
		}

		[Event("noclip")]
		public override void DoPlayerNoclip( Client user )
		{
			base.DoPlayerNoclip( user );
		}
	}
}
