using CargoWise.Application;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.NO.NCTS.Module.Testing.NCTS
{
	[TestedType(typeof(NctsMovementController))]
	sealed class NctsMovementControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.NctsMovementController;
		protected override string CountryCode => Core.Constants.CountryCodes.Norway;

		protected override void SetUp()
		{
			base.SetUp();
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(false);
			disposable = ObjectFactory.Substitute(mockSettings.Object);
		}

		protected override void TearDown()
		{
			base.TearDown();
			disposable?.Dispose();
			disposable = null;
		}

		System.IDisposable disposable;
	}
}
