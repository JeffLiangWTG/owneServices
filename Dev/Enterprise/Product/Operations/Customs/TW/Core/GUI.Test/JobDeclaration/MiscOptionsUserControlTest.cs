using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class MiscOptionsUserControlTest : TestCaseWithFactory
	{
		public void TestControlsVisibility_Import()
		{
			AssertControlsVisibility(JobMessageTypeList.Codes.Import);
		}

		public void TestControlsVisibility_Export()
		{
			AssertControlsVisibility(JobMessageTypeList.Codes.Export);
		}

		void AssertControlsVisibility(ZString messageType)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				if (jobDeclarationUserControl != null)
				{
					brokerageControl.MainTabControl.SelectedTab = brokerageControl.MiscOptionsTabPage;
					var miscControl = brokerageControl.MiscOptionsTabPage.Controls.OfType<MiscOptionsUserControl>().First();
					var paymentPartyDropEdit = miscControl.FindSingle<ZDropEdit>("PaymentPartyDropEdit");
					AssertEquals("Payment Method should not be visible", false, paymentPartyDropEdit.Visible);
					var brokerCodeFindBox = miscControl.Controls.Find("BrokerCodeFindBox", true)[0] as ZCodeFindBox;
					AssertEquals("Broker Code should not be visible", false, brokerCodeFindBox.Visible);
					var paidByDropEdit = miscControl.Controls.Find("PaidByDropEdit", true)[0] as ZDropEdit;
					AssertEquals("paid By should be visible", true, paidByDropEdit.Visible);
				}
			}
		}

		public void TestServiceLevelVisibility()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				if (jobDeclarationUserControl != null)
				{
					brokerageControl.MainTabControl.SelectedTab = brokerageControl.MiscOptionsTabPage;
					var miscControl = brokerageControl.MiscOptionsTabPage.Controls.OfType<MiscOptionsUserControl>().First();
					var serviceLevelControl = miscControl.FindSingleOrDefault<ZCodeFindBox>(x => x.Name == "JE_RS_NKServiceLevelBoundFindBox");
					Assert(!serviceLevelControl.Visible);
				}
			}

			declaration.JE_JS = ZGuid.Empty;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				if (jobDeclarationUserControl != null)
				{
					brokerageControl.MainTabControl.SelectedTab = brokerageControl.MiscOptionsTabPage;
					var miscControl = brokerageControl.MiscOptionsTabPage.Controls.OfType<MiscOptionsUserControl>().First();
					var serviceLevelControl = miscControl.FindSingleOrDefault<ZCodeFindBox>(x => x.Name == "JE_RS_NKServiceLevelBoundFindBox");
					Assert(serviceLevelControl.Visible);
				}
			}
		}
	}
}
