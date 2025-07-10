using System;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.NO.Module;

public sealed class SumARegisterController : EFTA.TemporaryStorageRegister.Module.SumARegisterController
{
	public override ControllerID ID => ControllerIDs.Customs.NO.TemporaryStorageRegister;

	public override ModuleIdentifier ModuleID => ModuleIDs.Customs.NO.TemporaryStorageRegister;

	public override Type TypeOfTopLevelBusinessObject => typeof(CusTempStorageRegHeader);
}
