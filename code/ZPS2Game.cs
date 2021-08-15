using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
			}
		}
		public void BeginGame()
		{
			RespawnAllHumans();
			CurState = RoundState.Start;
			TimeCurLeft = 10f;
		}

		public void StopGame()
		{
			CurState = RoundState.Idle;
		}

		public void RestartGame()
		{
			TimeCurLeft = 10f;
			RespawnAllHumans();
			CurState = RoundState.Start;
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
				return;
			}
		}

		//Turn a human player into a carrier zombie
		public static void TransformRandomHuman()
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

			CurState = RoundState.Active;

		}

		public void RespawnAllHumans()
		{
			foreach ( var player in Client.All )
			{
				if ( player.Pawn is ZPS2Player ply )
				{
					if ( ply.CurTeam == ZPS2Player.TeamType.Unassigned )
						break;

					ply.SwapTeam( ZPS2Player.TeamType.Survivor );
					ply.Respawn();
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
			var zombies = GetZombies();

			if ( humans.Count > 0 || zombies.Count > 0 )
				return;
			else if (humans.Count <= 0)
				Event.Run( "evnt_endgame", false );
			else if (zombies.Count <= 0 && ZombieLives <= 0)
				Event.Run( "evnt_endgame", true );
		}

		[Event( "evnt_endgame")]
		public void EndGame(bool isHumanWin)
		{
			CurState = RoundState.Post;

			if( isHumanWin )
				Sound.FromScreen( "round_end_human.sound" );
			else
				Sound.FromScreen( "round_end_zombie.sound" );

			Log.Info( isHumanWin );

			TimeCurLeft = 10f;
		}

		[ServerCmd("endstate")]
		public static void endstate( bool isHumanWin )
		{
			String filePath;
			if ( isHumanWin )
				filePath = "round_end_human.sound";
			else
				filePath = "round_end_human.sound";

			foreach(var p in Entity.All.OfType<ZPS2Player>())
			{
				using ( Prediction.Off() )
				{
					p.PlaySoundClient( filePath );
				}
			}
	
			Log.Info( isHumanWin );
		}

		[ServerCmd( "zps2debug" )]
		public static void SwitchDebugMode( bool turnOn )
		{
			DebugMode = true;
		}
		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			var player = new ZPS2Player();
			client.Pawn = player;

			player.InitialSpawn();

			if ( Client.All.Count > 1 && CurState == RoundState.Idle )
			{
				Log.Info( "Beginning game" );
				BeginGame();
			}
		}
		public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
		{
			base.ClientDisconnect( cl, reason );

			if ( Client.All.Count > 1 && CurState == RoundState.Idle )
			{
				Log.Info( "Stopping game" );
				StopGame();
			}

			CheckRoundStatus();
		}

		public override void DoPlayerNoclip( Client player )
		{
			if ( DebugMode == false )
				return;

			base.DoPlayerNoclip( player );
		}
	}
}
