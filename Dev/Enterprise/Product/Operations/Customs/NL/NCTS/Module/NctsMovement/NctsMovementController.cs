using System;
using Enterprise.Customs.NL.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Module;

public class NctsMovementController : EU.NCTS.Module.NctsMovementController
{
	public override Type TypeOfTopLevelBusinessObject => typeof(NctsHeader);
}
