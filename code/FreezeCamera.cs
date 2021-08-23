using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using ZPS2.Entities;

namespace ZPS2
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
