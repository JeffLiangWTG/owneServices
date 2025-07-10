using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(QuotedBookingDocumentSupporter))]
	public class QuotedBookingDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestDocumentSupporterDataStateWhenNotForwardingRegistered()
		{
			var query = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, "Cartage Advice");

			var command = Factory.LoadTop1<DocumentCommand>(query);

			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.Booking.JS_IsForwardRegistered = false;
			var supporter = quotedBooking.DocumentSupporter;

			var docData = supporter.GetDataStateBeforeRun(command);

			AssertEquals("No error message should be present on document data state if creating Transport Booking is allowed.", string.Empty, docData.ErrorMessage);
		}

		public void TestDocumentSupporterDataStateWhenForwardingRegistered()
		{
			var query = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, "Cartage Advice");

			var command = Factory.LoadTop1<DocumentCommand>(query);

			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.Booking.JS_IsForwardRegistered = true;
			var supporter = quotedBooking.DocumentSupporter;

			var docData = supporter.GetDataStateBeforeRun(command);

			AssertEquals("Document data state should have error message if creating Transport Booking is not allowed.", "You cannot create Cartage Advice for a Booking which has been converted to a Shipment. Run the Cartage Advice document directly from the converted Shipment instead.", docData.ErrorMessage);
		}

		public void TestDocumentSupporterDataStateWhenForwardingRegisteredAndHasTransportBooking()
		{
			var query = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, "Cartage Advice");

			var command = Factory.LoadTop1<DocumentCommand>(query);

			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.Booking.JS_IsForwardRegistered = true;
			var supporter = quotedBooking.DocumentSupporter;

			var transportBooking = Factory.New<IDtbBooking>();
			var consol = Factory.New<IDtbBookingConsolidation>();

			transportBooking.KM_KB_Booking = consol.PK;
			consol.KB_ParentID = quotedBooking.PK;

			var docData = supporter.GetChildCollection(command, BusinessContext.DtbBooking, command);

			AssertEquals("Document data state should be an empty array if creating Transport Booking is not allowed and a Transport Booking already exists.", Array.Empty<IDocumentSupportable>(), docData);
		}

		public void TestGetBODocDataProvidersNotFoundMessage_NoContainers()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.Booking.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			quotedBooking.QuotedBookingContainers.RemoveAndDeleteAll();

			var contextArray = new[]
			{
				Constants.DataContext.CartageAdvice,
			};

			var menu = Factory.New<StmMenuItem>();
			var expectedMessage = "This booking does not have any containers.";

			AssertNotFoundMessage(quotedBooking, menu, contextArray, true, expectedMessage);
		}

		public void TestBookingCustomFieldsAreUsedForShipmentContexts()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var vbField = AddCustomField(quotedBooking, "test1", "qbk");
			var jsField = AddCustomField(quotedBooking.Booking, "test1", "shp");
			Factory.Save();

			CombineAssertions("preconditions", () =>
			{
				AssertEquals("Booking has same PK for quick bookings", vbField.XV_ParentID, jsField.XV_ParentID);
				AssertNotEquals("VB != JS", vbField.XV_ParentTableCode, jsField.XV_ParentTableCode);
			});

			AssertBookingCustomFieldsAreUsed(quotedBooking, Constants.DataContext.Shipment);
			AssertBookingCustomFieldsAreUsed(quotedBooking, Constants.DataContext.FreightLabels);
			AssertBookingCustomFieldsAreUsed(quotedBooking, Constants.DataContext.GenericFreightJob);
		}

		public void TestDocumentEventIsCreatedUponDocumentDelivery_QuickBooking() => AssertDocumentEventIsCreatedUponDocumentDelivery(QuotedBooking.New(QuoteBookingType.QuickBooking, Factory));

		public void TestDocumentEventIsCreatedUponDocumentDelivery_BookingWithQuote() => AssertDocumentEventIsCreatedUponDocumentDelivery(QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory));

		void AssertDocumentEventIsCreatedUponDocumentDelivery(QuotedBooking quotedBooking)
		{
			var docType = Factory.New<RefDocType>();
			docType.RT_SE_NKDocumentReceivedEvent = Events.CustomisableEvent00.Code;
			docType.RT_DocType = "AAA";
			docType.RT_ReferenceType = "ALL";

			var command = Factory.Load<DocumentCommand>(new ZGuid("CA5830A6-5285-4648-A331-B44C73A6DC83"));
			AssertEquals("loaded Booking Confirmation", "Booking Confirmation", command.SU_MenuName);
			AssertEquals("loaded Booking Confirmation for Quoted Booking module", "QuotedBooking", command.SU_BusinessContext);

			foreach (var document in command.Documents.Cast<StmMenuTemplatePivot>())
			{
				document.SI_RT_DocType = docType.PK;
			}

			Factory.Save();

			var pack = new DocumentPack(command);
			pack.AddReportsToPack(command, null, quotedBooking, null);
			AssertEquals("prerequisite: added report to the pack", 1, pack.Count);

			var contact = new DocDeliveryContact(Factory);
			contact.AttachmentType = "PDF";
			contact.DeliveryMethod = "EML";
			contact.Email = "contact@test.com.au";

			var instructions = new DeliveryInstructions(pack);
			instructions.Destination = DeliveryInstructionDestination.Auto;
			instructions.Recipients.RemoveAndDeleteAll();
			instructions.Recipients.Add(contact);

			using (var printTask = new PrintTask(command))
			{
				printTask.Add(pack);
				printTask.Run(instructions);
			}

			var documentLogQuery = new ZQuery();
			documentLogQuery.AddToFilter(StmALogSchema.SL_Parent, quotedBooking.Booking.PK);
			documentLogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CustomisableEvent00.Code);

			var documentLog = Factory.Load<StmALog>(documentLogQuery);
			Assert("document log has been created", documentLog.Length > 0);
		}

		void AssertBookingCustomFieldsAreUsed(QuotedBooking quotedBooking, Constants.DataContext context)
		{
			var supporter = quotedBooking.DocumentSupporter;
			var wrappers = supporter.GetDocumentWrappers(context, null);
			foreach (var wrapper in wrappers)
			{
				AssertEquals($"{context} should find custom field", "qbk", wrapper.GetCustomField("test1"));
			}
		}

		GenCustomAddOnValue AddCustomField(BusinessObject bizo, ZString customValueName, ZString value, string type = AddOnColumnDataType.Codes.String)
		{
			var customAddOnValue = Factory.New<GenCustomAddOnValue>();
			customAddOnValue.XV_ParentID = bizo.PK;
			customAddOnValue.XV_ParentTableCode = bizo.TablePrefix;
			customAddOnValue.XV_Name = customValueName;
			customAddOnValue.XV_Type = type;
			customAddOnValue.XV_Data = value;

			return customAddOnValue;
		}

		protected override bool ShouldSkipWithContextAndMenu(Constants.DataContext context, IStmMenuItem menu)
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			var supporter = quotedBooking.DocumentSupporter;
			var wrappers = supporter.GetDocumentWrappers(context, menu);

			return wrappers != null && wrappers.Length > 0 && wrappers.All(c => c != null);
		}

		protected override IEnumerable<IDocumentSupportable> TopLevelBOsForMessageNotPrintingTest
		{
			get { return new[] { QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory) }; }
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			return documentCommand.SU_MenuName.Contains("Cartage Advice", StringComparison.InvariantCultureIgnoreCase) || base.ExcludeDocumentCommandTest(documentCommand);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);

			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_OuterPacks = 1;

			var pakcline = booking.OuterPackLines.AddNew();
			pakcline.JL_PackageCount = 5;

			var docAndCartage = booking.DocsAndCartage;
			docAndCartage.Services.AddNew();

			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			quotedBooking.QuotedBookingContainers.AddNew();
			quotedBooking.Booking.OuterPackLines.AddNew();

			if (quotedBooking.Job != null)
			{
				quotedBooking.Job.JH_GE = Env.CurrentDepartment.PK;
			}

			return quotedBooking;
		}
	}
}
