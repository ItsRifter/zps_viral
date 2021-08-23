using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ZPS2.Entities;

namespace ZPS2
{
	[Library( "ZPS2" )]
	public partial class ZPS2Game : Sandbox.Game
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

		public ZPS2Game()
		{
			if ( IsServer )
			{
				new ZPS2Hud();

				List<Vector3> cameraVector = new List<Vector3>();
				List<Rotation> cameraRotation = new List<Rotation>();

				foreach ( var point in Entity.All.OfType<SurvivorPoint>() )
				{
					Log.Info( "Hi" );
					cameraVector.Add( point.Position );
					cameraRotation.Add( point.Rotation );
				}

				Log.Info( cameraVector.Count );
				Log.Info( cameraRotation.Count );
			}
		}
		[Event("StartGame")]
		public void BeginGame()
		{
			foreach(var p in Entity.All.OfType<ZPS2Player>())
			{
				if ( p.CurTeam == ZPS2Player.TeamType.Unassigned )
					break;

				p.Camera = null;
				p.Camera = new FreezeCamera();
			}

			PlaySound( "round_begin" );

			CurState = RoundState.Start;
			TimeCurLeft = 10f;
		}

		public void StartActiveGame()
		{
			foreach( var p in Entity.All.OfType<ZPS2Player>() )
			{
				p.Camera = null;
				p.Camera = new FirstPersonCamera();
			}
		}

		public void StopGame()
		{
			CurState = RoundState.Idle;
		}

		public void RestartGame()
		{
			ResetAllPlayers();
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

		}

		//Turn a human player into a carrier zombie
		public static void TransformRandomHuman()
		{
			var infCheck = GetInfected();
			
			if(infCheck.Count < 1)
			{
				var randHuman = GetSurvivors();
				int randomInt = Rand.Int( 0, randHuman.Count - 1 );

				while ( randHuman[randomInt] == null )
				{
					randomInt = Rand.Int( 0, randHuman.Count - 1 );
				}

				randHuman[randomInt].SwapTeam( ZPS2Player.TeamType.Undead );
				randHuman[randomInt].CurZombieType = ZPS2Player.ZombieType.Carrier;
				randHuman[randomInt].Respawn();
			} else
			{
				infCheck[Rand.Int(0, infCheck.Count - 1)].CurZombieType = ZPS2Player.ZombieType.Carrier;
				foreach ( var zombie in infCheck )
				{
					zombie.Respawn();
				}
			}
	
			CurState = RoundState.Active;
		}

		public void ResetAllPlayers()
		{
			foreach ( var player in Client.All )
			{
				if ( player.Pawn is ZPS2Player ply )
				{
					if ( ply.CurTeam == ZPS2Player.TeamType.Unassigned )
						break;

					ply.SwapTeam( ZPS2Player.TeamType.Unassigned );
					ply.InitialSpawn();
				}
			}
		}

		public static List<ZPS2Player> GetSurvivors()
		{
			var curHumans = new List<ZPS2Player>();

			foreach ( var p in Entity.All.OfType<ZPS2Player>() )
			{
				if ( p.CurTeam == ZPS2Player.TeamType.Survivor )
					curHumans.Add( p );
			}

			return curHumans;
		}

		public static List<ZPS2Player> GetInfected()
		{
			var curInfected = new List<ZPS2Player>();

			foreach ( var p in Entity.All.OfType<ZPS2Player>() )
			{
				if ( p.CurTeam == ZPS2Player.TeamType.Infected )
					curInfected.Add( p );
			}

			return curInfected;
		}

		public static List<ZPS2Player> GetZombies()
		{
			var curZombies = new List<ZPS2Player>();

			foreach ( var p in Entity.All.OfType<ZPS2Player>() )
			{
				if ( p.CurTeam == ZPS2Player.TeamType.Undead )
					curZombies.Add( p );
			}

			return curZombies;
		}

		public static void CheckRoundStatus()
		{
			var humans = GetSurvivors();
			var infected = GetInfected();
			var zombies = GetZombies();

			//Start round checks
			//if ( humans.Count >= 2 || humans.Count >= 1 && infected.Count >= 1 )
			//TEMPORARY
			if ( humans.Count >= 1 )
				Event.Run( "StartGame" );


			//End win checks
			if ( (humans.Count > 0 || zombies.Count > 0 || infected.Count > 0) && (CurState == RoundState.Idle || CurState == RoundState.Start) )
				return;
			else if (humans.Count <= 0 && CurState == RoundState.Active)
				Event.Run( "evnt_endgame", false );
			else if (zombies.Count <= 0 && ZombieLives <= 0 && CurState == RoundState.Active )
				Event.Run( "evnt_endgame", true );
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

			foreach ( var p in Entity.All.OfType<ZPS2Player>() )
			{
				p.PlaySound( filePath );
			}

			Log.Info( isHumanWin );

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

			foreach(var p in Entity.All.OfType<ZPS2Player>())
			{
				using ( Prediction.Off() )
				{
					p.PlaySound( filePath );
				}
			}
		}

		[ServerCmd( "zps2debug" )]
		public static void SwitchDebugMode( bool turnOn )
		{
			DebugMode = turnOn;
		}

		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			var player = new ZPS2Player();
			client.Pawn = player;

			player.InitialSpawn();
		}
		public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
		{
			base.ClientDisconnect( cl, reason );

			CheckRoundStatus();
		}

		//DEBUG: Infects player, Will remove later
		[ServerCmd( "infect" )]
		public static void StartInfect()
		{
			var Human = ConsoleSystem.Caller as ZPS2Player;
			Event.Run( "InfectHuman" );
		}

		public override void DoPlayerNoclip( Client player )
		{
			if ( DebugMode == false )
				return;

			base.DoPlayerNoclip( player );
		}
	}
}
