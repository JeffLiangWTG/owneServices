using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgTradeDetailTotalsCalculatorTest : TestCaseWithFactory
	{
		#region TotalAnnualCount

		public void TestTotalAnnualCount()
		{
			var sales = Factory.New<OrgSales>();
			var collection = new OrgTradeDetailCollection(sales);

			var tradeDetail1 = collection.AddNew();
			tradeDetail1.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail1.CurrentProspectPeriod.PAS_RepeatsMnth = 2;

			var tradeDetail2 = collection.AddNew();
			tradeDetail2.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Yearly;
			tradeDetail2.CurrentProspectPeriod.PAS_RepeatsMnth = 5;

			var calculator = new OrgTradeDetailTotalsCalculator();
			AssertEquals((2m * 12) + 5, calculator.GetTotalAnnualCount(collection.Cast<OrgTradeDetail>()));
		}

		#endregion

		#region TotalAnnualWeight

		public void TestTotalAnnualWeight()
		{
			var sales = Factory.New<OrgSales>();
			var collection = new OrgTradeDetailCollection(sales);

			var tradeDetail1 = collection.AddNew();
			tradeDetail1.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail1.CurrentProspectPeriod.PAS_Weight = 10;
			tradeDetail1.CurrentProspectPeriod.PAS_WeightUQ = Constants.Weight.Kilograms;

			var tradeDetail2 = collection.AddNew();
			tradeDetail2.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Yearly;
			tradeDetail2.CurrentProspectPeriod.PAS_Weight = 5;
			tradeDetail2.CurrentProspectPeriod.PAS_WeightUQ = Constants.Weight.Pounds;

			var tradeDetail3 = collection.AddNew();
			tradeDetail3.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail3.CurrentProspectPeriod.PAS_Weight = 3;
			tradeDetail3.CurrentProspectPeriod.PAS_WeightUQ = "10";

			var calculator = new OrgTradeDetailTotalsCalculator();
			CombineAssertions(() =>
			{
				AssertEquals("TotalAnnualWeight", (10m * 12) + (5m * 0.45359237m), calculator.GetTotalAnnualWeight(collection.Cast<OrgTradeDetail>()), 0.01m);
				AssertEquals("TotalAnnualWeightUQ", Constants.Weight.Kilograms, calculator.GetTotalAnnualWeightUQ(collection.Cast<OrgTradeDetail>()));
			});
		}

		#endregion

		#region TotalAnnualVolume

		public void TestTotalAnnualVolume()
		{
			var sales = Factory.New<OrgSales>();
			var collection = new OrgTradeDetailCollection(sales);

			var tradeDetail1 = collection.AddNew();
			tradeDetail1.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail1.CurrentProspectPeriod.PAS_Volume = 10;
			tradeDetail1.CurrentProspectPeriod.PAS_VolumeUQ = Constants.Volume.CubicMetres;

			var tradeDetail2 = collection.AddNew();
			tradeDetail2.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Yearly;
			tradeDetail2.CurrentProspectPeriod.PAS_Volume = 5;
			tradeDetail2.CurrentProspectPeriod.PAS_VolumeUQ = Constants.Volume.CubicFeet;

			var tradeDetail3 = collection.AddNew();
			tradeDetail3.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail3.CurrentProspectPeriod.PAS_Volume = 3;
			tradeDetail3.CurrentProspectPeriod.PAS_VolumeUQ = "15";

			var calculator = new OrgTradeDetailTotalsCalculator();
			CombineAssertions(() =>
			{
				AssertEquals("TotalAnnualVolume", (10m * 12) + (5 * 0.0283168466m), calculator.GetTotalAnnualVolume(collection.Cast<OrgTradeDetail>()), 0.01m);
				AssertEquals("TotalAnnualVolumeUQ", Constants.Volume.CubicMetres, calculator.GetTotalAnnualVolumeUQ(collection.Cast<OrgTradeDetail>()));
			});
		}

		#endregion

		#region TotalAnnualChargeable

		public void TotalAnnualChargeable()
		{
			var sales = Factory.New<OrgSales>();
			var collection = new OrgTradeDetailCollection(sales);

			var tradeDetail1 = collection.AddNew();
			tradeDetail1.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail1.CurrentProspectPeriod.PAS_Chargeable = 2;

			var tradeDetail2 = collection.AddNew();
			tradeDetail2.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Yearly;
			tradeDetail2.CurrentProspectPeriod.PAS_Chargeable = 5;

			var calculator = new OrgTradeDetailTotalsCalculator();
			AssertEquals((2 * 12) + 5, calculator.GetTotalAnnualChargeable(false, collection.Cast<OrgTradeDetail>()));
		}

		#endregion

		#region TotalAnnualTEU

		public void TestTotalAnnualTEU()
		{
			var sales = Factory.New<OrgSales>();
			var collection = new OrgTradeDetailCollection(sales);

			var tradeDetail1 = collection.AddNew();
			tradeDetail1.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail1.CurrentProspectPeriod.PAS_TEUQuantity = 2;

			var tradeDetail2 = collection.AddNew();
			tradeDetail2.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Yearly;
			tradeDetail2.CurrentProspectPeriod.PAS_TEUQuantity = 5;

			var calculator = new OrgTradeDetailTotalsCalculator();
			AssertEquals((2m * 12) + 5m, calculator.GetTotalAnnualTEU(collection.Cast<OrgTradeDetail>()));
		}

		#endregion

		#region TotalAnnualPalletCount

		public void TotalAnnualPalletCount()
		{
			var sales = Factory.New<OrgSales>();
			var collection = new OrgTradeDetailCollection(sales);

			var tradeDetail1 = collection.AddNew();
			tradeDetail1.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail1.CurrentProspectPeriod.PAS_PalletCount = 2;

			var tradeDetail2 = collection.AddNew();
			tradeDetail2.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Yearly;
			tradeDetail2.CurrentProspectPeriod.PAS_PalletCount = 5;

			var calculator = new OrgTradeDetailTotalsCalculator();
			AssertEquals((2m * 12) + 5m, calculator.GetTotalAnnualPalletCount(collection.Cast<OrgTradeDetail>()));
		}

		#endregion

		#region TotalAnnualMetricVolume

		public void TestTotalAnnualMetricVolume()
		{
			var sales = Factory.New<OrgSales>();
			var collection = new OrgTradeDetailCollection(sales);

			var tradeDetail1 = collection.AddNew();
			tradeDetail1.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail1.CurrentProspectPeriod.PAS_Volume = 10;
			tradeDetail1.CurrentProspectPeriod.PAS_VolumeUQ = Constants.Volume.CubicMetres;

			var tradeDetail2 = collection.AddNew();
			tradeDetail2.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Yearly;
			tradeDetail2.CurrentProspectPeriod.PAS_Volume = 5;
			tradeDetail2.CurrentProspectPeriod.PAS_VolumeUQ = Constants.Volume.CubicFeet;

			var tradeDetail3 = collection.AddNew();
			tradeDetail3.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail3.CurrentProspectPeriod.PAS_Volume = 3;
			tradeDetail3.CurrentProspectPeriod.PAS_VolumeUQ = "AA";

			var calculator = new OrgTradeDetailTotalsCalculator();
			AssertEquals("TotalAnnualMetricVolume", (10m * 12) + (5 * 0.0283168466m), calculator.GetTotalAnnualMetricVolume(collection.Cast<OrgTradeDetail>()), 0.01m);
		}

		#endregion
	}
}
