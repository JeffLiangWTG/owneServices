using CargoWise.EntityFramework.Testing;

namespace Enterprise.Workflow.Business.Test
{
	public class EDIMessageDeliveryContextSelectorLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestECL_ContextType()
		{
			var parent = Factory.New<EDIMessageDeliveryContextSelector>();
			var line = parent.Lines.AddNew();
			var line2 = parent.Lines.AddNew();

			line.ECL_ContextType = "White Space";
			AssertHasError(line.ECL_ContextTypeInfo, "Context Type cannot contain whitespace.");

			line.ECL_ContextType = "";
			AssertMandatoryValidationError(line.ECL_ContextTypeInfo, true);

			line.ECL_ContextType = "BLAH𡶌";
			AssertHasError(line.ECL_ContextTypeInfo, "Context Type only accepts Western European languages characters.");

			line.ECL_ContextType = "ValidECL_Value";
			line2.ECL_ContextType = "ValidECL_Value";
			AssertNoErrors(line.ECL_ContextTypeInfo);
			AssertHasError(line2.ECL_ContextTypeInfo, "Context Type must be unique. Duplicate: ValidECL_Value.");
		}

		public void TestECL_Value()
		{
			var parent = Factory.New<EDIMessageDeliveryContextSelector>();
			var line = parent.Lines.AddNew();
			line.ECL_Value = "BLAH";
			AssertNoErrors(line.ECL_ValueInfo);
			line.ECL_Value = "";
			AssertMandatoryValidationError(line.ECL_ValueInfo, true);
		}
	}
}
