using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.SG.V4.GUI
{
	[SuppressCheckControlModuleId]
	[FormBasherTestPopupExclude]
	public class TariffGridFindBox : Universal.GUI.TariffGridFindBox
	{
		protected override IFindBoxPopup GetNewPopupForm()
		{
			var additionalData = new AdditionalDataForBorderWise("E", ZDateTime.Today, x => x.Replace(".", string.Empty));
			return BorderWiseTariffFindBoxProvider.GetTariffFindBoxWrapper(additionalData, () => base.GetNewPopupForm());
		}
	}
}
