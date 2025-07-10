
namespace Enterprise.Customs.US.GUI
{
	using System;

	public class TariffColumnStyleInfo : Common.GUI.TariffColumnStyleInfo
	{
		public override Type ColumnStyleType
		{
			get { return typeof(TariffColumnStyle); }
		}
	}
}
