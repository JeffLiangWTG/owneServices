using Enterprise.Core;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportConsignment.Business.Testing
{
	class DtbBookingConsignmentJobDataTest : AssemblyDataTest
	{
		#region TestBusinessObjectType

		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(DtbBookingConsignment), ConsignmentJobData.BusinessObjectType);
		}

		#endregion

		#region TestGetBusinessObjectCollection

		public void TestGetBusinessObjectCollection()
		{
			AssertEquals(typeof(DtbBookingConsignmentCollection), ConsignmentJobData.GetBusinessObjectCollection(Factory).GetType());
		}

		#endregion

		#region TestModuleID

		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.DtbBookingConsignment, ConsignmentJobData.ModuleID);
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
			AssertEquals("Booking Consignment", ConsignmentJobData.HumanReadableName.ToString());
		}

		#endregion

		#region TestIsAllowedForUnallocatedeDocs

		public void TestIsAllowedForUnallocatedeDocs()
		{
			AssertEquals(true, ConsignmentJobData.IsAllowedForUnallocatedeDocs);
		}

		#endregion

		#region Implementation

		DtbBookingConsignmentJobData ConsignmentJobData
		{
			get { return consignmentJobData ?? (consignmentJobData = new DtbBookingConsignmentJobData()); }
		}
		DtbBookingConsignmentJobData consignmentJobData;

		#endregion
	}
}
