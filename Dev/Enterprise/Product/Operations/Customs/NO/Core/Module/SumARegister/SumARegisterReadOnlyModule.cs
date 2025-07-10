using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.NO.Module;

public sealed  class SumARegisterReadOnlyModule : EFTA.TemporaryStorageRegister.Module.SumARegisterReadOnlyModule
{
	protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.NO.TemporaryStorageRegisterReadOnly);
}
