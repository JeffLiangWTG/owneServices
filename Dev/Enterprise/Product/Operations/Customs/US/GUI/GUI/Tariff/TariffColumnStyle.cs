
using CommonGUI = Enterprise.Customs.Common.GUI;

namespace Enterprise.Customs.US.GUI
{
	public class TariffColumnStyle : CommonGUI.TariffColumnStyle
	{
		public TariffColumnStyle(TariffColumnStyleInfo info)
			: base(() => new TariffGridFindBox(), info)
		{
		}
	}
}
