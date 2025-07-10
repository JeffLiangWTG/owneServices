using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.TransportBookings.Module.Testing
{
	abstract class BaseCreateDtbBookingsFromDtbBookingParentsApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestDirectionValidation()
		{
			Applicator.Direction = ZString.Empty;
			AssertHasError("Should be mandatory to enter Direction", Applicator.DirectionInfo, "Please enter a value.");

			Applicator.Direction = '\u001b'.ToString();
			AssertHasError("Should not allow non-Western European characters", Applicator.DirectionInfo, "Direction only accepts Western European languages characters.");

			Applicator.Direction = "XXX";
			AssertHasError("Should not allow invalid Direction", Applicator.DirectionInfo, "Enter a valid selection.");

			Applicator.Direction = "PIC";
			AssertNoErrors("Should allow PIC Direction", Applicator.DirectionInfo);

			Applicator.Direction = "DLV";
			AssertNoErrors("Should allow DLV Direction", Applicator.DirectionInfo);

			AssertExceptionThrown<MaxLengthExceededException>("Should throw MaxLengthExceededException if exceeding maximum direction length", () => Applicator.Direction = "XXXX");

			ErrorReporter.Clear();
		}

		public void TestBookingTemplateValidation()
		{
			Applicator.BookingTemplate = ZString.Empty;
			AssertHasError("Should be mandatory to enter BookingTemplate", Applicator.BookingTemplateInfo, "Please enter a value.");

			Applicator.BookingTemplate = '\u001b'.ToString();
			AssertHasError("BookingTemplate should not allow non-Western European characters", Applicator.BookingTemplateInfo, "Booking Template only accepts Western European languages characters.");

			Applicator.BookingTemplate = "XXXX";
			AssertHasError("Should not allow invalid BookingTemplate", Applicator.BookingTemplateInfo, "Enter a valid selection.");

			var query = new ZQuery();

			var bookingTemplateQuery = new ZDBOnlyQuery(typeof(DtbBookingTmpl));
			query.AddToFilter(bookingTemplateQuery);

			var bookingTmpls = Factory.Load<DtbBookingTmpl>(query);

			CombineAssertions("All booking templates in database should be valid", () =>
			{
				foreach (var bookingTmpl in bookingTmpls)
				{
					Applicator.BookingTemplate = bookingTmpl.KT_Code;
					AssertNoErrors("Should allow valid bookingTmpl " + bookingTmpl.KT_Code, Applicator.BookingTemplateInfo);
				}
			});

			AssertExceptionThrown<MaxLengthExceededException>("Should throw MaxLengthExceededException if exceeding maximum booking template length", () => Applicator.BookingTemplate = "XXXXX");

			ErrorReporter.Clear();
		}

		public void TestInvalidParameters()
		{
			var parents = CreateForwardingShipmentsToCreateDtbBookingsFrom();

			using (Applicator.GetValidationSuspender())
			{
				Applicator.Direction = "XXX";
				Applicator.BookingTemplate = "EFPR";

				var expectedLog = @"ERROR: Invalid Transport Booking direction chosen 'XXX', cannot create Transport Bookings";
				ApplyApplicator(parents, expectedLog);
			}
		}

		protected BusinessObject[] CreateForwardingShipmentsToCreateDtbBookingsFrom()
		{
			var setupFactory = new BusinessObjectFactory();
			var helper = new TransportBookingTestHelper(setupFactory);

			var shipment1PK = CreateForwardingShipmentWithConsolAndContainersAndReturnShipmentPK(helper, "S1", "C1", "LCL", "LCL");
			var shipment2PK = CreateForwardingShipmentWithConsolAndContainersAndReturnShipmentPK(helper, "S2", "C2", "LTL", "LTL");
			var shipment3PK = CreateForwardingShipmentWithConsolAndContainersAndReturnShipmentPK(helper, "S3", "C3", "LCL", "LCL");
			var shipment4PK = CreateForwardingShipmentWithConsolAndContainersAndReturnShipmentPK(helper, "S4", "C4", "FCL", "FCL");

			// need to load again otherwise containers do not populate on ForwardingShipment objects properly
			var shipment1 = Factory.Load<IForwardingShipment>(shipment1PK);
			var shipment2 = Factory.Load<IForwardingShipment>(shipment2PK);
			var shipment3 = Factory.Load<IForwardingShipment>(shipment3PK);
			var shipment4 = Factory.Load<IForwardingShipment>(shipment4PK);

			return new[] { shipment1, shipment2, shipment3, shipment4 }.Cast<BusinessObject>().ToArray();
		}

		ZGuid CreateForwardingShipmentWithConsolAndContainersAndReturnShipmentPK(TransportBookingTestHelper helper, string shipmentID, string consolID, string shipmentPackingMode, string containerMode)
		{
			var shipment = helper.CreateForwardingShipment(shipmentID, "", "SEA", shipmentPackingMode);
			var consol = helper.CreateForwardingConsol(shipment, consolID, "SEA", "");
			var containerA = helper.CreateForwardingContainer(consol, "CONT1A", "20GP");
			containerA.JC_ContainerMode = containerMode;
			var packlineA = (BusinessObject)helper.CreateForwardingPackline(shipment, containerA, 1);
			packlineA[JobPackLinesSchema.JL_ActualWeight] = 1;
			packlineA[JobPackLinesSchema.JL_ActualWeightUQ] = "KG";
			packlineA[JobPackLinesSchema.JL_ActualVolume] = 1;
			packlineA[JobPackLinesSchema.JL_ActualVolumeUQ] = "M3";
			var innerPacklineA = (BusinessObject)helper.CreateForwardingPackline(shipment, "PLT", 1);
			innerPacklineA[JobPackLinesSchema.JL_JL_OuterPackLine] = packlineA.PK;
			innerPacklineA[JobPackLinesSchema.JL_ActualWeight] = 1;
			innerPacklineA[JobPackLinesSchema.JL_ActualWeightUQ] = "KG";
			innerPacklineA[JobPackLinesSchema.JL_ActualVolume] = 1;
			innerPacklineA[JobPackLinesSchema.JL_ActualVolumeUQ] = "M3";
			innerPacklineA[JobPackLinesSchema.JL_FreightMode] = FreightConstants.InnerPackType;
			var containerB = helper.CreateForwardingContainer(consol, "CONT1B", "20GP");
			containerB.JC_ContainerMode = containerMode;
			var packlineB = (BusinessObject)helper.CreateForwardingPackline(shipment, containerB, 1);
			packlineB[JobPackLinesSchema.JL_ActualWeight] = 1;
			packlineB[JobPackLinesSchema.JL_ActualWeightUQ] = "KG";
			packlineB[JobPackLinesSchema.JL_ActualVolume] = 1;
			packlineB[JobPackLinesSchema.JL_ActualVolumeUQ] = "M3";
			var innerPacklineB = (BusinessObject)helper.CreateForwardingPackline(shipment, "PLT", 1);
			innerPacklineB[JobPackLinesSchema.JL_JL_OuterPackLine] = packlineB.PK;
			innerPacklineB[JobPackLinesSchema.JL_ActualWeight] = 1;
			innerPacklineB[JobPackLinesSchema.JL_ActualWeightUQ] = "KG";
			innerPacklineB[JobPackLinesSchema.JL_ActualVolume] = 1;
			innerPacklineB[JobPackLinesSchema.JL_ActualVolumeUQ] = "M3";
			innerPacklineB[JobPackLinesSchema.JL_FreightMode] = FreightConstants.InnerPackType;

			helper.Factory.Save();

			return shipment.PK;
		}
		new protected BaseCreateDtbBookingsFromDtbBookingParentsApplicator Applicator => (BaseCreateDtbBookingsFromDtbBookingParentsApplicator)base.Applicator;
	}
}
