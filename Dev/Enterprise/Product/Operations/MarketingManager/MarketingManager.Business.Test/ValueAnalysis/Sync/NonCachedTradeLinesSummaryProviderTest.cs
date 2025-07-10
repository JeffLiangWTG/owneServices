using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class NonCachedTradeLinesSummaryProviderTest : TestCaseWithFactory
	{
		[TestDate(2019, 03, 18)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestGetRawCollection_MainOrgFilter()
		{
			var chargeCode = TradeLinesSummaryProviderCommonTest.GetNonDSBChargeCode(Factory);

			var shp = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var nzakl = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL");

			var company1 = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "EDI");
			var company2 = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "SIN");

			var periodTestHelper = new AccountingPeriodTestHelper(Factory);
			periodTestHelper.SetupSinglePeriod(201903, ZDate.Today.AddYears(-6), ZDate.Today.AddYears(1), company1.PK);
			periodTestHelper.SetupSinglePeriod(201903, ZDate.Today.AddYears(-6), ZDate.Today.AddYears(1), company2.PK);

			var localClient1 = Factory.NewWithValidTestData<OrgHeader>();
			localClient1.OH_Code = "LOCALORG1";
			localClient1.OH_RL_NKClosestPort = "AUBNE";

			var localClient2 = Factory.NewWithValidTestData<OrgHeader>();
			localClient2.OH_Code = "LOCALORG2";
			localClient2.OH_RL_NKClosestPort = "NZAKL";

			var overseasAgent = Factory.NewWithValidTestData<OrgHeader>();
			overseasAgent.OH_Code = "OVEAGT";
			overseasAgent.OH_RL_NKClosestPort = "NZAKL";

			var orgSalesCollection = new OrgSalesCollection(localClient1);
			var collectionHelper = new OrgSalesCollectionTestHelper(orgSalesCollection, Factory);

			var consignor = collectionHelper.GetNewOrg("TESTORG1");
			var consignee = collectionHelper.GetNewOrg("TESTORG2");

			var shipment1 = collectionHelper.CreateShipment("AUSYD", "NZAKL", "SEA", "FCL", consignee.PK, consignor.PK, new ZDate(2017, 10, 10));
			var job11 = TradeLinesSummaryProviderCommonTest.CreateJobHeader((IJobHeaderParent)shipment1, localClient1, company1.PK);
			TradeLinesSummaryProviderCommonTest.CreateCharge(Factory, job11, localClient1, chargeCode, TransactionLineTypes.Revenue);
			var job12 = TradeLinesSummaryProviderCommonTest.CreateJobHeader((IJobHeaderParent)shipment1, localClient2, company2.PK);
			TradeLinesSummaryProviderCommonTest.CreateCharge(Factory, job12, localClient2, chargeCode, TransactionLineTypes.Revenue);
			job11.JH_A_JCL = ZDate.Today;
			job12.JH_A_JCL = ZDate.Today;

			var shipment2 = collectionHelper.CreateShipment("AUSYD", "NZAKL", "SEA", "FCL", consignee.PK, consignor.PK, new ZDate(2017, 10, 18));
			var job2 = TradeLinesSummaryProviderCommonTest.CreateJobHeader((IJobHeaderParent)shipment2, localClient1, overseasAgent, company1.PK);
			TradeLinesSummaryProviderCommonTest.CreateCharge(Factory, job2, localClient1, chargeCode, TransactionLineTypes.Revenue);
			TradeLinesSummaryProviderCommonTest.CreateCharge(Factory, job2, overseasAgent, chargeCode, TransactionLineTypes.Revenue);
			job2.JH_A_JCL = ZDate.Today;

			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var provider = new NonCachedTradeLinesSummaryProviderForTest(Db.Connection))
			{
				var syncRange = new TradeLinesSynchronizationRange(new ZDate(2017, 10, 1), new ZDate(2017, 11, 1));

				var tradeLinesCollection = provider.GetRawCollection_Exposed(consignor, syncRange);
				AssertEquals(true, tradeLinesCollection.Cast<DynamicBusinessObject>().All(x => (ZGuid)x["MainOrg"] == consignor.PK));

				tradeLinesCollection = provider.GetRawCollection_Exposed(consignee, syncRange);
				AssertEquals(true, tradeLinesCollection.Cast<DynamicBusinessObject>().All(x => (ZGuid)x["MainOrg"] == consignee.PK));

				tradeLinesCollection = provider.GetRawCollection_Exposed(localClient1, syncRange);
				AssertEquals(true, tradeLinesCollection.Cast<DynamicBusinessObject>().All(x => (ZGuid)x["MainOrg"] == localClient1.PK));

				tradeLinesCollection = provider.GetRawCollection_Exposed(localClient2, syncRange);
				AssertEquals(true, tradeLinesCollection.Cast<DynamicBusinessObject>().All(x => (ZGuid)x["MainOrg"] == localClient2.PK));

				tradeLinesCollection = provider.GetRawCollection_Exposed(overseasAgent, syncRange);
				AssertEquals(true, tradeLinesCollection.Cast<DynamicBusinessObject>().All(x => (ZGuid)x["MainOrg"] == overseasAgent.PK));
			}
		}

		class NonCachedTradeLinesSummaryProviderForTest : NonCachedTradeLinesSummaryProvider
		{
			public NonCachedTradeLinesSummaryProviderForTest(DbConnection connection)
				: base(connection)
			{
			}

			public DynamicBusinessObjectCollection GetRawCollection_Exposed(IOrgHeader org, TradeLinesSynchronizationRange syncRange)
			{
				return GetRawCollection(org, syncRange);
			}
		}
	}
}
