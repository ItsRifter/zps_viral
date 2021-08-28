using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace ZPS_Viral
{
	class MusicPlayer
	{
		private string LastPlayed;

		private int MusicDuration;

		public bool IsPlaying;

		private int NextThink;

		public MusicPlayer()
		{
			LastPlayed = "";
			IsPlaying = false;
		}

		public void PlayRandomMusic()
		{
			string[] RandomMusic = new[]
			{
				"music_abandonedmine",
				"music_chapelofunrest",
				"music_cityofsouls",
				"music_cloudofsorrow",
				"music_deepcaverns",
				"music_descent",
				"music_desertofdarkness",
				"music_foggymeadow",
				"music_forgottenkingdom",
				"music_frozenheart",
				"music_frozenwasteland",
				"music_halls",
				"music_hell",
				"music_hollow",
				"music_horde",
				"music_houseofwhispers",
				"music_nightmare",
				"music_orphanage",
				"music_pitchblack",
				"music_plague",
				"music_rebirth",
				"music_sewers",
				"music_subway",
				"music_torturechamber",
				"music_workhazard",
				"music_zomboeing"
			};

			int[] duration =
			{
				204, //Abandoned Mine
				214, //Chapel of Unrest
				212, //City of Souls
				247, //Cloud of Sorrow
				217, //Deep Caverns
				258, //Descent
				259, //Desert of Darkness
				258, //Foggy Meadow
				176, //Forgotten Kingdom
				236, //Frozen Heart
				294, //Frozen Wasteland
				240, //Halls
				329, //Hell
				294, //Hollow
				295, //Horde
				264, //House Of Whispers
				293, //Nightmare
				207, //Orphange
				247, //Pitchblack
				190, //Plague
				244, //Rebirth
				198, //Sewers
				240, //Subway
				182, //Torture Chamber
				287, //Work Hazard
				258, //Zomboeing

			};

			int index = Rand.Int( 0, RandomMusic.Length );

			PlayMusic( RandomMusic[index], duration[index] );
		}

		[Event("server.tick")]
		private void MusicThink()
		{
			if ( !IsPlaying )
				return;

			if (NextThink <= 0f )
			{
				MusicDuration -= 1;
				NextThink = 60;
			}
			else
				NextThink--;

			if( MusicDuration <= 0)
			{
				IsPlaying = false;
				PlayRandomMusic();
			}
		}
		

		private void PlayMusic(string MusicPath, int duration)
		{
			if ( IsPlaying )
				return;

			if ( LastPlayed == MusicPath )
			{
				PlayRandomMusic();
				return;
			}

			LastPlayed = MusicPath;
			MusicDuration = duration;
			IsPlaying = true;

			Log.Info( "Now playing: " + MusicPath );

			Sound.FromScreen( MusicPath );

		}
	}
}
