using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.SG.V4.GUI
{
	public class TariffColumnStyle : ZBaseFindBoxColumnStyle
	{
		public TariffColumnStyle(TariffColumnStyleInfo info)
			: base(() => new TariffGridFindBox
			{
				GetTariffType = info.GetTariffType
			}, info)
		{
		}
	}
}
