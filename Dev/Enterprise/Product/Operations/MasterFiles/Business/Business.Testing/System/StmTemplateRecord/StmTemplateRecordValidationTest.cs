using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class StmTemplateRecordValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateTemplateName()
		{
			var expectedError = "An active template record with the same template name already exists in current scope.";
			var templateName = "My New Template";
			var templateModule = "JobConsol";

			var templateRecord = Factory.New<StmTemplateRecord>();
			templateRecord.STR_TemplateName = templateName;
			templateRecord.STR_ModuleID = templateModule;

			var templateRecord_Inactive = Factory.New<StmTemplateRecord>();
			templateRecord_Inactive.STR_TemplateName = "Inactive Template";
			templateRecord_Inactive.STR_IsActive = false;
			templateRecord_Inactive.STR_ModuleID = templateModule;

			Factory.Save();

			var newTemplateRecord = Factory.New<StmTemplateRecord>();
			newTemplateRecord.STR_TemplateName = "";

			AssertNoError(
				"Pre-Condition - should not check empty template name for uniqueness",
				newTemplateRecord.STR_TemplateNameInfo,
				expectedError
			);

			newTemplateRecord.STR_ModuleID = templateModule;
			newTemplateRecord.STR_TemplateName = templateName;

			AssertHasError(
				"Should error, an active record with the same template name and module exists",
				newTemplateRecord.STR_TemplateNameInfo,
				expectedError
			);

			newTemplateRecord.STR_ModuleID = "JobShipment";
			newTemplateRecord.STR_TemplateName = templateName;

			AssertNoError(
				"Template name is duplicate but in different module should not show error",
				newTemplateRecord.STR_TemplateNameInfo,
				expectedError
			);

			newTemplateRecord.STR_ModuleID = templateModule;
			newTemplateRecord.STR_TemplateName = "Some other name";

			AssertNoError(
				"Template name which is unique should not error",
				newTemplateRecord.STR_TemplateNameInfo,
				expectedError
			);

			newTemplateRecord.STR_ModuleID = templateModule;
			newTemplateRecord.STR_TemplateName = "Inactive Template";

			AssertNoError(
				"Name and module match existing record, should not error because it is inactive",
				newTemplateRecord.STR_TemplateNameInfo,
				expectedError
			);
		}

		public void TestValidateTemplateName_OnSameRecord()
		{
			var expectedError = "An active template record with the same template name already exists in current scope.";
			var templateName = "My New Template";
			var otherTemplateName = "Other Template Name";
			var templateModule = "JobConsol";

			var templateRecord = Factory.New<StmTemplateRecord>();
			templateRecord.STR_TemplateName = templateName;
			templateRecord.STR_ModuleID = templateModule;

			var otherTemplateRecord = Factory.New<StmTemplateRecord>();
			otherTemplateRecord.STR_TemplateName = otherTemplateName;
			otherTemplateRecord.STR_ModuleID = templateModule;

			Factory.Save();

			templateRecord.STR_TemplateName = otherTemplateName;

			AssertHasError("Pre-Condition - change template name to already existing", templateRecord.STR_TemplateNameInfo, expectedError);

			templateRecord.STR_TemplateName = templateName;

			AssertNoError(
				"Change back to old name - Saved record in database is the same PK as current record, so it should not flag as duplicate",
				templateRecord.STR_TemplateNameInfo,
				expectedError
			);
		}
	}
}
