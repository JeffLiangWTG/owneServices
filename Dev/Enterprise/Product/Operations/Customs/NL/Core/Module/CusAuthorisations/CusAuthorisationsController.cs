using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.NL.Business;
using Enterprise.Customs.NL.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.Module;

public class CusAuthorisationsController : Customs.Module.CusAuthorisationsController
{
	public override Type TypeOfTopLevelBusinessObject => typeof(CusAuthorisationHeader);

	protected override IZForm GetForm(IBusiness businessEntity) => new CusAuthorisationForm((CusAuthorisationHeader)businessEntity);
}
