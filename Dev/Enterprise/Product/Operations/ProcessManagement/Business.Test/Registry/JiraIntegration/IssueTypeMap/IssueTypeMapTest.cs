using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(IssueTypeMap))]
	class IssueTypeMapTest : RegistryBusinessObjectTemplateTestCase<IssueTypeMap>
	{
		public void TestValidationWorksWell()
		{
			var map = new IssueTypeMap();
			var item1 = map.JiraClassificationMap.AddNew();

			item1.RunPreSaveValidation();

			BusinessObjectValidationTestCase.AssertMandatoryValidationError(item1.JiraEntityNameInfo, isExpectingError: true);
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(item1.SelectionCriterionFieldNameInfo, isExpectingError: true);
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(item1.SelectionCriterionFieldValueInfo, isExpectingError: true);

			item1.JiraEntityName = "Just a bunch of nonsense words";
			item1.SelectionCriterionFieldName = "Bloogies";

			AssertNoErrors(item1.JiraEntityNameInfo);
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

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override IssueTypeMap GetBusinessObjectToClone()
		{
			return ProcessMgmtTestHelper.GetDummyIssueTypeMap();
		}

		protected override IssueTypeMap GetBusinessObjectToSerialise()
		{
			return ProcessMgmtTestHelper.GetDummyIssueTypeMap();
		}
	}
}
