using System;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.NL.GUI;

[SuppressCheckControlModuleId]
[SuppressCheckControlLookupList]
public class GoodsLocationColumnStyleInfo : ZCodeFindBoxColumnStyleInfo
{
	public override Type ColumnStyleType => typeof(GoodsLocationColumnStyle);
}
