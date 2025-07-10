using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	public class CusAuthorisationsModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.CusAuthorisations;

		public override Security.SecurityCheckpoint SecurityCheckpoint => Env.Security.Authorisations;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;

		protected override bool ShouldLoadFilterBusinessObjectDefaults => true;

		public override bool AllowNew => true;

		public override bool AllowDelete => true;

		public override bool AllowEdit => true;

		public override bool AllowView => true;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.CusAuthorisations);

		protected override IFilterControl GetNewFilterControl() => new CusAuthorisationsControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CusAuthorisationHeaderCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new CusAuthorisationsFilterStripBusinessObject();
	}
}
