using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.eTail.Business.Testing
{
	class HVLVOriginLoadListDataTest : AssemblyDataTest
	{
		#region TestBusinessObjectType

		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(HVLVOriginLoadList), LoadListData.BusinessObjectType);
		}

		#endregion

		#region TestGetBusinessObjectCollection

		public void TestGetBusinessObjectCollection()
		{
			AssertType<HVLVOriginLoadListAdhocEdocsSupportCollection>(LoadListData.GetBusinessObjectCollection(Factory));
		}

		#endregion

		#region TestReferenceType

		public void TestReferenceType()
		{
			AssertEquals(Core.Constants.ReferenceTypes.SupplyChainLogistics, LoadListData.ReferenceType);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			AssertEquals("Load List", LoadListData.HumanReadableName.ToString());
		}

		#endregion

		#region TestIsAllowedForUnallocatedeDocs

		public void TestIsAllowedForUnallocatedeDocs()
		{
			Assert(LoadListData.IsAllowedForUnallocatedeDocs);
		}

		#endregion

		#region Implementation

		HVLVOriginLoadListData LoadListData
		{
			get
			{
				return loadListData ?? (loadListData = new HVLVOriginLoadListData());
			}
		}

		HVLVOriginLoadListData loadListData;

		#endregion
	}
}
