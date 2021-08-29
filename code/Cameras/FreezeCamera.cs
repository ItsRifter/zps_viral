﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using ZPS_Viral.Entities;

namespace ZPS_Viral
{
	public class FreezeCamera : Camera
	{
		Vector3 lastPos;

		public override void Activated()
		{
			List<Entity> cameraSpots = new List<Entity>();

			foreach(var point in Entity.All.OfType<SurvivorPoint>())
			{
				cameraSpots.Add( point );
			}

			if ( cameraSpots.Count <= 0 )
			{
				Log.Info( "Error" );
				return;
			}
				

			Pos = cameraSpots[Rand.Int( 0, cameraSpots.Count - 1 )].Position;
			Rot = cameraSpots[Rand.Int( 0, cameraSpots.Count - 1 )].Rotation;

			lastPos = Pos;
		}

		public override void Update()
		{
			lastPos = Pos;
		}
	}
}