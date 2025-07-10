using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobConsolWorkflowCondition2CodeListTest : TestCaseWithFactory
	{
		public void TestItems()
		{
			AssertEquals("IMP", CodeList[0].Code);
			AssertEquals("Import", CodeList[0].Description);
			AssertEquals("EXP", CodeList[1].Code);
			AssertEquals("Export", CodeList[1].Description);
			AssertEquals("", CodeList[2].Code);
			AssertEquals("", CodeList[2].Description);

			AssertEquals("Container mode FCL", true, CodeList.ContainsCode(Core.Constants.ContainerModes.FCL));
			AssertEquals("Container mode LCL", true, CodeList.ContainsCode(Core.Constants.ContainerModes.LCL));
			AssertEquals("Not contains container mode 'Other'", false, CodeList.ContainsCode(Core.Constants.ContainerModes.Other));
		}

		JobConsolWorkflowCondition2CodeList CodeList
		{
			get { return codeList ?? (codeList = new JobConsolWorkflowCondition2CodeList()); }
		}
		JobConsolWorkflowCondition2CodeList codeList;
	}
}
