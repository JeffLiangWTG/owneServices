using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.eTail.Business.Testing
{
	class HVLVConsignmentDataTest : AssemblyDataTest
	{
		#region TestBusinessObjectType

		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(HVLVConsignment), ConsignmentData.BusinessObjectType);
		}

		#endregion

		#region TestGetBusinessObjectCollection

		public void TestGetBusinessObjectCollection()
		{
			AssertType<HVLVConsignmentAdhocEdocsSupportCollection>(ConsignmentData.GetBusinessObjectCollection(Factory));
		}

		#endregion

		#region TestReferenceType

		public void TestReferenceType()
		{
			AssertEquals(Core.Constants.ReferenceTypes.SupplyChainLogistics, ConsignmentData.ReferenceType);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			AssertEquals("Consignment", ConsignmentData.HumanReadableName.ToString());
		}

		#endregion

		#region TestIsAllowedForUnallocatedeDocs

		public void TestIsAllowedForUnallocatedeDocs()
		{
			Assert(ConsignmentData.IsAllowedForUnallocatedeDocs);
		}

		#endregion

		#region Implementation

		HVLVConsignmentData ConsignmentData
		{
			get
			{
				return consignmentData ?? (consignmentData = new HVLVConsignmentData());
			}
		}

		HVLVConsignmentData consignmentData;

		#endregion
	}
}
