using CargoWise.EntityFramework;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ZA.Module
{
	public class GenralMessageModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ZAModuleIDs.GenralMessage;

		public override bool AllowNew => false;

		public override bool AllowEdit => false;

		public override bool AllowDelete => false;

		public override SecurityCheckpoint SecurityCheckpoint => (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZAGenralMessage);

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ZAControllerIDs.GenralMessage);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new GenralMessageFilterStripBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new GenralMessageFilterStripControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new GenralMessageCollection(Factory);
	}
}
