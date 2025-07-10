using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(HarmonizedNumber))]
	sealed class HarmonizedNumberTest : Customs.Business.Testing.CusCodeDataTest<HarmonizedNumber>
	{
		public void TestTariffFormatted()
		{
			var number = Factory.New<HarmonizedNumber>();
			number.CY_TariffFormatted = "0403109000";
			AssertEquals("CY_TariffFormatted", "0403.10.90 00", number.CY_TariffFormatted);
			AssertEquals("CY_Data", "0403109000", number.CY_Data);
			number.CY_TariffFormatted = "0403.10.90 10";
			AssertEquals("CY_TariffFormatted", "0403.10.90 10", number.CY_TariffFormatted);
			AssertEquals("CY_Data", "0403109010", number.CY_Data);
		}

		public void TestTariffFormattedTariffInfo()
		{
			var trip = Factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			var commodity = shipment.Commodities.AddNew();
			var number = commodity.HarmonizedNumbers.AddNew();
			AssertEquals("TariffType", TariffType.Import, number.CY_TariffFormattedTariffInfo.TariffType);
			AssertEquals("DateForDutyRate", ZDateTime.Today, number.CY_TariffFormattedTariffInfo.DateForDutyRate);
			shipment.B0_ShipmentType = ShipmentTypes.Codes.Inbond;
			AssertEquals("TariffType", TariffType.Import, number.CY_TariffFormattedTariffInfo.TariffType);
			AssertEquals("DateForDutyRate", ZDateTime.Today, number.CY_TariffFormattedTariffInfo.DateForDutyRate);
			shipment.InBond.BM_InBondEntryType = InbondTypes.Codes.ImmediateExportation;
			AssertEquals("TariffType", TariffType.Export, number.CY_TariffFormattedTariffInfo.TariffType);
			AssertEquals("DateForDutyRate", ZDateTime.Today, number.CY_TariffFormattedTariffInfo.DateForDutyRate);
		}

		public void TestSetDefaultValues()
		{
			var number = Factory.New<HarmonizedNumber>();
			AssertEquals(CusCodeDataTypeList.Codes.HarmonizedNumber, number.CY_Type);
			AssertEquals(CusCodeDataTypeList.Codes.HarmonizedNumber, number.CY_Code);
		}

		public void TestParent()
		{
			var commodity = Factory.New<Commodity>();
			var number = Factory.New<HarmonizedNumber>();
			number.CY_ParentID = commodity.PK;
			number.CY_ParentTableCode = commodity.TablePrefix;
			AssertEquals(commodity, number.Parent);
			number = commodity.HarmonizedNumbers.AddNew();
			AssertEquals(commodity.TablePrefix, number.CY_ParentTableCode);
			AssertEquals(commodity.PK, number.CY_ParentID);
			AssertEquals(commodity, number.Parent);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var trip = factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			var commondity = shipment.Commodities.AddNew();
			return commondity.HarmonizedNumbers.AddNew();
		}
	}
}
