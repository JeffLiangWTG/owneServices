using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(BillOfLading))]
	internal class BillOfLadingWorkflowProviderTest : AgencyWorkflowProviderTest<BillOfLading, BillOfLadingProcessTaskCollection>
	{
		#region Implementation
		protected override ZString ExpectedWorkflowType
		{
			get
			{
				return WorkflowDescriptors.BillOfLadingWorkflowDescriptorCode;
			}
		}
		#endregion
	}
}
