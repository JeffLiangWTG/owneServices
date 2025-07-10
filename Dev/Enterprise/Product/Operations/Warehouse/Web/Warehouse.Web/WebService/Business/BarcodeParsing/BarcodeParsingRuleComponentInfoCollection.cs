using System;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class BarcodeParsingRuleComponentInfoCollection : DataObjectInfoCollection<BarcodeParsingRuleComponentInfo>
	{
	}
}
