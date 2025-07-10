using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USReleaseDeclarationLoaderTest : TestCaseWithFactory
	{
		public void TestGetReleaseEntryDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B00110010";
			declaration.US_ConsolACE = true;
			var invoice = declaration.Invoices.AddNew();

			var releaseDeclaration = Factory.New<JobDeclaration>();
			releaseDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			releaseDeclaration.US_EntryFilerCode = "SV9";
			releaseDeclaration.JE_DeclarationReference = "B00110020";
			var releaseEntry1 = releaseDeclaration.CustomsEntryHeaders.AddNew();
			releaseEntry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			releaseEntry1.EntryNumber = "12345678";
			Factory.Save();

			invoice.US_ReleaseEntryNumber = "SV912345678";
			var retDeclaration = USReleaseDeclarationLoader.GetReleaseDeclaration(Factory, invoice.US_ReleaseEntryNumber, GlbCompany.CurrentCompany.PK);
			AssertEquals("B00110020", retDeclaration.JE_DeclarationReference);
		}
	}
}
