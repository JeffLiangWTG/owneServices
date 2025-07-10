using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusEntryLine.Loader))]
	sealed class CusEntryLineLoaderTest : LoaderTestCase
	{
		public void TestFindByDeclarationLineNumberAndEntryFilerCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ImportEntryNumber = "12351231";
			var entryNumber = declaration.ENSEntryNumber;
			var inBondNumber = CusEntryNumber.New(declaration, CusEntryHeaderMessageTypeList.Codes.InBond, Core.Constants.CountryCodes.UnitedStates);
			inBondNumber.CE_EntryNum = "12351231";
			var inBondHeader = declaration.CustomsEntryHeaders.AddNew();
			inBondHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			var inBondLine1 = inBondHeader.MergedLines.AddNew();
			inBondLine1.CL_LineNumber = 1;
			var inBondLine2 = inBondHeader.MergedLines.AddNew();
			inBondLine2.CL_LineNumber = 2;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_AddInfo = "XXX*SupLine=Y*XXX";
			var entryLine3 = entryHeader.MergedLines.AddNew();
			entryLine3.CL_LineNumber = 2;
			Factory.Save();
			var loader = new CusEntryLine.Loader(Factory);
			AssertNull(loader.FindParentByDeclarationLineNumberAndEntryFilerCode("12351231", 1, "X5J"));
			AssertNull(loader.FindParentByDeclarationLineNumberAndEntryFilerCode("12351231", 2, "X5J"));
			AssertEquals(entryLine1, loader.FindByDeclarationAndLineNumbers("12351231", CusEntryHeaderMessageTypeList.Codes.EntrySummary, 1));
			AssertEquals(entryLine2, loader.FindByDeclarationAndLineNumbers("12351231", CusEntryHeaderMessageTypeList.Codes.EntrySummary, 2));
			AssertEquals(inBondLine1, loader.FindByDeclarationAndLineNumbers("12351231", CusEntryHeaderMessageTypeList.Codes.InBond, 1));
			AssertEquals(inBondLine2, loader.FindByDeclarationAndLineNumbers("12351231", CusEntryHeaderMessageTypeList.Codes.InBond, 2));
			declaration.US_EntryFilerCode = "X5J";
			declaration.ImportEntryNumber = "12351231";
			Factory.Save();
			AssertEquals(entryLine1.PK, loader.FindParentByDeclarationLineNumberAndEntryFilerCode("12351231", 1, "X5J").PK);
			AssertEquals(entryLine3.PK, loader.FindParentByDeclarationLineNumberAndEntryFilerCode("12351231", 2, "X5J").PK);
			var entryLine4 = entryHeader.MergedLines.AddNew();
			entryLine4.CL_LineNumber = 2;
			entryLine4.US_ChildLineNum = 1;
			entryLine4.US_CL_ParentLine = entryLine3.PK;
			var entryLine5 = entryHeader.MergedLines.AddNew();
			entryLine5.CL_LineNumber = 2;
			entryLine5.US_ChildLineNum = 2;
			entryLine5.US_CL_ParentLine = entryLine3.PK;
			Factory.Save();
			AssertEquals(entryLine3.PK, loader.FindParentByDeclarationLineNumberAndEntryFilerCode("12351231", 2, "X5J").PK);
			entryLine4.CL_AdValoremTariff = "12345678";
			entryLine3.US_CL_ParentLine = entryLine3.PK;
			Factory.Save();
			AssertEquals(entryLine4.PK, loader.FindParentByDeclarationLineNumberAndEntryFilerCode("12351231", 2, "X5J").PK);
			entryLine3.US_SupLine = true;
			entryLine4.US_SupLine = true;
			Factory.Save();
			AssertEquals(entryLine5.PK, loader.FindParentByDeclarationLineNumberAndEntryFilerCode("12351231", 2, "X5J").PK);
		}

		public void TestFindByDeclarationLineNumberAndEntryFilerCodeMatchOnlyUS()
		{
			var newFactory = new BusinessObjectFactory();
			var auCompany = newFactory.New<GlbCompany>();
			auCompany.GC_Code = "!#3";
			auCompany.GC_Name = "AU DUMMY COMPANY";
			auCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			auCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var auBranch = auCompany.Branches.AddNew();
			auBranch.GB_Code = "$%#";
			auBranch.GB_BranchName = "AU DUMMY BRANCH";
			auBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var declaration = newFactory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "X5J";
			declaration.ImportEntryNumber = "12351231";
			var entryNumber = declaration.ENSEntryNumber;
			var inBondNumber = CusEntryNumber.New(declaration, CusEntryHeaderMessageTypeList.Codes.InBond, Core.Constants.CountryCodes.Australia);
			inBondNumber.CE_EntryNum = "12351231";
			var inBondHeader = declaration.CustomsEntryHeaders.AddNew();
			inBondHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			var inBondLine1 = inBondHeader.MergedLines.AddNew();
			inBondLine1.CL_LineNumber = 1;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			declaration.ENSEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			declaration.JE_GB = auBranch.PK;
			newFactory.Save();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			var loader = new CusEntryLine.Loader(new BusinessObjectFactory());
			AssertNull(loader.FindParentByDeclarationLineNumberAndEntryFilerCode("12351231", 1, "X5J"));
			AssertNull(loader.FindByDeclarationAndLineNumbers("12351231", CusEntryHeaderMessageTypeList.Codes.InBond, 1));
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new CusEntryLine.Loader(Factory);
	}
}
