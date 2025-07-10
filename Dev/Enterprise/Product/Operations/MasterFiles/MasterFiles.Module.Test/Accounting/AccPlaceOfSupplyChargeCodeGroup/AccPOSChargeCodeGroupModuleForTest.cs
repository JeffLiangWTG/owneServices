using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class AccPOSChargeCodeGroupModuleForTest : AccPOSChargeCodeGroupModule
	{
		public AccPOSChargeCodeGroupModuleForTest()
		{
		}

		public new ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return base.GetNewController(selectedBusinessObject);
		}

		public IFilterControl NewFilterControl
		{
			get { return GetNewFilterControl(); }
		}

		public IBusinessObjectCollection NewGridCollection
		{
			get { return GetNewGridCollection(); }
		}

		public FilterBusinessObject NewFilterBusinessObject
		{
			get { return GetNewFilterBusinessObject(); }
		}
	}
}
