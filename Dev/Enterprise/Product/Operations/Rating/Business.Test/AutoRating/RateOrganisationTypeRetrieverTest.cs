using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using static Enterprise.Rating.Business.RatingConstants;

namespace Enterprise.Rating.Business.Testing
{
	public class RateOrganisationTypeRetrieverTest : TestCaseWithFactory
	{
		public void TestGetEntrySourceTypeFromClient_Forwarding() => TestGetEntrySourceTypeFromClient(RateCategory.AIR, Core.Constants.RateMode.LSE);

		public void TestGetEntrySourceTypeFromClient_Customs() => TestGetEntrySourceTypeFromClient(RateCategory.CAI, Core.Constants.RateMode.LSE);

		void TestGetEntrySourceTypeFromClient(string rateCategory, string rateMode)
		{
			var paymentTerms = new PaymentTermInfos();
			var helper = new TestHelper(Factory);

			var client = helper.NewOrgHeader();
			var consignee = helper.NewOrgHeader();
			var consignor = helper.NewOrgHeader();
			var agent = helper.NewOrgHeader();
			var controllingCustomer = helper.NewOrgHeader();

			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, client);
			criteria.JobDirection = Directions.Export;
			criteria.Consignee = consignee;
			criteria.Consignor = consignor;
			criteria.OverseasAgent = agent;
			criteria.DebtorOrgs[RatingDebtorOrgTypes.CCUS] = controllingCustomer;

			var clientRateEntry = helper.NewClientRate(client).AddRateEntry(rateCategory, rateMode, "AU", "US");
			var consigneeRateEntry = helper.NewClientRate(consignee).AddRateEntry(rateCategory, rateMode, "AU", "US");
			var consignorRateEntry = helper.NewClientRate(consignor).AddRateEntry(rateCategory, rateMode, "AU", "US");
			var agentRateEntry = helper.NewClientRate(agent).AddRateEntry(rateCategory, rateMode, "AU", "US");
			var controllingCustomerRateEntry = helper.NewClientRate(controllingCustomer).AddRateEntry(rateCategory, rateMode, "AU", "US");
			var clientCostEntry = helper.NewCosting(client).AddRateEntry(rateCategory, rateMode, "AU", "US");
			var clientQuoteEntry = helper.NewQuote(client).AddRateEntry(rateCategory, rateMode, "AU", "US");
			var tariffEntry1 = Factory.New<CompanyTariff>().AddRateEntry(rateCategory, rateMode, "AU", "US");
			var tariffEntry2 = Factory.New<CompanyTariff>().AddRateEntry(rateCategory, rateMode, "AU", "US");
			var tariffEntry3 = Factory.New<CompanyTariff>().AddRateEntry(rateCategory, rateMode, "AU", "US");

			var collection = RatingDataRegistry.Instance.ExportPrepaidPriorities.Value;
			var ccus_priority = collection.AddNew();
			ccus_priority.OrganizationType = nameof(RatingDebtorOrgTypes.CCUS);

			RatingDataRegistry.Instance.ExportPrepaidPriorities.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			Factory.Save();
			var ranker = new IncotermsRanker();

			AssertContainsExactElementsInAnyOrder(new[] { RatingDebtorOrgTypes.LC }, ranker.Rank(criteria, clientRateEntry.AddRateLine("FRT", FlatCalculator.Code)).OrgTypesSellRatePriorityRegistryFiltered);
			AssertContainsExactElementsInAnyOrder(Array.Empty<RatingDebtorOrgTypes>(), ranker.Rank(criteria, consigneeRateEntry.AddRateLine("FRT", FlatCalculator.Code)).OrgTypesSellRatePriorityRegistryFiltered);
			AssertContainsExactElementsInAnyOrder(new[] { RatingDebtorOrgTypes.CNR }, ranker.Rank(criteria, consignorRateEntry.AddRateLine("FRT", FlatCalculator.Code)).OrgTypesSellRatePriorityRegistryFiltered);
			AssertContainsExactElementsInAnyOrder(new[] { RatingDebtorOrgTypes.CCUS }, ranker.Rank(criteria, controllingCustomerRateEntry.AddRateLine("FRT", FlatCalculator.Code)).OrgTypesSellRatePriorityRegistryFiltered);

