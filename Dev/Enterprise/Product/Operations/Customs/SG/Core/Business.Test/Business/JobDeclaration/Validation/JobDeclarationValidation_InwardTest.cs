using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public abstract class JobDeclarationValidation_InwardTest : CUSDECValidationTest
	{
		public void TestImporter()
		{
			Validation.ValidateJE_OH_Importer();
			AssertEquals(true, Declaration.JE_OH_ImporterInfo.HasMessageErrors());
			OrgHeader importer = Factory.New<OrgHeader>();
			Declaration.JE_OH_Importer = importer.PK;
			Validation.ValidateJE_OH_Importer();
			AssertEquals(true, Declaration.JE_OH_ImporterInfo.HasMessageErrors());
			importer.OH_IsConsignee = true;
			Validation.ValidateJE_OH_Importer();
			AssertEquals(true, Declaration.JE_OH_ImporterInfo.HasMessageErrors());
			importer.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "TEST");
			importer.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
			Validation.ValidateJE_OH_Importer();
			AssertEquals(false, Declaration.JE_OH_ImporterInfo.HasMessageErrors());
		}
	}
}
