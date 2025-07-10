using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.eTail.Business.Testing
{
	sealed class GBCustomsStatusStoreTest : TestCaseWithFactory
	{
		public void TestCustomsStatusCodeAndDescriptionList()
		{
			var statusStore = new GBCustomsStatusStore(Factory);
			var actualList = statusStore.GetAllRefCusCodeList(true);

			var expectedList = new List<(string, string)>()
			{
				("ACC", "Declaration has been legally accepted"),
				("RCV", "Message has been registered"),
				("CTL", "Declaration is subject to physical control"),
				("DOC", "Declaration is subject to physical control"),
				("TAX", "Duties and taxes have been calculated and are due"),
				("CLR", "Declaration is now cleared"),
				("CAN", "Declaration has been canceled"),
			};

			AssertCodeDescriptionPairList(actualList, expectedList.ToArray());
		}

		public void TestGetReleaseStatus()
		{
			var statusStore = new GBCustomsStatusStore(Factory);

			CombineAssertions(() =>
			{
				AssertEquals("HLD", statusStore.GetReleaseStatus("ACC", true, null));
				AssertEquals("HLD", statusStore.GetReleaseStatus("RCV", true, null));
				AssertEquals("HLD", statusStore.GetReleaseStatus("CTL", true, null));
				AssertEquals("HLD", statusStore.GetReleaseStatus("DOC", true, null));
				AssertEquals("HLD", statusStore.GetReleaseStatus("TAX", true, null));
				AssertEquals("CLR", statusStore.GetReleaseStatus("CLR", true, null));
				AssertEquals("HLD", statusStore.GetReleaseStatus("CAN", true, null));
			});
		}

		public void TestGetDescription()
		{
			var statusStore = new GBCustomsStatusStore(Factory);

			CombineAssertions(() =>
			{
				AssertEquals("Declaration has been legally accepted", statusStore.GetCustomStatusDescription("ACC", true, null));
				AssertEquals("Message has been registered", statusStore.GetCustomStatusDescription("RCV", true, null));
				AssertEquals("Declaration is subject to physical control", statusStore.GetCustomStatusDescription("CTL", true, null));
				AssertEquals("Declaration is subject to physical control", statusStore.GetCustomStatusDescription("DOC", true, null));
				AssertEquals("Duties and taxes have been calculated and are due", statusStore.GetCustomStatusDescription("TAX", true, null));
				AssertEquals("Declaration is now cleared", statusStore.GetCustomStatusDescription("CLR", true, null));
				AssertEquals("Declaration has been canceled", statusStore.GetCustomStatusDescription("CAN", true, null));
			});
		}
	}
}
