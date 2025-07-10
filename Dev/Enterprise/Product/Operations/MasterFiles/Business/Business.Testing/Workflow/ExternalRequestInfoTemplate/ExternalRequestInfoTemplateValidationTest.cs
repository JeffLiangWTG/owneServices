using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	internal class ExternalRequestInfoTemplateValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCode()
		{
			var existedOne = Factory.New<ExternalRequestInfoTemplate>();
			existedOne.RIT_Code = "XXX";
			existedOne.RIT_Description = "XXX Desc";
			existedOne.RIT_JobType = ExternalRequestTypeJobTypes.Codes.CLH;
			Factory.Save();

			var externalRequestType = Factory.New<ExternalRequestInfoTemplate>();

			externalRequestType.RunPreSaveValidation();
			AssertHasError(externalRequestType.RIT_CodeInfo, "Please enter a value.");

			externalRequestType.RIT_Code = "XXX";
			externalRequestType.RunPreSaveValidation();
			AssertHasError(externalRequestType.RIT_CodeInfo, "Type Code XXX already exists. Specify a different Type Code.");
		}

		public void TestDescription()
		{
			var externalRequestType = Factory.New<ExternalRequestInfoTemplate>();

			externalRequestType.RunPreSaveValidation();
			AssertHasError(externalRequestType.RIT_DescriptionInfo, "Please enter a value.");
		}

		public void TestJobType()
		{
			var externalRequestType = Factory.New<ExternalRequestInfoTemplate>();

			externalRequestType.RIT_JobType = "";
			externalRequestType.RunPreSaveValidation();
			AssertHasError(externalRequestType.RIT_JobTypeInfo, "Please enter a value.");

			externalRequestType.RIT_JobType = "@#$";
			externalRequestType.RunPreSaveValidation();
			AssertHasError(externalRequestType.RIT_JobTypeInfo, "Enter a valid selection.");
		}
	}
}
