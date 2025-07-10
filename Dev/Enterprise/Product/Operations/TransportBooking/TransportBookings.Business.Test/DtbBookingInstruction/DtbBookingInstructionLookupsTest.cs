using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.Business.Test
{
	class DtbBookingInstructionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAddToRelatedConsigneeConsignorFilter()
		{
			var cneOrg = Helper.CreateOrganisation("AAA");
			var cnrOrg = Helper.CreateOrganisation("BBB");
			var transportJob = GetNewInstruction().Booking;
			var cnrInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.PickUp);
			Helper.CreateInstruction(transportJob, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, cneOrg.MainAddress);

			cneOrg.SupplierLinks.AddNew(cnrOrg);
			cnrInstruction.OrganisationType = OrganisationTypesList.Codes.CNR;

			var cneCnrFilterDefaults = cnrInstruction.Lookups.ConsignorOrganisations.FilterBusinessObjectDefaults["Consignor - Related Consignee:Property"];
			AssertEquals("Precondition", "Consignor - Related Consignee", cneCnrFilterDefaults.FilterName);
			AssertEquals(cneOrg.PK, cneCnrFilterDefaults.Value);
		}

		public void TestAddToRelatedConsignorConsigneeFilter()
		{
			var cnrOrg = Helper.CreateOrganisation(OrganisationTypesList.Codes.CNR);
			var cneOrg = Helper.CreateOrganisation(OrganisationTypesList.Codes.CNE);
			var transportJob = GetNewInstruction().Booking;
			Helper.CreateInstruction(transportJob, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, cnrOrg.MainAddress);
			var cneInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.Delivery);

			cnrOrg.BuyerLinks.AddNew(cneOrg);
			cneInstruction.OrganisationType = OrganisationTypesList.Codes.CNE;

			var cnrCneFilterDefaults = cneInstruction.Lookups.ConsigneeOrganisations.FilterBusinessObjectDefaults["Consignee - Related Consignor:Property"];
			AssertEquals("Precondition", "Consignee - Related Consignor", cnrCneFilterDefaults.FilterName);
			AssertEquals(cnrOrg.PK, cnrCneFilterDefaults.Value);
		}

		public void TestDropModes()
		{
			var instruction = GetNewInstruction();
			AssertContainsExactElementsInAnyOrder(new TransportBindToLists(Factory).DropModes, instruction.Lookups.DropModes);
		}

		public void TestOrganisationTypes()
		{
			var instruction = GetNewInstruction();
			AssertContainsExactElementsInAnyOrder(OrganisationTypesList.Instance, instruction.Lookups.OrganisationTypes);
		}

		public void TestAllOrganisations()
		{
			var instruction = GetNewInstruction();
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(OrgHeaderCollection), instruction.Lookups.AllOrganisations.GetType());
		}

		public void TestConsigneeOrganisations()
		{
			var instruction = GetNewInstruction();
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(ConsigneeCollection), instruction.Lookups.ConsigneeOrganisations.GetType());
		}

		public void TestConsignorOrganisations()
		{
			var instruction = GetNewInstruction();
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(ConsignorCollection), instruction.Lookups.ConsignorOrganisations.GetType());
		}

		public void TestCFSOrganisations()
		{
			var instruction = GetNewInstruction();
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(DepotCollection), instruction.Lookups.CFSOrganisations.GetType());
		}

		public void TestCTOOrganisations()
		{
			var instruction = GetNewInstruction();
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(CTOCollection), instruction.Lookups.CTOOrganisations.GetType());
		}

		public void TestContainerYardOrganisations()
		{
			var instruction = GetNewInstruction();
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(ContainerYardCollection), instruction.Lookups.ContainerYardOrganisations.GetType());
		}

		public void TestPackageCategories()
		{
			var instruction = GetNewInstruction();
			AssertEquals(typeof(PackageCategories), instruction.Lookups.PackageCategories.GetType());
		}

		public void TestInstructionTypes()
		{
			var instruction = GetNewInstruction();
			AssertContainsExactElementsInAnyOrder(new InstructionTypes().List, instruction.Lookups.InstructionTypes);
		}

		public void TestBindToLists_IsInstructionSpecified()
		{
			var booking = GetNewInstruction().Booking;
			var org1 = Helper.CreateOrganisation(OrganisationTypesList.Codes.CNR + "1");
			var org2 = Helper.CreateOrganisation(OrganisationTypesList.Codes.CNR + "2");

			var instruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, org1.Addresses.MainAddress);
			var bindToList1 = instruction1.Lookups.BindToLists;

			var instruction2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, org2.Addresses.MainAddress);
			var bindToList2 = instruction2.Lookups.BindToLists;

			AssertNotEquals("Lookups of different instruction should have different BindToList", bindToList1, bindToList2);
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;

		DtbBookingInstruction GetNewInstruction()
		{
			var booking = Helper.CreateBooking();
			return booking.Instructions.AddNew();
		}
	}
}
