using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessWorkflowExceptionCause))]
	public class ProcessWorkflowExceptionCauseTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var type = Factory.New<ProcessWorkflowExceptionType>();
			var bizo = base.GetNewBusinessObject();
			(bizo as ProcessWorkflowExceptionCause).WEC_WET_Type = type.PK;
			return bizo;
		}
	}
}
