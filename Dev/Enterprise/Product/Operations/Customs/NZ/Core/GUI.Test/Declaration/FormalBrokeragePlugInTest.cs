using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet;
using Enterprise.Customs.NZ.GUI.Documents;
using Enterprise.Customs.NZ.GUI.MAFeBACCa.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.NZ.GUI.Declaration.Testing
{
	sealed class FormalBrokeragePlugInTest : BrokeragePlugInTest
	{
		protected override JobDeclaration GetNewJobDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}

		protected override ZPlugIn GetPlugInToTest()
		{
			return new BrokeragePlugIn(Shipment);
		}

		public void TestMAFCoverSheetEventHandler()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.DocNames.MAFCoverSheet;
			var eventArgs = new DocumentCancelEventArgs(menuItem);
			var docEvents = new DocumentEventsForTesting();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var shipment = Factory.New<ForwardingShipment>();
			shipment.DocumentSupporter.Initialise(docEvents);
			declaration.JE_JS = shipment.PK;
			using (var form = new ZForm(shipment))
			{
				form.Controls.Add(new ZTabControl());
				form.PlugIns.Add(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment);
				form.Show();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
				docEvents.RaiseDocumentPrintRequested(shipment, eventArgs);
				AssertEquals("Print should be cancelled", true, eventArgs.Cancel);
				AssertEquals("LastFormShownForTest", typeof(MAFCoverSheetForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertNull("NZDocsMAFCoverSheet should NOT be created in Factory", Factory.GetValue<NZDocsMAFCoverSheet>());
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				docEvents.RaiseDocumentPrintRequested(shipment, eventArgs);
				AssertEquals("Print should NOT be cancelled", false, eventArgs.Cancel);
				AssertEquals("LastFormShownForTest", typeof(MAFCoverSheetForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertNotNull("NZDocsMAFCoverSheet should be created in Factory", Factory.GetValue<NZDocsMAFCoverSheet>());
			}
		}
	}
}
