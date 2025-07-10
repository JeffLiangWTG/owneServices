using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.Module.Testing
{
	public class ValueAnalysisWarehouseOrgModuleForTest : ValueAnalysisWarehouseOrgModule, IValueAnalysisModuleForTest
	{
		public new IFilterControl GetNewFilterControl() => base.GetNewFilterControl();
		public new IBusinessObjectCollection GetNewGridCollection() => base.GetNewGridCollection();
		public new FilterBusinessObject GetNewFilterBusinessObject() => base.GetNewFilterBusinessObject();
	}
}
