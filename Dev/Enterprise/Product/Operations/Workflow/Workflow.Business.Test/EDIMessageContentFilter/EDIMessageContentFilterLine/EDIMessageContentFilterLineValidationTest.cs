using CargoWise.EntityFramework.Testing;

namespace Enterprise.Workflow.Business.Test
{
	class EDIMessageContentFilterLineValidationTest : BusinessObjectValidationTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			Line = Factory.New<EDIMessageContentFilter>().UniversalEvent.Lines.AddNew();
		}

		EDIMessageContentFilterLine Line { get; set; }

		public void TestSchemaElement()
		{
			Line.SchemaElement = "GEH";
			AssertMandatoryValidationError(Line.SchemaElementInfo, false);
			Line.SchemaElement = "";
			AssertMandatoryValidationError(Line.SchemaElementInfo, true);
		}

		public void TestSchemaElementWhiteSpaceAtEnd()
		{
			Line.SchemaElement = "GEH  ";
			AssertMandatoryValidationError(Line.SchemaElementInfo, false);
			AssertEquals("GEH", Line.SchemaElement);
		}

		public void TestSchemaElement_Duplicates()
		{
			var filter = Factory.New<EDIMessageContentFilter>().UniversalEvent;
			var element1 = filter.Lines.AddNew();
			var element2 = filter.Lines.AddNew();
			element2.SchemaElement = element1.SchemaElement = "NANG";

			AssertHasError(element2.SchemaElementInfo, EDIMessageContentFilterLineValidation.GetDuplicateMessage("NANG"));
			element2.SchemaElement = "NONG";
			AssertNoError(element2.SchemaElementInfo, EDIMessageContentFilterLineValidation.GetDuplicateMessage("NANG"));
		}

		public void TestDepth()
		{
			Line.Depth = -5;
			AssertHasError(Line.DepthInfo, EDIMessageContentFilterLineValidation.DepthMessage);

			Line.Depth = 0;
			AssertNoError(Line.DepthInfo, EDIMessageContentFilterLineValidation.DepthMessage);

			Line.Depth = 100;
			AssertNoError(Line.DepthInfo, EDIMessageContentFilterLineValidation.DepthMessage);
		}

		public void TestDataContext()
		{
			Line.SchemaElement = "SubShipmentCollection";

			Line.DataContext = "ForwardingShipment";
			AssertMandatoryValidationError(Line.DataContextInfo, false);

			Line.DataContext = string.Empty;
			AssertMandatoryValidationError(Line.DataContextInfo, true);

			Line.DataContext = "HVLVConsignment";
			AssertMandatoryValidationError(Line.DataContextInfo, false);

			Line.DataContext = "Test";
			AssertListValidationInvalidCodeError(Line.DataContextInfo, true);
		}
	}
}
