using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.eTail.Business.Testing
{
	class HVLVOuterPackageDataTest : AssemblyDataTest
	{
		#region TestBusinessObjectType

		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(HVLVOuterPackage), OuterPackageData.BusinessObjectType);
		}

		#endregion

		#region TestGetBusinessObjectCollection

		public void TestGetBusinessObjectCollection()
		{
			AssertType<HVLVOuterPackageAdhocEdocsSupportCollection>(OuterPackageData.GetBusinessObjectCollection(Factory));
		}

		#endregion

		#region TestReferenceType

		public void TestReferenceType()
		{
			AssertEquals(Core.Constants.ReferenceTypes.SupplyChainLogistics, OuterPackageData.ReferenceType);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			AssertEquals("Outer Package", OuterPackageData.HumanReadableName.ToString());
		}

		#endregion

		#region TestIsAllowedForUnallocatedeDocs

		public void TestIsAllowedForUnallocatedeDocs()
		{
			Assert(OuterPackageData.IsAllowedForUnallocatedeDocs);
		}

		#endregion

		#region Implementation

		HVLVOuterPackageData OuterPackageData
		{
			get
			{
				return outerPackageData ?? (outerPackageData = new HVLVOuterPackageData());
			}
		}

		HVLVOuterPackageData outerPackageData;

		#endregion
	}
}
