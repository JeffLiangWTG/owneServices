using System;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class BarcodeParsingRuleComponentInfoCollectionTest : DataObjectInfoCollectionTestCase<BarcodeParsingRuleComponentInfo>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(BarcodeParsingRuleComponentInfoCollection);
		}

		protected override Type GetExpectedObjectInfoType()
		{
			return typeof(BarcodeParsingRuleComponentInfo);
		}

		protected override BarcodeParsingRuleComponentInfo GetNewObjectInfo()
		{
			return new BarcodeParsingRuleComponentInfo();
		}

		protected override DataObjectInfoCollection<BarcodeParsingRuleComponentInfo> GetNewObjectInfoCollection()
		{
			return new BarcodeParsingRuleComponentInfoCollection();
		}
	}
}
