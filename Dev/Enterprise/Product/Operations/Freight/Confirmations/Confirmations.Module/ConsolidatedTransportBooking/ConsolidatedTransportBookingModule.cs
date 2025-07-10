using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Confirmations.Module
{
	public class ConsolidatedTransportBookingModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ConsolidatedTransportBooking; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.ConsolidatedTransportBooking);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ConsolidatedTransportBookingFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CommonConsolidatedTransportBookingCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ConsolidatedTransportBookingFilterBusinessObject();
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		#region Security / Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Forwarder; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ConsolidatedTransportBooking; }
		}

		#endregion
	}
}
