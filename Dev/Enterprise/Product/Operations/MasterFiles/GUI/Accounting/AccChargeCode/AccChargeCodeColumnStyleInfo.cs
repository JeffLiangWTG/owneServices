using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI;

[ZArchitecture.GUI.Testing.SuppressCheckControlLookupList]
[ZArchitecture.GUI.Testing.SuppressCheckControlModuleId]
public class AccChargeCodeColumnStyleInfo : ZCodeFindBoxColumnStyleInfo
{
	public override Type ColumnStyleType => typeof(AccChargeCodeColumnStyle);
}

public class AccChargeCodeColumnStyle : ZCodeFindBoxColumnStyle
{
	public AccChargeCodeColumnStyle(AccChargeCodeColumnStyleInfo columnInfo)
		: this(() => new AccChargeCodeGridFindBox(), columnInfo) { }

	protected AccChargeCodeColumnStyle(Func<AccChargeCodeGridFindBox> gridFindBox, AccChargeCodeColumnStyleInfo columnInfo) : base(gridFindBox, columnInfo) { }
}
