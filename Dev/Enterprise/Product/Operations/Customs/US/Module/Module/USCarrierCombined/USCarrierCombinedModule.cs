using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	/// <summary>
	/// Module for USCarrierCombined
	/// </summary>
	sealed class USCarrierCombinedModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.US.Carrier;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.USCustomsCarrier;

		protected override IFilterControl GetNewFilterControl() => new USCarrierCombinedFilterControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new USCarrierCombinedCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new USCarrierCombinedFilterStripBusinessObject();

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.US.USCarrierCombined);

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;
	}
}
