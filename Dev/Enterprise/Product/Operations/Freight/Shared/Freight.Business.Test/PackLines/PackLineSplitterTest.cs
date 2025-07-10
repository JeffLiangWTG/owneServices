using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(PackLineSplitter))]
	sealed class PackLineSplitterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			PackLine line = Factory.New<PackLine>();
			CommonContainer container = Factory.New<CommonContainer>();
			PackLineSplitter splitter = new PackLineSplitter(line, container);
			return splitter;
		}

		public void TestSplit()
		{
			AssertEquals("Expecting CommonShipment total volume to be 4", 4.0m, Shipment.JS_ActualVolume);
			AssertEquals("Expecting packline total volume to be 4", 4.0m, ExistingLine.JL_ActualVolume);

			Splitter.SplitPackages = 6;

			AssertEquals("Expecting split volume to be 2.0", 2.0m, Splitter.SplitVolume, 1);
			AssertEquals("Expecting split weight to be 400.0", 400.0m, Splitter.SplitWeight);

			Splitter.Split();

			AssertEquals("Expecting line volume to be 2.0", 2.0m, ExistingLine.JL_ActualVolume, 1);
			AssertEquals("Expecting line weight to be 400.0", 400.0m, ExistingLine.JL_ActualWeight);

			AssertEquals("Expecting line packs to be 6", 6, ExistingLine.JL_PackageCount);

			AssertEquals("Expecting container to have one packline", 2, ExistingContainer.PackLines.Count);
			AssertEquals("Expecting CommonShipment to have two packlines", 2, Shipment.OuterPackLines.Count);
			AssertEquals("Expecting container packline to have 6 packs", 6, ExistingContainer.PackLines[0].JL_PackageCount);
			AssertEquals("Expecting container total packs to be 6", 12, ExistingContainer.JC_Calc_TotalPackages);

			//split again - don't rejoin in container

			Splitter = new PackLineSplitter(ExistingLine, ExistingContainer);
			Consol.Containers.QueryReJoinPackLines += new CommonContainerCollection.QueryReJoinPackLinesEventHandler(OnContainers_QueryReJoinPackLines);
			fSetReJoinPackLines = false;

			Splitter.SplitPackages = 3;

			//Assert("Expecting ExistingContainer.ReJoinSameShipmentPackLines to be false", !ExistingContainer.ReJoinSameShipmentPackLines);

			Splitter.Split();

			AssertEquals("Expecting line packs to be 3", 3, ExistingLine.JL_PackageCount);
			AssertEquals("Expecting container to have 3 packlines", 3, ExistingContainer.PackLines.Count);
			AssertEquals("Expecting container total packs to be 12", 12, ExistingContainer.JC_Calc_TotalPackages);
			AssertEquals("Expecting container packline 1 to have 6 packs", 3, ExistingContainer.PackLines[0].JL_PackageCount);
			AssertEquals("Expecting container packline 2 to have 3 packs", 6, ExistingContainer.PackLines[1].JL_PackageCount);
			AssertEquals("Expecting container packline 2 to have 3 packs", 3, ExistingContainer.PackLines[2].JL_PackageCount);

			//split again - rejoin in container

			Splitter = new PackLineSplitter(ExistingLine, ExistingContainer);
			Consol.Containers.QueryReJoinPackLines += new CommonContainerCollection.QueryReJoinPackLinesEventHandler(OnContainers_QueryReJoinPackLines);
			fSetReJoinPackLines = true;

			Splitter.SplitPackages = 1;

			//Assert("Expecting ExistingContainer.ReJoinSameShipmentPackLines to be true", !ExistingContainer.ReJoinSameShipmentPackLines);

			Splitter.Split();

			AssertEquals("Expecting line packs to be 3", 3, ExistingLine.JL_PackageCount);
			AssertEquals("Expecting container to have 3 packlines", 3, ExistingContainer.PackLines.Count);
			AssertEquals("Expecting container total packs to be 12", 12, ExistingContainer.JC_Calc_TotalPackages);
			AssertEquals("Expecting container packline 1 to have 3 packs", 3, ExistingContainer.PackLines[0].JL_PackageCount);
			AssertEquals("Expecting container packline 2 to have 6 packs", 6, ExistingContainer.PackLines[1].JL_PackageCount);
			AssertEquals("Expecting container packline 2 to have 3 packs", 3, ExistingContainer.PackLines[2].JL_PackageCount);
		}

		public void TestSplit_DivotShouldMatchPackLine()
		{
			var deliveryConfirm = Shipment.DeliveryConfirms.AddNew();

			AssertEquals("Expecting CommonShipment total volume to be 4", 4.0m, Shipment.JS_ActualVolume);
			AssertEquals("Expecting packline total volume to be 4", 4.0m, ExistingLine.JL_ActualVolume);

			Splitter.SplitPackages = 6;

			AssertEquals("Expecting split volume to be 2.0", 2.0m, Splitter.SplitVolume, 1);
			AssertEquals("Expecting split weight to be 400.0", 400.0m, Splitter.SplitWeight);

			Splitter.Split();

			AssertEquals("Expecting line volume to be 2.0", 2.0m, ExistingLine.JL_ActualVolume, 1);
			AssertEquals("Expecting line weight to be 400.0", 400.0m, ExistingLine.JL_ActualWeight);

			AssertEquals("Expecting line packs to be 6", 6, ExistingLine.JL_PackageCount);

			AssertEquals("Expecting container to have one packline", 2, ExistingContainer.PackLines.Count);
			AssertEquals("Expecting CommonShipment to have two packlines", 2, Shipment.OuterPackLines.Count);
			AssertEquals("Expecting container packline to have 6 packs", 6, ExistingContainer.PackLines[0].JL_PackageCount);
			AssertEquals("Expecting container total packs to be 6", 12, ExistingContainer.JC_Calc_TotalPackages);
			AssertEquals("DeliveryConfirms count should be 2", 1, Shipment.DeliveryConfirms.Count);

			var divots = Shipment.DeliveryConfirms[0].Divots;
			AssertEquals("divots count should be 2", 2, divots.Count);
			AssertEquals("First divot should match first packline", ExistingContainer.PackLines[0].PK, divots[0].J8_JL);
			AssertEquals("Second divot should match second packline", ExistingContainer.PackLines[1].PK, divots[1].J8_JL);
		}

		public void TestSplitDeletedObj()
		{
			AssertEquals("Expecting CommonShipment total volume to be 4", 4.0m, Shipment.JS_ActualVolume);
			AssertEquals("Expecting packline total volume to be 4", 4.0m, ExistingLine.JL_ActualVolume);

			Splitter.SplitPackages = 6;
			Splitter.Line.Delete();

			Splitter.Split();
			Assert(Splitter.HasErrors);
			AssertEquals(1, Splitter.GetErrors().Count());
			AssertEquals("Error - record: The packline you are trying to split has been deleted or does not exist.", Splitter.GetErrors().GetFirst().Message);
		}

		public void TestSplitterRoundsAccurately()
		{
			Shipment.JS_ActualVolume = 30m;
			Shipment.JS_ActualWeight = 30m;
			Shipment.JS_OuterPacks = 30;

			Splitter.SplitPackages = 10;
			AssertEquals("Expecting split volume to be 10.0", 10.0m, Splitter.SplitVolume);
			AssertEquals("Expecting split weight to be 10.0", 10.0m, Splitter.SplitWeight);

			Splitter.Split();

			AssertEquals("Expecting line volume to be 20.0", 20.0m, ExistingLine.JL_ActualVolume);
			AssertEquals("Expecting line weight to be 20.0", 20.0m, ExistingLine.JL_ActualWeight);
			AssertEquals("Expecting line packs to be 20", 20, ExistingLine.JL_PackageCount);
		}

		public void TestSplitterUpdatePackagesDelivered()
		{
			var confirm = Shipment.PickupConfirms.AddNew();

			var divot = ExistingLine.ConfirmDivots[0];
			divot.J8_PackagesDelivered = ExistingLine.JL_PackageCount;

			Splitter.SplitPackages = 3;
			Splitter.Split();

			AssertEquals(2, confirm.Divots.Count);
			AssertEquals("Should update the packages delivered.", 9, confirm.Divots[0].J8_PackagesDelivered);
			AssertEquals("Should update the packages delivered.", 3, confirm.Divots[1].J8_PackagesDelivered);
		}

		public void TestSplitterUpdatePackagesDelivered_ReJoinPackLines()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();
			var container = consol.Containers.AddNew();

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 15;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 23;

			container.RemovePackLine(packLine1);
			container.AddPackLine(packLine2);

			var confirm = shipment.PickupConfirms.AddNew();

			var divot1 = packLine1.ConfirmDivots[0];
			divot1.J8_PackagesDelivered = packLine1.JL_PackageCount;

			var divot2 = packLine2.ConfirmDivots[0];
			divot2.J8_PackagesDelivered = packLine2.JL_PackageCount;

			AssertEquals(2, confirm.Divots.Count);
			AssertEquals("Should equal to the packLine1's packages delivered.", 15, confirm.Divots[0].J8_PackagesDelivered);
			AssertEquals("Should equal to the packLine2's packages delivered.", 23, confirm.Divots[1].J8_PackagesDelivered);

			consol.Containers.QueryReJoinPackLines += (sender, args) =>
			{
				args.Cancel = false;
			};

			var splitter = new PackLineSplitter(packLine1, container);
			splitter.SplitPackages = 3;
			splitter.Split();

			AssertEquals(2, confirm.Divots.Count);
			AssertEquals("Should not create a new Divot.", packLine1.PK, confirm.Divots[0].J8_JL);
			AssertEquals("Should not create a new Divot.", packLine2.PK, confirm.Divots[1].J8_JL);

			AssertEquals("Should update the packages delivered.", 12, confirm.Divots[0].J8_PackagesDelivered);
			AssertEquals("Should update the packages delivered.", 26, confirm.Divots[1].J8_PackagesDelivered);
		}

		public void TestNoNegativePackLineWeightWhenSplit()
		{
			var currentPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			var otherCountryFilter = new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var otherCountry = Factory.LoadTop1<RefCountry>(otherCountryFilter);
			var otherUnloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, otherCountry.RN_Code));
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = otherUnloco.RL_Code;

			var link = consignor.BuyerLinks.AddNew(consignee);
			link.OL_RN_NKImporterCountry = otherCountry.RN_Code;

			Shipment.JS_ActualWeight = 100m;
			Shipment.JS_OuterPacks = 20;
			Shipment.ConsignorPK = consignor.PK;
			Shipment.ConsigneePK = consignee.PK;
			Shipment.JS_RL_NKDestination = currentPort.RL_Code;

			var packType = Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "PLT");
			var packageDetails = link.PackPivots.AddNew();
			packageDetails.Q0_F3 = packType.PK;
			packageDetails.Q0_Weight = 10;
			packageDetails.Q0_UnitOfWeight = "KG";

			ExistingLine.JL_ActualWeight = 100m;
			ExistingLine.JL_ActualVolume = 100m;

			Factory.Save();

			Splitter.SplitPackages = 4;
			Splitter.Split();

			var splitLine = Splitter.Container.PackLines[1];
			AssertEquals(4, splitLine.JL_PackageCount);
			AssertEquals(20.0m, splitLine.JL_ActualWeight);
			AssertEquals(20.0m, splitLine.JL_ActualVolume);

			AssertEquals(16, ExistingLine.JL_PackageCount);
			AssertEquals(80.0m, ExistingLine.JL_ActualWeight);
			AssertEquals(80.0m, ExistingLine.JL_ActualVolume);
		}

		public void TestValidateWeightVolume()
		{
			Splitter.SplitVolume = -1;
			Splitter.SplitWeight = -1;
			AssertHasError("Should be an Error", Splitter.SplitVolumeInfo, "Please enter a 'Split Volume' greater than 0.");
			AssertHasError("Should be an Error", Splitter.SplitWeightInfo, "Please enter a 'Split Weight' greater than 0.");

			Splitter.SplitVolume = 5;
			Splitter.SplitWeight = 900;
			AssertHasError("Should be an Error", Splitter.SplitVolumeInfo, "Split volume cannot be more than the volume of the existing packline.");
			AssertHasError("Should be an Error", Splitter.SplitWeightInfo, "Split weight cannot be more than the weight of the existing packline.");

			Splitter.SplitVolume = 2;
			Splitter.SplitWeight = 400;
			AssertNoNotifications("Should be NO Errors", Splitter.SplitVolumeInfo);
			AssertNoNotifications("Should be NO Errors", Splitter.SplitWeightInfo);

			Splitter.SplitVolume = 0;
			Splitter.SplitWeight = 0;
			AssertHasError("Should be an Error", Splitter.SplitVolumeInfo, "Please enter a 'Split Volume' greater than 0.");
			AssertHasError("Should be an Error", Splitter.SplitWeightInfo, "Please enter a 'Split Weight' greater than 0.");

			ExistingLine.JL_ActualVolume = 0;
			ExistingLine.JL_ActualWeight = 0;
			Splitter.ValidateSplitVolume();
			Splitter.ValidateSplitWeight();
			AssertNoNotifications("Should be NO Errors", Splitter.SplitVolumeInfo);
			AssertNoNotifications("Should be NO Errors", Splitter.SplitWeightInfo);

			Splitter.SplitVolume = -1;
			Splitter.SplitWeight = -1;
			AssertHasError("Should be an Error", Splitter.SplitVolumeInfo, "The Split Volume and Volume must be the same.");
			AssertHasError("Should be an Error", Splitter.SplitWeightInfo, "The Split Weight and Weight must be the same.");
		}

		public void TestValidateConfirmDivotPackagesDelivered()
		{
			Splitter.SplitPackages = 3;
			AssertNoErrors(Splitter.SplitPackagesInfo);

			Shipment.PickupConfirms.AddNew();
			ExistingLine.ConfirmDivots[0].J8_PackagesDelivered = 4;

			Splitter.SplitPackages = 3;
			AssertHasError(Splitter.SplitPackagesInfo,
				@"Packline cannot be split because it has multiple confirmations or Packline packs quantity does not correspond to confirmed packs quantity.
Please remove these redundant confirmations or change the confirmed packs quantity via:

Forwarding > Shipment > Pickup Or Delivery > Confirmations");

			Shipment.OriginCFSArrivals.AddNew();
			ExistingLine.ConfirmDivots[1].J8_PackagesDelivered = 4;

			Splitter.SplitPackages = 2;
			AssertHasError(Splitter.SplitPackagesInfo,
				@"Packline cannot be split because it has multiple confirmations or Packline packs quantity does not correspond to confirmed packs quantity.
Please remove these redundant confirmations or change the confirmed packs quantity via:

Forwarding > Shipment > Pickup Or Delivery > Confirmations
CFS > Shipment > Arrival");

			Shipment.PickupConfirms.DeleteAll();

			Splitter.SplitPackages = 1;
			AssertHasError(Splitter.SplitPackagesInfo,
				@"Packline cannot be split because it has multiple confirmations or Packline packs quantity does not correspond to confirmed packs quantity.
Please remove these redundant confirmations or change the confirmed packs quantity via:

CFS > Shipment > Arrival");

			Shipment.OriginCFSArrivals.DeleteAll();
			Shipment.DestinationCFSDepartures.AddNew();
			ExistingLine.ConfirmDivots[0].J8_PackagesDelivered = 4;

			Splitter.SplitPackages = 3;
			AssertHasError(Splitter.SplitPackagesInfo,
				@"Packline cannot be split because it has multiple confirmations or Packline packs quantity does not correspond to confirmed packs quantity.
Please remove these redundant confirmations or change the confirmed packs quantity via:

CFS > Gate Pass > Delivery Information");
		}

		public void TestTransitWareHouseFieldsClonedWhenSplit()
		{
			ZDateTime lastKnownTWStatusDateTime = ZDateTime.Now;
			OrgAddress orgAddress = Factory.NewWithValidTestData<OrgAddress>();

			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();
			var container = consol.Containers.AddNew();

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 2;
			packLine1.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed;
			packLine1.JL_DepartureTransitWarehouseExcluded = false;
			packLine1.JL_OA_LastKnownTransitWarehouseAddress = orgAddress.PK;
			packLine1.JL_LastKnownTransitWarehouseStatus = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received;
			packLine1.JL_LastKnownTransitWarehouseStatusDateTime = lastKnownTWStatusDateTime;

			container.RemovePackLine(packLine1);
			Assert("Container does not have packline", container.PackLines.Count == 0);

			var splitter = new PackLineSplitter(packLine1, container);
			splitter.SplitPackages = 1;
			splitter.Split();

			Assert("Packline has been split", container.PackLines.Count == 1);
			Assert("Expecting Origin Transit Warehouse Status to be Confirmed", container.PackLines[0].JL_OriginTransitWarehouseStatus == FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed);
			Assert("Expecting Departure Transit Warehouse Excluded to be False", container.PackLines[0].JL_DepartureTransitWarehouseExcluded == false);
			Assert("Expecting Last Known Transit Warehouse Address to be copied", container.PackLines[0].JL_OA_LastKnownTransitWarehouseAddress == orgAddress.PK);
			Assert("Expecting Last Known Transit Warehouse Status to be Received", container.PackLines[0].JL_LastKnownTransitWarehouseStatus == FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received);
			Assert("Expecting Last Known Transit Warehouse Status Date Time to be copied", container.PackLines[0].JL_LastKnownTransitWarehouseStatusDateTime == lastKnownTWStatusDateTime);
		}

		#region Implementation

		CommonConsol Consol;
		CommonShipment Shipment;
		PackLine ExistingLine;
		CommonContainer ExistingContainer;
		PackLineSplitter Splitter;

		bool fSetReJoinPackLines;
		void OnContainers_QueryReJoinPackLines(object sender, CommonContainerCollection.QueryReJoinPackLinesEventArgs e)
		{
			e.Cancel = !fSetReJoinPackLines;
		}

		protected override void SetUp()
		{
			base.SetUp();

			Consol = Factory.New<CommonConsol>();
			Shipment = Consol.Shipments.AddNew();
			ExistingContainer = Factory.New<CommonContainer>();

			Shipment.JS_ActualVolume = new ZDecimal(4.0);
			Shipment.JS_ActualWeight = new ZDecimal(800.0);
			Shipment.JS_OuterPacks = new ZInt(12);

			ExistingLine = Shipment.OuterPackLines[0];

			Consol.Containers.Add(ExistingContainer);

			Splitter = new PackLineSplitter(ExistingLine, ExistingContainer);

			Factory.Save();
		}

		#endregion
	}
}
