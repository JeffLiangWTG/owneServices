using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(CusISFHeader))]
	sealed class CusISFHeaderWorkflowProviderTest : MasterFiles.Business.Testing.WorkflowProviderTest<CusISFHeader, CusISFHeaderProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => JobInvoicingConsumerTypes.ImporterSecurityFiling.Code;
	}
}
