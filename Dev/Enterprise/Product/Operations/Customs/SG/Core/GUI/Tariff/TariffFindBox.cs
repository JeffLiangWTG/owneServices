using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.V4.GUI
{
	public class TariffFindBox : Universal.GUI.TariffFindBox
	{
		protected override IFindBoxPopup GetNewPopupForm()
		{
			var additionalData = new AdditionalDataForBorderWise("E", ZDateTime.Today, x => x.Replace(".", string.Empty));
			return BorderWiseTariffFindBoxProvider.GetTariffFindBoxWrapper(additionalData, () => base.GetNewPopupForm());
		}
	}
}
