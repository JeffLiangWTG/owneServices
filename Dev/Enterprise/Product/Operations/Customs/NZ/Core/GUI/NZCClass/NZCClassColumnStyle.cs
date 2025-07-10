
namespace Enterprise.Customs.NZ.GUI
{
	using System;
	using Enterprise.Customs.Common.GUI;
	using Enterprise.ZArchitecture.GUI.Testing;

	[SuppressCheckControlModuleId]
	[FormBasherTestPopupExclude]
	public class NZCClassColumnStyle : TariffColumnStyle
	{
		public NZCClassColumnStyle(NZCClassColumnStyleInfo info)
			: base(() => new NZCClassGridFindBox(), info)
		{
		}
	}

	[SuppressCheckControlModuleId]
	[FormBasherTestPopupExclude]
	public class NZCClassColumnStyleInfo : TariffColumnStyleInfo
	{
		public override Type ColumnStyleType
		{
			get { return typeof(NZCClassColumnStyle); }
		}
	}
}
