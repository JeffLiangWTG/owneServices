using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Module.Testing
{
	[TestedType(typeof(JobDeclarationFilterLookups))]
	public sealed class JobDeclarationFilterLookupsTest : TestCaseWithFactory
	{
		public void TestEntryStatusList()
		{
			var entryStatusCodeList = Factory.GetCachedValue<EntryStatusCodeList>();
			AssertEquals(entryStatusCodeList.CodesAsString, lookups.EntryStatusList().CodesAsString);
		}

		public void TestDeclarationTypeList()
		{
			new TestTWCreator(Factory).CreateRefCusCodeForDeclarationType();
			AssertEquals("B1, B2, B6, B8, B9, D1, D2, D5, D7, D8, F1, F2, F3, F4, F5, G1, G2, G3, G5, G7, L1", lookups.DeclarationTypeList().CodesAsString);
		}

		public void TestMessageStatusList()
		{
			AssertEquals("ACC, ACE, ACG, ACO, AWC, AWE, AWG, AWO, ERC, ERE, ERG, ERO, NOT, UNK", lookups.MessageStatusList().CodesAsString);
		}

		public void TestClearanceStatusList()
		{
			AssertEquals("C1, C2, C3M, C3X, NOT", lookups.ClearanceStatusList().CodesAsString);
		}

		public void TestAgencyResponseCodeList()
		{
			var rejectionReasonList = new CPT_016_RejectionReasons();
			AssertEquals(rejectionReasonList.CodesAsString, lookups.AgencyResponseCodeList().CodesAsString);
			Assert("Should contain the code 'C86'.", rejectionReasonList.ContainsCode("C86"));
		}

		public void TestRequiredFormalitiesCodeList()
		{
			AssertEquals(new CTP_017_ErrorDocumentsOrRequiredFormalities().CodesAsString, lookups.RequiredFormalitiesCodeList().CodesAsString);
		}

		public void TestClearanceCodeList()
		{
			AssertEquals(new CPT_025_ExtraCondition().CodesAsString, lookups.ClearanceCodeList().CodesAsString);
		}

		JobDeclarationFilterLookups lookups
		{
			get
			{
				if (lookupsCached == null)
				{
					lookupsCached = new JobDeclarationFilterLookups(new JobDeclarationFilterBusinessObject());
				}
				return lookupsCached;
			}
		}

		JobDeclarationFilterLookups lookupsCached;
	}
}
