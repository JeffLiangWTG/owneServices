using CommonGUI = Enterprise.Customs.Common.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class TariffColumnStyleTest : CommonGUI.Testing.TariffColumnStyleTest
	{
		public override CommonGUI.TariffColumnStyleInfo GetNewTariffColumnStyleInfo() => new TariffColumnStyleInfo();
	}
}
