using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	sealed class CusInBondMoveHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSubApplicationCodeList()
		{
			CombineAssertions(() =>
			{
				var list = lookups.SubApplicationCodeList;
				AssertEquals("Codes", "AMS, INB, PTT, SIB", list.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<SubApplicationCodeList>(), list);
			});
		}

		public void TestImporterCodes()
		{
			AssertType<ActiveOrgCusCodeCollection>(lookups.ImporterCodes);
		}

		public void TestInBondStatusList()
		{
			CombineAssertions(() =>
			{
				var list = (CodeDescriptionPairList)lookups.InBondStatusList;
				AssertEquals("Codes", "AAV, ADP, ADV, AEX, ATF, CAV, CDP, CDV, CEX, CTF, DEL, DNG, ERR", list.CodesAsString);
				AssertSame("Cached", new CusInBondMoveHeaderLookups(moveHeader).InBondStatusList, list);
			});
		}

		public void TestMessageStatusList()
		{
			CombineAssertions(() =>
			{
				var list = (CodeDescriptionPairList)lookups.MessageStatusList;
				AssertEquals("Codes", "ADD, ANG, AAV, ADP, ADV, AEX, APT, ATF, CAV, CDP, CDV, CEX, CPT, CTF, DEL, DNG, ERR, MUL, UPD, UNG", list.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<AMSBillMessageStatusList>(), list);
			});
		}

		CusInBondMoveHeader moveHeader;
		CusInBondMoveHeaderLookups lookups;
		protected override void SetUp()
		{
			base.SetUp();
			moveHeader = Factory.New<CusInBondMoveHeader>();
			lookups = new CusInBondMoveHeaderLookups(moveHeader);
		}
	}
}
