using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using CommonGUI = Enterprise.Customs.Common.GUI;

namespace Enterprise.Customs.US.GUI
{
	public class TariffGridFindBox : CommonGUI.TariffGridFindBox
	{
		protected override IFindBoxListProvider GetNewListProvider()
		{
			return new TariffFormattedFindBoxListProvider((IBusinessObjectCollection)List);
		}
	}
}
