using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Module
{
	public class VoyageAccountingModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public VoyageAccountingModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AgencyVoyageAccounting; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.AgencyVoyageAccounting);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new VoyageAccountingFilterControl(GridCollection, (VoyageAccountingFilterStrip)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new VoyageAccountCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new VoyageAccountingFilterStrip();
		}

		#region Checkpoints

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.ShippingManagerVoyageAccounting; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AgencyVoyageAccount; }
		}

		#endregion

		#region IOperationalActionSupportable Members

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter
		{
			get { return new VoyageAccountingActionSupporter(); }
		}

		#endregion
	}
}


