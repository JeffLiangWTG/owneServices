using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.Business.Test
{
	class ProcessTaskIterationLinkValidationTest : BusinessObjectValidationTestCase
	{
		public void TestLinkType()
		{
			var link = Factory.New<ProcessTaskIterationLink>();
			link.P9I_LinkType = "";
			AssertHasError(link.P9I_LinkTypeInfo, "Please enter a Link Type.");

			link.P9I_LinkType = "BOO";
			AssertHasError(link.P9I_LinkTypeInfo, "Enter a valid Link Type.");

			link.P9I_LinkType = IterationLinkTypeList.Codes.PassedContainmentBarrier;
			AssertNoErrors(link.P9I_LinkTypeInfo);

			AssertExceptionThrown<MaxLengthExceededException>(() => link.P9I_IterationReason = "Reason Code too Long");
			ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
		}
	}
}
