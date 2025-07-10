using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Module.Testing;

[TestedType(typeof(SumARegisterReadOnlyController))]
sealed class SumARegisterReadOnlyControllerTest : ZControllerBasherTest
{
	public void TestTypeOfTopLevelBusinessObject()
		=> AssertEquals(typeof(CusTempStorageRegHeader), new SumARegisterReadOnlyController().TypeOfTopLevelBusinessObject);

	protected override ControllerID GetControllerID() => ControllerIDs.Customs.NO.TemporaryStorageRegisterReadOnly;

	protected override string CountryCode => Core.Constants.CountryCodes.Norway;
}
