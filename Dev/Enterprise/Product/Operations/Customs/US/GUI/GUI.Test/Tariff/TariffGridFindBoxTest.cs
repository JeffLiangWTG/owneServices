using System;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;
using CommonGUI = Enterprise.Customs.Common.GUI;

namespace Enterprise.Customs.US.GUI
{
	sealed class TariffGridFindBoxTest : CommonGUI.Testing.TariffGridFindBoxTest
	{
		protected override Type ExpectedFormTypeWhenBorderWiseNotEnabled => typeof(ZCodeFindBoxPopup);

		protected override Type ExpectedListProviderType => typeof(TariffFormattedFindBoxListProvider);

		protected override CommonGUI.TariffColumnStyleInfo GetNewTariffColumnStyleInfo() => new TariffColumnStyleInfo();

		protected override CommonGUI.TariffGridFindBox GetNewTariffGridFindBox() => new TariffGridFindBox();
	}
}
