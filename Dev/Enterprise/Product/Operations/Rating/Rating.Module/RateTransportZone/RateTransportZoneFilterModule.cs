using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Rating.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Module
{
	public class RateTransportZoneModule : ZFilterGridModule
	{
		#region Standard Module Overrides

		public override bool AllowEdit
		{
			get { return false; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.RateTransportZone; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.RateTransportZone);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new RateTransportZoneFilterControl(GridCollection, (RateTransportZoneFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new RateTransportZonesCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new RateTransportZoneFilterBusinessObject();
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.RateTransportProvider; }
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

