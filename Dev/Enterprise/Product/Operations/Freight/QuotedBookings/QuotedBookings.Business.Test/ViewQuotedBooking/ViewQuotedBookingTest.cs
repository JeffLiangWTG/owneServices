using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.UniversalCopy.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(ViewQuotedBooking))]
	public class ViewQuotedBookingTest : EnterpriseBusinessObjectTestCase
	{
		#region TestQuotedBooking

		public void TestIsNotPersistent()
		{
			var viewQuotedBooking = Factory.New<ViewQuotedBooking>();
			AssertNoExceptionThrown("should not throw CargoWise.EntityFramework.ZSaveException", () => Factory.Save());
		}

		public void TestLoadOrCreate()
		{
			AssertLoadOrCreate(QuotedBooking.New(QuoteBookingType.SpotQuote, Factory));
			AssertLoadOrCreate(QuotedBooking.New(QuoteBookingType.QuickBooking, Factory));
			AssertLoadOrCreate(QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory));
		}

		void AssertLoadOrCreate(QuotedBooking quotedBooking)
		{
			if (quotedBooking.Booking != null)
			{
				AssertEquals("prerequsite", false, quotedBooking.Booking.IsInDatabase);
			}

			if (quotedBooking.Quote != null)
			{
				AssertEquals("prerequsite", false, quotedBooking.Quote.IsInDatabase);
			}

			var viewQuotedBooking = ViewQuotedBooking.LoadOrCreate(quotedBooking);
			AssertNotNull(viewQuotedBooking);
			AssertViewQuotedBooking(viewQuotedBooking, quotedBooking);
			Factory.Save();
			AssertViewQuotedBookingAuditColumnsPostSave(viewQuotedBooking, quotedBooking);
		}

		void AssertViewQuotedBooking(ViewQuotedBooking viewQuotedBooking, QuotedBooking quotedBooking)
		{
			AssertEquals(viewQuotedBooking.PK, quotedBooking.PK);
			AssertEquals(viewQuotedBooking.VB_JS, quotedBooking.Booking != null ? quotedBooking.Booking.PK : ZGuid.Empty);
			AssertEquals(viewQuotedBooking.VB_TH, quotedBooking.Quote != null ? quotedBooking.Quote.PK : ZGuid.Empty);
		}

		void AssertViewQuotedBookingAuditColumnsPostSave(ViewQuotedBooking viewQuotedBooking, QuotedBooking quotedBooking)
		{
			var quotedBookingWithAuditDetails = quotedBooking as IAuditDetails;
			AssertEquals(viewQuotedBooking.VB_SystemCreateTimeUtc, quotedBookingWithAuditDetails.SystemCreateTimeUtc);
			AssertEquals(viewQuotedBooking.VB_SystemCreateUser, quotedBookingWithAuditDetails.SystemCreateUser);
			AssertEquals(viewQuotedBooking.VB_SystemLastEditTimeUtc, quotedBookingWithAuditDetails.SystemLastEditTimeUtc);
			AssertEquals(viewQuotedBooking.VB_SystemLastEditUser, quotedBookingWithAuditDetails.SystemLastEditUser);
		}

		public void TestIWorkflowProvider()
		{
			ViewQuotedBooking viewQuotedBooking = Factory.New<ViewQuotedBooking>();
			AssertNotNull(viewQuotedBooking);
			AssertNull(viewQuotedBooking.QuotedBooking);
			IWorkflowProvider provider = viewQuotedBooking;
			AssertNull(provider.GetTemplateSelectionCriteria());
			AssertNull(provider.GetWorkflowInformationProvider());
			AssertEquals(0, provider.WorkflowItems.Count);
			QuotedBooking quotedBooking = QuotedBooking.New(Integration.QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.WorkflowItems.AddNew();
			viewQuotedBooking.VB_JS = quotedBooking.Booking.PK;
			viewQuotedBooking.VB_TH = quotedBooking.Quote.PK;
			IWorkflowProvider prov1 = quotedBooking;
			IWorkflowProvider prov2 = viewQuotedBooking;
			AssertEquals(prov1.WorkflowItems.GetType(), prov2.WorkflowItems.GetType());
			AssertContainsExactElementsInAnyOrder(prov1.WorkflowItems, prov2.WorkflowItems);
			AssertEquals(prov1.WorkflowType, prov2.WorkflowType);
			AssertEquals(WorkflowDescriptors.QuotedBookingWorkflowDescriptorCode, prov2.WorkflowType);
			AssertEquals(1, prov2.WorkflowItems.Count);
			AssertEquals(1, prov1.WorkflowItems.Count);
		}

		public void TestQuotedBooking_ScreeningStatus()
		{
			var viewQuotedBooking = Factory.New<ViewQuotedBooking>();
			AssertEquals("Status should return empty string on initialisation", ZString.Empty, viewQuotedBooking.ScreeningStatus);
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			viewQuotedBooking.VB_JS = booking.PK;
			viewQuotedBooking.VB_TH = quote.PK;
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, viewQuotedBooking.ScreeningStatus);
			((ITemplateRecordProvider)viewQuotedBooking.QuotedBooking).IsTemplateRecord = true;
			AssertEquals("Status should return empty string if quoted booking is a template", ZString.Empty, viewQuotedBooking.ScreeningStatus);
		}

		public void TestQuotedBooking_Cache()
		{
			ViewQuotedBooking viewQuotedBooking = Factory.New<ViewQuotedBooking>();
			AssertNull("Quoted Booking should return null as VB_JS & VB_TH are empty", viewQuotedBooking.QuotedBooking);
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			viewQuotedBooking.VB_JS = booking.PK;
			viewQuotedBooking.VB_TH = quote.PK;
			QuotedBooking quotedBooking = viewQuotedBooking.QuotedBooking;
			AssertNotNull("Should not be null", quotedBooking);
			AssertEquals("Should Cache Quoted Booking", quotedBooking, viewQuotedBooking.QuotedBooking);
		}

		public void TestQuotedBooking_QuoteOnly()
		{
			ViewQuotedBooking viewQuotedBooking = Factory.New<ViewQuotedBooking>();
			AssertNull("Quoted Booking should return null as VB_JS & VB_TH are empty", viewQuotedBooking.QuotedBooking);
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			viewQuotedBooking.VB_TH = quote.PK;
			QuotedBooking quotedBooking = viewQuotedBooking.QuotedBooking;
			AssertNotNull("Should not be null", quotedBooking);
			AssertEquals("Should Cache Quoted Booking", quotedBooking, viewQuotedBooking.QuotedBooking);
		}

		public void TestQuotedBooking_BookingOnly()
		{
			ViewQuotedBooking viewQuotedBooking = Factory.New<ViewQuotedBooking>();
			AssertNull("Quoted Booking should return null as VB_JS & VB_TH are empty", viewQuotedBooking.QuotedBooking);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			viewQuotedBooking.VB_JS = booking.PK;
			QuotedBooking quotedBooking = viewQuotedBooking.QuotedBooking;
			AssertNotNull("Should not be null", quotedBooking);
			AssertEquals("Should Cache Quoted Booking", quotedBooking, viewQuotedBooking.QuotedBooking);
		}

		public void TestQuotedBookingCodePropertyAttribute_Quote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var job = new JobHeader.Loader(quotedBooking).TryLoadOrCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();
			ReleaseFactory();
			var viewQuotedBooking = Factory.Load<ViewQuotedBooking>(quotedBooking.PK);
			var bizObjCode = CodePropertyAttribute.CodeFromBusinessObject(viewQuotedBooking);
			AssertEquals("VB_QuoteNumber should equal CodePropertyAttribute from BusinessObject", viewQuotedBooking.VB_QuoteNumber, bizObjCode);
		}

		public void TestQuotedBookingCodePropertyAttribute_Booking()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			Factory.Save();
			ReleaseFactory();
			var viewQuotedBooking = Factory.Load<ViewQuotedBooking>(quotedBooking.PK);
			var bizObjCode = CodePropertyAttribute.CodeFromBusinessObject(viewQuotedBooking);
			AssertEquals("Booking.JS_UniqueConsignRef should equal CodePropertyAttribute from BusinessObject", viewQuotedBooking.QuotedBooking.Booking.JS_UniqueConsignRef, bizObjCode);
		}

		public void TestQuotedBookingCodePropertyAttribute_BookingWithQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			JobHeader job = new JobHeader.Loader(quotedBooking).TryLoadOrCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();
			ReleaseFactory();
			var viewQuotedBooking = Factory.Load<ViewQuotedBooking>(quotedBooking.PK);
			var bizObjCode = CodePropertyAttribute.CodeFromBusinessObject(viewQuotedBooking);
			AssertNotEquals("BusinessObject Code is not empty", ZString.Empty, bizObjCode);
			AssertEquals("VB_QuoteNumber should equal CodePropertyAttribute from BusinessObject", viewQuotedBooking.VB_QuoteNumber, bizObjCode);
		}

		public void TestLogs()
		{
			var quotedBooking = Factory.New<ViewQuotedBooking>();
			var logsType = quotedBooking.Logs.GetType();
			AssertEquals("ViewQuotedBooking Logs should be a ViewQuotedBookingLogs instance", typeof(ViewQuotedBookingLogs), logsType);
		}

		public void TestLogsExcludeShipmentLogs()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.Logs.AddNew(Events.Arrival);
			QuotedBooking.New(ZGuid.Empty, shipment.PK, Factory);
			Factory.Save();
			var quickBooking = Factory.Load<ViewQuotedBooking>(shipment.PK);
			ErrorReporter.Clear();
			var logs = quickBooking.Logs.GetAllLogs();
			AssertEquals("Quick booking should not have arrival log from shipment", 0, logs.Count);
			Assert("No Exception should have been thrown", string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
			ErrorReporter.Clear();
		}

		public void TestIEDocsPluginHostDecider()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var viewQuotedBooking = ViewQuotedBooking.LoadOrCreate(quotedBooking);
			var iEDocsPluginHostDecider = viewQuotedBooking as IEDocsPluginHostDecider;
			AssertNotNull(iEDocsPluginHostDecider);
			AssertEquals("HostBusinessEntity for ViewQuotedBooking should be same with QuotedBooking.", (quotedBooking as IEDocsPluginHostDecider).HostBusinessEntity, iEDocsPluginHostDecider.HostBusinessEntity);
		}

		public void TestDocManagerInfo()
		{
			var org = Factory.New<OrgHeader>();
			var clientDocManagerInfo = ((IDocManagerSupport)org).DocManagerInfo;
			clientDocManagerInfo.AddFileOrDocument(new byte[] { 1, 1, 1, 1, 1 }, "Test.pdf", "MSC");

			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quotedBooking.ClientPK = org.PK;

			var quotedBookingDocManager = ((IDocManagerSupport)quotedBooking).DocManagerInfo;
			var quotedBookingRelatedEdocs = quotedBookingDocManager.GetRelatedEDocs();
			Assert(quotedBookingDocManager is DocManagerInfo);
			AssertEquals(false, quotedBookingDocManager is RatingDocManagerInfo);

			var ooq = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			ooq.ClientPK = org.PK;

			var ooqDocManager = ((IDocManagerSupport)ooq).DocManagerInfo;
			var ooqRelatedEdocs = ooqDocManager.GetRelatedEDocs();
			Assert(ooqDocManager is RatingDocManagerInfo);
			AssertEquals(1, ooqRelatedEdocs.Count());

			AssertEquals("Test.pdf", ooqRelatedEdocs.First().FileName);
		}

		#endregion

		#region IViewComplianceRiskStatusProvider

		public void TestGetProviderBusinessObject_EmptyQuotedBooking()
		{
			var provider = Factory.New<ViewQuotedBooking>() as IViewComplianceRiskStatusProvider;
			AssertNull(provider.GetProviderBusinessObject());
		}

		public void TestGetProviderBusinessObject_Quote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			Factory.Save();
			var provider = Factory.Load<ViewQuotedBooking>(quotedBooking.PK) as IViewComplianceRiskStatusProvider;
			AssertNull(provider.GetProviderBusinessObject());
		}

		public void TestGetProviderBusinessObject_Booking()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			Factory.Save();
			var provider = Factory.Load<ViewQuotedBooking>(quotedBooking.PK) as IViewComplianceRiskStatusProvider;
			AssertEquals(quotedBooking.PK, provider.GetProviderBusinessObject().ParentID);
		}

		public void TestGetProviderBusinessObject_BookingWithQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			Factory.Save();
			var provider = Factory.Load<ViewQuotedBooking>(quotedBooking.PK) as IViewComplianceRiskStatusProvider;
			AssertEquals(quotedBooking.PK, provider.GetProviderBusinessObject().ParentID);
		}

		public void TestGetProviderBusinessObject_Template()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			Factory.Save();

			var templateRecord = Factory.New<StmTemplateRecord>();
			var templateRecordProvider = quotedBooking as ITemplateRecordProvider;
			templateRecordProvider.LoadFromTemplateRecord(templateRecord);

			var provider = Factory.Load<ViewQuotedBooking>(quotedBooking.PK) as IViewComplianceRiskStatusProvider;
			AssertNull(provider.GetProviderBusinessObject());
		}

		#endregion IViewComplianceRiskStatusProvider

		public void TestICancel()
		{
			Quote quoteOnlyQuote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			Quote quoteBookingQuote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment quoteBookingBooking = QuotedBooking.CreateNewBooking(Factory);
			ForwardingShipment bookingOnlyBooking = QuotedBooking.CreateNewBooking(Factory);
			ViewQuotedBooking quoteOnly = Factory.New<ViewQuotedBooking>();
			quoteOnly.VB_TH = quoteOnlyQuote.PK;
			ViewQuotedBooking bookingOnly = Factory.New<ViewQuotedBooking>();
			bookingOnly.VB_JS = bookingOnlyBooking.PK;
			ViewQuotedBooking quoteBooking = Factory.New<ViewQuotedBooking>();
			quoteBooking.VB_TH = quoteBookingQuote.PK;
			quoteBooking.VB_JS = quoteBookingBooking.PK;
			ICancellable iQuoteOnly = quoteOnly;
			ICancellable iBookingOnly = bookingOnly;
			ICancellable iQuoteBooking = quoteBooking;
			AssertEquals("can cancel", JobHeaderParentDeletionHelper.CheckIfCanCancelJobHeaderParent(quoteOnlyQuote.PK, quoteOnlyQuote.HumanReadableName), iQuoteOnly.CanCancel());
			AssertEquals("can cancel", bookingOnlyBooking.CanCancel(), iBookingOnly.CanCancel());
			AssertEquals("can cancel", quoteBookingBooking.CanCancel() ?? "", iQuoteBooking.CanCancel() ?? "");
			AssertEquals("can reactivate", null, iQuoteOnly.CanReactivate());
			AssertEquals("can reactivate", bookingOnlyBooking.CanReactivate(), iBookingOnly.CanReactivate());
			AssertEquals("can reactivate", quoteBookingBooking.CanReactivate(), iQuoteBooking.CanReactivate());
			AssertEquals("is cancelled", quoteOnlyQuote.TH_IsCancelled, iQuoteOnly.IsCancelled);
			AssertEquals("is cancelled", ((ICancellable)bookingOnlyBooking).IsCancelled, iBookingOnly.IsCancelled);
			AssertEquals("is cancelled", ((ICancellable)quoteBookingBooking).IsCancelled && quoteBookingQuote.TH_IsCancelled, iQuoteBooking.IsCancelled);
			AssertEquals("IsCancelledHasChanged", quoteOnlyQuote.TH_IsCancelledInfo.HasChanges, iQuoteOnly.IsCancelledHasChanged);
			AssertEquals("IsCancelledHasChanged", ((ICancellable)bookingOnlyBooking).IsCancelledHasChanged, iBookingOnly.IsCancelledHasChanged);
			AssertEquals("IsCancelledHasChanged", ((ICancellable)quoteBookingBooking).IsCancelledHasChanged || quoteBookingQuote.TH_IsCancelledInfo.HasChanges, iQuoteBooking.IsCancelledHasChanged);
			iQuoteOnly.IsCancelled = true;
			iBookingOnly.IsCancelled = true;
			iQuoteBooking.IsCancelled = true;
			AssertEquals("is cancelled", quoteOnlyQuote.TH_IsCancelled, iQuoteOnly.IsCancelled);
			AssertEquals("is cancelled", ((ICancellable)bookingOnlyBooking).IsCancelled, iBookingOnly.IsCancelled);
			AssertEquals("is cancelled", ((ICancellable)quoteBookingBooking).IsCancelled && quoteBookingQuote.TH_IsCancelled, iQuoteBooking.IsCancelled);
			AssertEquals("IsCancelledHasChanged", quoteOnlyQuote.TH_IsCancelledInfo.HasChanges, iQuoteOnly.IsCancelledHasChanged);
			AssertEquals("IsCancelledHasChanged", ((ICancellable)bookingOnlyBooking).IsCancelledHasChanged, iBookingOnly.IsCancelledHasChanged);
			AssertEquals("IsCancelledHasChanged", ((ICancellable)quoteBookingBooking).IsCancelledHasChanged || quoteBookingQuote.TH_IsCancelledInfo.HasChanges, iQuoteBooking.IsCancelledHasChanged);
		}

		public void TestHumanReadableName()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			ViewQuotedBooking view = Factory.New<ViewQuotedBooking>();
			view.VB_TH = quote.PK;
			view.VB_JS = booking.PK;
			QuotedBooking quotedBooking = QuotedBooking.New(view, Factory);
			AssertEquals(view.HumanReadableName, quotedBooking.HumanReadableName);
		}

		public void PreventDeleteAttribute()
		{
			Assert(CargoWise.EntityFramework.PreventDeleteAttribute.IsTrue(typeof(ViewQuotedBooking)));
		}

		public void TestUniversalCopyInstanceType()
		{
			var instanceTypeAttribute = typeof(ViewQuotedBooking).GetCustomAttribute<UniversalCopyInstanceTypeAttribute>();
			AssertEquals(typeof(QuotedBooking), instanceTypeAttribute.InstanceType);
		}

		public void TestValidateUniversalCopyPreconditions_TemplateRequiresBooking_NoBookingExists()
		{
			var ucFactory = new UniversalCopyFactory(typeof(QuotedBooking), null);
			var template = ucFactory.GetNewCopyTemplate(null);
			template.PrepareForSave();
			template.Factory.Save();
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var viewQuotedBooking = Factory.New<ViewQuotedBooking>();
			viewQuotedBooking.VB_TH = quote.PK;
			viewQuotedBooking.VB_JS = ZGuid.Empty;
			var quotedBooking = QuotedBooking.New(viewQuotedBooking, Factory);
			var innerNode = template.CopyTemplateTree.CopyTemplateNode.InnerNode as EntityCopyTemplateNode;
			((RelatedEntityCopyTemplateNode)(innerNode.Nodes.Find(node => node.Name == "Booking"))).CopyMethod = RelatedEntityCopyMethod.Copy;
			Factory.Save();
			var validationErrorMessages = viewQuotedBooking.ValidateUniversalCopyPreconditions(template.CopyTemplateTree.CopyTemplateNode);
			var expectedErrorMessage = "This copy template copies only Booking but the One Off Quote is missing a Booking, and thus cannot create a valid copy.";
			AssertEquals("Validation error message thrown: Required booking is missing", expectedErrorMessage, validationErrorMessages);
		}

		public void TestLoadByTablePrefixAndPK_QuickBooking()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var viewQuotedBooking = ViewQuotedBooking.LoadOrCreate(quotedBooking);

			AssertEquals("VB_JS", viewQuotedBooking.VB_JS, quotedBooking.Booking.PK);
			AssertEquals("VB_TH", viewQuotedBooking.VB_TH, ZGuid.Empty);
			AssertEquals("VB_GC", viewQuotedBooking.VB_GC, ZGuid.Empty);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();

			var loaded = otherFactory.Load(ViewQuotedBookingSchema.Constants.Prefix, viewQuotedBooking.PK);

			if (loaded is QuotedBooking quotedBookingLoaded)
			{
				AssertEquals("ViewQuotedBookingSchema was hit once to load ViewQuotedBooking", 1, otherFactory.GetTableHitCount(ViewQuotedBookingSchema.Constants.TableName));
				AssertEquals("RateOneOffShipment wasn't hit to load Quote", 0, otherFactory.GetTableHitCount(RateOneOffShipmentSchema.Constants.TableName));
				AssertEquals("JobShipment was hit once to load Booking", 1, otherFactory.GetTableHitCount(JobShipmentSchema.Constants.TableName));

				AssertNotNull("Booking", quotedBookingLoaded.Booking);
				AssertNull("Quote", quotedBookingLoaded.Quote);
			}
			else
			{
				Fail("QuotedBooking was loaded by PK");
			}
		}

		public void TestLoadByQuotedBookingPK_QuickBooking()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var viewQuotedBooking = ViewQuotedBooking.LoadOrCreate(quotedBooking);

			AssertEquals("VB_JS", viewQuotedBooking.VB_JS, quotedBooking.Booking.PK);
			AssertEquals("VB_TH", viewQuotedBooking.VB_TH, ZGuid.Empty);
			AssertEquals("VB_GC", viewQuotedBooking.VB_GC, ZGuid.Empty);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();

			var quotedBookingLoaded = otherFactory.Load<QuotedBooking>(viewQuotedBooking.PK);

			AssertNotNull("QuotedBooking was loaded by PK", quotedBookingLoaded);
			AssertNotNull("Booking", quotedBookingLoaded.Booking);
			AssertNull("Quote", quotedBookingLoaded.Quote);
		}

		public void TestLoadQuotedBookingTwice()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var viewQuotedBooking = ViewQuotedBooking.LoadOrCreate(quotedBooking);

			AssertEquals("VB_JS", viewQuotedBooking.VB_JS, quotedBooking.Booking.PK);
			AssertEquals("VB_TH", viewQuotedBooking.VB_TH, ZGuid.Empty);
			AssertEquals("VB_GC", viewQuotedBooking.VB_GC, ZGuid.Empty);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();

			var quotedBookingLoaded1 = otherFactory.Load<QuotedBooking>(viewQuotedBooking.PK);
			var quotedBookingLoaded2 = otherFactory.Load<QuotedBooking>(viewQuotedBooking.PK);

			AssertEquals("Same QuotedBooking was returned when loaded by PK",
				quotedBookingLoaded1, quotedBookingLoaded2);
		}

		public void TestLoadByTablePrefixAndPK_BookingWithQuote()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			var viewQuotedBooking = ViewQuotedBooking.LoadOrCreate(quotedBooking);

			AssertEquals("VB_JS", viewQuotedBooking.VB_JS, quotedBooking.Booking.PK);
			AssertEquals("VB_TH", viewQuotedBooking.VB_TH, quotedBooking.Quote.PK);
			AssertEquals("VB_GC", viewQuotedBooking.VB_GC, quotedBooking.Quote.TH_GC);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();

			var loaded = otherFactory.Load(ViewQuotedBookingSchema.Constants.Prefix, viewQuotedBooking.PK);

			if (loaded is QuotedBooking quotedBookingLoaded)
			{
				AssertEquals("ViewQuotedBookingSchema was hit once to load ViewQuotedBooking", 1, otherFactory.GetTableHitCount(ViewQuotedBookingSchema.Constants.TableName));
				AssertEquals("RateOneOffShipment was hit once to load Quote", 1, otherFactory.GetTableHitCount(RateOneOffShipmentSchema.Constants.TableName));
				AssertEquals("JobShipment was hit twice to 1. load Booking and 2. from fetch hint to load JobShipment by JS_TH_OneTimeQuote", 2, otherFactory.GetTableHitCount(JobShipmentSchema.Constants.TableName));

				AssertNotNull("Booking", quotedBookingLoaded.Booking);
				AssertNotNull("Quote", quotedBookingLoaded.Quote);
			}
			else
			{
				Fail("QuotedBooking was loaded by PK");
			}
		}

		public void TestLoadByQuotedBookingPK_BookingWithQuote()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			var viewQuotedBooking = ViewQuotedBooking.LoadOrCreate(quotedBooking);

			AssertEquals("VB_JS", viewQuotedBooking.VB_JS, quotedBooking.Booking.PK);
			AssertEquals("VB_TH", viewQuotedBooking.VB_TH, quotedBooking.Quote.PK);
			AssertEquals("VB_GC", viewQuotedBooking.VB_GC, quotedBooking.Quote.TH_GC);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();

			var quotedBookingLoaded = otherFactory.Load<QuotedBooking>(viewQuotedBooking.PK);

			AssertNotNull("QuotedBooking was loaded by PK", quotedBookingLoaded);
			AssertNotNull("Booking", quotedBookingLoaded.Booking);
			AssertNotNull("Quote", quotedBookingLoaded.Quote);
		}

		public void TestLoadBookingWithQuoteTwice()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			var viewQuotedBooking = ViewQuotedBooking.LoadOrCreate(quotedBooking);

			AssertEquals("VB_JS", viewQuotedBooking.VB_JS, quotedBooking.Booking.PK);
			AssertEquals("VB_TH", viewQuotedBooking.VB_TH, quotedBooking.Quote.PK);
			AssertEquals("VB_GC", viewQuotedBooking.VB_GC, quotedBooking.Quote.TH_GC);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();

			var quotedBookingLoaded1 = otherFactory.Load<QuotedBooking>(viewQuotedBooking.PK);
			var quotedBookingLoaded2 = otherFactory.Load<QuotedBooking>(viewQuotedBooking.PK);

			AssertEquals("Same QuotedBooking was returned when loaded by PK",
				quotedBookingLoaded1, quotedBookingLoaded2);
		}

		#region IImportParentRelatedActivityInfoOnNew
		public void TestImportParentRelatedActivityInfoOnNew()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTAA";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_OH_ConvertedToQualifiedLead = org.PK;
			inquiry.O1_OC_LinkedContact = contact.PK;
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var view = Factory.New<ViewQuotedBooking>();
			view.VB_TH = quote.PK;
			((IImportParentRelatedActivityInfoOnNew)view).ImportParentInfo(inquiry, new ImportRelatedActivityNoDecisionFactory());
			AssertEquals(org.PK, view.QuotedBooking.ClientPK);
		}

		#endregion

		#region FieldChangeEvents

		public void TestQuotedBooking_QuoteOnly_FieldChangeEventsOnParentAndChild()
		{
			ObjectFactory.Get<IProcessFieldChangeRuleTestHelper>().TurnOffBlacklistForTest();

			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("JS", new List<IBusinessObjectParentLocator>() { new ShipmentToQuotedBookingParentLocator() });
				businessObjectParentLocators.Add("TH", new List<IBusinessObjectParentLocator>() { new QuoteToQuotedBookingParentLocator() });
			});

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("JS", new ShipmentFieldStateChange());
				fieldChangeState.Add("TH", new QuoteFieldStateChange());
			});

			var quotedBookingRule = Factory.New<IProcessFieldChangeRule>();
			quotedBookingRule.PFR_ProcessType = "QBK";
			quotedBookingRule.PFR_GroupName = "Group1";
			quotedBookingRule.PFR_SE_NKEvent = "Z05";
			quotedBookingRule.Fields.AddNew().PFL_FieldName = "TH_GlobalRateDescription";

			var quoteRule = Factory.New<IProcessFieldChangeRule>();
			quoteRule.PFR_ProcessType = "QTN";
			quoteRule.PFR_GroupName = "Group2";
			quoteRule.PFR_SE_NKEvent = "Z02";
			quoteRule.Fields.AddNew().PFL_FieldName = "TH_GlobalRateDescription";
			Factory.Save();

			var factory = new BusinessObjectFactory();

			ViewQuotedBooking viewQuotedBooking = factory.New<ViewQuotedBooking>();
			AssertNull("Quoted Booking should return null as VB_JS & VB_TH are empty", viewQuotedBooking.QuotedBooking);

			Quote quote = QuotedBooking.CreateNewQuote(factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			viewQuotedBooking.VB_TH = quote.PK;

			QuotedBooking quotedBooking = viewQuotedBooking.QuotedBooking;
			quote.TH_GlobalRateDescription = "Rate";

			AssertEquals(1, quotedBooking.Logs.GetAllLogs().Count);
			AssertEquals(1, quote.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*TH_GlobalRateDescription.*Rate"), quotedBooking.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertMatch(new Regex($".*TH_GlobalRateDescription.*Rate"), quote.Logs.GetAllLogs()[0].Parameters["CHG"]);
		}

		public void TestQuotedBooking_BookingOnly_FieldChangeEventsOnQuotedBooking()
		{
			ObjectFactory.Get<IProcessFieldChangeRuleTestHelper>().TurnOffBlacklistForTest();

			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("JS", new List<IBusinessObjectParentLocator>() { new ShipmentToQuotedBookingParentLocator() });
			});

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("JS", new ShipmentFieldStateChange());
			});

			var quotedBookingRule = Factory.New<IProcessFieldChangeRule>();
			quotedBookingRule.PFR_ProcessType = "QBK";
			quotedBookingRule.PFR_GroupName = "Group2";
			quotedBookingRule.PFR_SE_NKEvent = "Z05";
			quotedBookingRule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";
			Factory.Save();

			var factory = new BusinessObjectFactory();

			ViewQuotedBooking viewQuotedBooking = factory.New<ViewQuotedBooking>();
			AssertNull("Quoted Booking should return null as VB_JS & VB_TH are empty", viewQuotedBooking.QuotedBooking);

			ForwardingShipment booking = QuotedBooking.CreateNewBooking(factory);
			viewQuotedBooking.VB_JS = booking.PK;

			QuotedBooking quotedBooking = viewQuotedBooking.QuotedBooking;
			booking.JS_GoodsDescription = "Odds";

			AssertEquals(1, quotedBooking.Logs.GetAllLogs().Count);
			AssertEquals(0, booking.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*JS_GoodsDescription.*Odds"), quotedBooking.Logs.GetAllLogs()[0].Parameters["CHG"]);
		}

		public void TestQuotedBooking_QuoteAndBooking_FieldChangeEventsOnParentAndChild()
		{
			ObjectFactory.Get<IProcessFieldChangeRuleTestHelper>().TurnOffBlacklistForTest();

			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("JS", new List<IBusinessObjectParentLocator>() { new ShipmentToQuotedBookingParentLocator() });
				businessObjectParentLocators.Add("TH", new List<IBusinessObjectParentLocator>() { new QuoteToQuotedBookingParentLocator() });
			});

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("JS", new ShipmentFieldStateChange());
				fieldChangeState.Add("TH", new QuoteFieldStateChange());
			});

			var quotedBookingRule = Factory.New<IProcessFieldChangeRule>();
			quotedBookingRule.PFR_ProcessType = "QBK";
			quotedBookingRule.PFR_GroupName = "Group1";
			quotedBookingRule.PFR_SE_NKEvent = "Z01";
			quotedBookingRule.Fields.AddNew().PFL_FieldName = "TH_GlobalRateDescription";
			quotedBookingRule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";

			var quoteRule = Factory.New<IProcessFieldChangeRule>();
			quoteRule.PFR_ProcessType = "QTN";
			quoteRule.PFR_GroupName = "Group2";
			quoteRule.PFR_SE_NKEvent = "Z02";
			quoteRule.Fields.AddNew().PFL_FieldName = "TH_GlobalRateDescription";

			var shipmentRule = Factory.New<IProcessFieldChangeRule>();
			shipmentRule.PFR_ProcessType = "SHP";
			shipmentRule.PFR_GroupName = "Group3";
			shipmentRule.PFR_SE_NKEvent = "Z03";
			shipmentRule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";
			Factory.Save();

			var factory = new BusinessObjectFactory();

			ViewQuotedBooking viewQuotedBooking = factory.New<ViewQuotedBooking>();
			AssertNull("Quoted Booking should return null as VB_JS & VB_TH are empty", viewQuotedBooking.QuotedBooking);

			Quote quote = QuotedBooking.CreateNewQuote(factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			viewQuotedBooking.VB_TH = quote.PK;
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory).Booking;
			viewQuotedBooking.VB_JS = booking.PK;
			QuotedBooking quotedBooking = viewQuotedBooking.QuotedBooking;
			quote.TH_GlobalRateDescription = "Rate";
			booking.JS_GoodsDescription = "Odds";

			AssertEquals(1, quotedBooking.Logs.GetAllLogs().Count);
			AssertEquals(1, quote.Logs.GetAllLogs().Count);
			AssertEquals(0, booking.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*TH_GlobalRateDescription.*Rate"), quote.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertMatch(new Regex($".*TH_GlobalRateDescription.*Rate.*JS_GoodsDescription.*Odds"), quotedBooking.Logs.GetAllLogs()[0].Parameters["CHG"]);
		}

		public void TestQuotedBooking_QuoteAndBooking_FieldChangeEventsForAllExposedProperties()
		{
			WorkflowDescriptors.Instance.TryGetValue("QBK", out var workflowDescriptor);
			var configManager = ObjectFactory.New<IProcessFieldChangeRuleConfigurationManager>();
			var rule = Factory.New<IProcessFieldChangeRule>();
			rule.PFR_ProcessType = workflowDescriptor.Code;
			rule.PFR_GroupName = $"Group1";
			rule.PFR_SE_NKEvent = "Z00";
			foreach (var field in configManager.GetFields(workflowDescriptor.Code))
			{
				rule.Fields.AddNew().PFL_FieldName = ((ICodeDescription)field).Code;
			}

			Factory.Save();

			CombineAssertions(() =>
			{
				foreach (var field in configManager.GetFields(workflowDescriptor.Code))
				{
					var factory = new BusinessObjectFactory();
					ViewQuotedBooking viewQuotedBooking = factory.New<ViewQuotedBooking>();
					AssertNull("Quoted Booking should return null as VB_JS & VB_TH are empty", viewQuotedBooking.QuotedBooking);

					Quote quote = QuotedBooking.CreateNewQuote(factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
					viewQuotedBooking.VB_TH = quote.PK;
					var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory).Booking;
					booking.GetReasonForChangingDeliveryDueDateEventHandler += (sender, arg) => arg.Reason = "mock reason";

					viewQuotedBooking.VB_JS = booking.PK;
					QuotedBooking quotedBooking = viewQuotedBooking.QuotedBooking;

					object val;
					BusinessObject bizoToSet;

					try
					{
						val = booking.GetPropertyValue(((ICodeDescription)field).Code);
						bizoToSet = booking;
					}
					catch (Exception)
					{
						try
						{
							val = quote.GetPropertyValue(((ICodeDescription)field).Code);
							bizoToSet = quote;
						}
						catch (Exception ex)
						{
							Fail($"Exception {ex.InnerException?.Message ?? ex.Message} occured getting property value {((ICodeDescription)field).Code} for type {quotedBooking.GetType().FullName}, workflow {workflowDescriptor.Code}");
							continue;
						}
					}

					var newVal = val;

					if (typeof(ZString).IsAssignableFrom(val.GetType()))
					{
						newVal = new ZString("~");
					}
					else if (typeof(ZShort).IsAssignableFrom(val.GetType()))
					{
						newVal = new ZShort(42);
					}
					else if (typeof(ZInt).IsAssignableFrom(val.GetType()))
					{
						newVal = new ZInt(42);
					}
					else if (typeof(ZDecimal).IsAssignableFrom(val.GetType()))
					{
						newVal = new ZDecimal(42);
					}
					else if (typeof(ZDate).IsAssignableFrom(val.GetType()))
					{
						if (val is ZDate date && date.IsValid)
						{
							newVal = date.AddDays(-1);
						}
						else
						{
							newVal = ZDate.Today;
						}
					}
					else if (typeof(ZDateTime).IsAssignableFrom(val.GetType()))
					{
						if (val is ZDateTime dateTime && dateTime.IsValid)
						{
							newVal = dateTime.AddDays(-1);
						}
						else
						{
							newVal = ZDateTime.Now;
						}
					}
					else if (typeof(ZDateTimeOffset).IsAssignableFrom(val.GetType()))
					{
						if (val is ZDateTimeOffset dateTime && dateTime.IsValid)
						{
							newVal = dateTime.AddDays(-1);
						}
						else
						{
							newVal = ZDateTimeOffset.Now;
						}
					}
					else if (typeof(ZGuid).IsAssignableFrom(val.GetType()))
					{
						newVal = ZGuid.NewZGuid();
					}
					else if (typeof(ZBool).IsAssignableFrom(val.GetType()))
					{
						newVal = new ZBool(!(ZBool)val);
					}
					else if (typeof(ZByte).IsAssignableFrom(val.GetType()))
					{
						newVal = new ZByte(0x42);
					}
					else if (typeof(ZBlob).IsAssignableFrom(val.GetType()))
					{
						newVal = new ZBlob(new byte[] { 1, 4, 9, 16, 25, 10, 13, 34, 43, 89, 10, 13, 25, 87, 34, 87, 9 });
					}
					else
					{
						Fail($"Unknown property type {val.GetType().FullName} for {((ICodeDescription)field).Code} for type {bizoToSet.GetType().FullName}, workflow {workflowDescriptor.Code}");
						continue;
					}

					if (((IStmALogProvider)quotedBooking).Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomisableEvent00.Code)).Where(log => !log.IsInDatabase).ToList().Count > 0)
					{
						Fail($"Event exists without setting anything {((ICodeDescription)field).Code} for type {quotedBooking.GetType().FullName}, workflow {workflowDescriptor.Code}");
						continue;
					}

					try
					{
						bizoToSet.SetPropertyValue(((ICodeDescription)field).Code, newVal);
					}
					catch (Exception ex)
					{
						Fail($"Exception {ex.InnerException?.Message ?? ex.Message} occured setting property value {((ICodeDescription)field).Code} for type {bizoToSet.GetType().FullName}, workflow {workflowDescriptor.Code}");
						continue;
					}

					if (((IStmALogProvider)quotedBooking).Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomisableEvent00.Code)).Where(log => !log.IsInDatabase).ToList().Count != 1)
					{
						Fail($"No event for {((ICodeDescription)field).Code} for type {quotedBooking.GetType().FullName}, workflow {workflowDescriptor.Code}");
						continue;
					}

					try
					{
						bizoToSet.SetPropertyValue(((ICodeDescription)field).Code, val);
						if (bizoToSet.GetPropertyValue(((ICodeDescription)field).Code).Equals(val))
						{
							if (((IStmALogProvider)quotedBooking).Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomisableEvent00.Code)).Where(log => !log.IsInDatabase).ToList().Count != 0)
							{
								AssertNotContains($"Event not cleared for {((ICodeDescription)field).Code} for type {bizoToSet.GetType().FullName}, workflow {workflowDescriptor.Code}", $"{((ICodeDescription)field).Code} ", ((IStmALogProvider)quotedBooking).Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomisableEvent00.Code)).Where(log => !log.IsInDatabase).ToList()[0].SL_Reference);
								continue;
							}
						}
						else
						{
							continue;
						}
					}
					catch (Exception ex)
					{
						Fail($"Exception {ex.InnerException?.Message ?? ex.Message} occured setting property value back to its original value {((ICodeDescription)field).Code} for type {quotedBooking.GetType().FullName}, workflow {workflowDescriptor.Code}");
						continue;
					}
				}
			});
		}

		public void TestQuotedBooking_BookingOnly_FieldChangeEventsOnQuotedBooking_DifferentFactoryDataRefreshBuss()
		{
			ObjectFactory.Get<IProcessFieldChangeRuleTestHelper>().TurnOffBlacklistForTest();

			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("JS", new List<IBusinessObjectParentLocator>() { new ShipmentToQuotedBookingParentLocator() });
			});

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("JS", new ShipmentFieldStateChange());
			});

			var quotedBookingRule = Factory.New<IProcessFieldChangeRule>();
			quotedBookingRule.PFR_ProcessType = "QBK";
			quotedBookingRule.PFR_GroupName = "Group2";
			quotedBookingRule.PFR_SE_NKEvent = "Z05";
			quotedBookingRule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";
			Factory.Save();

			var factory = new BusinessObjectFactory();

			ViewQuotedBooking viewQuotedBooking = factory.New<ViewQuotedBooking>();
			AssertNull("Quoted Booking should return null as VB_JS & VB_TH are empty", viewQuotedBooking.QuotedBooking);

			ForwardingShipment booking = QuotedBooking.CreateNewBooking(factory);
			viewQuotedBooking.VB_JS = booking.PK;

			QuotedBooking quotedBooking = viewQuotedBooking.QuotedBooking;
			factory.Save();
			var otherFactory = new BusinessObjectFactory();
			var bookingInOtherFactory = otherFactory.Load<ForwardingShipment>(booking.PK);
			bookingInOtherFactory.JS_GoodsDescription = "Odds";
			otherFactory.Save();

			AssertEquals(1, quotedBooking.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*JS_GoodsDescription.*Odds"), quotedBooking.Logs.GetAllLogs()[0].Parameters["CHG"]);
		}

		public void TestQuotedBooking_BookingOnly_NoFieldChangeEventsOnShipment()
		{
			ObjectFactory.Get<IProcessFieldChangeRuleTestHelper>().TurnOffBlacklistForTest();

			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("JS", new List<IBusinessObjectParentLocator>() { new ShipmentToQuotedBookingParentLocator() });
			});

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("JS", new ShipmentFieldStateChange());
			});

			var shipmentRule = Factory.New<IProcessFieldChangeRule>();
			shipmentRule.PFR_ProcessType = "SHP";
			shipmentRule.PFR_GroupName = "Group2";
			shipmentRule.PFR_SE_NKEvent = "Z05";
			shipmentRule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";
			Factory.Save();

			var factory = new BusinessObjectFactory();

			ViewQuotedBooking viewQuotedBooking = factory.New<ViewQuotedBooking>();
			AssertNull("Quoted Booking should return null as VB_JS & VB_TH are empty", viewQuotedBooking.QuotedBooking);

			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory).Booking;
			viewQuotedBooking.VB_JS = booking.PK;

			QuotedBooking quotedBooking = viewQuotedBooking.QuotedBooking;
			booking.JS_GoodsDescription = "Odds";

			AssertEquals(0, quotedBooking.Logs.GetAllLogs().Count);
			AssertEquals(0, booking.Logs.GetAllLogs().Count);
		}

		public void TestQuotedBooking_BookingOnly_FieldChangeEventsOnShipmentOnceQuotedBookingConverted()
		{
			ObjectFactory.Get<IProcessFieldChangeRuleTestHelper>().TurnOffBlacklistForTest();

			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("JS", new List<IBusinessObjectParentLocator>() { new ShipmentToQuotedBookingParentLocator() });
			});

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("JS", new ShipmentFieldStateChange());
			});

			var quotedBookingRule = Factory.New<IProcessFieldChangeRule>();
			quotedBookingRule.PFR_ProcessType = "QBK";
			quotedBookingRule.PFR_GroupName = "Group1";
			quotedBookingRule.PFR_SE_NKEvent = "Z05";
			quotedBookingRule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";

			var shipmentRule = Factory.New<IProcessFieldChangeRule>();
			shipmentRule.PFR_ProcessType = "SHP";
			shipmentRule.PFR_GroupName = "Group2";
			shipmentRule.PFR_SE_NKEvent = "Z05";
			shipmentRule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";
			Factory.Save();

			var factory = new BusinessObjectFactory();

			ViewQuotedBooking viewQuotedBooking = factory.New<ViewQuotedBooking>();
			AssertNull("Quoted Booking should return null as VB_JS & VB_TH are empty", viewQuotedBooking.QuotedBooking);

			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory).Booking;
			viewQuotedBooking.VB_JS = booking.PK;

			QuotedBooking quotedBooking = viewQuotedBooking.QuotedBooking;
			booking.JS_GoodsDescription = "Odds";

			AssertEquals(1, quotedBooking.Logs.GetAllLogs().Count);
			AssertEquals(0, booking.Logs.GetAllLogs().Count);

			new BuildConsolHelper().TurnBookingIntoShipment(quotedBooking.Booking, null, quotedBooking.PK);
			booking.JS_GoodsDescription = "Sods";
			AssertMatch(new Regex($".*JS_GoodsDescription.*Sods"), booking.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertMatch(new Regex($".*JS_GoodsDescription.*Sods"), quotedBooking.Logs.GetAllLogs()[0].Parameters["CHG"]);
		}

		public void TestQuotedBooking_QuoteOnly_FieldChangeEventsOnShipmentOnceQuoteConverted()
		{
			ObjectFactory.Get<IProcessFieldChangeRuleTestHelper>().TurnOffBlacklistForTest();

			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("JS", new List<IBusinessObjectParentLocator>() { new ShipmentToQuotedBookingParentLocator() });
				businessObjectParentLocators.Add("TH", new List<IBusinessObjectParentLocator>() { new QuoteToQuotedBookingParentLocator() });
			});

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("JS", new ShipmentFieldStateChange());
				fieldChangeState.Add("TH", new QuoteFieldStateChange());
			});

			var quotedBookingRule = Factory.New<IProcessFieldChangeRule>();
			quotedBookingRule.PFR_ProcessType = "QBK";
			quotedBookingRule.PFR_GroupName = "Group1";
			quotedBookingRule.PFR_SE_NKEvent = "Z05";
			quotedBookingRule.PFR_Reference = "1";
			quotedBookingRule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";

			var quotedBookingRule2 = Factory.New<IProcessFieldChangeRule>();
			quotedBookingRule2.PFR_ProcessType = "QBK";
			quotedBookingRule2.PFR_GroupName = "Group2";
			quotedBookingRule2.PFR_SE_NKEvent = "Z05";
			quotedBookingRule2.Fields.AddNew().PFL_FieldName = "TH_GlobalRateDescription";

			var quoteRule = Factory.New<IProcessFieldChangeRule>();
			quoteRule.PFR_ProcessType = "QTN";
			quoteRule.PFR_GroupName = "Group3";
			quoteRule.PFR_SE_NKEvent = "Z02";
			quoteRule.PFR_Reference = "2";
			quoteRule.Fields.AddNew().PFL_FieldName = "TH_GlobalRateDescription";

			var shipmentRule = Factory.New<IProcessFieldChangeRule>();
			shipmentRule.PFR_ProcessType = "SHP";
			shipmentRule.PFR_GroupName = "Group4";
			shipmentRule.PFR_SE_NKEvent = "Z05";
			shipmentRule.PFR_Reference = "3";
			shipmentRule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";
			Factory.Save();

			var factory = new BusinessObjectFactory();

			ViewQuotedBooking viewQuotedBooking = factory.New<ViewQuotedBooking>();
			AssertNull("Quoted Booking should return null as VB_JS & VB_TH are empty", viewQuotedBooking.QuotedBooking);

			var quote = QuotedBooking.New(QuoteBookingType.SpotQuote, factory).Quote;
			viewQuotedBooking.VB_TH = quote.PK;

			QuotedBooking quotedBooking = viewQuotedBooking.QuotedBooking;
			quote.TH_GlobalRateDescription = "Rate";

			AssertEquals(1, quote.Logs.GetAllLogs().Count);
			AssertEquals(1, quotedBooking.Logs.GetAllLogs().Count);

			quotedBooking.ConvertQuoteToQuotedBooking();
			var booking = quotedBooking.Booking;

			booking.JS_GoodsDescription = "Sods";
			AssertMatch(new Regex($".*TH_GlobalRateDescription.*Rate"), quotedBooking.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertMatch(new Regex($".*TH_GlobalRateDescription.*Rate"), quote.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertMatch(new Regex($".*JS_GoodsDescription.*Sods"), quotedBooking.Logs.GetAllLogs()[1].Parameters["CHG"]);
		}

		public void TestQuotedBooking_QuickBooking_FieldChangeEventsOnShipmentOnceShipmentConverted()
		{
			ObjectFactory.Get<IProcessFieldChangeRuleTestHelper>().TurnOffBlacklistForTest();

			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("JS", new List<IBusinessObjectParentLocator>() { new ShipmentToQuotedBookingParentLocator() });
				businessObjectParentLocators.Add("TH", new List<IBusinessObjectParentLocator>() { new QuoteToQuotedBookingParentLocator() });
			});

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("JS", new ShipmentFieldStateChange());
				fieldChangeState.Add("TH", new QuoteFieldStateChange());
			});

			var quotedBookingRule = Factory.New<IProcessFieldChangeRule>();
			quotedBookingRule.PFR_ProcessType = "QBK";
			quotedBookingRule.PFR_GroupName = "Group1";
			quotedBookingRule.PFR_SE_NKEvent = "Z01";
			quotedBookingRule.PFR_Reference = "1";
			quotedBookingRule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";

			var quotedBookingRule2 = Factory.New<IProcessFieldChangeRule>();
			quotedBookingRule2.PFR_ProcessType = "QBK";
			quotedBookingRule2.PFR_GroupName = "Group2";
			quotedBookingRule2.PFR_SE_NKEvent = "Z02";
			quotedBookingRule2.PFR_Reference = "2";
			quotedBookingRule2.Fields.AddNew().PFL_FieldName = "TH_GlobalRateDescription";

			var quoteRule = Factory.New<IProcessFieldChangeRule>();
			quoteRule.PFR_ProcessType = "QTN";
			quoteRule.PFR_GroupName = "Group3";
			quoteRule.PFR_SE_NKEvent = "Z03";
			quoteRule.PFR_Reference = "3";
			quoteRule.Fields.AddNew().PFL_FieldName = "TH_GlobalRateDescription";

			var shipmentRule = Factory.New<IProcessFieldChangeRule>();
			shipmentRule.PFR_ProcessType = "SHP";
			shipmentRule.PFR_GroupName = "Group4";
			shipmentRule.PFR_SE_NKEvent = "Z04";
			shipmentRule.PFR_Reference = "4";
			shipmentRule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";
			Factory.Save();

			var factory = new BusinessObjectFactory();

			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			quotedBooking.Booking.JS_GoodsDescription = "Book";

			new BuildConsolHelper().TurnBookingIntoShipment(quotedBooking.Booking, null, quotedBooking.PK);

			factory.Save();
			AssertEquals(1, quotedBooking.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*JS_GoodsDescription.*Book"), quotedBooking.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertEquals(1, quotedBooking.Booking.Logs.GetAllLogs().Count);
			AssertEquals("ADD", quotedBooking.Booking.Logs.GetAllLogs()[0].SL_SE_NKEvent);

			factory = new BusinessObjectFactory();
			var reloadBooking = factory.Load<ForwardingShipment>(quotedBooking.Booking.PK);
			reloadBooking.JS_GoodsDescription = "Sods";

			var reloadQuotedBooking = reloadBooking.Factory.Load<ViewQuotedBooking>(reloadBooking.PK).QuotedBooking;
			AssertEquals(1, reloadQuotedBooking.Logs.GetAllLogs().Count);
			AssertEquals(2, reloadBooking.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*JS_GoodsDescription.*Book"), reloadQuotedBooking.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertEquals("ADD", reloadBooking.Logs.GetAllLogs()[0].SL_SE_NKEvent);
			AssertMatch(new Regex($".*JS_GoodsDescription.*Sods"), reloadBooking.Logs.GetAllLogs()[1].Parameters["CHG"]);
		}

		public void TestQuotedBooking_ShipmentFieldStateChange_IsRoot()
		{
			ObjectFactory.Get<IProcessFieldChangeRuleTestHelper>().TurnOffBlacklistForTest();

			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("JS", new List<IBusinessObjectParentLocator>() { new ShipmentToQuotedBookingParentLocator() });
			});

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("JS", new ShipmentFieldStateChange());
			});

			var quotedBookingRule = Factory.New<IProcessFieldChangeRule>();
			quotedBookingRule.PFR_ProcessType = "QBK";
			quotedBookingRule.PFR_GroupName = "Group2";
			quotedBookingRule.PFR_SE_NKEvent = "Z05";
			quotedBookingRule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";
			Factory.Save();

			var factory = new BusinessObjectFactory();

			ViewQuotedBooking viewQuotedBooking = factory.New<ViewQuotedBooking>();
			AssertNull("Quoted Booking should return null as VB_JS & VB_TH are empty", viewQuotedBooking.QuotedBooking);

			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory).Booking;
			viewQuotedBooking.VB_JS = booking.PK;

			QuotedBooking quotedBooking = viewQuotedBooking.QuotedBooking;
			AssertEquals("Booking is not a shipment yet", false, (new ShipmentFieldStateChange()).IsRootObject(booking));

			new BuildConsolHelper().TurnBookingIntoShipment(quotedBooking.Booking, null, quotedBooking.PK);
			AssertEquals("Booking is not a shipment yet", true, (new ShipmentFieldStateChange()).IsRootObject(booking));
		}

		#endregion

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			ViewQuotedBooking viewQuotedBooking = Factory.New<ViewQuotedBooking>();
			viewQuotedBooking.VB_TH = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted).PK;
			return viewQuotedBooking;
		}

		protected override bool IsDeleteSupported()
		{
			return false;
		}
		#endregion
	}
}
