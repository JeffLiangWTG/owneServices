using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportCommon.Business.Testing
{
	public class TransportBindToListsTest : TestCaseWithFactory
	{
		#region TestDropModes

		public void TestDropModes()
		{
			AssertContainsExactElementsInAnyOrder(new CombinedEquipmentNeededList(), BindToLists.DropModes);
		}

		#endregion

		#region TestOrganisations

		#region TestAllOrganisations

		public void TestAllOrganisations()
		{
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(OrgHeaderCollection), BindToLists.AllOrganisations.GetType());
		}

		#endregion

		#region TestConsigneeOrganisations

		public void TestConsigneeOrganisations()
		{
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(ConsigneeCollection), BindToLists.ConsigneeOrganisations.GetType());
		}

		#endregion

		#region TestConsignorOrganisations

		public void TestConsignorOrganisations()
		{
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(ConsignorCollection), BindToLists.ConsignorOrganisations.GetType());
		}

		#endregion

		#region TestCFSOrganisations

		public void TestCFSOrganisations()
		{
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(DepotCollection), BindToLists.CFSOrganisations.GetType());
		}

		#endregion

		#region TestCTOOrganisations

		public void TestCTOOrganisations()
		{
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(CTOCollection), BindToLists.CTOOrganisations.GetType());
		}

		#endregion

		#region TestContainerYardOrganisations

		public void TestContainerYardOrganisations()
		{
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(ContainerYardCollection), BindToLists.ContainerYardOrganisations.GetType());
		}

		#endregion

		#region TestLocalTransportOrganisations

		public void TestLocalTransportOrganisations()
		{
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(LocalTransportCollection), BindToLists.LocalTransportOrganisations.GetType());
		}

		#endregion

		#endregion

		#region TestPackageCategories

		public void TestPackageCategories()
		{
			AssertEquals(typeof(PackageCategories), BindToLists.PackageCategories.GetType());
		}

		#endregion

		#region TestInstructionTypes

		public void TestInstructionTypes()
		{
			AssertContainsExactElementsInAnyOrder(new InstructionTypes().List, BindToLists.InstructionTypes);
		}

		#endregion

		#region TestStatuses

		public void TestStatuses()
		{
			AssertContainsExactElementsInAnyOrder(new TransportStatuses(), BindToLists.Statuses);
		}

		public void TestBookingStatuses()
		{
			AssertContainsExactElementsInAnyOrder(new BookingStatuses(), BindToLists.BookingStatuses);
		}

		public void TestBookingInstructionStatuses()
		{
			AssertContainsExactElementsInAnyOrder(new BookingInstructionStatuses(), BindToLists.BookingInstructionStatuses);
		}

		public void TestBookingConsolidationStatuses()
		{
			AssertContainsExactElementsInAnyOrder(new BookingConsolidationStatuses(), BindToLists.BookingConsolidationStatuses);
		}

		#endregion

		#region Implementation

		protected TransportBindToLists BindToLists
		{
			get { return bindToLists ?? (bindToLists = GetNewBindToLists()); }
		}

		protected virtual TransportBindToLists GetNewBindToLists()
		{
			return new TransportBindToLists(Factory);
		}

		TransportBindToLists bindToLists;

		#endregion
	}
}
