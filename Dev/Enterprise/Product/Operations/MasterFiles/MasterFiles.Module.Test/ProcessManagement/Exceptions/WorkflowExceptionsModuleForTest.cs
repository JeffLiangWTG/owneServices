using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class WorkflowExceptionsModuleForTest : WorkflowExceptionsModule
	{
		public new IFilterControl GetNewFilterControl()
		{
			return base.GetNewFilterControl();
		}

		public new IBusinessObjectCollection GetNewGridCollection()
		{
			return base.GetNewGridCollection();
		}

		public new FilterBusinessObject GetNewFilterBusinessObject()
		{
			return base.GetNewFilterBusinessObject();
		}
	}
}
