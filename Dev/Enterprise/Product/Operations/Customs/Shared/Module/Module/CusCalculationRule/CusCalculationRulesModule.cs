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
	public abstract class CusCalculationRulesModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.CusCalculationRules;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CusCalculationRules;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.CusCalculationRules);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new CusCalculationRulesFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new CusCalculationRulesFilterControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CusCalculationRuleCollection<CusCalculationRule>(Factory);

		public override bool AllowUniversalCopy => false;
	}
}
