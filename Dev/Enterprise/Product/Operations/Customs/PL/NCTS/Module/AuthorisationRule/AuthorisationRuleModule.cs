using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.PL.NCTS.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.PL.NCTS.Module;

public class AuthorisationRuleModule : ZFilterGridModule
{
	public override ModuleIdentifier ID => ModuleIDs.Customs.EU.PL.AuthorisationRule;

	protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.PL.AuthorisationRule);

	protected override ZArchitecture.GUI.IFilterControl GetNewFilterControl() => new AuthorisationRuleControl(GridCollection, FilterBusinessObject);

	protected override IBusinessObjectCollection GetNewGridCollection() => new CusAuthorisationRuleCollection(Factory);

	protected override FilterBusinessObject GetNewFilterBusinessObject() => new AuthorisationRuleFilterBusinessObject();

	protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

	public override SecurityCheckpoint SecurityCheckpoint => Env.Security.Authorisations;

	public override ZBool HasActions => false;

	protected override bool ShowRecentItemsCore() => false;
}
