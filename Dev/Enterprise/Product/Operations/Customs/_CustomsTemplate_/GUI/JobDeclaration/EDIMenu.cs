using System;
using Enterprise.Customs._CustomsTemplate_.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs._CustomsTemplate_.GUI
{
	public class EDIMenu : Customs.GUI.EDIMenu
	{
		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
			set { base.Declaration = value; }
		}

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();
			MenuItems.Add(new ZMenuItem((NoResString)"Send Message", new EventHandler(SendMessageMenuItem_Click)));
		}

		protected void SendMessageMenuItem_Click(object sender, EventArgs e)
		{
			if (Declaration != null)
			{
				// TODO Something like Declaration.MessageManager.DeclareDeclaration(new Customs.GUI.SendsMessagesToCustomsGUI());
			}
		}
	}
}
