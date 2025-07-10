using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Environment.Module
{
	public class ProductStyleModule : ZFilterGridModule
	{
		#region Module

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.WhsConfigProductStyle);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ProductStyleFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ProductStyleFilterControl(GridCollection, (ProductStyleFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsProductStyleCollection(Factory);
		}

		public override ModuleIdentifier ID => ModuleIDs.WhsConfigProductStyle;

		#endregion

		#region Workflow

		public override bool SupportsWorkflow => false;

		#endregion

		#region Security

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsConfigProductStyle;

		#endregion
	}
}
