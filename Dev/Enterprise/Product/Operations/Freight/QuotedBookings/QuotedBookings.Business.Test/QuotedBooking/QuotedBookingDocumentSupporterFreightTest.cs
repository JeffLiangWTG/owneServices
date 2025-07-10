using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	public class QuotedBookingDocumentSupporterFreightTest : BaseFreightTest
	{
		#region TestSupportedDataContext
		public void TestSupportedDataContext()
		{
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			AssertEquals("Core.Constants.DataContext.Shipment is Supported", true, quotedBooking.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.Shipment)));
			AssertEquals("Core.Constants.DataContext.CartageAdvice is Supported", true, quotedBooking.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.CartageAdvice)));
			AssertEquals("Core.Constants.DataContext.RequestForService is Supported", true, quotedBooking.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.RequestForService)));
			AssertEquals("Core.Constants.DataContext.Service is Supported", true, quotedBooking.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.Service)));
			AssertEquals("Core.Constants.DataContext.FreightLabels is Supported", true, quotedBooking.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.FreightLabels)));
			AssertEquals("Core.Constants.DataContext.GenericFreightJob is Supported", true, quotedBooking.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.GenericFreightJob)));
			AssertEquals("Core.Constants.DataContext.GenericFreightJobServices is Supported", true, quotedBooking.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.GenericFreightJobServices)));
			AssertEquals("Core.Constants.DataContext.GenericFreightJobByPackages is Supported", true, quotedBooking.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.GenericFreightJobByPackages)));
		}

		public void TestSupportedChildBusinessContexts()
		{
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			AssertEquals(1, quotedBooking.DocumentSupporter.SupportedChildBusinessContexts.Length);
			AssertCollectionContains(BusinessContext.DtbBooking, quotedBooking.DocumentSupporter.SupportedChildBusinessContexts);
		}

		#endregion
		#region TestGetDocBusinessObjectsForCartageAdvice
		public void TestGetDocBusinessObjectsForCartageAdvice()
		{
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			quotedBooking.Booking.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			JobVoyage voyage1 = Factory.New<JobVoyage>();
			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage1.GenerateSailings();
			JobSailing sailing1 = voyage1.Sailings[0];
			quotedBooking.Booking.JS_JX = sailing1.PK;
			DocumentWrapper[] wrappers = quotedBooking.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.CartageAdvice, StmMenuItem.GetForTesting(DocumentDirection.DEP));
			AssertEquals("Count", 1, wrappers.Length);
			AssertEquals("Wrapper type should be DocForwardingShipment", "DocForwardingShipment", wrappers[0].GetType().Name);
			quotedBooking.Booking.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			wrappers = quotedBooking.DocumentSupporter.GetDocumentWrappers(Enterprise.Core.Constants.DataContext.CartageAdvice, StmMenuItem.GetForTesting(DocumentDirection.DEP));
			AssertEquals("Count", 0, wrappers.Length);
			ForwardingContainer fCLBookingContainer1 = quotedBooking.QuotedBookingContainers.AddNew();
			ForwardingContainer fCLBookingContainer2 = quotedBooking.QuotedBookingContainers.AddNew();
			Factory.Save();
			wrappers = quotedBooking.DocumentSupporter.GetDocumentWrappers(Enterprise.Core.Constants.DataContext.CartageAdvice, StmMenuItem.GetForTesting(DocumentDirection.DEP));
			AssertEquals("Count", 2, wrappers.Length);
			AssertEquals("Wrapper type should be DocContainer", "DocContainer", wrappers[0].GetType().Name);
			AssertEquals("Wrapper type should be DocContainer", "DocContainer", wrappers[1].GetType().Name);
		}

		#endregion
		#region TestGenericFreightJobServicesWrapper
		public void TestGenericFreightJobServicesWrapper()
		{
			var dataContext = Constants.DataContext.GenericFreightJobServices;
			var quotedBooking = GetNewQuotedBooking();
			quotedBooking.Services.AddNew();
			var documentSupporter = (QuotedBookingDocumentSupporter)quotedBooking.DocumentSupporter;
			DocumentWrapper[] documentWrappersToTest = documentSupporter.GetDocumentWrappers(dataContext, null);
			var wrapper = documentWrappersToTest[0];
			var propertyInfo = wrapper.GetType().GetProperty("Services");
			var services = (DocumentWrapperCollection)propertyInfo.GetValue(wrapper, null);
			AssertEquals("1 Service expected.", 1, services.Count);
		}

		#endregion
		#region TestGenericFreightJobByPackages
		public void TestGenericFreightJobByPackages()
		{
			var quotedBooking = GetNewQuotedBooking();
			var packLine = quotedBooking.Booking.OuterPackLines.AddNew();
			packLine.FillWithValidTestData();
			packLine.JL_PackageCount = 5;
			AssertGetWrapperWithDocumentOptions(quotedBooking, Constants.DataContext.GenericFreightJobByPackages, 5, "FreightWrapperFromQuotedBooking");
		}

		void AssertGetWrapperWithDocumentOptions(QuotedBooking booking, Constants.DataContext context, int expectedCount, string expectedType)
		{
			var queryProvider = new Mock<ICommonShipmentDocumentSupporterQueryProvider>(MockBehavior.Strict);
			Factory.SetValue(() => queryProvider.Object);
			queryProvider.Setup(m => m.GetDocumentOptions(It.Is<DocumentShipment>((docShipment) => docShipment.Shipment == booking.Booking)))
				.Returns(new DocumentShipment(booking.Booking, context));

			var wrapper = booking.DocumentSupporter.GetDocumentWrappers(context, null);
			AssertEquals("Shipment wrapper should be created", expectedCount, wrapper.Length);
			var message = string.Format("Wrapper type should be {0}", expectedType);
			AssertEquals(message, expectedType, wrapper[0].GetType().Name);
			queryProvider.Verify(m => m.GetDocumentOptions(It.Is<DocumentShipment>((docShipment) => docShipment.Shipment == booking.Booking)), Times.Once());
		}

		#endregion
		public void TestGetDocBusinessObjectsForGenericWrapper()
		{
			DocumentWrapper[] wrappers = GetNewQuote().DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, StmMenuItem.GetForTesting(DocumentDirection.DEP));
			AssertEquals(1, wrappers.Length);
			AssertEquals("FreightWrapperFromOneOffQuote", wrappers[0].GetType().Name);
			wrappers = GetNewBooking().DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, StmMenuItem.GetForTesting(DocumentDirection.DEP));
			AssertEquals(1, wrappers.Length);
			AssertEquals("FreightWrapperFromQuotedBooking", wrappers[0].GetType().Name);
			wrappers = GetNewQuotedBooking().DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, StmMenuItem.GetForTesting(DocumentDirection.DEP));
			AssertEquals(1, wrappers.Length);
			AssertEquals("FreightWrapperFromQuotedBooking", wrappers[0].GetType().Name);
		}

		#region TestGetDocBusinessObjectForService
		public void TestGetDocBusinessObjectForService()
		{
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			DocumentWrapper[] wrapper = quotedBooking.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.Service, null);
			AssertEquals("Booking Shipment wrapper should be created", 1, wrapper.Length);
			AssertEquals("Wrapper type should be DocForwardingShipment", "DocForwardingShipment", wrapper[0].GetType().Name);
		}

		#endregion
		#region TestGetDocBusinessObjectsForRequestForService
		public void TestGetDocBusinessObjectsForRequestForService()
		{
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			var servicesSelectionProvider = new Mock<IServicesSelectionProvider>(MockBehavior.Strict);
			Factory.SetValue(() => servicesSelectionProvider.Object);
			servicesSelectionProvider.Setup(m => m.GetServicesToPrint(quotedBooking.Booking.DocsAndCartage)).Returns((JobService[])null);//.Repeat.Once();
			DocumentWrapper[] wrapper = quotedBooking.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.RequestForService, null);
			AssertEquals("wrapper should be null", null, wrapper);

			JobService service1 = quotedBooking.Booking.DocsAndCartage.Services.AddNew();
			JobService service2 = quotedBooking.Booking.DocsAndCartage.Services.AddNew();
			servicesSelectionProvider.Setup(m => m.GetServicesToPrint(quotedBooking.Booking.DocsAndCartage)).Returns(new JobService[] { service1, service2 });//.Repeat.Once();

			wrapper = quotedBooking.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.RequestForService, null);
			AssertEquals("Service Wrappers should be created", 2, wrapper.Length);
			AssertEquals("Wrapper type should be DocService", "DocService", wrapper[0].GetType().Name);
			AssertEquals("Wrapper type should be DocService", "DocService", wrapper[1].GetType().Name);
		}

		#endregion
		#region TestGetDocBusinessObjectForFreightLabels
		public void TestGetDocBusinessObjectForFreightLabels()
		{
			var quotedBooking = GetNewQuotedBooking();
			AssertGetWrapperWithDocumentOptions(quotedBooking, Constants.DataContext.FreightLabels, 1, "DocForwardingShipment");
		}

		public void TestGetDocBusinessObjectForFreightLabelsCheckContinuePrinting()
		{
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			var queryProvider = new Mock<ICommonShipmentDocumentSupporterQueryProvider>(MockBehavior.Strict);
			Factory.SetValue(() => queryProvider.Object);
			queryProvider.Setup(m => m.GetDocumentOptions(It.Is<DocumentShipment>((docShipment) => docShipment.Shipment == quotedBooking.Booking)))
				.Returns((DocumentShipment)null);//.Repeat.Once();

			DocumentWrapper[] wrapper = quotedBooking.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.FreightLabels, null);
			AssertNull("Shipment wrapper should not be created", wrapper);

			queryProvider.Setup(m => m.GetDocumentOptions(It.Is<DocumentShipment>((docShipment) => docShipment.Shipment == quotedBooking.Booking)))
				.Returns(new DocumentShipment(quotedBooking.Booking, Core.Constants.DataContext.FreightLabels));//.Repeat.Once();

			wrapper = quotedBooking.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.FreightLabels, null);
			AssertNotNull("Shipment wrapper should be created", wrapper);
		}

		#endregion
		#region TestGetDocBusinessObjectForBookingConfirmation
		public void TestGetDocBusinessObjectForBookingConfirmation()
		{
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			DocumentWrapper[] wrapper = quotedBooking.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.Shipment, null);
			AssertEquals("Booking Shipment wrapper should be created", 1, wrapper.Length);
			AssertEquals("Wrapper type should be DocForwardingShipment", "DocQuotedBooking", wrapper[0].GetType().Name);
		}

		public void TestGetDocBusinessObjectForQuotedBookingDataContext()
		{
			QuotedBooking quotedBooking = GetNewQuotedBooking();
			DocumentWrapper[] wrapper = quotedBooking.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.QuotedBooking, null);
			AssertEquals("Quoted Booking wrapper should be created", 1, wrapper.Length);
			AssertEquals("Wrapper type should be DocQuotedBooking", "DocQuotedBooking", wrapper[0].GetType().Name);
		}

		#endregion
		#region TestGetChildCollection_DtbBooking
		[GuiTest]
		public void TestGetChildCollection_DtbBooking()
		{
			var quotedBooking = GetNewQuotedBooking();
			var docSupporter = quotedBooking.DocumentSupporter;
			AssertEquals("normally should show", true, docSupporter.ShowReasonForNotPrinting(Constants.DataContext.None, null));
			var menu = Factory.New<StmMenuItem>();
			menu.SU_DocumentDirection = "DEP";
			AssertEquals(0, docSupporter.GetChildCollection(menu, BusinessContext.DtbBooking, null).Length);
			AssertEquals("Issues are handled in TB, so should not show reason for not printing", false, docSupporter.ShowReasonForNotPrinting(Constants.DataContext.None, menu));
			var savedUseInteractiveValue = Globals.IsUserInteractive;
			Globals.IsUserInteractive = false;
			try
			{
				AssertEquals(1, docSupporter.GetChildCollection(menu, BusinessContext.DtbBooking, null).Length);
				AssertEquals("Transport booking has been created, so should show reason for not printing", true, docSupporter.ShowReasonForNotPrinting(Constants.DataContext.None, menu));
			}
			finally
			{
				Globals.IsUserInteractive = savedUseInteractiveValue;
			}
		}

		#endregion
		#region Implementaion
		QuotedBooking GetNewQuote()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			return GetNewQuotedBooking(quote.PK, ZGuid.Empty);
		}

		QuotedBooking GetNewBooking()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			return GetNewQuotedBooking(ZGuid.Empty, booking.PK);
		}

		QuotedBooking GetNewQuotedBooking()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			return GetNewQuotedBooking(quote.PK, booking.PK);
		}

		QuotedBooking GetNewQuotedBooking(ZGuid quotePK, ZGuid bookingPK)
		{
			QuotedBooking quotedBooking = QuotedBooking.New(quotePK, bookingPK, Factory);
			quotedBooking.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			if (quotedBooking.Job != null)
			{
				quotedBooking.Job.JH_GE = Env.CurrentDepartment.PK;
			}

			Factory.Save();
			return quotedBooking;
		}
		#endregion
	}
}
