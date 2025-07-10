using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ContainerNonCustomsAdditionalReferenceCodesCodeListTest : TestCase
	{
		public void TestListCodes()
		{
			AssertEquals("CMR", List[0].Code);
		}

		public void TestListDescriptions()
		{
			AssertEquals("Carrier Message Reference", List[0].Description);
		}

		ContainerNonCustomsAdditionalReferenceCodesCodeList List
		{
			get
			{
				if (list == null)
				{
					list = new ContainerNonCustomsAdditionalReferenceCodesCodeList();
				}
				return list;
			}
		}
		ContainerNonCustomsAdditionalReferenceCodesCodeList list;
	}
}
