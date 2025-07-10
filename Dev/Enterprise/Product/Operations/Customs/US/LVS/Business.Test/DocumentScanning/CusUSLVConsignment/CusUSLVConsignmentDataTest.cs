using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	class CusUSLVConsignmentDataTest : AssemblyDataTest
	{
		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(CusUSLVConsignment), ConsignmentData.BusinessObjectType);
		}

		public void TestGetBusinessObjectCollection()
		{
			AssertType<CusUSLVConsignmentAdhocEdocsSupportCollection>(ConsignmentData.GetBusinessObjectCollection(Factory));
		}

		public void TestReferenceType()
		{
			AssertEquals(Core.Constants.ReferenceTypes.SupplyChainLogistics, ConsignmentData.ReferenceType);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Consignment Bill", ConsignmentData.HumanReadableName.ToString());
		}

		public void TestIsAllowedForUnallocatedeDocs()
		{
			AssertEquals(true, ConsignmentData.IsAllowedForUnallocatedeDocs);
		}

		#region Implementation

		CusUSLVConsignmentData ConsignmentData => consignmentData ?? (consignmentData = new CusUSLVConsignmentData());
		CusUSLVConsignmentData consignmentData;

		#endregion
	}
}
