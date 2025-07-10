using System;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.NO.Module;

public sealed class SumARegisterReadOnlyController : EFTA.TemporaryStorageRegister.Module.SumARegisterReadOnlyController
{
	public override ControllerID ID => ControllerIDs.Customs.NO.TemporaryStorageRegisterReadOnly;

	public override ModuleIdentifier ModuleID => ModuleIDs.Customs.NO.TemporaryStorageRegisterReadOnly;

	public override Type TypeOfTopLevelBusinessObject => typeof(CusTempStorageRegHeader);
}
