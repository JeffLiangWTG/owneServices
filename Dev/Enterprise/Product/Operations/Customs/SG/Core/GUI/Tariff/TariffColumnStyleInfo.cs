using System;

namespace Enterprise.Customs.SG.V4.GUI
{
	public class TariffColumnStyleInfo : Universal.GUI.TariffColumnStyleInfo
	{
		public override Type ColumnStyleType
		{
			get { return typeof(TariffColumnStyle); }
		}
	}
}
