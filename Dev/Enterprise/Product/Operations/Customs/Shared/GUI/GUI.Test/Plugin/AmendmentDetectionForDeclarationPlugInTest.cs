using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.PlugIn.Testing
{
	sealed class AmendmentDetectionForDeclarationPlugInTest : AmendmentDetectionOnSavingAbstractTest
	{
		protected override IDecFormOrPlugIn GetDeclarationFormOrPlugIn(BaseJobDeclaration declaration)
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			return new TestPlugIn(shipment);
		}

		sealed class TestPlugIn : BaseBrokeragePlugIn, IDecFormOrPlugIn
		{
			public TestPlugIn(ForwardingShipment shipment)
				: base(shipment)
			{
			}

			protected override MenuItem GetNewTopLevelMenuCore() => new EDIMenu();

			public SendsMessagesToCustomsGUI controller;
			protected override SendsMessagesToCustomsGUI GetNewMessagingActionsController() => controller ?? base.GetNewMessagingActionsController();

			ContinueWithSave IShowPreSaveDialog.ShowPreSaveDialogs() => ShowPreSaveDialogs();

			SendsMessagesToCustomsGUI IDecFormOrPlugIn.Controller
			{
				get { return GetNewMessagingActionsController(); }
				set { controller = value; }
			}
		}
	}
}
