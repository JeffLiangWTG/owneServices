using Enterprise.Core;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbConsignmentJobDataTest : AssemblyDataTest
	{
		#region TestBusinessObjectType

		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(DtbConsignment), ConsignmentJobData.BusinessObjectType);
		}

		#endregion

		#region TestGetBusinessObjectCollection

		public void TestGetBusinessObjectCollection()
		{
			AssertEquals(typeof(DtbConsignmentCollection), ConsignmentJobData.GetBusinessObjectCollection(Factory).GetType());
		}

		#endregion

		#region TestModuleID

		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.DtbConsignment, ConsignmentJobData.ModuleID);
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
			AssertEquals("Land Transport Consignment", ConsignmentJobData.HumanReadableName.ToString());
		}

		#endregion

		#region TestIsAllowedForUnallocatedeDocs

		public void TestIsAllowedForUnallocatedeDocs()
		{
			AssertEquals(true, ConsignmentJobData.IsAllowedForUnallocatedeDocs);
		}

		#endregion

		#region Implementation

		DtbConsignmentJobData ConsignmentJobData
		{
			get { return consignmentJobData ?? (consignmentJobData = new DtbConsignmentJobData()); }
		}
		DtbConsignmentJobData consignmentJobData;

		#endregion
	}
}
