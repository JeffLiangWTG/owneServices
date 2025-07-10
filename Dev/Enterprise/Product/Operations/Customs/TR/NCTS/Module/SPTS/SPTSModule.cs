using CargoWise.EntityFramework;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.TR.NCTS.Module
{
	public class SPTSModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.TR.SimplifiedProcedureTransitSystem;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.SimplifiedProcedureTransitSystem;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.TR.SimplifiedProcedureTransitSystem);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new SPTSFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl()
		{
			return new SPTSFilterControl(GridCollection, (SPTSFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection() => new SPTSHeaderCollection(Factory, GlbCompany.CurrentCompany);

		public override bool AllowNew => true;

		public override bool AllowDelete => true;

		public override bool AllowEdit => true;
		public override bool SupportsWorkflow => false;
	}
}

