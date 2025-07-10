using Enterprise.Core;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Test
{
	[TestedType(typeof(DtbBookingInstructionProcessTaskCollection))]
	public class DtbBookingInstructionProcessTaskCollectionTest : ProcessTaskCollectionTest<DtbBookingInstructionProcessTaskCollection>
	{
		protected override DtbBookingInstructionProcessTaskCollection GetCollectionToTestCore()
		{
			var template = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.PickUp);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.Delivery);

			var booking = Helper.CreateBooking();
			booking.KM_KT_NKBookingTemplate = template.KT_Code;

			return new DtbBookingInstructionProcessTaskCollection(booking.Instructions[0]);
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}
}
