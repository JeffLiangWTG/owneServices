using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class ModuleEntryHeaderCollectionTest<T> : ActiveBusinessObjectCollectionTestCase<T> where T : ModuleEntryHeaderCollection
	{
		public virtual void TestEntryHeaderCollection()
		{
			CusEntryHeader entry1 = GetNewElementToAddToTheCollection() as CusEntryHeader;
			CusEntryHeader entry2 = GetNewElementToAddToTheCollection() as CusEntryHeader;
			CusEntryHeader entry3 = GetNewElementToAddToTheCollection() as CusEntryHeader;
			CusEntryHeader entry4 = GetNewElementToAddToTheCollection() as CusEntryHeader;

			entry1.CH_BGMReference = "";
			entry3.CH_BGMReference = "";

			entry2.CH_BGMReference = "TEST1";

			var entryNumber1 = CusEntryNumber.New(entry3, CusEntryNumberTypes.Standard.MovementReferenceNumber, entry3.CountryCode);

			var entryNumber2 = CusEntryNumber.New(entry4, CusEntryNumberTypes.Standard.MovementReferenceNumber, entry4.CountryCode);
			entryNumber2.CE_EntryNum = "TEST_MRN";

			Factory.Save();

			var entries = GetCollectionToTest();

			Assert("Shouldn't contain entry 1", !entries.Contains(entry1));
			Assert("Should contain entry 2", entries.Contains(entry2));
			Assert("Shouldn't contain entry 3", !entries.Contains(entry3));
			Assert("Should contain entry 4", entries.Contains(entry4));
		}

		public void TestDontLoadEntriesCreatedInOtherCountriesOrCompanies()
		{
			GlbCompany usCompany = Factory.New<GlbCompany>();
			usCompany.FillWithValidTestData();
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			GlbCompany usCompany2 = Factory.New<GlbCompany>();
			usCompany2.FillWithValidTestData();
			usCompany2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			GlbBranch usBranch = usCompany.Branches.AddNew();
			usBranch.FillWithValidTestData();
			usBranch.GB_RL_NKHomePort = "USCHI";

			GlbBranch usBranch2 = usCompany2.Branches.AddNew();
			usBranch2.FillWithValidTestData();
			usBranch2.GB_RL_NKHomePort = "USCHI";

			GlbCompany.CurrentCompany.SetCountry("US");
			BaseJobDeclaration usDeclaration = Factory.New<BaseJobDeclaration>();
			usDeclaration.JE_GB = usBranch.PK;
			CusEntryHeader usEntry = usDeclaration.CustomsEntryHeaders.AddNew();

			BaseJobDeclaration usDeclaration2 = Factory.New<BaseJobDeclaration>();
			usDeclaration2.JE_GB = usBranch2.PK;
			CusEntryHeader usEntry2 = usDeclaration2.CustomsEntryHeaders.AddNew();

			GlbCompany auCompany = Factory.New<GlbCompany>();
			auCompany.FillWithValidTestData();
			auCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			GlbBranch auBranch = auCompany.Branches.AddNew();
			auBranch.FillWithValidTestData();
			auBranch.GB_RL_NKHomePort = "AUSYD";

			GlbCompany.CurrentCompany.SetCountry("AU");
			BaseJobDeclaration auDeclaration = Factory.New<BaseJobDeclaration>();
			auDeclaration.JE_GB = auBranch.PK;
			CusEntryHeader auEntry = auDeclaration.CustomsEntryHeaders.AddNew();
			auEntry.CH_BGMReference = "AU_REF";

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			ModuleEntryHeaderCollection usEntries = new ModuleEntryHeaderCollection(factory2, usCompany, null);
			AssertEquals(1, usEntries.Count);
			AssertEquals(usEntry.PK, usEntries[0].PK);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			CusEntryHeader result = declaration.CustomsEntryHeaders.AddNew();
			result.CH_BGMReference = "TEST_REF";
			Factory.Save();
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			try
			{
				//GlbCompany.CurrentCompany.SetCountry changes GC_RN_NKCountryCode, and needs to be saved to db as JobDeclarationFilter(DBOnlyQuery) is performed in FilterObject.
				GlbCompany.CurrentCompany.Factory.Save();
			}
			finally
			{
				((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = false;
			}
		}
	}
}
