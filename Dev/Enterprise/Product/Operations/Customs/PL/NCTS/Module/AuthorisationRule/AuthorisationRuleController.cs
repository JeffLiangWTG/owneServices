using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.PL.NCTS.Module;

public class AuthorisationRuleController : ZController
{
	public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

	protected override IZForm GetForm(IBusiness businessEntity)
	{
		var authorisationRule = (CusAuthorisationRule)businessEntity;
		return new CusAuthorisationForm(authorisationRule.AuthorisationHeader);
	}

	protected override SecurityCheckpoint CheckPointForDelete => Env.Security.AuthorisationsDelete;

	protected override SecurityCheckpoint CheckPointForEdit => Env.Security.AuthorisationsEdit;

	protected override SecurityCheckpoint CheckPointForNew => Env.Security.AuthorisationsNew;

	protected override SecurityCheckpoint CheckPointForView => Env.Security.AuthorisationsView;

	public override ControllerID ID => ControllerIDs.Customs.PL.AuthorisationRule;

	public override Type TypeOfTopLevelBusinessObject => typeof(CusAuthorisationRule);

	public override ModuleIdentifier ModuleID => ModuleIDs.Customs.EU.PL.AuthorisationRule;
}
