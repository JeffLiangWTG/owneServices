using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	class RunSheetJobDataTest : AssemblyDataTest
	{
		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(CommonWorkSheet), new RunSheetJobData().BusinessObjectType);
		}

		public void TestGetBusinessObjectCollection()
		{
			AssertEquals(typeof(ModuleCartageRunSheetCollection), new RunSheetJobData().GetBusinessObjectCollection(Factory).GetType());
		}

		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.CartageWorkSheet, new RunSheetJobData().ModuleID);
		}

		public void TestReferenceType()
		{
			AssertEquals(Core.Constants.ReferenceTypes.SupplyChainLogistics, new RunSheetJobData().ReferenceType);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Port Transport Run Sheet", new RunSheetJobData().HumanReadableName.ToString());
		}

		public void TestIsAllowedForUnallocatedeDocs()
		{
			AssertEquals(true, new RunSheetJobData().IsAllowedForUnallocatedeDocs);
		}
	}
}
