using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ImportJobDeclaration))]
	sealed public class ImportJobDeclarationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCountryToCountryCopy()
		{
			var org = new TestTWCreator(Factory).CreateOrganization();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = org.PK;

			var declarationFrom = Factory.NewWithValidTestData<JobDeclaration>();
			declarationFrom.JE_OA_DeclarantAddress = ZGuid.Empty;
			var importJobDeclaration = new ImportJobDeclaration(Factory);
			importJobDeclaration.DeclarationPK = declarationFrom.PK;
			var declarationCopied = importJobDeclaration.CreateDeclarationAgainstShipment();
			AssertEquals(org.MainAddress.PK, declarationCopied.JE_OA_DeclarantAddress);
		}
	}
}
