using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MarketingManager.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.Module
{
	public class SalesProductModule : ZFilterGridModule
	{
		#region ID

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.SalesProduct; }
		}

		#endregion

		#region Allowed Actions

		public override bool AllowNew
		{
			get { return false; }
		}

		#endregion

		#region Controller

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.SalesProduct);
		}

		#endregion

		#region Grid Collection

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new OrgSalesProductCollection(Factory);
		}

		#endregion

		#region Filter

		protected override IFilterControl GetNewFilterControl()
		{
			return new SalesProductFilterControl(GridCollection, (SalesProductFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new SalesProductFilterBusinessObject();
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.SalesProducts; }
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion
	}
}
