using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	public class CustomsRulesModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.CustomsRules;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CustomsRules;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.CustomsRules);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new CustomsRulesFilterStripBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new CustomsRulesFilterControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CustomsRuleCollection(Factory);

		public override bool AllowUniversalCopy => false;
	}
}
