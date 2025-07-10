using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Freight.CFS.GUI.Testing
{
	sealed class GatePassShipmentFormInternalTest : BaseFreightTest
	{
		const string ClearReference = "CLEAR 9914N";
		const string ConditionalClearReference = "CONDCLEAR 9914N";
		const string AcsSeizedReference = "ACSSEIZED 9914N";
		const string AqisSeizedReference = "AQISSEIZED 9914N";
		const string ClearHrmReference = "CLEARHRM 9914N";
		const string HeldReference = "HELD 9914N";
		const string SubUbMovReference = "SUBUBMOV 9914N";
		const string TranshipReference = "TRANSHIP 9914N";
		const string TranshpHrmReference = "TRANSHPHRM 9914N";
		const string TransitReference = "TRANSIT 9914N";

		public void TestSpecialGatePassSaveAndPrint()
		{
			AssertUserMessage(ClearReference, ZString.Empty, DialogResult.OK);
			AssertUserMessage(ConditionalClearReference, "Confirm conditional clearance actions have been completed", DialogResult.OK);
			AssertUserMessage(AcsSeizedReference, "This shipment has been seized by customs and may not be gate passed", DialogResult.OK);
			AssertUserMessage(AqisSeizedReference, "This shipment has been seized by Quarantine and may not be gate passed", DialogResult.OK);
			AssertUserMessage(ClearHrmReference, "This shipment is marked as high risk", DialogResult.OK);
			AssertUserMessage(HeldReference, "This shipment has not been cleared by customs.  Continue with Contingency Release?", DialogResult.Yes);
			AssertUserMessage(SubUbMovReference, "This shipment is clear to be moved underbond but may not be delivered for home consumption", DialogResult.OK);
			AssertUserMessage(TranshipReference, ZString.Empty, DialogResult.OK);
			AssertUserMessage(TranshpHrmReference, "This shipment is marked as high risk", DialogResult.OK);
			AssertUserMessage(TransitReference, ZString.Empty, DialogResult.OK);
		}

		public void TestShipmentGetsPassedToDetails()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();

			using (ShipmentGatePassForm form = new ShipmentGatePassForm(shipment))
			{
				form.Show();
				AssertEquals(shipment, form.GatePassDetailsUserControl.GatePassShipment);
			}
		}

		void AssertUserMessage(ZString reference, ZString expectedMessage, DialogResult answer)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			GatePassShipment shipment = Factory.New<GatePassShipment>();
			shipment.Logs.AddNew(Events.SeaCargoDepotEvent, reference);

			using (ShipmentGatePassFormForTest form = new ShipmentGatePassFormForTest(shipment))
			{
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(answer);
				form.SpecialGatePassSaveAndPrint();
				if (expectedMessage.IsEmpty)
				{
					AssertEquals("User Message should not have been displayed", null, UnitTestUserNotification.Instance.LastMessage.Text);
				}
				else
				{
					AssertContains("Last Message should have contained string " + expectedMessage, expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestTryToSetupForPrintAndRunPrintJobCancelsProperly()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			SetupLocalBranchAsDepot();
			SetupLocalBranchAsLocalCartage();

			GatePassShipment shipment = (GatePassShipment)GetImportShipment(typeof(GatePassShipment));

			shipment.ConsigneeDeliveryAddress.E2_OA_Address = shipment.Consignee.MainAddress.PK;
			CFSPackLine line = shipment.OuterPackLines.AddNew();
			line.JL_PackageCount = 20;
			line.JL_ActualVolume = 20m;
			line.JL_ActualWeight = 20000m;

			CFSLoadListConsol loadList = shipment.Consols.AddNew();
			loadList.JK_OA_UnpackDepotAddress = LocalDepot.MainAddress.PK;
			loadList.Containers.AddNew();
			line.Containers.Add(loadList.Containers[0]);

			loadList.Containers[0].JC_LCLUnpack = ZDateTime.Today;
			line.JL_Outturn = 20;

			Factory.Save();

			BusinessObjectFactory gatePassFactory = new BusinessObjectFactory();
			GatePassShipment gatePass = gatePassFactory.Load<GatePassShipment>(shipment.PK);
			CommonPickupDeliveryConfirm containerLeg1 = gatePass.DestinationCFSDepartures.AddNew();
			containerLeg1.EU_VehicleRegistration = "aaa";
			containerLeg1.EU_PickupDeliveryTime = ZDateTime.Now;
			CommonPickupDeliveryConfirm containerLeg2 = gatePass.DestinationCFSDepartures.AddNew();
			containerLeg2.EU_VehicleRegistration = "bbb";
			containerLeg2.EU_PickupDeliveryTime = ZDateTime.Now.AddDays(1);

			AssertEquals("Expecting 2 new deliveries on the shipment.", 2, gatePass.NumberOfNewDeliveries());

			using (ShipmentGatePassFormForTest form = new ShipmentGatePassFormForTest(gatePass))
			{
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				form.TryToSetupForPrintAndRunPrintJob();
				AssertEquals("User Message should not have been displayed", "Document not saved - please print or uncheck the Print On Save check box.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDocumentNames()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();

			Func<string, DocumentZQuery> createFilterForDocument = (documentName) =>
			{
				DocumentZQuery query = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, documentName);
				query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, shipment.DocumentSupporter.BusinessContext);
				query.AddToFilter(StmMenuItemSchema.SU_MenuPath, "");
				return query;
			};

			AssertNotNull(string.Format("{0} is MIA", ShipmentGatePassForm.DocumentNames.GatePass),
				Factory.LoadTop1<DocumentCommand>(createFilterForDocument(ShipmentGatePassForm.DocumentNames.GatePass)));

			AssertNotNull(string.Format("{0} is MIA", ShipmentGatePassForm.DocumentNames.GatePassSingapore),
				Factory.LoadTop1<DocumentCommand>(createFilterForDocument(ShipmentGatePassForm.DocumentNames.GatePassSingapore)));
		}

		public void TestPrintDocuments()
		{
			GatePassShipment gatePass = CreateNewGatePassShipment();

			CommonPickupDeliveryConfirm confirm = gatePass.DestinationCFSDepartures.AddNew();
			confirm.EU_VehicleRegistration = "ABC123";
			confirm.EU_PickupDeliveryTime = ZDateTime.Now;

			var mockRepository = new MockRepository(MockBehavior.Default);
			var mockDocumentPrintSet = mockRepository.Create<DocumentPrintSet>(Factory.New<DocumentCommand>(), new UserControlProviderList());
			mockDocumentPrintSet.Setup(m => m.RunWithPartialInstructions(AllowedDeliveryOptions.All, null, Env.Security.None)).Returns(DeliveryInstructionDestination.Print);

			using (ShipmentGatePassFormForTest form = new ShipmentGatePassFormForTest(gatePass))
			{
				form.Show();

				AssertNull(form.Task);

				form.TryToSetupForPrintAndRunPrintJob();

				AssertNotNull(form.Task);
				AssertEquals(1, form.Task.Count);
				AssertEquals(1, form.Task[0].Count);
				AssertEquals("System Document Elements", ((Report)form.Task[0][0]).Template.TemplateName);

				form.Task = mockDocumentPrintSet.Object;

				Factory.Save();
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			using (ShipmentGatePassFormForTest form = new ShipmentGatePassFormForTest(gatePass))
			{
				form.Show();

				AssertNull(form.Task);

				form.TryToSetupForPrintAndRunPrintJob();

				AssertNotNull(form.Task);
				AssertEquals(1, form.Task.Count);
				AssertEquals(3, form.Task[0].Count);
				Assert(form.Task[0].Cast<Report>().All((report) => report.Template.TemplateName == "Gate Pass Shipment Singapore"));
			}
		}
		public void TestPrintDocuments_CreateAdditionalCopies()
		{
			GatePassShipment gatePass = CreateNewGatePassShipment();

			CommonPickupDeliveryConfirm confirm = gatePass.DestinationCFSDepartures.AddNew();
			confirm.EU_VehicleRegistration = "ABC123";
			confirm.EU_PickupDeliveryTime = ZDateTime.Now;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			using (CFSDataRegistry.Instance.GatepassCopiesToPrint.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, 2))
			using (ShipmentGatePassFormForTest form = new ShipmentGatePassFormForTest(gatePass))
			{
				form.Show();

				AssertNull("Precondition:", form.Task);

				form.TryToSetupForPrintAndRunPrintJob();

				AssertNotNull("Task should be created", form.Task);
				AssertEquals("One task should be created", 1, form.Task.Count);
				AssertEquals("One additional copies should be created", 2, form.Task[0].Count);
				Assert("All reports should have 2 DataProviders", form.Task[0].Cast<Report>().All((report) => report.DataProviderList.AllDataProviders.Length == 2));
			}
		}

		public void TestPrintDocuments_CreateAdditionalCopies_OnlyConsidersReports()
		{
			var gatePass = CreateNewGatePassShipment();

			var confirm = gatePass.DestinationCFSDepartures.AddNew();
			confirm.EU_VehicleRegistration = "ABC123";
			confirm.EU_PickupDeliveryTime = ZDateTime.Now;

			gatePass.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("Test eDoc"), "test.txt", "GTP");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			using (CFSDataRegistry.Instance.GatepassCopiesToPrint.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, 2))
			using (var form = new ShipmentGatePassFormForTest(gatePass))
			{
				form.Show();

				AssertEquals("Pre-Condition - eDoc exists on GatePass", 1, gatePass.DocManagerInfo.EDocsView.Count);
				AssertNoExceptionThrown(
					"Should not throw exception if eDoc (IStorageFile) in Document Pack",
					() => form.TryToSetupForPrintAndRunPrintJob()
				);
			}
		}

		public void TestDocumentContext()
		{
			GatePassShipment gatePass = CreateNewGatePassShipment();

			CommonPickupDeliveryConfirm confirm = gatePass.DestinationCFSDepartures.AddNew();
			confirm.EU_VehicleRegistration = "ABC123";
			confirm.EU_PickupDeliveryTime = ZDateTime.Now;

			using (ShipmentGatePassFormForTest form = new ShipmentGatePassFormForTest(gatePass))
			{
				form.Show();

				AssertExceptionThrown(typeof(InvalidOperationException),
					() => { ZString code = ((IDocWrapperContext)Factory.GetDocWrapperContextManager()).DocumentContactTypeCode; });

				form.TryToSetupForPrintAndRunPrintJob();

				AssertNoExceptionThrown(() => { ZString code = ((IDocWrapperContext)Factory.GetDocWrapperContextManager()).DocumentContactTypeCode; });
			}
		}

		public void TestRNSCFSShipmentPlugIn()
		{
			GatePassShipment gatePass = CreateNewGatePassShipment();

			using (ShipmentGatePassForm form = new ShipmentGatePassForm(gatePass))
			{
				form.Show();

				AssertNull("RNSCFSShipmentPlugIn should not be plugged in", form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.RNSCFSShipmentPlugIn));
				AssertNull("RNS menus should not be plugged in", form.Menu.MenuItems.FindByName("RNS"));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				gatePass.JS_RL_NKOrigin = HomePort;
				gatePass.JS_RL_NKDestination = "";

				using (ShipmentGatePassForm shipmentForm = new ShipmentGatePassForm(gatePass))
				{
					shipmentForm.Show();

					var plugIn = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.RNSCFSShipmentPlugIn);
					AssertNotNull("RNSCFSShipmentPlugIn should be plugged in", plugIn);
					Assert("RNSCFSShipmentPlugIn should not be enabled", !plugIn.Enabled);

					var menu = shipmentForm.Menu.MenuItems.FindByText("RNS");
					AssertNull("RNS menus should not be plugged in", menu);
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				gatePass.JS_RL_NKOrigin = "";
				gatePass.JS_RL_NKDestination = HomePort;

				using (ShipmentGatePassForm form = new ShipmentGatePassForm(gatePass))
				{
					form.Show();

					var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.RNSCFSShipmentPlugIn);
					AssertNotNull("RNSCFSShipmentPlugIn should be plugged in", plugIn);
					Assert("RNSCFSShipmentPlugIn should be enabled", plugIn.Enabled);

					var menu = form.Menu.MenuItems.FindByText("RNS");
					AssertNull("RNS menus should not be plugged in", menu);
				}
			}
		}

		GatePassShipment CreateNewGatePassShipment()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			SetupLocalBranchAsDepot();
			SetupLocalBranchAsLocalCartage();

			GatePassShipment shipment = (GatePassShipment)GetImportShipment(typeof(GatePassShipment));

			shipment.ConsigneeDeliveryAddress.E2_OA_Address = shipment.Consignee.MainAddress.PK;
			CFSPackLine line = shipment.OuterPackLines.AddNew();
			line.JL_PackageCount = 20;
			line.JL_ActualVolume = 20m;
			line.JL_ActualWeight = 20000m;

			CFSLoadListConsol loadList = shipment.Consols.AddNew();
			loadList.JK_OA_UnpackDepotAddress = LocalDepot.MainAddress.PK;
			loadList.Containers.AddNew();
			line.Containers.Add(loadList.Containers[0]);

			loadList.Containers[0].JC_LCLUnpack = ZDateTime.Today;
			line.JL_Outturn = 20;

			Factory.Save();

			return shipment;
		}

		public void TestAllowNew()
		{
			var gatePass = CreateNewGatePassShipment();
			using (var form = new ShipmentGatePassForm(gatePass))
			{
				IPostingButtonsProvider postingProvider = form;
				AssertEquals(false, postingProvider.AllowNew);
			}
		}

		class ShipmentGatePassFormForTest : ShipmentGatePassForm
		{
			public ShipmentGatePassFormForTest(GatePassShipment shipment)
				: base(shipment)
			{
			}

			public new void SpecialGatePassSaveAndPrint()
			{
				base.SpecialGatePassSaveAndPrint();
			}

			public new void TryToSetupForPrintAndRunPrintJob()
			{
				base.TryToSetupForPrintAndRunPrintJob();
			}

			public new DocumentPrintSet Task
			{
				get { return base.Task; }
				set { base.Task = value; }
			}
		}
	}
}
