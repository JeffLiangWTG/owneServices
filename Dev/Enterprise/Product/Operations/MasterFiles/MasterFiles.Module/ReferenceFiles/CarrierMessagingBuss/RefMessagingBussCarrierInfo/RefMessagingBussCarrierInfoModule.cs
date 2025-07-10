using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class RefMessagingBussCarrierInfoModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.RefMessagingBussCarrierInfo;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.RefMessagingBussCarrierInfo;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
			=> ZControllerFactory.Create(ControllerIDs.RefMessagingBussCarrierInfo);

		protected override FilterBusinessObject GetNewFilterBusinessObject()
			=> new RefMessagingBussCarrierInfoFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl()
			=> new RefMessagingBussCarrierInfoFilterControl(GridCollection, (RefMessagingBussCarrierInfoFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new RefMessagingBussCarrierInfoCollection(Factory);

		public override bool AllowDelete => false;

		public override bool AllowNew => false;

		public override bool AllowEdit => false;

		public override bool SupportsWorkflow => false;
	}
}
