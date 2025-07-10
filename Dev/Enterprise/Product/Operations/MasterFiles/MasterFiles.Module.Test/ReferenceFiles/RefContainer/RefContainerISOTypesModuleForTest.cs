using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class RefContainerISOTypesModuleForTest : RefContainerISOTypesModule
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

		public new MenuItem[] GetNewAdditionalMenuItems()
		{
			return base.GetNewAdditionalMenuItems();
		}

		public void PerformSearch()
		{
			base.PerformSearch();
		}

		public new bool CanReloadWithFilter(BusinessObjectFactory newFactory, BusinessObject selectedBusinessObject, ZQuery filter)
		{
			return base.CanReloadWithFilter(newFactory, selectedBusinessObject, filter);
		}

		public ZDisplayGrid GetGrid => base.Grid;
	}
}
