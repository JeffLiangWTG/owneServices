using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusEntryNumberExtensionTest : TestCaseWithFactory
	{
		public void TestGetEntryFilerCode()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.AllocateEntryNumber("12345678");

			JobDeclaration declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration2.US_EntryFilerCode = "ABC";
			declaration2.US_EnableENS = true;
			declaration2.Invoices.AddNew();
			declaration2.InvoiceLines.AddNew();
			declaration2.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration2.DecEntryNumber = "12345678";//should be attached to a CusEntryHeader

			CusEntryNumber entryNumber = CusEntryNumber.Load(declaration, CusEntryHeaderMessageTypeList.Codes.EntrySummary, declaration.CountryCode);
			AssertEquals(declaration, entryNumber.GetJobDeclaration());
			AssertEquals("EntryFilerCode", "XJ5", entryNumber.GetEntryFilerCode());

			entryNumber = declaration2.CustomsEntryHeaders[0].CusEntryNumber;
			AssertEquals(declaration2, entryNumber.GetJobDeclaration());
			AssertEquals("EntryFilerCode", "ABC", entryNumber.GetEntryFilerCode());
		}

		public void TestGetBranch()
		{
			GlbCompany company1 = Factory.New<GlbCompany>();
			GlbBranch branch1 = company1.Branches.AddNew();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EnableINB = true;
			declaration.AllocateEntryNumber("12345678");
			CusEntryNumber entryNumber = CusEntryNumber.Load(declaration, CusEntryHeaderMessageTypeList.Codes.EntrySummary, declaration.CountryCode);
			AssertEquals(GlbBranch.CurrentBranch.PK, entryNumber.GetBranch().PK);

			declaration.JE_GB = branch1.PK;
			AssertEquals(branch1.PK, entryNumber.GetBranch().PK);
		}

		public void TestGetCompany()
		{
			GlbCompany company1 = Factory.New<GlbCompany>();
			GlbBranch branch1 = company1.Branches.AddNew();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EnableINB = true;
			declaration.AllocateEntryNumber("12345678");
			CusEntryNumber entryNumber = CusEntryNumber.Load(declaration, CusEntryHeaderMessageTypeList.Codes.EntrySummary, declaration.CountryCode);
			AssertEquals(GlbBranch.CurrentBranch.Company.PK, entryNumber.GetCompany().PK);

			declaration.JE_GB = branch1.PK;
			AssertEquals(branch1.Company.PK, entryNumber.GetCompany().PK);
		}
	}
}
