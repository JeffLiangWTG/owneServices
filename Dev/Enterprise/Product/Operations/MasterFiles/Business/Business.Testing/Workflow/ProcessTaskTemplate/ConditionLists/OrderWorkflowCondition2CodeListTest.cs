using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrderWorkflowCondition2CodeListTest : TestCaseWithFactory
	{
		public void TestItems()
		{
			AssertEquals(Core.Constants.TransportModes.Air, CodeList[0].Code);
			AssertEquals(Core.Constants.TransportModes.Sea, CodeList[1].Code);
			AssertEquals(Core.Constants.TransportModes.Road, CodeList[2].Code);
			AssertEquals(Core.Constants.TransportModes.Rail, CodeList[3].Code);
			AssertEquals(Core.Constants.TransportModes.Mail, CodeList[4].Code);
			AssertEquals("", CodeList[5].Code);
			AssertEquals(Core.Constants.ContainerModes.LCL, CodeList[6].Code);
			AssertEquals(Core.Constants.ContainerModes.FCL, CodeList[7].Code);
		}

		OrderWorkflowCondition2CodeList CodeList
		{
			get { return codeList ?? (codeList = new OrderWorkflowCondition2CodeList()); }
		}
		OrderWorkflowCondition2CodeList codeList;
	}
}
