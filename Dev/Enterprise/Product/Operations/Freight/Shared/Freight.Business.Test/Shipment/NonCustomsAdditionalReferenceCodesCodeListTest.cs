using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class NonCustomsAdditionalReferenceCodesCodeListTest : TestCase
	{
		public void TestListCodes()
		{
			AssertEquals("TWR", List[0].Code);
			AssertEquals("HIR", List[1].Code);
			AssertEquals("CMR", List[2].Code);
			AssertEquals("SPT", List[3].Code);
		}

		public void TestListDescriptions()
		{
			AssertEquals("Transit Warehouse Receive", List[0].Description);
			AssertEquals("eHub Interchange Reference", List[1].Description);
			AssertEquals("Carrier Message Reference", List[2].Description);
			AssertEquals("Virtual shipment number", List[3].Description);
		}

		ShipmentNonCustomsAdditionalReferenceCodesCodeList List
		{
			get
			{
				if (list == null)
				{
					list = new ShipmentNonCustomsAdditionalReferenceCodesCodeList();
				}
				return list;
			}
		}
		ShipmentNonCustomsAdditionalReferenceCodesCodeList list;
	}
}
