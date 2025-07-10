using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USAIILineAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUS_TSCAIndicatorList()
		{
			var us_TSCAIndicatorList = lookups.US_TSCAIndicatorList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "-, +", us_TSCAIndicatorList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<TSCAIndicatorList>(), us_TSCAIndicatorList);
			});
		}

		public void TestUS_QtyDiffReasonCodeList()
		{
			var us_QtyDiffReasonCodeList = lookups.US_QtyDiffReasonCodeList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "AQ, BQ, BD, DC, EV, GU, GW, LD, ZZ, PC, PD, PQ, PZ, MC, PW, PS, QP, QO, QT, SS, SC, UM, UP, WO, WD", us_QtyDiffReasonCodeList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<QtyDiffReasonList>(), us_QtyDiffReasonCodeList);
			});
		}

		public void TestUS_UnitOfMeasureList()
		{
			var us_UnitOfMeasureList = lookups.US_UnitOfMeasureList;
			var expectedCodes = "AC, BBL, BOL, CAP, CAR, CS, C, CG, CM, CY, CYG, CYK, CGM, CKG, CTN, CU, CC, CM3, CFT, M3, CYD, CUR, DEG, D, DC, DOZ, DPR, DPC, FT, FBM, FIB, GBQ, G, GR, GRL, GVW, HZ, HUN, IRC, KG, KHZ, KN, KPA, KVA, KWH, KW, LIN, LNM, L, TON, MBQ, MHZ, MPA, M, T, MC, MG, ML, MM, X, NO, JWL, FOZ, TOZ, OZ, ODE, PK, PRS, PCS, PTL, LB, PF, PFG, PFL, QTL, RPM, STN, SFT, SQI, SQ, CM2, M2, SYD, SBE, SUP, TAB, K, KM3, KM, KM2, KSB, GAL, V, W, WT, WG, WL, YD";
			CombineAssertions(() =>
			{
				AssertEquals("Codes", expectedCodes, us_UnitOfMeasureList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<ABIUnitOfMeasureList>(), us_UnitOfMeasureList);
			});
		}

		public void TestCurrencyList()
		{
			AssertType<RefCurrencyCollection>(lookups.CurrencyList);
		}

		public void TestListAttributeForUS_QtyDiffReasonCode()
		{
			AssertListAttribute(USAIILineAddInfo.Schema.US_QtyDiffRsnCode, "Lookups.US_QtyDiffReasonCodeList");
		}

		public void TestListAttributeForUS_UnitOfMeasureList()
		{
			AssertListAttribute(USAIILineAddInfo.Schema.US_InvUQDisp, "Lookups.US_UnitOfMeasureList");
		}

		USAIILineAddInfoLookups lookups;
		protected override void SetUp()
		{
			base.SetUp();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var addInfo = new USAIILineAddInfo(invoiceLine.AIILines.AddNew().B7_AddInfoDataInfo);
			lookups = new USAIILineAddInfoLookups(addInfo);
		}

		void AssertListAttribute(string fieldName, string expectedListDataSourceMember)
		{
			AssertHasCustomAttribute(typeof(USAIILineAddInfo), fieldName, false, new Predicate<ListAttribute>((ListAttribute match) => match.ListDataSourceMember == expectedListDataSourceMember));
		}
	}
}
