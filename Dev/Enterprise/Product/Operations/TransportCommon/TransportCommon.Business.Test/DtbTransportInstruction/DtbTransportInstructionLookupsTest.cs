using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DtbTransportInstructionLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestAddToRelatedConsigneeConsignorFilter

		public void TestAddToRelatedConsigneeConsignorFilter()
		{
			var cneOrg = TransportHelper.CreateOrganisation("AAA");
			var cnrOrg = TransportHelper.CreateOrganisation("BBB");
			var transportJob = GetNewInstruction().Booking;
			var cnrInstruction = TransportHelper.CreateInstruction(transportJob, InstructionTypes.Codes.PickUp);
			TransportHelper.CreateInstruction(transportJob, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, cneOrg.MainAddress);

			cneOrg.SupplierLinks.AddNew(cnrOrg);
			cnrInstruction.OrganisationType = OrganisationTypesList.Codes.CNR;

			var cneCnrFilterDefaults = cnrInstruction.Lookups.ConsignorOrganisations.FilterBusinessObjectDefaults["Consignor - Related Consignee:Property"];
			AssertEquals("Precondition", "Consignor - Related Consignee", cneCnrFilterDefaults.FilterName);
			AssertEquals(cneOrg.PK, cneCnrFilterDefaults.Value);
		}

		#endregion

		#region TestAddToRelatedConsignorConsigneeFilter

		public void TestAddToRelatedConsignorConsigneeFilter()
		{
			var cnrOrg = TransportHelper.CreateOrganisation(OrganisationTypesList.Codes.CNR);
			var cneOrg = TransportHelper.CreateOrganisation(OrganisationTypesList.Codes.CNE);
			var transportJob = GetNewInstruction().Booking;
			TransportHelper.CreateInstruction(transportJob, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, cnrOrg.MainAddress);
			var cneInstruction = TransportHelper.CreateInstruction(transportJob, InstructionTypes.Codes.Delivery);

			cnrOrg.BuyerLinks.AddNew(cneOrg);
			cneInstruction.OrganisationType = OrganisationTypesList.Codes.CNE;

			var cnrCneFilterDefaults = cneInstruction.Lookups.ConsigneeOrganisations.FilterBusinessObjectDefaults["Consignee - Related Consignor:Property"];
			AssertEquals("Precondition", "Consignee - Related Consignor", cnrCneFilterDefaults.FilterName);
			AssertEquals(cnrOrg.PK, cnrCneFilterDefaults.Value);
		}

		#endregion

		#region TestDropModes

		public void TestDropModes()
		{
			var instruction = GetNewInstruction();
			AssertContainsExactElementsInAnyOrder(new TransportBindToLists(Factory).DropModes, instruction.Lookups.DropModes);
		}

		#endregion

		#region TestOrganisationTypes

		public void TestOrganisationTypes()
		{
			var instruction = GetNewInstruction();
			AssertContainsExactElementsInAnyOrder(OrganisationTypesList.Instance, instruction.Lookups.OrganisationTypes);
		}

		#endregion

		#region TestOrganisations

		#region TestAllOrganisations

		public void TestAllOrganisations()
		{
			var instruction = GetNewInstruction();
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(OrgHeaderCollection), instruction.Lookups.AllOrganisations.GetType());
		}

		#endregion

		#region TestConsigneeOrganisations

		public void TestConsigneeOrganisations()
		{
			var instruction = GetNewInstruction();
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(ConsigneeCollection), instruction.Lookups.ConsigneeOrganisations.GetType());
		}

		#endregion

		#region TestConsignorOrganisations

		public void TestConsignorOrganisations()
		{
			var instruction = GetNewInstruction();
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(ConsignorCollection), instruction.Lookups.ConsignorOrganisations.GetType());
		}

		#endregion

		#region TestCFSOrganisations

		public void TestCFSOrganisations()
		{
			var instruction = GetNewInstruction();
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(DepotCollection), instruction.Lookups.CFSOrganisations.GetType());
		}

		#endregion

		#region TestCTOOrganisations

		public void TestCTOOrganisations()
		{
			var instruction = GetNewInstruction();
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(CTOCollection), instruction.Lookups.CTOOrganisations.GetType());
		}

		#endregion

		#region TestContainerYardOrganisations

		public void TestContainerYardOrganisations()
		{
			var instruction = GetNewInstruction();
			AssertEquals("The type of OrgHeaderCollection is incorrect.", typeof(ContainerYardCollection), instruction.Lookups.ContainerYardOrganisations.GetType());
		}

		#endregion

		#endregion

		#region TestPackageCategories

		public void TestPackageCategories()
		{
			var instruction = GetNewInstruction();
			AssertEquals(typeof(PackageCategories), instruction.Lookups.PackageCategories.GetType());
		}

		#endregion

		#region TestInstructionTypes

		public void TestInstructionTypes()
		{
			var instruction = GetNewInstruction();
			AssertContainsExactElementsInAnyOrder(new InstructionTypes().List, instruction.Lookups.InstructionTypes);
		}

		#endregion

		#region TestBindToLists

		public void TestBindToLists_IsInstructionSpecified()
		{
			var transport = GetNewInstruction().Booking;
			var org1 = TransportHelper.CreateOrganisation(OrganisationTypesList.Codes.CNR + "1");
			var org2 = TransportHelper.CreateOrganisation(OrganisationTypesList.Codes.CNR + "2");

			var instruction1 = TransportHelper.CreateInstruction(transport, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, org1.Addresses.MainAddress);
			var bindToList1 = instruction1.Lookups.BindToLists;

			var instruction2 = TransportHelper.CreateInstruction(transport, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, org2.Addresses.MainAddress);
			var bindToList2 = instruction2.Lookups.BindToLists;

			AssertNotEquals("Lookups of different instruction should have different BindToList", bindToList1, bindToList2);
		}

		#endregion

		#region Implementation

		protected abstract DtbTransportInstruction GetNewInstruction();

		protected TransportCommonTestHelper TransportHelper
		{
			get { return helper ?? (helper = new TransportCommonTestHelper(Factory)); }
		}
		TransportCommonTestHelper helper;

		#endregion
	}
}
