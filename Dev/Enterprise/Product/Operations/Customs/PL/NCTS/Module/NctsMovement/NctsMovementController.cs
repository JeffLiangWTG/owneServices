using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.NCTS.Module;

public class NctsMovementController : EU.NCTS.Module.NctsMovementController
{
	public override Type TypeOfTopLevelBusinessObject => typeof(NctsHeader);

	protected override IZForm GetPhase5DepartureForm(NctsHeader nctsHeader) => new Phase5DepartureMovementForm(nctsHeader);

	protected override IZForm GetPhase5ArrivalMovementForm(NctsHeader nctsHeader) => new Phase5ArrivalMovementForm(nctsHeader);
}
