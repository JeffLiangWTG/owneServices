using Enterprise.Core;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbConsignmentRunSheetJobDataTest : AssemblyDataTest
	{
		#region TestBusinessObjectType

		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(DtbConsignmentRunSheet), ConsignmentJobData.BusinessObjectType);
		}

		#endregion

		#region TestGetBusinessObjectCollection

		public void TestGetBusinessObjectCollection()
		{
			AssertEquals(typeof(DtbConsignmentRunSheetCollection), ConsignmentJobData.GetBusinessObjectCollection(Factory).GetType());
		}

		#endregion

		#region TestModuleID

		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.DtbConsignmentRunSheet, ConsignmentJobData.ModuleID);
		}

		#endregion

		#region TestReferenceType

		public void TestReferenceType()
		{
			AssertEquals(Constants.ReferenceTypes.SupplyChainLogistics, ConsignmentJobData.ReferenceType);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			AssertEquals("Land Transport Run Sheet", ConsignmentJobData.HumanReadableName.ToString());
		}

		#endregion

		#region TestIsAllowedForUnallocatedeDocs

		public void TestIsAllowedForUnallocatedeDocs()
		{
			AssertEquals(true, ConsignmentJobData.IsAllowedForUnallocatedeDocs);
		}

		#endregion

		#region Implementation

		DtbConsignmentRunSheetJobData ConsignmentJobData
		{
			get { return consignmentJobData ?? (consignmentJobData = new DtbConsignmentRunSheetJobData()); }
		}
		DtbConsignmentRunSheetJobData consignmentJobData;

		#endregion
	}
}
