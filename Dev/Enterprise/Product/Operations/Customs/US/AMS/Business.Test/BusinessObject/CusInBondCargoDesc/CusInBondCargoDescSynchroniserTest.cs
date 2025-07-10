using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	sealed class CusInBondCargoDescSynchroniserTest : Customs.Business.Testing.CusInBondCargoDescSynchroniserTest
	{
		public override void TestSynchroniseBY_ManifestUnitCode()
		{
			base.TestSynchroniseBY_ManifestUnitCode();

			var packType = Factory.New<RefPackType>();
			packType.F3_Code = "IBC";
			packType.F3_Description = "IBC";
			var packConversion = Factory.New<CusRefPacks>();
			packConversion.RP_CommercialPack = "IBC";
			packConversion.RP_ConversionFactor = 1;
			packConversion.RP_CustomsPack = ManifestUnitList.Codes.Tank;
			packConversion.RP_Type = RPTypeList.Codes.AMSManifest;
			Factory.Save();

			packLine.JL_PackageCount = 11;
			packLine.JL_F3_NKPackType = "IBC";

			synchroniser.Synchronise(true);
			AssertEquals(11, cargoDesc.BY_PieceCount);
			AssertEquals(ManifestUnitList.Codes.Tank, cargoDesc.BY_ManifestUnitCode);

			packConversion.RP_CustomsPack = "PK";
			Factory.Save();

			synchroniser.Synchronise(true);
			AssertEquals(11, cargoDesc.BY_PieceCount);
			AssertEquals("IBC", cargoDesc.BY_ManifestUnitCode);
		}

		public void TestOuterWithMultipleContainers()
		{
			var consol = CreateFCLConsol();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			var shipment = consol.Shipments.AddNew();
			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			packLine1.JL_JC = container1.PK;
			packLine2.JL_JC = container2.PK;

			var header = (CusInBondHeader)Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.AMS;
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var bill1 = header.Bills.AddNew();
			var moveDetail1 = bill1.MovementDetail;
			var billContainer1 = moveDetail1.Containers.AddNew();
			var billContainer2 = moveDetail1.Containers.AddNew();
			var cargoDesc1 = billContainer1.Commodities.AddNew();
			cargoDesc1.BY_Description = "DESC1";
			var cargoDesc2 = billContainer2.Commodities.AddNew();
			cargoDesc2.BY_Description = "DESC2";

			var synchroniser3 = new CusInBondBillSynchroniser(bill1, shipment);
			synchroniser3.Synchronise(true);
			var synchroniser1 = new CusInBondCargoDescSynchroniser(cargoDesc1, packLine1);
			synchroniser1.Synchronise(true);
			var synchroniser2 = new CusInBondCargoDescSynchroniser(cargoDesc2, packLine2);
			synchroniser2.Synchronise(true);

			packLine1.JL_PackageCount = 90;
			packLine1.JL_F3_NKPackType = "PKG";
			packLine2.JL_PackageCount = 110;
			packLine2.JL_F3_NKPackType = "PKG";

			CombineAssertions(() =>
			{
				AssertEquals(200, bill1.B0_ManifestQty);
				AssertEquals("PKG", bill1.B0_ManifestUQ);
				AssertEquals(90, cargoDesc1.BY_PieceCount);
				AssertEquals("PKG", cargoDesc1.BY_ManifestUnitCode);
				AssertEquals(110, cargoDesc2.BY_PieceCount);
				AssertEquals("PKG", cargoDesc2.BY_ManifestUnitCode);
			});
		}

		public void TestSynchronisation_WhenLinePriceIsDecimal()
		{
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_LinePrice = 1.23m;
			var commodity1 = billContainer.Commodities.AddNew();
			synchroniser = new CusInBondCargoDescSynchroniser(commodity1, packLine1);
			synchroniser.Synchronise(true);
			AssertEquals(1m, commodity1.BY_MonetaryValue);
			packLine1.JL_LinePrice = 2.55m;
			synchroniser.Synchronise(true);
			AssertEquals(3m, commodity1.BY_MonetaryValue);

			var packLine2 = shipment.InnerPackLines.AddNew();
			packLine2.JL_LinePrice = 11.4514m;
			var commodity2 = billContainer.Commodities.AddNew();
			synchroniser = new CusInBondCargoDescSynchroniser(commodity2, packLine2);
			synchroniser.Synchronise(true);
			AssertEquals(11m, commodity2.BY_MonetaryValue);
			packLine2.JL_LinePrice = 191.9810m;
			synchroniser.Synchronise(true);
			AssertEquals(192m, commodity2.BY_MonetaryValue);
		}

		public void TestSynchronisation_InnerPacklinesEnteredOuterPackID_FieldsFillInCorrectly()
		{
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_JC = container.PK;
			FillInPackLine(packLine1, "10.01 01.10 A", 1, 1, WeightUnitList.Codes.Kilograms, 1, ManifestUnitList.Codes.Case, "outerpack desc 1", "outerpack marks and number 1", "US");

			var commodity1 = billContainer.Commodities.AddNew();
			synchroniser = new CusInBondCargoDescSynchroniser(commodity1, packLine1);
			synchroniser.Synchronise(true);
			AssertFieldsFillInCorrectly(commodity1, "1001.01.10", 1, 1, WeightUnitList.Codes.Kilograms, 1, ManifestUnitList.Codes.Case, "outerpack desc 1", "outerpack marks and number 1", "US");

			var packLine2 = shipment.InnerPackLines.AddNew();
			packLine2.JL_JL_OuterPackLine = packLine1.PK;
			FillInPackLine(packLine2, "30.03 03.30 C", 10, 10, WeightUnitList.Codes.Pounds, 10, ManifestUnitList.Codes.Bag, "innerpack desc 1", "innerpack marks and number 1", "AU");

			var commodity2 = billContainer.Commodities.AddNew();
			synchroniser = new CusInBondCargoDescSynchroniser(commodity2, packLine2, packLine1);
			synchroniser.Synchronise(true);
			AssertFieldsFillInCorrectly(commodity2, "1001.01.10", 1, 10, WeightUnitList.Codes.Pounds, 10, ManifestUnitList.Codes.Bag, "innerpack desc 1", "outerpack marks and number 1", "US");

			packLine2.JL_Description = ZString.Empty;
			Assert("Description replace by outer cuz inner is empty", commodity2.BY_Description.EqualsIgnoringCase("outerpack desc 1"));
		}

		void AssertFieldsFillInCorrectly(CusInBondCargoDesc commodity, ZString harmonisedTariff, ZInt price, ZInt weight, ZString weightUnit, ZInt count, ZString manifestUnit, ZString description, ZString marksAndNumbers, ZString origin)
		{
			AssertNotNull(commodity);
			AssertEquals("Tariff from outer", harmonisedTariff, commodity.BY_FormattedHarmonisedTariff);
			AssertEquals("Price from outer", new ZDecimal(price), commodity.BY_MonetaryValue);
			AssertEquals("Actual weight from inner", new ZDecimal(weight), commodity.BY_GrossWeight);
			AssertEquals("Actual weight unit from inner", weightUnit, commodity.BY_GrossWeightUnit);
			AssertEquals("Piece count from inner", count, commodity.BY_PieceCount);
			AssertEquals("Manifest unit from inner", manifestUnit, commodity.BY_ManifestUnitCode);
			Assert("Description from inner", description.EqualsIgnoringCase(commodity.BY_Description));
			Assert("Marks and numbers from outer", marksAndNumbers.EqualsIgnoringCase(commodity.BY_MarksAndNumbers));
			AssertEquals("Origin from outer", origin, commodity.BY_RN_NKCountryOfOrigin);
		}

		public void TestGetDestination()
		{
			Assert(synchroniser.Destination is CusInBondCargoDesc);
		}

		ForwardingContainer container;
		ForwardingShipment shipment;
		PackLine packLine;
		CusInBondBill bill;
		CusInBondMoveDetail moveDetail;
		CusInBondContainer billContainer;
		CusInBondCargoDesc cargoDesc;
		CusInBondCargoDescSynchroniser synchroniser;

		protected override void SetUp()
		{
			base.SetUp();

			var consol = CreateFCLConsol();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;

			container = consol.Containers.AddNew();
			shipment = consol.Shipments.AddNew();
			packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_JC = container.PK;

			var header = (CusInBondHeader)Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.AMS;
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;

			bill = header.Bills.AddNew();
			moveDetail = bill.MovementDetail;
			billContainer = moveDetail.Containers.AddNew();
			cargoDesc = billContainer.Commodities.AddNew();

			synchroniser = new CusInBondCargoDescSynchroniser(cargoDesc, packLine);
			synchroniser.Synchronise(true);
		}

		void FillInPackLine(PackLine packLine, ZString harmonisedCode, ZDecimal price, ZDecimal weight, ZString weightUQ, ZInt packCount, ZString packType, ZString desc, ZString marks, ZString origin)
		{
			packLine.JL_HarmonisedCode = harmonisedCode;
			packLine.JL_LinePrice = price;
			packLine.JL_ActualWeight = weight;
			packLine.JL_ActualWeightUQ = weightUQ;
			packLine.JL_PackageCount = packCount;
			packLine.JL_F3_NKPackType = packType;
			packLine.JL_Description = desc;
			packLine.JL_MarksAndNumbers = marks;
			packLine.JL_RN_NKOrigin = origin;
		}
	}
}
