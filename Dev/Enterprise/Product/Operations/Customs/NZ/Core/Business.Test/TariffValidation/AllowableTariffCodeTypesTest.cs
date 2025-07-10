using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.TariffValidation.Testing
{
	public class AllowableTariffCodeTypesTest : TestCaseWithFactory
	{
		public void TestWithDeclaration()
		{
			Declaration.JobDeclaration declaration = Factory.New<Declaration.JobDeclaration>();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AllowableTariffCodeTypes allowableTypes = new AllowableTariffCodeTypes(declaration);
			AssertEquals("allowableTypes.Import", true, allowableTypes.Import);
			AssertEquals("allowableTypes.Export", false, allowableTypes.Export);
			AssertEquals("allowableTypes.Excise", false, allowableTypes.Excise);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			allowableTypes = new AllowableTariffCodeTypes(declaration);
			AssertEquals("allowableTypes.Import", false, allowableTypes.Import);
			AssertEquals("allowableTypes.Export", true, allowableTypes.Export);
			AssertEquals("allowableTypes.Excise", false, allowableTypes.Excise);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Excise;
			allowableTypes = new AllowableTariffCodeTypes(declaration);
			AssertEquals("allowableTypes.Import", false, allowableTypes.Import);
			AssertEquals("allowableTypes.Export", false, allowableTypes.Export);
			AssertEquals("allowableTypes.Excise", true, allowableTypes.Excise);

			declaration.JE_MessageType = ZString.Empty;
			allowableTypes = new AllowableTariffCodeTypes(declaration);
			AssertEquals("allowableTypes.Import", true, allowableTypes.Import);
			AssertEquals("allowableTypes.Export", true, allowableTypes.Export);
			AssertEquals("allowableTypes.Excise", true, allowableTypes.Excise);

			allowableTypes = new AllowableTariffCodeTypes(null);
			AssertEquals("allowableTypes.Import", true, allowableTypes.Import);
			AssertEquals("allowableTypes.Export", true, allowableTypes.Export);
			AssertEquals("allowableTypes.Excise", true, allowableTypes.Excise);
		}

		public void TestBooleanConstructor()
		{
			AllowableTariffCodeTypes allowableTypes = new AllowableTariffCodeTypes(true, false, false);
			AssertEquals("allowableTypes.Import", true, allowableTypes.Import);
			AssertEquals("allowableTypes.Export", false, allowableTypes.Export);
			AssertEquals("allowableTypes.Excise", false, allowableTypes.Excise);

			allowableTypes = new AllowableTariffCodeTypes(false, true, false);
			AssertEquals("allowableTypes.Import", false, allowableTypes.Import);
			AssertEquals("allowableTypes.Export", true, allowableTypes.Export);
			AssertEquals("allowableTypes.Excise", false, allowableTypes.Excise);

			allowableTypes = new AllowableTariffCodeTypes(false, true, true);
			AssertEquals("allowableTypes.Import", false, allowableTypes.Import);
			AssertEquals("allowableTypes.Export", true, allowableTypes.Export);
			AssertEquals("allowableTypes.Excise", true, allowableTypes.Excise);
		}

		public void TestImportOrExport()
		{
			AllowableTariffCodeTypes allowableTypes = new AllowableTariffCodeTypes(true, false, false);
			AssertEquals("allowableTypes.ImportOrExport", true, allowableTypes.ImportOrExport);

			allowableTypes = new AllowableTariffCodeTypes(false, true, false);
			AssertEquals("allowableTypes.ImportOrExport", true, allowableTypes.ImportOrExport);

			allowableTypes = new AllowableTariffCodeTypes(false, true, true);
			AssertEquals("allowableTypes.ImportOrExport", true, allowableTypes.ImportOrExport);

			allowableTypes = new AllowableTariffCodeTypes(false, false, true);
			AssertEquals("allowableTypes.ImportOrExport", false, allowableTypes.ImportOrExport);

			allowableTypes = new AllowableTariffCodeTypes(false, false, false);
			AssertEquals("allowableTypes.ImportOrExport", false, allowableTypes.ImportOrExport);
		}
	}
}
