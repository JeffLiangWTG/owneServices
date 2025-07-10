using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class ProcessTasksModuleForTest : ProcessTasksModule
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

		public BusinessObject[] selectedElements;

		protected override BusinessObject[] GetSelectedElements()
		{
			return selectedElements;
		}

		protected override BusinessObject[] SelectedBusinessObjects => selectedElements;
	}
}
