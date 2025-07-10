using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Rating.Business.Testing
{
	public class CompanyTariffOrCostLineCloneHelperTest : TestCaseWithFactory
	{
		#region Test Get Clone

		#region Conversion Factor

		public void TestGetClone_CTGWithEmpyConversionFactor_CTB()
			=> TestGetClone_CTGWithEmpyConversionFactor
			(
				baseRatingHeader: Helper.NewCompanyTariff(),
				ratingHeader: Helper.NewClientRate(Helper.NewOrgHeader(1)),
				ratingHeaderCalculator: CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode
			);

		public void TestGetClone_CTGWithEmpyConversionFactor_CST()
			=> TestGetClone_CTGWithEmpyConversionFactor
			(
				baseRatingHeader: Factory.New<Costing>(),
				ratingHeader: Helper.NewCosting(Helper.NewOrgHeader(1)),
				ratingHeaderCalculator: CompanyTariffOrCostBasedCalculator.CostBasedCode
			);

		void TestGetClone_CTGWithEmpyConversionFactor(RatingHeader baseRatingHeader, RatingHeader ratingHeader, string ratingHeaderCalculator)
		{
			var baseRateEntry = baseRatingHeader.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", removeLines: true);
			var baseRateLine = baseRateEntry.AddRateLine("BAF", CartageCalculator.Code, "KG");
			baseRateLine.ConversionFactor = ConversionFactor.Empty;
			var baseCalculator = baseRateLine.GetCalculator<CartageCalculator>();
			baseCalculator["-100"] = (ZDecimal)5m;
			baseCalculator["+100"] = (ZDecimal)10m;

			baseRatingHeader.Factory.Save();

			var ratingHeaderRateEntry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", removeLines: true);
			var ratingHeaderRateLine = ratingHeaderRateEntry.AddRateLine("BAF", ratingHeaderCalculator);

			var result = new CompanyTariffOrCostLineCloneHelper(ratingHeaderRateLine).GetClone();

			AssertEquals("Conversion Factor", true, result.ConversionFactor.IsEmpty);
		}

		#endregion

		public void TestCostingFromGenericCosting()
		{
			var baseCosting = Factory.New<Costing>();
			var baseCostingEntry = baseCosting.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			var aIRRateLine11 = baseCostingEntry.AddRateLine("FRT", FlatCalculator.Code);
			((FlatCalculator)aIRRateLine11.Calculator).BaseRate = 1000m;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierCosting = Helper.NewCosting(carrier);
			var carrierEntry = carrierCosting.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			var rateLine = carrierEntry.AddRateLine("FRT", CompanyTariffOrCostBasedCalculator.CostBasedCode);
			((CompanyTariffOrCostBasedCalculator)rateLine.Calculator).Percent = 20m;

			Factory.Save();

			var result = new CompanyTariffOrCostLineCloneHelper(rateLine).GetClone();
			AssertNotNull("Generic costing found", result);
			AssertEquals("Generic costing correct uplift", 1200m, ((FlatCalculator)result.Calculator).BaseRate);
		}

		public void TestCostingFromSupplierThenCarrier()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var transportProvider1 = Factory.NewWithValidTestData<OrgHeader>();
			var transportProvider2 = Factory.NewWithValidTestData<OrgHeader>();

			var standardCosting = Helper.NewCosting(null);
			standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", "FRT", 4000);

			var supplierCosting = Helper.NewCosting(supplier);
			var supplierEntry = supplierCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", "FRT", 2000);
			supplierEntry.TI_OH_TransportProvider = transportProvider1.PK;

			var carrierCosting = Helper.NewCosting(transportProvider1);
			carrierCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", "FRT", 1000);

			var clientRate = Helper.NewClientRate(Client2);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			rateEntry.TI_OH_Supplier = supplier.PK;
			rateEntry.TI_OH_TransportProvider = transportProvider1.PK;
			var rateLine = rateEntry.AddRateLine("FRT", CompanyTariffOrCostBasedCalculator.CostBasedCode);
			((CompanyTariffOrCostBasedCalculator)rateLine.Calculator).Percent = 20m;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var clone = new CompanyTariffOrCostLineCloneHelper(rateLine, newFactory).GetClone();
			var cloneValue = ((FlatCalculator)clone.Calculator).BaseRate;

			AssertEquals("Costing taken from supplier/carrier combination", 2400m, cloneValue);

			rateEntry.TI_OH_Supplier = ZGuid.Empty;
			rateEntry.TI_OH_TransportProvider = transportProvider1.PK;
			clone = new CompanyTariffOrCostLineCloneHelper(rateLine, newFactory).GetClone();
			cloneValue = ((FlatCalculator)clone.Calculator).BaseRate;

			AssertEquals("resulting rates are based on Airline Costs", 1200M, cloneValue);

			rateEntry.TI_OH_Supplier = supplier.PK;
			rateEntry.TI_OH_TransportProvider = ZGuid.Empty;
			clone = new CompanyTariffOrCostLineCloneHelper(rateLine, newFactory).GetClone();
			cloneValue = ((FlatCalculator)clone.Calculator).BaseRate;

			AssertEquals("resulting rates are based on standard costs as supplier's transport provider doesn't match", 4800M, cloneValue);

			rateEntry.TI_OH_Supplier = ZGuid.Empty;
			rateEntry.TI_OH_TransportProvider = transportProvider2.PK;
			clone = new CompanyTariffOrCostLineCloneHelper(rateLine, newFactory).GetClone();

			AssertNull("No costing result as no cost found for this supplier or carrier", clone);

			rateEntry.TI_OH_Supplier = supplier.PK;
			rateEntry.TI_OH_TransportProvider = transportProvider2.PK;
			clone = new CompanyTariffOrCostLineCloneHelper(rateLine, newFactory).GetClone();

			AssertNull("No costing result as no cost found for the carrier within the supplier's costing", clone);

			rateEntry.TI_OH_Supplier = supplier.PK;
			rateEntry.TI_OH_TransportProvider = transportProvider1.PK;
			clone = new CompanyTariffOrCostLineCloneHelper(rateLine, newFactory).GetClone();
			cloneValue = ((FlatCalculator)clone.Calculator).BaseRate;

			AssertEquals("resulting rates are based on Supplier with correct airline Costs", 2400M, cloneValue);
		}

		public void TestGetCompanyTariffLevel2CloneOverriddenLineDiscount()
		{
			var baseTariff = new BusinessObjectFactory().New<CompanyTariff>();
			var companyTarrifEntry = baseTariff.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.AIR, "AUSYD", "");
			var companyTarrifLine = companyTarrifEntry.AddRateLine("ODOC", FlatCalculator.Code);
			((FlatCalculator)companyTarrifLine.Calculator).BaseRate = 15m;
			baseTariff.Factory.Save();

			var tarrif2 = new BusinessObjectFactory().New<CompanyTariff>();
			var tarrif2Entry = tarrif2.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG)[0];
			tarrif2Entry.RateLines.OverrideTariffLines(new[] { tarrif2Entry.RateLines[0] });
			tarrif2.Factory.Save();

			var clientRate = Helper.NewClientRate(Client2);
			var clientEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.AIR, "AUSYD", "");
			var rateLine = clientEntry.AddRateLine("ODOC", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			((CompanyTariffOrCostBasedCalculator)rateLine.Calculator).BaseRate = -5m;

			var result = new CompanyTariffOrCostLineCloneHelper(rateLine).GetClone();

			AssertEquals(10m, (ZDecimal)result.Calculator[Calculator.Items.Operator.BAS]);
		}

		public void TestGetCompanyTariffLevel2CloneMainDiscount()
		{
			var baseTariff = new BusinessObjectFactory().New<CompanyTariff>();
			var baseTariffEntry = baseTariff.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.AIR, "AUSYD", "");
			var baseTariffLine = baseTariffEntry.AddRateLine("ODOC", FlatCalculator.Code);
			((FlatCalculator)baseTariffLine.Calculator).BaseRate = 15m;

			baseTariff.Factory.Save();

			var level2Tariff = new BusinessObjectFactory().New<CompanyTariff>();
			level2Tariff.Discounts.SetDiscount(RatingConstants.RateCategory.ORG, 10m);

			level2Tariff.Factory.Save();

			var clientRate = Helper.NewClientRate(Client2);
			var clientEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.AIR, "AUSYD", "");
			var rateLine = clientEntry.AddRateLine("ODOC", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			((CompanyTariffOrCostBasedCalculator)rateLine.Calculator).BaseRate = -5m;

			var result = new CompanyTariffOrCostLineCloneHelper(rateLine).GetClone();

			AssertEquals(8.5m, (ZDecimal)result.Calculator[Calculator.Items.Operator.BAS]);
		}

		public void TestGetCloneReturnsNullIfTariffHasBeenDeleted()
		{
			var baseTariff = new BusinessObjectFactory().New<CompanyTariff>();
			var baseTariffEntry = baseTariff.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.AIR, "AUSYD", "");
			var baseTariffLine = baseTariffEntry.AddRateLine("ODOC", FlatCalculator.Code);
			baseTariffLine.GetCalculator<FlatCalculator>().BaseRate = 15m;
			baseTariff.Factory.Save();

			var level2Factory = new BusinessObjectFactory();
			var level2Tariff = level2Factory.New<CompanyTariff>();
			level2Factory.Save();

			var level3Tariff = new BusinessObjectFactory().New<CompanyTariff>();
			var level3Entry = level3Tariff.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG)[0];
			var level3Line = level3Entry.AddRateLine("ODOC", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			level3Line.GetCalculator<CompanyTariffOrCostBasedCalculator>().BaseRate = 10m;
			level3Tariff.Factory.Save();

			var client = Helper.NewOrgHeader(3);
			var clientRate = Helper.NewClientRate(client);
			var clientEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.AIR, "AUSYD", "");
			var rateLine = clientEntry.AddRateLine("ODOC", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			rateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().BaseRate = -5m;

			var result = new CompanyTariffOrCostLineCloneHelper(rateLine).GetClone();

			AssertNotNull(result);
			AssertEquals("Should be able to use CTB calc on a CTB calc and accumulate the results", 20m, result.GetCalculator<FlatCalculator>().BaseRate);

			clientRate.TH_OH = Helper.NewOrgHeader(2).PK;
			level2Tariff.Delete();
			level2Factory.Save();

			result = new CompanyTariffOrCostLineCloneHelper(rateLine).GetClone();

			AssertNull("Cannot get CTB clone when tariff has been deleted", result);
		}

		public void TestGetCloneReturnsNullIfCostCannotBeFoundForCSTLineOnTariff()
		{
			var factory = new BusinessObjectFactory();
			var costing = Helper.NewCosting(null);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.AIR, "AUSYD", "");
			var costLine = costEntry.AddRateLine("ODOC", FlatCalculator.Code);
			costLine.GetCalculator<FlatCalculator>().BaseRate = 15m;

			var baseTariff = factory.New<CompanyTariff>();
			var baseTariffEntry = baseTariff.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.AIR, "AUSYD", "");
			var baseTariffLine = baseTariffEntry.AddRateLine("ODOC", CompanyTariffOrCostBasedCalculator.CostBasedCode);
			baseTariffLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().BaseRate = 10m;
			baseTariff.Factory.Save();

			var level2Factory = new BusinessObjectFactory();
			var level2Tariff = level2Factory.New<CompanyTariff>();
			level2Factory.Save();

			var level3Tariff = new BusinessObjectFactory().New<CompanyTariff>();
			var level3Entry = level3Tariff.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG)[0];
			var level3Line = level3Entry.AddRateLine("ODOC", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			level3Line.GetCalculator<CompanyTariffOrCostBasedCalculator>().BaseRate = 10m;
			level3Tariff.Factory.Save();

			level2Tariff.Delete();
			level2Factory.Save();

			var client = Helper.NewOrgHeader(3);
			var clientRate = Helper.NewClientRate(client);
			var clientEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.AIR, "AUSYD", "");
			var rateLine = clientEntry.AddRateLine("ODOC", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			rateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().BaseRate = 5m;

			var result = new CompanyTariffOrCostLineCloneHelper(rateLine).GetClone();

			AssertNull("No level 2 client exists, should not fall back to base company tariff", result);
		}

		public void TestGetCloneReturnsNothingIfNotMatchingCostingCanBeFound()
		{
			var baseTariff = new BusinessObjectFactory().New<CompanyTariff>();
			var baseTariffEntry = baseTariff.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.AIR, "AUSYD", "");
			var baseTariffLine = baseTariffEntry.AddRateLine("ODOC", CompanyTariffOrCostBasedCalculator.CostBasedCode);
			baseTariffLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().BaseRate = 10m;
			baseTariff.Factory.Save();

			var level2Factory = new BusinessObjectFactory();
			var level2Tariff = level2Factory.New<CompanyTariff>();
			level2Factory.Save();

			var level3Tariff = new BusinessObjectFactory().New<CompanyTariff>();
			var level3Entry = level3Tariff.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG)[0];
			var level3Line = level3Entry.AddRateLine("ODOC", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			level3Line.GetCalculator<CompanyTariffOrCostBasedCalculator>().BaseRate = 10m;
			level3Tariff.Factory.Save();

			level2Tariff.Delete();
			level2Factory.Save();

			var client = Helper.NewOrgHeader(3);
			var clientRate = Helper.NewClientRate(client);
			var clientEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.AIR, "AUSYD", "");
			var rateLine = clientEntry.AddRateLine("ODOC", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			rateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().BaseRate = 5m;

			var result = new CompanyTariffOrCostLineCloneHelper(rateLine).GetClone();

			AssertNull("Base Tariff uses CST calculator but no corresponding cost can be found, expect no result", result);
		}

		public void TestGetCloneForExpiredOrFutureRates()
		{
			var baseTariff = Helper.NewCompanyTariff();

			var companyTarrifEntry1 = baseTariff.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.AIR, "AUSYD", "");
			companyTarrifEntry1.TI_RateStartDate = ZDate.Today.AddMonths(-4);
			companyTarrifEntry1.TI_RateEndDate = ZDate.Today.AddMonths(-2).AddDays(-1);
			companyTarrifEntry1.AddRateLine("ODOC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 15m;

			var companyTarrifEntry2 = baseTariff.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.AIR, "AUSYD", "");
			companyTarrifEntry2.TI_RateStartDate = ZDate.Today.AddMonths(-2);
			companyTarrifEntry2.TI_RateEndDate = ZDate.Today.AddMonths(2);
			companyTarrifEntry2.AddRateLine("ODOC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 20m;

			var companyTarrifEntry3 = baseTariff.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.AIR, "AUSYD", "");
			companyTarrifEntry3.TI_RateStartDate = ZDate.Today.AddMonths(2).AddDays(1);
			companyTarrifEntry3.TI_RateEndDate = ZDate.Empty;
			companyTarrifEntry3.AddRateLine("ODOC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 25m;

			baseTariff.Factory.Save();
			Factory.Save();

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.AIR, "AUSYD", "");
			var rateLine = rateEntry.AddRateLine("ODOC", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			var clone = new CompanyTariffOrCostLineCloneHelper(rateLine).GetClone();

			AssertEquals(20m, clone.GetCalculator<FlatCalculator>().BaseRate);

			rateEntry.TI_RateStartDate = ZDate.Today.AddMonths(-3);
			rateEntry.TI_RateEndDate = ZDate.Today.AddMonths(-3).AddDays(10);
			clone = new CompanyTariffOrCostLineCloneHelper(rateLine).GetClone();

			AssertEquals(15m, clone.GetCalculator<FlatCalculator>().BaseRate);

			rateEntry.TI_RateStartDate = ZDate.Today.AddMonths(3);
			rateEntry.TI_RateEndDate = ZDate.Empty;
			clone = new CompanyTariffOrCostLineCloneHelper(rateLine).GetClone();

			AssertEquals(25m, clone.GetCalculator<FlatCalculator>().BaseRate);
		}

		public void TestGetCloneGetsMoreSpecificLocation()
		{
			var baseTariff = Helper.NewCompanyTariff();

			var tarrifEntryAustralia = baseTariff.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.AIR, "AU", "USLAX");
			tarrifEntryAustralia.AddRateLine("ODOC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 22m;

			var tarrifEntrySydney = baseTariff.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.AIR, "AUSYD", "USLAX");
			tarrifEntrySydney.AddRateLine("ODOC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 17m;

			baseTariff.Factory.Save();
			Factory.Save();

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.AIR, "AUSYD", "USLAX");
			var rateLine = rateEntry.AddRateLine("ODOC", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);

			var clone = new CompanyTariffOrCostLineCloneHelper(rateLine).GetClone();
			AssertEquals("Should be based on more specific location", 17m, clone.GetCalculator<FlatCalculator>().BaseRate);
		}

		public void TestCloneHasOwnFactory()
		{
			var baseTariff = Helper.NewCompanyTariff();
			var baseTarrifEntry = baseTariff.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.AIR, "AUSYD", "USLAX");
			baseTarrifEntry.AddRateLine("ODOC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 17m;

			baseTariff.Factory.Save();
			Factory.Save();

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.AIR, "AUSYD", "USLAX");
			var rateLine = rateEntry.AddRateLine("ODOC", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);

			var clone = new CompanyTariffOrCostLineCloneHelper(rateLine).GetClone();
			AssertNotEquals("New cloned line should have its' own Factory tp prevent it from saving", rateLine.Factory, clone.Factory);
		}

		public void TestCostingFromGenericCostingForRateLineViewResults()
		{
			void TestCase(bool viewResults)
			{
				var baseCosting = Factory.New<Costing>();
				var baseCostingEntry = baseCosting.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
				var aIRRateLine11 = baseCostingEntry.AddRateLine("FRT", FlatCalculator.Code);

				var carrier = Factory.NewWithValidTestData<OrgHeader>();
				var carrierCosting = Helper.NewCosting(carrier);
				var carrierEntry = carrierCosting.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
				var rateLine = carrierEntry.AddRateLine("FRT", CompanyTariffOrCostBasedCalculator.CostBasedCode);

				Factory.Save();

				rateLine.ViewResults = viewResults;
				var result = new CompanyTariffOrCostLineCloneHelper(rateLine).GetClone();
				AssertEquals("ViewResults should not change after Clone", rateLine.ViewResults, viewResults);
				AssertNotNull("Generic costing found", result);
				baseCosting.Delete();
			}

			TestCase(viewResults: true);
			TestCase(viewResults: false);
		}

		#endregion

		#region Implementation

		OrgHeader client2;

		OrgHeader Client2
		{
			get { return client2 ?? (client2 = Helper.NewOrgHeader(2)); }
		}

		protected TestHelper Helper
		{
			get { return helper ?? (helper = new TestHelper(Factory)); }
		}
		TestHelper helper;

		#endregion
	}
}
