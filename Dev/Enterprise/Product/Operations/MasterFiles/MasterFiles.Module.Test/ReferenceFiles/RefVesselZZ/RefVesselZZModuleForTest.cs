using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class RefVesselZZModuleForTest : RefVesselZZModule
	{
		public RefVesselZZModuleForTest()
		{
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
