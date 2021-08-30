﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZPS_Viral
{
	partial class Inventory : BaseInventory
	{

		public List<Entity> CurWeapons;

		public Inventory( Player player ) : base( player )
		{
			CurWeapons = new List<Entity>();
		}

		public override bool Add( Entity ent, bool makeActive = false )
		{
			var player = Owner as ZPSVPlayer;
			var weapon = ent as WeaponBase;

			if ( weapon != null && IsCarryingType( ent.GetType() ) )
			{
				var ammo = weapon.AmmoClip;
				var ammoType = weapon.AmmoType;

				if ( ammo > 0 )
				{
					player.GiveAmmo( ammoType, ammo );
				}

				CurWeapons.Add( weapon );

				// Despawn it
				ent.Delete();
				return false;
			}

			return base.Add( ent, makeActive );
		}

		public override bool SetActive( Entity ent )
		{
			if ( Active == ent ) return false;
			if ( !Contains( ent ) ) return false;

			Owner.ActiveChild = ent;
			return true;
		}
		public bool IsCarryingType( Type t )
		{
			return List.Any( x => x.GetType() == t );
		}

		public override Entity DropActive()
		{
			return base.DropActive();
		}

		public List<Entity> GetAllWeapons()
		{
			return CurWeapons;
		}

		public override bool CanAdd( Entity ent )
		{
			foreach( var weapon in GetAllWeapons() )
			{
				if(ent == weapon)
				{
					Log.Info( "Already carrying: " + weapon );
					return false;
				}
			}

			return base.CanAdd( ent );
		}

		public void DropWeapon()
		{

		}
	}
}
