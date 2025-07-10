using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	class CusUSLVClearanceDataTest : AssemblyDataTest
	{
		#region TestBusinessObjectType

		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(CusUSLVClearance), ClearanceData.BusinessObjectType);
		}

		#endregion

		#region TestGetBusinessObjectCollection

		public void TestGetBusinessObjectCollection()
		{
			AssertType<CusUSLVClearanceAdhocEdocsSupportCollection>(ClearanceData.GetBusinessObjectCollection(Factory));
		}

		#endregion

		#region TestReferenceType

		public void TestReferenceType()
		{
			AssertEquals(Core.Constants.ReferenceTypes.SupplyChainLogistics, ClearanceData.ReferenceType);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			AssertEquals("Low Value Entries", ClearanceData.HumanReadableName.ToString());
		}

		#endregion

		#region TestIsAllowedForUnallocatedeDocs

		public void TestIsAllowedForUnallocatedeDocs()
		{
			Assert(ClearanceData.IsAllowedForUnallocatedeDocs);
		}

		#endregion

		#region Implementation

		CusUSLVClearanceData ClearanceData
		{
			get
			{
				return clearanceData ?? (clearanceData = new CusUSLVClearanceData());
			}
		}
		CusUSLVClearanceData clearanceData;

		#endregion
	}
}
