using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CusEntryLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestQuantityUnitCodeList()
		{
			CombineAssertions(() =>
			{
				var list = lookups.QuantityUnitCodeList;
				AssertEquals("Codes", "AA, SM, BW, BS, CT, CR, GW, GJ, GK, GE, IU, KW, CM, ME, MM, GS, GR, KG, KK, GN, KN, MU, MW, NO, KU, PR, NX, LC, PA, RD, LI, MC, ML, LA", list.CodesAsString);
				AssertSame("Cached", lookups.QuantityUnitCodeList, list);
			});
		}

		public void TestCountableUnitCodeList()
		{
			CombineAssertions(() =>
			{
				var list = lookups.CountableUnitCodeList;
				const string codes = "AE, AM, AP, AT, BG, BL, BN, BF, BP, BR, BA, BZ, BK, CB, BI, BD, BY, BB, BT, BC, BS, BO, BV, BQ, BX, BJ, VG, VQ, VL, VY, VR, VO, BH, BE, BU, CG, CX, CI, " +
"CA, CZ, CO, CP, CT, CS, CK, CH, CC, CF, CJ, CL, CV, CR, CE, CU, CY, DJ, DP, DR, EV, FP, FI, FL, FO, FR, FD, FC, GB, GI, GZ, HR, HG, IN, IZ, JR, JY, JC, JG, JT, KG, LI, LG, LZ, MT, MX, MC, MB, " +
"MS, NS, NT, PK, PA, PL, PE, PC, PI, PH, PN, PZ, PG, PY, PT, PO, RT, RL, RG, RD, RZ, RO, SH, SA, SE, SC, ST, SM, SZ, SL, SW, SK, SD, SU, TY, TK, TC, TN, PU, TR, TS, TB, TU, TD, TZ, TO, NO, NE, " +
"VP, VA, VI, WB";
				AssertEquals("Codes", codes, list.CodesAsString);
				AssertSame("Cached", lookups.CountableUnitCodeList, list);
			});
		}

		public void TestCountryOfOrigins()
		{
			AssertType<RefCountryCollection>(lookups.CountryOfOrigins);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var cusEntryLine = Factory.New<CusEntryLine>();
			lookups = new CusEntryLineLookups(cusEntryLine);
		}
		CusEntryLineLookups lookups;
	}
}
