using System.Collections.Generic;
using Sandbox;

namespace ZPS_Viral.Entities
{
	[Library( "item_crate" )]
	[Hammer.EditorModel( "models/misc/itemcrate.vmdl" )]
	[Hammer.EntityTool( "Item Crate", "Ammo", "An item crate which gives specific items" )]

	public class ItemCrate : Prop
	{
		private List<string> PossibleItems;
		
		public string ModelPath => "models/misc/itemcrate.vmdl";
		
		[Property( "ItemToSpawn 1", "What item to spawn, MUST BE CASE SENSITIVE") ]
		public string ItemToGive1 { get; }
	
		[Property( "ItemToSpawn 2", "What item to spawn, MUST BE CASE SENSITIVE") ]
		public string ItemToGive2 { get; }
		
		[Property( "ItemToSpawn 3", "What item to spawn, MUST BE CASE SENSITIVE") ]
		public string ItemToGive3 { get; }
		
		public override void Spawn()
		{
			base.Spawn();
			
			SetModel(ModelPath);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			
			if ( ItemToGive1 != null )
			{
				if ( ItemToGive1.Equals( "PistolAmmo" ) )
                {
                	var ammo = new PistolAmmo();
                    ammo.PhysicsGroup.ApplyImpulse( Velocity + 25 * 250.0f + Vector3.Up * 100.0f, true );
                    ammo.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100.0f, true );
                }
			}

			if ( ItemToGive2 != null )
			{
				
			}
			
			if ( ItemToGive3 != null )
			{
				
			}
		}
	}
}
