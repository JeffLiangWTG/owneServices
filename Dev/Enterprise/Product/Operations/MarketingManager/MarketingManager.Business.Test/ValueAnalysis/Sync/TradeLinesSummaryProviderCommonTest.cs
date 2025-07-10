using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class TradeLinesSummaryProviderCommonTest : TestCaseWithFactory
	{
		#region Forwarding Shipment

		[TestDate(2019, 03, 18)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestForwarding()
		{
			var chargeCode = GetNonDSBChargeCode(Factory);

			var shp = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var nzakl = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL");

			var company1 = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "EDI");
			var companyBranch1 = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, company1.PK));

			var company2 = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "SIN");
			var companyBranch2 = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, company2.PK));

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

			var collection = new OrgSalesCollection(localClient1);
			var collectionHelper = new OrgSalesCollectionTestHelper(collection, Factory);

			var consignor = collectionHelper.GetNewOrg("TESTORG1");
			var consignee = collectionHelper.GetNewOrg("TESTORG2");

			var shipment1 = collectionHelper.CreateShipment("AUSYD", "NZAKL", "SEA", "FCL", consignee.PK, consignor.PK, new ZDate(2017, 10, 10));
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), companyBranch1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var job11 = CreateJobHeader((IJobHeaderParent)shipment1, localClient1, company1.PK);
				CreateCharge(Factory, job11, localClient1, chargeCode, TransactionLineTypes.Revenue);
				job11.JH_A_JCL = ZDate.Today;
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), companyBranch2.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var job12 = CreateJobHeader((IJobHeaderParent)shipment1, localClient2, company2.PK);
				CreateCharge(Factory, job12, localClient2, chargeCode, TransactionLineTypes.Revenue);
				job12.JH_A_JCL = ZDate.Today;
			}

			var shipment2 = collectionHelper.CreateShipment("AUSYD", "NZAKL", "SEA", "FCL", consignee.PK, consignor.PK, new ZDate(2017, 10, 18));
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), companyBranch1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var job2 = CreateJobHeader((IJobHeaderParent)shipment2, localClient1, overseasAgent, company1.PK);
				CreateCharge(Factory, job2, localClient1, chargeCode, TransactionLineTypes.Revenue);
				CreateCharge(Factory, job2, overseasAgent, chargeCode, TransactionLineTypes.Revenue);
				job2.JH_A_JCL = ZDate.Today;
			}

			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var provider = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var period = new ZDate(2017, 10, 1);

				// Consignor
				var consignorSummary = provider.GetForOrg(consignor, new ZDate(2017, 10, 1), new ZDate(2017, 11, 1));

				var consignorActual = consignorSummary.ActualValues;
				AssertEquals(1, consignorActual.Count);

				var consignorTradeLane = consignorActual.First();
				AssertEquals(shp.PK, consignorTradeLane.Key.ProductPk);
				AssertEquals(ausyd.PK, consignorTradeLane.Key.OriginPk);
				AssertEquals(nzakl.PK, consignorTradeLane.Key.DestinationPk);
				AssertEquals(consignor.PK, consignorTradeLane.Key.Supplier);
				AssertEquals(consignee.PK, consignorTradeLane.Key.Buyer);
				AssertEquals(1, consignorTradeLane.Value.TradeDetails.Count);

				var consignorTradeDetail = consignorTradeLane.Value.TradeDetails.First();
				AssertEquals("SEA", consignorTradeDetail.Key.Mode);
				AssertEquals("FCL", consignorTradeDetail.Key.Type);
				AssertEquals(2, consignorTradeDetail.Value.TradePeriods.Count);

				AssertEquals(true, consignorTradeDetail.Value.TradePeriods.All(x => x.Key.Period == period));

				var consignorJobPeriod = consignorTradeDetail.Value.TradePeriods[new TradePeriodKey(period, consignor.PK, true)];
				AssertEquals(2, consignorJobPeriod.NumberOfJobs);
				AssertEquals(new ZDate(2017, 10, 18), consignorJobPeriod.PeriodLastTrade);
				AssertEquals(2, consignorJobPeriod.TradeValues.Count);
				var consignorJobCompany1Value = consignorJobPeriod.TradeValues[new TradeValueBreakdownKey("AUD", company1.PK)];
				AssertEquals(300m, consignorJobCompany1Value.Revenue);
				AssertEquals(0m, consignorJobCompany1Value.Cost);
				var consignorJobCompany2Value = consignorJobPeriod.TradeValues[new TradeValueBreakdownKey("SGD", company2.PK)];
				AssertEquals(100m, consignorJobCompany2Value.Revenue);
				AssertEquals(0m, consignorJobCompany2Value.Cost);

				var consignorPeriod = consignorTradeDetail.Value.TradePeriods[new TradePeriodKey(period, consignor.PK, false)];
				AssertEquals(2, consignorPeriod.NumberOfJobs);
				AssertEquals(new ZDate(2017, 10, 18), consignorPeriod.PeriodLastTrade);
				AssertEquals(2, consignorPeriod.TradeValues.Count);
				var consignorCompany1Value = consignorPeriod.TradeValues[new TradeValueBreakdownKey("AUD", company1.PK)];
				AssertEquals(0m, consignorCompany1Value.Revenue);
				AssertEquals(0m, consignorCompany1Value.Cost);
				var consignorCompany2Value = consignorPeriod.TradeValues[new TradeValueBreakdownKey("SGD", company2.PK)];
				AssertEquals(0m, consignorCompany2Value.Revenue);
				AssertEquals(0m, consignorCompany2Value.Cost);

				// Consignee
				var consigneeSummary = provider.GetForOrg(consignee, new ZDate(2017, 10, 1), new ZDate(2017, 11, 1));

				var consigneeActual = consigneeSummary.ActualValues;
				AssertEquals(1, consigneeActual.Count);
				var consigneeTradeLane = consigneeActual.First();
				var consigneeTradeDetail = consigneeTradeLane.Value.TradeDetails.First();
				AssertEquals(2, consigneeTradeDetail.Value.TradePeriods.Count);
				AssertEquals(true, consigneeTradeDetail.Value.TradePeriods.All(x => x.Key.Period == period));

				var consigneeJobPeriod = consigneeTradeDetail.Value.TradePeriods[new TradePeriodKey(period, consignee.PK, true)];
				AssertEquals(2, consigneeJobPeriod.NumberOfJobs);
				AssertEquals(new ZDate(2017, 10, 18), consigneeJobPeriod.PeriodLastTrade);
				AssertEquals(2, consigneeJobPeriod.TradeValues.Count);
				var consigneeJobCompany1Value = consigneeJobPeriod.TradeValues[new TradeValueBreakdownKey("AUD", company1.PK)];
				AssertEquals(300m, consigneeJobCompany1Value.Revenue);
				AssertEquals(0m, consigneeJobCompany1Value.Cost);
				var consigneeJobCompany2Value = consigneeJobPeriod.TradeValues[new TradeValueBreakdownKey("SGD", company2.PK)];
				AssertEquals(100m, consigneeJobCompany2Value.Revenue);
				AssertEquals(0m, consigneeJobCompany2Value.Cost);

				var consigneePeriod = consigneeTradeDetail.Value.TradePeriods[new TradePeriodKey(period, consignee.PK, false)];
				AssertEquals(2, consigneePeriod.NumberOfJobs);
				AssertEquals(new ZDate(2017, 10, 18), consigneePeriod.PeriodLastTrade);
				AssertEquals(2, consigneePeriod.TradeValues.Count);
				var consigneeCompany1Value = consigneePeriod.TradeValues[new TradeValueBreakdownKey("AUD", company1.PK)];
				AssertEquals(0m, consigneeCompany1Value.Revenue);
				AssertEquals(0m, consigneeCompany1Value.Cost);
				var consigneeCompany2Value = consigneePeriod.TradeValues[new TradeValueBreakdownKey("SGD", company2.PK)];
				AssertEquals(0m, consigneeCompany2Value.Revenue);
				AssertEquals(0m, consigneeCompany2Value.Cost);

				// Local client 1
				var localClient1Summary = provider.GetForOrg(localClient1, new ZDate(2017, 10, 1), new ZDate(2017, 11, 1));

				var localClient1Actual = localClient1Summary.ActualValues;
				AssertEquals(1, localClient1Actual.Count);
				var localClient1TradeLane = localClient1Actual.First();
				var localClient1TradeDetail = localClient1TradeLane.Value.TradeDetails.First();
				AssertEquals(2, localClient1TradeDetail.Value.TradePeriods.Count);
				AssertEquals(true, localClient1TradeDetail.Value.TradePeriods.All(x => x.Key.Period == period));

				var localClient1Period = localClient1TradeDetail.Value.TradePeriods[new TradePeriodKey(period, localClient1.PK, false)];
				AssertEquals(2, localClient1Period.NumberOfJobs);
				AssertEquals(new ZDate(2017, 10, 18), localClient1Period.PeriodLastTrade);
				AssertEquals(1, localClient1Period.TradeValues.Count);
				var localClient1Company1Value = localClient1Period.TradeValues[new TradeValueBreakdownKey("AUD", company1.PK)];
				AssertEquals(200m, localClient1Company1Value.Revenue);
				AssertEquals(0m, localClient1Company1Value.Cost);

				var localClient1JobPeriod = localClient1TradeDetail.Value.TradePeriods[new TradePeriodKey(period, localClient1.PK, true)];
				AssertEquals(2, localClient1JobPeriod.NumberOfJobs);
				AssertEquals(new ZDate(2017, 10, 18), localClient1JobPeriod.PeriodLastTrade);
				AssertEquals(1, localClient1JobPeriod.TradeValues.Count);
				var localClient1JobCompany1Value = localClient1JobPeriod.TradeValues[new TradeValueBreakdownKey("AUD", company1.PK)];
				AssertEquals(300m, localClient1JobCompany1Value.Revenue);
				AssertEquals(0m, localClient1JobCompany1Value.Cost);

				// Local client 2
				var localClient2Summary = provider.GetForOrg(localClient2, new ZDate(2017, 10, 1), new ZDate(2017, 11, 1));

				var localClient2Actual = localClient2Summary.ActualValues;
				AssertEquals(1, localClient2Actual.Count);
				var localClient2TradeLane = localClient2Actual.First();
				var localClient2TradeDetail = localClient2TradeLane.Value.TradeDetails.First();
				AssertEquals(2, localClient2TradeDetail.Value.TradePeriods.Count);
				AssertEquals(true, localClient2TradeDetail.Value.TradePeriods.All(x => x.Key.Period == period));

				var localClient2Period = localClient2TradeDetail.Value.TradePeriods[new TradePeriodKey(period, localClient2.PK, false)];
				AssertEquals(1, localClient2Period.NumberOfJobs);
				AssertEquals(new ZDate(2017, 10, 10), localClient2Period.PeriodLastTrade);
				AssertEquals(1, localClient2Period.TradeValues.Count);
				var localClient2Company2Value = localClient2Period.TradeValues[new TradeValueBreakdownKey("SGD", company2.PK)];
				AssertEquals(100m, localClient2Company2Value.Revenue);
				AssertEquals(0m, localClient2Company2Value.Cost);

				var localClient2JobPeriod = localClient2TradeDetail.Value.TradePeriods[new TradePeriodKey(period, localClient2.PK, true)];
				AssertEquals(1, localClient2JobPeriod.NumberOfJobs);
				AssertEquals(new ZDate(2017, 10, 10), localClient2JobPeriod.PeriodLastTrade);
				AssertEquals(1, localClient2JobPeriod.TradeValues.Count);
				var localClient2JobCompany2Value = localClient2JobPeriod.TradeValues[new TradeValueBreakdownKey("SGD", company2.PK)];
				AssertEquals(100m, localClient2JobCompany2Value.Revenue);
				AssertEquals(0m, localClient2JobCompany2Value.Cost);

				// Overseas agent
				var overseasAgentSummary = provider.GetForOrg(overseasAgent, new ZDate(2017, 10, 1), new ZDate(2017, 11, 1));

				var overseasAgentActual = overseasAgentSummary.ActualValues;
				AssertEquals(1, overseasAgentActual.Count);
				var overseasAgentTradeLane = overseasAgentActual.First();
				var overseasAgentTradeDetail = overseasAgentTradeLane.Value.TradeDetails.First();
				AssertEquals(2, overseasAgentTradeDetail.Value.TradePeriods.Count);
				AssertEquals(true, overseasAgentTradeDetail.Value.TradePeriods.All(x => x.Key.Period == period));

				var overseasAgentPeriod = overseasAgentTradeDetail.Value.TradePeriods[new TradePeriodKey(period, overseasAgent.PK, false)];
				AssertEquals(1, overseasAgentPeriod.NumberOfJobs);
				AssertEquals(new ZDate(2017, 10, 18), overseasAgentPeriod.PeriodLastTrade);
				AssertEquals(1, overseasAgentPeriod.TradeValues.Count);
				var overseasAgentCompany1Value = overseasAgentPeriod.TradeValues[new TradeValueBreakdownKey("AUD", company1.PK)];
				AssertEquals(100m, overseasAgentCompany1Value.Revenue);
				AssertEquals(0m, overseasAgentCompany1Value.Cost);

				var overseasAgentJobPeriod = overseasAgentTradeDetail.Value.TradePeriods[new TradePeriodKey(period, overseasAgent.PK, true)];
				AssertEquals(1, overseasAgentJobPeriod.NumberOfJobs);
				AssertEquals(new ZDate(2017, 10, 18), overseasAgentJobPeriod.PeriodLastTrade);
				AssertEquals(1, overseasAgentJobPeriod.TradeValues.Count);
				var overseasAgentJobCompany1Value = overseasAgentJobPeriod.TradeValues[new TradeValueBreakdownKey("AUD", company1.PK)];
				AssertEquals(200m, overseasAgentJobCompany1Value.Revenue);
				AssertEquals(0m, overseasAgentJobCompany1Value.Cost);
			}
		}

		public void TestOnlyIncludeValidShipments()
		{
			var insertSql = @"
DECLARE @Org UNIQUEIDENTIFIER = 'D6016AA8-5C87-4791-9F1C-7337E770211D'
DECLARE @Address UNIQUEIDENTIFIER = '15CF9D78-0C34-41E2-A730-BE7164D901B1';
DECLARE @Company UNIQUEIDENTIFIER = (SELECT TOP 1 GC_PK FROM dbo.GlbCompany)
DECLARE @Branch UNIQUEIDENTIFIER = (SELECT TOP 1 GB_PK FROM dbo.GlbBranch)
DECLARE @Department UNIQUEIDENTIFIER = (SELECT TOP 1 GE_PK FROM dbo.GlbDepartment)

INSERT INTO dbo.OrgHeader
	(OH_PK, OH_Code)
VALUES
	(@Org, 'XXXX')

INSERT INTO dbo.OrgAddress
	(OA_PK, OA_OH, OA_Address1)
VALUES
	(@Address, @Org, '111 St')

INSERT INTO dbo.JobShipment
	(JS_PK, JS_UniqueConsignRef, JS_IsShipping, JS_PackingMode, JS_IsCancelled, JS_IsForwardRegistered, JS_RL_NKOrigin, JS_RL_NKDestination, JS_SystemCreateTimeUtc)
VALUES
	-- Forwarding
	('8F3ACA3C-4CBE-445D-8390-56B0E128F8F1', 'JS00001001', 0, 'AIR', 0, 1, 'AUSYD', 'AUSYD', '2015-1-1'),
	('8F3ACA3C-4CBE-445D-8390-56B0E128F8F3', 'JS00001003', 0, 'AIR', 1, 1, 'AUSYD', 'AUSYD', '2015-1-1'),
	('8F3ACA3C-4CBE-445D-8390-56B0E128F8F5', 'JS00001005', 0, 'LCL', 0, 0, 'AUSYD', 'AUSYD', '2015-1-1'),
	-- Liner and agency
	('8F3ACA3C-4CBE-445D-8390-56B0E128F8F2', 'JS00001002', 1, 'FCL', 0, 1, 'AUSYD', 'AUSYD', '2015-1-1'),
	('8F3ACA3C-4CBE-445D-8390-56B0E128F8F4', 'JS00001004', 1, 'FCL', 1, 1, 'AUSYD', 'AUSYD', '2015-1-1'),
	('8F3ACA3C-4CBE-445D-8390-56B0E128F8F6', 'JS00001006', 1, 'LCL', 0, 0, 'AUSYD', 'AUSYD', '2015-1-1')

INSERT INTO dbo.JobHeader
	(JH_PK, JH_ParentID, JH_ParentTableCode, JH_JobNum, JH_GC, JH_GB, JH_GE, JH_OA_LocalChargesAddr, JH_Status)
VALUES
	(NEWID(), '8F3ACA3C-4CBE-445D-8390-56B0E128F8F1', 'JS', 'JS00001001', @Company, @Branch, @Department, @Address, 'WRK'),
	(NEWID(), '8F3ACA3C-4CBE-445D-8390-56B0E128F8F2', 'JS', 'JS00001002', @Company, @Branch, @Department, @Address, 'WRK'),
	(NEWID(), '8F3ACA3C-4CBE-445D-8390-56B0E128F8F3', 'JS', 'JS00001003', @Company, @Branch, @Department, @Address, 'WRK'),
	(NEWID(), '8F3ACA3C-4CBE-445D-8390-56B0E128F8F4', 'JS', 'JS00001004', @Company, @Branch, @Department, @Address, 'WRK')

INSERT INTO dbo.JobDocAddress
	(E2_PK, E2_ParentID, E2_ParentTableCode, E2_OA_Address, E2_AddressType)
VALUES
	(NEWID(), '8F3ACA3C-4CBE-445D-8390-56B0E128F8F1', 'Z0', @Address, 'CRD'),
	(NEWID(), '8F3ACA3C-4CBE-445D-8390-56B0E128F8F2', 'Z0', @Address, 'CRD'),
	(NEWID(), '8F3ACA3C-4CBE-445D-8390-56B0E128F8F3', 'Z0', @Address, 'CRD'),
	(NEWID(), '8F3ACA3C-4CBE-445D-8390-56B0E128F8F4', 'Z0', @Address, 'CRD'),
	(NEWID(), '8F3ACA3C-4CBE-445D-8390-56B0E128F8F5', 'Z0', @Address, 'CRD'),
	(NEWID(), '8F3ACA3C-4CBE-445D-8390-56B0E128F8F6', 'Z0', @Address, 'CRD')
";

			TestConnection.ExecuteNonQuery(insertSql);
			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(TestConnection);

			using (var provider = new NonCachedTradeLinesSummaryProvider(TestConnection))
			{
				var shpPk = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP").PK;
				var lgyPk = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "LGY").PK;

				var org = Factory.Load<OrgHeader>(Guid.Parse("D6016AA8-5C87-4791-9F1C-7337E770211D"));
				var summary = provider.GetForOrg(org, new ZDate(2014, 6, 1), new ZDate(2015, 6, 1));

				var actualForwardingTradeLane = summary.ActualValues.Single(x => x.Key.ProductPk == shpPk).Value;

				var actualAir = actualForwardingTradeLane.TradeDetails.Single(x => x.Key.Type == "AIR").Value;
				var actualAirPeriod = actualAir.TradePeriods.Single(x => x.Key.IsJobValue).Value;
				AssertEquals(1, actualAirPeriod.NumberOfJobs);
				var hasActualLcl = actualForwardingTradeLane.TradeDetails.Any(x => x.Key.Type == "LCL");
				AssertEquals(false, hasActualLcl);

				var actualLinerAgencyTradeLane = summary.ActualValues.Single(x => x.Key.ProductPk == lgyPk).Value;
				var actualSco = actualLinerAgencyTradeLane.TradeDetails.Single(x => x.Key.Type == "FCL").Value;
				var actualScoPeriod = actualSco.TradePeriods.Single(x => x.Key.IsJobValue).Value;
				AssertEquals(1, actualScoPeriod.NumberOfJobs);

				var actualNco = actualLinerAgencyTradeLane.TradeDetails.Single(x => x.Key.Type == "LCL").Value;
				var actualNcoPeriod = actualNco.TradePeriods.Single(x => x.Key.IsJobValue).Value;
				AssertEquals(1, actualNcoPeriod.NumberOfJobs);
			}
		}

		#endregion

		#region Port Transport

		public void TestPortTransport()
		{
			var client1PK = ZGuid.NewZGuid();
			var client2PK = ZGuid.NewZGuid();

			var address1Client1Pk = ZGuid.NewZGuid();
			var address2Client1Pk = ZGuid.NewZGuid();
			var addressClient2Pk = ZGuid.NewZGuid();

			var transport1PK = ZGuid.NewZGuid();
			var transport2PK = ZGuid.NewZGuid();
			var transport3PK = ZGuid.NewZGuid();

			var insertSql = $@"
DECLARE @Company UNIQUEIDENTIFIER = (SELECT TOP 1 GC_PK FROM dbo.GlbCompany)
DECLARE @Branch UNIQUEIDENTIFIER = (SELECT TOP 1 GB_PK FROM dbo.GlbBranch)
DECLARE @Department UNIQUEIDENTIFIER = (SELECT TOP 1 GE_PK FROM dbo.GlbDepartment)

DECLARE @Client1 UNIQUEIDENTIFIER = '{client1PK}'
DECLARE @Client2 UNIQUEIDENTIFIER = '{client2PK}'

DECLARE @OrgAddress1Client1 UNIQUEIDENTIFIER = '{address1Client1Pk}'
DECLARE @OrgAddress2Client1 UNIQUEIDENTIFIER = '{address2Client1Pk}'
DECLARE @OrgAddressClient2 UNIQUEIDENTIFIER = '{addressClient2Pk}'

DECLARE @ChargeCode UNIQUEIDENTIFIER = (SELECT TOP 1 AC_PK FROM dbo.AccChargeCode WHERE AC_ChargeType != 'DSB')
DECLARE @AccGLHeader UNIQUEIDENTIFIER = (SELECT TOP 1 AG_PK FROM dbo.AccGLHeader)

INSERT INTO dbo.OrgHeader
	(OH_PK, OH_Code)
VALUES
	(@Client1, 'XXXX1'),
	(@Client2, 'XXXX2')

INSERT INTO dbo.OrgAddress
	(OA_PK, OA_OH, OA_Code, OA_Address1, OA_RL_NKRelatedPortCode)
VALUES
	(@OrgAddress1Client1, @Client1, '1ST', '1st Street', 'AUSYD'),
	(@OrgAddress2Client1, @Client1, '2ND', '2nd Street', 'US2CW'),
	(@OrgAddressClient2,  @Client2, '3RD', '3rd Street', 'FRPAR')

DECLARE @Transport1 UNIQUEIDENTIFIER = '{transport1PK}'
DECLARE @Transport2 UNIQUEIDENTIFIER = '{transport2PK}'
DECLARE @Transport3 UNIQUEIDENTIFIER = '{transport3PK}'

INSERT INTO dbo.JobCartage
	(JJ_PK, JJ_ConsignmentID, JJ_E3_NKJobType, JJ_EstimatedPickup, JJ_ContainerMode, JJ_Weight, JJ_WeightUQ, JJ_Volume, JJ_VolumeUQ, JJ_GB, JJ_SystemCreateTimeUtc, JJ_SystemCreateUser, JJ_SystemLastEditTimeUtc, JJ_SystemLastEditUser)
VALUES
	(@Transport1, 'T00012345', 'ESFF', '2019-08-19 09:00:00', 'CNT', 1.000, 'KG', 9.000, 'M3', @Branch, '2019-08-18 08:00:00', 'A', '2019-08-18 08:00:00', 'A'),
	(@Transport2, 'T00012346', 'ESFF', '2019-08-20 10:00:00', 'CNT', 20.000, 'KG', 80.000, 'M3', @Branch, '2019-08-18 08:00:00', 'A', '2019-08-18 08:00:00', 'A'),
	(@Transport3, 'T00012347', 'ESFF', '2019-08-21 11:00:00', 'CNT', 300.000, 'KG', 700.000, 'M3', @Branch, '2019-08-18 08:00:00', 'A', '2019-08-18 08:00:00', 'A')

INSERT INTO dbo.JobDocAddress
	(E2_PK, E2_ParentID, E2_ParentTableCode, E2_AddressType, E2_AddressOverride, E2_ValidationStatus, E2_OA_Address, E2_RN_NKCountryCode, E2_AddressSequence)
VALUES
	(NEWID(), @Transport1, 'JJ', 'LCT', 0, 'NRQ', @OrgAddress1Client1, '', 0),
	(NEWID(), @Transport1, 'JJ', 'LCT', 0, 'NRQ', @OrgAddress2Client1, '', 1),
	(NEWID(), @Transport2, 'JJ', 'LCT', 0, 'NRQ', @OrgAddressClient2, '', 0),
	(NEWID(), @Transport3, 'JJ', 'LCT', 0, 'NRQ', @OrgAddress1Client1, '', 0)

DECLARE @JobHeader1PK UNIQUEIDENTIFIER = NewID()
DECLARE @JobHeader2PK UNIQUEIDENTIFIER = NewID()
DECLARE @JobHeader3PK UNIQUEIDENTIFIER = NewID()

INSERT INTO dbo.JobHeader
	(JH_PK,         JH_ParentID, JH_ParentTableCode, JH_JobNum,   JH_GC,    JH_GB,   JH_GE,      JH_OA_LocalChargesAddr, JH_Status, JH_A_JCL)
VALUES
	(@JobHeader1PK, @Transport1, 'JJ', 'T00012345', @Company, @Branch, @Department, @OrgAddress1Client1, 'WRK', '2019-08-18'),
	(@JobHeader2PK, @Transport2, 'JJ', 'T00012346', @Company, @Branch, @Department, @OrgAddressClient2,  'WRK', '2019-08-18'),
	(@JobHeader3PK, @Transport3, 'JJ', 'T00012347', @Company, @Branch, @Department, @OrgAddress1Client1, 'WRK', '2019-08-18')

DECLARE @TransactionHeader1PK UNIQUEIDENTIFIER = NewID()
DECLARE @TransactionHeader2PK UNIQUEIDENTIFIER = NewID()
DECLARE @TransactionHeader3PK UNIQUEIDENTIFIER = NewID()

INSERT INTO dbo.AccTransactionHeader
	(AH_PK, AH_TransactionNum, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_Ledger, AH_TransactionType)
VALUES
	(@transactionHeader1PK, 'INV00001001', @Company, @Branch, @Department, '2019-08-22', 'AR', 'INV'),
	(@transactionHeader2PK, 'INV00001002', @Company, @Branch, @Department, '2019-08-22', 'AR', 'INV'),
	(@transactionHeader3PK, 'INV00001003', @Company, @Branch, @Department, '2019-08-22', 'AR', 'INV')

DECLARE @TransactionLine1PK UNIQUEIDENTIFIER = NewID()
DECLARE @TransactionLine2PK UNIQUEIDENTIFIER = NewID()
DECLARE @TransactionLine3PK UNIQUEIDENTIFIER = NewID()
DECLARE @TransactionLine4PK UNIQUEIDENTIFIER = NewID()
DECLARE @TransactionLine5PK UNIQUEIDENTIFIER = NewID()

INSERT INTO dbo.AccTransactionLines
	(AL_PK, AL_AH, AL_JH, AL_AC, AL_LineType, AL_LineAmount, AL_OH, AL_GC, AL_GB, AL_GE, AL_AG, AL_ReverseDate)
VALUES
	(@TransactionLine1PK, @TransactionHeader1PK, @JobHeader1PK, @ChargeCode, 'REV', 1,     @Client1, @Company, @Branch, @Department, @AccGLHeader, '2019-08-22'),
	(@TransactionLine2PK, @TransactionHeader1PK, @JobHeader1PK, @ChargeCode, 'REV', 10,    @Client1, @Company, @Branch, @Department, @AccGLHeader, '2019-08-22'),
	(@TransactionLine3PK, @TransactionHeader2PK, @JobHeader2PK, @ChargeCode, 'REV', 100,   @Client2, @Company, @Branch, @Department, @AccGLHeader, '2019-08-22'),
	(@TransactionLine4PK, @TransactionHeader2PK, @JobHeader2PK, @ChargeCode, 'REV', 1000,  @Client2, @Company, @Branch, @Department, @AccGLHeader, '2019-08-22'),
	(@TransactionLine5PK, @TransactionHeader3PK, @JobHeader3PK, @ChargeCode, 'REV', 10000, @Client2, @Company, @Branch, @Department, @AccGLHeader, '2019-08-22')

DECLARE @JobCharge1PK UNIQUEIDENTIFIER = NewID()
DECLARE @JobCharge2PK UNIQUEIDENTIFIER = NewID()
DECLARE @JobCharge3PK UNIQUEIDENTIFIER = NewID()
DECLARE @JobCharge4PK UNIQUEIDENTIFIER = NewID()
DECLARE @JobCharge5PK UNIQUEIDENTIFIER = NewID()

INSERT INTO dbo.JobCharge (JR_PK, JR_JH, JR_AL_ARLine, JR_AC, JR_GB, JR_GC, JR_GE, JR_SystemLastEditTimeUtc, JR_SystemLastEditUser)
VALUES 
	(@JobCharge1PK, @JobHeader1PK, @TransactionLine1PK, @ChargeCode, @Branch, @Company, @Department, GETUTCDATE(), '~BP'),
	(@JobCharge2PK, @JobHeader1PK, @TransactionLine2PK, @ChargeCode, @Branch, @Company, @Department, GETUTCDATE(), '~BP'),
	(@JobCharge3PK, @JobHeader2PK, @TransactionLine3PK, @ChargeCode, @Branch, @Company, @Department, GETUTCDATE(), '~BP'),
	(@JobCharge4PK, @JobHeader2PK, @transactionLine4PK, @ChargeCode, @Branch, @Company, @Department, GETUTCDATE(), '~BP'),
	(@JobCharge5PK, @JobHeader3PK, @transactionLine5PK, @ChargeCode, @Branch, @Company, @Department, GETUTCDATE(), '~BP')

INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced, AM_GC_Company)
VALUES
	(NEWID(), 201908, 2019, '1999-03-18', '2020-03-18', 0, 0, 0, 0, @Company)
