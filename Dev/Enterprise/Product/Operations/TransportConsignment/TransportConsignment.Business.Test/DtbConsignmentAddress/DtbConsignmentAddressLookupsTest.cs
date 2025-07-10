using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbConsignmentAddressLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestOrganisations

		#region TestAllOrganisations

		public void TestAllOrganisations()
		{
			var instruction = GetNewConsignmentAddress();
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(OrgHeaderCollection), instruction.Lookups.AllOrganisations.GetType());
		}

		#endregion

		#region TestConsigneeOrganisations

		public void TestConsigneeOrganisations()
		{
			var instruction = GetNewConsignmentAddress();
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(ConsigneeCollection), instruction.Lookups.ConsigneeOrganisations.GetType());
		}

		#endregion

		#region TestConsignorOrganisations

		public void TestConsignorOrganisations()
		{
			var instruction = GetNewConsignmentAddress();
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(ConsignorCollection), instruction.Lookups.ConsignorOrganisations.GetType());
		}

		#endregion

		#region TestCFSOrganisations

		public void TestCFSOrganisations()
		{
			var instruction = GetNewConsignmentAddress();
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(DepotCollection), instruction.Lookups.CFSOrganisations.GetType());
		}

		#endregion

		#region TestCTOOrganisations

		public void TestCTOOrganisations()
		{
			var instruction = GetNewConsignmentAddress();
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(CTOCollection), instruction.Lookups.CTOOrganisations.GetType());
		}

		#endregion

		#region TestContainerYardOrganisations

		public void TestContainerYardOrganisations()
		{
			var instruction = GetNewConsignmentAddress();
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(ContainerYardCollection), instruction.Lookups.ContainerYardOrganisations.GetType());
		}

		#endregion

		#endregion

		#region Helper Functions

		DtbConsignmentAddress GetNewConsignmentAddress()
		{
			return Helper.CreateConsignmentAddress();
		}

		#endregion

		#region Helper

		TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}

		TransportConsignmentTestHelper helper;

		#endregion
	}
}
