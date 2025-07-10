using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	public class DtbBookingConsolidationMultiJobDataObjectReaderTest : DtbBookingTestCaseWithFactory
	{
		public void TestDataContextType()
		{
			var reader = new DtbBookingConsolidationMultiJobDataObjectReader(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance), Logger, new UniversalObjectFactory());
			AssertEquals(DataContextType.TransportBookingConsolidation, reader.DataContextType);
		}

		public void TestGetNewBusinessObject()
		{
			var reader = new DtbBookingConsolidationMultiJobDataObjectReader(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance), Logger, new UniversalObjectFactory());
			var consolidation = reader.ReadIntoBusinessObject();

			AssertNotNull(consolidation);
			AssertEquals(true, consolidation.IsMultiBooking);
			AssertEquals(TransportConsolidationJobTypes.Codes.BookingTransportConsolidation, consolidation.KB_JobType);
		}

		public void TestSingleJobConsolidationsMadeForEachBooking()
		{
			var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var bookingDataObject1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var bookingDataObject2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			consolidationDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { bookingDataObject1, bookingDataObject2 });

			var reader1 = new DtbBookingConsolidationMultiJobDataObjectReader(consolidationDataObject, Logger, new UniversalObjectFactory());
			var consolidationWithTwoBookings = reader1.ReadIntoBusinessObject();
			AssertEquals("Should have read in both Bookings as Consolidation was top Level.", 2, consolidationWithTwoBookings.Bookings.Count);

			AssertEquals("Both bookings should have same multijob consolidation", consolidationWithTwoBookings.Bookings[0].ConsolidationMultiJob, consolidationWithTwoBookings.Bookings[1].ConsolidationMultiJob);
			AssertNotEquals("Bookings should have unique singlejob consolidations", consolidationWithTwoBookings.Bookings[0].ConsolidationSingleJob, consolidationWithTwoBookings.Bookings[1].ConsolidationSingleJob);
		}

		public void TestPopulateAddresses()
		{
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "New Local Client Co";
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.AddOrgAddress(writeManager, org.MainAddress, DocAddressType.TransportCompanyDocumentaryAddress);

			var reader1 = new DtbBookingConsolidationMultiJobDataObjectReader(shipment, Logger, new UniversalObjectFactory());
			var bookingFromReader1 = reader1.ReadIntoBusinessObject();
			AssertEquals("New Local Client Co", bookingFromReader1.DocAddresses.FindByDocAddressType(DocAddressType.TransportCompanyDocumentaryAddress).E2_CompanyName);
		}

		public void TestPopulatePackagesOnChildConsolidation()
		{
			var packageDataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packageDataObject.PackType = new PackageType() { Code = "" }; // customs sets this to blank
			packageDataObject.GoodsDescription = "My Pack";

			var containerDataObject = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject.ContainerNumber = "CONT1234567";
			containerDataObject.ContainerType = new ContainerType() { Code = "20GP" };

			var childConsolShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			childConsolShipment.DataContext = DataContextFactory.New();
			childConsolShipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			childConsolShipment.DataContext.AddDataSource(DataContextType.CustomsDeclaration, "C123");
			Logger.TopLevelDataObject = childConsolShipment;
			childConsolShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packageDataObject });
			childConsolShipment.SetContainerCollection(() => new DataObjectList<Container> { containerDataObject });
			childConsolShipment.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "ILDV" };

			var childConsolWithoutPackagesShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			var multiJobConsolShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ShipmentType = new CodeDescriptionPair { Code = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation },
			};
			multiJobConsolShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { childConsolShipment, childConsolWithoutPackagesShipment });

			var reader = new DtbBookingConsolidationMultiJobDataObjectReader(multiJobConsolShipment, Logger, new UniversalObjectFactory());
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertEquals(2, consolidationRead.Bookings.Count);
			var booking1 = consolidationRead.Bookings[0];
			AssertEquals("ILDV", booking1.KM_KT_NKBookingTemplate);
			AssertEquals(2, booking1.Instructions.Count);
			AssertEquals(1, booking1.AssignedPackages.Count);
			AssertEquals("My Pack", booking1.AssignedPackages[0].KP_GoodsDescription);

			var booking2 = consolidationRead.Bookings[1];
			AssertEquals(0, booking2.Instructions.Count);
			AssertEquals(0, booking2.AssignedPackages.Count);
		}

		public void TestPopulateBusinessObject_JobTypeChanged_HasBookings_ThrowsException()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobType = TransportConsolidationJobTypes.Codes.Booking;
			Helper.CreateBooking(consolidation);

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var universalFactory = new UniversalObjectFactory();
			var topLevelReader = (ITopLevelDataObjectReader)new DtbBookingConsolidationMultiJobDataObjectReader(consol, Logger, universalFactory);
			var con = (BusinessObject)consolidation;
			AssertExceptionThrown(typeof(DataObjectReadFailureException), DtbBookingConsolidationSchema.Constants.KB_JobType + " cannot be changed from BKG if the Consolidation already has Bookings.", () => topLevelReader.ReadIntoBusinessObject(ref con));
		}

		public void TestPopulateBusinessObject_JobTypeNotChanged_DoesNotThrowException()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobType = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;
			Helper.CreateBooking(consolidation);

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var universalFactory = new UniversalObjectFactory();
			var topLevelReader = (ITopLevelDataObjectReader)new DtbBookingConsolidationMultiJobDataObjectReader(consol, Logger, universalFactory);
			var con = (BusinessObject)consolidation;
			AssertNoExceptionThrown(() => topLevelReader.ReadIntoBusinessObject(ref con));
		}

		public void TestPopulateBusinessObject_JobTypeChanged_NoBookings_DoesNotThrowException()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobType = TransportConsolidationJobTypes.Codes.Booking;

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var universalFactory = new UniversalObjectFactory();
			var topLevelReader = (ITopLevelDataObjectReader)new DtbBookingConsolidationMultiJobDataObjectReader(consol, Logger, universalFactory);
			var con = (BusinessObject)consolidation;
			AssertNoExceptionThrown(() => topLevelReader.ReadIntoBusinessObject(ref con));
		}

		protected override void SetUp()
		{
			base.SetUp();
			Logger = new TestErrorLogger();
		}

		TestErrorLogger Logger;
	}
}
