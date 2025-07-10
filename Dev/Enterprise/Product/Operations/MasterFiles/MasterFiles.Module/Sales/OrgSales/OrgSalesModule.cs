using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class OrgSalesModule : ZFilterGridModule
	{
		protected override bool ShowRecentItemsCore()
		{
			return false;
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return null;
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new OrgSalesFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new OrgSalesFilterControl(GridCollection, (OrgSalesFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new OrgSalesCollection(Factory);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Sales; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.None; }
		}
	}
}
