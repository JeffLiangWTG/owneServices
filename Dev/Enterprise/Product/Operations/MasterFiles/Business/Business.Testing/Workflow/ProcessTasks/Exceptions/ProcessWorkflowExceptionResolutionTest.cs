using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessWorkflowExceptionResolution))]
	public class ProcessWorkflowExceptionResolutionTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var type = Factory.New<ProcessWorkflowExceptionType>();
			var bizo = base.GetNewBusinessObject();
			(bizo as ProcessWorkflowExceptionResolution).WER_WET_Type = type.PK;
			return bizo;
		}
	}
}
