using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class RefPremisesGateCodeModuleForTest : RefPremisesGateCodeModule
	{
		public IFilterControl GetNewFilterControlForTest()
		{
			return GetNewFilterControl();
		}

		public IBusinessObjectCollection GetNewGridCollectionForTest()
		{
			return GetNewGridCollection();
		}

		public FilterBusinessObject GetNewFilterBusinessObjectForTest()
		{
			return GetNewFilterBusinessObject();
		}

		public new ZController OrganisationController
		{
			get { return base.OrganisationController; }
		}
	}
}