			paymentTerms.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "EXW"));
			criteria.PaymentTerm = paymentTerms;
			criteria.ClearCache();
			AssertContainsExactElementsInAnyOrder(new[] { RatingDebtorOrgTypes.CNE }, ranker.Rank(criteria, consigneeRateEntry.AddRateLine("FRT", FlatCalculator.Code)).OrgTypesSellRatePriorityRegistryFiltered);
			AssertContainsExactElementsInAnyOrder(new[] { RatingDebtorOrgTypes.AG }, ranker.Rank(criteria, agentRateEntry.AddRateLine("FRT", FlatCalculator.Code)).OrgTypesSellRatePriorityRegistryFiltered);

			paymentTerms.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "DDP"));
			criteria.ClearCache();
			AssertContainsExactElementsInAnyOrder(Array.Empty<RatingDebtorOrgTypes>(), ranker.Rank(criteria, clientCostEntry.AddRateLine("FRT", FlatCalculator.Code)).OrgTypesSellRatePriorityRegistryFiltered);
			AssertContainsExactElementsInAnyOrder(new[] { RatingDebtorOrgTypes.LC }, ranker.Rank(criteria, clientQuoteEntry.AddRateLine("FRT", FlatCalculator.Code)).OrgTypesSellRatePriorityRegistryFiltered);
			AssertContainsExactElementsInAnyOrder(Array.Empty<RatingDebtorOrgTypes>(), ranker.Rank(criteria, tariffEntry1.AddRateLine("FRT", FlatCalculator.Code)).OrgTypesSellRatePriorityRegistryFiltered);

			client.CompanyData.RateTariffLevels.SetLevel("FRT", 1);
			consignor.CompanyData.RateTariffLevels.SetLevel("FRT", "EXP", "ALL", 3);
			consignee.CompanyData.RateTariffLevels.SetLevel("FRT", "IMP", "ALL", 2);
			agent.CompanyData.RateTariffLevels.SetLevel("FRT", "EXP", "LCL", 3);
			criteria.ClearCache();
			AssertContainsExactElementsInAnyOrder(new[] { RatingDebtorOrgTypes.LC }, ranker.Rank(criteria, tariffEntry1.AddRateLine("FRT", FlatCalculator.Code)).OrgTypesSellRatePriorityRegistryFiltered);
			AssertContainsExactElementsInAnyOrder(Array.Empty<RatingDebtorOrgTypes>(), ranker.Rank(criteria, tariffEntry2.AddRateLine("FRT", FlatCalculator.Code)).OrgTypesSellRatePriorityRegistryFiltered);
			AssertContainsExactElementsInAnyOrder(new[] { RatingDebtorOrgTypes.CNR }, ranker.Rank(criteria, tariffEntry3.AddRateLine("FRT", FlatCalculator.Code)).OrgTypesSellRatePriorityRegistryFiltered);

			consignee.CompanyData.RateTariffLevels.SetLevel("FRT", "EXP", "ALL", 2);
			agent.CompanyData.RateTariffLevels.SetLevel("FRT", "EXP", "LSE", 3);
			criteria.ClearCache();
			AssertContainsExactElementsInAnyOrder(new[] { RatingDebtorOrgTypes.CNR }, ranker.Rank(criteria, tariffEntry3.AddRateLine("FRT", FlatCalculator.Code)).OrgTypesSellRatePriorityRegistryFiltered);

			paymentTerms.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "EXW"));
			criteria.ClearCache();
			AssertContainsExactElementsInAnyOrder(new[] { RatingDebtorOrgTypes.CNE }, ranker.Rank(criteria, tariffEntry2.AddRateLine("FRT", FlatCalculator.Code)).OrgTypesSellRatePriorityRegistryFiltered);

			paymentTerms.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "DDP"));
			criteria.ImportBroker = client;
			criteria.ClearCache();
			AssertContainsExactElementsInAnyOrder(new[] { RatingDebtorOrgTypes.LCBK }, ranker.Rank(criteria, clientRateEntry.AddRateLine("FRT", FlatCalculator.Code)).OrgTypesSellRatePriorityRegistryFiltered);
		}
	}
}
