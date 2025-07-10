using Enterprise.TransportCommon.Business.Testing;

namespace Enterprise.TransportBookings.Business.Testing
{
	class DtbBookingInstructionTmplLookupsTest : DtbTransportInstructionTmplLookupsTest
	{
		public void TestOrganisationTypes()
		{
			var template = Factory.New<DtbBookingTmpl>();
			var instructionTemplate = template.Instructions.AddNew();

			AssertContainsExactElementsInAnyOrder(new BindToLists(Factory).OrganisationTypes, instructionTemplate.Lookups.OrganisationTypes);
		}

		public void TestInstructionTypes()
		{
			var template = Factory.New<DtbBookingTmpl>();
			var instructionTemplate = template.Instructions.AddNew();

			AssertContainsExactElementsInAnyOrder(new BindToLists(Factory).InstructionTypes, instructionTemplate.Lookups.InstructionTypes);
		}

		public void TestDropModes()
		{
			var template = Factory.New<DtbBookingTmpl>();
			var instructionTemplate = template.Instructions.AddNew();

			AssertContainsExactElementsInAnyOrder(new BindToLists(Factory).DropModes, instructionTemplate.Lookups.DropModes);
		}

		public void TestDefaultPackageTypes()
		{
			var template = Factory.New<DtbBookingTmpl>();
			var instructionTemplate = template.Instructions.AddNew();

			AssertContainsExactElementsInAnyOrder(new BindToLists(Factory).DefaultPackageTypes, instructionTemplate.Lookups.DefaultPackageTypes);
		}
	}
}
