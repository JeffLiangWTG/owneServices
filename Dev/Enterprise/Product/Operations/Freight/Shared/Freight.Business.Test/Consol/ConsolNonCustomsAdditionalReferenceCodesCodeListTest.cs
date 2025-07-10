using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ConsolNonCustomsAdditionalReferenceCodesCodeListTest : TestCase
	{
		public void TestListCodes()
		{
			AssertEquals("CSR", List[0].Code);
			AssertEquals("CMR", List[1].Code);
		}

		public void TestListDescriptions()
		{
			AssertEquals("Carrier Shipper Reference", List[0].Description);
			AssertEquals("Carrier Message Reference", List[1].Description);
		}

		ConsolNonCustomsAdditionalReferenceCodesCodeList List
		{
			get
			{
				if (list == null)
				{
					list = new ConsolNonCustomsAdditionalReferenceCodesCodeList();
				}
				return list;
			}
		}
		ConsolNonCustomsAdditionalReferenceCodesCodeList list;
	}
}
