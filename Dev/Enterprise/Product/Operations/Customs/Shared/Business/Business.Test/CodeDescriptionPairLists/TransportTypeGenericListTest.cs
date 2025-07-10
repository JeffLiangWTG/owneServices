using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.CustomsLists.Testing
{
	sealed class TransportTypeGenericListTest : TestCaseWithFactory
	{
		public void TestListCodesMatchTransportTypeListWherePossible()
		{
			TransportTypeGenericList list = new TransportTypeGenericList();
			foreach (CodeDescriptionPair pair in new TransportTypeList())
			{
				Assert("TransportTypeGenericList is missing code: " + pair.Code, !string.IsNullOrEmpty(list.GetDescriptionFromCode(pair.Code)));
			}
		}
	}
}
