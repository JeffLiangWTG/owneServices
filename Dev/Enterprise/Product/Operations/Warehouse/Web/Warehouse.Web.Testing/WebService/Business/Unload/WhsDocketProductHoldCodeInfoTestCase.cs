using System.Collections.Generic;
using System.Linq;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WhsDocketProductHoldCodeInfo))]
	public class WhsDocketProductHoldCodeInfoTestCase : DataObjectInfoTestCase<WhsDocketProductHoldCodeInfo>
	{
		public void TestAdditionalContructors()
		{
			var productCode = "P1";
			var hccs = new HashSet<string> { "HEL", "LCC" };
			var docketHoldCodeInfo = new WhsDocketProductHoldCodeInfo(productCode, hccs.ToArray());

			AssertEquals("ProductPK is correct", productCode, docketHoldCodeInfo.ProductCode);
			AssertContainsExactElementsInAnyOrder("Hold Codes are correct", hccs, docketHoldCodeInfo.HoldCodes);
		}

		public void TestProductCode()
		{
			AssertEquals("Default ProductCode is correct", string.Empty, Parent.ProductCode);

			var code1 = "P1";
			Parent.ProductCode = code1;
			AssertEquals("ProductPK is correct", code1, Parent.ProductCode);

			var code2 = "P2";
			Parent.ProductCode = code2;
			AssertEquals("ProductPK is correct", code2, Parent.ProductCode);
		}

		public void TestHoldCodes()
		{
			AssertContainsExactElementsInAnyOrder("Default HoldCodes is correct", Enumerable.Empty<string>(), Parent.HoldCodes);

			var codes1 = new HashSet<string> { "HEL", "DAM" };
			Parent.HoldCodes = codes1;
			AssertContainsExactElementsInAnyOrder("HoldCodes is correct", codes1, Parent.HoldCodes);

			var codes2 = new HashSet<string> { "LCC", "GE" };
			Parent.HoldCodes = codes2;
			AssertContainsExactElementsInAnyOrder("HoldCodes is correct", codes2, Parent.HoldCodes);
		}

		#region Implementation

		protected new WhsDocketProductHoldCodeInfo Parent
		{
			get
			{
				return (WhsDocketProductHoldCodeInfo)base.Parent;
			}
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new WhsDocketProductHoldCodeInfo();
		}

		#endregion
	}
}
