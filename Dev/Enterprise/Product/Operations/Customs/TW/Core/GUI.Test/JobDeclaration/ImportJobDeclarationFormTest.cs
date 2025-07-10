using System;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class ImportJobDeclarationFormTest : JobDeclarationFormTest
	{
		public override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;

		protected override JobDeclaration GetPopulatedDeclarationForFormBashingCore()
		{
			var declaration = base.GetPopulatedDeclarationForFormBashingCore();
			return declaration;
		}

		public void TestPackingDetailsIsAvailable()
		{
			using (TWCustomsDataRegistry.Instance.EnableCustomsDeclarationPackingList.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				using (JobDeclarationForm form = (JobDeclarationForm)GetFormToBash())
				{
					form.Show();
					var brokerageControl = form.CustomsBrokerageUserControl;
					var declaration = form.Declaration;
					declaration.JE_MessageType = MessageTypeForFormBashing;
					AssertEquals("brokerageControl.PackingTabPage.TabVisible should be True when the message type is " + MessageTypeForFormBashing + " and the customs declaration packingList is enabled.", true, brokerageControl.PackingTabPage.TabVisible);
					declaration.JE_MessageType = ZString.Empty;
					AssertEquals("brokerageControl.PackingTabPage.TabVisible should be True when the message type is empty and the customs declaration packingList is enabled", true, brokerageControl.PackingTabPage.TabVisible);
				}
			}

			using (TWCustomsDataRegistry.Instance.EnableCustomsDeclarationPackingList.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				using (JobDeclarationForm form = (JobDeclarationForm)GetFormToBash())
				{
					form.Show();
					var brokerageControl = form.CustomsBrokerageUserControl;
					var declaration = form.Declaration;
					declaration.JE_MessageType = MessageTypeForFormBashing;
					AssertEquals("brokerageControl.PackingTabPage.TabVisible should be True when the message type is " + MessageTypeForFormBashing + " and the customs declaration packingList is disabled.", true, brokerageControl.PackingTabPage.TabVisible);
					declaration.JE_MessageType = ZString.Empty;
					AssertEquals("brokerageControl.PackingTabPage.TabVisible should be True when the message type is empty and the customs declaration packingList is disabled", true, brokerageControl.PackingTabPage.TabVisible);
				}
			}
		}
	}
}
