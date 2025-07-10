using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(JiraCustomFieldMap))]
	class JiraCustomFieldMapTest : RegistryBusinessObjectTemplateTestCase<JiraCustomFieldMap>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		public void TestValidationWorksWell()
		{
			var map = new JiraCustomFieldMap();
			var item1 = map.JiraClassificationMap.AddNew();

			item1.RunPreSaveValidation();

			BusinessObjectValidationTestCase.AssertMandatoryValidationError(item1.JiraEntityNameInfo, isExpectingError: true);
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(item1.SelectionCriterionFieldNameInfo, isExpectingError: true);
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(item1.SelectionCriterionFieldValueInfo, isExpectingError: true);
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(item1.CustomFieldIdInfo, isExpectingError: true);

			item1.JiraEntityName = "Just a bunch of nonsense words";
			item1.SelectionCriterionFieldName = "Bloogies";
			item1.CustomFieldId = "42";

			AssertNoErrors(item1.JiraEntityNameInfo);
			AssertNoErrors(item1.CustomFieldIdInfo);
			BusinessObjectValidationTestCase.AssertListValidationInvalidCodeError(item1.SelectionCriterionFieldNameInfo, isExpectingError: true);

			item1.SelectionCriterionFieldName = WorkItemSchema.Constants.WKI_ActivitySubtype;
			AssertNoErrors(item1.SelectionCriterionFieldNameInfo);

			AssertNullOrEmpty(ErrorReporter.LastMessageReported);

			AssertExceptionThrown<MaxLengthExceededException>(() => item1.SelectionCriterionFieldValue = "NONSENSE");
			AssertContains("The maximum length of 'SelectionCriterionFieldValue' has been exceeded", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			item1.SelectionCriterionFieldValue = "NON";
			AssertNoErrors(item1.SelectionCriterionFieldValueInfo);

			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
		}

		protected override JiraCustomFieldMap GetBusinessObjectToClone()
			=> ProcessMgmtTestHelper.GetDummyJiraCustomFieldMap();

		protected override JiraCustomFieldMap GetBusinessObjectToSerialise()
			=> ProcessMgmtTestHelper.GetDummyJiraCustomFieldMap();
	}
}