";

			TestConnection.ExecuteNonQuery(insertSql);
			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(TestConnection);

			using (var provider = new NonCachedTradeLinesSummaryProvider(TestConnection))
			{
				var transport = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Transport);

				CombineAssertions(() =>
				{
					var client1 = Factory.Load<OrgHeader>(client1PK);
					var summaryClient1 = provider.GetForOrg(client1, new ZDate(2018, 1, 1), new ZDate(2020, 1, 1));

					var client1TradeLanes = summaryClient1.ActualValues;
					AssertEquals("There should be a single Trade Lane", 1, client1TradeLanes.Count);

					var client1TradeLane = client1TradeLanes.First(x => x.Key.Buyer == client1.PK);
					AssertEquals("Unexpected Trade Lane's Main Org", client1.PK, client1TradeLane.Key.MainOrg);
					AssertEquals("Unexpected Trade Lane's Buyer", client1.PK, client1TradeLane.Key.Buyer);
					AssertEquals("Unexpected Trade Lane's ProductPk", transport.PK, client1TradeLane.Key.ProductPk);

					var client1TradeDetails = client1TradeLane.Value.TradeDetails;
					AssertEquals("There should be a single Trade Detail", 1, client1TradeDetails.Count);

					var client1TradeDetail = client1TradeDetails.First();
					AssertEquals("Unexpected Trade Detail's Mode", "PTR", client1TradeDetail.Key.Mode);
					AssertEquals("Unexpected Trade Detail's Type", "CNT", client1TradeDetail.Key.Type);

					var client1TradePeriod = client1TradeDetail.Value.TradePeriods.Single(x => x.Key.IsJobValue);
					AssertEquals("Unexpected Trade Period's OrgPK", client1PK, client1TradePeriod.Key.OrgPk);
					AssertEquals("Unexpected Trade Period's Period", new ZDate(2019, 8, 1), client1TradePeriod.Key.Period);
					AssertEquals("Unexpected Trade Period's NumberOfJobs", 2, client1TradePeriod.Value.NumberOfJobs);
					AssertEquals("Unexpected Trade Period's Volume", 709m, client1TradePeriod.Value.Volume);
					AssertEquals("Unexpected Trade Period's Weight", 0.301m, client1TradePeriod.Value.Weight);
					AssertEquals("Unexpected Trade Period's PeriodLastTrade", new ZDateTime(2019, 8, 21, 11, 0, 0), client1TradePeriod.Value.PeriodLastTrade);

					var client1TradeValues = client1TradePeriod.Value.TradeValues;
					AssertEquals("There should be a single Trade Value", 1, client1TradeValues.Count);

					var client1TradeValue = client1TradeValues.First();
					AssertEquals("Unexpected Value revenue ", 10_011m, client1TradeValue.Value.Revenue);
				});

				CombineAssertions(() =>
				{
					var client2 = Factory.Load<OrgHeader>(client2PK);
					var summaryClient2 = provider.GetForOrg(client2, new ZDate(2018, 1, 1), new ZDate(2020, 1, 1));

					var client2TradeLanes = summaryClient2.ActualValues;
					AssertEquals("There should be a single Trade Lane", 1, client2TradeLanes.Count);

					var client2TradeLane = client2TradeLanes.First(x => x.Key.Buyer == client2.PK);
					AssertEquals("Unexpected Trade Lane's Main Org", client2.PK, client2TradeLane.Key.MainOrg);
					AssertEquals("Unexpected Trade Lane's Buyer", client2.PK, client2TradeLane.Key.Buyer);
					AssertEquals("Unexpected Trade Lane's ProductPk", transport.PK, client2TradeLane.Key.ProductPk);

					var client2TradeDetails = client2TradeLane.Value.TradeDetails;
					AssertEquals("There should be a single Trade Detail", 1, client2TradeDetails.Count);

					var client2TradeDetail = client2TradeDetails.First();
					AssertEquals("Unexpected Trade Detail's Mode", "PTR", client2TradeDetail.Key.Mode);
					AssertEquals("Unexpected Trade Detail's Type", "CNT", client2TradeDetail.Key.Type);

					var client2TradePeriod = client2TradeDetail.Value.TradePeriods.Single(x => x.Key.IsJobValue);
					AssertEquals("Unexpected Trade Period's OrgPK", client2PK, client2TradePeriod.Key.OrgPk);
					AssertEquals("Unexpected Trade Period's Period", new ZDate(2019, 8, 1), client2TradePeriod.Key.Period);
					AssertEquals("Unexpected Trade Period's NumberOfJobs", 1, client2TradePeriod.Value.NumberOfJobs);
					AssertEquals("Unexpected Trade Period's Volume", 80m, client2TradePeriod.Value.Volume);
					AssertEquals("Unexpected Trade Period's Weight", 0.02m, client2TradePeriod.Value.Weight);
					AssertEquals("Unexpected Trade Period's PeriodLastTrade", new ZDateTime(2019, 8, 20, 10, 0, 0), client2TradePeriod.Value.PeriodLastTrade);

					var client2TradeValues = client2TradePeriod.Value.TradeValues;
					AssertEquals("There should be a single Trade Value", 1, client2TradeValues.Count);

					var client2TradeValue = client2TradeValues.First();
					AssertEquals("Unexpected Value revenue ", 1_100m, client2TradeValue.Value.Revenue);
				});
			}
		}

		#endregion

		#region Warehouse

		public void TestWarehouse()
		{
			var org1Pk = ZGuid.NewZGuid();
			var org2Pk = ZGuid.NewZGuid();
			var adddress1Pk = ZGuid.NewZGuid();
			var adddress2Pk = ZGuid.NewZGuid();

			var warehouse1Pk = ZGuid.NewZGuid();
			var warehouse2Pk = ZGuid.NewZGuid();
			var partAPk = ZGuid.NewZGuid();
			var partBPk = ZGuid.NewZGuid();
			var partCPk = ZGuid.NewZGuid();

			var order1Pk = ZGuid.NewZGuid();
			var order2Pk = ZGuid.NewZGuid();
			var receive1Pk = ZGuid.NewZGuid();
			var receive2Pk = ZGuid.NewZGuid();

			var insertSql = $@"
ALTER TABLE dbo.WhsDocket DROP CONSTRAINT IF EXISTS Constraint_WD_WP

DECLARE @Company UNIQUEIDENTIFIER = (SELECT TOP 1 GC_PK FROM dbo.GlbCompany)
DECLARE @Branch UNIQUEIDENTIFIER = (SELECT TOP 1 GB_PK FROM dbo.GlbBranch)
DECLARE @Department UNIQUEIDENTIFIER = (SELECT TOP 1 GE_PK FROM dbo.GlbDepartment)

DECLARE @Org1 UNIQUEIDENTIFIER = '{org1Pk}'
DECLARE @Org2 UNIQUEIDENTIFIER = '{org2Pk}'

DECLARE @Org1Address UNIQUEIDENTIFIER = '{adddress1Pk}'
DECLARE @Org2Address UNIQUEIDENTIFIER = '{adddress2Pk}'

DECLARE @Warehouse1 UNIQUEIDENTIFIER = '{warehouse1Pk}'
DECLARE @Warehouse2 UNIQUEIDENTIFIER = '{warehouse2Pk}'
DECLARE @SupplierPartA UNIQUEIDENTIFIER = '{partAPk}'
DECLARE @SupplierPartB UNIQUEIDENTIFIER = '{partBPk}'
DECLARE @SupplierPartC UNIQUEIDENTIFIER = '{partCPk}'

DECLARE @LocationType UNIQUEIDENTIFIER = (SELECT TOP 1 WLT_PK FROM dbo.WhsLocationType WHERE WLT_Code = 'RNO')
DECLARE @ChargeCode UNIQUEIDENTIFIER = (SELECT TOP 1 AC_PK FROM dbo.AccChargeCode WHERE AC_ChargeType != 'DSB')
DECLARE @AccGLHeader UNIQUEIDENTIFIER = (SELECT TOP 1 AG_PK FROM dbo.AccGLHeader)

INSERT INTO dbo.OrgHeader
	(OH_PK, OH_Code)
VALUES
	(@Org1, 'XXXX1'),
	(@Org2, 'XXXX2')

INSERT INTO dbo.OrgAddress
	(OA_PK, OA_OH, OA_Code, OA_Address1, OA_RL_NKRelatedPortCode)
VALUES
	(@Org1Address, @Org1, '1ST', '1st Street', 'AUSYD'),
	(@Org2Address, @Org2, '2ND', '2nd Street', 'US2CW')

INSERT INTO dbo.WhsWarehouse
	(WW_PK, WW_WarehouseCode, WW_GB_RelatedCompanyBranch, WW_OA_WarehouseAddress, WW_WarehouseType, WW_DefaultOutboundDockDoor, WW_DefaultInboundDockDoor, WW_WLT_DefaultLocationType, WW_SystemCreateTimeUtc, WW_SystemCreateUser, WW_SystemLastEditTimeUtc, WW_SystemLastEditUser)
VALUES
	(@Warehouse1, 'WH1', @Branch, @Org1Address, 'TRW', NULL, NULL, @LocationType, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(@Warehouse2, 'WH2', @Branch, @Org2Address, 'PRW', NEWID(), NEWID(), @LocationType, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.OrgSupplierPart
	(OP_PK, OP_PartNum)
VALUES
	(@SupplierPartA, 'AAAA'),
	(@SupplierPartB, 'BBBB'),
	(@SupplierPartC, 'AAAA')

DECLARE @Order1 UNIQUEIDENTIFIER = '{order1Pk}'
DECLARE @Order2 UNIQUEIDENTIFIER = '{order2Pk}'
DECLARE @Receive1 UNIQUEIDENTIFIER = '{receive1Pk}'
DECLARE @Receive2 UNIQUEIDENTIFIER = '{receive2Pk}'
INSERT INTO dbo.WhsDocket
	(WD_PK, WD_OH_Client, WD_DocketID, WD_ExternalReference, WD_WW_Whs, WD_DocketType, WD_DocketSubType, WD_TotalPallets, WD_PalletsSent, WD_TotalUnits, WD_TotalWeight, WD_TotalWeightUnit, WD_TotalCubic, WD_TotalCubicUnit, WD_BookingDate, WD_DocketStatus, WD_FinalisedDate, WD_GS_NKFinalizedBy, WD_UnloadCompletedTime, WD_SystemCreateTimeUtc, WD_ArrivalDate, WD_SystemCreateUser, WD_SystemLastEditTimeUtc, WD_SystemLastEditUser)
VALUES
	(@Order1,   @Org1,    'W00010001', 'W00010001',          @Warehouse1,       'ORD',       'ORD',               2,             10,             5,          20000,               'HG',         80000,              'D3',     '2002-1-1',           'FIN',       '2002-1-1', '~BP'              , NULL      , '2002-1-1', NULL, '~BP', GetUtcDate(), '~BP'),
	(@Order2,   @Org1,    'W00010002', 'W00010002',          @Warehouse1,       'ORD',       'ORD',               1,              5,             4,            100,               'KG',            50,              'M3',     '2002-1-1',           'FIN',       '2002-1-1', '~BP'              , NULL      , '2002-1-1', NULL, '~BP', GetUtcDate(), '~BP'),
	(@Receive1, @Org1,    'W00010003', 'W00010003',          @Warehouse2,       'INW',       'REC',               2,             10,             4,           5000,               'HG',         40000,              'D3',     '2002-1-1',           'FIN',       '2002-1-1', '~BP'              , '2002-1-1', '2002-1-1', '2002-1-1', '~BP', GetUtcDate(), '~BP'),
	(@Receive2, @Org1,    'W00010004', 'W00010004',          @Warehouse2,       'INW',       'REC',               1,              5,             1,             10,               'KG',            10,              'M3',     '2002-1-1',           'FIN',       '2002-1-1', '~BP'              , '2002-1-1', '2002-1-1', '2002-1-1', '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.JobDocAddress
	(E2_PK, E2_ParentID, E2_ParentTableCode, E2_AddressType, E2_AddressOverride, E2_ValidationStatus, E2_OA_Address, E2_RN_NKCountryCode)
VALUES
	(NEWID(), @Order1, 'WD', 'CEA', 0, 'NRQ', @Org2Address, ''),
	(NEWID(), @Order2, 'WD', 'CEA', 1, 'NYV', NULL, 'GB'),

	(NEWID(), @Receive1, 'WD', 'SUD', 0, 'NRQ', @Org2Address, ''),
	(NEWID(), @Receive2, 'WD', 'SUD', 1, 'NYV', NULL, 'FR')

DECLARE @DocketLine1PK UNIQUEIDENTIFIER = NewID()
DECLARE @DocketLine2PK UNIQUEIDENTIFIER = NewID()
DECLARE @DocketLine3PK UNIQUEIDENTIFIER = NewID()
DECLARE @DocketLine4PK UNIQUEIDENTIFIER = NewID()

INSERT INTO dbo.WhsDocketLine
	(WE_PK, WE_WD, WE_OP, WE_DocketLineStatus, WE_FinalisedDate, WE_DocketLineType, WE_F3_NKPackType, WE_AdjustmentArrivalDate, WE_OriginalInventoryStatus, WE_CurrentInventoryStatus, WE_WE_OriginalDocketLineForRating, WE_SystemCreateTimeUtc, WE_SystemCreateUser, WE_SystemLastEditTimeUtc, WE_SystemLastEditUser)
VALUES
	(@DocketLine1PK, @Receive1, @SupplierPartA, 'FIN', '2002-1-1', 'INW', 'UNT', '2002-1-1', 'AVL', 'AVL', @DocketLine1PK, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(@DocketLine2PK, @Receive1, @SupplierPartA, 'FIN', '2002-1-1', 'INW', 'UNT', '2002-1-1', 'AVL', 'AVL', @DocketLine2PK, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(@DocketLine3PK, @Receive1, @SupplierPartB, 'FIN', '2002-1-1', 'INW', 'UNT', '2002-1-1', 'AVL', 'AVL', @DocketLine3PK, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(@DocketLine4PK, @Receive2, @SupplierPartA, 'FIN', '2002-1-1', 'INW', 'UNT', '2002-1-1', 'AVL', 'AVL', @DocketLine4PK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

DECLARE @JobHeader1PK UNIQUEIDENTIFIER = NewID()
DECLARE @JobHeader2PK UNIQUEIDENTIFIER = NewID()
DECLARE @JobHeader3PK UNIQUEIDENTIFIER = NewID()
DECLARE @JobHeader4PK UNIQUEIDENTIFIER = NewID()

INSERT INTO dbo.JobHeader
	(JH_PK, JH_ParentID, JH_ParentTableCode, JH_JobNum, JH_GC, JH_GB, JH_GE, JH_OA_LocalChargesAddr, JH_Status, JH_A_JCL)
VALUES
	(@JobHeader1PK, @Order1, 'WD', 'WD00001001', @Company, @Branch, @Department, @Org1Address, 'WRK', '2019-03-18'),
	(@JobHeader2PK, @Order2, 'WD', 'WD00001002', @Company, @Branch, @Department, @Org1Address, 'WRK', '2019-03-18'),
	(@JobHeader3PK, @Receive1, 'WD', 'WD00001003', @Company, @Branch, @Department, @Org1Address, 'WRK', '2019-03-18'),
	(@JobHeader4PK, @Receive2, 'WD', 'WD00001004', @Company, @Branch, @Department, @Org1Address, 'WRK', '2019-03-18')

DECLARE @TransactionHeader1PK UNIQUEIDENTIFIER = NewID()
DECLARE @TransactionHeader2PK UNIQUEIDENTIFIER = NewID()
DECLARE @TransactionHeader3PK UNIQUEIDENTIFIER = NewID()
DECLARE @TransactionHeader4PK UNIQUEIDENTIFIER = NewID()

INSERT INTO dbo.AccTransactionHeader
	(AH_PK, AH_TransactionNum, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_Ledger, AH_TransactionType)
VALUES
	(@transactionHeader1PK, 'INV00001001', @Company, @Branch, @Department, '2000-1-1', 'AR', 'INV'),
	(@transactionHeader2PK, 'INV00001002', @Company, @Branch, @Department, '2000-1-1', 'AR', 'INV'),
	(@transactionHeader3PK, 'INV00001003', @Company, @Branch, @Department, '2000-1-1', 'AR', 'INV'),
	(@transactionHeader4PK, 'INV00001004', @Company, @Branch, @Department, '2000-1-1', 'AR', 'INV')

DECLARE @TransactionLine1PK UNIQUEIDENTIFIER = NewID()
DECLARE @TransactionLine2PK UNIQUEIDENTIFIER = NewID()
DECLARE @TransactionLine3PK UNIQUEIDENTIFIER = NewID()
DECLARE @transactionLine4PK UNIQUEIDENTIFIER = NewID()

INSERT INTO dbo.AccTransactionLines
	(AL_PK, AL_AH, AL_JH, AL_AC, AL_LineType, AL_LineAmount, AL_OH, AL_GC, AL_GB, AL_GE, AL_AG, AL_ReverseDate)
VALUES
	(@TransactionLine1PK, @TransactionHeader1PK, @JobHeader1PK, @ChargeCode, 'REV', 100, @Org1, @Company, @Branch, @Department, @AccGLHeader, '2002-1-1'),
	(@TransactionLine2PK, @TransactionHeader2PK, @JobHeader2PK, @ChargeCode, 'REV', 100, @Org1, @Company, @Branch, @Department, @AccGLHeader, '2002-1-1'),
	(@TransactionLine3PK, @TransactionHeader3PK, @JobHeader3PK, @ChargeCode, 'REV', 100, @Org1, @Company, @Branch, @Department, @AccGLHeader, '2002-1-1'),
	(@TransactionLine4PK, @TransactionHeader4PK, @JobHeader4PK, @ChargeCode, 'REV', 100, @Org1, @Company, @Branch, @Department, @AccGLHeader, '2002-1-1')

DECLARE @JobCharge1PK UNIQUEIDENTIFIER = NewID()
DECLARE @JobCharge2PK UNIQUEIDENTIFIER = NewID()
DECLARE @JobCharge3PK UNIQUEIDENTIFIER = NewID()
DECLARE @JobCharge4PK UNIQUEIDENTIFIER = NewID()

INSERT INTO dbo.JobCharge (JR_PK, JR_JH, JR_AL_ARLine, JR_AC, JR_GB, JR_GC, JR_GE, JR_SystemLastEditTimeUtc, JR_SystemLastEditUser)
VALUES 
	(@JobCharge1PK, @JobHeader1PK, @TransactionLine1PK, @ChargeCode, @Branch, @Company, @Department, GETUTCDATE(), '~BP'),
	(@JobCharge2PK, @JobHeader2PK, @TransactionLine2PK, @ChargeCode, @Branch, @Company, @Department, GETUTCDATE(), '~BP'),
	(@JobCharge3PK, @JobHeader3PK, @TransactionLine3PK, @ChargeCode, @Branch, @Company, @Department, GETUTCDATE(), '~BP'),
	(@JobCharge4PK, @JobHeader4PK, @transactionLine4PK, @ChargeCode, @Branch, @Company, @Department, GETUTCDATE(), '~BP')

INSERT INTO dbo.JobChargeAttrib (EC_PK, EC_JR, EC_NAME, EC_VALUE) 
VALUES
	(NEWID(), @JobCharge3PK, 'PRD', 'AAAA'),
	(NEWID(), @JobCharge3PK, 'PRD', 'BBBB'),
	(NEWID(), @JobCharge4PK, 'PRD', 'AAAA')

INSERT INTO dbo.OrgPartRelation (OU_PK, OU_OH, OU_OP, OU_Relationship, OU_SystemCreateTimeUtc, OU_SystemCreateUser, OU_SystemLastEditTimeUtc, OU_SystemLastEditUser) 
VALUES 
	(NEWID(), @Org1, @SupplierPartA, 'BTH', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(NEWID(), @Org1, @SupplierPartB, 'BTH', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(NEWID(), @Org1, @SupplierPartC, 'SUP', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced, AM_GC_Company)
VALUES
	(NEWID(), 201903, 2019, '1999-03-18', '2020-03-18', 0, 0, 0, 0, @Company)
";

			TestConnection.ExecuteNonQuery(insertSql);
			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(TestConnection);

			using (var provider = new NonCachedTradeLinesSummaryProvider(TestConnection))
			{
				var whs = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse).PK;
				var org1 = Factory.Load<OrgHeader>(org1Pk);
				var org2 = Factory.Load<OrgHeader>(org2Pk);
				var summary = provider.GetForOrg(org1, new ZDate(2000, 6, 1), new ZDate(2015, 6, 1));

				var receiveTradeLane1 = summary.ActualValues.Single(x => x.Key.Service == "REC" && x.Key.Supplier == org2.PK);
				AssertEquals("Main Org", org1.PK, receiveTradeLane1.Key.MainOrg);
				AssertEquals("Supplier", org2.PK, receiveTradeLane1.Key.Supplier);
				AssertEquals("Buyer", org1.PK, receiveTradeLane1.Key.Buyer);
				AssertEquals("WarehousePk", warehouse2Pk, receiveTradeLane1.Key.WarehousePk);
				AssertEquals("ProductPk", whs, receiveTradeLane1.Key.ProductPk);
				AssertEquals("Service", "REC", receiveTradeLane1.Key.Service);
				{
					var noPartsDetail = receiveTradeLane1.Value.TradeDetails.Single(x => x.Key.SupplierPartPk.IsEmpty).Value;
					var recWithOrgAddressJobTotals = noPartsDetail.TradePeriods.Single(x => x.Key.IsJobValue);
					CombineAssertions("recWithOrgAddressJobTotals properties", () =>
					{
						AssertEquals("TradePeriodStart", new ZDate(2002, 1, 1), recWithOrgAddressJobTotals.Key.Period);
						AssertEquals("PeriodLastTrade", new ZDateTime(2002, 1, 1), recWithOrgAddressJobTotals.Value.PeriodLastTrade);
						AssertEquals("NumberOfJobs", 1, recWithOrgAddressJobTotals.Value.NumberOfJobs);
						AssertEquals("NumberOfPallets", 2, recWithOrgAddressJobTotals.Value.NumberOfPallets);
						AssertEquals("NumberOfLines - should be zero as it is a per product count", 0, recWithOrgAddressJobTotals.Value.NumberOfLines);
						AssertEquals("WeightVolume", 0m, recWithOrgAddressJobTotals.Value.WeightVolume);
						AssertEquals("Weight", 0.5m, recWithOrgAddressJobTotals.Value.Weight);
						AssertEquals("Volume", 40.0m, recWithOrgAddressJobTotals.Value.Volume);
					});
				}

				{
					var partADetail = receiveTradeLane1.Value.TradeDetails.Single(x => x.Key.SupplierPartPk == partAPk).Value;
					var recWithOrgAddressForPartA = partADetail.TradePeriods.Single(x => x.Key.IsJobValue);
					CombineAssertions("recWithOrgAddressForPartA properties", () =>
					{
						AssertEquals("TradePeriodStart", new ZDate(2002, 1, 1), recWithOrgAddressForPartA.Key.Period);
						AssertEquals("PeriodLastTrade", new ZDateTime(2002, 1, 1), recWithOrgAddressForPartA.Value.PeriodLastTrade);
						AssertEquals("NumberOfJobs", 1, recWithOrgAddressForPartA.Value.NumberOfJobs);
						AssertEquals("NumberOfPallets - should be zero as it is a per job count", 0, recWithOrgAddressForPartA.Value.NumberOfPallets);
						AssertEquals("NumberOfLines", 2, recWithOrgAddressForPartA.Value.NumberOfLines);
						AssertEquals("WeightVolume - should be zero as it is a per job count", 0m, recWithOrgAddressForPartA.Value.WeightVolume);
						AssertEquals("Weight - should be zero as it is a per job count", 0m, recWithOrgAddressForPartA.Value.Weight);
						AssertEquals("Volume - should be zero as it is a per job count", 0m, recWithOrgAddressForPartA.Value.Volume);
					});

					var recWithOrgAddressForPartAValue = recWithOrgAddressForPartA.Value.TradeValues.Single();
					CombineAssertions("recWithOrgAddressForPartAValue properties", () =>
					{
						AssertEquals("Currency", "AUD", recWithOrgAddressForPartAValue.Key.Currency);
						AssertEquals("Revenue", 100m, recWithOrgAddressForPartAValue.Value.Revenue);
					});
				}

				{
					var partBDetail = receiveTradeLane1.Value.TradeDetails.Single(x => x.Key.SupplierPartPk == partBPk).Value;
					var recWithOrgAddressForPartB = partBDetail.TradePeriods.Single(x => x.Key.IsJobValue);
					CombineAssertions("recWithOrgAddressForPartB properties", () =>
					{
						AssertEquals("TradePeriodStart", new ZDate(2002, 1, 1), recWithOrgAddressForPartB.Key.Period);
						AssertEquals("PeriodLastTrade", new ZDateTime(2002, 1, 1), recWithOrgAddressForPartB.Value.PeriodLastTrade);
						AssertEquals("NumberOfJobs", 1, recWithOrgAddressForPartB.Value.NumberOfJobs);
						AssertEquals("NumberOfPallets - should be zero as it is a per job count", 0, recWithOrgAddressForPartB.Value.NumberOfPallets);
						AssertEquals("NumberOfLines", 1, recWithOrgAddressForPartB.Value.NumberOfLines);
						AssertEquals("WeightVolume - should be zero as it is a per job count", 0m, recWithOrgAddressForPartB.Value.WeightVolume);
						AssertEquals("Weight - should be zero as it is a per job count", 0m, recWithOrgAddressForPartB.Value.Weight);
						AssertEquals("Volume - should be zero as it is a per job count", 0m, recWithOrgAddressForPartB.Value.Volume);
					});

					var recWithOrgAddressForPartBValue = recWithOrgAddressForPartB.Value.TradeValues.Single();
					CombineAssertions("recWithOrgAddressForPartBValue properties", () =>
					{
						AssertEquals("Currency", "AUD", recWithOrgAddressForPartBValue.Key.Currency);
						AssertEquals("Revenue", 100m, recWithOrgAddressForPartBValue.Value.Revenue);
					});
				}

				var receiveTradeLane2 = summary.ActualValues.Single(x => x.Key.Service == "REC" && x.Key.Supplier == ZGuid.Empty);
				AssertEquals("Org1Pk", org1.PK, receiveTradeLane2.Key.MainOrg);
				AssertEquals("Org2Pk", ZGuid.Empty, receiveTradeLane2.Key.Supplier);
				AssertEquals("Org3Pk", org1.PK, receiveTradeLane2.Key.Buyer);
				AssertEquals("WarehousePk", warehouse2Pk, receiveTradeLane2.Key.WarehousePk);
				AssertEquals("ProductPk", whs, receiveTradeLane2.Key.ProductPk);
				AssertEquals("Service", "REC", receiveTradeLane2.Key.Service);
				{
					var noPartsDetail = receiveTradeLane2.Value.TradeDetails.Single(x => x.Key.SupplierPartPk.IsEmpty).Value;
					var recWithOverridenAddressJobTotals = noPartsDetail.TradePeriods.Single(x => x.Key.IsJobValue);
					CombineAssertions("recWithOverridenAddressJobTotals", () =>
					{
						AssertEquals("TradePeriodStart", new ZDate(2002, 1, 1), recWithOverridenAddressJobTotals.Key.Period);
						AssertEquals("PeriodLastTrade", new ZDateTime(2002, 1, 1), recWithOverridenAddressJobTotals.Value.PeriodLastTrade);
						AssertEquals("NumberOfJobs", 1, recWithOverridenAddressJobTotals.Value.NumberOfJobs);
						AssertEquals("NumberOfPallets", 1, recWithOverridenAddressJobTotals.Value.NumberOfPallets);
						AssertEquals("NumberOfLines", 0, recWithOverridenAddressJobTotals.Value.NumberOfLines);
						AssertEquals("WeightVolume", 0m, recWithOverridenAddressJobTotals.Value.WeightVolume);
						AssertEquals("Weight", 0.01m, recWithOverridenAddressJobTotals.Value.Weight);
						AssertEquals("Volume", 10m, recWithOverridenAddressJobTotals.Value.Volume);
					});
				}

				{
					var partADetail = receiveTradeLane2.Value.TradeDetails.Single(x => x.Key.SupplierPartPk == partAPk).Value;
					var recWithOverridenAddress = partADetail.TradePeriods.Single(x => x.Key.IsJobValue);
					CombineAssertions("recWithOverridenAddressForPartA properties", () =>
					{
						AssertEquals("TradePeriodStart", new ZDate(2002, 1, 1), recWithOverridenAddress.Key.Period);
						AssertEquals("PeriodLastTrade", new ZDateTime(2002, 1, 1), recWithOverridenAddress.Value.PeriodLastTrade);
						AssertEquals("NumberOfJobs", 1, recWithOverridenAddress.Value.NumberOfJobs);
						AssertEquals("NumberOfPallets", 0, recWithOverridenAddress.Value.NumberOfPallets);
						AssertEquals("NumberOfLines", 1, recWithOverridenAddress.Value.NumberOfLines);
						AssertEquals("WeightVolume", 0m, recWithOverridenAddress.Value.WeightVolume);
						AssertEquals("Weight", 0m, recWithOverridenAddress.Value.Weight);
						AssertEquals("Volume", 0m, recWithOverridenAddress.Value.Volume);
					});

					var recWithOverridenAddressValue = recWithOverridenAddress.Value.TradeValues.Single();
					CombineAssertions("recWithOverridenAddressValue properties", () =>
					{
						AssertEquals("Currency", "AUD", recWithOverridenAddressValue.Key.Currency);
						AssertEquals("Revenue", 100m, recWithOverridenAddressValue.Value.Revenue);
					});
				}

				var orderTradeLane1 = summary.ActualValues.Single(x => x.Key.Service == "ORD" && x.Key.Buyer == org2.PK);
				AssertEquals("Org1Pk", org1.PK, orderTradeLane1.Key.MainOrg);
				AssertEquals("Org2Pk", org1.PK, orderTradeLane1.Key.Supplier);
				AssertEquals("Org3Pk", org2.PK, orderTradeLane1.Key.Buyer);
				AssertEquals("WarehousePk", warehouse1Pk, orderTradeLane1.Key.WarehousePk);
				AssertEquals("ProductPk", whs, orderTradeLane1.Key.ProductPk);
				AssertEquals("Service", "ORD", orderTradeLane1.Key.Service);
				{
					var ordDetail = orderTradeLane1.Value.TradeDetails.Single();
					AssertEquals("SupplierPartPk", ZGuid.Empty, ordDetail.Key.SupplierPartPk);

					var ordWithOrgAddress = ordDetail.Value.TradePeriods.Single(x => x.Key.IsJobValue);
					CombineAssertions("ordWithOrgAddress properties", () =>
					{
						AssertEquals("TradePeriodStart", new ZDate(2002, 1, 1), ordWithOrgAddress.Key.Period);
						AssertEquals("PeriodLastTrade", new ZDateTime(2002, 1, 1), ordWithOrgAddress.Value.PeriodLastTrade);
						AssertEquals("NumberOfJobs", 1, ordWithOrgAddress.Value.NumberOfJobs);
						AssertEquals("NumberOfPallets", 10, ordWithOrgAddress.Value.NumberOfPallets);
						AssertEquals("NumberOfLines", 0, ordWithOrgAddress.Value.NumberOfLines);
						AssertEquals("WeightVolume", 0m, ordWithOrgAddress.Value.WeightVolume);
						AssertEquals("Weight", 2.0m, ordWithOrgAddress.Value.Weight);
						AssertEquals("Volume", 80.0m, ordWithOrgAddress.Value.Volume);
					});

					var ordWithOrgAddressValue = ordWithOrgAddress.Value.TradeValues.Single();
					CombineAssertions("ordWithOrgAddressValue properties", () =>
					{
						AssertEquals("Currency", "AUD", ordWithOrgAddressValue.Key.Currency);
						AssertEquals("Revenue", 100m, ordWithOrgAddressValue.Value.Revenue);
					});
				}

				var orderTradeLane2 = summary.ActualValues.Single(x => x.Key.Service == "ORD" && x.Key.Buyer == ZGuid.Empty);
				AssertEquals("Org1Pk", org1.PK, orderTradeLane2.Key.MainOrg);
				AssertEquals("Org2Pk", org1.PK, orderTradeLane2.Key.Supplier);
				AssertEquals("Org3Pk", ZGuid.Empty, orderTradeLane2.Key.Buyer);
				AssertEquals("WarehousePk", warehouse1Pk, orderTradeLane2.Key.WarehousePk);
				AssertEquals("ProductPk", whs, orderTradeLane2.Key.ProductPk);
				AssertEquals("Service", "ORD", orderTradeLane2.Key.Service);
				{
					var ordDetail = orderTradeLane2.Value.TradeDetails.Single();
					AssertEquals("SupplierPartPk", ZGuid.Empty, ordDetail.Key.SupplierPartPk);

					var ordWithOverridenAddress = ordDetail.Value.TradePeriods.Single(x => x.Key.IsJobValue);
					CombineAssertions("ordWithOverridenAddress properties", () =>
					{
						AssertEquals("TradePeriodStart", new ZDate(2002, 1, 1), ordWithOverridenAddress.Key.Period);
						AssertEquals("PeriodLastTrade", new ZDateTime(2002, 1, 1), ordWithOverridenAddress.Value.PeriodLastTrade);
						AssertEquals("NumberOfJobs", 1, ordWithOverridenAddress.Value.NumberOfJobs);
						AssertEquals("NumberOfPallets", 5, ordWithOverridenAddress.Value.NumberOfPallets);
						AssertEquals("NumberOfLines", 0, ordWithOverridenAddress.Value.NumberOfLines);
						AssertEquals("WeightVolume", 0m, ordWithOverridenAddress.Value.WeightVolume);
						AssertEquals("Weight", 0.1m, ordWithOverridenAddress.Value.Weight);
						AssertEquals("Volume", 50.0m, ordWithOverridenAddress.Value.Volume);
					});
				}

				AssertEquals(4, summary.ActualValues.Count);
				AssertEquals(7, summary.ActualValues.Sum(x => x.Value.TradeDetails.Count));
			}
		}

		public void TestWarehouseFinalisedDateIsConvertedToUTC()
		{
			var orgPk = ZGuid.NewZGuid();
			var addressPk = ZGuid.NewZGuid();
			var warehousePk = ZGuid.NewZGuid();
			var orgSalesPk = ZGuid.NewZGuid();
			var docket1Pk = ZGuid.NewZGuid();
			var docket2Pk = ZGuid.NewZGuid();
			var docketLine1Pk = ZGuid.NewZGuid();
			var docketLine2Pk = ZGuid.NewZGuid();
			var partPk = ZGuid.NewZGuid();

			var insertSql = $@"
ALTER TABLE dbo.WhsDocket DROP CONSTRAINT IF EXISTS Constraint_WD_WP

DECLARE @Company UNIQUEIDENTIFIER = (SELECT TOP 1 GC_PK FROM dbo.GlbCompany)
DECLARE @Branch UNIQUEIDENTIFIER = (SELECT TOP 1 GB_PK FROM dbo.GlbBranch)
DECLARE @Department UNIQUEIDENTIFIER = (SELECT TOP 1 GE_PK FROM dbo.GlbDepartment)
DECLARE @LocationType UNIQUEIDENTIFIER = (SELECT TOP 1 WLT_PK FROM dbo.WhsLocationType WHERE WLT_Code = 'RNO')

DECLARE @Org UNIQUEIDENTIFIER = '{orgPk}'
DECLARE @OrgAddress UNIQUEIDENTIFIER = '{addressPk}'
DECLARE @Warehouse UNIQUEIDENTIFIER = '{warehousePk}'
DECLARE @OrgSales UNIQUEIDENTIFIER = '{orgSalesPk}'
DECLARE @Docket1 UNIQUEIDENTIFIER = '{docket1Pk}'
DECLARE @Docket2 UNIQUEIDENTIFIER = '{docket2Pk}'
DECLARE @DocketLine1 UNIQUEIDENTIFIER = '{docketLine1Pk}'
DECLARE @DocketLine2 UNIQUEIDENTIFIER = '{docketLine2Pk}'
DECLARE @SupplierPart UNIQUEIDENTIFIER = '{partPk}'

INSERT INTO dbo.OrgHeader
	(OH_PK, OH_Code)
VALUES
	(@Org, 'XXXX1')

INSERT INTO dbo.OrgAddress
	(OA_PK, OA_OH, OA_Code, OA_Address1, OA_RL_NKRelatedPortCode)
VALUES
	(@OrgAddress, @Org, '1ST', '1st Street', 'AUSYD')

INSERT INTO dbo.WhsWarehouse
	(WW_PK, WW_WarehouseCode, WW_GB_RelatedCompanyBranch, WW_OA_WarehouseAddress, WW_WarehouseType, WW_DefaultOutboundDockDoor, WW_DefaultInboundDockDoor, WW_WLT_DefaultLocationType, WW_SystemCreateTimeUtc, WW_SystemCreateUser, WW_SystemLastEditTimeUtc, WW_SystemLastEditUser)
VALUES
	(@Warehouse, 'WH2', @Branch, @OrgAddress, 'TRW', NULL, NULL, @LocationType, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.OrgSales
	(OW_PK, OW_OH_Buyer, OW_Service, OW_WW, OW_IsTraded, OW_SystemCreateTimeUtc, OW_SystemLastEditTimeUtc, OW_SystemCreateUser, OW_SystemLastEditUser)
VALUES
	(@OrgSales, @Org, 'REC', @Warehouse, 1, GETUTCDATE(), GETUTCDATE(), '~BP', '~BP')

INSERT INTO dbo.WhsDocket
	(WD_PK, WD_OH_Client, WD_DocketID, WD_ExternalReference, WD_WW_Whs, WD_DocketType, WD_DocketSubType, WD_TotalPallets, WD_PalletsSent, WD_TotalUnits, WD_TotalWeight, WD_TotalWeightUnit, WD_TotalCubic, WD_TotalCubicUnit, WD_BookingDate, WD_DocketStatus, WD_FinalisedDate, WD_GS_NKFinalizedBy, WD_UnloadCompletedTime, WD_SystemCreateTimeUtc, WD_ArrivalDate, WD_SystemCreateUser, WD_SystemLastEditTimeUtc, WD_SystemLastEditUser)
VALUES
	(@Docket1, @Org,    'W00010001', 'W00010001',          @Warehouse,       'INW',       'REC',               2,             10,             4,           5000,               'HG',         40000,              'D3',     '2002-1-1',           'FIN',       '2023-03-01 00:53:15 +11:00', '~BP'              , '2023-03-05 00:53:15 +11:00', '2002-1-1', '2002-1-1', '~BP', GetUtcDate(), '~BP'),
	(@Docket2, @Org,    'W00010002', 'W00010002',          @Warehouse,       'INW',       'REC',               2,             10,             4,           5000,               'HG',         40000,              'D3',     '2002-1-1',           'FIN',       '2023-03-05 00:53:15 +11:00', '~BP'              , '2023-03-05 00:53:15 +11:00', '2002-1-1', '2002-1-1', '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.OrgSupplierPart
	(OP_PK, OP_PartNum)
VALUES
	(@SupplierPart, 'AAAA')

INSERT INTO dbo.WhsDocketLine
	(WE_PK, WE_WD, WE_OP, WE_DocketLineStatus, WE_FinalisedDate, WE_DocketLineType, WE_F3_NKPackType, WE_AdjustmentArrivalDate, WE_OriginalInventoryStatus, WE_CurrentInventoryStatus, WE_WE_OriginalDocketLineForRating, WE_SystemCreateTimeUtc, WE_SystemCreateUser, WE_SystemLastEditTimeUtc, WE_SystemLastEditUser)
VALUES
	(@DocketLine1, @Docket1, @SupplierPart, 'FIN', '2023-03-01 00:53:15 +11:00', 'INW', 'UNT', '2002-1-1', 'AVL', 'AVL', @DocketLine1, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(@DocketLine2, @Docket2, @SupplierPart, 'FIN', '2023-03-05 00:53:15 +11:00', 'INW', 'UNT', '2002-1-1', 'AVL', 'AVL', @DocketLine2, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";

			TestConnection.ExecuteNonQuery(insertSql);
			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(TestConnection);

			using (var provider = new CachedTradeLinesSummaryProvider(TestConnection, new ZDate(2023, 3, 1), new ZDate(2023, 4, 1)))
			{
				var org = Factory.Load<OrgHeader>(orgPk);
				var summary = provider.GetForOrg(org, new ZDate(2023, 3, 1), new ZDate(2023, 4, 1));

				var synchroniser = new TradeLinesSynchroniser(org.PK);
				synchroniser.Execute(summary);

				var query = new ZQuery(OrgTradePeriodSchema.PAS_Period, new ZDate(2023, 03, 01));
				var periodsInMarch = Factory.Load<OrgTradePeriod>(query);
				AssertEquals(2, periodsInMarch.Length);
			}

			using (var provider = new CachedTradeLinesSummaryProvider(TestConnection, new ZDate(2023, 2, 1), new ZDate(2023, 3, 1)))
			{
				var org = Factory.Load<OrgHeader>(orgPk);
				var summary = provider.GetForOrg(org, new ZDate(2023, 2, 1), new ZDate(2023, 3, 1));

				var synchroniser = new TradeLinesSynchroniser(org.PK);
				synchroniser.Execute(summary);

				var query = new ZQuery(OrgTradePeriodSchema.PAS_Period, new ZDate(2023, 02, 01));
				var periodsInFeb = Factory.Load<OrgTradePeriod>(query);
				AssertEquals(2, periodsInFeb.Length);
			}
		}

		#endregion

		#region Warehouse Storage

		public void TestWarehouseStorage()
		{
			var org1Pk = ZGuid.NewZGuid();
			var address1Pk = ZGuid.NewZGuid();

			var warehouse1Pk = ZGuid.NewZGuid();
			var partAPk = ZGuid.NewZGuid();

			var receive1Pk = ZGuid.NewZGuid();
			var jobStoragePk = ZGuid.NewZGuid();

			var insertSql = $@"
ALTER TABLE dbo.WhsDocket DROP CONSTRAINT IF EXISTS Constraint_WD_WP

DECLARE @Company UNIQUEIDENTIFIER = (SELECT TOP 1 GC_PK FROM dbo.GlbCompany)
DECLARE @Branch UNIQUEIDENTIFIER = (SELECT TOP 1 GB_PK FROM dbo.GlbBranch)
DECLARE @Department UNIQUEIDENTIFIER = (SELECT TOP 1 GE_PK FROM dbo.GlbDepartment)

DECLARE @WaPk UNIQUEIDENTIFIER = newid();
DECLARE @WrPk UNIQUEIDENTIFIER = newid();
DECLARE @WlPk UNIQUEIDENTIFIER = newid();

DECLARE @Org1 UNIQUEIDENTIFIER = '{org1Pk}'

DECLARE @Org1Address UNIQUEIDENTIFIER = '{address1Pk}'

DECLARE @Warehouse1 UNIQUEIDENTIFIER = '{warehouse1Pk}'
DECLARE @SupplierPartA UNIQUEIDENTIFIER = '{partAPk}'

DECLARE @LocationType UNIQUEIDENTIFIER = (SELECT TOP 1 WLT_PK FROM dbo.WhsLocationType WHERE WLT_Code = 'RNO')
DECLARE @ChargeCode UNIQUEIDENTIFIER = (SELECT TOP 1 AC_PK FROM dbo.AccChargeCode WHERE AC_ChargeType != 'DSB')
DECLARE @AccGLHeader UNIQUEIDENTIFIER = (SELECT TOP 1 AG_PK FROM dbo.AccGLHeader)

INSERT INTO dbo.OrgHeader
	(OH_PK, OH_Code)
VALUES
	(@Org1, 'XXXX1')

INSERT INTO dbo.OrgAddress
	(OA_PK, OA_OH, OA_Code, OA_Address1, OA_RL_NKRelatedPortCode)
VALUES
	(@Org1Address, @Org1, '1ST', '1st Street', 'AUSYD')

INSERT INTO dbo.WhsWarehouse
	(WW_PK, WW_WarehouseCode, WW_GB_RelatedCompanyBranch, WW_OA_WarehouseAddress, WW_WarehouseType, WW_DefaultOutboundDockDoor, WW_DefaultInboundDockDoor, WW_WLT_DefaultLocationType, WW_SystemCreateTimeUtc, WW_SystemCreateUser, WW_SystemLastEditTimeUtc, WW_SystemLastEditUser)
VALUES
	(@Warehouse1, 'WH1', @Branch, @Org1Address, 'TRW', NULL, NULL, @LocationType, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.WhsArea (WA_PK, WA_Name, WA_AreaType, WA_WW_Whs, WA_SystemCreateTimeUtc, WA_SystemCreateUser, WA_SystemLastEditTimeUtc, WA_SystemLastEditUser) VALUES (@WaPk, 'AREA01', 'FRE', @Warehouse1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.WhsRow (WR_PK, WR_Name, WR_Columns, WR_Levels, WR_Trays, WR_WW_Whs, WR_SystemLastEditTimeUtc, WR_SystemCreateTimeUtc, WR_SystemCreateUser, WR_SystemLastEditUser) VALUES
	(@WrPk, 'ROW01', 1, 1, 1, @Warehouse1, GETUTCDATE(), GETUTCDATE(), '~BP', '~BP');

INSERT INTO dbo.WhsLocation (WL_PK, WL_WA_PickingArea, WL_WA_PutawayArea, WL_WR, WL_Column, WL_Level, WL_Tray, WL_WLT_LocationType, WL_LocationStatus, WL_PickMethod, WL_PickPathSequence, WL_MaxQuantityUnit, WL_PutawayPathSequence, WL_SystemLastEditTimeUtc, WL_SystemCreateTimeUtc, WL_SystemCreateUser, WL_SystemLastEditUser) VALUES
	(@WlPk, @WaPk, @WaPk, @WrPk, 1, 1, 1, '16C9FD62-730A-42ED-A20E-699606FFF360', 'NOR', 'ANY', 1, 'UNT', 1, GETUTCDATE(), GETUTCDATE(), '~BP', '~BP');

INSERT INTO dbo.OrgSupplierPart
	(OP_PK, OP_PartNum)
VALUES
	(@SupplierPartA, 'AAAA')

DECLARE @Receive1 UNIQUEIDENTIFIER = '{receive1Pk}'

INSERT INTO dbo.WhsDocket
	(WD_PK, WD_OH_Client, WD_DocketID, WD_ExternalReference, WD_WW_Whs, WD_DocketType, WD_DocketSubType, WD_TotalPallets, WD_PalletsSent, WD_TotalUnits, WD_TotalWeight, WD_TotalWeightUnit, WD_TotalCubic, WD_TotalCubicUnit, WD_BookingDate, WD_DocketStatus, WD_FinalisedDate, WD_GS_NKFinalizedBy, WD_UnloadCompletedTime, WD_SystemCreateTimeUtc, WD_ArrivalDate, WD_SystemCreateUser, WD_SystemLastEditTimeUtc, WD_SystemLastEditUser)
VALUES
	(@Receive1, @Org1, 'W00010003', 'W00010003', @Warehouse1, 'INW', 'REC', 2, 10, 4, 5000, 'HG', 40000, 'D3', '2002-1-1', 'FIN', '2002-1-1', '~BP', '2002-1-1', '2002-1-1', '2002-1-1', '~BP', GetUtcDate(), '~BP')

DECLARE @DocketLine1PK UNIQUEIDENTIFIER = NewID()
DECLARE @DocketLine2PK UNIQUEIDENTIFIER = NewID()

INSERT INTO dbo.WhsDocketLine
	(WE_PK, WE_WD, WE_WL, WE_OP, WE_DocketLineStatus, WE_FinalisedDate, WE_DocketLineType, WE_F3_NKPackType, WE_AdjustmentArrivalDate, WE_OriginalInventoryStatus, WE_CurrentInventoryStatus, WE_WE_OriginalDocketLineForRating, WE_SystemCreateTimeUtc, WE_SystemCreateUser, WE_SystemLastEditTimeUtc, WE_SystemLastEditUser)
VALUES
	(@DocketLine1PK, @Receive1, @WlPk, @SupplierPartA, 'FIN', '2002-1-1', 'INW', 'UNT', '2019-6-25', 'AVL', 'AVL', @DocketLine1PK, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(@DocketLine2PK, @Receive1, @WlPk, @SupplierPartA, 'FIN', '2002-1-1', 'INW', 'UNT', '2019-7-2', 'AVL', 'AVL', @DocketLine2PK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

DECLARE @JobStoragePk UNIQUEIDENTIFIER = '{jobStoragePk}'

INSERT INTO dbo.JobStorage (ET_PK, ET_StorageJobNumber, ET_StorageType, ET_WW, ET_OH_Client, ET_StorageFromDate, ET_StorageToDate, ET_SystemCreateTimeUtc, ET_SystemCreateUser, ET_SystemLastEditTimeUtc, ET_SystemLastEditUser)
VALUES
	(@JobStoragePk, 'I00000001', 'WHS', @Warehouse1, @Org1, '2019-7-1', '2019-7-2', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

DECLARE @JobHeader1PK UNIQUEIDENTIFIER = NewID()

INSERT INTO dbo.JobHeader
	(JH_PK, JH_ParentID, JH_ParentTableCode, JH_JobNum, JH_GC, JH_GB, JH_GE, JH_OA_LocalChargesAddr, JH_Status, JH_A_JCL)
VALUES
	(@JobHeader1PK, @JobStoragePk, 'ET', 'ET00001001', @Company, @Branch, @Department, @Org1Address, 'WRK', '2019-03-18')

DECLARE @TransactionHeader1PK UNIQUEIDENTIFIER = NewID()

INSERT INTO dbo.AccTransactionHeader
	(AH_PK, AH_TransactionNum, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_Ledger, AH_TransactionType)
VALUES
	(@transactionHeader1PK, 'INV00001001', @Company, @Branch, @Department, '2000-1-1', 'AR', 'INV')

DECLARE @TransactionLine1PK UNIQUEIDENTIFIER = NewID()

INSERT INTO dbo.AccTransactionLines
	(AL_PK, AL_AH, AL_JH, AL_AC, AL_LineType, AL_LineAmount, AL_OH, AL_GC, AL_GB, AL_GE, AL_AG, AL_ReverseDate)
VALUES
	(@TransactionLine1PK, @TransactionHeader1PK, @JobHeader1PK, @ChargeCode, 'REV', 100, @Org1, @Company, @Branch, @Department, @AccGLHeader, '2002-1-1')

DECLARE @JobCharge1PK UNIQUEIDENTIFIER = NewID()

INSERT INTO dbo.JobCharge (JR_PK, JR_JH, JR_AL_ARLine, JR_AC, JR_GB, JR_GC, JR_GE, JR_SystemLastEditTimeUtc, JR_SystemLastEditUser)
VALUES 
	(@JobCharge1PK, @JobHeader1PK, @TransactionLine1PK, @ChargeCode, @Branch, @Company, @Department, GETUTCDATE(), '~BP')

INSERT INTO dbo.JobChargeAttrib (EC_PK, EC_JR, EC_NAME, EC_VALUE) 
VALUES
	(NEWID(), @JobCharge1PK, 'PRD', 'AAAA')

INSERT INTO dbo.OrgPartRelation (OU_PK, OU_OH, OU_OP, OU_Relationship, OU_SystemCreateTimeUtc, OU_SystemCreateUser, OU_SystemLastEditTimeUtc, OU_SystemLastEditUser) 
VALUES 
	(NEWID(), @Org1, @SupplierPartA, 'BTH', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced, AM_GC_Company)
VALUES
	(NEWID(), 201903, 2019, '1999-03-18', '2020-03-18', 0, 0, 0, 0, @Company)
";

			TestConnection.ExecuteNonQuery(insertSql);
			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(TestConnection);

			using (var provider = new NonCachedTradeLinesSummaryProvider(TestConnection))
			{
				var whs = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse).PK;
				var org1 = Factory.Load<OrgHeader>(org1Pk);
				var summary = provider.GetForOrg(org1, new ZDate(2019, 6, 1), new ZDate(2019, 7, 1));
				AssertEquals("Count", 1, summary.ActualValues.Count);
				AssertTradeLaneKey("Period 1/6/2019", summary.ActualValues.Keys.First(), org1.PK, warehouse1Pk, whs);
				AssertTradeLaneValue("Period 1/6/2019", summary.ActualValues.Values.First(), partAPk, org1.PK, new ZDate(2019, 6, 1));

				summary = provider.GetForOrg(org1, new ZDate(2019, 7, 1), new ZDate(2019, 8, 1));
				AssertEquals("Count", 1, summary.ActualValues.Count);
				AssertTradeLaneKey("Period 1/7/2019", summary.ActualValues.Keys.First(), org1.PK, warehouse1Pk, whs);
				AssertTradeLaneValue("Period 1/7/2019", summary.ActualValues.Values.First(), partAPk, org1.PK, new ZDate(2019, 7, 1));
			}
		}

		void AssertTradeLaneKey(string description, TradeLaneKey key, ZGuid mainOrgPk, ZGuid warehousePk, ZGuid productPk)
		{
			AssertEquals($"{description} Main Org", mainOrgPk, key.MainOrg);
			AssertEquals($"{description} Product", productPk, key.ProductPk);
			AssertEquals($"{description} Warehouse", warehousePk, key.WarehousePk);
		}

		void AssertTradeLaneValue(string description, TradeLaneValue value, ZGuid partPK, ZGuid orgPk, ZDate period)
		{
			foreach (var tradeDetail in value.TradeDetails)
			{
				AssertEquals($"{description} Supplier Part", partPK, tradeDetail.Key.SupplierPartPk);
				foreach (var tradePeriod in tradeDetail.Value.TradePeriods)
				{
					AssertEquals($"{description} period Org", orgPk, tradePeriod.Key.OrgPk);
					AssertEquals($"{description} period", period, tradePeriod.Key.Period);
				}
			}
		}

		public void TestWarehouseStorageArrivalDateIsConvertedToUTC()
		{
			var orgPk = ZGuid.NewZGuid();
			var addressPk = ZGuid.NewZGuid();
			var warehousePk = ZGuid.NewZGuid();

			var whsAreaPk = ZGuid.NewZGuid();
			var whsRowPk = ZGuid.NewZGuid();
			var whsLocationPk = ZGuid.NewZGuid();

			var docket1Pk = ZGuid.NewZGuid();
			var docket2Pk = ZGuid.NewZGuid();
			var docketLine1Pk = ZGuid.NewZGuid();
			var docketLine2Pk = ZGuid.NewZGuid();
			var partPk = ZGuid.NewZGuid();

			var insertSql = $@"
ALTER TABLE dbo.WhsDocket DROP CONSTRAINT IF EXISTS Constraint_WD_WP

DECLARE @Company UNIQUEIDENTIFIER = (SELECT TOP 1 GC_PK FROM dbo.GlbCompany)
DECLARE @Branch UNIQUEIDENTIFIER = (SELECT TOP 1 GB_PK FROM dbo.GlbBranch)
DECLARE @Department UNIQUEIDENTIFIER = (SELECT TOP 1 GE_PK FROM dbo.GlbDepartment)
DECLARE @LocationType UNIQUEIDENTIFIER = (SELECT TOP 1 WLT_PK FROM dbo.WhsLocationType WHERE WLT_Code = 'RNO')

DECLARE @Org UNIQUEIDENTIFIER = '{orgPk}'
DECLARE @OrgAddress UNIQUEIDENTIFIER = '{addressPk}'
DECLARE @Warehouse UNIQUEIDENTIFIER = '{warehousePk}'

DECLARE @WhsArea UNIQUEIDENTIFIER = '{whsAreaPk}'
DECLARE @WhsRow UNIQUEIDENTIFIER = '{whsRowPk}'
DECLARE @WhsLocation UNIQUEIDENTIFIER = '{whsLocationPk}'

DECLARE @Docket1 UNIQUEIDENTIFIER = '{docket1Pk}'
DECLARE @Docket2 UNIQUEIDENTIFIER = '{docket2Pk}'
DECLARE @DocketLine1 UNIQUEIDENTIFIER = '{docketLine1Pk}'
DECLARE @DocketLine2 UNIQUEIDENTIFIER = '{docketLine2Pk}'
DECLARE @SupplierPart UNIQUEIDENTIFIER = '{partPk}'

INSERT INTO dbo.OrgHeader
	(OH_PK, OH_Code)
VALUES
	(@Org, 'XXXX1')

INSERT INTO dbo.OrgAddress
	(OA_PK, OA_OH, OA_Code, OA_Address1, OA_RL_NKRelatedPortCode)
VALUES
	(@OrgAddress, @Org, '1ST', '1st Street', 'AUSYD')

INSERT INTO dbo.WhsWarehouse
	(WW_PK, WW_WarehouseCode, WW_GB_RelatedCompanyBranch, WW_OA_WarehouseAddress, WW_WarehouseType, WW_DefaultOutboundDockDoor, WW_DefaultInboundDockDoor, WW_WLT_DefaultLocationType, WW_SystemCreateTimeUtc, WW_SystemCreateUser, WW_SystemLastEditTimeUtc, WW_SystemLastEditUser)
VALUES
	(@Warehouse, 'WH2', @Branch, @OrgAddress, 'TRW', NULL, NULL, @LocationType, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.WhsArea (WA_PK, WA_Name, WA_AreaType, WA_WW_Whs, WA_SystemCreateTimeUtc, WA_SystemCreateUser, WA_SystemLastEditTimeUtc, WA_SystemLastEditUser) 
VALUES (@WhsArea, 'AREA01', 'FRE', @Warehouse, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.WhsRow (WR_PK, WR_Name, WR_Columns, WR_Levels, WR_Trays, WR_WW_Whs, WR_SystemLastEditTimeUtc, WR_SystemCreateTimeUtc, WR_SystemCreateUser, WR_SystemLastEditUser) 
VALUES
	(@WhsRow, 'ROW01', 1, 1, 1, @Warehouse, GETUTCDATE(), GETUTCDATE(), '~BP', '~BP');

INSERT INTO dbo.WhsLocation (WL_PK, WL_WA_PickingArea, WL_WA_PutawayArea, WL_WR, WL_Column, WL_Level, WL_Tray, WL_WLT_LocationType, WL_LocationStatus, WL_PickMethod, WL_PickPathSequence, WL_MaxQuantityUnit, WL_PutawayPathSequence, WL_SystemLastEditTimeUtc, WL_SystemCreateTimeUtc, WL_SystemCreateUser, WL_SystemLastEditUser) 
VALUES
	(@WhsLocation, @WhsArea, @WhsArea, @WhsRow, 1, 1, 1, @LocationType, 'NOR', 'ANY', 1, 'UNT', 1, GETUTCDATE(), GETUTCDATE(), '~BP', '~BP');

INSERT INTO dbo.WhsDocket
	(WD_PK, WD_OH_Client, WD_DocketID, WD_ExternalReference, WD_WW_Whs, WD_DocketType, WD_DocketSubType, WD_TotalPallets, WD_PalletsSent, WD_TotalUnits, WD_TotalWeight, WD_TotalWeightUnit, WD_TotalCubic, WD_TotalCubicUnit, WD_BookingDate, WD_DocketStatus, WD_FinalisedDate, WD_GS_NKFinalizedBy, WD_SystemCreateTimeUtc, WD_ArrivalDate, WD_SystemCreateUser, WD_SystemLastEditTimeUtc, WD_SystemLastEditUser, WD_UnloadCompletedTime)
VALUES
	(@Docket1, @Org,    'W00010001', 'W00010001',          @Warehouse,       'INW',       'REC',               2,             10,             4,           5000,               'HG',         40000,              'D3',     '2002-1-1',           'FIN',       '2002-1-1', '~BP'              , '2002-1-1', '2002-1-1', '~BP', GetUtcDate(), '~BP', '2002-1-1'),
	(@Docket2, @Org,    'W00010002', 'W00010002',          @Warehouse,       'INW',       'REC',               2,             10,             4,           5000,               'HG',         40000,              'D3',     '2002-1-1',           'FIN',       '2002-1-1', '~BP'              , '2002-1-1', '2002-1-1', '~BP', GetUtcDate(), '~BP', '2002-1-1')

INSERT INTO dbo.OrgSupplierPart
	(OP_PK, OP_PartNum)
VALUES
	(@SupplierPart, 'AAAA')

INSERT INTO dbo.WhsDocketLine
	(WE_PK, WE_WD, WE_OP, WE_WL, WE_DocketLineStatus, WE_FinalisedDate, WE_DocketLineType, WE_F3_NKPackType, WE_AdjustmentArrivalDate, WE_OriginalInventoryStatus, WE_CurrentInventoryStatus, WE_WE_OriginalDocketLineForRating, WE_SystemCreateTimeUtc, WE_SystemCreateUser, WE_SystemLastEditTimeUtc, WE_SystemLastEditUser)
VALUES
	(@DocketLine1, @Docket1, @SupplierPart, @WhsLocation, 'FIN', '2002-1-1', 'INW', 'UNT', '2023-03-01 00:53:15 +11:00', 'AVL', 'AVL', @DocketLine1, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(@DocketLine2, @Docket2, @SupplierPart, @WhsLocation, 'FIN', '2002-1-1', 'INW', 'UNT', '2023-03-02 00:53:15 +11:00', 'AVL', 'AVL', @DocketLine2, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";

			TestConnection.ExecuteNonQuery(insertSql);
			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(TestConnection);

			using (var provider = new CachedTradeLinesSummaryProvider(TestConnection, new ZDate(2023, 3, 1), new ZDate(2023, 4, 1)))
			{
				var org = Factory.Load<OrgHeader>(orgPk);
				var summary = provider.GetForOrg(org, new ZDate(2023, 3, 1), new ZDate(2023, 4, 1));

				var synchroniser = new TradeLinesSynchroniser(org.PK);
				synchroniser.Execute(summary);

				var query = new ZQuery(OrgTradePeriodSchema.PAS_Period, new ZDate(2023, 03, 01));
				var periodsInMarch = Factory.Load<OrgTradePeriod>(query);
				AssertEquals(2, periodsInMarch.Length);
			}

			using (var provider = new CachedTradeLinesSummaryProvider(TestConnection, new ZDate(2023, 2, 1), new ZDate(2023, 3, 1)))
			{
				var org = Factory.Load<OrgHeader>(orgPk);
				var summary = provider.GetForOrg(org, new ZDate(2023, 2, 1), new ZDate(2023, 3, 1));

				var synchroniser = new TradeLinesSynchroniser(org.PK);
				synchroniser.Execute(summary);

				var query = new ZQuery(OrgTradePeriodSchema.PAS_Period, new ZDate(2023, 02, 01));
				var periodsInFeb = Factory.Load<OrgTradePeriod>(query);
				AssertEquals(2, periodsInFeb.Length);
			}
		}

		#endregion

		#region Quotation

		public void TestQuotations_Forwarding()
		{
			var insertSql = @"
DECLARE @Company UNIQUEIDENTIFIER = (SELECT TOP 1 GC_PK FROM dbo.GlbCompany)
DECLARE @Org UNIQUEIDENTIFIER = 'E8C9E76C-C515-405B-A5AC-177170F2644B'
DECLARE @Quotation UNIQUEIDENTIFIER = '43FB1BBE-8070-4376-8BB5-1006A18419F2';

INSERT INTO dbo.OrgHeader
	(OH_PK, OH_Code)
VALUES
	(@Org, 'XXXX')

INSERT INTO dbo.RatingHeader
	(TH_PK, TH_OH, TH_RateType, TH_QuoteNumber, TH_QuoteDate, TH_IsCancelled, TH_GC, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser, TH_SystemCreateTimeUtc, TH_SystemCreateUser)
VALUES
	(@Quotation, @Org, 'QTE', 'QTE00001', '2015-1-1', 0, @Company, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.RateEntry
	(TI_PK, TI_TH, TI_GC_Publisher, TI_OriginLRC, TI_DestinationLRC, TI_RateCategory, TI_Mode, TI_RateStartDate, TI_RateEndDate, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser, TI_SystemCreateTimeUtc, TI_SystemCreateUser)
VALUES
	('650B489F-E809-43C9-816B-A61A6A021695', @Quotation, @Company, 'AUSYD', '', 'AIR', 'LSE', '2014-1-1',       NULL, '2015-1-1', '~BP', '2015-1-1', '~BP'),
	('E9D5E094-6B04-4095-8531-E6A1496B4B6B', @Quotation, @Company, 'AUSYD', '', 'AIR', 'ULD', '2014-1-1',       NULL, '2015-1-1', '~BP', '2015-1-1', '~BP'),
	('42B3CA1B-9014-4245-9AE5-4CFAE199C2C8', @Quotation, @Company, 'AUSYD', '', 'FCL', 'SEA', '2014-1-1',       NULL, '2015-1-1', '~BP', '2015-1-1', '~BP'),

	('D87695D8-0D50-4716-BB36-EE28595B9A5D', @Quotation, @Company, 'AUSYD', '', 'FCL', 'RAI', '2014-1-1', '2016-6-6', '2015-1-1', '~BP', '2015-1-1', '~BP'),
	('CA7F5FE5-B7F0-41FF-A602-493A81E8C1D0', @Quotation, @Company, 'AUSYD', '', 'LCL', 'LCL', '2014-1-1',       NULL, '2015-1-1', '~BP', '2015-1-1', '~BP'),

	('64F34E36-6B57-4B4B-9825-DF2FA11D9D6D', @Quotation, @Company, 'AUSYD', '', 'LCL', 'LRO', '2014-1-1',       NULL, '2015-1-1', '~BP', '2015-1-1', '~BP'),
	('372DAAD7-4E65-4537-ACC8-A336372161B9', @Quotation, @Company, 'AUSYD', '', 'LCL', 'FTL', '2014-1-1',       NULL, '2015-1-1', '~BP', '2015-1-1', '~BP'),
	('FAC63AD8-44D0-4B8D-9E68-F89B735F7EA5', @Quotation, @Company, 'AUSYD', '', 'LCL', 'LRA', '2014-1-1',       NULL, '2015-1-1', '~BP', '2015-1-1', '~BP'),
	('99A28F6B-C46B-4D95-9087-7C1E55C4BF06', @Quotation, @Company, 'AUSYD', '', 'LCL', 'FWL', '2014-1-1',       NULL, '2015-1-1', '~BP', '2015-1-1', '~BP'),

	('9284BFBE-E932-4905-AA37-B913B51A683B', @Quotation, @Company, 'AUSYD', '', 'SCO', 'SEA', '2014-1-1',       NULL, '2015-1-1', '~BP', '2015-1-1', '~BP'),
	('255FD73B-A33C-4837-87F7-40612180F5B8', @Quotation, @Company, 'AUSYD', '', 'SNC', 'LCL', '2014-1-1',       NULL, '2015-1-1', '~BP', '2015-1-1', '~BP'),

	('F7F69905-FE0F-40F0-A42F-598EA0873A1B', @Quotation, @Company, 'AUSYD', '', 'FCL', 'ROA', '2010-1-1', '2011-1-1', '2015-1-1', '~BP', '2015-1-1', '~BP'), -- Expired earlier than the @From sync date - should not be synched
	('F7F69905-FE0F-40F0-A42F-598EA0873A1C', @Quotation, @Company, 'AUSYD', '', 'FCL', 'RAI', '2010-1-1', '2011-1-1', '2014-1-1', '~BP', '2014-1-1', '~BP')  -- Edited and expired earlier than the @From sync date - should not be synched
";

			TestConnection.ExecuteNonQuery(insertSql);
			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(TestConnection);

			using (var provider = new NonCachedTradeLinesSummaryProvider(TestConnection))
			{
				var shpPk = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP").PK;
				var lgyPk = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "LGY").PK;

				var expected = new Tuple<ZGuid, ZGuid, string, string>[]
				{
					Tuple.Create(new ZGuid("650B489F-E809-43C9-816B-A61A6A021695"), shpPk, "AIR", "LSE"),
					Tuple.Create(new ZGuid("E9D5E094-6B04-4095-8531-E6A1496B4B6B"), shpPk, "AIR", "ULD"),

					Tuple.Create(new ZGuid("42B3CA1B-9014-4245-9AE5-4CFAE199C2C8"), shpPk, "SEA", "FCL"),
					Tuple.Create(new ZGuid("D87695D8-0D50-4716-BB36-EE28595B9A5D"), shpPk, "SEA", "FCL"),
					Tuple.Create(new ZGuid("CA7F5FE5-B7F0-41FF-A602-493A81E8C1D0"), shpPk, "SEA", "LCL"),

					Tuple.Create(new ZGuid("64F34E36-6B57-4B4B-9825-DF2FA11D9D6D"), shpPk, "ROA", "LTL"),
					Tuple.Create(new ZGuid("372DAAD7-4E65-4537-ACC8-A336372161B9"), shpPk, "ROA", "FTL"),

					Tuple.Create(new ZGuid("FAC63AD8-44D0-4B8D-9E68-F89B735F7EA5"), shpPk, "RAI", "LCL"),
					Tuple.Create(new ZGuid("99A28F6B-C46B-4D95-9087-7C1E55C4BF06"), shpPk, "RAI", "LCL"),

					Tuple.Create(new ZGuid("9284BFBE-E932-4905-AA37-B913B51A683B"), lgyPk, "BOL", "FCL"),
					Tuple.Create(new ZGuid("255FD73B-A33C-4837-87F7-40612180F5B8"), lgyPk, "BOL", "BBK")
				};

				var org = Factory.Load<OrgHeader>(new ZGuid("E8C9E76C-C515-405B-A5AC-177170F2644B"));
				var summary = provider.GetForOrg(org, new ZDate(2014, 6, 1), new ZDate(2015, 6, 1));

				var actual = new List<Tuple<ZGuid, ZGuid, string, string>>();
				foreach (var tradeLane in summary.ProspectValues)
				{
					foreach (var tradeDetail in tradeLane.Value.TradeDetails)
					{
						foreach (var rateEntryPk in tradeDetail.Value.RelatedRateEntryPks)
						{
							actual.Add(Tuple.Create<ZGuid, ZGuid, string, string>(rateEntryPk, tradeLane.Key.ProductPk, tradeDetail.Key.Mode, tradeDetail.Key.Type));
						}
					}
				}

				AssertContainsExactElementsInAnyOrder(expected, actual);
			}
		}

		public void TestQuotations_Forwarding_AllSynchedModesAndTypesAreValid()
		{
			var quote = Factory.New<Quote>();
			var rateCategoriesToTest = new[] { RatingConstants.RateCategory.AIR, RatingConstants.RateCategory.FCL, RatingConstants.RateCategory.LCL, RatingConstants.RateCategory.SCO, RatingConstants.RateCategory.SNC };

			foreach (var category in rateCategoriesToTest)
			{
				var rateEntry = quote.EntryCollections[category].LazyLoadingCollection.AddNew();
				foreach (ICodeDescription mode in rateEntry.Lookups.TransportModes)
				{
					TestQuotations_Forwarding_SynchedModeAndTypeIsValid(category, mode.Code);
				}
			}
		}

		void TestQuotations_Forwarding_SynchedModeAndTypeIsValid(string quoteCategory, string quoteMode)
		{
			var orgPk = ZGuid.NewZGuid();

			var insertSql = @"
DECLARE @Company UNIQUEIDENTIFIER = (SELECT TOP 1 GC_PK FROM dbo.GlbCompany)
DECLARE @Quotation UNIQUEIDENTIFIER = NEWID();
DECLARE @RateEntry UNIQUEIDENTIFIER = NEWID();

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@Org, 'XXX' + @TI_RateCategory + @TI_Mode)

INSERT INTO dbo.RatingHeader
	(TH_PK, TH_OH, TH_RateType, TH_QuoteNumber, TH_QuoteDate, TH_IsCancelled, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser, TH_SystemCreateTimeUtc, TH_SystemCreateUser)
VALUES
	(@Quotation, @Org, 'QTE', 'QTE00001', '2015-1-1', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.RateEntry
	(TI_PK, TI_TH, TI_GC_Publisher, TI_OriginLRC, TI_DestinationLRC, TI_RateCategory, TI_Mode, TI_RateStartDate, TI_RateEndDate, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser, TI_SystemCreateTimeUtc, TI_SystemCreateUser)
VALUES
	(@RateEntry, @Quotation, @Company, 'AUSYD', '', @TI_RateCategory, @TI_Mode, '2014-1-1',       NULL, '2015-1-1', '~BP', GetUtcDate(), '~BP')
";

			using (var command = TestConnection.Command(insertSql))
			{
				command.AddParameter("@Org", SqlDbType.UniqueIdentifier, orgPk.ToGuid());
				command.AddParameter("@TI_RateCategory", SqlDbType.VarChar, quoteCategory);
				command.AddParameter("@TI_Mode", SqlDbType.VarChar, quoteMode);
				command.ExecuteNonQuery();
			}

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(TestConnection);

			using (var provider = new NonCachedTradeLinesSummaryProvider(TestConnection))
			{
				var org = Factory.Load<OrgHeader>(orgPk);
				var synchroniser = new TradeLinesSynchroniser(org.PK);
				synchroniser.Execute(provider, new ZDate(2014, 6, 1), new ZDate(2015, 6, 1));
				org.SalesCollection.Load();

				CombineAssertions("Quote TI_RateCategory:" + quoteCategory + "  TI_Mode:" + quoteMode, () =>
				{
					var sales = (OrgSales)org.SalesCollection.Single();
					var tradeDetail = (OrgTradeDetail)sales.TradeDetails.Single();
					tradeDetail.Validation.ValidatePA_TradeMode();
					tradeDetail.Validation.ValidatePA_TradeType();

					AssertNoErrors("Actual PA_TradeMode:" + tradeDetail.PA_TradeMode, tradeDetail.PA_TradeModeInfo);
					AssertNoErrors("Actual PA_TradeMode:" + tradeDetail.PA_TradeMode + "  PA_TradeType:" + tradeDetail.PA_TradeType, tradeDetail.PA_TradeTypeInfo);
				});
			}
		}

		public void TestQuotations_OriginAndDestination()
		{
			var insertSql = string.Format(CultureInfo.InvariantCulture, @"
DECLARE @Company UNIQUEIDENTIFIER = (SELECT TOP 1 GC_PK FROM dbo.GlbCompany)
DECLARE @BRKChargeCode UNIQUEIDENTIFIER = 'EE97ECFF-ECCE-4B31-9592-ECA3F80BAEF1';
DECLARE @BONChargeCode UNIQUEIDENTIFIER = 'EE97ECFF-ECCE-4B31-9592-ECA3F80BAEF2';
DECLARE @OBRChargeCode UNIQUEIDENTIFIER = 'EE97ECFF-ECCE-4B31-9592-ECA3F80BAEF3';
DECLARE @OBOChargeCode UNIQUEIDENTIFIER = 'EE97ECFF-ECCE-4B31-9592-ECA3F80BAEF4';
DECLARE @OTHChargeCode UNIQUEIDENTIFIER = 'EE97ECFF-ECCE-4B31-9592-ECA3F80BAEF5';

DECLARE @Org UNIQUEIDENTIFIER = 'E8C9E76C-C515-405B-A5AC-177170F2644B';
DECLARE @Quotation UNIQUEIDENTIFIER = '43FB1BBE-8070-4376-8BB5-1006A18419F2';

INSERT INTO dbo.AccChargeCode
	(AC_PK, AC_Code, AC_ChargeGroup, AC_GC)
VALUES
	(@BRKChargeCode, '{0}', '{0}', @Company),
	(@BONChargeCode, '{1}', '{1}', @Company),
	(@OBRChargeCode, '{2}', '{2}', @Company),
	(@OBOChargeCode, '{3}', '{3}', @Company),
	(@OTHChargeCode, 'OTH', 'NGC', @Company)

INSERT INTO dbo.OrgHeader
	(OH_PK, OH_Code)
VALUES
	(@Org, 'XXXX')

INSERT INTO dbo.RatingHeader
	(TH_PK, TH_OH, TH_RateType, TH_QuoteNumber, TH_QuoteDate, TH_IsCancelled, TH_GC, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser, TH_SystemCreateTimeUtc, TH_SystemCreateUser)
VALUES
	(@Quotation, @Org, 'QTE', 'QTE00001', '2015-1-1', 0, @Company, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.RateEntry
	(TI_PK, TI_TH, TI_GC_Publisher, TI_OriginLRC, TI_DestinationLRC, TI_RateCategory, TI_Mode, TI_RateStartDate, TI_RateEndDate, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser, TI_SystemCreateTimeUtc, TI_SystemCreateUser)
VALUES
	('90385327-E485-477E-9F6C-9B225FCE6571', @Quotation, @Company, 'AUSYD', 'AUMEL', 'DST', 'ULD', '2014-1-1',       NULL, '2015-1-1', '~BP', GetUtcDate(), '~BP'), -- DST Entry with both BRK and non-BRK charge codes
	('90385327-E485-477E-9F6C-9B225FCE6572', @Quotation, @Company,      '', 'AUSYD', 'DST', 'FCL', '2014-1-1',       NULL, '2015-1-1', '~BP', GetUtcDate(), '~BP'), -- DST Entry with only BRK charge code

	('90385327-E485-477E-9F6C-9B225FCE6573', @Quotation, @Company, 'AUMEL', 'AUSYD', 'ORG', 'FRO', '2014-1-1',  NULL, '2015-1-1', '~BP', GetUtcDate(), '~BP'), -- ORG Entry with both BRK and non-BRK charge codes
	('90385327-E485-477E-9F6C-9B225FCE6574', @Quotation, @Company, 'AUSYD',      '', 'ORG', 'FRA', '2014-1-1',  NULL, '2015-1-1', '~BP', GetUtcDate(), '~BP'), -- ORG Entry with only BRK charge code

	('90385327-E485-477E-9F6C-9B225FCE6575', @Quotation, @Company, 'AUSYD', '', 'ORG', 'MAI', '2014-1-1',       NULL, '2015-1-1', '~BP', GetUtcDate(), '~BP'), -- ORG Entry without any charge codes
	('90385327-E485-477E-9F6C-9B225FCE6576', @Quotation, @Company, '', 'AUSYD', 'DST', 'AIR', '2014-1-1',       NULL, '2015-1-1', '~BP', GetUtcDate(), '~BP')  -- DST Entry without any charge codes

INSERT INTO dbo.RateLines
	(TL_PK, TL_TI, TL_AC, TL_RX_NKCurrency, TL_SystemLastEditTimeUtc, TL_SystemLastEditUser, TL_SystemCreateTimeUtc, TL_SystemCreateUser)
VALUES
	('B1D0CFB5-B091-4450-9948-757D234FC0A1', '90385327-E485-477E-9F6C-9B225FCE6571', @BRKChargeCode, 'USD', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('B1D0CFB5-B091-4450-9948-757D234FC0A2', '90385327-E485-477E-9F6C-9B225FCE6571', @OTHChargeCode, 'USD', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

	('B1D0CFB5-B091-4450-9948-757D234FC0A3', '90385327-E485-477E-9F6C-9B225FCE6572', @BONChargeCode, 'USD', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

	('B1D0CFB5-B091-4450-9948-757D234FC0A5', '90385327-E485-477E-9F6C-9B225FCE6573', @OBRChargeCode, 'USD', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('B1D0CFB5-B091-4450-9948-757D234FC0A6', '90385327-E485-477E-9F6C-9B225FCE6573', @OTHChargeCode, 'USD', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

	('B1D0CFB5-B091-4450-9948-757D234FC0A7', '90385327-E485-477E-9F6C-9B225FCE6574', @OBOChargeCode, 'USD', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

", ChargeCodeGroupList.Codes.Brokerage, ChargeCodeGroupList.Codes.BrokerageOnly, ChargeCodeGroupList.Codes.OriginBrokerage, ChargeCodeGroupList.Codes.OriginBrokerageOnly);

			TestConnection.ExecuteNonQuery(insertSql);
			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(TestConnection);

			using (var provider = new NonCachedTradeLinesSummaryProvider(TestConnection))
			{
				var brkPk = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "BRK").PK;
				var shpPk = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP").PK;
				var ausydPk = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD").PK;
				var aumelPk = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL").PK;

				var expected = new Tuple<ZGuid, ZGuid, ZGuid, ZGuid, string, string>[]
				{
					Tuple.Create(new ZGuid("90385327-E485-477E-9F6C-9B225FCE6571"), shpPk, ausydPk, aumelPk, "AIR", "ULD"),
					Tuple.Create(new ZGuid("90385327-E485-477E-9F6C-9B225FCE6571"), brkPk, aumelPk, ZGuid.Empty, "AIR", "IMP"),

					Tuple.Create(new ZGuid("90385327-E485-477E-9F6C-9B225FCE6572"), brkPk, ausydPk, ZGuid.Empty, "SEA", "IMP"),

					Tuple.Create(new ZGuid("90385327-E485-477E-9F6C-9B225FCE6573"), shpPk, aumelPk, ausydPk, "ROA", "FTL"),
					Tuple.Create(new ZGuid("90385327-E485-477E-9F6C-9B225FCE6573"), brkPk, aumelPk, ZGuid.Empty, "ROA", "EXP"),

					Tuple.Create(new ZGuid("90385327-E485-477E-9F6C-9B225FCE6574"), brkPk, ausydPk, ZGuid.Empty, "RAI", "EXP"),

					Tuple.Create(new ZGuid("90385327-E485-477E-9F6C-9B225FCE6575"), shpPk, ausydPk, ZGuid.Empty, "COU", ""),

					Tuple.Create(new ZGuid("90385327-E485-477E-9F6C-9B225FCE6576"), shpPk, ZGuid.Empty, ausydPk, "AIR", ""),
				};

				var org = Factory.Load<OrgHeader>(Guid.Parse("E8C9E76C-C515-405B-A5AC-177170F2644B"));
				var summary = provider.GetForOrg(org, new ZDate(2014, 6, 1), new ZDate(2015, 6, 1));

				var actual = new List<Tuple<ZGuid, ZGuid, ZGuid, ZGuid, string, string>>();
				foreach (var tradeLane in summary.ProspectValues)
				{
					foreach (var tradeDetail in tradeLane.Value.TradeDetails)
					{
						foreach (var rateEntryPk in tradeDetail.Value.RelatedRateEntryPks)
						{
							actual.Add(Tuple.Create<ZGuid, ZGuid, ZGuid, ZGuid, string, string>(rateEntryPk, tradeLane.Key.ProductPk, tradeLane.Key.OriginPk, tradeLane.Key.DestinationPk, tradeDetail.Key.Mode, tradeDetail.Key.Type));
						}
					}
				}

				AssertContainsExactElementsInAnyOrder(expected, actual);
			}
		}

		public void TestQuotations_OriginAndDestination_ModeAndTypes()
		{
			AssertQuotations_OriginAndDestination_ModeAndType("AIR", false, "AIR", "");
			AssertQuotations_OriginAndDestination_ModeAndType("ULD", false, "AIR", "ULD");
			AssertQuotations_OriginAndDestination_ModeAndType("LSE", false, "AIR", "LSE");

			AssertQuotations_OriginAndDestination_ModeAndType("SEA", false, "SEA", "");
			AssertQuotations_OriginAndDestination_ModeAndType("LCL", false, "SEA", "LCL");
			AssertQuotations_OriginAndDestination_ModeAndType("FCL", false, "SEA", "FCL");

			AssertQuotations_OriginAndDestination_ModeAndType("ROA", false, "ROA", "");
			AssertQuotations_OriginAndDestination_ModeAndType("LRO", false, "ROA", "LTL");
			AssertQuotations_OriginAndDestination_ModeAndType("FRO", false, "ROA", "FTL");
			AssertQuotations_OriginAndDestination_ModeAndType("FTL", false, "ROA", "FTL");

			AssertQuotations_OriginAndDestination_ModeAndType("RAI", false, "RAI", "");
			AssertQuotations_OriginAndDestination_ModeAndType("LRA", false, "RAI", "LCL");
			AssertQuotations_OriginAndDestination_ModeAndType("FRA", false, "RAI", "FCL");
			AssertQuotations_OriginAndDestination_ModeAndType("FWL", false, "RAI", "FCL");

			AssertQuotations_OriginAndDestination_ModeAndType("MAI", false, "COU", "");

			AssertQuotations_OriginAndDestination_ModeAndType("AIR", true, "AIR", "IMP");
			AssertQuotations_OriginAndDestination_ModeAndType("ULD", true, "AIR", "IMP");
			AssertQuotations_OriginAndDestination_ModeAndType("LSE", true, "AIR", "IMP");

			AssertQuotations_OriginAndDestination_ModeAndType("SEA", true, "SEA", "IMP");
			AssertQuotations_OriginAndDestination_ModeAndType("LCL", true, "SEA", "IMP");
			AssertQuotations_OriginAndDestination_ModeAndType("FCL", true, "SEA", "IMP");

			AssertQuotations_OriginAndDestination_ModeAndType("ROA", true, "ROA", "IMP");
			AssertQuotations_OriginAndDestination_ModeAndType("LRO", true, "ROA", "IMP");
			AssertQuotations_OriginAndDestination_ModeAndType("FRO", true, "ROA", "IMP");
			AssertQuotations_OriginAndDestination_ModeAndType("FTL", true, "ROA", "IMP");

			AssertQuotations_OriginAndDestination_ModeAndType("RAI", true, "RAI", "IMP");
			AssertQuotations_OriginAndDestination_ModeAndType("LRA", true, "RAI", "IMP");
			AssertQuotations_OriginAndDestination_ModeAndType("FRA", true, "RAI", "IMP");
			AssertQuotations_OriginAndDestination_ModeAndType("FWL", true, "RAI", "IMP");

			AssertQuotations_OriginAndDestination_ModeAndType("MAI", true, "MAI", "IMP");
		}

		void AssertQuotations_OriginAndDestination_ModeAndType(string quoteMode, bool withBrokerageLine, string expectedMode, string expectedType)
		{
			var org = CreateNewOrg_WithQuotationForOriginDestionation(quoteMode, withBrokerageLine);
			using (var provider = new NonCachedTradeLinesSummaryProvider(TestConnection))
			{
				var synchroniser = new TradeLinesSynchroniser(org.PK);
				synchroniser.Execute(provider, new ZDate(2014, 6, 1), new ZDate(2015, 6, 1));
				org.SalesCollection.Load();

				CombineAssertions("Quote IsBrokerage:" + (withBrokerageLine ? "Y" : "N") + "  TI_Mode:" + quoteMode, () =>
				{
					var sales = (OrgSales)org.SalesCollection.Single();
					var tradeDetail = (OrgTradeDetail)sales.TradeDetails.Single();

					AssertEquals("PA_TradeMode", expectedMode, tradeDetail.PA_TradeMode);
					AssertEquals("PA_TradeType", expectedType, tradeDetail.PA_TradeType);
				});
			}
		}

		public void TestQuotations_OriginAndDestination_AllSynchedModesAndTypesAreValid()
		{
			var quote = Factory.New<Quote>();
			var rateEntry = quote.ORGRateEntriesForBinding.AddNew();
			foreach (ICodeDescription transportModeCodeDescription in rateEntry.Lookups.TransportModes)
			{
				var mode = transportModeCodeDescription.Code;
				if (mode != Core.Constants.RateMode.ALL) // tested separately in TestQuotations_OriginAndDestination_SyncAllModeDoesNotCreateTradeDetail
				{
					TestQuotations_OriginAndDestination_SynchedModeAndTypeIsValid(mode, false);
				}

				TestQuotations_OriginAndDestination_SynchedModeAndTypeIsValid(mode, true);
			}
		}

		void TestQuotations_OriginAndDestination_SynchedModeAndTypeIsValid(string quoteMode, bool withBrokerageLine)
		{
			var org = CreateNewOrg_WithQuotationForOriginDestionation(quoteMode, withBrokerageLine);

			using (var provider = new NonCachedTradeLinesSummaryProvider(TestConnection))
			{
				var synchroniser = new TradeLinesSynchroniser(org.PK);
				synchroniser.Execute(provider, new ZDate(2014, 6, 1), new ZDate(2015, 6, 1));
				org.SalesCollection.Load();

				CombineAssertions("Quote IsBrokerage:" + (withBrokerageLine ? "Y" : "N") + "  TI_Mode:" + quoteMode, () =>
				{
					var sales = (OrgSales)org.SalesCollection.Single();
					var tradeDetail = (OrgTradeDetail)sales.TradeDetails.Single();
					tradeDetail.Validation.ValidatePA_TradeMode();
					tradeDetail.Validation.ValidatePA_TradeType();

					AssertNoErrors("Actual PA_TradeMode:" + tradeDetail.PA_TradeMode, tradeDetail.PA_TradeModeInfo);
					AssertNoErrors("Actual PA_TradeMode:" + tradeDetail.PA_TradeMode + "  PA_TradeType:" + tradeDetail.PA_TradeType, tradeDetail.PA_TradeTypeInfo);
				});
			}
		}

		public void TestQuotations_OriginAndDestination_SyncAllModeDoesNotCreateTradeDetail()
		{
			var org = CreateNewOrg_WithQuotationForOriginDestionation(Core.Constants.RateMode.ALL, false);
			using (var provider = new NonCachedTradeLinesSummaryProvider(TestConnection))
			{
				var synchroniser = new TradeLinesSynchroniser(org.PK);
				synchroniser.Execute(provider, new ZDate(2014, 6, 1), new ZDate(2015, 6, 1));
				org.SalesCollection.Load();
			}

			var sales = (OrgSales)org.SalesCollection.Single();
			AssertEquals("Should not create any trade details, because mode is mandatory for forwarding, and 'ALL' is not a valid estimate value mode for forwarding", 0, sales.TradeDetails.Count);
		}

		public void TestQuotations_Warehouse()
		{
			var productA = Guid.NewGuid();
			var insertSql = string.Format(CultureInfo.InvariantCulture, @"
DECLARE @Company UNIQUEIDENTIFIER = (SELECT TOP 1 GC_PK FROM dbo.GlbCompany)
DECLARE @AusydBranch UNIQUEIDENTIFIER = '8ACE694F-63CC-4D33-8FB5-1C94DE7C6521'
DECLARE @UsnycBranch UNIQUEIDENTIFIER = '8ACE694F-63CC-4D33-8FB5-1C94DE7C6522'
DECLARE @Org UNIQUEIDENTIFIER = 'E8C9E76C-C515-405B-A5AC-177170F2644B'
DECLARE @Address UNIQUEIDENTIFIER = (SELECT TOP 1 OA_PK FROM dbo.OrgAddress)
DECLARE @Quotation UNIQUEIDENTIFIER = '43FB1BBE-8070-4376-8BB5-1006A18419F2';

DECLARE @WOUChargeCode UNIQUEIDENTIFIER = 'EE97ECFF-ECCE-4B31-9592-ECA3F80BAEF1'
DECLARE @WINChargeCode UNIQUEIDENTIFIER = 'EE97ECFF-ECCE-4B31-9592-ECA3F80BAEF2'
DECLARE @WSTChargeCode UNIQUEIDENTIFIER = 'EE97ECFF-ECCE-4B31-9592-ECA3F80BAEF3'
DECLARE @OTHChargeCode UNIQUEIDENTIFIER = 'EE97ECFF-ECCE-4B31-9592-ECA3F80BAEF4'

DECLARE @AusydWhs UNIQUEIDENTIFIER = '6DEE0741-57CF-4402-A183-CC839DE17B21'
DECLARE @USnycWhs UNIQUEIDENTIFIER = '6DEE0741-57CF-4402-A183-CC839DE17B22'

DECLARE @ProductA UNIQUEIDENTIFIER = '{0}'
DECLARE @ProductB UNIQUEIDENTIFIER = '1E917120-944C-4A34-8DBC-D7D00381F9FB'
DECLARE @LocationType UNIQUEIDENTIFIER = '16C9FD62-730A-42ED-A20E-699606FFF360'

INSERT INTO dbo.GlbBranch
	(GB_PK, GB_Code, GB_RL_NKHomePort, GB_GC)
VALUES
	(@AusydBranch, 'XXX', 'AUSYD', @Company),
	(@UsnycBranch, 'YYY', 'USNYC', @Company)

INSERT INTO dbo.AccChargeCode
	(AC_PK, AC_Code, AC_ChargeGroup, AC_GC)
VALUES
	(@WOUChargeCode, 'WOU', '{1}', @Company),
	(@WINChargeCode, 'WIN', '{2}', @Company),
	(@WSTChargeCode, 'WST', '{3}', @Company),
	(@OTHChargeCode, 'OTH', 'NGC', @Company)

INSERT INTO dbo.WhsWarehouse
	(WW_PK, WW_WarehouseCode, WW_GB_RelatedCompanyBranch, WW_OA_WarehouseAddress, WW_DefaultOutboundDockDoor, WW_DefaultInboundDockDoor, WW_WLT_DefaultLocationType, WW_SystemCreateTimeUtc, WW_SystemCreateUser, WW_SystemLastEditTimeUtc, WW_SystemLastEditUser)
VALUES
	(@AusydWhs, 'AU', @AusydBranch, @Address, NEWID(), NEWID(), @LocationType, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(@UsnycWhs, 'US', @UsnycBranch, @Address, NEWID(), NEWID(), @LocationType, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.OrgHeader
	(OH_PK, OH_Code)
VALUES
	(@Org, 'XXXX')

INSERT INTO dbo.OrgSupplierPart
	(OP_PK, OP_PartNum)
VALUES
	(@ProductA, REPLACE(CONVERT(char(36), @ProductA), '-', '')),
	(@ProductB, REPLACE(CONVERT(char(36), @ProductB), '-', ''))

INSERT INTO dbo.RatingHeader
	(TH_PK, TH_OH, TH_RateType, TH_QuoteNumber, TH_QuoteDate, TH_IsCancelled, TH_GC, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser, TH_SystemCreateTimeUtc, TH_SystemCreateUser)
VALUES
	(@Quotation, @Org, 'QTE', 'QTE00001', '2015-1-1', 0, @Company, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.RateEntry
	(TI_PK, TI_TH, TI_GC_Publisher, TI_ParentID, TI_ParentTableCode, TI_RateCategory, TI_Mode, TI_RateStartDate, TI_RateEndDate, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser, TI_SystemCreateTimeUtc, TI_SystemCreateUser)
VALUES
	('650B489F-E809-43C9-816B-A61A6A021691', @Quotation, @Company, @AusydWhs, 'WW', 'WHS', 'ALL', '2014-1-1',       NULL, '2015-1-1', '~BP', GetUtcDate(), '~BP'),
	('650B489F-E809-43C9-816B-A61A6A021692', @Quotation, @Company, @USnycWhs, 'WW', 'WHS', 'ALL', '2014-1-1', '2015-1-1', '2015-1-1', '~BP', GetUtcDate(), '~BP'),
	('F7F69905-FE0F-40F0-A42F-598EA0873A1B', @Quotation, @Company, @USnycWhs, 'WW', 'WHS', 'ALL', '2010-1-1', '2011-1-1', '2015-1-1', '~BP', GetUtcDate(), '~BP')  -- Expired earlier than the @From sync date - should not be synched

INSERT INTO dbo.RateLines
	(TL_PK, TL_TI, TL_AC, TL_ParentID, TL_ParentTableCode, TL_RX_NKCurrency, TL_SystemLastEditTimeUtc, TL_SystemLastEditUser, TL_SystemCreateTimeUtc, TL_SystemCreateUser)
VALUES
	('B1D0CFB5-B091-4450-9948-757D234FC0A1', '650B489F-E809-43C9-816B-A61A6A021691', @WOUChargeCode, @ProductA, 'OP', 'AUD', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('B1D0CFB5-B091-4450-9948-757D234FC0A2', '650B489F-E809-43C9-816B-A61A6A021691', @WINChargeCode, @ProductB, 'OP', 'NZD', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('B1D0CFB5-B091-4450-9948-757D234FC0A3', '650B489F-E809-43C9-816B-A61A6A021692', @WSTChargeCode, @ProductA, 'OP', 'USD', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('B1D0CFB5-B091-4450-9948-757D234FC0A4', '650B489F-E809-43C9-816B-A61A6A021692', @OTHChargeCode, @ProductB, 'OP', 'USD', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('B1D0CFB5-B091-4450-9948-757D234FC0A6', 'F7F69905-FE0F-40F0-A42F-598EA0873A1B', @WINChargeCode, @ProductB, 'OP', 'NZD', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
",
	productA,
	ChargeCodeGroupList.Codes.WHSOutwards,
	ChargeCodeGroupList.Codes.WHSInwards,
	ChargeCodeGroupList.Codes.WHSStorage);

			TestConnection.ExecuteNonQuery(insertSql);
			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(TestConnection);

			using (var provider = new NonCachedTradeLinesSummaryProvider(TestConnection))
			{
				var whsProductPk = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "WHS").PK;
				var ausydUnlocoPk = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD").PK;
				var usnycUnlocoPk = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USNYC").PK;
				var ausydWhsPk = Factory.Load<IWhsWarehouse>(new ZGuid("6DEE0741-57CF-4402-A183-CC839DE17B21")).PK;
				var usnycWhsPk = Factory.Load<IWhsWarehouse>(new ZGuid("6DEE0741-57CF-4402-A183-CC839DE17B22")).PK;
				var productAPk = productA;
				var productBPk = Factory.Load<OrgSupplierPart>(new ZGuid("1E917120-944C-4A34-8DBC-D7D00381F9FB")).PK;

				var expected = new Tuple<ZGuid, ZGuid, string, ZGuid, string, ZGuid>[]
				{
					Tuple.Create<ZGuid, ZGuid, string, ZGuid, string, ZGuid>(new ZGuid("650B489F-E809-43C9-816B-A61A6A021691"), ausydUnlocoPk, "RL", ausydWhsPk, OrgSalesWarehouseServiceTypesList.Codes.Orders, productAPk),
					Tuple.Create(new ZGuid("650B489F-E809-43C9-816B-A61A6A021691"), ausydUnlocoPk, "RL", ausydWhsPk, OrgSalesWarehouseServiceTypesList.Codes.Receipts, productBPk),
					Tuple.Create<ZGuid, ZGuid, string, ZGuid, string, ZGuid>(new ZGuid("650B489F-E809-43C9-816B-A61A6A021692"), usnycUnlocoPk, "RL", usnycWhsPk, OrgSalesWarehouseServiceTypesList.Codes.Storage, productAPk)
				};

				var org = Factory.Load<OrgHeader>(Guid.Parse("E8C9E76C-C515-405B-A5AC-177170F2644B"));
				var summary = provider.GetForOrg(org, new ZDate(2014, 6, 1), new ZDate(2015, 6, 1));

				var actual = new List<Tuple<ZGuid, ZGuid, string, ZGuid, string, ZGuid>>();
				foreach (var tradeLane in summary.ProspectValues.Where(x => x.Key.ProductPk == whsProductPk))
				{
					foreach (var tradeDetail in tradeLane.Value.TradeDetails)
					{
						foreach (var rateEntryPk in tradeDetail.Value.RelatedRateEntryPks)
						{
							actual.Add(Tuple.Create<ZGuid, ZGuid, string, ZGuid, string, ZGuid>(
								rateEntryPk,
								tradeLane.Key.OriginPk,
								tradeLane.Key.OriginTableCode,
								tradeLane.Key.WarehousePk,
								tradeLane.Key.Service,
								tradeDetail.Key.SupplierPartPk));
						}
					}
				}

				AssertContainsExactElementsInAnyOrder(expected, actual);
			}
		}

		OrgHeader CreateNewOrg_WithQuotationForOriginDestionation(string quoteMode, bool withBrokerageLine)
		{
			var orgPk = ZGuid.NewZGuid();

			var insertSql = @"
DECLARE @Company UNIQUEIDENTIFIER = (SELECT TOP 1 GC_PK FROM dbo.GlbCompany)
DECLARE @Quotation UNIQUEIDENTIFIER = NEWID();
DECLARE @RateEntry UNIQUEIDENTIFIER = NEWID();
DECLARE @Code VARCHAR(MAX) = 'XXX" + (withBrokerageLine ? "B" : "") + @"' + @TI_Mode

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@Org, @Code)

INSERT INTO dbo.RatingHeader
	(TH_PK, TH_OH, TH_RateType, TH_QuoteNumber, TH_QuoteDate, TH_IsCancelled, TH_GC, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser, TH_SystemCreateTimeUtc, TH_SystemCreateUser)
VALUES
	(@Quotation, @Org, 'QTE', 'QTE00001', '2015-1-1', 0, @Company, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.RateEntry
	(TI_PK, TI_TH, TI_GC_Publisher, TI_OriginLRC, TI_DestinationLRC, TI_RateCategory, TI_Mode, TI_RateStartDate, TI_RateEndDate, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser, TI_SystemCreateTimeUtc, TI_SystemCreateUser)
VALUES
	(@RateEntry, @Quotation, @Company, '', 'AUSYD', 'DST', @TI_Mode, '2014-1-1',       NULL, '2015-1-1', '~BP', GetUtcDate(), '~BP')
";

			if (withBrokerageLine)
			{
				insertSql += @"

DECLARE @BRKChargeCode UNIQUEIDENTIFIER = NEWID();

INSERT INTO dbo.AccChargeCode
	(AC_PK, AC_Code, AC_ChargeGroup, AC_GC)
VALUES
	(@BRKChargeCode, @Code, 'BRK', @Company)

INSERT INTO dbo.RateLines
	(TL_PK, TL_TI, TL_AC, TL_RX_NKCurrency, TL_SystemLastEditTimeUtc, TL_SystemLastEditUser, TL_SystemCreateTimeUtc, TL_SystemCreateUser)
VALUES
	(NEWID(), @RateEntry, @BRKChargeCode, 'USD', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			}

			using (var command = TestConnection.Command(insertSql))
			{
				command.AddParameter("@Org", SqlDbType.UniqueIdentifier, orgPk.ToGuid());
				command.AddParameter("@TI_Mode", SqlDbType.VarChar, quoteMode);
				command.ExecuteNonQuery();
			}

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(TestConnection);

			return Factory.Load<OrgHeader>(orgPk);
		}

		#endregion

		#region Accrued Revenue / Cost

		[TestDate(2019, 03, 18)]
		public void TestAccruedRevenueAndCost_Forwarding()
		{
			var chargeCode = GetNonDSBChargeCode(Factory);

			var shp = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var nzakl = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL");

			var company1 = GlbCompany.CurrentCompany;
			company1.GC_Code = "CMA";

			var periodTestHelper = new AccountingPeriodTestHelper(Factory);
			periodTestHelper.SetupSinglePeriod(201903, ZDate.Today.AddYears(-6), ZDate.Today.AddYears(1), company1.PK);

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_Code = "LOCALORG1";
			localClient.OH_RL_NKClosestPort = "AUBNE";

			var salesCollection = new OrgSalesCollection(localClient);
			var helper = new OrgSalesCollectionTestHelper(salesCollection, Factory);

			var consignor = helper.GetNewOrg("TESTORG1");
			var consignee = helper.GetNewOrg("TESTORG2");

			var shipment1 = helper.CreateShipment("AUSYD", "NZAKL", "SEA", "FCL", consignee.PK, consignor.PK, new ZDate(2017, 10, 10));
			var job1 = CreateJobHeader((IJobHeaderParent)shipment1, localClient, company1.PK);
			CreateCharge(Factory, job1, localClient, chargeCode, TransactionLineTypes.Revenue, 100m);
			CreateCharge(Factory, job1, localClient, chargeCode, TransactionLineTypes.Cost, 50m);
			CreateCharge(Factory, job1, localClient, chargeCode, TransactionLineTypes.WIP, 200m);
			CreateCharge(Factory, job1, localClient, chargeCode, TransactionLineTypes.Accrual, 80m);
			job1.JH_A_JCL = ZDate.Today;

			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var provider = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var periodStart = new ZDate(2017, 10, 1);

				// Local Client
				var summary = provider.GetForOrg(localClient, new ZDate(2017, 10, 1), new ZDate(2017, 11, 1));

				var actual = summary.ActualValues;
				AssertEquals(1, actual.Count);

				var tradeLane = actual.First();
				var tradeDetail = tradeLane.Value.TradeDetails.First();
				var jobPeriod = tradeDetail.Value.TradePeriods[new TradePeriodKey(periodStart, localClient.PK, true)];

				var jobCompany1Value = jobPeriod.TradeValues[new TradeValueBreakdownKey("AUD", company1.PK)];
				AssertEquals(300m, jobCompany1Value.Revenue);
				AssertEquals(130m, jobCompany1Value.Cost);

				var period = tradeDetail.Value.TradePeriods[new TradePeriodKey(periodStart, localClient.PK, false)];
				var company1Value = period.TradeValues[new TradeValueBreakdownKey("AUD", company1.PK)];
				AssertEquals(100m, company1Value.Revenue);
				AssertEquals(50m, company1Value.Cost);
			}
		}

		#endregion

		#region Revenue and Cost Overflow for Forwarding

		[TestDate(2019, 03, 18)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestRevenueAndCostOverflow_Forwarding()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "CMA";
			Factory.Save();

			SetMaxAllowedAmountRegistry(company1.PK);

			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_GC = company1.PK;
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_GC = company1.PK;

			var periodTestHelper = new AccountingPeriodTestHelper(Factory);
			periodTestHelper.SetupSinglePeriod(201903, ZDate.Today.AddYears(-6), ZDate.Today.AddYears(1), company1.PK);

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_Code = "LOCALORG1";
			localClient.OH_RL_NKClosestPort = "AUBNE";
			localClient.CompanyData.OB_IsDebtor = true;

			var salesCollection = new OrgSalesCollection(localClient);
			var helper = new OrgSalesCollectionTestHelper(salesCollection, Factory);

			var consignor = helper.GetNewOrg("TESTORG1");
			var consignee = helper.GetNewOrg("TESTORG2");

			var shipment1 = helper.CreateShipment("AUSYD", "NZAKL", "SEA", "FCL", consignee.PK, consignor.PK, new ZDate(2017, 10, 10));
			var job1 = CreateJobHeader((IJobHeaderParent)shipment1, localClient, company1.PK);
			job1.JH_A_JCL = ZDate.Today;

			CreateARAndAPCharge(Factory, job1, localClient, chargeCode1, 900000000000000m);

			Factory.Save();
			CreateARAndAPCharge(Factory, job1, localClient, chargeCode2, 900000000000000m);

			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var provider = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var periodStart = new ZDate(2017, 10, 1);

				// Local Client
				var summary = provider.GetForOrg(localClient, new ZDate(2017, 10, 1), new ZDate(2017, 11, 1));

				var actual = summary.ActualValues;
				AssertEquals(1, actual.Count);

				var tradeLane = actual.First();
				var tradeDetail = tradeLane.Value.TradeDetails.First();
				var jobPeriod = tradeDetail.Value.TradePeriods[new TradePeriodKey(periodStart, localClient.PK, true)];

				var jobCompany1Value = jobPeriod.TradeValues[new TradeValueBreakdownKey("AUD", company1.PK)];
				AssertEquals(900000000000000m, jobCompany1Value.Revenue);
				AssertEquals(-900000000000000m, jobCompany1Value.Cost);
			}
		}

		#endregion

		#region Revenue and Cost Overflow for Customs Brokerage

		[TestDate(2017, 9, 1)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestRevenueAndCostOverflow_CustomsBrokerage()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "CMA";
			Factory.Save();

			SetMaxAllowedAmountRegistry(company1.PK);

			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_GC = company1.PK;
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_GC = company1.PK;

			var periodTestHelper = new AccountingPeriodTestHelper(Factory);
			periodTestHelper.SetupSinglePeriod(201903, ZDate.Today.AddYears(-6), ZDate.Today.AddYears(1), company1.PK);

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_Code = "LOCALORG1";
			localClient.OH_RL_NKClosestPort = "AUBNE";
			localClient.CompanyData.OB_IsDebtor = true;

			var salesCollection = new OrgSalesCollection(localClient);
			var helper = new OrgSalesCollectionTestHelper(salesCollection, Factory);

			var org1 = helper.GetNewOrg("TESTORG1");
			var org2 = helper.GetNewOrg("TESTORG2");

			var declaration = helper.CreateDeclaration("AUSYD", "NZAKL", org1.PK, org2.PK, "SEA");
			var job1 = CreateJobHeader((IJobHeaderParent)declaration, localClient, company1.PK);
			((BusinessObject)declaration)[JobDeclarationSchema.Constants.JE_ExportDate] = ZDate.Today.AddDays(2);
			((BusinessObject)declaration)[JobDeclarationSchema.Constants.JE_DateAtOrigin] = ZDate.Today.AddDays(2);

			CreateARAndAPCharge(Factory, job1, localClient, chargeCode1, 310000000000000m);
			CreateARAndAPCharge(Factory, job1, localClient, chargeCode2, 310000000000000m);

			job1.JH_A_JCL = ZDate.Today;
			job1.JH_JobLocalReference = "a";

			Factory.Save();

			var declaration2 = helper.CreateDeclaration("AUSYD", "NZAKL", org1.PK, org2.PK, "SEA");
			((BusinessObject)declaration2)[JobDeclarationSchema.Constants.JE_TotalVolume] = 3;
			((BusinessObject)declaration2)[JobDeclarationSchema.Constants.JE_TotalVolumeUnit] = "M3";
			((BusinessObject)declaration2)[JobDeclarationSchema.Constants.JE_ExportDate] = ZDate.Today.AddDays(4);
			((BusinessObject)declaration2)[JobDeclarationSchema.Constants.JE_DateAtOrigin] = ZDate.Today.AddDays(4);

			var job2 = CreateJobHeader((IJobHeaderParent)declaration2, localClient, company1.PK);

			CreateARAndAPCharge(Factory, job2, localClient, chargeCode1, 310000000000000m); // 900000000000000m);

			job2.JH_A_JCL = ZDate.Today.AddDays(1);
			job2.JH_JobLocalReference = "b";

			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var provider = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var periodStart = new ZDate(2017, 9, 1);

				// Local Client
				var summary = provider.GetForOrg(org1, new ZDate(2017, 8, 1), new ZDate(2017, 10, 1));

				var actual = summary.ActualValues;
				AssertEquals(1, actual.Count);

				var tradeLane = actual.First();
				var tradeDetail = tradeLane.Value.TradeDetails.First();
				var jobPeriod = tradeDetail.Value.TradePeriods[new TradePeriodKey(periodStart, org1.PK, true)];

				var jobCompany1Value = jobPeriod.TradeValues[new TradeValueBreakdownKey("AUD", company1.PK)];
				AssertEquals(900000000000000m, jobCompany1Value.Revenue);
				AssertEquals(-900000000000000m, jobCompany1Value.Cost);
			}
		}

		#endregion

		#region Revenue and Cost Overflow for Port Transport

		[TestDate(2017, 10, 5)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestRevenueAndCostOverflow_PortTransport()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "CMA";
			Factory.Save();

			SetMaxAllowedAmountRegistry(company1.PK);

			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_GC = company1.PK;
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_GC = company1.PK;

			var periodTestHelper = new AccountingPeriodTestHelper(Factory);
			periodTestHelper.SetupSinglePeriod(201903, ZDate.Today.AddYears(-6), ZDate.Today.AddYears(1), company1.PK);

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_Code = "LOCALORG1";
			localClient.OH_RL_NKClosestPort = "AUBNE";
			localClient.CompanyData.OB_IsDebtor = true;

			var fromOrg = Factory.NewWithValidTestData<OrgHeader>();
			fromOrg.OH_Code = "FROMORG1";
			fromOrg.MainAddress.Address1 = "Pickup Address";
			fromOrg.MainAddress.OA_City = "ABBEYWOOD";
			fromOrg.MainAddress.OA_State = "QLD";

			var toOrg = Factory.NewWithValidTestData<OrgHeader>();
			toOrg.OH_Code = "DELIVERORG1";
			toOrg.MainAddress.Address1 = "Delivery Address";
			toOrg.MainAddress.OA_City = "ABERDEEN";
			toOrg.MainAddress.OA_State = "NSW";

			var portTransportForAnotherCompany = (BusinessObject)Factory.New<Freight.LocalCartage.Integration.ICommonCartage>();
			portTransportForAnotherCompany.FillWithValidTestData();
			portTransportForAnotherCompany["JJ_E3_NKJobType"] = "ESFF";
			portTransportForAnotherCompany["JJ_ContainerMode"] = "CNT";

			var job1 = CreateJobHeader((IJobHeaderParent)portTransportForAnotherCompany, localClient, company1.PK);
			job1.JH_A_JCL = ZDate.Today;

			CreateARAndAPCharge(Factory, job1, localClient, chargeCode1, 900000000000000m);
			Factory.Save();

			CreateARAndAPCharge(Factory, job1, localClient, chargeCode2, 900000000000000m);

			Factory.Save();

			string updateSql = $@"declare @jobCartagePk uniqueidentifier = '{portTransportForAnotherCompany.PK}'
UPDATE dbo.JobDocAddress
SET
	E2_OA_Address = '{fromOrg.MainAddress.PK}',
	E2_SystemLastEditTimeUtc = GETUTCDATE(),
	E2_SystemLastEditUser = '~BP'
WHERE
	E2_ParentID = @jobCartagePk and E2_AddressType = 'LCF'
UPDATE dbo.JobDocAddress
SET
	E2_OA_Address = '{toOrg.MainAddress.PK}',
	E2_SystemLastEditTimeUtc = GETUTCDATE(),
	E2_SystemLastEditUser = '~BP'
WHERE
	E2_ParentID = @jobCartagePk and E2_AddressType = 'LCT'";

			TestConnection.ExecuteNonQuery(updateSql);

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var provider = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var periodStart = new ZDate(2017, 10, 1);

				// Local Client
				var summary = provider.GetForOrg(localClient, new ZDate(2017, 10, 1), new ZDate(2017, 11, 1));

				var actual = summary.ActualValues;
				AssertEquals(1, actual.Count);

				var tradeLane = actual.First();
				var tradeDetail = tradeLane.Value.TradeDetails.First();
				var jobPeriod = tradeDetail.Value.TradePeriods[new TradePeriodKey(periodStart, localClient.PK, true)];

				var jobCompany1Value = jobPeriod.TradeValues[new TradeValueBreakdownKey("AUD", company1.PK)];
				AssertEquals(900000000000000m, jobCompany1Value.Revenue);
				AssertEquals(-900000000000000m, jobCompany1Value.Cost);
			}
		}

		#endregion

		void SetMaxAllowedAmountRegistry(ZGuid companyPK)
		{
			var registryValue = new MaximumAllowedTransactionAmount()
			{
				MaximumAllowedHeaderAmount = 1200000000000000M,
				MaximumAllowedLineAmount = 1200000000000000M
			};

			AccountingMasterFilesRegistry.Instance.SystemDefinedMaximumAllowedTransactionAmount.SetValue(companyPK.ToGuid(), Guid.Empty, Guid.Empty, registryValue);
			AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount.SetValue(companyPK.ToGuid(), Guid.Empty, Guid.Empty, registryValue);
		}

		[TestDate(2017, 9, 1)]
		public void TestIncludeDSBCharges_Customs()
		{
			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_Code = "TESTORGXXX";
			localClient.OH_RL_NKClosestPort = "AUBNE";

			var periodTestHelper = new AccountingPeriodTestHelper(Factory);
			periodTestHelper.SetupSinglePeriod(201903, ZDate.Today.AddYears(-6), ZDate.Today.AddYears(1), Env.CurrentCompanyPK);

			var collection = new OrgSalesCollection(localClient);
			var collectionHelper = new OrgSalesCollectionTestHelper(collection, Factory);

			var org1 = collectionHelper.GetNewOrg("TESTORG1");
			var org2 = collectionHelper.GetNewOrg("TESTORG2");

			var uslax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");

			var dsbCharge = GetDSBChargeCode(Factory);
			var declaration = collectionHelper.CreateDeclaration("AUSYD", "USLAX", org1.PK, org2.PK, "SEA");
			var job = CreateJobHeader((IJobHeaderParent)declaration, localClient, Env.CurrentCompanyPK);
			CreateCharge(Factory, job, localClient, dsbCharge, TransactionLineTypes.Revenue);
			job.JH_A_JCL = ZDate.Today;

			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var provider = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var summary = provider.GetForOrg(org1, new ZDate(2017, 8, 1), new ZDate(2017, 10, 1));
				var tradeLane = summary.ActualValues.First();
				var tradeDetail = tradeLane.Value.TradeDetails.First();

				var jobPeriod = tradeDetail.Value.TradePeriods[new TradePeriodKey(new ZDate(2017, 9, 1), org1.PK, true)];

				Assert("DSB charge should be included", jobPeriod.TradeValues.ContainsKey(new TradeValueBreakdownKey("AUD", Env.CurrentCompanyPK)));
			}
		}

		[TestDate(2017, 9, 1)]
		public void TestIncludeDSBCharges_Shipment()
		{
			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_Code = "TESTORGXXX";
			localClient.OH_RL_NKClosestPort = "AUBNE";

			var periodTestHelper = new AccountingPeriodTestHelper(Factory);
			periodTestHelper.SetupSinglePeriod(201903, ZDate.Today.AddYears(-6), ZDate.Today.AddYears(1), Env.CurrentCompanyPK);

			var collection = new OrgSalesCollection(localClient);
			var collectionHelper = new OrgSalesCollectionTestHelper(collection, Factory);

			var org1 = collectionHelper.GetNewOrg("TESTORG1");
			var org2 = collectionHelper.GetNewOrg("TESTORG2");

			var uslax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");

			var nonDsbCharge = GetDSBChargeCode(Factory);
			var shipment1 = collectionHelper.CreateShipment("AUSYD", "USLAX", "SEA", "FCL", org1.PK, org2.PK, new ZDate(2017, 9, 1));
			var job11 = CreateJobHeader((IJobHeaderParent)shipment1, localClient, Env.CurrentCompanyPK);
			CreateCharge(Factory, job11, localClient, nonDsbCharge, TransactionLineTypes.Revenue);
			job11.JH_A_JCL = ZDate.Today;

			Factory.Save();

			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			using (var provider = new NonCachedTradeLinesSummaryProvider(Db.Connection))
			{
				var summary = provider.GetForOrg(org1, new ZDate(2017, 8, 1), new ZDate(2017, 10, 1));
				var tradeLane = summary.ActualValues.First();
				var tradeDetail = tradeLane.Value.TradeDetails.First();

				var jobPeriod = tradeDetail.Value.TradePeriods[new TradePeriodKey(new ZDate(2017, 9, 1), org1.PK, true)];

				Assert("DSB charge should be included", jobPeriod.TradeValues.ContainsKey(new TradeValueBreakdownKey("AUD", Env.CurrentCompanyPK)));
			}
		}

		internal static AccChargeCode GetNonDSBChargeCode(BusinessObjectFactory factory)
		{
			var nonDsbChargeQuery = new ZQuery();
			nonDsbChargeQuery.AddToFilter(AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.NotEqual, "DSB");
			nonDsbChargeQuery.AddToFilter(AccChargeCodeSchema.AC_AG_RevenueAccount, SQLComparisonOperator.NotEqual, null);
			nonDsbChargeQuery.OrderBy = AccChargeCodeSchema.Constants.AC_Code;
			return factory.LoadTop1<AccChargeCode>(nonDsbChargeQuery);
		}

		internal static AccChargeCode GetDSBChargeCode(BusinessObjectFactory factory)
		{
			var dsbChargeQuery = new ZQuery();
			dsbChargeQuery.AddToFilter(AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.Equal, "DSB");
			dsbChargeQuery.AddToFilter(AccChargeCodeSchema.AC_AG_RevenueAccount, SQLComparisonOperator.NotEqual, null);
			dsbChargeQuery.OrderBy = AccChargeCodeSchema.Constants.AC_Code;
			return factory.LoadTop1<AccChargeCode>(dsbChargeQuery);
		}

		internal static JobHeader CreateJobHeader(IJobHeaderParent parent, OrgHeader localClient, ZGuid companyPK)
		{
			var job = new JobHeader.Loader(parent).TryCreate();
			job.JH_GC = companyPK;
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
			return job;
		}

		internal static JobHeader CreateJobHeader(IJobHeaderParent parent, OrgHeader localClient, OrgHeader overseasAgent, ZGuid companyPK)
		{
			var job = new JobHeader.Loader(parent).TryCreate();
			job.JH_GC = companyPK;
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
			job.JH_OA_AgentCollectAddr = overseasAgent.MainAddress.PK;
			return job;
		}

		internal static void CreateCharge(BusinessObjectFactory factory, JobHeader job, OrgHeader localClient, AccChargeCode chargeCode, ZString lineType, decimal amount = 100m)
		{
			var transaction = factory.NewWithValidTestData<AccTransactionHeader>();
			if (lineType == TransactionLineTypes.Revenue || lineType == TransactionLineTypes.WIP)
			{
				transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
				transaction.AH_TransactionType = TransactionTypes.Invoice;
			}
			else if (lineType == TransactionLineTypes.Cost || lineType == TransactionLineTypes.Accrual)
			{
				transaction.AH_Ledger = LedgerTypes.AccountsPayable;
				transaction.AH_TransactionType = TransactionTypes.CreditNote;
			}

			transaction.AH_JH = job.PK;
			transaction.AH_GC = job.JH_GC;
			transaction.AH_OH = localClient.PK;

			var line = factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AH = transaction.PK;
			line.AL_JH = job.PK;
			line.AL_AC = chargeCode.PK;
			line.AL_AG = chargeCode.AC_AG_RevenueAccount;
			line.AL_GC = job.JH_GC;
			line.AL_RX_NKTransactionCurrency = "AUD";
			line.AL_LineType = lineType;
			line.AL_LineAmount = amount;
			line.AL_OSAmount = amount;
			line.AL_PostDate = ZDate.Today;
			if (lineType != TransactionLineTypes.Accrual && lineType != TransactionLineTypes.WIP)
			{
				line.AL_ReverseDate = ZDate.Today;
			}
			line.AL_OH = localClient.PK;

			if (lineType == TransactionLineTypes.Revenue || lineType == TransactionLineTypes.Cost)
			{
				line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			}
			else
			{
				line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
			}

			var jobCharge = factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = job.PK;
			jobCharge.JR_GC = job.JH_GC;

			if (lineType == TransactionLineTypes.Revenue || lineType == TransactionLineTypes.WIP)
			{
				jobCharge.JR_LocalSellAmt = amount;
				jobCharge.JR_OSSellAmt = amount;
				jobCharge.JR_AL_ARLine = line.PK;
			}
			else if (lineType == TransactionLineTypes.Cost || lineType == TransactionLineTypes.Accrual)
			{
				jobCharge.JR_LocalCostAmt = -amount;
				jobCharge.JR_OSCostAmt = -amount;
				jobCharge.JR_AL_APLine = line.PK;
			}
		}

		internal static void CreateARAndAPCharge(BusinessObjectFactory factory, JobHeader job, OrgHeader localClient, AccChargeCode chargeCode, decimal amount)
		{
			var lineAP = factory.NewWithValidTestData<AccTransactionLines>();

			lineAP.AL_JH = job.PK;
			lineAP.AL_AC = chargeCode.PK;
			lineAP.AL_AG = chargeCode.AC_AG_RevenueAccount;
			lineAP.AL_GB = job.JH_GB;
			lineAP.AL_GE = job.JH_GE;
			lineAP.AL_GC = job.JH_GC;
			lineAP.AL_RX_NKTransactionCurrency = "AUD";
			lineAP.AL_LineType = TransactionLineTypes.Accrual;
			lineAP.AL_LineAmount = amount;
			lineAP.AL_OSAmount = amount;
			lineAP.AL_PostDate = ZDate.Today;
			lineAP.AL_PostToGL = "N";
			lineAP.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			lineAP.AL_ReverseDate = ZDateTime.Empty;

			var lineAR = factory.NewWithValidTestData<AccTransactionLines>();

			lineAR.AL_JH = job.PK;
			lineAR.AL_AC = chargeCode.PK;
			lineAR.AL_AG = chargeCode.AC_AG_RevenueAccount;
			lineAR.AL_GB = job.JH_GB;
			lineAR.AL_GE = job.JH_GE;
			lineAR.AL_GC = job.JH_GC;
			lineAR.AL_RX_NKTransactionCurrency = "AUD";
			lineAR.AL_LineType = TransactionLineTypes.WIP;
			lineAR.AL_LineAmount = -amount;
			lineAR.AL_OSAmount = -amount;
			lineAR.AL_PostDate = ZDate.Today;
			lineAR.AL_PostToGL = "N";
			lineAR.AL_OH = localClient.PK;
			lineAR.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			lineAR.AL_ReverseDate = ZDateTime.Empty;

			var jobCharge = factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = job.PK;
			jobCharge.JR_AC = chargeCode.PK;
			jobCharge.JR_GB = job.JH_GB;
			jobCharge.JR_GE = job.JH_GE;
			jobCharge.JR_GC = job.JH_GC;
			jobCharge.JR_CostRatingOverride = true;
			jobCharge.JR_LineType = "BTH";
			jobCharge.JR_LocalSellAmt = amount;
			jobCharge.JR_OSSellAmt = amount;
			jobCharge.JR_LocalCostAmt = amount;
			jobCharge.JR_OSCostAmt = amount;
			jobCharge.JR_RX_NKCostCurrency = "AUD";
			jobCharge.JR_AL_ARLine = lineAR.PK;
			jobCharge.JR_AL_APLine = lineAP.PK;
			jobCharge.JR_InvoiceType = "FIN";
			jobCharge.JR_OH_SellAccount = localClient.PK;
		}
	}
}
