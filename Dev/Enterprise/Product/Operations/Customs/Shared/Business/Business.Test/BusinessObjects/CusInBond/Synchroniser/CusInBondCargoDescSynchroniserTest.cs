using CargoWise.Types;
using Enterprise.Customs.Business.AMS;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.Business.Testing
{
	public class CusInBondCargoDescSynchroniserTest : SynchroniserTestCase
	{
		public void TestSynchroniseBY_FormattedHarmonisedTariff()
		{
			packLine.JL_HarmonisedCode = "10.10 .1001 A";
			AssertEquals("1010.10.01", cargoDesc.BY_FormattedHarmonisedTariff);

			packLine.JL_HarmonisedCode = "2010B1001";
			AssertEquals("2010.10.01", cargoDesc.BY_FormattedHarmonisedTariff);
		}

		public void TestSynchroniseBY_GrossWeight()
		{
			CombineAssertions(() =>
			{
				packLine.JL_ActualWeight = 10m;
				AssertEquals("Initial", 10m, cargoDesc.BY_GrossWeight);

				packLine.JL_ActualWeight = 253.45m;
				AssertEquals("Updated", 253.45m, cargoDesc.BY_GrossWeight);
			});
		}

		public void TestSynchroniseBY_GrossWeightUnit()
		{
			CombineAssertions(() =>
			{
				packLine.JL_ActualWeightUQ = Core.Constants.Weight.Hectograms;
				AssertEquals("Initial", Core.Constants.Weight.Hectograms, cargoDesc.BY_GrossWeightUnit);

				packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
				AssertEquals("Updated", Core.Constants.Weight.Tonnes, cargoDesc.BY_GrossWeightUnit);
			});
		}

		public void TestSynchroniseBY_RN_NKCountryOfOrigin()
		{
			packLine.JL_RN_NKOrigin = Core.Constants.CountryCodes.Australia;
			AssertEquals(Core.Constants.CountryCodes.Australia, cargoDesc.BY_RN_NKCountryOfOrigin);

			packLine.JL_RN_NKOrigin = Core.Constants.CountryCodes.NewZealand;
			AssertEquals(Core.Constants.CountryCodes.NewZealand, cargoDesc.BY_RN_NKCountryOfOrigin);
		}

		public void TestSynchroniseBY_MonetaryValue()
		{
			packLine.JL_LinePrice = 10m;
			AssertEquals(10m, cargoDesc.BY_MonetaryValue);

			packLine.JL_LinePrice = 253.45m;
			AssertEquals(253m, cargoDesc.BY_MonetaryValue);

			packLine.JL_LinePrice = 253.54m;
			AssertEquals(254m, cargoDesc.BY_MonetaryValue);
		}

		public void TestSynchroniseBY_Description()
		{
			packLine.JL_Description = "HELLO WORLD";
			AssertEquals("HELLO WORLD", cargoDesc.BY_Description);
		}

		public void TestSynchroniseBY_MarksAndNumbers()
		{
			shipment.JS_MarksAndNumbers = "WHAT WORLD".PadRight(2000, 'A');
			packLine.JL_MarksAndNumbers = "HELLO WORLD";
			AssertEquals("HELLO WORLD", cargoDesc.BY_MarksAndNumbers);

			AssertEquals(2048, cargoDesc.BY_MarksAndNumbersInfo.MaxLength);
			packLine.JL_MarksAndNumbers = "HELLO WORLD".PadRight(2049, 'B');
			AssertEquals("HELLO WORLD".PadRight(2048, 'B'), cargoDesc.BY_MarksAndNumbers);
			packLine.JL_MarksAndNumbers = ZString.Empty;
			AssertEquals("WHAT WORLD".PadRight(2000, 'A'), cargoDesc.BY_MarksAndNumbers);
			shipment.JS_MarksAndNumbers = ZString.Empty;
			AssertEquals(ZString.Empty, cargoDesc.BY_MarksAndNumbers);
		}

		public void TestSynchroniseBY_PieceCount()
		{
			packLine.JL_PackageCount = 10;
			AssertEquals(10, cargoDesc.BY_PieceCount);

			packLine.JL_PackageCount = 253;
			AssertEquals(253, cargoDesc.BY_PieceCount);
		}

		public virtual void TestSynchroniseBY_ManifestUnitCode()
		{
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Basket;
			AssertEquals(Core.Constants.PkgUnit.Basket, cargoDesc.BY_ManifestUnitCode);

			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			AssertEquals(AMSConstants.PackageType.Pail, cargoDesc.BY_ManifestUnitCode);

			shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Piece;
			AssertEquals(AMSConstants.PackageType.Pieces, cargoDesc.BY_ManifestUnitCode);

			shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Bundle;
			AssertEquals(AMSConstants.PackageType.Bundle, cargoDesc.BY_ManifestUnitCode);

			shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Roll;
			AssertEquals(AMSConstants.PackageType.Roll, cargoDesc.BY_ManifestUnitCode);

			shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Pail;
			AssertEquals(AMSConstants.PackageType.Pail, cargoDesc.BY_ManifestUnitCode);
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

			bill = (CusInBondBill)header.Bills.AddNew();
			moveDetail = bill.MovementDetail;
			billContainer = (CusInBondContainer)moveDetail.Containers.AddNew();
			cargoDesc = billContainer.Commodities.AddNew();

			synchroniser = new CusInBondCargoDescSynchroniser(cargoDesc, packLine);
			synchroniser.Synchronise(true);
		}
	}
}
