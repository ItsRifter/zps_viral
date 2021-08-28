
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;


namespace ZPS_Viral
{
	[Library]
	public partial class ZPS2Hud : HudEntity<RootPanel>
	{
		public ZPS2Hud()
		{
			if ( !IsClient )
				return;

			RootPanel.AddChild<Vitals>();

			RootPanel.AddChild<Ammo>().StyleSheet.Load( "/ui/ZPS2Hud.scss" );

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
