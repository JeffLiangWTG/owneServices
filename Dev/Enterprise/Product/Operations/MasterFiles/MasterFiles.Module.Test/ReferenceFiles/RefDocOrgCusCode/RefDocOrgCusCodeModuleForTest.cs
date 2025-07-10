using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class RefDocOrgCusCodeModuleForTest : RefDocOrgCusCodeModule
	{
		public RefDocOrgCusCodeModuleForTest() { }

		public IFilterControl NewFilterControl => GetNewFilterControl();

		public IBusinessObjectCollection NewGridCollection => GetNewGridCollection();

		public FilterBusinessObject NewFilterBusinessObject => GetNewFilterBusinessObject();
	}
}
