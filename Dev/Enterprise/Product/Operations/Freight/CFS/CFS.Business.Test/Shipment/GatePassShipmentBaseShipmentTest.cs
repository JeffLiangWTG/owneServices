using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business.Testing
{
	internal class GatePassShipmentBaseShipmentTest : CFSShipmentTest
	{
		#region GetNewValidation

		public override void TestGetNewValidation()
		{
			GatePassShipment shipment = Factory.NewWithValidTestData<GatePassShipment>();
			AssertEquals("Type of Validation", typeof(GatePassShipmentValidation), shipment.Validation.GetType());
		}

		#endregion

		protected override CommonShipment GetShipment()
		{
			return Factory.New<GatePassShipment>();
		}

		void AssertGatePassID(string comment, CommonPickupDeliveryConfirm leg, ZString code)
		{
			AssertEquals(comment, leg.FirstShipment.JS_UniqueConsignRef + "/" + code.ToString(), leg.FullGatePass);
		}

		[ExpectNoExceptions]
		public void TestOnAddedSetPrint()
		{
			GatePassShipment shipment = GatePassShipment.New(Factory);
			shipment.OuterPackLines.AddNew();
			CommonPickupDeliveryConfirm containerLeg1 = shipment.DestinationCFSDepartures.AddNew();
			CommonPickupDeliveryConfirm containerLeg2 = shipment.DestinationCFSDepartures.AddNew();
			CommonPickupDeliveryConfirm containerLeg3 = shipment.DestinationCFSDepartures.AddNew();
			containerLeg1.EU_DriversName = "1";
			containerLeg2.EU_DriversName = "2";
			containerLeg3.EU_DriversName = "3";

			containerLeg2.EU_GatePassCount = 1;

			var queryProvider = new Mock<ICommonShipmentDocumentSupporterQueryProvider>();
			Factory.SetValue(() => queryProvider.Object);

			queryProvider
				.Setup(m => m.GetConfirmsToPrint(It.Is<CommonShipment>(p => p == shipment),
					It.Is<DocumentPickupDeliveryConfirmOptions>(p =>
						p.Confirms.Cast<DocumentPickupDeliveryConfirm>().Count(confirm => confirm.PrintConfirm) == 2)))
				.Returns((DocumentPickupDeliveryConfirm[])null);

			var documentSupporter = (GatePassShipmentDocumentSupporter)shipment.DocumentSupporter;
			documentSupporter.GetDocumentWrappers(Constants.DataContext.GatePassContainerLeg, null);

			queryProvider.Verify(
				m => m.GetConfirmsToPrint(It.Is<CommonShipment>(p => p == shipment),
					It.Is<DocumentPickupDeliveryConfirmOptions>(p =>
						p.Confirms.Cast<DocumentPickupDeliveryConfirm>().Count(confirm => confirm.PrintConfirm) == 2)),
				Times.Once);
		}

		public void TestEventLogs()
		{
			Factory.Save();
			GatePassPackLine originalPacks1 = Shipment.OuterPackLines.AddNew();
			originalPacks1.JL_PackageCount = 20;
			originalPacks1.JL_Outturn = 19;
			originalPacks1.JL_F3_NKPackType = Constants.PkgUnit.Box;

			GatePassPackLine originalPacks2 = Shipment.OuterPackLines.AddNew();
			originalPacks2.JL_PackageCount = 12;
			originalPacks2.JL_Outturn = 13;
			originalPacks2.JL_F3_NKPackType = Constants.PkgUnit.Drum;

			Shipment.ResetForNextSaveAndPrint();
			CommonPickupDeliveryConfirm leg = Shipment.DestinationCFSDepartures.AddNew();
			Shipment.CleanUpAfterSaveAndPrint(true);

			Factory.Save();
			AssertEquals("Failed to get Gate Pass Number on Delivery", (ZByte)1, leg.EU_GatePassCount);

			((ILogsInternals)Shipment.Logs).ReloadFromDB();

			StmALog[] drumPrints = Shipment.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, leg.FullGatePass));
			AssertEquals("should have found delivery", 1, drumPrints.Length);
			AssertEquals("should have GPP type", Events.GatePassPrinted.Code, drumPrints[0].SL_SE_NKEvent);
			AssertEquals("should not be cancelled", false, drumPrints[0].SL_IsCancelled);

			ZString drumDeliveryID = leg.FullGatePass;
			CancelDeliveryOfPackages(leg);
			Factory.Save();
			drumPrints = Shipment.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, drumDeliveryID));
			AssertEquals("should have found delivery", 1, drumPrints.Length);
			AssertEquals("should have GPP type", Events.GatePassPrinted.Code, drumPrints[0].SL_SE_NKEvent);
			AssertEquals("should be cancelled", true, drumPrints[0].SL_IsCancelled);
			AssertEquals("should have been unticked by cancel", false, Shipment.JS_ToBeFullyDelivered);
		}

		public void TestValidateJS_MarksAndNumbers()
		{
			JobSailing importSailing = CreateNewSailing(true);

			Shipment.JS_RL_NKDestination = importSailing.Destination.JB_RL_NKPortOfDischarge;
			Shipment.JS_RL_NKOrigin = importSailing.Origin.JA_RL_NKPortOfLoading;
			Shipment.JS_GoodsDescription = "x";
			Shipment.JS_HouseBill = "x";
			Shipment.ConsigneePK = LocalConsignee.PK;
			Shipment.ConsignorPK = OverseasConsignor.PK;
			Shipment.JS_ActualVolume = 1;
			Shipment.JS_ActualWeight = 1;

			Shipment.JS_MarksAndNumbers = "";
			Shipment.RunPreSaveValidation();
			AssertEquals("should not be any errors but was\r\n" + Shipment.NotificationsIncludingChildren.GetErrors().ToUniqueMessageListString(), false, Shipment.HasErrors);
		}

		public void TestValidateDeliveryVersusShipmentTotals_Import()
		{
			GatePassLoadListConsol consol = (GatePassLoadListConsol)GetImportConsol(typeof(GatePassLoadListConsol));
			GatePassContainer container = consol.Containers.AddNew();
			consol.Shipments.Add(Shipment);

			GatePassPackLine pack = Shipment.OuterPackLines.AddNew();
			pack.JL_Outturn = pack.JL_PackageCount = 10;
			pack.JL_F3_NKPackType = "ENV";
			Shipment.JS_OuterPacks = 10;
			Shipment.JS_GoodsDescription = "x";
			Shipment.JS_RL_NKOrigin = consol.Schedule.JX_JA_RL_NKPortOfLoading;
			Shipment.JS_RL_NKDestination = consol.Schedule.JX_JB_RL_NKPortOfDischarge;
			Shipment.JS_MarksAndNumbers = "x";
			Shipment.JS_HouseBill = "x";
			Shipment.ConsigneePK = LocalConsignee.PK;

			pack.SetContainer(container.PK);
			Shipment.ResetForNextSaveAndPrint();
			Shipment.RunPreSaveValidation();
			AssertNoErrors(Shipment);
			AssertEquals("should have created 1 aggregate", 1, Shipment.OuterPackLines.Count);
			AssertEquals("should not be any deliveries yet", 0, Shipment.DestinationCFSDepartures.Count);

			CommonPickupDeliveryConfirm newDelivery = Shipment.DestinationCFSDepartures.AddNew();
			newDelivery.EU_DriversName = "x";
			newDelivery.EU_TransportCoName = "x";
			newDelivery.EU_VehicleRegistration = "x";
			newDelivery.EU_DriversLicence = "x";
			newDelivery.EU_PickupDeliveryTime = ZDateTime.Now;

			Shipment.RunPreSaveValidation();
			AssertNoErrors(Shipment);

			pack.JL_Outturn = pack.JL_PackageCount = 0;
			Shipment.RunPreSaveValidation();
			AssertEquals("should have warning on the delivery package count", true, Shipment.HasWarnings);
		}

		public void TestValidateDeliveryVersusShipmentTotals_Export()
		{
			GatePassLoadListConsol consol = (GatePassLoadListConsol)GetExportConsol(typeof(GatePassLoadListConsol));
			GatePassContainer container = consol.Containers.AddNew();

			consol.Shipments.Add(Shipment);
			Shipment.JS_MarksAndNumbers = "x";
			Shipment.JS_GoodsDescription = "x";
			Shipment.JS_HouseBill = "x";
			Shipment.ConsignorPK = LocalConsignor.PK;
			Shipment.JS_RL_NKOrigin = consol.Schedule.JX_JA_RL_NKPortOfLoading;
			Shipment.JS_RL_NKDestination = consol.Schedule.JX_JB_RL_NKPortOfDischarge;
			GatePassPackLine pack = Shipment.OuterPackLines.AddNew();
			pack.JL_Outturn = pack.JL_PackageCount = 10;

			Shipment.RunPreSaveValidation();
			AssertNoErrors(Shipment);

			CommonPickupDeliveryConfirm newDelivery = Shipment.DestinationCFSDepartures.AddNew();
			newDelivery.EU_DriversName = "x";
			newDelivery.EU_TransportCoName = "x";
			newDelivery.EU_VehicleRegistration = "x";
			newDelivery.EU_DriversLicence = "x";
			newDelivery.EU_PickupDeliveryTime = ZDateTime.Now; //GUI
			newDelivery.GetDivot(pack).J8_PackagesDelivered++;
			Shipment.RunPreSaveValidation();
			AssertEquals("should have warning- trying to deliver 11 packs when JS_OuterPacks == 10", true, Shipment.HasWarnings);

			newDelivery.GetDivot(pack).J8_PackagesDelivered--;
			Shipment.RunPreSaveValidation();
			AssertEquals("should be no errors, but shipment has these ones:"
				+ System.Environment.NewLine + Shipment.NotificationsIncludingChildren.GetErrors().ToUniqueMessageListString(), false, Shipment.HasErrors);

			AssertEquals("just a check that non persistent BizOs are not being thrown away", newDelivery, Shipment.DestinationCFSDepartures[0]);

			Shipment.ResetForNextSaveAndPrint();
			AssertEquals("error status should not change after recalculating non persistent objects", false, Shipment.HasErrors);
			CommonPickupDeliveryConfirm anotherNewDelivery = Shipment.DestinationCFSDepartures.AddNew();
			AssertEquals("should be able to add one more without error", false, Shipment.HasErrors);
		}

		public void TestGetDataStateBeforeRun()
		{
			AssertEquals("precondition", false, Shipment.DocumentSupporter.GetDataStateBeforeRun(null).IsValid);
			PackLine newPack = Shipment.OuterPackLines.AddNew();
			newPack.JL_PackageCount = 1;
			newPack.JL_Outturn = 1;
			newPack.JL_JS = Shipment.PK;
			CommonPickupDeliveryConfirm leg = Shipment.DestinationCFSDepartures.AddNew();
			Shipment.CleanUpAfterSaveAndPrint(true);
			Factory.Save();
			Shipment.ResetForNextSaveAndPrint();
			AssertEquals("1 pack delivered", true, Shipment.DocumentSupporter.GetDataStateBeforeRun(null).IsValid);
		}

		public void TestDeliveryEventsForContainerisedImportPackline()
		{
			Factory.Save();
			Shipment.FullyDelivered += new EventHandler(TestFullyDeliveredEventHandler);
			Shipment.FullDeliveryCancelled += new EventHandler(TestFullDeliveryCancelledEventHandler);
			FullyDeliveredEventCalled = false;
			FullDeliveryCancelledEventCalled = false;

			GatePassLoadListConsol consol = Factory.New<GatePassLoadListConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Transports[0].JW_JX = ImportSailing1.PK;

			Shipment.JS_RL_NKOrigin = "CNSHA";
			Shipment.JS_RL_NKDestination = "AUSYD";

			consol.Shipments.Add(Shipment);
			GatePassContainer container = consol.Containers.AddNew();
			GatePassPackLine originalPacks1 = Shipment.OuterPackLines.AddNew();
			container.AddPackLine(originalPacks1);

			originalPacks1.JL_PackageCount = 20;
			originalPacks1.JL_Outturn = 19;
			originalPacks1.JL_F3_NKPackType = Constants.PkgUnit.Box;

			GatePassPackLine originalPacks2 = Shipment.OuterPackLines.AddNew();
			container.AddPackLine(originalPacks2);
			originalPacks2.JL_PackageCount = 12;
			originalPacks2.JL_Outturn = 13;
			originalPacks2.JL_F3_NKPackType = Constants.PkgUnit.Drum;

			Shipment.ResetForNextSaveAndPrint();
			CommonPickupDeliveryConfirm drumDelivery = Shipment.DestinationCFSDepartures.AddNew();
			drumDelivery.Divots[0].J8_PackagesDelivered = 10;
			Shipment.CleanUpAfterSaveAndPrint(true);
			Factory.Save();
			AssertGatePassID("DrumDelivery", drumDelivery, "A");

			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);

			CommonPickupDeliveryConfirm firstDelivery = Shipment.DestinationCFSDepartures.AddNew();
			firstDelivery.Divots[0].J8_PackagesDelivered = 5;
			Shipment.CleanUpAfterSaveAndPrint(true);
			Factory.Save();
			AssertGatePassID("FirstDelivery", firstDelivery, "B");
			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);

			CancelDeliveryOfPackages(firstDelivery);
			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);

			firstDelivery = Shipment.DestinationCFSDepartures.AddNew();
			firstDelivery.Divots[0].J8_PackagesDelivered = 6;
			Shipment.CleanUpAfterSaveAndPrint(true);
			Factory.Save();
			AssertGatePassID("Re-Issue FirstDelivery", firstDelivery, "B");
			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);

			CommonPickupDeliveryConfirm secondDelivery = Shipment.DestinationCFSDepartures.AddNew();
			Shipment.CleanUpAfterSaveAndPrint(true);
			Factory.Save();

			AssertGatePassID("SecondDelivery", secondDelivery, "C");
			AssertEquals("Fully Delivery Event Called", true, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);
			FullyDeliveredEventCalled = false;

			// check that the cancel doesn't affect the other deliveries
			AssertGatePassID("DrumDelivery Pre-Cancel", drumDelivery, "A");
			AssertGatePassID("FirstDelivery Pre-Cancel", firstDelivery, "B");
			AssertGatePassID("SecondDelivery Pre-Cancel", secondDelivery, "C");

			CancelDeliveryOfPackages(secondDelivery);

			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", true, FullDeliveryCancelledEventCalled);
			FullDeliveryCancelledEventCalled = false;

			// check that the cancel doesn't affect the other deliveries
			AssertGatePassID("DrumDelivery Post-Cancel", drumDelivery, "A");
			AssertGatePassID("FirstDelivery Post-Cancel", firstDelivery, "B");

			CancelDeliveryOfPackages(firstDelivery);

			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);

			CommonPickupDeliveryConfirm fullDelivery = Shipment.DestinationCFSDepartures.AddNew();
			Shipment.CleanUpAfterSaveAndPrint(true);
			Factory.Save();
			AssertEquals("Fully Delivery Event Called", true, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);
			FullyDeliveredEventCalled = false;

			CancelDeliveryOfPackages(drumDelivery);
			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", true, FullDeliveryCancelledEventCalled);
		}

		public void TestDeliveryEventsForLooseImportPackLine()
		{
			Factory.Save();
			Shipment.FullyDelivered += new EventHandler(TestFullyDeliveredEventHandler);
			Shipment.FullDeliveryCancelled += new EventHandler(TestFullDeliveryCancelledEventHandler);
			Shipment.JS_RL_NKOrigin = OverseasPort;
			Shipment.JS_RL_NKDestination = HomePort;
			FullyDeliveredEventCalled = false;
			FullDeliveryCancelledEventCalled = false;

			GatePassPackLine originalPacks1 = Shipment.OuterPackLines.AddNew();
			originalPacks1.JL_PackageCount = 20;
			originalPacks1.JL_Outturn = 19;
			originalPacks1.JL_F3_NKPackType = Constants.PkgUnit.Box;

			GatePassPackLine originalPacks2 = Shipment.OuterPackLines.AddNew();
			originalPacks2.JL_PackageCount = 12;
			originalPacks2.JL_Outturn = 13;
			originalPacks2.JL_F3_NKPackType = Constants.PkgUnit.Drum;

			Shipment.ResetForNextSaveAndPrint();
			CommonPickupDeliveryConfirm drumDelivery = Shipment.DestinationCFSDepartures.AddNew();
			drumDelivery.Divots[0].J8_PackagesDelivered = 10;
			Shipment.CleanUpAfterSaveAndPrint(true);
			Factory.Save();
			AssertGatePassID("DrumDelivery", drumDelivery, "A");

			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);

			CommonPickupDeliveryConfirm firstDelivery = Shipment.DestinationCFSDepartures.AddNew();
			firstDelivery.Divots[0].J8_PackagesDelivered = 5;
			Shipment.CleanUpAfterSaveAndPrint(true);
			Factory.Save();
			AssertGatePassID("FirstDelivery", firstDelivery, "B");
			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);

			CancelDeliveryOfPackages(firstDelivery);
			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);

			firstDelivery = Shipment.DestinationCFSDepartures.AddNew();
			firstDelivery.Divots[0].J8_PackagesDelivered = 6;
			Shipment.CleanUpAfterSaveAndPrint(true);
			Factory.Save();
			AssertGatePassID("Re-Issue FirstDelivery", firstDelivery, "B");
			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);

			CommonPickupDeliveryConfirm secondDelivery = Shipment.DestinationCFSDepartures.AddNew();
			Shipment.CleanUpAfterSaveAndPrint(true);
			Factory.Save();

			AssertGatePassID("SecondDelivery", secondDelivery, "C");
			AssertEquals("Fully Delivery Event Called", true, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);
			FullyDeliveredEventCalled = false;

			// check that the cancel doesn't affect the other deliveries
			AssertGatePassID("DrumDelivery Pre-Cancel", drumDelivery, "A");
			AssertGatePassID("FirstDelivery Pre-Cancel", firstDelivery, "B");
			AssertGatePassID("SecondDelivery Pre-Cancel", secondDelivery, "C");

			CancelDeliveryOfPackages(secondDelivery);

			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", true, FullDeliveryCancelledEventCalled);
			FullDeliveryCancelledEventCalled = false;

			// check that the cancel doesn't affect the other deliveries
			AssertGatePassID("DrumDelivery Post-Cancel", drumDelivery, "A");
			AssertGatePassID("FirstDelivery Post-Cancel", firstDelivery, "B");

			CancelDeliveryOfPackages(firstDelivery);

			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);

			CommonPickupDeliveryConfirm fullDelivery = Shipment.DestinationCFSDepartures.AddNew();
			Shipment.CleanUpAfterSaveAndPrint(true);
			Factory.Save();
			AssertEquals("Fully Delivery Event Called", true, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);
			FullyDeliveredEventCalled = false;

			CancelDeliveryOfPackages(drumDelivery);
			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", true, FullDeliveryCancelledEventCalled);
		}

		public void TestDeliveryEventsForExportWithOutturn()
		{
			Factory.Save();
			Shipment.FullyDelivered += new EventHandler(TestFullyDeliveredEventHandler);
			Shipment.FullDeliveryCancelled += new EventHandler(TestFullDeliveryCancelledEventHandler);
			Shipment.JS_RL_NKOrigin = HomePort;
			Shipment.JS_RL_NKDestination = OverseasPort;
			Shipment.JS_TranshipToOtherCFS = true;
			FullyDeliveredEventCalled = false;
			FullDeliveryCancelledEventCalled = false;

			GatePassPackLine originalPacks1 = Shipment.OuterPackLines.AddNew();
			originalPacks1.JL_PackageCount = 20;
			originalPacks1.JL_Outturn = 19;
			originalPacks1.JL_F3_NKPackType = Constants.PkgUnit.Box;

			GatePassPackLine originalPacks2 = Shipment.OuterPackLines.AddNew();
			originalPacks2.JL_PackageCount = 12;
			originalPacks2.JL_Outturn = 13;
			originalPacks2.JL_F3_NKPackType = Constants.PkgUnit.Drum;

			Shipment.ResetForNextSaveAndPrint();
			CommonPickupDeliveryConfirm drumDelivery = Shipment.DestinationCFSDepartures.AddNew();
			drumDelivery.Divots[0].J8_PackagesDelivered = 10;
			Shipment.CleanUpAfterSaveAndPrint(true);
			Factory.Save();
			AssertGatePassID("DrumDelivery", drumDelivery, "A");

			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);

			CommonPickupDeliveryConfirm firstDelivery = Shipment.DestinationCFSDepartures.AddNew();
			firstDelivery.Divots[0].J8_PackagesDelivered = 5;
			Shipment.CleanUpAfterSaveAndPrint(true);
			Factory.Save();
			AssertGatePassID("FirstDelivery", firstDelivery, "B");
			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);

			CancelDeliveryOfPackages(firstDelivery);
			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);

			firstDelivery = Shipment.DestinationCFSDepartures.AddNew();
			firstDelivery.Divots[0].J8_PackagesDelivered = 6;
			Shipment.CleanUpAfterSaveAndPrint(true);
			Factory.Save();
			AssertGatePassID("Re-Issue FirstDelivery", firstDelivery, "B");
			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);

			CommonPickupDeliveryConfirm secondDelivery = Shipment.DestinationCFSDepartures.AddNew();
			Shipment.CleanUpAfterSaveAndPrint(true);
			Factory.Save();

			AssertGatePassID("SecondDelivery", secondDelivery, "C");
			AssertEquals("Fully Delivery Event Called", true, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);
			FullyDeliveredEventCalled = false;

			// check that the cancel doesn't affect the other deliveries
			AssertGatePassID("DrumDelivery Pre-Cancel", drumDelivery, "A");
			AssertGatePassID("FirstDelivery Pre-Cancel", firstDelivery, "B");
			AssertGatePassID("SecondDelivery Pre-Cancel", secondDelivery, "C");

			CancelDeliveryOfPackages(secondDelivery);

			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", true, FullDeliveryCancelledEventCalled);
			FullDeliveryCancelledEventCalled = false;

			// check that the cancel doesn't affect the other deliveries
			AssertGatePassID("DrumDelivery Post-Cancel", drumDelivery, "A");
			AssertGatePassID("FirstDelivery Post-Cancel", firstDelivery, "B");

			CancelDeliveryOfPackages(firstDelivery);

			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);

			CommonPickupDeliveryConfirm fullDelivery = Shipment.DestinationCFSDepartures.AddNew();
			Shipment.CleanUpAfterSaveAndPrint(true);
			Factory.Save();
			AssertEquals("Fully Delivery Event Called", true, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);
			FullyDeliveredEventCalled = false;

			CancelDeliveryOfPackages(drumDelivery);
			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", true, FullDeliveryCancelledEventCalled);
		}

		public void TestDeliveryEventsForExportWithoutOutturn()
		{
			Factory.Save();
			Shipment.FullyDelivered += new EventHandler(TestFullyDeliveredEventHandler);
			Shipment.FullDeliveryCancelled += new EventHandler(TestFullDeliveryCancelledEventHandler);
			Shipment.JS_RL_NKOrigin = HomePort;
			Shipment.JS_RL_NKDestination = OverseasPort;
			Shipment.JS_TranshipToOtherCFS = true;
			FullyDeliveredEventCalled = false;
			FullDeliveryCancelledEventCalled = false;

			GatePassPackLine originalPacks1 = Shipment.OuterPackLines.AddNew();
			originalPacks1.JL_PackageCount = 20;
			originalPacks1.JL_F3_NKPackType = Constants.PkgUnit.Box;

			GatePassPackLine originalPacks2 = Shipment.OuterPackLines.AddNew();
			originalPacks2.JL_PackageCount = 12;
			originalPacks2.JL_F3_NKPackType = Constants.PkgUnit.Drum;

			Shipment.ResetForNextSaveAndPrint();
			CommonPickupDeliveryConfirm drumDelivery = Shipment.DestinationCFSDepartures.AddNew();
			drumDelivery.Divots[0].J8_PackagesDelivered = 10;
			Shipment.CleanUpAfterSaveAndPrint(true);
			Factory.Save();
			AssertGatePassID("DrumDelivery", drumDelivery, "A");

			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);

			CommonPickupDeliveryConfirm firstDelivery = Shipment.DestinationCFSDepartures.AddNew();
			firstDelivery.Divots[0].J8_PackagesDelivered = 5;
			Shipment.CleanUpAfterSaveAndPrint(true);
			Factory.Save();
			AssertGatePassID("FirstDelivery", firstDelivery, "B");
			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);

			CancelDeliveryOfPackages(firstDelivery);
			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);

			firstDelivery = Shipment.DestinationCFSDepartures.AddNew();
			firstDelivery.Divots[0].J8_PackagesDelivered = 6;
			Shipment.CleanUpAfterSaveAndPrint(true);
			Factory.Save();
			AssertGatePassID("Re-Issue FirstDelivery", firstDelivery, "B");
			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);

			CommonPickupDeliveryConfirm secondDelivery = Shipment.DestinationCFSDepartures.AddNew();
			Shipment.CleanUpAfterSaveAndPrint(true);
			Factory.Save();

			AssertGatePassID("SecondDelivery", secondDelivery, "C");
			AssertEquals("Fully Delivery Event Called", true, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);
			FullyDeliveredEventCalled = false;

			// check that the cancel doesn't affect the other deliveries
			AssertGatePassID("DrumDelivery Pre-Cancel", drumDelivery, "A");
			AssertGatePassID("FirstDelivery Pre-Cancel", firstDelivery, "B");
			AssertGatePassID("SecondDelivery Pre-Cancel", secondDelivery, "C");

			CancelDeliveryOfPackages(secondDelivery);

			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", true, FullDeliveryCancelledEventCalled);
			FullDeliveryCancelledEventCalled = false;

			// check that the cancel doesn't affect the other deliveries
			AssertGatePassID("DrumDelivery Post-Cancel", drumDelivery, "A");
			AssertGatePassID("FirstDelivery Post-Cancel", firstDelivery, "B");

			CancelDeliveryOfPackages(firstDelivery);

			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);

			CommonPickupDeliveryConfirm fullDelivery = Shipment.DestinationCFSDepartures.AddNew();
			Shipment.CleanUpAfterSaveAndPrint(true);
			Factory.Save();
			AssertEquals("Fully Delivery Event Called", true, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", false, FullDeliveryCancelledEventCalled);
			FullyDeliveredEventCalled = false;

			CancelDeliveryOfPackages(drumDelivery);
			AssertEquals("Fully Delivery Event Called", false, FullyDeliveredEventCalled);
			AssertEquals("Fully Delivery Cancelled Event Called", true, FullDeliveryCancelledEventCalled);
		}

		public void TestJS_Calc_TotalInStockForExportShipmentWithOutturn()
		{
			GatePassPackLine packLine1 = Shipment.OuterPackLines.AddNew();
			Shipment.JS_RL_NKOrigin = HomePort;
			Shipment.JS_RL_NKDestination = OverseasPort;

			packLine1.JL_PackageCount = 8;
			packLine1.JL_Outturn = 7;

			GatePassPackLine packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 5;
			packLine2.JL_Outturn = 4;

			AssertEquals("Not delivered", 11, Shipment.JS_Calc_TotalInStock);

			EventWatcher watcher = new EventWatcher();
			Shipment.JS_Calc_TotalInStockInfo.ValueChanged += watcher.Handler;
			AssertEquals("Adding an event listener should not raise it", 0, watcher.Count);

			CommonPickupDeliveryConfirm leg = Shipment.DestinationCFSDepartures.AddNew();
			AssertEquals("Adding a delivery should trigger JS_Calc_TotalInStockInfo.ValueChanged Once", 2, watcher.Count);
			AssertEquals("Fully Delivered", 0, Shipment.JS_Calc_TotalInStock);

			watcher.Clear();
			leg.Divots[0].J8_PackagesDelivered = 3;
			AssertEquals("Changing J8_PackagesDelivered should trigger JS_Calc_TotalInStockInfo.ValueChanged Once", 1, watcher.Count);
			AssertEquals("Partly Delivered", 4, Shipment.JS_Calc_TotalInStock);

			watcher.Clear();
			leg.Delete();
			// Uncomment the follwing line when work item I00024653 has been completed
			//AssertEquals("Removing a delivery should trigger JS_Calc_TotalInStockInfo.ValueChanged Once", 1, Watcher.Count);
			AssertEquals("Not delivered anymore", 11, Shipment.JS_Calc_TotalInStock);

			//Should really be a CFSShipment as adding a ContainerLeg auto defaults to a GatePass type leg (eg LCLImport)
			CommonPickupDeliveryConfirm leg2 = Shipment.OriginCFSArrivals.AddNew();
			leg2.Divots[0].J8_PackagesDelivered = 6;

			Factory.Save();

			AssertEquals("JS_Calc_TotalInStock should be unaffected by Arrival Divots", 11, Shipment.JS_Calc_TotalInStock);
		}

		public void TestJS_Calc_TotalInStockForExportShipmentNoOutturn()
		{
			GatePassPackLine packLine1 = Shipment.OuterPackLines.AddNew();
			Shipment.JS_RL_NKOrigin = HomePort;
			Shipment.JS_RL_NKDestination = OverseasPort;

			packLine1.JL_PackageCount = 8;

			GatePassPackLine packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 5;

			AssertEquals("Not delivered", 13, Shipment.JS_Calc_TotalInStock);

			EventWatcher watcher = new EventWatcher();
			Shipment.JS_Calc_TotalInStockInfo.ValueChanged += watcher.Handler;
			AssertEquals("Adding an event listener should not raise it", 0, watcher.Count);

			CommonPickupDeliveryConfirm leg = Shipment.DestinationCFSDepartures.AddNew();
			AssertEquals("Adding a delivery should trigger JS_Calc_TotalInStockInfo.ValueChanged Once", 2, watcher.Count);
			AssertEquals("Fully Delivered", 0, Shipment.JS_Calc_TotalInStock);

			watcher.Clear();
			leg.Divots[0].J8_PackagesDelivered = 3;
			AssertEquals("Changing J8_PackagesDelivered should trigger JS_Calc_TotalInStockInfo.ValueChanged Once", 1, watcher.Count);
			AssertEquals("Partly Delivered", 5, Shipment.JS_Calc_TotalInStock);

			watcher.Clear();
			leg.Delete();
			// Uncomment the follwing line when work item I00024653 has been completed
			//AssertEquals("Removing a delivery should trigger JS_Calc_TotalInStockInfo.ValueChanged Once", 1, Watcher.Count);
			AssertEquals("Not delivered anymore", 13, Shipment.JS_Calc_TotalInStock);

			//Should really be a CFSShipment as adding a ContainerLeg auto defaults to a GatePass type leg (eg LCLImport)
			CommonPickupDeliveryConfirm leg2 = Shipment.OriginCFSArrivals.AddNew();
			leg2.Divots[0].J8_PackagesDelivered = 6;

			Factory.Save();

			AssertEquals("JS_Calc_TotalInStock should be unaffected by Arrival Divots", 13, Shipment.JS_Calc_TotalInStock);
		}

		public void TestJS_Calc_TotalInStockForLooseCargoImportShipment()
		{
			Shipment.JS_RL_NKOrigin = OverseasPort;
			Shipment.JS_RL_NKDestination = HomePort;

			GatePassPackLine packLine1 = Shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 8;
			packLine1.JL_Outturn = 0;

			GatePassPackLine packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 5;
			packLine2.JL_Outturn = 0;

			AssertEquals("Not yet unpacked", 0, Shipment.JS_Calc_TotalInStock);

			packLine1.JL_Outturn = 7;
			packLine2.JL_Outturn = 4;
			AssertEquals("Unpacked, but not delivered (total outturned)", 11, Shipment.JS_Calc_TotalInStock);

			EventWatcher watcher = new EventWatcher();
			Shipment.JS_Calc_TotalInStockInfo.ValueChanged += watcher.Handler;
			AssertEquals("Adding an event listener should not raise it", 0, watcher.Count);

			CommonPickupDeliveryConfirm leg = Shipment.DestinationCFSDepartures.AddNew();
			AssertEquals("Adding a delivery should trigger JS_Calc_TotalInStockInfo.ValueChanged Once", 2, watcher.Count);
			AssertEquals("Fully Delivered (total outturned - total delivered)", 0, Shipment.JS_Calc_TotalInStock);

			watcher.Clear();
			leg.Divots[0].J8_PackagesDelivered = 3;
			AssertEquals("Changing J8_PackagesDelivered should trigger JS_Calc_TotalInStockInfo.ValueChanged Once", 1, watcher.Count);
			AssertEquals("Partly Delivered (total outturned - total delivered)", 4, Shipment.JS_Calc_TotalInStock);

			watcher.Clear();
			leg.Delete();
			// Uncomment the follwing line when work item I00024653 has been completed
			//AssertEquals("Removing a delivery should trigger JS_Calc_TotalInStockInfo.ValueChanged Once", 1, Watcher.Count);
			AssertEquals("Not delivered anymore (total outturned)", 11, Shipment.JS_Calc_TotalInStock);

			CommonPickupDeliveryConfirm leg2 = Shipment.OriginCFSArrivals.AddNew();
			leg2.Divots[0].J8_PackagesDelivered = 6;

			Factory.Save();

			AssertEquals("JS_Calc_TotalInStock should be unaffected by Arrival Divots (total outturned)", 11, Shipment.JS_Calc_TotalInStock);
		}

		public void TestJ8_PackagesDeliveredFallsBackToManifestedPackageCountForExportPackLinesWithNoOutturnSetForContainerisedShipment()
		{
			Assert("GlbBranch.CurrentBranch.Country must not be null", GlbBranch.CurrentBranch.Country != null);
			ZString localCountryCode = GlbBranch.CurrentBranch.Country.Code;

			RefUNLOCO localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.Equal, localCountryCode));
			RefUNLOCO foreignPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, localCountryCode));

			GatePassShipment shipment = Factory.New<GatePassShipment>();
			shipment.JS_RL_NKDestination = foreignPort.RL_Code;
			shipment.JS_RL_NKOrigin = localPort.RL_Code;

			GatePassLoadListConsol consol = Factory.New<GatePassLoadListConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = ImportSailing1.JX_JA_RL_NKPortOfLoading;
			consol.JK_RL_NKDischargePort = ImportSailing1.JX_JB_RL_NKPortOfDischarge;
			consol.Transports[0].JW_JX = ImportSailing1.PK;
			consol.Shipments.Add(shipment);
			GatePassContainer container = consol.Containers.AddNew();
			GatePassPackLine packLine = shipment.OuterPackLines.AddNew();
			container.AddPackLine(packLine);
			packLine.JL_PackageCount = 20;

			packLine.JL_Outturn = 5;
			AssertEquals("Use outturn when available", 5, shipment.JS_Calc_TotalInStock);

			packLine.JL_Outturn = 0;
			AssertEquals("use JL_PackageCount when JL_Outturn is 0 on an export shipment", 20, shipment.JS_Calc_TotalInStock);

			shipment.JS_RL_NKOrigin = foreignPort.RL_Code;
			AssertEquals("only use outturn for Cross-Trade shipments", 0, shipment.JS_Calc_TotalInStock);

			//Shipment.JS_RL_NKDestination = LocalPort.RL_Code;
			//AssertEquals("only use outturn for Import shipments", 0, Shipment.JS_Calc_TotalInStock);

			//Shipment.JS_RL_NKOrigin = LocalPort.RL_Code;
			//AssertEquals("only use outturn for Domestic shipments", 0, Shipment.JS_Calc_TotalInStock);
		}

		public void TestJ8_PackagesDeliveredFallsBackToManifestedPackageCountForExportPackLinesWithNoOutturnSetForLooseShipment()
		{
			Assert("GlbBranch.CurrentBranch.Country must not be null", GlbBranch.CurrentBranch.Country != null);
			ZString localCountryCode = GlbBranch.CurrentBranch.Country.Code;

			RefUNLOCO localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.Equal, localCountryCode));
			RefUNLOCO forrenPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, localCountryCode));

			GatePassShipment shipment = Factory.New<GatePassShipment>();
			shipment.JS_RL_NKDestination = forrenPort.RL_Code;
			shipment.JS_RL_NKOrigin = localPort.RL_Code;

			GatePassPackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 20;

			packLine.JL_Outturn = 5;
			AssertEquals("Use outturn when available", 5, shipment.JS_Calc_TotalInStock);

			packLine.JL_Outturn = 0;
			AssertEquals("use JL_PackageCount when JL_Outturn is 0 on an export shipment", 20, shipment.JS_Calc_TotalInStock);

			shipment.JS_RL_NKOrigin = forrenPort.RL_Code;
			AssertEquals("only use outturn for Cross-Trade shipments", 0, shipment.JS_Calc_TotalInStock);

			shipment.JS_RL_NKDestination = localPort.RL_Code;
			AssertEquals("only use outturn for Import shipments", 0, shipment.JS_Calc_TotalInStock);
		}

		[ExpectNoExceptions]
		public void TestManyDuplicateDeliverysDontBreak()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();

			GatePassPackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 8;
			packLine1.JL_Outturn = 8;

			GatePassPackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 9;
			packLine2.JL_Outturn = 9;

			GatePassPackLine packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_PackageCount = 7;
			packLine3.JL_Outturn = 7;

			CommonPickupDeliveryConfirm leg1 = shipment.DestinationCFSDepartures.AddNew();
			leg1.Divots[1].J8_PackagesDelivered = 0;
			leg1.Divots[2].J8_PackagesDelivered = 0;

			CommonPickupDeliveryConfirm leg2 = shipment.DestinationCFSDepartures.AddNew();
			leg2.Divots[2].J8_PackagesDelivered = 0;

			CommonPickupDeliveryConfirm leg3 = shipment.DestinationCFSDepartures.AddNew();

			shipment.RunPreSaveValidation(); // KABOOM!!
		}

		public void TestReadOnlynessDoesntPropagateToANewDivot()
		{
			var shipment = Factory.New<GatePassShipment>();
			shipment.JS_TranshipToOtherCFS = true;
			var packLine = shipment.OuterPackLines.AddNew();
			var leg = shipment.DestinationCFSDepartures.AddNew();
			leg.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 30);
			var divot = leg.Divots[0];

			AssertEquals("new leg should not be readonly", false, leg.ReadOnly);
			AssertEquals("new Divot should not be readonly", false, divot.ReadOnly);

			Factory.Save();
			AssertEquals("saved leg should be readonly", true, leg.ReadOnly);
			AssertEquals("saved divot should be readonly", true, divot.ReadOnly);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			GatePassShipment shipment2 = factory2.Load<GatePassShipment>(shipment.PK);
			GatePassPackLine packLine2 = shipment2.OuterPackLines[0];
			if (shipment2.DestinationCFSDepartures[0].Divots != null)
			{ } //poke.
			CommonPickupDeliveryConfirm leg2 = shipment.DestinationCFSDepartures.AddNew();
			CommonConfirmDivot divot2 = leg2.Divots[0];

			AssertEquals("new leg should not be readonly", false, leg2.ReadOnly);
			AssertEquals("new divot should not be readonly, even when an existing one is", false, divot2.ReadOnly);
		}

		public new void TestBlankJobDocsAndCartageIsCreatedForNewShipment()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();
			AssertNotNull("A new JobDocsAndCartage should have been created when JS_RL_NKOrigin was set in SetDefaultValues when the shipment was created", shipment.DocsAndCartage);
			AssertEquals("The JobDocsAndCartage should be of type GatePassDocsAndCartage.", shipment.DocsAndCartage.GetType(), typeof(GatePassDocsAndCartage));
		}

		public new void TestJobDocsAndCartageIsReloaded()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();
			AssertNotNull("Touch JobDocsAndCartage so it's created", shipment.DocsAndCartage);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			GatePassShipment loadedShipment = factory2.Load<GatePassShipment>(shipment.PK);
			AssertEquals("The correct JobDocsAndCartage should have been reloaded", shipment.DocsAndCartage.PK, loadedShipment.DocsAndCartage.PK);
		}

		public new void TestAuditSecurity()
		{
			AssertEquals("AuditSecurity", Env.Security.CFSGatePassAuditBilling, ((IJobInvoicingPlugIn)Factory.New<GatePassShipment>()).InvoicingSupporter.AuditSecurity);
		}

		public void TestJobInvoicingSecurity()
		{
			AssertEquals("JobInvoicingSecurity", Env.Security.CFSGatePassJobInvoicing, ((IJobInvoicingPlugIn)Factory.New<GatePassShipment>()).InvoicingSupporter.JobInvoicingSecurity);
		}

		public void TestGatePassExportShipment()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;

			CFSShipment newShipment = Factory.New<CFSShipment>();
			newShipment.JS_RL_NKOrigin = HomePort;
			newShipment.JS_RL_NKDestination = OverseasPort;
			newShipment.JS_OuterPacks = 20;
			newShipment.JS_TranshipToOtherCFS = true;
			newShipment.JS_GoodsDescription = "Description";
			newShipment.ConsigneePK = consignee.PK;
			newShipment.ConsignorPK = consignor.PK;

			PackLine packLine = newShipment.OuterPackLines[0];

			CommonPickupDeliveryConfirm leg2 = newShipment.OriginCFSArrivals.AddNew();
			CommonConfirmDivot arrivalDivot = leg2.Divots[0];

			arrivalDivot.Confirm.EU_DriversName = "SOMEBODY";
			arrivalDivot.J8_PackagesDelivered = 20;
			newShipment.RunPreSaveValidation();
			AssertNoErrors("Pre-Condition: Shipment can not have errors for this test to pass", newShipment);
			Factory.Save();

			BusinessObjectFactory gatePassFactory = new BusinessObjectFactory();
			GatePassShipment shipment = gatePassFactory.Load<GatePassShipment>(newShipment.PK);

			GatePassPackLine gPPackLine = shipment.OuterPackLines[0];
			CommonPickupDeliveryConfirm deliveryLeg = newShipment.DestinationCFSDepartures.AddNew();
			CommonConfirmDivot deliveryDivot = deliveryLeg.Divots[0];
			deliveryLeg.EU_DriversName = "Fread";
			deliveryLeg.EU_TransportCoName = "Freads Five Fingers";
			deliveryLeg.EU_VehicleRegistration = "Truck R";
			deliveryDivot.J8_PackagesDelivered = 20;

			shipment.RunPreSaveValidation();
			AssertNoNotifications("Delivering All Packlines", deliveryDivot.J8_PackagesDeliveredInfo);

			deliveryDivot.J8_PackagesDelivered = 21;
			shipment.RunPreSaveValidation();
			AssertHasWarnings("Delivering To Many Packages - Expecting Warning", deliveryDivot.J8_PackagesDeliveredInfo);
			AssertHasWarningContaining(deliveryDivot.J8_PackagesDeliveredInfo, "You cannot deliver more packs than there are in the shipment. Please check before proceeding.");
			deliveryDivot.J8_PackagesDelivered = 19;
			shipment.RunPreSaveValidation();
			AssertNoNotifications("Part Delivery", deliveryDivot.J8_PackagesDeliveredInfo);

			CommonPickupDeliveryConfirm secondDeliveryLeg = newShipment.DestinationCFSDepartures.AddNew();
			CommonConfirmDivot secondDeliveryDivot = secondDeliveryLeg.Divots[0];
			secondDeliveryLeg.EU_DriversName = "Bob";
			secondDeliveryLeg.EU_TransportCoName = "Bob's Big Finger";
			secondDeliveryLeg.EU_VehicleRegistration = "Zipup";
			secondDeliveryDivot.J8_PackagesDelivered = 1;

			shipment.RunPreSaveValidation();
			AssertNoNotifications("Part Delivery", deliveryDivot.J8_PackagesDeliveredInfo);

			secondDeliveryDivot.J8_PackagesDelivered = 2;
			shipment.RunPreSaveValidation();
			AssertHasWarnings("Part Delivery", secondDeliveryDivot.J8_PackagesDeliveredInfo);
			AssertHasWarningContaining(secondDeliveryDivot.J8_PackagesDeliveredInfo, "You cannot deliver more packs than there are in the shipment. Please check before proceeding.");
		}

		#region Test Note Types

		public void TestNoteTypes()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();

			bool gatePassNotesFound = false;
			foreach (PredefinedNoteType noteType in shipment.NoteTypes)
			{
				if (noteType == PredefinedNoteTypes.Instance.GatePassNotes)
				{
					gatePassNotesFound = true;
					break;
				}
			}

			AssertEquals("Gate Pass Notes was not found.", true, gatePassNotesFound);
		}

		#endregion

		#region DocumentSupporter

		public void TestSupportedDataContext()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();
			AssertEquals("Core.Constants.DataContext.GatePassShipment is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.GatePassShipment)));
			AssertEquals("Core.Constants.DataContext.GatePassContainerLeg is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.GatePassContainerLeg)));
			AssertEquals("Core.Constants.DataContext.Service is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.Service)));
		}

		public void TestGetDocBusinessObjectForService()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();
			DocumentWrapper[] wrapper = shipment.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.Service, null);
			AssertEquals("Wrapper should be created", 1, wrapper.Length);
			AssertEquals("Wrapper should be of type GatePassShipment", "DocGatePassShipment", wrapper[0].GetType().Name);
		}

		#endregion

		#region Test New()

		public void TestNew()
		{
			GatePassShipment shipment = GatePassShipment.New(Factory);

			AssertNotNull("GatePassShipment.New(Factory) returned null.", shipment);
			AssertEquals("GatePassShipment.New(Factory) did not return a typeof(GatePassShipment).", typeof(GatePassShipment), shipment.GetType());
		}

		#endregion

		#region DocumentEventSource_DocumentPrintRequested

		public void TestDocumentEventSourceGatePassDocumentPrintRequested()
		{
			GatePassShipmentForTest shipment = Factory.New<GatePassShipmentForTest>();

			DocumentCancelEventArgs @event = GetGatePassDocumentCancelEventArgs("Gate Pass");

			CFSDataRegistry.Instance.GatepassCopiesToPrint.SetValue(
				Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, 3);
			shipment.SetDocumentPrintRequested(this, @event);
			AssertEquals("Default number of Gate Pass copies to print", ZShort.Parse("3"), @event.MenuItem.NumberOfCopies);

			CFSDataRegistry.Instance.GatepassCopiesToPrint.SetValue(
				Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, 1);
			shipment.SetDocumentPrintRequested(this, @event);
			AssertEquals("Default number of Gate Pass copies to print", ZShort.Parse("1"), @event.MenuItem.NumberOfCopies);

			@event = GetGatePassDocumentCancelEventArgs("Gate Pass Singapore");
			shipment.SetDocumentPrintRequested(this, @event);
			AssertEquals("Default number of Signapore Gate Pass copies to print", ZShort.Parse("3"), @event.MenuItem.NumberOfCopies);
		}

		DocumentCancelEventArgs GetGatePassDocumentCancelEventArgs(ZString menuName)
		{
			DocumentZQuery filter = new DocumentZQuery(Shipment.DocumentSupporter.BusinessContext, menuName);
			filter.AddToFilter(StmMenuItemSchema.SU_IsSystemDefined, ZBool.True);
			var gatePassLabelMenu = Factory.LoadTop1<StmMenuItem>(filter);
			return new DocumentCancelEventArgs(gatePassLabelMenu);
		}

		#endregion

		#region Packline tests

		public void TestCoLoadMasterOuterPackLinesReadOnly()
		{
			Shipment.ParentContainerRegistration = Factory.New<GatePassContainer>();
			PackLine pack1 = Shipment.OuterPackLines.AddNew();
			pack1.JL_PackageCount = 10;
			GatePassPackLine aggPackLine = Shipment.OuterPackLines[0];
			AssertEquals("Read Only", false, pack1.ReadOnly);
			Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			CommonShipment newCoLoad = Shipment.CoLoadShipments.AddNew();
			AssertEquals("Read Only", true, Shipment.OuterPackLines.ReadOnly);
		}

		#endregion

		#region Implementation

		protected GatePassShipment Shipment;
		bool FullyDeliveredEventCalled;
		bool FullDeliveryCancelledEventCalled;

		protected override void SetUp()
		{
			base.SetUp();
			Shipment = Factory.New<GatePassShipment>();
			Shipment.JS_TranshipToOtherCFS = true;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AU";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
			GlbBranch.CurrentBranch.GB_Code = "BNE";
			GlbDepartment.CurrentDepartment.GE_Code = "BRN";
		}

		void CancelDeliveryOfPackages(CommonPickupDeliveryConfirm leg)
		{
			Shipment.NotifyDeliveryHasBeenCancelled(leg.FullGatePass, Shipment.JS_IsFullyDelivered);
			leg.Delete();
		}

		void TestFullyDeliveredEventHandler(object sender, EventArgs e)
		{
			FullyDeliveredEventCalled = true;
		}

		void TestFullDeliveryCancelledEventHandler(object sender, EventArgs e)
		{
			FullDeliveryCancelledEventCalled = true;
		}

		protected JobSailing CreateNewSailing(bool import)
		{
			ZString domesticPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			ZString foreignPort;

			if (GlbBranch.CurrentBranch.HomePort.RL_RN_NKCountryCode == Constants.CountryCodes.Singapore)
			{
				foreignPort = "AUBNE";
			}
			else
			{
				foreignPort = "SGSIN";
			}

			if (import)
			{
				return CreateNewSailing(foreignPort, domesticPort);
			}
			else
			{
				return CreateNewSailing(domesticPort, foreignPort);
			}
		}

		protected JobSailing CreateNewSailing(ZString portOfLoading, ZString portOfDischarge)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "123";

			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = portOfLoading;
			origin.JA_E_DEP = ZDateTime.Today.AddDays(-10);
			voyage.Origins.Add(origin);

			VoyageDestination destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = portOfDischarge;
			destination.JB_E_ARV = ZDateTime.Today;
			voyage.Destinations.Add(destination);

			voyage.GenerateSailings();
			return voyage.Sailings[0];
		}

		#region class GatePassShipment for Test

		class GatePassShipmentForTest : GatePassShipment
		{
			public GatePassShipmentForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public void SetDocumentPrintRequested(object sender, DocumentCancelEventArgs e)
			{
				new GatePassShipmentForTestDocumentSupporter(this).SetDocumentPrintRequested(sender, e);
			}

			public class GatePassShipmentForTestDocumentSupporter : GatePassShipmentDocumentSupporter
			{
				public GatePassShipmentForTestDocumentSupporter(GatePassShipment shipment)
					: base(shipment)
				{
				}

				public void SetDocumentPrintRequested(object sender, DocumentCancelEventArgs e)
				{
					base.DocumentEventSource_DocumentPrintRequested(sender, e);
				}
			}
		}

		#endregion

		#endregion
	}
}
