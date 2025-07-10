using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.Module
{
	public class JobDeclarationModule : Customs.Module.JobDeclarationModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new JobDeclarationFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new JobDeclarationFilterStripControl(this, GridCollection, FilterBusinessObject);

		protected internal IBusinessObjectCollection GetNewGridCollectionForTest() => GetNewGridCollection();
		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			SetGuiProviders(Factory);
			return new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
		}

		protected new internal MenuItem[] GetNewStandardMenuItems() => base.GetNewStandardMenuItems();

		protected new internal bool HasExWarehouseMenu => base.HasExWarehouseMenu;
	}
}
