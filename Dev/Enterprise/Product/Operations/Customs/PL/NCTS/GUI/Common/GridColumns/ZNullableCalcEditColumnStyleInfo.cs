using System.ComponentModel;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.PL.NCTS.GUI;

sealed class ZNullableCalcEditColumnStyleInfo : ZCalcEditColumnStyleInfo
{
	public ZNullableCalcEditColumnStyleInfo()
	{
		ShowEmptyStringForEmptyValue = true;
	}

	[DefaultValue(true)]
	public new bool ShowEmptyStringForEmptyValue
	{
		get => base.ShowEmptyStringForEmptyValue;
		set => base.ShowEmptyStringForEmptyValue = value;
	}
}
