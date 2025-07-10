using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Loader = Enterprise.Customs.US.Business.CusEntryHeader.Loader;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(Loader))]
	sealed class CusEntryHeaderLoaderTest : LoaderTestCase
	{
		public void TestFindDuplicate()
		{
			JobDeclaration declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_EntryFilerCode = "XJ5";
			CusEntryHeader entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry1.EntryNumber = "1238765";
			JobDeclaration declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_EntryFilerCode = "XJ5";
			CusEntryHeader entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry2.EntryNumber = "1238765";
			Loader loader = new Loader(Factory);
			AssertEquals(entry2, loader.FindDuplicate(entry1));
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			AssertNull(loader.FindDuplicate(entry1));
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry1.EntryNumber = "2238765";
			AssertNull(loader.FindDuplicate(entry1));
			declaration1.US_EntryFilerCode = "XJ6";
			entry1.EntryNumber = "1238765";
			AssertNull(loader.FindDuplicate(entry1));
			declaration2.US_EntryFilerCode = "XJ6";
			AssertEquals(entry2, loader.FindDuplicate(entry1));
		}

		public void TestFindByEntryNumber()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.EntryNumber = "E123";
			JobDeclaration declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_EntryFilerCode = "XJ5";
			CusEntryHeader entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconEntry;
			entryHeader2.EntryNumber = "1238765";
			Loader loader = new Loader(Factory);
			AssertEquals(entryHeader, loader.FindByEntryNumberAndFilerCode(GlbCompany.CurrentCompany.PK, "E123", "XJ5", CusEntryHeaderMessageTypeList.Codes.EntrySummary));
			AssertEquals(entryHeader2, loader.FindByEntryNumberAndFilerCode(GlbCompany.CurrentCompany.PK, "1238765", "XJ5", CusEntryHeaderMessageTypeList.Codes.ReconEntry));
		}

		public void TestNonENSNumbersAreNotMatched()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.EntryNumber = "E123";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_EntryFilerCode = "XJ5";
			var addRef = declaration2.AdditionalReferenceNumbers.AddNew();
			addRef.CE_EntryType = "TES";
			addRef.CE_EntryNum = "E123";
			var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader2.EntryNumber = "1238765";
			var loader = new Loader(Factory);
			AssertEquals("Non ENS CE_EntryNum should not match", entryHeader, loader.FindByEntryNumberAndFilerCode(GlbCompany.CurrentCompany.PK, "E123", "XJ5", CusEntryHeaderMessageTypeList.Codes.EntrySummary));
			AssertEquals(entryHeader2, loader.FindByEntryNumberAndFilerCode(GlbCompany.CurrentCompany.PK, "1238765", "XJ5", CusEntryHeaderMessageTypeList.Codes.EntrySummary));
		}

		public void TestNonUSNumbersAreNotMatched()
		{
			var auCompany = Factory.New<GlbCompany>();
			auCompany.GC_Code = "!#3";
			auCompany.GC_Name = "AU DUMMY COMPANY";
			auCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			auCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var auBranch = auCompany.Branches.AddNew();
			auBranch.GB_Code = "$%#";
			auBranch.GB_BranchName = "AU DUMMY BRANCH";
			auBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.EntryNumber = "E123";
			var entryNumber1 = entryHeader.CusEntryNumber;
			entryNumber1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_EntryFilerCode = "XJ5";
			var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconEntry;
			entryHeader2.EntryNumber = "1238765";
			var entryNumber2 = entryHeader2.CusEntryNumber;
			entryNumber2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			Factory.Save();
			var loader = new Loader(new BusinessObjectFactory());
			AssertNull(loader.FindByEntryNumberAndFilerCode(GlbCompany.CurrentCompany.PK, "E123", "XJ5", CusEntryHeaderMessageTypeList.Codes.EntrySummary));
			AssertNull(loader.FindByEntryNumberAndFilerCode(GlbCompany.CurrentCompany.PK, "1238765", "XJ5", CusEntryHeaderMessageTypeList.Codes.ReconEntry));
			entryNumber1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			entryNumber2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();
			loader = new Loader(new BusinessObjectFactory());
			AssertNotNull(loader.FindByEntryNumberAndFilerCode(GlbCompany.CurrentCompany.PK, "E123", "XJ5", CusEntryHeaderMessageTypeList.Codes.EntrySummary));
			AssertNotNull(loader.FindByEntryNumberAndFilerCode(GlbCompany.CurrentCompany.PK, "1238765", "XJ5", CusEntryHeaderMessageTypeList.Codes.ReconEntry));
			declaration.JE_GB = auBranch.PK;
			declaration2.JE_GB = auBranch.PK;
			Factory.Save();
			loader = new Loader(new BusinessObjectFactory());
			AssertNull(loader.FindByEntryNumberAndFilerCode(GlbCompany.CurrentCompany.PK, "E123", "XJ5", CusEntryHeaderMessageTypeList.Codes.EntrySummary));
			AssertNull(loader.FindByEntryNumberAndFilerCode(GlbCompany.CurrentCompany.PK, "1238765", "XJ5", CusEntryHeaderMessageTypeList.Codes.ReconEntry));
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new Loader(Factory);
	}
}
