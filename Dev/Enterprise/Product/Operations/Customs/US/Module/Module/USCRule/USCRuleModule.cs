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
	public class USCRuleModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.US.USCRule;

		public override bool AllowDelete => false;

		public override bool AllowNew => false;

		public override bool AllowEdit => false;

		protected override IFilterControl GetNewFilterControl() => new USCRuleFilterUserControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new USCRuleCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new USCRuleFilterBusinessObject();

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.US.USCRule);

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.USCustomsTariffRule;

		protected override bool ShowRecentItemsCore() => false;
	}
}
