using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Rating.Business.Testing
{
	class RateEntryAdapterTest : RatingTestCase
	{
		public void TestImportBroker()
		{
			var org = Helper.NewOrgHeader();
			var quote = Helper.NewQuote(org);
			var criteria = GetRateEntryAdapter(quote);
			AssertNull(criteria.ImportBroker);
		}

		public void TestExportBroker()
		{
			var org = Helper.NewOrgHeader();
			var quote = Helper.NewQuote(org);
			var criteria = GetRateEntryAdapter(quote);
			AssertNull(criteria.ExportBroker);
		}

		public void TestLocalClient()
		{
			var org = Helper.NewOrgHeader();
			var quote = Helper.NewQuote(org);
			var clientRate = Helper.NewClientRate(org);
			var companyTariff = Helper.NewCompanyTariff();
			var costing = Helper.NewCosting(org);

			CombineAssertions(() =>
			{
				AssertEquals(org, GetRateEntryAdapter(quote).DebtorOrgs[RatingDebtorOrgTypes.LC]);
				AssertEquals(org, GetRateEntryAdapter(clientRate).DebtorOrgs[RatingDebtorOrgTypes.LC]);
				AssertNull(GetRateEntryAdapter(companyTariff).DebtorOrgs[RatingDebtorOrgTypes.LC]);
				AssertNull("Costing Service Provider is not a Local Client", GetRateEntryAdapter(costing).DebtorOrgs[RatingDebtorOrgTypes.LC]);
			});
		}

		public void TestConsignor()
		{
			var consignor1 = Helper.NewOrgHeader();
			var consignor2 = Helper.NewOrgHeader();
			consignor1.OH_IsConsignor = true;
			consignor2.OH_IsConsignor = true;

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var companyTariff = Factory.NewWithValidTestData<CompanyTariff>();
			var costing = Helper.NewCosting(null);
			var quote = Helper.NewQuote(Helper.NewOrgHeader());

			CombineAssertions(() =>
			{
				AssertConsignor(clientRate, consignor1, consignor2);
				AssertConsignor(companyTariff, consignor1, consignor2);
				AssertConsignor(costing, consignor1, consignor2);
				AssertConsignor(quote, consignor1, consignor2);
			});
		}

		static void AssertConsignor(RatingHeader header, OrgHeader consignor1, OrgHeader consignor2)
		{
			var rateEntry = header.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.LCL, "AUSYD", "GBLHR");
			var rateEntryAdapter = new RateEntryAdapter(rateEntry);

			var message = rateEntry.ParentRatingHeader.DisplayInfo() + " CNR is empty by default";
			AssertNull(message, rateEntryAdapter.DebtorOrgs[RatingDebtorOrgTypes.CNR]);

			rateEntry.TI_OH_Consignor = consignor1.PK;
			rateEntryAdapter = new RateEntryAdapter(rateEntry);

			message = rateEntry.ParentRatingHeader.DisplayInfo() + " CNR should match TI_OH_Consignor";
			AssertEquals(message, consignor1, rateEntryAdapter.DebtorOrgs[RatingDebtorOrgTypes.CNR]);

			rateEntry.TI_OA_CartagePickupAddressOverride = consignor2.MainAddress.PK;
			rateEntryAdapter = new RateEntryAdapter(rateEntry);

			message = rateEntry.ParentRatingHeader.DisplayInfo() + " CNR should be the override TI_OH_Consignor (although validation shouldn't allow this)";
			AssertEquals(message, consignor2, rateEntryAdapter.DebtorOrgs[RatingDebtorOrgTypes.CNR]);
		}

		public void TestContractNumber()
		{
			var companyTariff = Factory.NewWithValidTestData<CompanyTariff>();
			var rateEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AUSYD", "GBLHR");
			var rateEntryAdapter = new RateEntryAdapter(rateEntry);

			AssertContainsExactElementsInAnyOrder("Precondition: Should be empty", [""], rateEntryAdapter.ClientContractNumbers);

			rateEntry.TI_ContractNumber = "123";
			rateEntryAdapter = new RateEntryAdapter(rateEntry);
			AssertContainsExactElementsInAnyOrder(["123"], rateEntryAdapter.ClientContractNumbers);
		}

		public void TestFMCTariffID()
		{
			var companyTariff = Factory.NewWithValidTestData<CompanyTariff>();
			var rateEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AUSYD", "GBLHR");
			var rateEntryAdapter = new RateEntryAdapter(rateEntry);

			AssertEquals("Precondition: Should be empty", ZString.Empty, rateEntryAdapter.FMCTariffID);

			rateEntry.TI_FMCTariffID = "123";
			rateEntryAdapter = new RateEntryAdapter(rateEntry);
			AssertEquals("123", rateEntryAdapter.FMCTariffID);
		}

		public void TestIsNonOperatedReefer()
		{
			var companyTariff = Factory.NewWithValidTestData<CompanyTariff>();
			var rateEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.FCL, "AUSYD", "GBLHR", container: "20FR");
			var rateEntryAdapter = new RateEntryAdapter(rateEntry);

			var measure = rateEntryAdapter.RateableMeasures as RateableMeasureSet;

			AssertEquals("Precondition: Should be false", false, measure.GetPartList(MeasureType.ContainerCount).GetDistinctContainerIsNonOperatingReefers().First());

			rateEntry.TI_IsNonOperatedReefer = "Y";
			rateEntryAdapter = new RateEntryAdapter(rateEntry);

			measure = rateEntryAdapter.RateableMeasures as RateableMeasureSet;
			AssertEquals(true, measure.GetPartList(MeasureType.ContainerCount).GetDistinctContainerIsNonOperatingReefers().First());
		}

		public void TestConsignee()
		{
			var consignee1 = Helper.NewOrgHeader();
			var consignee2 = Helper.NewOrgHeader();
			consignee1.OH_IsConsignee = true;
			consignee2.OH_IsConsignee = true;

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var companyTariff = Factory.NewWithValidTestData<CompanyTariff>();
			var costing = Helper.NewCosting(null);
			var quote = Helper.NewQuote(Helper.NewOrgHeader());

			CombineAssertions(() =>
			{
				AssertConsignee(clientRate, consignee1, consignee2);
				AssertConsignee(companyTariff, consignee1, consignee2);
				AssertConsignee(costing, consignee1, consignee2);
				AssertConsignee(quote, consignee1, consignee2);
			});
		}

		static void AssertConsignee(RatingHeader header, OrgHeader consignee1, OrgHeader consignee2)
		{
			var rateEntry = header.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.LCL, "AUSYD", "GBLHR");
			var rateEntryAdapter = new RateEntryAdapter(rateEntry);

			var message = rateEntry.ParentRatingHeader.DisplayInfo() + " CNE is empty by default";
			AssertNull(message, rateEntryAdapter.DebtorOrgs[RatingDebtorOrgTypes.CNE]);

			rateEntry.TI_OH_Consignee = consignee1.PK;
			rateEntryAdapter = new RateEntryAdapter(rateEntry);

			message = rateEntry.ParentRatingHeader.DisplayInfo() + " CNE should match TI_OH_Consignee";
			AssertEquals(message, consignee1, rateEntryAdapter.DebtorOrgs[RatingDebtorOrgTypes.CNE]);

			rateEntry.TI_OA_CartageDeliveryAddressOverride = consignee2.MainAddress.PK;
			rateEntryAdapter = new RateEntryAdapter(rateEntry);

			message = rateEntry.ParentRatingHeader.DisplayInfo() + " CNE should be the override TI_OH_Consignee (although validation shouldn't allow this)";
			AssertEquals(message, consignee2, rateEntryAdapter.DebtorOrgs[RatingDebtorOrgTypes.CNE]);
		}

		public void TestControllingCustomer()
		{
			var controllingCustomer = Helper.NewOrgHeader();
			controllingCustomer.OH_IsControllingCustomer = true;

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var companyTariff = Factory.NewWithValidTestData<CompanyTariff>();
			var costing = Helper.NewCosting(null);
			var quote = Helper.NewQuote(Helper.NewOrgHeader());

			CombineAssertions(() =>
			{
				AssertControllingCustomer(clientRate, controllingCustomer);
				AssertControllingCustomer(companyTariff, controllingCustomer);
				AssertControllingCustomer(costing, controllingCustomer);
				AssertControllingCustomer(quote, controllingCustomer);
			});
		}

		public void TestPlannedDischarge()
		{
			var companyTariff = Factory.NewWithValidTestData<CompanyTariff>();

			var rateEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LCL, "AUSYD", "GBLHR");
			rateEntry.TI_PlannedDischargeLRC = "AUMEL";

			var rateEntryAdapter = new RateEntryAdapter(rateEntry);
			AssertEquals("AUMEL", rateEntryAdapter.PlannedDischarge(Enterprise.Integration.Accounting.CostSell.Revenue).Code);
		}

		public void TestPlannedLoad()
		{
			var companyTariff = Factory.NewWithValidTestData<CompanyTariff>();

			var rateEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LCL, "AUSYD", "GBLHR");
			rateEntry.TI_PlannedLoadLRC = "AUMEL";

			var rateEntryAdapter = new RateEntryAdapter(rateEntry);
			AssertEquals("AUMEL", rateEntryAdapter.PlannedLoad(Enterprise.Integration.Accounting.CostSell.Revenue).Code);
		}

		public void TestRateOrigin()
		{
			var companyTariff = Factory.NewWithValidTestData<CompanyTariff>();

			var rateEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LCL, "AUSYD", "GBLHR");
			rateEntry.TI_RateOrigin = "AUMEL";

			var rateEntryAdapter = new RateEntryAdapter(rateEntry);
			AssertEquals("AUMEL", rateEntryAdapter.RateOrigin.Code);
		}

		public void TestRateDestination()
		{
			var companyTariff = Factory.NewWithValidTestData<CompanyTariff>();

			var rateEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LCL, "AUSYD", "GBLHR");
			rateEntry.TI_RateDestination = "AUMEL";

			var rateEntryAdapter = new RateEntryAdapter(rateEntry);
			AssertEquals("AUMEL", rateEntryAdapter.RateDestination.Code);
		}

		void AssertControllingCustomer(RatingHeader header, OrgHeader controllingCustomer)
		{
			var rateEntry = header.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.LCL, "AUSYD", "GBLHR");
			var rateEntryAdapter = new RateEntryAdapter(rateEntry);

			var message = rateEntry.ParentRatingHeader.DisplayInfo() + " CCUS is empty by default";
			AssertNull(message, rateEntryAdapter.DebtorOrgs[RatingDebtorOrgTypes.CCUS]);

			rateEntry.TI_OH_ControllingCustomer = controllingCustomer.PK;
			rateEntryAdapter = new RateEntryAdapter(rateEntry);

			message = rateEntry.ParentRatingHeader.DisplayInfo() + " CCUS should match TI_OH_ControllingCustomer";
			AssertEquals(message, controllingCustomer, rateEntryAdapter.DebtorOrgs[RatingDebtorOrgTypes.CCUS]);
		}

		public void TestIAutoRatingWarehouseInfo()
		{
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS);
			entry.TI_WW_Warehouse = warehouse.PK;

			var adapter = new RateEntryAdapter(entry);
			AssertEquals(warehouse.PK, adapter.WarehousePK);
			AssertEquals(null, ((IAutoRatingWarehouseInfo)adapter).WarehouseFallbackConsignorForFilterOnly);
		}

		public void TestMeasures()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "GBLHR", container: "20GP", commodity: "GEN");
			var adapter = new RateEntryAdapter(entry);

			var measures = (RateableMeasureSet)adapter.RateableMeasures;
			var containerAttribs = measures.GetContainerTypeAndCommodityList().Single();
			AssertEquals(entry.TI_RH_NKCommodityCode, containerAttribs.CommodityCode);
			AssertEquals(entry.TI_RC, containerAttribs.ContainerTypePk);
			AssertEquals(1, containerAttribs.ContainerCount);
			AssertEquals(1m, measures.GetActual(MeasureType.ContainerCount));
			AssertEquals(0m, measures.GetActual(MeasureType.Weight));
			AssertEquals(Core.Constants.Weight.Kilograms, measures.GetUnit(MeasureType.Weight));
			AssertEquals(0m, measures.GetActual(MeasureType.Volume));
			AssertEquals(Core.Constants.Volume.CubicMetres, measures.GetUnit(MeasureType.Volume));
		}

		#region Implementation

		static RateEntryAdapter GetRateEntryAdapter(RatingHeader header)
		{
			var rateEntry = header.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LCL, "AUSYD", "GBLHR");

			return new RateEntryAdapter(rateEntry);
		}

		#endregion
	}
}
