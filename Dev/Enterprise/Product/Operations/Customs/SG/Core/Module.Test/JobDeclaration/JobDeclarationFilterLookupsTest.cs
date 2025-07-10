using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Module.Testing
{
	sealed class JobDeclarationFilterLookupsTest : TestCaseWithFactory
	{
		public void TestApplicationCodeFilterList()
		{
			var applicationCodeList = lookups.ApplicationCodeList();
			AssertSame(bizObj.Lookups.ApplicationCodeList(), applicationCodeList);
			AssertEquals("4.1, NTP", applicationCodeList.CodesAsString);
		}

		public void TestContainerModeList()
		{
			var containerModeList = lookups.ContainerModeList;
			AssertSame(bizObj.Lookups.ContainerModeList, containerModeList);
			AssertEquals("1, 2, 3, 4", containerModeList.CodesAsString);
		}

		public void TestContainerModeList_TN41()
		{
			var containerModeList_TN41 = lookups.ContainerModeList_TN41;
			AssertSame(bizObj.Lookups.ContainerModeList_TN41, containerModeList_TN41);
			AssertEquals("5, 9", containerModeList_TN41.CodesAsString);
		}

		public void TestEntryStatusList()
		{
			var entryStyleList = lookups.EntryStatusList();
			AssertSame(bizObj.Lookups.EntryStatusList(), entryStyleList);
			AssertEquals("NOT, DPD, DSN, DRJ, DER, DOK, DQY, APD, ASN, ARJ, AER, AOK, AQY, CPD, CSN, CRJ, CER, COK, CQY, RPD, RSN, RRJ, RER, ROK, RQY", entryStyleList.CodesAsString);
		}

		public void TestMessageSubTypeList()
		{
			var messageSubTypeList = lookups.MessageSubTypeList();
			AssertSame(bizObj.Lookups.MessageSubTypeList(), messageSubTypeList);
			AssertEquals("APS, BKT, BKT, BKT, BKT, BRE, DES, DNG, DRT, DUT, GST, GTR, IGM, REM, REX, SFZ, SHO, TCE, TCI, TCO, TCR, TCS, TTF, TTI", messageSubTypeList.CodesAsString);
		}

		public void TestTransportTypeList()
		{
			var transportTypeList = lookups.TransportTypeList;
			AssertSame(bizObj.Lookups.TransportTypeList, transportTypeList);
			AssertEquals("SEA, RAI, ROA, AIR, MAI, OTH", transportTypeList.CodesAsString);
		}

		public void TestPaymentPartyList()
		{
			var paymentPartyList = lookups.PaymentPartyList();
			AssertSame(bizObj.Lookups.PaymentPartyList(), paymentPartyList);
			AssertEquals("D, I", paymentPartyList.CodesAsString);
		}

		public void TestGlobalManifestStatusList()
		{
			var globalManifestStatusList = lookups.GlobalManifestStatusList;
			AssertSame(bizObj.Lookups.GlobalManifestStatusList, globalManifestStatusList);
			AssertEquals("CN, CR, IP, NS", globalManifestStatusList.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			bizObj = new JobDeclarationFilterBusinessObject();
			lookups = new JobDeclarationFilterLookups(bizObj);
		}

		JobDeclarationFilterBusinessObject bizObj;
		JobDeclarationFilterLookups lookups;
	}
}
