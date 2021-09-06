
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;


namespace ZPS_Viral
{
	[Library]
	public partial class ZPSViralHud : HudEntity<RootPanel>
	{
		public ZPSViralHud()
		{
			if ( !IsClient )
				return;
			
			RootPanel.AddChild<Vitals>().StyleSheet.Load( "/ui/ZPSViralHud.scss" );;

			RootPanel.AddChild<Ammo>().StyleSheet.Load( "/ui/ZPSViralHud.scss" );;

			RootPanel.AddChild<Crosshair>();
			
			RootPanel.AddChild<NameTags>();
			RootPanel.AddChild<DamageIndicator>();
			RootPanel.AddChild<HitIndicator>();

			RootPanel.AddChild<InventoryBar>();

			RootPanel.AddChild<ChatBox>();
			RootPanel.AddChild<KillFeed>();
			RootPanel.AddChild<Scoreboard>();
			RootPanel.AddChild<VoiceList>();
			RootPanel.AddChild<Lives>();
		}

		[ClientRpc]
		public void OnPlayerDied( string victim, string attacker = null )
		{
			Host.AssertClient();
		}

		[ClientRpc]
		public void ShowDeathScreen( string attackerName )
		{
			Host.AssertClient();
		}
	}
}
