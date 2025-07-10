using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Module.Testing;

[TestedType(typeof(SumARegisterController))]
sealed class SumARegisterControllerTest : ZControllerBasherTest
{
	public void TestTypeOfTopLevelBusinessObject()
		=> AssertEquals(typeof(CusTempStorageRegHeader), new SumARegisterController().TypeOfTopLevelBusinessObject);

	protected override ControllerID GetControllerID() => ControllerIDs.Customs.NO.TemporaryStorageRegister;

	protected override string CountryCode => Core.Constants.CountryCodes.Norway;
}
