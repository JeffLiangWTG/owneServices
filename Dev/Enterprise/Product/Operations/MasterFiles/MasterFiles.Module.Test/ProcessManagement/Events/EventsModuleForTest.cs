using CargoWise.EntityFramework;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class EventsModuleForTest : EventsModule
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

		public ZDisplayGrid GridForTest
		{
			get { return Grid; }
		}

		public LicenceCheckpoint LicenceCheckPointCoreForTest
		{
			get { return LicenceCheckPointCore; }
		}
	}
}
