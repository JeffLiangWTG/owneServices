using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.PortHubs.Module
{
	public class PortDepotCarrierSelectionModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.PortDepotCarrierSelection; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.PortDepotSelection; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.PortDepotCarrierSelection);

		protected override IBusinessObjectCollection GetNewGridCollection() => new ActiveBusinessObjectCollection<PortHubSelection>(Factory);

		protected override IFilterControl GetNewFilterControl() => new PortDepotCarrierFilterStripControl(GridCollection, (PortDepotCarrierFilterStripBusinessObject)FilterBusinessObject);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new PortDepotCarrierFilterStripBusinessObject();
	}
}
