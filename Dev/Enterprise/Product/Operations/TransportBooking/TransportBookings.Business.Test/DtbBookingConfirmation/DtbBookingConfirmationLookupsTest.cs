using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportBookings.Business.Testing
{
	public class DtbBookingConfirmationLookupsTest : DtbTransportConfirmationLookupsTest
	{
		public void TestConfirmationTypes()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew();
			var confirmation = instruction.Confirmations.AddNew();

			AssertContainsExactElementsInAnyOrder(new BindToLists(Factory).GetConfirmationTypes("", "", ""), confirmation.Lookups.ConfirmationTypes);

			instruction.OrganisationType = "CTO";
			AssertContainsExactElementsInAnyOrder(new BindToLists(Factory).GetConfirmationTypes("", "", "CTO"), confirmation.Lookups.ConfirmationTypes);

			instruction.OrganisationType = "CNE";
			AssertContainsExactElementsInAnyOrder(new BindToLists(Factory).GetConfirmationTypes("", "", "CNE"), confirmation.Lookups.ConfirmationTypes);
		}

		public void TestInstructionOrPackageDivots()
		{
			var booking = Helper.CreateBooking();
			var instruction = booking.Instructions.AddNew();
			var divot1 = instruction.PackageDivots.AddNew();
			var divot2 = instruction.PackageDivots.AddNew();
			var package1 = Factory.New<PkgPackage>();
			var package2 = Factory.New<PkgPackage>();
			divot1.KD_KP_Package = package1.PK;
			divot2.KD_KP_Package = package2.PK;
			var confirmation = instruction.Confirmations.AddNew();

			var expected = new CodeDescriptionPairList();
			expected.Add(new InstructionOrPackageDivotCodeDescriptionPair(instruction.PK, "ALL", (NoResString)"Applies to All Packages on Instruction"));
			expected.Add(new InstructionOrPackageDivotCodeDescriptionPair(divot1.PK, divot1.PackageDescriptionWithIDAndQty, (NoResString)string.Format("Applies to '{0}' Only", divot1.PackageDescriptionWithIDAndQty)));
			expected.Add(new InstructionOrPackageDivotCodeDescriptionPair(divot2.PK, divot2.PackageDescriptionWithIDAndQty, (NoResString)string.Format("Applies to '{0}' Only", divot2.PackageDescriptionWithIDAndQty)));
			AssertContainsExactElementsInAnyOrder(expected, confirmation.Lookups.InstructionOrPackageDivots);
		}

		public void TestDriversReturnsCorrectOrgContacts()
		{
			var organisation1 = Helper.CreateOrganisation("ABCDEF");
			var organisation2 = Helper.CreateOrganisation("GHIJKL");

			var contact1 = organisation1.Contacts.AddNew();
			var contact2 = organisation2.Contacts.AddNew();

			Factory.Save();

			var org1Booking = Helper.CreateBooking(organisation1);
			var org1Instruction = org1Booking.Instructions.AddNew();
			var org1Confirmation = org1Instruction.Confirmations.AddNew();

			var org2Booking = Helper.CreateBooking(organisation2);
			var org2Instruction = org2Booking.Instructions.AddNew();
			var org2Confirmation = org2Instruction.Confirmations.AddNew();

			var noOrgBooking = Helper.CreateBooking();
			var noOrgInstruction = noOrgBooking.Instructions.AddNew();
			var noOrgConfirmation = noOrgInstruction.Confirmations.AddNew();

			Factory.Save();

			var org1Lookup = new DtbBookingConfirmationLookups(org1Confirmation);
			var org2Lookup = new DtbBookingConfirmationLookups(org2Confirmation);
			var noOrgLookup = new DtbBookingConfirmationLookups(noOrgConfirmation);

			AssertCollectionContains("Should contain Organisation 1's driver.", contact1, org1Lookup.Drivers);
			AssertCollectionContains("Should contain Organisation 2's driver.", contact2, org2Lookup.Drivers);
			AssertEquals("Should contain an empty list of OrgContacts", 0, noOrgLookup.Drivers.Count);
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;
	}
}
