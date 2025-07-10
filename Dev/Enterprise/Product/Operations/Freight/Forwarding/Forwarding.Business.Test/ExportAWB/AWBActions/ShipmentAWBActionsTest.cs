using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(ShipmentAWBActions))]
	sealed class ShipmentAWBActionsTest : AWBActionsTest
	{
		#region Tests

		const string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
			"<ShipmentAWBActionsSettings><PrintBarcodeLabel>Y</PrintBarcodeLabel><FiveInchLabel>N</FiveInchLabel>" +
			"<PrintOptionalInformation>Y</PrintOptionalInformation><LabelPrinter>0c762aec-2819-4b36-ac7a-289932636da5</LabelPrinter><LabelUseEPrint>N</LabelUseEPrint>" +
			"<PrintNeutralAWBsAsLaser>N</PrintNeutralAWBsAsLaser><MAWBPrinter>00000000-0000-0000-0000-000000000000</MAWBPrinter><MAWBUseEPrint>N</MAWBUseEPrint>" +
			"<PrintHAWBBarcodeLabels>N</PrintHAWBBarcodeLabels><HAWBLabelPrinter>a418bbbb-d379-44d6-a4db-6b2b2ab81afa</HAWBLabelPrinter><HAWBLabelUseEPrint>N</HAWBLabelUseEPrint>" +
			"<AWBPackagesLabel>Y</AWBPackagesLabel>" +
			"<ConsolTotalPiecesFromMAWB>Y</ConsolTotalPiecesFromMAWB></ShipmentAWBActionsSettings>";

		const string xmlWithoutShipmentAWBSpecificProperties = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
			"<ShipmentAWBActionsSettings><PrintBarcodeLabel>Y</PrintBarcodeLabel><FiveInchLabel>N</FiveInchLabel>" +
			"<PrintOptionalInformation>Y</PrintOptionalInformation><LabelPrinter>0c762aec-2819-4b36-ac7a-289932636da5</LabelPrinter><LabelUseEPrint>N</LabelUseEPrint>" +
			"<PrintNeutralAWBsAsLaser>N</PrintNeutralAWBsAsLaser><MAWBPrinter>00000000-0000-0000-0000-000000000000</MAWBPrinter><MAWBUseEPrint>N</MAWBUseEPrint>" +
			"<PrintHAWBBarcodeLabels>N</PrintHAWBBarcodeLabels><HAWBLabelPrinter>a418bbbb-d379-44d6-a4db-6b2b2ab81afa</HAWBLabelPrinter><HAWBLabelUseEPrint>N</HAWBLabelUseEPrint>" +
			"</ShipmentAWBActionsSettings>";

		public override void TestLoadSettings()
		{
			CreatePrinter(new Guid("0c762aec-2819-4b36-ac7a-289932636da5"));

			Env.Registry.SetFilterCriteria("ShipmentAWBActionsSettings", xml);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			var awbActions = new ShipmentAWBActions(shipment, AWBActions.ActionsModeType.LabelsOnly);

			AssertEquals(true, awbActions.PrintBarcodeLabel);
			AssertEquals(false, awbActions.FiveInchLabel);
			AssertEquals(true, awbActions.PrintOptionalInformation);
			AssertEquals("existing printer guid", new ZGuid("0c762aec-2819-4b36-ac7a-289932636da5"), awbActions.LabelPrinter);
			AssertEquals(false, awbActions.PrintNeutralAWBsAsLaser);
			AssertEquals("empty guid", ZGuid.Empty, awbActions.MAWBPrinter);
			AssertEquals(false, awbActions.PrintHAWBBarcodeLabels);
			AssertEquals(new ZGuid("a418bbbb-d379-44d6-a4db-6b2b2ab81afa"), awbActions.HAWBLabelPrinter);
			AssertEquals(true, awbActions.AWBPackagesLabel);
			AssertEquals(true, awbActions.ConsolTotalPiecesFromMAWB);
		}

		#region TestDoPrintFiveInchBarcodeLabel

		public override void TestDoPrintFiveInchBarcodeLabel()
		{
			var queryProvider = new Mock<IForwardingShipmentDocumentSupporterQueryProvider>();
			Factory.SetValue(() => queryProvider.Object);

			queryProvider
				.SetupSequence(m => m.PrintAWBBarcodeLabel())
				.Returns(true)
				.CallBase();
			base.TestDoPrintFiveInchBarcodeLabel();
			queryProvider.Verify(m => m.PrintAWBBarcodeLabel(), Times.Once);
		}

		public override void TestDoPrintFiveInchBarcodeLabel_EPrint()
		{
			var queryProvider = new Mock<IForwardingShipmentDocumentSupporterQueryProvider>();
			Factory.SetValue(() => queryProvider.Object);

			queryProvider
				.SetupSequence(m => m.PrintAWBBarcodeLabel())
				.Returns(true)
				.CallBase();
			base.TestDoPrintFiveInchBarcodeLabel_EPrint();
			queryProvider.Verify(m => m.PrintAWBBarcodeLabel(), Times.Once);
		}

		#endregion

		#region TestDoPrintSixInchBarcodeLabel

		public override void TestDoPrintSixInchBarcodeLabel()
		{
			var queryProvider = new Mock<IForwardingShipmentDocumentSupporterQueryProvider>();
			Factory.SetValue(() => queryProvider.Object);

			queryProvider
				.SetupSequence(m => m.PrintAWBBarcodeLabel())
				.Returns(true)
				.CallBase();

			base.TestDoPrintSixInchBarcodeLabel();
			queryProvider.Verify(m => m.PrintAWBBarcodeLabel(), Times.Once);
		}

		#endregion

		public void TestLoadSettingsFromXmlWitoutShipmentAWBSpecificPropertiesWillNotCrash()
		{
			CreatePrinter(new Guid("0c762aec-2819-4b36-ac7a-289932636da5"));

			Env.Registry.SetFilterCriteria("ShipmentAWBActionsSettings", xmlWithoutShipmentAWBSpecificProperties);

			var awbActions = new ShipmentAWBActions(Factory.New<ForwardingShipment>(), AWBActions.ActionsModeType.LabelsOnly);

			AssertEquals(true, awbActions.PrintBarcodeLabel);
			AssertEquals(false, awbActions.FiveInchLabel);
			AssertEquals(true, awbActions.PrintOptionalInformation);
			AssertEquals("existing printer guid", new ZGuid("0c762aec-2819-4b36-ac7a-289932636da5"), awbActions.LabelPrinter);
			AssertEquals(false, awbActions.PrintNeutralAWBsAsLaser);
			AssertEquals("empty guid", ZGuid.Empty, awbActions.MAWBPrinter);
			AssertEquals(false, awbActions.PrintHAWBBarcodeLabels);
			AssertEquals(new ZGuid("a418bbbb-d379-44d6-a4db-6b2b2ab81afa"), awbActions.HAWBLabelPrinter);
		}

		public override void TestSaveSettings()
		{
			CreatePrinter(new Guid("0c762aec-2819-4b36-ac7a-289932636da5"));
			CreatePrinter(new Guid("a418bbbb-d379-44d6-a4db-6b2b2ab81afa"));
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var awbActions = new ShipmentAWBActions(shipment, AWBActions.ActionsModeType.LabelsOnly);
			awbActions.PrintBarcodeLabel = true;
			awbActions.FiveInchLabel = false;
			awbActions.PrintOptionalInformation = true;
			awbActions.LabelPrinter = new ZGuid("0c762aec-2819-4b36-ac7a-289932636da5");
			awbActions.PrintNeutralAWBsAsLaser = false;
			awbActions.MAWBPrinter = ZGuid.Empty;
			awbActions.PrintHAWBBarcodeLabels = false;
			awbActions.HAWBLabelPrinter = new ZGuid("a418bbbb-d379-44d6-a4db-6b2b2ab81afa");
			awbActions.AWBPackagesLabel = true;
			awbActions.ConsolTotalPiecesFromMAWB = true;

			awbActions.SaveSettings();

			AssertEquals(xml, Env.Registry.GetFilterCriteria("ShipmentAWBActionsSettings"));
		}

		public void TestSetAdditionalDefaults()
		{
			Assert(AWBActions.PrintBarcodeLabel);
			AssertEquals(true, AWBActions.ParentPackagesLabel);
			AssertEquals(false, AWBActions.AWBPackagesLabel);
			Assert(AWBActions.PrintOptionalInformation);
			Assert(AWBActions.FiveInchLabel);
			Assert(!AWBActions.SixInchLabel);
			AssertEquals(7, AWBActions.TotalPacks);
			AssertEquals(7, AWBActions.LabelRangeTo);

			AWBActions.PrintBarcodeLabel = false;
			AWBActions.LabelPrinter = ZGuid.NewZGuid();
			AWBActions.SixInchLabel = true;
			AWBActions.FiveInchLabel = false;
			AWBActions.PrintOptionalInformation = false;

			AWBActions.SaveSettings();

			fAWBActions = new ShipmentAWBActions(shipment, AWBActions.ActionsModeType.All);

			Assert(!AWBActions.PrintBarcodeLabel);
			Assert(!AWBActions.LabelPrinter.IsValid);
			Assert(!AWBActions.FiveInchLabel);
			Assert(AWBActions.SixInchLabel);
			Assert(!AWBActions.PrintOptionalInformation);

			StmPrintQueue queue = Factory.New<StmPrintQueue>();
			AWBActions.LabelPrinter = queue.PK;

			AWBActions.SaveSettings();

			fAWBActions = new ShipmentAWBActions(shipment, AWBActions.ActionsModeType.All);
			Assert(AWBActions.LabelPrinter.IsValid);
			AssertEquals(1, AWBActions.LabelRangeFrom);
			AssertEquals(7, AWBActions.LabelRangeTo);

			fAWBActions = new ShipmentAWBActions(shipment, AWBActions.ActionsModeType.LabelsOnly);
			Assert(AWBActions.PrintBarcodeLabel);
			AssertEquals(1, AWBActions.LabelRangeFrom);
			AssertEquals(7, AWBActions.LabelRangeTo);

			fAWBActions = new ShipmentAWBActions(shipment, AWBActions.ActionsModeType.None);
			Assert(!AWBActions.PrintBarcodeLabel);
			AssertEquals(1, AWBActions.LabelRangeFrom);
			AssertEquals(7, AWBActions.LabelRangeTo);

			var consol = shipment.Consols[0];
			consol.JK_AgentType = Core.Constants.AgentType.Courier;
			consol.JK_UniqueConsignRef = "C000123";
			Factory.Save();

			var awbActions = new ShipmentAWBActions(shipment, AWBActions.ActionsModeType.LabelsOnly);
			AssertEquals(ZGuid.Empty, awbActions.LabelConsol);
			AssertEquals(true, awbActions.LabelConsolInfo.HasErrors());
			AssertEquals(true, awbActions.LabelConsolInfo.HasError("Please enter a value."));
		}

		public void TestLabelConsolRequired()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
			var awbActions = new ShipmentAWBActions(shipment1, Business.AWB.AWBActions.ActionsModeType.LabelsOnly);

			AssertEquals("Prerequisite", ZGuid.Empty, awbActions.LabelConsol);
			awbActions.ValidateLabelConsol();

			AssertEquals(true, awbActions.LabelConsolInfo.HasErrors());
			AssertEquals(true, awbActions.LabelConsolInfo.HasError("Please enter a value."));

			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();

			awbActions.LabelConsol = consol1.PK;

			awbActions.ValidateLabelConsol();

			AssertEquals(false, awbActions.LabelConsolInfo.HasErrors());
			AssertEquals(false, awbActions.LabelConsolInfo.HasError("Please enter a value."));

			awbActions.ValidateLabelConsol();
		}

		public void TestPrintAWBBarcodeLabel()
		{
			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";
			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			var awbActions = new ShipmentAWBActions(shipment, Business.AWB.AWBActions.ActionsModeType.LabelsOnly)
			{
				LabelPrinter = printer.PK,
				PrintBarcodeLabel = ZBool.True
			};

			var queryProvider = new Mock<IForwardingShipmentDocumentSupporterQueryProvider>();
			Factory.SetValue(() => queryProvider.Object);

			queryProvider
				.SetupSequence(m => m.PrintAWBBarcodeLabel())
				.Returns(true)
				.CallBase();
			awbActions.PrintAWBBarcodeLabel();
			var filter = new ZQuery(StmPrintJobSchema.SP_SQ, printer.PK);
			BusinessObject[] printJobs = Factory.Load(typeof(StmPrintJob), filter);
			AssertEquals(1, printJobs.Length);
			queryProvider.Verify(m => m.PrintAWBBarcodeLabel(), Times.Once);
		}

		public void TestPrintTotalNumberOfPiecesOnShipmentAWBBarcodeLabel()
		{
			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";
			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_HouseBill = "12345678";
			shipment.JS_OuterPacks = 5;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			AssertEquals("Prerequisite", consol, shipment.CurrentBranchDepartureAirConsol);

			FreightDataRegistry.Instance.PrintTotalNumberOfPiecesOnShipmentAWBBarcodeLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var awbActions = new ShipmentAWBActions(shipment, Business.AWB.AWBActions.ActionsModeType.LabelsOnly) { LabelPrinter = printer.PK, PrintBarcodeLabel = ZBool.True };
			awbActions.PrintAWBBarcodeLabel();

			AssertEquals(5, shipment.AWBHeader.MAWBLabelTotalPacks);

			FreightDataRegistry.Instance.PrintTotalNumberOfPiecesOnShipmentAWBBarcodeLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			awbActions = new ShipmentAWBActions(shipment, Business.AWB.AWBActions.ActionsModeType.LabelsOnly) { LabelPrinter = printer.PK, PrintBarcodeLabel = ZBool.True };
			awbActions.PrintAWBBarcodeLabel();

			AssertEquals(0, shipment.AWBHeader.MAWBLabelTotalPacks);
		}

		public void TestShipmentTotalPieces()
		{
			var awbActions = (ShipmentAWBActions)AWBActions;

			awbActions.ParentPackagesLabel = true;
			AssertEquals(7, awbActions.TotalPacks);

			awbActions.AWBPackagesLabel = true;
			AssertEquals(13, awbActions.TotalPacks);
		}

		public void TestConsolTotalPieces()
		{
			var awbActions = (ShipmentAWBActions)AWBActions;

			awbActions.ConsolTotalPiecesFromConsol = true;
			AssertEquals(11, awbActions.ConsolTotalPacks);

			awbActions.ConsolTotalPiecesFromMAWB = true;
			AssertEquals(19, awbActions.ConsolTotalPacks);
		}

		public void TestConsolTotalPiecesSuppress()
		{
			var awbActions = (ShipmentAWBActions)AWBActions;

			awbActions.ConsolTotalPiecesSuppress = false;
			AssertEquals(false, awbActions.ConsolTotalRangeFromInfo.ReadOnly);

			awbActions.ConsolTotalPiecesSuppress = true;
			AssertEquals(true, awbActions.ConsolTotalRangeFromInfo.ReadOnly);

			AssertEquals("Prerequisite", 11, awbActions.ConsolTotalPacks);

			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";
			Factory.Save();

			awbActions.LabelPrinter = printer.PK;

			awbActions.PrintAWBBarcodeLabel();

			AssertEquals(7, AWB.LabelTotalPacks);
			AssertEquals(0, AWB.MAWBLabelTotalPacks);
		}

		public void TestConsolTotalRangeTo()
		{
			var awbActions = (ShipmentAWBActions)AWBActions;
			awbActions.ParentPackagesLabel = true;

			AssertEquals("Prerequisite", 1, awbActions.LabelRangeFrom);
			AssertEquals("Prerequisite", 7, awbActions.LabelRangeTo);
			AssertEquals("Prerequisite", 1, awbActions.ConsolTotalRangeFrom);
			AssertEquals("Prerequisite", 11, awbActions.ConsolTotalPacks);

			AssertEquals(7, awbActions.ConsolTotalRangeTo);

			awbActions.LabelRangeFrom = 3;
			AssertEquals(5, awbActions.ConsolTotalRangeTo);

			awbActions.LabelRangeTo = 5;
			AssertEquals(3, awbActions.ConsolTotalRangeTo);

			awbActions.ConsolTotalRangeFrom = 3;
			AssertEquals(5, awbActions.ConsolTotalRangeTo);
		}

		public void TestValidateConsolTotalRangeFrom()
		{
			var awbActions = (ShipmentAWBActions)AWBActions;
			awbActions.ParentPackagesLabel = true;

			AssertEquals("Prerequisite", 1, awbActions.LabelRangeFrom);
			AssertEquals("Prerequisite", 7, awbActions.LabelRangeTo);
			AssertEquals("Prerequisite", 1, awbActions.ConsolTotalRangeFrom);
			AssertEquals("Prerequisite", 11, awbActions.ConsolTotalPacks);

			awbActions.ConsolTotalRangeFrom = 0;
			AssertHasError(awbActions.ConsolTotalRangeFromInfo, "Start range needs to be greater or equal to 1");

			awbActions.ConsolTotalRangeFrom = 1;
			AssertEquals(false, awbActions.ConsolTotalRangeFromInfo.HasErrors());

			awbActions.ConsolTotalRangeFrom = 12;
			AssertHasError(awbActions.ConsolTotalRangeFromInfo, "Start range needs to be less or equal to 5");

			awbActions.ConsolTotalPiecesSuppress = true;
			AssertEquals(false, awbActions.ConsolTotalRangeFromInfo.HasErrors());

			awbActions.ConsolTotalPiecesSuppress = false;
			AssertHasError(awbActions.ConsolTotalRangeFromInfo, "Start range needs to be less or equal to 5");
		}

		public override void TestDocumentSettings()
		{
			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";
			Factory.Save();

			var awbActions = (ShipmentAWBActions)AWBActions;

			awbActions.LabelPrinter = printer.PK;

			awbActions.FiveInchLabel = false;
			awbActions.LabelRangeFrom = 3;
			awbActions.LabelRangeTo = 5;
			awbActions.TotalPacks = 7;
			awbActions.ConsolTotalRangeFrom = 1;

			awbActions.PrintOptionalInformation = true;

			awbActions.PrintAWBBarcodeLabel();

			AssertEquals("6 Inch", AWB.DocumentSize);
			AssertEquals(3, AWB.LabelStartRange);
			AssertEquals(5, AWB.LabelEndRange);
			AssertEquals(7, AWB.LabelTotalPacks);
			AssertEquals(1, AWB.MAWBLabelStartRange);
			AssertEquals(11, AWB.MAWBLabelTotalPacks);
			AssertEquals(true, AWB.PrintOptionalInformation);
			AssertEquals(true, AWB.DocumentSettingsPopulated);
		}

		public void TestConsolAWBPopulateIsCalledOnlyOnce()
		{
			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";
			Factory.Save();

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol2.JK_OverrideWaybillDefaults = true;

			var awbActions = (ShipmentAWBActions)AWBActions;
			awbActions.LabelPrinter = printer.PK;
			awbActions.LabelConsol = ZGuid.Empty;
			awbActions.ConsolTotalPiecesFromMAWB = true;

			AssertEquals("Precondition", 2, shipment.Consols.Count);

			shipment.Consols[0].JK_OverrideWaybillDefaults = false;
			shipment.Consols[1].JK_OverrideWaybillDefaults = false;

			List<ZGuid> consolsWithPopulatedAWB = new List<ZGuid>();

			EventHandler handler = (s, e) =>
			{
				var consolPK = ((ExportAWBHeader)s).EH_ParentID;

				if (consolsWithPopulatedAWB.Contains(consolPK))
				{
					Fail("Consol called PopulateAWB() more than once");
				}
				else
				{
					consolsWithPopulatedAWB.Add(consolPK);
				}
			};

			ExportAWBHeader.OnPopulated += handler;

			try
			{
				awbActions.LabelConsol = shipment.Consols[0].PK;
				awbActions.LabelConsol = shipment.Consols[1].PK;
				awbActions.LabelConsol = shipment.Consols[0].PK;
				awbActions.LabelConsol = shipment.Consols[1].PK;
				awbActions.PrintAWBBarcodeLabel();
			}
			finally
			{
				ExportAWBHeader.OnPopulated -= handler;
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_HouseBill = "08112345678";
			shipment.JS_OuterPacks = 7;

			shipment.JS_OverrideWaybillDefaults = true;
			shipment.AWBHeader.AWBRateLines[0].ER_NoOfPiecesOrRCP = "13";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var anotherShipment = consol.Shipments.AddNew();
			anotherShipment.JS_RL_NKOrigin = "AUSYD";
			anotherShipment.JS_RL_NKDestination = "SGSIN";
			anotherShipment.JS_OuterPacks = 4;

			consol.JK_OverrideWaybillDefaults = true;
			consol.AWBHeader.AWBRateLines[0].ER_NoOfPiecesOrRCP = "19";

			fAWBActions = new ShipmentAWBActions(shipment, Business.AWB.AWBActions.ActionsModeType.All);
			AWB = shipment.AWBHeader;
		}

		ForwardingShipment shipment;

		protected override BusinessObject GetNewBusinessObject()
		{
			return AWBActions;
		}

		#endregion
	}
}
