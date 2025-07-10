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
	public class USCTariffRuleModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.US.USCTariffRule;

		protected override IFilterControl GetNewFilterControl() => new USCTariffRuleFilterUserControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new USCTariffRuleCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new USCTariffRuleFilterBusinessObject();

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.US.USCTariffRule);

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.USCustomsTariffRule;

		public override bool AllowDelete => false;

		public override bool AllowEdit => false;

		public override bool AllowNew => false;

		protected override bool ShowRecentItemsCore() => false;
	}
}
