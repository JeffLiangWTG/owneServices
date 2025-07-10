using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using BusinessContext = Enterprise.Integration.Accounting.BusinessContext;
using ExchangeRate = Enterprise.Accounting.Business.JobInvoicing.ExchangeRate;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class QuoteTest : RatingTestCase
	{
		public void TestTH_StatusDate()
		{
			var today = ZDate.Today;
			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			quote.TH_QuoteEndDate = today.AddDays(1);
			quote.TH_QuoteDate = today.AddDays(-1);

			quote.TH_IsCancelled = true;
			AssertEquals(quote.TH_QuoteEndDate, quote.TH_StatusDate);
			AssertEquals(Quote.QuoteStatusOptions.Cancelled, quote.QuoteStatus);
			quote.TH_IsCancelled = false;

			quote.TH_Accepted = today;
			AssertEquals(quote.TH_Accepted, quote.TH_StatusDate);
			AssertEquals(Quote.QuoteStatusOptions.Accepted, quote.QuoteStatus);
			quote.TH_Accepted = ZDateTime.Empty;

			quote.TH_ClientAccepted = today;
			AssertEquals(quote.TH_ClientAccepted, quote.TH_StatusDate);
			AssertEquals(Quote.QuoteStatusOptions.ClientAccepted, quote.QuoteStatus);
			quote.TH_ClientAccepted = ZDateTime.Empty;

			quote.TH_QuoteEndDate = today.AddDays(-5);
			quote.TH_QuoteDate = today.AddDays(-6);
			AssertEquals(quote.TH_QuoteEndDate, quote.TH_StatusDate);
			AssertEquals(Quote.QuoteStatusOptions.Expired, quote.QuoteStatus);

			quote.TH_QuoteEndDate = today.AddDays(5);
			quote.TH_QuoteDate = today.AddDays(-6);
			AssertEquals(ZDateTime.Empty, quote.TH_StatusDate);
			AssertEquals(Quote.QuoteStatusOptions.Active, quote.QuoteStatus);
		}

		public void TestBusinessObjectsWithRelatedEvents()
		{
			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			quote.TH_OneTimeQuote = true;
			quote.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "USLAX", "AUSYD");

			Factory.Save();

			var relatedObjects = quote.BusinessObjectsWithRelatedEvents;
			var rateOneOffShipment = quote.CurrentOneOffQuote;
			Assert("Should load oneOffQuotes", relatedObjects.Any(x => x.PK == rateOneOffShipment.PK));
		}

		#region Actual Start Date

		public void TestFactorySave_QuoteApprovalConfirmed_UpdateQOPMilestone()
		{
			var workflowTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workflowTemplate.P0_ProcessType = "QBK";
			workflowTemplate.P0_SubType2 = "SQT";

			var mileStoneTemplate = workflowTemplate.WorkflowItems.Milestones.AddNew();
			mileStoneTemplate.TriggerConditions.TriggerEventCode = AutoEvents.QuotationInternallyApprovedCode;

			Factory.Save();

			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.SpotQuote, Factory);
			var workflowProvider = quotedBooking as IWorkflowProvider;
			var oneOffQuote = quotedBooking.Quote as Quote;

			Assert("Is One-Off quote", oneOffQuote.TH_OneTimeQuote);
			Assert("No Milestones created yet before save", !workflowProvider.WorkflowItems.Milestones.Any());
			Assert("Quote has not been approved yet", !oneOffQuote.IsApproved);

			oneOffQuote.ShowApprovalDialog += Quote_ShowApprovalDialog;
			confirmDialog = true;
			Factory.Save();

			Assert("One Milestone has been created after Factory.Save()", workflowProvider.WorkflowItems.Milestones.Any());
			Assert("Quote has now been approved yet after Factory.Save()", oneOffQuote.IsApproved);

			Assert("Quote is now approved", oneOffQuote.IsApproved);
			var mileStone = (ProcessTask)workflowProvider.WorkflowItems.Milestones.FirstOrDefault();
			Assert("Actual Start Date is set", !mileStone.P9_ActualDate.IsEmpty);
		}

		public void TestFactorySave_QuoteApprovalCancelled_NotUpdateQOPMilestone()
		{
			var workflowTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workflowTemplate.P0_ProcessType = "QBK";

			var mileStoneTemplate = workflowTemplate.WorkflowItems.Milestones.AddNew();
			mileStoneTemplate.TriggerConditions.TriggerEventCode = AutoEvents.QuotationInternallyApprovedCode;

			Factory.Save();

			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.SpotQuote, Factory);
			var workflowProvider = quotedBooking as IWorkflowProvider;
			var oneOffQuote = quotedBooking.Quote as Quote;

			Assert("Is One-Off quote", oneOffQuote.TH_OneTimeQuote);
			Assert("No Milestones created yet before save", !workflowProvider.WorkflowItems.Milestones.Any());
			Assert("Quote has not been approved yet", !oneOffQuote.IsApproved);

			oneOffQuote.ShowApprovalDialog += Quote_ShowApprovalDialog;
			confirmDialog = false;
			Factory.Save();

			Assert("Quote has not been approved yet", !oneOffQuote.IsApproved);
			var mileStone = (ProcessTask)workflowProvider.WorkflowItems.Milestones.FirstOrDefault();
			Assert("Actual Start Date is NOT set", mileStone.P9_ActualDate.IsEmpty);
		}

		#endregion

		#region Quote Is...

		public void TestQuoteIs()
		{
			var client = Helper.NewOrgHeader();
			var quote = Helper.NewQuote(client);

			AssertEquals(false, quote.IsClientRate());
			AssertEquals(false, quote.IsClientRateHavingSubsidiaryRelations());

			AssertEquals(false, quote.IsTariff());
			AssertEquals(false, quote.IsLevelOneTariff());
			AssertEquals(false, quote.IsAdditionalTariff());

			AssertEquals(true, quote.IsQuote());

			AssertEquals(false, quote.IsCosting());
			AssertEquals(false, quote.IsWiseCostRate());
			AssertEquals(false, quote.IsStandardCostRate());
		}

		#endregion

		#region CFX Properties

		public void TestCFXProperties()
		{
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);

			AssertEquals(0m, clientRate.TH_AirCFX);
			AssertEquals(0m, clientRate.TH_SeaCFX);
			AssertEquals(0m, clientRate.TH_ExportAirCFX);
			AssertEquals(0m, clientRate.TH_ExportSeaCFX);

			var cfxConfigurations = clientRate.Header.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany).AccCFXConfigurations;
			cfxConfigurations.SetUplifts("ALL", "IMP", "AIR", 6m);
			cfxConfigurations.SetUplifts("ALL", "IMP", "SEA", 7m);
			cfxConfigurations.SetUplifts("ALL", "EXP", "AIR", 8m);
			cfxConfigurations.SetUplifts("ALL", "EXP", "SEA", 9m);

			AssertEquals(6m, clientRate.TH_AirCFX);
			AssertEquals(7m, clientRate.TH_SeaCFX);
			AssertEquals(8m, clientRate.TH_ExportAirCFX);
			AssertEquals(9m, clientRate.TH_ExportSeaCFX);
		}

		public void TestTryAcceptQuote()
		{
			var quote = GetQuote(false);
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			quote.Header.Delete();
			AssertEquals(true, quote.TryAcceptQuote(out clientRate));
		}

		#endregion

		#region Notes

		public void TestNotes()
		{
			var quote = Factory.New<Quote>();
			var quoteNotes = quote.NoteTypes.Cast<PredefinedNoteType>().ToArray();

			Assert("PredefinedNoteTypes.Instance.QuoteCoverPageText should be in BO.NoteTypes", quoteNotes.Contains(PredefinedNoteTypes.Instance.QuoteCoverPageText));
			Assert("PredefinedNoteTypes.Instance.HandlingInstructions should be in BO.NoteTypes", quoteNotes.Contains(PredefinedNoteTypes.Instance.HandlingInstructions));
			Assert("PredefinedNoteTypes.Instance.InternalWorkNotes should be in BO.NoteTypes", quoteNotes.Contains(PredefinedNoteTypes.Instance.InternalWorkNotes));
			Assert("Autorating log is a valid pre-defined note type", quoteNotes.Contains(PredefinedNoteTypes.Instance.AutoRatingAuditLog));
		}

		#endregion

		#region Test IsInComparisonMode

		public void TestIsInComparisonMode()
		{
			var testQuote = Factory.New<Quote>();
			AssertEquals(ZBool.False, testQuote.IsInComparisonMode);

			testQuote.IsInComparisonMode = ZBool.True;
			AssertEquals(ZBool.True, testQuote.IsInComparisonMode);

			testQuote.IsInComparisonMode = ZBool.False;
			AssertEquals(ZBool.False, testQuote.IsInComparisonMode);
		}

		#endregion

		#region Test QuotationClientAddress

		public void TestQuotationClientAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var quote = Factory.New<Quote>();

			AssertNotNull(quote.QuotationClientAddress);

			quote.QuotationClientAddress.OrganisationPK = org.PK;

			AssertEquals(org.PK, quote.QuotationClientAddress.OrganisationPK);
		}

		#endregion

		#region Test TH_OH

		public void TestTH_OH()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var quote = Factory.NewWithValidTestData<Quote>();

			quote.QuotationClientAddress.OrganisationPK = org1.PK;

			AssertEquals(org1.PK, quote.TH_OH);

			quote.TH_OH = org2.PK;

			AssertEquals(org2.PK, quote.QuotationClientAddress.OrganisationPK);
			AssertEquals(org2.PK, quote.TH_OH);
		}

		#endregion

		#region Test TH_OHInfo

		public void TestTH_OHInfo()
		{
			var quote = Factory.NewWithValidTestData<Quote>();

			Assert(quote.TH_OHInfo is ZWrappedPropertyInfo);
			AssertEquals("TH_OH", quote.TH_OHInfo.Name);
		}

		#endregion

		#region TestPrintInheritedOriginDestinationChargesDefault

		public void TestPrintInheritedOriginDestinationChargesDefault()
		{
			try
			{
				var client = Helper.NewOrgHeader();
				var quote = Helper.NewQuote(client);
				Assert(quote.TH_PrintInheritedOriginCharges);
				Assert(quote.TH_PrintInheritedDestinationCharges);

				DocumentsDataRegistry.Instance.PrintInheritedOriginChargesDefault.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, false);
				quote = Helper.NewQuote(client);
				Assert(!quote.TH_PrintInheritedOriginCharges);
				Assert(quote.TH_PrintInheritedDestinationCharges);

				DocumentsDataRegistry.Instance.PrintInheritedDestinationChargesDefault.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, false);
				quote = Helper.NewQuote(client);
				Assert(!quote.TH_PrintInheritedOriginCharges);
				Assert(!quote.TH_PrintInheritedDestinationCharges);
			}
			finally
			{
				DocumentsDataRegistry.Instance.PrintInheritedOriginChargesDefault.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, true);
				DocumentsDataRegistry.Instance.PrintInheritedDestinationChargesDefault.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, true);
			}
		}

		#endregion

		#region Approve Quote

		public void TestInternalApproveQuote_Interactive()
		{
			var quote = GetQuote(false);

			Globals.IsUserInteractive = false; // service task
			var actionResult = quote.InternalApproveQuote();
			Factory.Save();
			AssertEquals("Should not be Approved", false, actionResult);
			AssertApproved(quote, false);
			AssertEquals(0, quoteApprovalSecurityDeniedEventFiredCount);
			Globals.IsUserInteractive = true;
		}

		public void TestGetExistingRatesInDifferentGlbCompany()
		{
			var client = Helper.NewOrgHeader();
			ClientRate clientRateInAnotherCompany;

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "NEW";
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			company1.Branches.Add(branch1);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				clientRateInAnotherCompany = Helper.NewClientRate(client);
			}
			Factory.Save();

			AssertNotNull(clientRateInAnotherCompany);

			ClientRate clientRateForQuote;
			var canAcceptQuote = Helper.NewQuote(client).TryAcceptQuote(out clientRateForQuote);

			Assert(canAcceptQuote);
			AssertNotEquals("Should not match quotes from other companies", clientRateForQuote, clientRateInAnotherCompany);
			AssertEquals("Instead client rate should be newly created", false, clientRateForQuote.IsInDatabase);

			var client2 = Helper.NewOrgHeader();
			var clientRate2 = Helper.NewClientRate(client2);
			Factory.Save();
			canAcceptQuote = Helper.NewQuote(client2).TryAcceptQuote(out clientRateForQuote);

			Assert(canAcceptQuote);
			AssertEquals("Should match client rate for the same client in the same company", clientRateForQuote, clientRate2);
			AssertEquals("Should match existing client rates", true, clientRateForQuote.IsInDatabase);
		}

		public void TestInternalApproveQuote()
		{
			var quote = GetQuote(false);

			var actionResult = quote.InternalApproveQuote();
			Factory.Save();
			AssertEquals("Should be Approved", true, actionResult);
			AssertApproved(quote, true);
			AssertEquals(0, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be NO Messages");

			actionResult = quote.InternalApproveQuote();
			Factory.Save();
			AssertEquals("Should be Approved", true, actionResult);
			AssertEquals(0, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be Message", "AlreadyApprovedMessage");

			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			quote = GetQuote(false);

			actionResult = quote.InternalApproveQuote();
			Factory.Save();
			AssertEquals("Should be NOT Approved", false, actionResult);
			AssertApproved(quote, false);
			AssertEquals(0, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be Dialog", "WishToApprove", "WishToApproveWhenSaving");

			confirmDialog = true;
			actionResult = quote.InternalApproveQuote();
			Factory.Save();
			AssertEquals("Should be Approved", true, actionResult);
			AssertApproved(quote, true);
			AssertEquals(0, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be Dialog", "WishToApprove");

			Env.Security.QuotationApprove.IsAllowed = false;
			quote = GetQuote(false);

			confirmDialog = false;
			quoteApprovalSecurityDeniedCancel = true;

			actionResult = quote.InternalApproveQuote();
			Factory.Save();
			AssertEquals("Should be NOT Approved", false, actionResult);
			AssertApproved(quote, false);
			AssertEquals(1, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be NO Messages");

			quoteApprovalSecurityDeniedCancel = false;

			actionResult = quote.InternalApproveQuote();
			Factory.Save();
			AssertEquals("Should be NOT Approved", false, actionResult);
			AssertApproved(quote, false);
			AssertEquals(2, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be Message", "LoginFailedMessage");

			nextSecurityForApprovalTest = new UserLoginController().GetSecurityForUser(GlbStaff.CurrentUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
			nextSecurityForApprovalTest.QuotationApprove.IsAllowed = false;
			actionResult = quote.InternalApproveQuote();
			Factory.Save();
			AssertEquals("Should be NOT Approved", false, actionResult);
			AssertApproved(quote, false);
			AssertEquals(3, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be Message", "HaveNoRightsMessage");

			nextSecurityForApprovalTest = new UserLoginController().GetSecurityForUser(GlbStaff.CurrentUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
			nextSecurityForApprovalTest.QuotationApprove.IsAllowed = true;
			actionResult = quote.InternalApproveQuote();
			Factory.Save();
			AssertEquals("Should be Approved", true, actionResult);
			AssertApproved(quote, true);
			AssertEquals(4, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be NO Messages");
		}

		public void TestInternalApproveQuoteWhenAccepting()
		{
			var quote = GetQuote(false);

			AssertEquals("Should be NOT Accepted", ZDateTime.Empty, quote.TH_Accepted);

			ClientRate clientRate;
			quote.TryAcceptQuote(out clientRate);
			Factory.Save();

			AssertNotNull(clientRate);
			AssertNotEquals("Should be Accepted", ZDateTime.Empty, quote.TH_Accepted);
			AssertApproved(quote, true);
			AssertEquals(0, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be NO Messages");

			quote.TryAcceptQuote(out clientRate);
			Factory.Save();

			AssertNotNull(clientRate);
			AssertEquals(0, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be NO Messages");

			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			quote = GetQuote(false);
			AssertEquals("Should be NOT Accepted", ZDateTime.Empty, quote.TH_Accepted);

			quote.TryAcceptQuote(out clientRate);
			Factory.Save();

			AssertNull(clientRate);
			AssertEquals("Should be NOT Accepted", ZDateTime.Empty, quote.TH_Accepted);
			AssertApproved(quote, false);
			AssertEquals(0, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be Dialog", "WishToApproveWhenAccepting", "WishToApproveWhenSaving");

			confirmDialog = true;

			quote.TryAcceptQuote(out clientRate);
			Factory.Save();

			AssertNotNull(clientRate);
			AssertNotEquals("Should be Accepted", ZDateTime.Empty, quote.TH_Accepted);
			AssertApproved(quote, true);
			AssertEquals(0, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be Dialog", "WishToApproveWhenAccepting");

			Env.Security.QuotationApprove.IsAllowed = false;

			quote = GetQuote(false);

			AssertEquals("Should be NOT Accepted", ZDateTime.Empty, quote.TH_Accepted);

			confirmDialog = false;
			quoteApprovalSecurityDeniedCancel = true;

			quote.TryAcceptQuote(out clientRate);
			Factory.Save();

			AssertNull(clientRate);
			AssertEquals("Should be NOT Accepted", ZDateTime.Empty, quote.TH_Accepted);
			AssertApproved(quote, false);
			AssertEquals(1, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be NO Messages");

			quoteApprovalSecurityDeniedCancel = false;

			quote.TryAcceptQuote(out clientRate);
			Factory.Save();

			AssertNull(clientRate);
			AssertEquals("Should be NOT Accepted", ZDateTime.Empty, quote.TH_Accepted);
			AssertApproved(quote, false);
			AssertEquals(2, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be Message", "LoginFailedMessage");

			nextSecurityForApprovalTest = new UserLoginController().GetSecurityForUser(GlbStaff.CurrentUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
			nextSecurityForApprovalTest.QuotationApprove.IsAllowed = false;

			quote.TryAcceptQuote(out clientRate);
			Factory.Save();

			AssertNull(clientRate);
			AssertEquals("Should be NOT Accepted", ZDateTime.Empty, quote.TH_Accepted);
			AssertApproved(quote, false);
			AssertEquals(3, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be Message", "HaveNoRightsMessage");

			nextSecurityForApprovalTest = new UserLoginController().GetSecurityForUser(GlbStaff.CurrentUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
			nextSecurityForApprovalTest.QuotationApprove.IsAllowed = true;

			quote.TryAcceptQuote(out clientRate);
			Factory.Save();

			AssertNotNull(clientRate);
			AssertNotEquals("Should be Accepted", ZDateTime.Empty, quote.TH_Accepted);
			AssertApproved(quote, true);
			AssertEquals(4, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be NO Messages");
		}

		[TestDate(2015, 02, 10)]
		public void TestAcceptQuote_CreatesClientRateWithQuoteStartAsRateStartDate()
		{
			var clientPK = Helper.NewOrgHeader().PK;
			Factory.Save();

			var clientRateFactory = new BusinessObjectFactory();
			var clientRate = clientRateFactory.NewWithValidTestData<ClientRate>();
			clientRate.TH_OH = clientPK;
			var clientRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "", "FRT", 150m);
			clientRateEntry.TI_RateStartDate = ZDate.Today.AddDays(-30);
			clientRateFactory.Save();

			var quote = GetQuote(false);
			quote.TH_OH = clientPK;
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "", "FRT", 85m);

			AssertEquals("Expected to default to today's date", ZDate.Today, quote.TH_QuoteDate);
			Assert("Should be able to accept quote", quote.TryAcceptQuote(out clientRate));

			clientRateFactory.Save();

			var airClientRates = clientRate.AllEntries.ToArray();
			AssertEquals("Quote should have been merged into client collection", 2, airClientRates.Length);
			AssertEquals("Expected the rate entry copied from quote to start with the quote accepted date", ZDate.Today, airClientRates.FirstOrDefault(x => x.TI_OriginLRC == "AUSYD").TI_RateStartDate);

			quote = GetQuote(false);
			quote.TH_OH = clientPK;
			quote.TH_QuoteDate = ZDate.Today.AddDays(-9);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUMEL", "", "FRT", 95m);

			Assert("Should be able to accept quote", quote.TryAcceptQuote(out clientRate));

			clientRateFactory.Save();

			airClientRates = clientRate.AllEntries.ToArray();
			AssertEquals("Quote should have been merged into client collection", 3, airClientRates.Length);
			AssertEquals("Expected the rate entry copied from quote to start with the quote accepted date", ZDate.Today.AddDays(-9), airClientRates.FirstOrDefault(x => x.TI_OriginLRC == "AUMEL").TI_RateStartDate);
		}

		class PrintTaskForTesting : PrintTask
		{
			public DeliveryInstructions passedInstructions;
			public bool instructionsWerePassed;

			public override DeliveryInstructionDestination RunWithPartialInstructions(AllowedDeliveryOptions deliveryOptions, DeliveryInstructions deliveryInstructions, ZArchitecture.Modules.ISecurityCheckpoint modifyDocumentCheckPoint)
			{
				instructionsWerePassed = true;
				passedInstructions = deliveryInstructions;
				return base.RunWithPartialInstructions(deliveryOptions, deliveryInstructions, modifyDocumentCheckPoint);
			}
		}

		[GuiTest]
		public void TestQuotationDocumentModeIsPassedWithDeliveryInstructions()
		{
			var quote = GetQuote(false);
			quote.AddRateEntry(RatingConstants.RateCategory.FCL, "ALL", "AU", "NL");

			var documentSupporter = (Quote.QuoteDocumentSupporter)quote.DocumentSupporter;

			var testTask = new PrintTaskForTesting();
			setFinalModeResult = false;

			documentSupporter.BuildPrintTask(null);
			Assert(!testTask.instructionsWerePassed);
			AssertNull(testTask.passedInstructions);
			documentSupporter.RunTask(testTask);
			Assert(testTask.instructionsWerePassed);
			Assert("QuotationDocumentMode is Draft at this point -- should be passed with delivery instructions", testTask.passedInstructions.IsDraft);
			Assert("Draft should be set to read only here to prevent user from changing it on the preview form", testTask.passedInstructions.IsDraft_ReadOnly);

			testTask = new PrintTaskForTesting();
			setFinalModeResult = true;

			documentSupporter.BuildPrintTask(null);
			Assert(!testTask.instructionsWerePassed);
			AssertNull(testTask.passedInstructions);
			documentSupporter.RunTask(testTask);
			Assert("QuotationDocumentMode is Final at this point -- delivery instructions should be passed as null", testTask.instructionsWerePassed && testTask.passedInstructions.ExcludeDocumentsThatHaveSectionsWhichContainNoDataRows);

			Assert("Passing null delivery instructions should not cause document to be draft", !new DeliveryInstructions().IsDraft);
		}

		[GuiTest]
		public void TestQuoteEmailSubjectLine()
		{
			var quote = GetQuote(false);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "", "FRT", 85m);

			var documentSupporter = (Quote.QuoteDocumentSupporter)quote.DocumentSupporter;

			var query = new DocumentZQuery(CargoWise.Definitions.BusinessContext.Quotation, "Acceptance Page");
			query.AddToFilter(StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Individual Pages");
			var menuItems = Factory.Load<DocumentCommand>(query);

			AssertNotNull("MenuItems should not be null", menuItems);
			AssertEquals("One Acceptance Page document exists", 1, menuItems.Length);
			AssertNotNull("MenuItem should not be null", menuItems[0]);

			menuItems[0].SU_EmailSubjectLine = "That's a poor email subject...";
			menuItems[0].Parent = quote;

			var task = documentSupporter.BuildPrintTask(menuItems[0]);

			documentSupporter.RunTask(task);
			AssertNotNull("DeliveryInstructions should not be null", documentSupporter.DeliveryInstructionsForTest);
			documentSupporter.DeliveryInstructionsForTest.Destination = DeliveryInstructionDestination.DocManager;
			task.Run(documentSupporter.DeliveryInstructionsForTest);

			var deliveryGroups = documentSupporter.DeliveryInstructionsForTest.DeliveryGroups;
			AssertNotNull("DeliveryGroups should not be null", deliveryGroups);
			AssertEquals("One DeliveryGroup exists", 1, deliveryGroups.Count);
			AssertNotNull("DeliveryGroup should not be null", deliveryGroups[0]);

			AssertEquals("Wrong email subject!", menuItems[0].SU_EmailSubjectLine, deliveryGroups[0].SB_EmailSubjectLine);
		}

		public void TestInternalApproveQuoteWhenFinalizing_Interactive()
		{
			var quote = GetQuote(false);
			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var documentSupporter = (Quote.QuoteDocumentSupporter)((IDocumentSupportable)quote).DocumentSupporter;
			confirmDialog = true;
			setFinalModeResult = true;
			Globals.IsUserInteractive = false;
			documentSupporter.BuildPrintTask(null);
			AssertEquals("Should be Draft Mode", QuotationDocumentMode.Draft, quote.DocumentPrintMode);

			Globals.IsUserInteractive = true;
		}

		public void TestInternalApproveQuoteWhenFinalizing()
		{
			Env.Security.QuotationApprove.IsAllowed = true;
			quoteApprovalSecurityDeniedCancel = false;
			DataRegistryRating.Instance.SpotQuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			setFinalModeResult = false;
			confirmDialog = false;

			var quote = GetQuote(false);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL", "FRT", 10);

			Factory.Save();
			AssertQuote(quote, true, 0, "Should be NO Messages");

			var documentSupporter = (Quote.QuoteDocumentSupporter)((IDocumentSupportable)quote).DocumentSupporter;

			documentSupporter.BuildPrintTask(null);
			AssertQuote(quote, true, 0, "Should be NO Messages");
			AssertEquals("Should be Draft Mode", QuotationDocumentMode.Draft, quote.DocumentPrintMode);

			setFinalModeResult = true;

			documentSupporter.BuildPrintTask(null);
			AssertQuote(quote, true, 0, "Should be NO Messages");
			AssertEquals("Should be Final Mode", QuotationDocumentMode.Final, quote.DocumentPrintMode);

			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			quote = GetQuote(false);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL", "FRT", 20);

			Factory.Save();
			AssertQuote(quote, false, 0, "Should be Dialog", "WishToApproveWhenSaving");

			documentSupporter = (Quote.QuoteDocumentSupporter)((IDocumentSupportable)quote).DocumentSupporter;

			documentSupporter.BuildPrintTask(null);
			AssertQuote(quote, false, 0, "Should be Dialog", "WishToApproveWhenFinalizing");
			AssertEquals("Should be Draft Mode", QuotationDocumentMode.Draft, quote.DocumentPrintMode);

			confirmDialog = true;

			documentSupporter.BuildPrintTask(null);
			AssertQuote(quote, true, 0, "Should be Dialog", "WishToApproveWhenFinalizing");
			AssertEquals("Should be Final Mode", QuotationDocumentMode.Final, quote.DocumentPrintMode);

			Env.Security.QuotationApprove.IsAllowed = false;
			confirmDialog = false;
			quoteApprovalSecurityDeniedCancel = true;

			quote = GetQuote(false);
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL", "FRT", 30);

			Factory.Save();
			AssertQuote(quote, false, 0, "Should be NO Messages");

			documentSupporter = (Quote.QuoteDocumentSupporter)((IDocumentSupportable)quote).DocumentSupporter;

			documentSupporter.BuildPrintTask(null);
			AssertQuote(quote, false, 1, "Should be NO Messages");
			AssertEquals("Should be Draft Mode", QuotationDocumentMode.Draft, quote.DocumentPrintMode);

			quoteApprovalSecurityDeniedCancel = false;

			documentSupporter.BuildPrintTask(null);
			AssertQuote(quote, false, 2, "Should be Message", "LoginFailedMessage");
			AssertEquals("Should be Draft Mode", QuotationDocumentMode.Draft, quote.DocumentPrintMode);

			nextSecurityForApprovalTest = new UserLoginController().GetSecurityForUser(GlbStaff.CurrentUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
			nextSecurityForApprovalTest.QuotationApprove.IsAllowed = false;
			documentSupporter.BuildPrintTask(null);
			AssertQuote(quote, false, 3, "Should be Message", "HaveNoRightsMessage");
			AssertEquals("Should be Draft Mode", QuotationDocumentMode.Draft, quote.DocumentPrintMode);

			nextSecurityForApprovalTest = new UserLoginController().GetSecurityForUser(GlbStaff.CurrentUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
			nextSecurityForApprovalTest.QuotationApprove.IsAllowed = true;
			documentSupporter.BuildPrintTask(null);
			AssertQuote(quote, true, 4, "Should be NO Messages");
			AssertEquals("Should be Final Mode", QuotationDocumentMode.Final, quote.DocumentPrintMode);
		}

		public void TestInternalApproveQuoteWhenSaving_Interactive()
		{
			Globals.IsUserInteractive = false;
			var quote = GetQuote(false);
			Factory.Save();
			AssertQuote(quote, false, 0, "Should be NO Messages");
			AssertApprovalWasCancelled(quote, 0);

			Globals.IsUserInteractive = true;
		}

		public void TestInternalApproveQuoteWhenSaving()
		{
			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			confirmDialog = false;
			Env.Security.QuotationApprove.IsAllowed = true;

			var quote = GetQuote(false);

			Factory.Save();
			AssertQuote(quote, true, 0, "Should be NO Messages");
			AssertApprovalWasCancelled(quote, 0);

			quote.TH_QuoteNumber = "1";
			quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			Factory.Save();
			AssertQuote(quote, true, 0, "Should be NO Messages");
			AssertApprovalWasCancelled(quote, 0);

			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			quote = GetQuote(false);

			Factory.Save();
			AssertQuote(quote, false, 0, "Should be Dialog", "WishToApproveWhenSaving");
			AssertApprovalWasCancelled(quote, 0);

			quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			Factory.Save();
			AssertQuote(quote, false, 0, "Should be NO Messages");
			AssertApprovalWasCancelled(quote, 0);

			confirmDialog = true;

			quote.InternalApproveQuote();
			AssertQuote(quote, true, 0, "Should be Dialog", "WishToApprove");
			AssertApprovalWasCancelled(quote, 0);

			quote.AddRateEntry("AIR", "LSE", "AUBNE", "USLAX");
			Factory.Save();
			AssertQuote(quote, true, 0, "Should be NO Messages");
			AssertApprovalWasCancelled(quote, 1);

			quote = GetQuote(false);

			quote.TH_QuoteNumber = "2";
			Factory.Save();
			AssertQuote(quote, true, 0, "Should be Dialog", "WishToApproveWhenSaving");
			AssertApprovalWasCancelled(quote, 0);

			quote.TH_QuoteNumber = "3";
			Factory.Save();
			AssertQuote(quote, true, 0, "Should be NO Messages");
			AssertApprovalWasCancelled(quote, 0);

			quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			Factory.Save();
			AssertQuote(quote, true, 0, "Should be NO Messages");
			AssertApprovalWasCancelled(quote, 1);

			Env.Security.QuotationApprove.IsAllowed = false;
			confirmDialog = false;

			quote = GetQuote(false);

			Factory.Save();
			AssertQuote(quote, false, 0, "Should be NO Messages");
			AssertApprovalWasCancelled(quote, 0);
		}

		public void TestInternalApproveSpotQuote_Interactive()
		{
			var quote = GetQuote(true);
			confirmDialog = true;

			Globals.IsUserInteractive = false;

			var actionResult = quote.InternalApproveQuote();
			Factory.Save();
			AssertEquals("Should be NOT Approved", false, actionResult);
			AssertApproved(quote, false);

			Globals.IsUserInteractive = true;
		}

		void SetQuoteRevenue(Quote quote, ZDecimal revenue)
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = quote.PK;
			job.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;
			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			job.LocalChargesPK = localClient.PK;

			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_JH = job.PK;
			charge.JR_LocalSellAmt = revenue;
			charge.JR_OSSellAmt = revenue;

			job.LoadCharges_ForTestOnly();
		}

		void SetFirstLevelApprovalRequiredInRegistry(int firstLevelUpTo)
		{
			var authorisationSettings = new PaymentThreeLevelAuthorisationSettingsCollection();
			var upTo = authorisationSettings.AddNew();
			upTo.Amount = firstLevelUpTo;
			upTo.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			upTo.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			var above = authorisationSettings.AddNew();
			above.Amount = firstLevelUpTo;
			above.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			above.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			DataRegistryRating.Instance.SpotQuoteApprovalSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, authorisationSettings);
		}

		public void TestInternalApproveSpotQuote()
		{
			var quote = GetQuote(true);
			var actionResult = quote.InternalApproveQuote();
			Factory.Save();
			AssertEquals("Should be NOT Approved", false, actionResult);
			AssertApproved(quote, false);
			AssertEquals(0, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be Dialog", "WishToApprove", "WishToApproveWhenSaving");

			confirmDialog = true;

			actionResult = quote.InternalApproveQuote();
			Factory.Save();
			AssertEquals("Should be Approved", true, actionResult);
			AssertApproved(quote, true);
			AssertEquals(0, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be Dialog", "WishToApprove");

			actionResult = quote.InternalApproveQuote();
			Factory.Save();
			AssertEquals("Should be Approved", true, actionResult);
			AssertEquals(0, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be Message", "AlreadyApprovedMessage");

			Env.Security.OneOffQuoteFirstLevelApproval.IsAllowed = false;
			SetFirstLevelApprovalRequiredInRegistry(1000);

			quote = GetQuote(true);
			SetQuoteRevenue(quote, 500);

			confirmDialog = false;
			quoteApprovalSecurityDeniedCancel = true;

			actionResult = quote.InternalApproveQuote();
			Factory.Save();
			AssertEquals("Should be NOT Approved", false, actionResult);
			AssertApproved(quote, false);
			AssertEquals(1, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be NO Messages");

			quoteApprovalSecurityDeniedCancel = false;

			actionResult = quote.InternalApproveQuote();
			Factory.Save();
			AssertEquals("Should be NOT Approved", false, actionResult);
			AssertApproved(quote, false);
			AssertEquals(2, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be Message", "LoginFailedMessage");

			nextSecurityForApprovalTest = new UserLoginController().GetSecurityForUser(GlbStaff.CurrentUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
			nextSecurityForApprovalTest.OneOffQuoteFirstLevelApproval.IsAllowed = false;

			actionResult = quote.InternalApproveQuote();
			Factory.Save();
			AssertEquals("Should be NOT Approved", false, actionResult);
			AssertApproved(quote, false);
			AssertEquals(3, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be Message", "HaveNoRightsMessage");

			nextSecurityForApprovalTest = new UserLoginController().GetSecurityForUser(GlbStaff.CurrentUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
			nextSecurityForApprovalTest.OneOffQuoteFirstLevelApproval.IsAllowed = true;

			actionResult = quote.InternalApproveQuote();
			Factory.Save();
			AssertEquals("Should be Approved", true, actionResult);
			AssertApproved(quote, true);
			AssertEquals(4, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be NO Messages");

			DataRegistryRating.Instance.SpotQuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			quote = GetQuote(true);
			SetQuoteRevenue(quote, 500);

			actionResult = quote.InternalApproveQuote();
			Factory.Save();
			AssertEquals("Should be Approved", true, actionResult);
			AssertApproved(quote, true);
			AssertMessages("Should be NO Messages");
		}

		public void TestInternalApproveSpotQuoteWhenFinalizing_Interactive()
		{
			setFinalModeResult = true;
			var quote = GetQuote(true);
			quote.SpotQuoteChargesIncorrect += delegate
			{ };
			var documentSupporter = (Quote.QuoteDocumentSupporter)((IDocumentSupportable)quote).DocumentSupporter;
			confirmDialog = true;
			Globals.IsUserInteractive = false;

			documentSupporter.BuildPrintTask(null);
			AssertApproved(quote, false);
			AssertEquals("Should be Draft Mode", QuotationDocumentMode.Draft, quote.DocumentPrintMode);
			AssertEquals(0, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be Dialog", "WishToApproveWhenFinalizing");

			Globals.IsUserInteractive = true;
		}

		public void TestHideClientReplyLink()
		{
			var quote = Helper.NewQuote(NewClient);
			quote.DocumentPrintMode = QuotationDocumentMode.Unknown;
			AssertEquals("Print Mode is Unknown", true, quote.HideClientReplyLink);

			quote.DocumentPrintMode = QuotationDocumentMode.Final;
			AssertEquals(false, quote.HideClientReplyLink);

			quote.DocumentPrintMode = QuotationDocumentMode.Draft;
			AssertEquals("Print Mode is Draft", true, quote.HideClientReplyLink);

			quote.DocumentPrintMode = QuotationDocumentMode.Final;
			AssertEquals(false, quote.HideClientReplyLink);

			quote.TH_IsCancelled = true;
			AssertEquals("When Quote is Deactivated", true, quote.HideClientReplyLink);

			quote.TH_IsCancelled = false;
			AssertEquals(false, quote.HideClientReplyLink);

			quote.TH_ClientAccepted = ZDateTime.Now;
			AssertEquals("When client accepted quote", true, quote.HideClientReplyLink);

			quote.TH_ClientAccepted = ZDateTime.Empty;
			AssertEquals(false, quote.HideClientReplyLink);

			quote.TH_IsOneOffQuoteConsumed = true;
			AssertEquals("When Quote used for Autorating", true, quote.HideClientReplyLink);

			quote.TH_IsOneOffQuoteConsumed = false;
			AssertEquals(false, quote.HideClientReplyLink);

			quote.TH_Accepted = ZDateTime.Now;
			AssertEquals("When Quote was converted to BWQ", true, quote.HideClientReplyLink);

			quote.TH_Accepted = ZDateTime.Empty;
			AssertEquals(false, quote.HideClientReplyLink);
		}

		public void TestInternalApproveSpotQuoteWhenFinalizing()
		{
			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			setFinalModeResult = false;
			confirmDialog = false;

			var quote = GetQuote(true);
			quote.SpotQuoteChargesIncorrect += delegate
			{ };
			Factory.Save();
			AssertApproved(quote, false);
			AssertEquals(0, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be Dialog", "WishToApproveWhenSaving");

			var documentSupporter = (Quote.QuoteDocumentSupporter)((IDocumentSupportable)quote).DocumentSupporter;

			documentSupporter.BuildPrintTask(null);
			AssertApproved(quote, false);
			AssertEquals("Should be Draft Mode", QuotationDocumentMode.Draft, quote.DocumentPrintMode);
			AssertEquals(0, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be Dialog", "WishToApproveWhenFinalizing");

			setFinalModeResult = true;

			documentSupporter.BuildPrintTask(null);
			AssertApproved(quote, false);
			AssertEquals("Should be Draft Mode", QuotationDocumentMode.Draft, quote.DocumentPrintMode);
			AssertEquals(0, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be Dialog", "WishToApproveWhenFinalizing");

			confirmDialog = true;

			documentSupporter.BuildPrintTask(null);
			AssertApproved(quote, true);
			AssertEquals("Should be Final Mode", QuotationDocumentMode.Final, quote.DocumentPrintMode);
			AssertEquals(0, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be Dialog", "WishToApproveWhenFinalizing");

			Env.Security.OneOffQuoteFirstLevelApproval.IsAllowed = false;
			SetFirstLevelApprovalRequiredInRegistry(1000);

			quote = GetQuote(true);
			SetQuoteRevenue(quote, 500);
			quote.SpotQuoteChargesIncorrect += delegate
			{ };
			documentSupporter = (Quote.QuoteDocumentSupporter)((IDocumentSupportable)quote).DocumentSupporter;

			confirmDialog = false;
			quoteApprovalSecurityDeniedCancel = true;

			documentSupporter.BuildPrintTask(null);
			AssertApproved(quote, false);
			AssertEquals("Should be Draft Mode", QuotationDocumentMode.Draft, quote.DocumentPrintMode);
			AssertEquals(1, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be NO Messages");

			quoteApprovalSecurityDeniedCancel = false;

			documentSupporter.BuildPrintTask(null);
			AssertApproved(quote, false);
			AssertEquals("Should be Draft Mode", QuotationDocumentMode.Draft, quote.DocumentPrintMode);
			AssertEquals(2, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be Message", "LoginFailedMessage");

			nextSecurityForApprovalTest = new UserLoginController().GetSecurityForUser(GlbStaff.CurrentUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
			nextSecurityForApprovalTest.OneOffQuoteFirstLevelApproval.IsAllowed = false;
			documentSupporter.BuildPrintTask(null);
			AssertApproved(quote, false);
			AssertEquals("Should be Draft Mode", QuotationDocumentMode.Draft, quote.DocumentPrintMode);
			AssertEquals(3, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be Message", "HaveNoRightsMessage");

			nextSecurityForApprovalTest = new UserLoginController().GetSecurityForUser(GlbStaff.CurrentUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
			nextSecurityForApprovalTest.OneOffQuoteFirstLevelApproval.IsAllowed = true;
			documentSupporter.BuildPrintTask(null);
			AssertApproved(quote, true);
			AssertEquals("Should be Final Mode", QuotationDocumentMode.Final, quote.DocumentPrintMode);
			AssertEquals(4, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be NO Messages");

			DataRegistryRating.Instance.SpotQuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			quote = GetQuote(true);
			SetQuoteRevenue(quote, 500);
			quote.SpotQuoteChargesIncorrect += delegate
			{ };
			documentSupporter = (Quote.QuoteDocumentSupporter)((IDocumentSupportable)quote).DocumentSupporter;
			Factory.Save();

			documentSupporter.BuildPrintTask(null);
			AssertApproved(quote, true);
			AssertEquals("Should be Final Mode", QuotationDocumentMode.Final, quote.DocumentPrintMode);
			AssertEquals(4, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages("Should be NO Messages");
		}

		public void TestInternalApproveSpotQuoteWhenSaving_Interactive()
		{
			DataRegistryRating.Instance.SpotQuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			confirmDialog = false;
			Env.Security.OneOffQuoteApproveOneOffQuotes.IsAllowed = true;

			var quote = GetQuote(true);

			Globals.IsUserInteractive = false;

			Factory.Save();
			AssertQuote(quote, false, 0, "Should be NO Messages");
			AssertApprovalWasCancelled(quote, 0);

			Globals.IsUserInteractive = true;
		}

		public void TestInternalApproveSpotQuoteWhenSaving()
		{
			DataRegistryRating.Instance.SpotQuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			confirmDialog = false;
			Env.Security.OneOffQuoteFirstLevelApproval.IsAllowed = true;
			SetFirstLevelApprovalRequiredInRegistry(1000);

			var quote = GetQuote(true);
			SetQuoteRevenue(quote, 500);

			Factory.Save();
			AssertQuote(quote, true, 0, "Should be NO Messages");
			AssertApprovalWasCancelled(quote, 0);

			quote.TH_QuoteNumber = "1";
			quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			Factory.Save();
			AssertQuote(quote, true, 0, "Should be NO Messages");
			AssertApprovalWasCancelled(quote, 0);

			DataRegistryRating.Instance.SpotQuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			quote = GetQuote(true);
			SetQuoteRevenue(quote, 500);

			Factory.Save();
			AssertQuote(quote, false, 0, "Should be Dialog", "WishToApproveWhenSaving");
			AssertApprovalWasCancelled(quote, 0);

			quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			Factory.Save();
			AssertQuote(quote, false, 0, "Should be NO Messages");
			AssertApprovalWasCancelled(quote, 0);

			confirmDialog = true;

			quote.InternalApproveQuote();
			AssertQuote(quote, true, 0, "Should be Dialog", "WishToApprove");
			AssertApprovalWasCancelled(quote, 0);

			quote.AddRateEntry("AIR", "LSE", "AUBNE", "USLAX");
			Factory.Save();
			AssertQuote(quote, true, 0, "Should be NO Messages");
			AssertApprovalWasCancelled(quote, 1);

			quote = GetQuote(true);
			SetQuoteRevenue(quote, 500);

			quote.TH_QuoteNumber = "2";
			Factory.Save();
			AssertQuote(quote, true, 0, "Should be Dialog", "WishToApproveWhenSaving");
			AssertApprovalWasCancelled(quote, 0);

			quote.TH_QuoteNumber = "3";
			Factory.Save();
			AssertQuote(quote, true, 0, "Should be NO Messages");
			AssertApprovalWasCancelled(quote, 0);

			quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			Factory.Save();
			AssertQuote(quote, true, 0, "Should be NO Messages");
			AssertApprovalWasCancelled(quote, 1);

			Env.Security.OneOffQuoteFirstLevelApproval.IsAllowed = false;
			confirmDialog = false;

			quote = GetQuote(true);
			SetQuoteRevenue(quote, 500);

			Factory.Save();
			AssertQuote(quote, false, 0, "Should be NO Messages");
			AssertApprovalWasCancelled(quote, 0);
		}

		readonly List<string> lastQuoteApprovalMessages = new List<string>();
		bool confirmDialog;
		bool setFinalModeResult;

		SecurityCore nextSecurityForApprovalTest;
		int quoteApprovalSecurityDeniedEventFiredCount;
		bool quoteApprovalSecurityDeniedCancel;

		Quote GetQuote(bool isOneTime)
		{
			var quote = Helper.NewQuote(NewClient);
			quote.ShowApprovalDialog += Quote_ShowApprovalDialog;
			quote.ShowApprovalMessage += Quote_ShowApprovalMessage;
			quote.QuoteApprovalSecurityNotGranted += Quote_QuoteApprovalSecurityNotGranted;
			quote.SetFinalMode += Quote_SetFinalMode;

			quote.TH_OneTimeQuote = isOneTime;

			AssertApproved(quote, false);

			return quote;
		}

		void AssertApproved(Quote quote, bool expected)
		{
			var message = string.Format("Quote Should be {0}Approved", expected ? "" : "NOT ");

			if (quote.TH_OneTimeQuote)
			{
				AssertEquals(message, expected, quote.CurrentOneOffQuote.TT_QuoteApprovedByManager);
			}

			AssertEquals(message, expected ? 1 : 0, quote.Logs.Find(Quote.GetApprovedLogQuery(quote.PK, ZBool.False)).Length);
		}

		void AssertMessages(ZString message, params string[] expectedMessages)
		{
			if (expectedMessages == null || expectedMessages.Length == 0)
			{
				AssertEquals(message, 0, lastQuoteApprovalMessages.Count);
			}
			else
			{
				AssertEquals(message, string.Join(", ", expectedMessages), string.Join(", ", lastQuoteApprovalMessages.ToArray()));
				lastQuoteApprovalMessages.Clear();
			}
		}

		void AssertQuote(Quote quote, bool approved, int expectedEventFiredCount, ZString message, params string[] expectedMessages)
		{
			AssertApproved(quote, approved);
			AssertEquals(expectedEventFiredCount, quoteApprovalSecurityDeniedEventFiredCount);
			AssertMessages(message, expectedMessages);
		}

		void AssertApprovalWasCancelled(Quote quote, int expectedEventCount)
		{
			AssertEquals(expectedEventCount, quote.Logs.Find(Quote.GetApprovedLogQuery(quote.PK, ZBool.True)).Length);
		}

		void Quote_ShowApprovalDialog(object sender, Quote.ApprovalDialogEventArgs e)
		{
			lastQuoteApprovalMessages.Add(e.Dialog.ToString());
			e.Cancel = !confirmDialog;
		}

		void Quote_ShowApprovalMessage(Quote.ApprovalMessage approvalMessage)
		{
			lastQuoteApprovalMessages.Add(approvalMessage.ToString());
		}

		void Quote_QuoteApprovalSecurityNotGranted(object sender, Quote.QuoteApprovalSecurityEventArgs e)
		{
			quoteApprovalSecurityDeniedEventFiredCount++;
			e.Cancel = quoteApprovalSecurityDeniedCancel;
			if (nextSecurityForApprovalTest != null)
			{
				e.OverrideLogin.SetUserSecurityForTests(nextSecurityForApprovalTest);
			}
		}

		void Quote_SetFinalMode(object sender, EventArgs e)
		{
			((Quote.SetFinalModeArgs)e).Result = setFinalModeResult;
		}

		#endregion

		#region Status

		public void TestQuoteStatus()
		{
			var testQuote = Factory.New<Quote>();

			testQuote.TH_ClientAccepted = ZDateTime.Today;
			AssertEquals("Quote is accepted by client", Quote.QuoteStatusOptions.ClientAccepted, testQuote.QuoteStatus);
			AssertEquals("Quote is accepted by client", "Client Accepted", testQuote.QuoteStatus);

			testQuote.TH_Accepted = ZDateTime.Today;
			AssertEquals("Quote is accepted", Quote.QuoteStatusOptions.Accepted, testQuote.QuoteStatus);
			AssertEquals("Quote is accepted", "Accepted", testQuote.QuoteStatus);

			testQuote.TH_IsCancelled = true;
			AssertEquals("Quote is Cancelled", Quote.QuoteStatusOptions.Cancelled, testQuote.QuoteStatus);
			AssertEquals("Quote is Cancelled", "Canceled", testQuote.QuoteStatus);

			testQuote.TH_IsCancelled = false;
			testQuote.TH_Accepted = ZDateTime.Empty;
			testQuote.TH_ClientAccepted = ZDateTime.Empty;
			AssertEquals("Quote is Active", Quote.QuoteStatusOptions.Active, testQuote.QuoteStatus);
			AssertEquals("Quote is Active", "Active", testQuote.QuoteStatus);

			testQuote.InternalApproveQuote();
			AssertEquals("Quote is Approved", Quote.QuoteStatusOptions.Approved, testQuote.QuoteStatus);
			AssertEquals("Quote is Approved", "Approved", testQuote.QuoteStatus);

			testQuote.TH_IsLocked = true;
			AssertEquals("Quote is Finalised", Quote.QuoteStatusOptions.Finalized, testQuote.QuoteStatus);
			AssertEquals("Quote is Finalised", "Finalized", testQuote.QuoteStatus);

			testQuote.TH_QuoteEndDate = ZDate.Today.AddDays(-5);
			AssertEquals("Quote is Expired", Quote.QuoteStatusOptions.Expired, testQuote.QuoteStatus);
			AssertEquals("Quote is Expired", "Expired", testQuote.QuoteStatus);

			testQuote.TH_QuoteEndDate = ZDate.Empty;
			testQuote.TH_OneTimeQuote = true;
			testQuote.TH_IsOneOffQuoteConsumed = true;
			AssertEquals("Quote is Used", Quote.QuoteStatusOptions.Used, testQuote.QuoteStatus);
			AssertEquals("Quote is Used", "Used", testQuote.QuoteStatus);
		}

		public void TestQuoteStatusAfterClientAccept()
		{
			var testQuote = Factory.New<Quote>();
			testQuote.TH_ClientAccepted = ZDateTime.Now;
			AssertEquals("Quote is Client Accepted", Quote.QuoteStatusOptions.ClientAccepted, testQuote.QuoteStatus);

			testQuote.TH_Accepted = ZDateTime.Now;
			AssertEquals("Quote is Accepted After Consolidated", Quote.QuoteStatusOptions.Accepted, testQuote.QuoteStatus);

			testQuote.TH_Accepted = ZDateTime.Empty;
			AssertEquals("Quote is Client Accepted", Quote.QuoteStatusOptions.ClientAccepted, testQuote.QuoteStatus);

			testQuote.TH_IsOneOffQuoteConsumed = true;
			AssertEquals("Quote is Used After Autorating", Quote.QuoteStatusOptions.Used, testQuote.QuoteStatus);
		}

		#endregion

		#region Properties

		public void TestIsOneOffQuoteInDatabase()
		{
			var standardQuote = Factory.NewWithValidTestData<Quote>();
			AssertEquals(false, standardQuote.IsOneOffQuoteInDatabase);

			Factory.Save();
			AssertEquals(false, standardQuote.IsOneOffQuoteInDatabase);

			var newQuote = Factory.NewWithValidTestData<Quote>();
			newQuote.TH_OneTimeQuote = true;
			AssertEquals(false, newQuote.IsOneOffQuoteInDatabase);

			Factory.Save();
			AssertEquals(true, newQuote.IsOneOffQuoteInDatabase);
		}

		public void TestSetStartEndDateForOneOffQuotes()
		{
			var quote = Factory.New<Quote>();

			quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var airRateEntries = quote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR);

			AssertEquals(1, airRateEntries.Count);

			quote.TH_QuoteDate = ZDate.Today.AddDays(-1);
			quote.TH_QuoteEndDate = ZDate.Today.AddDays(1);

			quote.TH_OneTimeQuote = true;

			foreach (RateEntry rateEntry in airRateEntries)
			{
				AssertEquals(quote.TH_QuoteDate, rateEntry.TI_RateStartDate);
				AssertEquals(quote.TH_QuoteEndDate, rateEntry.TI_RateEndDate);
			}
		}

		public void TestSetStartEndDateForOneOffQuotesDbHitCount()
		{
			var quote = Factory.New<Quote>();
			quote.TH_OneTimeQuote = true;
			AssertEquals(0, Factory.GetTableHitCount(RateEntrySchema.Constants.TableName));
		}

		public void TestShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges()
		{
			var quote = Factory.New<Quote>();
			Assert("Should update audit fields if only children have changes", ((IUpdateAuditFields)quote).ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges);
		}

		#endregion

		#region QuoteDateTime

		public void TestQuoteDateTimeUpdatesRateEntriesStartDate()
		{
			var quote = Factory.New<Quote>();
			var rateEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.FCL);
			var rateEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.LCL);

			quote.TH_QuoteDate = new ZDate(2014, 08, 28);
			AssertEquals(new ZDate(2014, 08, 28), rateEntry1.TI_RateStartDate);
			AssertEquals(new ZDate(2014, 08, 28), rateEntry2.TI_RateStartDate);
		}

		#endregion

		#region QuoteEndDate

		public void TestQuoteEndDateUpdatesRateEntriesEndDate()
		{
			var quote = Factory.New<Quote>();
			var rateEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.FCL);
			var rateEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.LCL);

			quote.TH_QuoteEndDate = new ZDate(2014, 08, 29);
			AssertEquals(new ZDate(2014, 08, 29), rateEntry1.TI_RateEndDate);
			AssertEquals(new ZDate(2014, 08, 29), rateEntry2.TI_RateEndDate);
		}

		#endregion

		#region QuoteFollowUpDate

		public void TestQuoteFollowUpDateValidation()
		{
			var quote = Factory.New<Quote>();
			var testDate = ZDate.Today;
			quote.TH_QuoteDate = testDate;
			quote.TH_FollowUpDateInfo.ClearValue();
			quote.TH_QuoteEndDate = testDate.AddMonths(6);
			quote.TH_FollowUpDate = testDate.AddDays(5);
			quote.Validation.ValidateTH_QuoteDate();
			AssertNoWarnings(quote.TH_FollowUpDateInfo);

			quote.TH_FollowUpDateInfo.ClearValue();
			quote.TH_FollowUpDate = testDate.AddDays(-5);
			quote.Validation.ValidateTH_QuoteDate();
			AssertEquals("Warning for Follow Up Date Before Quote Date", true, quote.TH_FollowUpDateInfo.HasWarnings());
			AssertHasWarning(quote.TH_FollowUpDateInfo, "The Follow Up Date is not within the Quotation Period.");

			quote.TH_FollowUpDateInfo.ClearValue();
			quote.TH_FollowUpDate = quote.TH_QuoteEndDate.AddDays(5);
			quote.Validation.ValidateTH_QuoteDate();
			AssertEquals("Warning for Follow Up Date After Quote End Date", true, quote.TH_FollowUpDateInfo.HasWarnings());
			AssertHasWarning(quote.TH_FollowUpDateInfo, "The Follow Up Date is not within the Quotation Period.");
		}

		#endregion

		#region QuotationClientAddress

		public void TestQuotationClientAddressWithUniversalCopyRelatedEntityAttribute()
		{
			var propertyInfo = typeof(Quote).GetProperty("QuotationClientAddress", BindingFlags.Public | BindingFlags.Instance);
			AssertNotNull(propertyInfo);

			var relatedEntityAttribute = propertyInfo.GetCustomAttributes(typeof(UniversalCopyRelatedEntityAttribute), true).FirstOrDefault() as UniversalCopyRelatedEntityAttribute;
			AssertNotNull(relatedEntityAttribute);
			AssertEquals("should skip property E2_ParentID in Universal Copy.", "E2_ParentID", relatedEntityAttribute.CommaSeparatedSkipPropertiesNames);
			AssertEquals("should call 'MakePersistentEvenIfEmpty' method to make address saved by factory", "MakePersistentEvenIfEmpty", relatedEntityAttribute.MakeRelatedEntitySavedByFactoryMethod);
		}

		#endregion

		public void TestActiveRateEntryCollection()
		{
			var quote = Factory.New<Quote>();
			var rateEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.FCL);
			var rateEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.LCL);

			AssertEquals(2, quote.AllEntriesCollection.Count);
		}

		public void TestClientRateEntriesAreNotIncorrectlyCastToQuoteRateEntries()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			clientRate.TH_GC = GlbCompany.CurrentCompany.PK;
			clientRate.AddRateEntry("AIR");

			var quote = Factory.New<Quote>();
			quote.TH_OneTimeQuote = true;
			quote.TH_GC = GlbCompany.CurrentCompany.PK;
			quote.TH_QuoteNumber = "00001000";
			quote.TH_OH = clientRate.Header.PK;

			quote.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection.AddNew(typeof(RateEntry));
			quote.EntryCollections[RatingConstants.RateCategory.FCL].LazyLoadingCollection.AddNew(typeof(RateEntry));
			quote.EntryCollections[RatingConstants.RateCategory.LCL].LazyLoadingCollection.AddNew(typeof(RateEntry));

			AssertNoExceptionThrown("Quote should only iterate EntryCollection as RateEntry", () => { quote.TryAcceptQuote(out clientRate); });
			AssertNoExceptionThrown("Quote should only iterate EntryCollection as RateEntry", () => { quote.PublishedAirFreightAgents.ToList(); });
			AssertNoExceptionThrown("Quote should only iterate EntryCollection as RateEntry", () => { quote.PublishedSeaFreightAgents.ToList(); });
		}

		#region Follow Up Reminders

		[TestDate(2013, 04, 30, 6, 0, 0)]
		public void TestCalendarReminder()
		{
			var client = Factory.New<OrgHeader>();
			client.OH_FullName = "Some really random client";
			client.OH_Code = "RAN";

			var salesRep = Factory.New<GlbStaff>();
			salesRep.GS_FullName = "Sales Rep";
			salesRep.GS_Code = "SR";
			salesRep.GS_EmailAddress = "salesrep@email.com";
			client.StaffAssignments.OverallSalesRep = salesRep.GS_Code;

			RatingDataRegistry.Instance.QuoteFollowUpDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			var quote = Factory.New<Quote>();
			quote.TH_OH = client.PK;
			AssertEquals("No quote follow up date specified", ZDateTime.Empty, quote.TH_FollowUpDate);
			quote.TH_QuoteNumber = "9999";

			AssertEquals("No calendar reminders for empty follow up date", false, quote.ShouldCreateReminder);

			quote.TH_FollowUpDate = new ZDateTime(2004, 12, 3);
			AssertEquals("Calendar reminder created", true, quote.ShouldCreateReminder);

			Env.OutgoingMailManager.EmailsCreated.Clear();

			Factory.Save();

			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("Correct subject", "Follow up for Quote 9999 for client Some really random client", sentEmail.Subject);

			RatingDataRegistry.Instance.QuoteFollowUpDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 7);
			quote = Factory.New<Quote>();
			quote.TH_OH = client.PK;
			AssertEquals("Quote follow up date specified for today + 7 days", ZDateTime.Today.AddDays(7), quote.TH_FollowUpDate);

			AssertExceptionThrown("Should not allow to set more than 365 days in the Registry",
								typeof(RegistryValidationException),
								() => RatingDataRegistry.Instance.QuoteFollowUpDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 777));
		}

		[TestDate(2013, 04, 30, 6, 0, 0)]
		public void TestCalendarReminderWithAttachment()
		{
			var client = Factory.New<OrgHeader>();
			client.OH_FullName = "Some really random client";
			client.OH_Code = "RAN";

			var salesRep = Factory.New<GlbStaff>();
			salesRep.GS_FullName = "Sales Rep";
			salesRep.GS_Code = "SR";
			salesRep.GS_EmailAddress = "salesrep@email.com";
			client.StaffAssignments.OverallSalesRep = salesRep.GS_Code;

			RatingDataRegistry.Instance.QuoteFollowUpDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			var quote = Factory.New<Quote>();
			quote.TH_OH = client.PK;
			AssertEquals("No quote follow up date specified", ZDateTime.Empty, quote.TH_FollowUpDate);
			quote.TH_QuoteNumber = "9999";

			AssertEquals("No calendar reminders for empty follow up date", false, quote.ShouldCreateReminder);
			var attachment = ((IDocManagerSupport)quote).DocManagerInfo.AddFileOrDocument(new byte[] { 2 }, "Not visible.txt", Core.Constants.RefDocTypes.SystemQuotation);

			quote.TH_FollowUpDate = new ZDateTime(2004, 12, 3);
			AssertEquals("Calendar reminder created", true, quote.ShouldCreateReminder);

			Env.OutgoingMailManager.EmailsCreated.Clear();

			Factory.Save();

			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("Reminder has Attachments", 1, sentEmail.Attachments.Count);
			AssertEquals("Correct attachment", attachment.ImageData, sentEmail.Attachments[0].Data);
		}

		public void TestCalendarReminderNoOrg()
		{
			var quote = Factory.New<Quote>();
			quote.TH_QuoteNumber = "9999";
			quote.TH_FollowUpDate = new ZDateTime(2004, 12, 3);

			AssertEquals("Calendar reminder created", false, quote.ShouldCreateReminder);
		}

		#endregion

		#region Test Published Air and Sea Freight Agents

		public void TestPublishedAirFreightAgents()
		{
			ZString origin = "INBOM";
			ZString destination = Env.CurrentBranch.NKUNLOCO == "AUMEL" ? "AUSYD" : "AUMEL";

			var client = GetForwarder();
			var quote = Helper.NewQuote(client);

			AssertPublishedFreightAgents(quote.PublishedAirFreightAgents, "Air Agents Collection should be Empty");

			var aIR_DES_EXP_HAN = AddForwarderAgent(client, transportAir, destination, directionExport, statusHandles);
			var aIR_ORI_IMP_PUB = AddForwarderAgent(client, transportAir, origin, directionImport, statusPublished);
			var aIR_DES_EXP_PUB = AddForwarderAgent(client, transportAir, destination, directionExport, statusPublished);
			var sEA_DES_IMP_PUB = AddForwarderAgent(client, transportSea, destination, directionImport, statusPublished);
			var aIR_LAX_IMP_PUB = AddForwarderAgent(client, transportAir, "USLAX", directionImport, statusPublished);
			AssertPublishedFreightAgents(quote.PublishedAirFreightAgents, "Air Agents Collection should be Empty");

			var entry = quote.AddRateEntry("AIR", "LSE", origin, destination);
			AssertPublishedFreightAgents(quote.PublishedAirFreightAgents, "Air Agents Collection should be Empty");

			var aIR_DES_IMP_PUB = AddForwarderAgent(client, transportAir, destination, directionImport, statusPublished);
			AssertPublishedFreightAgents(quote.PublishedAirFreightAgents, "Organisation is a published Air Freight Agent so should be included", aIR_DES_IMP_PUB);

			var anotherClient = GetForwarder();
			entry.TI_OH_AgentOverride = anotherClient.PK;
			AssertPublishedFreightAgents(quote.PublishedAirFreightAgents, "Manually specified agent is added", anotherClient.MainAddress);
		}

		public void TestPublishedAirFreightAgents_OneOffQuote()
		{
			ZString destination = Env.CurrentBranch.NKUNLOCO == "AUMEL" ? "AUSYD" : "AUMEL";

			var client = GetForwarder();
			client.OH_RL_NKClosestPort = destination;

			var quote = Helper.NewQuote(client);
			quote.TH_OneTimeQuote = true;
			quote.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Air;
			quote.CurrentOneOffQuote.TT_ContainerMode = Constants.ContainerModes.Loose;
			quote.CurrentOneOffQuote.TT_RL_NKDeliveryLocation = destination;

			AddForwarderAgent(client, transportAir, destination, directionImport, statusHandles);
			AddForwarderAgent(client, transportAir, destination, directionExport, statusPublished);
			AssertPublishedFreightAgents(quote.PublishedAirFreightAgents, "Organisation is not a published Air Freight Agent so should not be included");

			var aIR_DES_IMP_PUB = AddForwarderAgent(client, transportAir, destination, directionImport, statusPublished);
			AssertPublishedFreightAgents(quote.PublishedAirFreightAgents, "Organisation is a published Air Freight Agent so should be included", aIR_DES_IMP_PUB);
		}

		public void TestPublishedSeaFreightAgents()
		{
			ZString origin = Env.CurrentBranch.NKUNLOCO == "AUMEL" ? "AUSYD" : "AUMEL";
			ZString destination = "GBLON";

			var client = GetForwarder();
			var quote = Helper.NewQuote(client);
			var entry = quote.AddRateEntry("FCL", "SEA", origin, destination, "", "20GP");

			var sEA_ORI_EXP_APP = AddForwarderAgent(client, transportSea, origin, directionExport, statusAppointed);
			var sEA_DES_EXP_PUB = AddForwarderAgent(client, transportSea, destination, directionExport, statusPublished);
			var aIR_ORI_EXP_PUB = AddForwarderAgent(client, transportAir, origin, directionExport, statusPublished);
			var sEA_AMD_EXP_PUB = AddForwarderAgent(client, transportSea, "INAMD", directionExport, statusPublished);
			AssertPublishedFreightAgents(quote.PublishedSeaFreightAgents, "Sea Agents Collection should be Empty");

			var sEA_ORI_EXP_PUB = AddForwarderAgent(client, transportSea, origin, directionExport, statusPublished);
			AssertPublishedFreightAgents(quote.PublishedSeaFreightAgents, "Organisation is a published Sea Freight Agent so should be included", sEA_ORI_EXP_PUB);
		}

		public void TestPublishedSeaFreightAgents_OneOffQuote()
		{
			ZString origin = Env.CurrentBranch.NKUNLOCO == "AUMEL" ? "AUSYD" : "AUMEL";

			var client = GetForwarder();

			var quote = Helper.NewQuote(client);
			quote.TH_OneTimeQuote = true;
			quote.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Sea;
			quote.CurrentOneOffQuote.TT_ContainerMode = Constants.ContainerModes.FCL;
			quote.CurrentOneOffQuote.TT_RL_NKReceivalLocation = origin;

			var sEA_ORI_EXP_HAN = AddForwarderAgent(client, transportSea, origin, directionExport, statusHandles);
			AssertPublishedFreightAgents(quote.PublishedSeaFreightAgents, "Organisation is not a published Sea Freight Agent so should not be included");

			var sEA_ORI_EXP_PUB = AddForwarderAgent(client, transportSea, origin, directionExport, statusPublished);
			AssertPublishedFreightAgents(quote.PublishedSeaFreightAgents, "Organisation is a published Sea Freight Agent so should be included", sEA_ORI_EXP_PUB);
		}

		const string transportAir = Core.Constants.TransportModes.Air;
		const string transportSea = Core.Constants.TransportModes.Sea;
		const string statusPublished = AgentStatusList.Codes.Published;
		const string statusAppointed = AgentStatusList.Codes.Appointed;
		const string statusHandles = AgentStatusList.Codes.Handles;
		const string directionExport = AgentDirectionList.Codes.Export;
		const string directionImport = AgentDirectionList.Codes.Import;

		int addressCount;

		OrgHeader GetForwarder()
		{
			var result = Factory.New<OrgHeader>();
			result.OH_Code = string.Format("FWD{0}", ++addressCount);
			result.OH_RL_NKClosestPort = Env.CurrentBranch.NKUNLOCO;
			result.MainAddress.OA_Address1 = string.Format("{0} main address", result.OH_Code);
			result.OH_IsForwarder = true;

			return result;
		}

		OrgAddress AddForwarderAgent(OrgHeader forwarder, ZString agentTransportMode, ZString agentPortOrCountry, ZString agentDirection, ZString agentStatus)
		{
			var address = forwarder.Addresses.AddNew();
			address.OA_Code = string.Format("{0}_{1}_{2}_{3}_{4}", agentTransportMode, agentPortOrCountry, agentDirection, agentStatus, ++addressCount);
			address.OA_Address1 = address.OA_Code;

			return AddForwarderAgent(forwarder, address, agentPortOrCountry, agentTransportMode, agentDirection, agentStatus);
		}

		OrgAddress AddForwarderAgent(OrgHeader forwarder, OrgAddress address, ZString agentPortOrCountry, ZString agentTransportMode, ZString agentDirection, ZString agentStatus)
		{
			var newAppAgent = forwarder.AppointedAgentPorts.AddNew();
			newAppAgent.O5_PortOrCountry = agentPortOrCountry;
			newAppAgent.O5_OA_AgentOfficeAddress = address.PK;
			newAppAgent.O5_AgentDirection = agentDirection;

			switch (agentTransportMode)
			{
				case Core.Constants.TransportModes.Air:
					newAppAgent.O5_AirAgentStatus = agentStatus;
					break;

				case Core.Constants.TransportModes.Rail:
					newAppAgent.O5_RailAgentStatus = agentStatus;
					break;

				case Core.Constants.TransportModes.Road:
					newAppAgent.O5_RoadAgentStatus = agentStatus;
					break;

				case Core.Constants.TransportModes.Sea:
					newAppAgent.O5_SeaAgentStatus = agentStatus;
					break;
			}

			forwarder.Factory.Save();

			return address;
		}

		void AssertPublishedFreightAgents(OrgAddressCollection collection, ZString message, params OrgAddress[] expected)
		{
			AssertContainsExactElementsInAnyOrder(message, expected, collection);
		}

		#endregion

		#region Amendment Quote Number Generation

		public void TestAmendmentQuoteNumberGeneration()
		{
			var originalQuote = Helper.NewQuote(NewClient);
			originalQuote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			originalQuote.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");

			var originalQuoteNumber = originalQuote.TH_QuoteNumber;
			var newQuoteNumber = originalQuote.GetNewQuoteNumberForAmendment();

			AssertEquals("Amended quote has suffix on quote number", originalQuoteNumber + "/A", newQuoteNumber);
		}

		public void TestSetQuoteNumberTwiceOnSameQuoteDoesntAmend()
		{
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				var quote = Factory.New<Quote>();
				quote.SetQuoteNumber();
				var previousQuote = quote.TH_QuoteNumber;
				quote.SetQuoteNumber();
				AssertEquals($"Previous quote: {previousQuote} does not match the current quote: {quote.TH_QuoteNumber}.", previousQuote, quote.TH_QuoteNumber);
			}
		}

		#endregion

		#region Customized Quote Number Generation

		public void TestCustomizedQuoteNumberGeneration()
		{
			RatingDataRegistry.Instance.IncludeBranchCodeInQuoteNumber.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			CargoWise.Data.Db.Connection.BeginTransaction(); // TH_QuoteNumber hits a number fountain and so needs to be in a transation or it blows up.
			try
			{
				var quote = Factory.New<Quote>();
				quote.SetQuoteNumber();
				Assert("No QuoteNumberCustomization - should not be empty", !quote.TH_QuoteNumber.IsEmpty);
				Assert("No QuoteNumberCustomization - should only contain numbers", quote.TH_QuoteNumber.IsNumbersOnlyOrEmpty);
			}
			finally
			{
				CargoWise.Data.Db.Connection.RollbackTransaction(); // TH_QuoteNumber hits a number fountain and so needs to be in a transation or it blows up.
			}

			RatingDataRegistry.Instance.IncludeBranchCodeInQuoteNumber.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			CargoWise.Data.Db.Connection.BeginTransaction(); // TH_QuoteNumber hits a number fountain and so needs to be in a transation or it blows up.
			try
			{
				var quote = Factory.New<Quote>();
				quote.SetQuoteNumber();
				Assert("QuoteNumberCustomization Prefix - should not be empty", !quote.TH_QuoteNumber.IsEmpty);
				AssertEquals("QuoteNumberCustomization Prefix - should be in correct format", "Q" + Env.CurrentBranch.Code, quote.TH_QuoteNumber.SubstringSafe(0, 4));
				Assert("QuoteNumberCustomization Fountain Number - should not be empty", !quote.TH_QuoteNumber.SubstringSafe(4).IsEmpty);
				Assert("QuoteNumberCustomization Fountain Number - should be only be numbers", quote.TH_QuoteNumber.SubstringSafe(4).IsNumbersOnlyOrEmpty);

				var newQuoteNumber = quote.GetNewQuoteNumberForAmendment();

				Assert("QuoteNumberCustomization Prefix - should not be empty", !newQuoteNumber.IsEmpty);
				AssertEquals("QuoteNumberCustomization Prefix - should be in correct format", "Q" + Env.CurrentBranch.Code, newQuoteNumber.SubstringSafe(0, 4));
				Assert("QuoteNumberCustomization Fountain Number - should not be empty", !newQuoteNumber.SubstringSafe(4).IsEmpty);
				Assert("QuoteNumberCustomization Fountain Number - should be only be numbers", newQuoteNumber.SubstringSafe(4, 8).IsNumbersOnlyOrEmpty);
				AssertEquals("Amended quote has suffix on quote number", "/A", newQuoteNumber.SubstringSafe(12));
			}
			finally
			{
				CargoWise.Data.Db.Connection.RollbackTransaction(); // TH_QuoteNumber hits a number fountain and so needs to be in a transation or it blows up.
			}
		}

		#endregion

		#region NumberFountain failure handling

		[UseSnapshotProtection]
		public void TestQuoteNumber_GivenErrorOnSaving_WhenReSave_ThenNumberFountainShouldGetNextQuoteNumber()
		{
			using (RunNonTransactioned())
			{
				var newFactory1 = new BusinessObjectFactory();
				var quote1 = newFactory1.NewWithValidTestData<QuoteForTest>();
				quote1.Header.OH_Code = "ORG1";
				quote1.TH_QuoteNumber = ZString.Empty; // empty QuoteNumber that is set by NewWithValidTestData
				quote1.AfterOnSaving = () => { throw new ZSaveException(new ZDataException(new InvalidOperationException("Error on saving"), null, null), newFactory1); };
				AssertExceptionThrown<ZSaveException>("Precondition: error on saving", () => newFactory1.Save());
				AssertNullOrEmpty("Error on saving should clear out quote1.TH_QuoteNumber", quote1.TH_QuoteNumber);

				var newFactory2 = new BusinessObjectFactory();
				var quoteBooking2 = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.SpotQuote, newFactory2);
				var quote2 = ((Quote)quoteBooking2.Quote);
				quote2.TH_QuoteNumber = ZString.Empty;
				quote2.TH_GC = ZGuid.Empty; // avoid index error: FK_UX__TH_OH_TH_GC_TH_RateType_TH_QuoteNumber_TH_GlobalRateLevel
				newFactory2.Save();
				AssertEquals("quote2.TH_QuoteNumber", "00001000", quote2.TH_QuoteNumber);

				quote1.AfterOnSaving = null;
				newFactory1.Save();

				CombineAssertions("GIVEN error on saving WHEN re-save THEN NumberFountain should getNext QuoteNumber.", () =>
				{
					AssertEquals("quote1.TH_QuoteNumber", "00001001", quote1.TH_QuoteNumber);
					AssertEquals("quote2.TH_QuoteNumber", "00001000", quote2.TH_QuoteNumber);
				});
			}
		}

		class QuoteForTest : Quote
		{
			public QuoteForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			public override void OnSaving()
			{
				base.OnSaving();
				AfterOnSaving?.Invoke();
			}

			public Action AfterOnSaving { get; set; }
		}

		[UseSnapshotProtection]
		public void TestQuoteNumberUniqueIndexFailureHandler()
		{
			using (RunNonTransactioned())
			{
				RatingDataRegistry.Instance.IncludeBranchCodeInQuoteNumber.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				var quote = Factory.New<Quote>();
				Factory.Save();
				AssertEquals("Quote number: ", "00001000", quote.TH_QuoteNumber);

				quote.TH_QuoteNumber = "00001001"; // this number would conflict with Fountain.GetNext()
				Factory.Save();
				AssertEquals("Quote number: ", "00001001", quote.TH_QuoteNumber);

				var anotherQuote = Factory.New<Quote>();
				try
				{
					Factory.Save();
					Fail("Index exception should happen");
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
					ErrorReporter.Clear();
				}

				Factory.Save();

				AssertEquals("No QuoteNumberCustomization - Quotation number", "00001001", quote.TH_QuoteNumber);
				AssertEquals("No QuoteNumberCustomization - Quotation number", "00001002", anotherQuote.TH_QuoteNumber);
			}
		}

		[UseSnapshotProtection]
		public void TestQuoteCustomisedNumberUniqueIndexFailureHandler()
		{
			using (RunNonTransactioned())
			{
				RatingDataRegistry.Instance.IncludeBranchCodeInQuoteNumber.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				var quote = Factory.New<Quote>();
				Factory.Save();
				var branchCode = Env.CurrentBranch.Code;

				AssertEquals("Customised quote number: ", "Q" + branchCode + "00001000", quote.TH_QuoteNumber);

				quote.TH_QuoteNumber = string.Format("Q{0}00001001", branchCode); // this number would conflict with Fountain.GetNext()
				Factory.Save();
				AssertEquals("Quote number: ", string.Format("Q{0}00001001", branchCode), quote.TH_QuoteNumber);

				// Create more than 2 quotes in another branch so it has the higher number
				var otherBranchQuery = new ZQuery(GlbBranchSchema.GB_GC, Env.CurrentCompany.PK)
					.AddToFilter(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentBranchPK);
				var otherBranch = Factory.LoadTop1<GlbBranch>(otherBranchQuery);
				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, otherBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var branchFactory = new BusinessObjectFactory() { RefreshEnabled = false };
					branchFactory.New<Quote>();
					branchFactory.New<Quote>();
					branchFactory.New<Quote>();
					branchFactory.Save();
				}

				var anotherQuote = Factory.New<Quote>();
				try
				{
					Factory.Save();
					Fail("Index exception should happen");
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
					ErrorReporter.Clear();
				}

				Factory.Save();

				AssertEquals("QuoteNumberCustomization - Quotation number", string.Format("Q{0}00001001", branchCode), quote.TH_QuoteNumber);
				AssertEquals("QuoteNumberCustomization - Quotation number", string.Format("Q{0}00001002", branchCode), anotherQuote.TH_QuoteNumber);
			}
		}

		#endregion

		public class QuoteCustomisedNumberUniqueIndexFailureHandlerTest : NumberFountainUniqueIndexFailureHandlingTest
		{
			protected override Type BizOTypeToTest
			{
				get { return typeof(Quote); }
			}

			protected override SchemaColumn ColumnThatUsesNumberFountain
			{
				get { return RatingHeaderSchema.TH_QuoteNumber; }
			}

			protected override INumberFountainProxy NumberFountainToTest
			{
				get { return (RatingDataRegistry.Instance.IncludeBranchCodeInQuoteNumber.Value) ? Env.NumberFountains.QuoteCustomisedNumber(Env.CurrentBranch.Code) : Env.NumberFountains.QuoteNumber; }
			}

			protected override NameValueCollection AdditionalInsertValues
			{
				get
				{
					var additionalInsertValues = base.AdditionalInsertValues;
					additionalInsertValues.Add(RatingHeaderSchema.TH_GC.Name, string.Format("'{0}'", Env.CurrentCompany.PK));
					additionalInsertValues.Add(RatingHeaderSchema.TH_RateType.Name, "'QTE'");
					additionalInsertValues.Add(RatingHeaderSchema.TH_QuoteDate.Name, string.Format("'{0}'", ZDate.Today.AddMonths(-6).ToString("yyyy-MM-dd")));

					return additionalInsertValues;
				}
			}
		}

		#region Most Recent Ammendment Retrieval

		public void TestMostRecentAmendmentRetrieval()
		{
			var originalQuote = Helper.NewQuote(NewClient);
			originalQuote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			originalQuote.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");
			originalQuote.TH_ClientAccepted = ZDateTime.Today;
			originalQuote.TH_Accepted = ZDateTime.Today;
			Factory.Save();
			var newQuote1 = (Quote)originalQuote.CopyIncludingChildren();
			newQuote1.TH_QuoteNumber = originalQuote.GetNewQuoteNumberForAmendment();
			AssertEquals(ZDateTime.Empty, newQuote1.TH_ClientAccepted);
			AssertEquals(ZDateTime.Empty, newQuote1.TH_Accepted);

			var newQuote2 = (Quote)originalQuote.CopyIncludingChildren();
			newQuote2.TH_QuoteNumber = originalQuote.GetNewQuoteNumberForAmendment();

			var newQuote3 = (Quote)originalQuote.CopyIncludingChildren();
			newQuote3.TH_IsLocked = true;
			newQuote3.TH_QuoteNumber = originalQuote.GetNewQuoteNumberForAmendment();

			AssertEquals("3rd Quote is most recent Amendment", newQuote3, originalQuote.GetMostRecentAmendment(true));
			AssertEquals("2nd Quote is most recent un-locked Amendment", newQuote2, originalQuote.GetMostRecentAmendment(false));
		}

		#endregion

		#region Copy Quote

		[TestDate(2023, 06, 18)]
		public void TestCopyQuote()
		{
			var originalQuote = Helper.NewQuote(NewClient);
			originalQuote.TH_QuoteDate = ZDate.Today.AddDays(-30);
			originalQuote.TH_QuoteEndDate = ZDate.Today.AddDays(30);
			originalQuote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").AddRateLine("WAR", FlatCalculator.Code);
			originalQuote.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX").AddRateLine("WAR", FlatCalculator.Code);
			Factory.Save();

			var newQuote = (Quote)originalQuote.CopyIncludingChildren();
			var followUpDays = RatingDataRegistry.Instance.QuoteFollowUpDays.Value;
			var expectedFollowUpDays = ZDateTime.Today.AddDays(followUpDays);
			AssertEquals("Client PK", ZGuid.Empty, newQuote.TH_OH);
			AssertEquals("Accepted Date", ZDateTime.Empty, newQuote.TH_Accepted);
			AssertEquals("Qupte Start Date", ZDateTime.Today, newQuote.TH_QuoteDate);
			AssertEquals("Quote End Date", newQuote.DefaultQuoteEndDate, newQuote.TH_QuoteEndDate);
			AssertEquals("Is Locked", false, newQuote.TH_IsLocked);
			AssertEquals("Is Cancelled", false, newQuote.TH_IsCancelled);
			AssertEquals("Follow Up Date", expectedFollowUpDays, newQuote.TH_FollowUpDate);

			var originalAirRateEntries = originalQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR);
			var originalLclRateEntries = originalQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.LCL);
			var newAirRateEntries = newQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR);
			var newLclRateEntries = newQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.LCL);

			AssertEquals("AIR Collection Count", originalAirRateEntries.Count, newAirRateEntries.Count);
			AssertEquals("LCL Collection Count", originalLclRateEntries.Count, newLclRateEntries.Count);

			AssertEquals("AIR 0 Origin", originalAirRateEntries[0].TI_OriginLRC, newAirRateEntries[0].TI_OriginLRC);
			AssertEquals("AIR 0 Destination", originalAirRateEntries[0].TI_DestinationLRC, newAirRateEntries[0].TI_DestinationLRC);
			AssertEquals("AIR 0 Start Date", newQuote.TH_QuoteDate, newAirRateEntries[0].TI_RateStartDate);
			AssertEquals("AIR 0 End Date", newQuote.TH_QuoteEndDate, newAirRateEntries[0].TI_RateEndDate);

			AssertEquals("LCL 0 Origin", originalLclRateEntries[0].TI_OriginLRC, newLclRateEntries[0].TI_OriginLRC);
			AssertEquals("LCL 0 Destination", originalLclRateEntries[0].TI_DestinationLRC, newLclRateEntries[0].TI_DestinationLRC);
			AssertEquals("LCL 0 Start Date", newQuote.TH_QuoteDate, newLclRateEntries[0].TI_RateStartDate);
			AssertEquals("LCL 0 End Date", newQuote.TH_QuoteEndDate, newLclRateEntries[0].TI_RateEndDate);

			AssertEquals("AIR 0 Rate Lines Count", originalAirRateEntries[0].RateLines.Count, newAirRateEntries[0].RateLines.Count);
			AssertEquals("LCL 0 Rate Lines Count", originalLclRateEntries[0].RateLines.Count, newLclRateEntries[0].RateLines.Count);
		}

		public void TestCopyCancelledQuote()
		{
			var quote = Helper.NewQuote(NewClient);
			quote.CancelQuote();
			quote.TH_QuoteCancellationReason = "AAA";
			Factory.Save();

			var newQuote = (Quote)quote.CopyIncludingChildren();
			AssertEquals(ZString.Empty, newQuote.TH_QuoteCancellationReason);
		}

		public void TestCopyOneOffQuote()
		{
			// System Exchange Rate: BUY - USD - 0.98
			ExchangeRateReader.GetReaderInstance().ClearCache();
			var rateFactory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(rateFactory);
			creator.CreateExchangeRate(Helper.Currencies["USD"], Constants.ExchangeRateTypes.Code.BuyRate, 0.98m, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			rateFactory.Save();

			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();

			AccChequeBook checkbook = Factory.NewWithValidTestData<AccChequeBook>();
			checkbook.AK_AutoPrintCheque = true;
			checkbook.AK_StartNo = 1;
			checkbook.AK_LastNo = 100;
			checkbook.AK_CurrentNo = 3;
			checkbook.AK_AB = bankAccount.PK;
			checkbook.AK_GB = GlbBranch.CurrentBranch.PK;

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			var taxRate = testObjectCreator.GSTFREE1;
			var taxMsg = testObjectCreator.TaxMsg2;

			var originalQuote = Helper.NewQuote(NewClient);
			originalQuote.TH_OneTimeQuote = true;
			originalQuote.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Sea;
			originalQuote.CurrentOneOffQuote.TT_ContainerMode = Constants.RateMode.SEA;
			originalQuote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			originalQuote.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");

			var container = originalQuote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_ContainerCount = (short)2;
			container.TC_RC = GP20.PK;

			var loose = originalQuote.CurrentOneOffQuote.LooseCargo.AddNew();
			loose.TPL_PackLineCount = (short)3;
			loose.TPL_Height = 2;
			loose.TPL_Width = 3;
			loose.TPL_Length = 4;

			var originalJob = new Job.Loader(originalQuote).TryCreate();
			var exchangeRate = originalJob.ExchangeRates.AddNew();
			exchangeRate.JF_RX_NKRateCurrency = "USD";
			exchangeRate.JF_BaseRate = 0.97m;

			var charge = originalJob.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OSSellAmt = 98m;

			charge.JR_APInvoiceNum = "12345";
			charge.JR_APInvoiceDate = ZDateTime.Now;
			charge.JR_APDocumentReceivedDate = ZDateTime.Now;
			charge.JR_PaymentDate = ZDateTime.Now;
			charge.JR_PaymentType = ReceiptTypes.Cheque;
			charge.JR_AB = bankAccount.PK;
			charge.JR_AK = checkbook.PK;
			charge.JR_ChequeNo = "000002";
			charge.JR_CostReference = "ABC124";

			charge.JR_AT_CostGSTRate = taxRate.PK;
			charge.JR_CostTaxDate = ZDate.Today;
			charge.JR_A9_CostVATClass = taxMsg.PK;

			charge.JR_AT_SellGSTRate = taxRate.PK;
			charge.JR_SellTaxDate = ZDate.Today;
			charge.JR_A9_SellVATClass = taxMsg.PK;

			charge.JR_CostRated = true;
			charge.JR_SellRated = true;
			charge.JR_CostRatingOverride = false;
			charge.JR_SellRatingOverride = false;
			charge.JR_CostRatingOverrideComment = "Hello Cost from FRT";
			charge.JR_SellRatingOverrideComment = "Hello Sell from FRT";

			var chargeWithEmptyComment = originalJob.Charges.AddNew();
			chargeWithEmptyComment.JR_AC = Helper.ChargeCodes["BAF"].PK;
			chargeWithEmptyComment.JR_CostRatingOverrideComment = "";
			chargeWithEmptyComment.JR_SellRatingOverrideComment = "";

			Factory.Save();
			using (DataRegistryRating.Instance.PreserveQuoteRevenueRatingBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var newQuote = (Quote)originalQuote.CopyIncludingChildren();

				AssertEquals("Client PK", ZGuid.Empty, newQuote.TH_OH);

				AssertEquals("AIR Collection Count", originalQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR).Count, newQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR).Count);
				AssertEquals("LCL Collection Count", originalQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.LCL).Count, newQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.LCL).Count);

				Assert("New quote is a one off quote", newQuote.TH_OneTimeQuote);

				AssertEquals("1 one off container details exist", 1, newQuote.CurrentOneOffQuote.Containers.Count);
				AssertEquals("1 one off loose cargo details exist", 1, newQuote.CurrentOneOffQuote.LooseCargo.Count);

				AssertEquals("Loose cargo details correct - ContainerCount", (ZShort)3, newQuote.CurrentOneOffQuote.LooseCargo[0].TPL_PackLineCount);
				AssertEquals("Loose cargo details correct - Height", 2m, newQuote.CurrentOneOffQuote.LooseCargo[0].TPL_Height);
				AssertEquals("Loose cargo details correct - Width", 3m, newQuote.CurrentOneOffQuote.LooseCargo[0].TPL_Width);
				AssertEquals("Loose cargo details correct - Length", 4m, newQuote.CurrentOneOffQuote.LooseCargo[0].TPL_Length);

				AssertEquals("Container details correct - ContainerCount", (ZShort)2, newQuote.CurrentOneOffQuote.Containers[0].TC_ContainerCount);
				AssertEquals("Container details correct - Container Type", GP20.PK, newQuote.CurrentOneOffQuote.Containers[0].TC_RC);

				var newJob = new Job.Loader(newQuote).Load();

				var actualExchangeRates = newJob.ExchangeRates.Select(er => $"{er.JF_BaseRate}|{er.JF_SellRate}|{er.JF_OrgType}|{er.JF_OH_Org}|{er.JF_RX_NKRateCurrency}").ToArray();
				var expectedExchangeRates = new[]
				{
					"0.980000|0.980000|CRD|00000000-0000-0000-0000-000000000000|USD",
					"0.980000|0.980000|DEB|00000000-0000-0000-0000-000000000000|USD"
				};

				AssertContainsExactElementsInAnyOrder(
					"New Job Exchange Rates are created from new charge, not from copying source job exchange rates",
					expectedExchangeRates,
					actualExchangeRates
				);

				AssertEquals(2, newJob.Charges.Count);

				var allCopiedCharges = newJob.Charges.ToArray<Charge>();

				var newCharge = allCopiedCharges.FirstOrDefault(x => x.JR_AC == charge.JR_AC);
				AssertNotNull(nameof(newCharge), newCharge);

				CombineAssertions("Should copy Charges.", () =>
				{
					AssertEquals(newJob.PK, newJob.Charges[0].JR_JH);
					AssertEquals(98m, newCharge.JR_OSSellAmt);
					AssertEquals(100m, newCharge.JR_LocalSellAmt); // calculated from new job exchange rates => 98 / 0.98
					AssertEquals("Retain Cost Override", "Copied from (Quote 00000999/A)", newCharge.JR_CostRatingOverrideComment);
					AssertEquals("Hello Sell from FRT", newCharge.JR_SellRatingOverrideComment);

					AssertEquals("cost tax rate copied", taxRate.PK, newCharge.JR_AT_CostGSTRate);
					AssertEquals("sell tax rate copied", taxRate.PK, newCharge.JR_AT_SellGSTRate);
					AssertEquals("cost tax msg copied", taxMsg.PK, newCharge.JR_A9_CostVATClass);
					AssertEquals("sell tax msg copied", taxMsg.PK, newCharge.JR_A9_SellVATClass);
				});

				CombineAssertions("Should not copy following fields.", () =>
				{
					AssertEquals("JR_APInvoiceNum", ZString.Empty, newCharge.JR_APInvoiceNum);
					AssertEquals("JR_APInvoiceDate", ZDateTime.Empty, newCharge.JR_APInvoiceDate);
					AssertEquals("JR_APDocumentReceivedDate", ZDateTime.Empty, newCharge.JR_APDocumentReceivedDate);
					AssertEquals("JR_PaymentDate", ZDateTime.Empty, newCharge.JR_PaymentDate);
					AssertEquals("JR_PaymentType", ZString.Empty, newCharge.JR_PaymentType);
					AssertEquals("JR_AB", ZGuid.Empty, newCharge.JR_AB);
					AssertEquals("JR_AK", ZGuid.Empty, newCharge.JR_AK);
					AssertEquals("JR_ChequeNo", ZString.Empty, newCharge.JR_ChequeNo);
					AssertEquals("JR_CostReference", ZString.Empty, newCharge.JR_CostReference);

					AssertEquals("JR_CostTaxDate", ZDate.Empty, newCharge.JR_CostTaxDate);
					AssertEquals("JR_SellTaxDate", ZDate.Empty, newCharge.JR_SellTaxDate);

					AssertEquals("JR_CostRated", false, newCharge.JR_CostRated);
					AssertEquals("JR_SellRated", false, newCharge.JR_SellRated);

					AssertEquals("JR_SellRatingOverride", true, newCharge.JR_SellRatingOverride);
					AssertEquals("JR_CostRatingOverride", false, newCharge.JR_CostRatingOverride);
				});

				var newChargeComment = allCopiedCharges.FirstOrDefault(x => x.JR_AC == chargeWithEmptyComment.JR_AC);
				AssertNotNull(nameof(newChargeComment), newChargeComment);

				CombineAssertions("Should display 'Copied from (Quote [QuoteNum])' for comment field if it is empty at source", () =>
				{
					AssertEquals($"Copied from (Quote {originalQuote.TH_QuoteNumber})", newChargeComment.JR_CostRatingOverrideComment);
					AssertEquals($"Copied from (Quote {originalQuote.TH_QuoteNumber})", newChargeComment.JR_SellRatingOverrideComment);
				});
			}
		}

		public void TestCopyConsumedOneOffQuote()
		{
			var originalQuote = Helper.NewQuote(NewClient);
			originalQuote.TH_OneTimeQuote = true;
			originalQuote.TH_IsOneOffQuoteConsumed = true;
			originalQuote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			originalQuote.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");
			Factory.Save();
			var newQuote = (Quote)originalQuote.CopyIncludingChildren();

			AssertEquals("Client PK", ZGuid.Empty, newQuote.TH_OH);

			AssertEquals("AIR Collection Count", originalQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR).Count, newQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR).Count);
			AssertEquals("LCL Collection Count", originalQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.LCL).Count, newQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.LCL).Count);

			Assert("New quote is a one off quote", newQuote.TH_OneTimeQuote);
			AssertEquals("New quote is NOT marked as used", false, newQuote.TH_IsOneOffQuoteConsumed);
		}

		public void TestCopyWithSignatures_CurrentUser()
		{
			var org = Helper.NewOrgHeader();
			var rep1 = Factory.New<GlbStaff>();
			rep1.GS_Code = "TR";
			var rep2 = Factory.New<GlbStaff>();
			rep2.GS_Code = "TQ";

			var originalQuote = Helper.NewQuote(org);
			originalQuote.TH_GS_NKFirstSignatory = rep1.GS_Code;
			originalQuote.TH_GS_NKSecondSignatory = rep2.GS_Code;

			originalQuote.SameClientCopy = true;
			var newQuote = (Quote)((ITemplateCopyable)originalQuote).TemplateCopy();
			AssertEquals(GlbStaff.CurrentUser.GS_Code, newQuote.TH_GS_NKFirstSignatory);
			AssertEquals(ZString.Empty, newQuote.TH_GS_NKSecondSignatory);

			originalQuote.SameClientCopy = false;
			newQuote = (Quote)((ITemplateCopyable)originalQuote).TemplateCopy();
			AssertEquals(GlbStaff.CurrentUser.GS_Code, newQuote.TH_GS_NKFirstSignatory);
			AssertEquals(ZString.Empty, newQuote.TH_GS_NKSecondSignatory);
		}

		public void TestCopyWithSignatures_OverallRep()
		{
			var org = Helper.NewOrgHeader();
			var rep1 = Factory.New<GlbStaff>();
			rep1.GS_Code = "TR";
			var rep2 = Factory.New<GlbStaff>();
			rep2.GS_Code = "TQ";

			var originalQuote = Helper.NewQuote(org);
			originalQuote.TH_GS_NKFirstSignatory = rep1.GS_Code;
			originalQuote.TH_GS_NKSecondSignatory = rep2.GS_Code;

			org.StaffAssignments.OverallSalesRep = rep1.GS_Code;
			originalQuote.SameClientCopy = true;
			var newQuote = (Quote)((ITemplateCopyable)originalQuote).TemplateCopy();
			AssertEquals(rep1.GS_Code, newQuote.TH_GS_NKFirstSignatory);
			AssertEquals(ZString.Empty, newQuote.TH_GS_NKSecondSignatory);

			originalQuote.SameClientCopy = false;
			newQuote = (Quote)((ITemplateCopyable)originalQuote).TemplateCopy();
			AssertEquals(GlbStaff.CurrentUser.GS_Code, newQuote.TH_GS_NKFirstSignatory);
			AssertEquals(ZString.Empty, newQuote.TH_GS_NKSecondSignatory);
		}

		public void TestCopyApplyDefaultFollowUpDate()
		{
			var originalQuote = Helper.NewQuote(Helper.NewOrgHeader());
			originalQuote.TH_FollowUpDate = ZDateTime.Today;
			var newQuote = (Quote)((ITemplateCopyable)originalQuote).TemplateCopy();
			var followUpDays = RatingDataRegistry.Instance.QuoteFollowUpDays.Value;
			var expectedFollowUpDays = ZDateTime.Today.AddDays(followUpDays);
			AssertEquals(expectedFollowUpDays, newQuote.TH_FollowUpDate);
		}

		public void TestGivenTheRegistryQuotationFollowUpDaysIsSetToZero_WhenCopyOneExistingQuotationWithTH_FollowUpDate_ThenTH_FollowUpDateOfNewQuotationShouldBeEmpty()
		{
			var originalQuote = Helper.NewQuote(Helper.NewOrgHeader());
			originalQuote.TH_FollowUpDate = ZDateTime.Today;
			using (RatingDataRegistry.Instance.QuoteFollowUpDays.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var newQuote = (Quote)((ITemplateCopyable)originalQuote).TemplateCopy();
				AssertEquals(ZDateTime.Empty, newQuote.TH_FollowUpDate);
			}
		}

		public void TestTemplateCopy_CreatesSalesRelation()
		{
			var org = Helper.NewOrgHeader();
			var originalQuote = Helper.NewQuote(org);
			Factory.Save();

			originalQuote.SameClientCopy = true;
			var newSameClientQuote = (Quote)((ITemplateCopyable)originalQuote).TemplateCopy();
			AssertCollectionContains(originalQuote, newSameClientQuote.RelatedParentActivityPivotCollection.Activities);

			originalQuote.SameClientCopy = false;
			var newDifferentClientQuote = (Quote)((ITemplateCopyable)originalQuote).TemplateCopy();
			AssertCollectionNotContains(originalQuote, newDifferentClientQuote.RelatedParentActivityPivotCollection.Activities);
		}

		public void TestCopyOneOffQuote_ShouldRecalculateChargesCostAndSellExchangeRates()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			var rateFactory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(rateFactory);
			var currencyUSD = Helper.Currencies["USD"];
			creator.CreateExchangeRate(currencyUSD, Constants.ExchangeRateTypes.Code.BuyRate, 100m, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			creator.CreateExchangeRate(currencyUSD, Constants.ExchangeRateTypes.Code.SellRate, 101m, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			rateFactory.Save();

			NewClient.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 8m, 5m);
			Factory.Save();

			var originalQuote = Helper.NewQuote(NewClient);
			originalQuote.TH_OneTimeQuote = true;
			originalQuote.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Air;
			originalQuote.CurrentOneOffQuote.TT_ContainerMode = Constants.RateMode.LSE;
			originalQuote.CurrentOneOffQuote.TT_OH_Carrier = TransportProvider1.PK;
			originalQuote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");

			var originalJob = new Job.Loader(originalQuote).TryCreate();
			var charge = originalJob.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_OH_SellAccount = NewClient.PK;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OSSellAmt = 10000;
			charge.JR_OH_CostAccount = TransportProvider1.PK;
			charge.JR_RX_NKCostCurrency = "USD";
			charge.JR_OSCostAmt = 5000;

			var originalBillingExchangeRates = originalJob.ExchangeRates.OfType<ExchangeRate>();
			var expectedOriginalRates = new[]
			{
				new
				{
					JF_OrgType = (ZString)"DEB",
					JF_BaseRate = 100m,
					JF_CFXPercent = 8m,
					JF_CFXMinimum = 5m,
					JF_OH_Org = NewClient.PK,
				},
				new
				{
					JF_OrgType = (ZString)"CRD",
					JF_BaseRate = 100m,
					JF_CFXPercent = 0m,
					JF_CFXMinimum = 0m,
					JF_OH_Org = TransportProvider1.PK,
				}
			};

			var actualOriginalRates = originalBillingExchangeRates
				.Select(x => $"{x.JF_OrgType}|{x.JF_BaseRate:0.2}|{x.JF_CFXPercent}|{x.JF_CFXMinimum}|{x.JF_OH_Org}")
				.ToArray();

			var expectedOriginalRatesAsStrings = expectedOriginalRates
				.Select(x => $"{x.JF_OrgType}|{x.JF_BaseRate:0.2}|{x.JF_CFXPercent}|{x.JF_CFXMinimum}|{x.JF_OH_Org}")
				.ToArray();

			AssertContainsExactElementsInAnyOrder("The original billing exchange rates should match the expected rates.", expectedOriginalRatesAsStrings, actualOriginalRates);

			AssertEquals("The cost exchange rate should be 100m.", 100m, charge.JR_OSCostExRate);
			AssertEquals("The sell exchange rate should be 92m (100 less 8%).", 92m, charge.JR_OSSellExRate);

			originalBillingExchangeRates.ForEach(x =>
			{
				x.JF_BaseRate = 1;
				x.JF_CFXPercent = 7;
				x.JF_CFXMinimum = 2;
			});

			var exRateARConfig = NewClient.CompanyData.AccARExchangeRateConfigurations.AddNew();
			exRateARConfig.JCE_JobType = "ALL";
			exRateARConfig.JCE_ServiceDirection = "ALL";
			exRateARConfig.JCE_TransportMode = "ALL";
			exRateARConfig.GetCurrencyConfig(ZString.Empty, ZDate.Empty).JCT_ExRateType = Constants.ExchangeRateTypes.Code.SellRate;

			var exRateAPConfig = TransportProvider1.CompanyData.AccAPExchangeRateConfigurations.AddNew();
			exRateAPConfig.JCE_JobType = "ALL";
			exRateAPConfig.JCE_ServiceDirection = "ALL";
			exRateAPConfig.JCE_TransportMode = "ALL";
			exRateAPConfig.GetCurrencyConfig(ZString.Empty, ZDate.Empty).JCT_ExRateType = Constants.ExchangeRateTypes.Code.SellRate;

			NewClient.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 10m, 4m);

			var newQuote = (Quote)originalQuote.CopyIncludingChildren();
			var newJob = new Job.Loader(newQuote).Load();
			var newExpectedRates = new[]
			{
				new
				{
					JF_OrgType = (ZString)"DEB",
					JF_BaseRate = 101m,
					JF_CFXPercent = 10m,
					JF_CFXMinimum = 4m,
					JF_OH_Org = NewClient.PK
				},
				new
				{
					JF_OrgType = (ZString)"CRD",
					JF_BaseRate = 101m,
					JF_CFXPercent = 0m,
					JF_CFXMinimum = 0m,
					JF_OH_Org = TransportProvider1.PK
				}
			};

			var actualNewRates = newJob.ExchangeRates.OfType<ExchangeRate>()
				.Select(x => $"{x.JF_OrgType}|{x.JF_BaseRate:0.2}|{x.JF_CFXPercent}|{x.JF_CFXMinimum}|{x.JF_OH_Org}")
				.ToArray();

			var expectedNewRatesAsStrings = newExpectedRates
				.Select(x => $"{x.JF_OrgType}|{x.JF_BaseRate:0.2}|{x.JF_CFXPercent}|{x.JF_CFXMinimum}|{x.JF_OH_Org}")
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				"New billing exchange rate should be created with new values and from organization configurations rather than being copied from source.",
				expectedNewRatesAsStrings,
				actualNewRates
			);

			var newCharge = (Charge)newJob.Charges.Single();
			AssertEquals("The new cost exchange rate should be 101m.", 101m, newCharge.JR_OSCostExRate);
			AssertEquals("The new sell exchange rate should be 90.9m (101 less 10%).", 90.9m, newCharge.JR_OSSellExRate);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestCopyOneOffQuote_ShouldRecalculateChargesCostAndSellExchangeRates_ExchangeRateIsZero()
		{
			var originalQuote = Helper.NewQuote(NewClient);
			originalQuote.TH_OneTimeQuote = true;
			originalQuote.CurrentOneOffQuote.TT_TransportMode = Core.Constants.RateMode.AIR;
			originalQuote.CurrentOneOffQuote.TT_OH_Carrier = TransportProvider1.PK;
			originalQuote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");

			var originalJob = new Job.Loader(originalQuote).TryCreate();
			var charge = originalJob.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_OH_SellAccount = NewClient.PK;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OSSellAmt = 10000;
			charge.JR_OH_CostAccount = TransportProvider1.PK;
			charge.JR_RX_NKCostCurrency = "USD";
			charge.JR_OSCostAmt = 5000;

			var originalBillingExchangeRates = originalJob.ExchangeRates.OfType<ExchangeRate>();
			var expectedOriginalRates = new[]
			{
				new { JF_OrgType = (ZString)"DEB", JF_BaseRate = 0m, JF_CFXPercent = 0m, JF_CFXMinimum = 0m, JF_OH_Org = NewClient.PK },
				new { JF_OrgType = (ZString)"CRD", JF_BaseRate = 0m, JF_CFXPercent = 0m, JF_CFXMinimum = 0m, JF_OH_Org = TransportProvider1.PK }
			};
			var actualOriginalRates = originalBillingExchangeRates
				.Select(i => $"{i.JF_OrgType}|{i.JF_BaseRate}|{i.JF_CFXPercent}|{i.JF_CFXMinimum}|{i.JF_OH_Org}")
				.ToArray();
			var expectedOriginalRatesSerialized = expectedOriginalRates
				.Select(i => $"{i.JF_OrgType}|{i.JF_BaseRate}|{i.JF_CFXPercent}|{i.JF_CFXMinimum}|{i.JF_OH_Org}")
				.ToArray();
			AssertContainsExactElementsInAnyOrder(
				"Pre-condition: Exchange rates should not be present or should be zero.",
				expectedOriginalRatesSerialized,
				actualOriginalRates
			);

			charge.JR_OSCostExRate = 100;
			charge.JR_OSSellExRate = 100;

			AssertEquals("local sell amount = 10000/100", 100m, charge.JR_LocalSellAmt);
			AssertEquals("local cost amount = 5000/100", 50m, charge.JR_LocalCostAmt);

			var newQuote = (Quote)originalQuote.CopyIncludingChildren();
			var newJob = new Job.Loader(newQuote).Load();
			var newBillingExchangeRates = newJob.ExchangeRates.OfType<ExchangeRate>();
			var actualNewRates = newBillingExchangeRates
				.Select(i => $"{i.JF_OrgType}|{i.JF_BaseRate}|{i.JF_CFXPercent}|{i.JF_CFXMinimum}|{i.JF_OH_Org}")
				.ToArray();
			AssertContainsExactElementsInAnyOrder(
				"Pre-condition: Exchange rates should not be present or should be zero.",
				expectedOriginalRatesSerialized,
				actualNewRates
			);

			var newCharge = (Charge)newJob.Charges.Single();

			AssertEquals(5000m, newCharge.JR_OSCostAmt);
			AssertEquals("exchange rate should be recalculated, since it's not present it should be zero.", 0m, newCharge.JR_OSCostExRate);
			AssertEquals("As there is no exchange rates in system, local cost amount should be zero", 0m, newCharge.JR_LocalCostAmt);

			AssertEquals(10000m, newCharge.JR_OSSellAmt);
			AssertEquals("exchange rate should be recalculated, since it's not present it should be zero.", 0m, newCharge.JR_OSSellExRate);
			AssertEquals("As there is no exchange rates in system, local sell amount should be zero", 0m, newCharge.JR_LocalSellAmt);
		}

		#endregion

		#region Accept Quotes

		public void TestQuoatationAcceptedEventLog()
		{
			var testQuote = Helper.NewQuote(NewClient);
			AssertEquals(0, testQuote.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.QuotationAccepted.Code)).Length);

			ClientRate clientRate;
			testQuote.TryAcceptQuote(out clientRate);
			AssertEquals(1, testQuote.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.QuotationAccepted.Code)).Length);
		}

		public void TestAcceptQuote_RatesExistInOtherCompany()
		{
			var testQuote = Helper.NewQuote(NewClient);
			testQuote.TH_GC = GlbCompany.CurrentCompany.PK;

			var newCompany = Factory.New<GlbCompany>();
			var rate = Helper.NewClientRate(NewClient);
			rate.TH_GC = newCompany.PK;

			var clientRateFilter = new ZQuery();
			clientRateFilter.AddToFilter(RatingHeaderSchema.TH_GC, GlbCompany.CurrentCompany.PK);
			clientRateFilter.AddToFilter(RatingHeaderSchema.TH_OH, NewClient.PK);
			clientRateFilter.AddToFilter(RatingHeaderSchema.TH_RateType, (ZString)RatingConstants.RatingHeaderTypes.ClientRate);

			var clientRates = Factory.Load<ClientRate>(clientRateFilter);
			AssertEquals("No client rates exist in the current company.", 0, clientRates.Length);

			ClientRate clientRate;
			testQuote.TryAcceptQuote(out clientRate);

			clientRates = Factory.Load<ClientRate>(clientRateFilter);
			AssertEquals("Client Rate created in this company for accepted quote", 1, clientRates.Length);
		}

		[TestDate(2015, 02, 10)]
		public void TestAcceptQuote_ExpireExistingRates_AlreadyExpiredDuplicateRatesAreNotUpdated()
		{
			var client = Helper.NewOrgHeader();
			Factory.Save();

			var today = ZDate.Today;

			var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var anotherHelper = new TestHelper(anotherFactory);
			var clientRate = anotherHelper.NewClientRate(client);

			var expiredRateEntry1 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "", "FRT", 15m);
			expiredRateEntry1.TI_RateStartDate = today.AddDays(-20);
			expiredRateEntry1.TI_RateEndDate = today.AddDays(-10);

			var expiredRateEntry2 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "GB", "FRT", 15m);
			expiredRateEntry2.TI_RateStartDate = today.AddDays(-2);
			expiredRateEntry2.TI_RateEndDate = today.AddDays(-1);

			anotherFactory.Save();

			var quote = Helper.NewQuote(client);
			quote.TH_QuoteDate = today;
			quote.TH_QuoteEndDate = today.AddDays(30);
			var quoteEntry1 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "", "FRT", 20m);
			var quoteEntry2 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "GB", "FRT", 20m);

			Assert("Pre-condition", quoteEntry1.IsDuplicateExcludingColumns(expiredRateEntry1, nameof(RateEntry.TI_TH)));
			Assert("Pre-condition", quoteEntry2.IsDuplicateExcludingColumns(expiredRateEntry2, nameof(RateEntry.TI_TH)));

			Assert("Should be able to accept quote", quote.TryAcceptQuote(out clientRate));

			anotherFactory.Save();

			var results = clientRate.AllEntries.ToArray();

			var message = "End date of existing rate entry NOT changed as it was already expired";
			AssertEquals(message, quote.TH_QuoteDate.AddDays(-10), results.First(x => x.PK == expiredRateEntry1.PK).TI_RateEndDate);
			AssertEquals(message, quote.TH_QuoteDate.AddDays(-1), results.First(x => x.PK == expiredRateEntry2.PK).TI_RateEndDate);

			message = "Expected two new client rate entries to have been created from the accepted quoted entries";
			AssertEquals(message, 2, results.Count(x => x.TI_RateEndDate == quote.TH_QuoteEndDate));
		}

		[TestDate(2015, 02, 10)]
		public void TestAcceptQuote_ExpireExistingRates_OverlappingDates()
		{
			var client = Helper.NewOrgHeader();
			Factory.Save();

			var today = ZDate.Today;

			var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var anotherHelper = new TestHelper(anotherFactory);
			var clientRate = anotherHelper.NewClientRate(client);

			var expiredRateEntry1 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "GB", "FRT", 15m);
			expiredRateEntry1.TI_RateStartDate = today.AddDays(-10);
			expiredRateEntry1.TI_RateEndDate = today.AddDays(5);

			var expiredRateEntry2 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "GB", "FRT", 15m);
			expiredRateEntry2.TI_RateStartDate = today.AddDays(6);
			expiredRateEntry2.TI_RateEndDate = today.AddDays(12);

			anotherFactory.Save();

			var quote = Helper.NewQuote(client);
			quote.TH_QuoteDate = today;
			quote.TH_QuoteEndDate = today.AddDays(10);
			var quoteEntry1 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "GB", "FRT", 20m);

			Assert("Pre-condition", quoteEntry1.IsDuplicateExcludingColumns(expiredRateEntry1, nameof(RateEntry.TI_TH)));

			Assert("Should be able to accept quote", quote.TryAcceptQuote(out clientRate));

			anotherFactory.Save();

			var results = clientRate.AllEntries.ToArray();

			var message = "End date of existing rate entry changed to quote date minues 1";
			AssertEquals(message, quote.TH_QuoteDate.AddDays(-1), results.First(x => x.PK == expiredRateEntry1.PK).TI_RateEndDate);
			AssertEquals(message, quote.TH_QuoteEndDate.AddDays(1), results.First(x => x.PK == expiredRateEntry2.PK).TI_RateStartDate);
		}

		[TestDate(2015, 02, 10)]
		public void TestGivenNewRatesHasOverlappingDateRangeWithExistingRate_WhenAcceptQuote_ThenRateLinesShouldBeUpdatedToo()
		{
			var client = Helper.NewOrgHeader();
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var anotherHelper = new TestHelper(anotherFactory);
			var clientRate = anotherHelper.NewClientRate(client);

			var expiredRateEntry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "GB");
			expiredRateEntry1.TI_RateStartDate = new ZDate(2014, 08, 1);
			expiredRateEntry1.TI_RateEndDate = new ZDate(2014, 08, 30);

			var rateLine1 = expiredRateEntry1.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			rateLine1.TL_RateStartDate = new ZDate(2014, 08, 1);
			rateLine1.TL_RateEndDate = new ZDate(2014, 08, 10);
			var rateLine2 = expiredRateEntry1.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			rateLine2.TL_RateStartDate = new ZDate(2014, 08, 10);
			rateLine2.TL_RateEndDate = new ZDate(2014, 08, 20);
			var rateLine3 = expiredRateEntry1.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			rateLine3.TL_RateStartDate = new ZDate(2014, 08, 20);
			rateLine3.TL_RateEndDate = new ZDate(2014, 08, 30);
			var rateLine4 = expiredRateEntry1.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			rateLine4.TL_RateEndDate = new ZDate(2014, 08, 30);
			var rateLine5 = expiredRateEntry1.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");

			var expiredRateEntry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "GB");
			expiredRateEntry2.TI_RateStartDate = new ZDate(2014, 09, 1);
			expiredRateEntry2.TI_RateEndDate = new ZDate(2014, 09, 30);

			var rateLine6 = expiredRateEntry2.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			rateLine6.TL_RateStartDate = new ZDate(2014, 09, 1);
			rateLine6.TL_RateEndDate = new ZDate(2014, 09, 10);
			var rateLine7 = expiredRateEntry2.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			rateLine7.TL_RateStartDate = new ZDate(2014, 09, 10);
			rateLine7.TL_RateEndDate = new ZDate(2014, 09, 20);
			var rateLine8 = expiredRateEntry2.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			rateLine8.TL_RateStartDate = new ZDate(2014, 09, 20);
			rateLine8.TL_RateEndDate = new ZDate(2014, 09, 30);
			var rateLine9 = expiredRateEntry2.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			rateLine9.TL_RateStartDate = new ZDate(2014, 09, 1);
			var rateLine10 = expiredRateEntry2.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");

			anotherFactory.Save();

			var quote = Helper.NewQuote(client);
			quote.TH_QuoteDate = new ZDate(2014, 08, 15);
			quote.TH_QuoteEndDate = new ZDate(2014, 09, 15);
			var quoteEntry1 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "GB", "FRT", 20m);

			Assert("Pre-condition", quoteEntry1.IsDuplicateExcludingColumns(expiredRateEntry1, nameof(RateEntry.TI_TH)));

			Assert("Should be able to accept quote", quote.TryAcceptQuote(out clientRate));

			anotherFactory.Save();

			var results = clientRate.AllEntries.ToArray();

			var acceptedQuoteRateEntry1 = results.First(x => x.PK == expiredRateEntry1.PK);
			var message = "End date of existing rate entry changed to quote date minus 1";

			AssertEquals(message, new ZDate(2014, 08, 1), acceptedQuoteRateEntry1.TI_RateStartDate);
			AssertEquals(message, new ZDate(2014, 08, 14), acceptedQuoteRateEntry1.TI_RateEndDate);

			var acceptedQuoteRateEntry1RateLines = acceptedQuoteRateEntry1.RateLines.ToArray<RateLine>();
			message = "Rate lines end date changed to quote date minus 1 when there is overlapping, otherwise it will not be changed";

			AssertEquals(message, new ZDate(2014, 08, 1), acceptedQuoteRateEntry1RateLines.First(x => x.PK == rateLine1.PK).TL_RateStartDate);
			AssertEquals(message, new ZDate(2014, 08, 10), acceptedQuoteRateEntry1RateLines.First(x => x.PK == rateLine1.PK).TL_RateEndDate);

			AssertEquals(message, new ZDate(2014, 08, 10), acceptedQuoteRateEntry1RateLines.First(x => x.PK == rateLine2.PK).TL_RateStartDate);
			AssertEquals(message, new ZDate(2014, 08, 14), acceptedQuoteRateEntry1RateLines.First(x => x.PK == rateLine2.PK).TL_RateEndDate);

			AssertEquals(message, new ZDate(2014, 08, 20), acceptedQuoteRateEntry1RateLines.First(x => x.PK == rateLine3.PK).TL_RateStartDate);
			AssertEquals(message, new ZDate(2014, 08, 30), acceptedQuoteRateEntry1RateLines.First(x => x.PK == rateLine3.PK).TL_RateEndDate);

			AssertEquals(message, ZDate.Empty, acceptedQuoteRateEntry1RateLines.First(x => x.PK == rateLine4.PK).TL_RateStartDate);
			AssertEquals(message, new ZDate(2014, 08, 14), acceptedQuoteRateEntry1RateLines.First(x => x.PK == rateLine4.PK).TL_RateEndDate);

			AssertEquals(message, ZDate.Empty, acceptedQuoteRateEntry1RateLines.First(x => x.PK == rateLine5.PK).TL_RateStartDate);
			AssertEquals(message, ZDate.Empty, acceptedQuoteRateEntry1RateLines.First(x => x.PK == rateLine5.PK).TL_RateEndDate);

			var acceptedQuoteRateEntry2 = results.First(x => x.PK == expiredRateEntry2.PK);
			message = "Start date of existing rate entry changed to quote end date plus 1";

			AssertEquals(message, new ZDate(2014, 09, 16), acceptedQuoteRateEntry2.TI_RateStartDate);
			AssertEquals(message, new ZDate(2014, 09, 30), acceptedQuoteRateEntry2.TI_RateEndDate);

			var acceptedQuoteRateEntry2RateLines = acceptedQuoteRateEntry2.RateLines.ToArray<RateLine>();
			message = "Rate lines start date changed to quote date plus 1 when there is overlapping, otherwise it will not be changed";

			AssertEquals(message, new ZDate(2014, 09, 1), acceptedQuoteRateEntry2RateLines.First(x => x.PK == rateLine6.PK).TL_RateStartDate);
			AssertEquals(message, new ZDate(2014, 09, 10), acceptedQuoteRateEntry2RateLines.First(x => x.PK == rateLine6.PK).TL_RateEndDate);

			AssertEquals(message, new ZDate(2014, 09, 16), acceptedQuoteRateEntry2RateLines.First(x => x.PK == rateLine7.PK).TL_RateStartDate);
			AssertEquals(message, new ZDate(2014, 09, 20), acceptedQuoteRateEntry2RateLines.First(x => x.PK == rateLine7.PK).TL_RateEndDate);

			AssertEquals(message, new ZDate(2014, 09, 20), acceptedQuoteRateEntry2RateLines.First(x => x.PK == rateLine8.PK).TL_RateStartDate);
			AssertEquals(message, new ZDate(2014, 09, 30), acceptedQuoteRateEntry2RateLines.First(x => x.PK == rateLine8.PK).TL_RateEndDate);

			AssertEquals(message, new ZDate(2014, 09, 16), acceptedQuoteRateEntry2RateLines.First(x => x.PK == rateLine9.PK).TL_RateStartDate);
			AssertEquals(message, ZDate.Empty, acceptedQuoteRateEntry2RateLines.First(x => x.PK == rateLine9.PK).TL_RateEndDate);

			AssertEquals(message, ZDate.Empty, acceptedQuoteRateEntry2RateLines.First(x => x.PK == rateLine10.PK).TL_RateStartDate);
			AssertEquals(message, ZDate.Empty, acceptedQuoteRateEntry2RateLines.First(x => x.PK == rateLine10.PK).TL_RateEndDate);
		}

		[TestDate(2015, 02, 10)]
		public void TestGivenNewRatesWithinExistingRateDateRange_WhenAcceptQuote_ThenRateLinesShouldBeUpdatedToo()
		{
			var client = Helper.NewOrgHeader();
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var anotherHelper = new TestHelper(anotherFactory);
			var clientRate = anotherHelper.NewClientRate(client);

			var expiredRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "GB");
			expiredRateEntry.TI_RateStartDate = new ZDate(2014, 08, 1);
			expiredRateEntry.TI_RateEndDate = new ZDate(2014, 08, 30);

			var rateLine1 = expiredRateEntry.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			rateLine1.TL_RateStartDate = new ZDate(2014, 08, 1);
			rateLine1.TL_RateEndDate = new ZDate(2014, 08, 10);
			var rateLine2 = expiredRateEntry.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			rateLine2.TL_RateStartDate = new ZDate(2014, 08, 10);
			rateLine2.TL_RateEndDate = new ZDate(2014, 08, 20);
			var rateLine3 = expiredRateEntry.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			rateLine3.TL_RateStartDate = new ZDate(2014, 08, 20);
			rateLine3.TL_RateEndDate = new ZDate(2014, 08, 30);
			var rateLine4 = expiredRateEntry.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			rateLine4.TL_RateEndDate = new ZDate(2014, 08, 30);
			var rateLine5 = expiredRateEntry.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");

			anotherFactory.Save();

			var quote = Helper.NewQuote(client);
			quote.TH_QuoteDate = new ZDate(2014, 08, 15);
			quote.TH_QuoteEndDate = new ZDate(2014, 08, 18);
			var quoteEntry1 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "GB", "FRT", 20m);

			Assert("Pre-condition", quoteEntry1.IsDuplicateExcludingColumns(expiredRateEntry, nameof(RateEntry.TI_TH)));

			Assert("Should be able to accept quote", quote.TryAcceptQuote(out clientRate));

			anotherFactory.Save();

			var results = clientRate.AllEntries.ToArray();

			var acceptedQuoteRateEntry = results.First(x => x.PK == expiredRateEntry.PK);
			var message = "End date of existing rate entry changed to quote date minus 1";

			AssertEquals(message, new ZDate(2014, 08, 1), acceptedQuoteRateEntry.TI_RateStartDate);
			AssertEquals(message, new ZDate(2014, 08, 14), acceptedQuoteRateEntry.TI_RateEndDate);

			var acceptedQuoteRateEntry1RateLines = acceptedQuoteRateEntry.RateLines.ToArray<RateLine>();
			message = "Rate lines end date changed to quote date minus 1 when there is overlapping, otherwise it will not be changed";

			AssertEquals(message, new ZDate(2014, 08, 1), acceptedQuoteRateEntry1RateLines.First(x => x.PK == rateLine1.PK).TL_RateStartDate);
			AssertEquals(message, new ZDate(2014, 08, 10), acceptedQuoteRateEntry1RateLines.First(x => x.PK == rateLine1.PK).TL_RateEndDate);

			AssertEquals(message, new ZDate(2014, 08, 10), acceptedQuoteRateEntry1RateLines.First(x => x.PK == rateLine2.PK).TL_RateStartDate);
			AssertEquals(message, new ZDate(2014, 08, 14), acceptedQuoteRateEntry1RateLines.First(x => x.PK == rateLine2.PK).TL_RateEndDate);

			AssertEquals(message, new ZDate(2014, 08, 20), acceptedQuoteRateEntry1RateLines.First(x => x.PK == rateLine3.PK).TL_RateStartDate);
			AssertEquals(message, new ZDate(2014, 08, 30), acceptedQuoteRateEntry1RateLines.First(x => x.PK == rateLine3.PK).TL_RateEndDate);

			AssertEquals(message, ZDate.Empty, acceptedQuoteRateEntry1RateLines.First(x => x.PK == rateLine4.PK).TL_RateStartDate);
			AssertEquals(message, new ZDate(2014, 08, 14), acceptedQuoteRateEntry1RateLines.First(x => x.PK == rateLine4.PK).TL_RateEndDate);

			AssertEquals(message, ZDate.Empty, acceptedQuoteRateEntry1RateLines.First(x => x.PK == rateLine5.PK).TL_RateStartDate);
			AssertEquals(message, ZDate.Empty, acceptedQuoteRateEntry1RateLines.First(x => x.PK == rateLine5.PK).TL_RateEndDate);
		}

		public void TestAcceptQuote_ExpireExistingRates_DifferentMatchContainerRateClass()
		{
			// two rate lines with same Container Type but different value of TI_MatchContainerRateClass
			// in result, nothing will expire
			var client = Helper.NewOrgHeader();
			Factory.Save();

			var today = ZDate.Today;

			var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var anotherHelper = new TestHelper(anotherFactory);

			var clientRate = anotherHelper.NewClientRate(client);
			anotherFactory.Save();

			var quote = Helper.NewQuote(client);
			quote.TH_QuoteDate = today;
			quote.TH_QuoteEndDate = today.AddDays(60);

			var quoteEntryNonDuplicate1 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST,
				Core.Constants.RateMode.FCL, "AU", "JP", "DDOC", 50m, container: "20GP");
			quoteEntryNonDuplicate1.TI_MatchContainerRateClass = false;

			var quoteEntryNonDuplicate2 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST,
				Core.Constants.RateMode.FCL, "AU", "JP", "DDOC", 50m, container: "20GP");
			quoteEntryNonDuplicate2.TI_MatchContainerRateClass = true;

			Assert("Should be able to accept quote", quote.TryAcceptQuote(out clientRate));

			var results = clientRate.AllEntries.ToArray();

			var message = "Expected two rate lines with same Container Type but different value of TI_MatchContainerRateClass";
			AssertEquals(message, 2, results.Length);

			message = "Expected the accepted quoted entries to have original start and end dates and not expired";
			AssertEquals(message, 2, clientRate.AllEntries.Count(x => today == x.TI_RateStartDate));
			AssertEquals(message, 2, clientRate.AllEntries.Count(x => today.AddDays(60) == x.TI_RateEndDate));
		}

		public void TestAcceptQuote_ExpireExistingRates_SameContainerClass()
		{
			// two rate lines with both true for TI_MatchContainerRateClass with same Container Class
			// in result, one should expire
			var client = Helper.NewOrgHeader();
			Factory.Save();

			var today = ZDate.Today;

			var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var anotherHelper = new TestHelper(anotherFactory);

			var clientRate = anotherHelper.NewClientRate(client);
			anotherFactory.Save();

			var quote = Helper.NewQuote(client);
			quote = Helper.NewQuote(client);
			quote.TH_QuoteDate = today;
			quote.TH_QuoteEndDate = today.AddDays(60);

			var quoteEntryNonDuplicate1 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST,
				Core.Constants.RateMode.FCL, "AU", "JP", "DDOC", 50m, container: "20GP");
			quoteEntryNonDuplicate1.TI_MatchContainerRateClass = true;

			var quoteEntryNonDuplicate2 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST,
				Core.Constants.RateMode.FCL, "AU", "JP", "DDOC", 50m, container: "40GP");
			quoteEntryNonDuplicate2.TI_MatchContainerRateClass = true;

			Assert("Should be able to accept quote", quote.TryAcceptQuote(out clientRate));

			var results = clientRate.AllEntries.ToArray();

			var message = "Expected two rate lines with both true for TI_MatchContainerRateClass with same Container Class";
			AssertEquals(message, 2, results.Length);

			message = "Expected one entry to expired";
			AssertEquals(message, 1, clientRate.AllEntries.Count(x => today == x.TI_RateStartDate));
			AssertEquals(message, 1, clientRate.AllEntries.Count(x => today.AddDays(60) == x.TI_RateEndDate));
		}

		public void TestAcceptQuote_ExpireExistingRates_SameContainerType()
		{
			// two rate lines with both false for TI_MatchContainerRateClass with same Container Type
			// in result, one should expire
			var client = Helper.NewOrgHeader();
			Factory.Save();

			var today = ZDate.Today;

			var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var anotherHelper = new TestHelper(anotherFactory);

			var clientRate = anotherHelper.NewClientRate(client);
			anotherFactory.Save();

			var quote = Helper.NewQuote(client);
			quote.TH_QuoteDate = today;
			quote.TH_QuoteEndDate = today.AddDays(60);

			var quoteEntryNonDuplicate1 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST,
				Core.Constants.RateMode.FCL, "AU", "JP", "DDOC", 50m, container: "20GP");
			quoteEntryNonDuplicate1.TI_MatchContainerRateClass = false;

			var quoteEntryNonDuplicate2 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST,
				Core.Constants.RateMode.FCL, "AU", "JP", "DDOC", 50m, container: "20GP");
			quoteEntryNonDuplicate2.TI_MatchContainerRateClass = false;

			Assert("Should be able to accept quote", quote.TryAcceptQuote(out clientRate));

			var results = clientRate.AllEntries.ToArray();

			var message = "Expected two rate lines with both false for TI_MatchContainerRateClass with same Container Type";
			AssertEquals(message, 2, results.Length);

			message = "Expected one entry to expired";
			AssertEquals(message, 1, clientRate.AllEntries.Count(x => today == x.TI_RateStartDate));
			AssertEquals(message, 1, clientRate.AllEntries.Count(x => today.AddDays(60) == x.TI_RateEndDate));
		}

		public void TestAcceptQuote_ExpireExistingRates_DifferentContainerType()
		{
			// two rate lines with both false for TI_MatchContainerRateClass with different Container Types
			// in result, nothing will expire
			var client = Helper.NewOrgHeader();
			Factory.Save();

			var today = ZDate.Today;

			var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var anotherHelper = new TestHelper(anotherFactory);

			var clientRate = anotherHelper.NewClientRate(client);
			anotherFactory.Save();

			var quote = Helper.NewQuote(client);
			quote.TH_QuoteDate = today;
			quote.TH_QuoteEndDate = today.AddDays(60);

			var quoteEntryNonDuplicate1 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST,
				Core.Constants.RateMode.FCL, "AU", "JP", "DDOC", 50m, container: "20GP");
			quoteEntryNonDuplicate1.TI_MatchContainerRateClass = false;

			var quoteEntryNonDuplicate2 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST,
				Core.Constants.RateMode.FCL, "AU", "JP", "DDOC", 50m, container: "40GP");
			quoteEntryNonDuplicate2.TI_MatchContainerRateClass = false;

			Assert("Should be able to accept quote", quote.TryAcceptQuote(out clientRate));

			var results = clientRate.AllEntries.ToArray();

			var message = "Expected two rate lines with both false for TI_MatchContainerRateClass with different Container Types";
			AssertEquals(message, 2, results.Length);

			message = "Expected the accepted quoted entries to have original start and end dates and not expired";
			AssertEquals(message, 2, clientRate.AllEntries.Count(x => today == x.TI_RateStartDate));
			AssertEquals(message, 2, clientRate.AllEntries.Count(x => today.AddDays(60) == x.TI_RateEndDate));
		}

		[TestDate(2015, 02, 10)]
		public void TestAcceptQuote_ExpireExistingRates_ExpiresDuplicates()
		{
			var client = Helper.NewOrgHeader();
			Factory.Save();

			var today = ZDate.Today;

			var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var anotherHelper = new TestHelper(anotherFactory);
			var clientRate = anotherHelper.NewClientRate(client);

			var rateEntry1 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "", "FRT", 1m);
			rateEntry1.TI_RateStartDate = today.AddDays(-30);

			var rateEntry2 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "CN", "FRT", 2m);
			rateEntry2.TI_RateStartDate = today.AddDays(-30);

			var rateEntry3 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "", "CN", "FRT", 3m);
			rateEntry3.TI_RateStartDate = today.AddDays(-30);

			var rateEntry4 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Core.Constants.RateMode.LSE, "AU", "", "DDOC", 4m);
			rateEntry4.TI_RateStartDate = today.AddDays(-30);
			rateEntry4.TI_RateEndDate = today.AddDays(30);

			var rateEntry5 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Core.Constants.RateMode.LSE, "AU", "CN", "DDOC", 5m);
			rateEntry5.TI_RateStartDate = today.AddDays(-30);
			rateEntry5.TI_RateEndDate = today.AddDays(30);

			var nonDuplicateRateEntry1 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "", "FRT", 6m);
			nonDuplicateRateEntry1.TI_RateStartDate = today.AddDays(-30);
			nonDuplicateRateEntry1.TI_RateEndDate = ZDate.Empty;

			var nonDuplicateRateEntry2 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Core.Constants.RateMode.LSE, "", "CN", "DDOC", 7m);
			nonDuplicateRateEntry2.TI_RateStartDate = today.AddDays(-30);
			nonDuplicateRateEntry2.TI_RateEndDate = today.AddDays(30);

			anotherFactory.Save();

			var quote = Helper.NewQuote(client);
			quote.TH_QuoteDate = today;
			quote.TH_QuoteEndDate = today.AddDays(60);

			var quoteEntry1 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "", "FRT", 10m);
			var quoteEntry2 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "CN", "FRT", 20m);
			var quoteEntry3 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "", "CN", "FRT", 30m);
			var quoteEntry4 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Core.Constants.RateMode.LSE, "AU", "", "DDOC", 40m);
			var quoteEntry5 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Core.Constants.RateMode.LSE, "AU", "CN", "DDOC", 50m);

			var message = "Pre-condition: only client rate entries that are duplicates of accepted quote entries will be expired";
			Assert(message, quoteEntry1.IsDuplicateExcludingColumns(rateEntry1, nameof(RateEntry.TI_TH)));
			Assert(message, quoteEntry2.IsDuplicateExcludingColumns(rateEntry2, nameof(RateEntry.TI_TH)));
			Assert(message, quoteEntry3.IsDuplicateExcludingColumns(rateEntry3, nameof(RateEntry.TI_TH)));
			Assert(message, quoteEntry4.IsDuplicateExcludingColumns(rateEntry4, nameof(RateEntry.TI_TH)));
			Assert(message, quoteEntry5.IsDuplicateExcludingColumns(rateEntry5, nameof(RateEntry.TI_TH)));

			Assert("Should be able to accept quote", quote.TryAcceptQuote(out clientRate));

			anotherFactory.Save();

			var results = clientRate.AllEntries.ToArray();
			message = "Expected the duplicate existing client rate to have been expired";
			AssertEquals(message, quote.TH_QuoteDate.AddDays(-1), results.First(x => x.PK == rateEntry1.PK).TI_RateEndDate);
			AssertEquals(message, quote.TH_QuoteDate.AddDays(-1), results.First(x => x.PK == rateEntry2.PK).TI_RateEndDate);
			AssertEquals(message, quote.TH_QuoteDate.AddDays(-1), results.First(x => x.PK == rateEntry3.PK).TI_RateEndDate);
			AssertEquals(message, quote.TH_QuoteDate.AddDays(-1), results.First(x => x.PK == rateEntry4.PK).TI_RateEndDate);

			message = "Expected non-duplicate existing client rate entries to remain unchanged";
			AssertEquals(message, ZDate.Empty, results.First(x => x.PK == nonDuplicateRateEntry1.PK).TI_RateEndDate);
			AssertEquals(message, today.AddDays(30), results.First(x => x.PK == nonDuplicateRateEntry2.PK).TI_RateEndDate);

			message = "Expected the accepted quoted entries to create new client rate entries that expire on the original quote's end date";
			AssertEquals(message, 5, results.Count(x => x.TI_RateEndDate == quote.TH_QuoteEndDate));
		}

		public void TestAcceptQuote_NoRatesExist()
		{
			var testQuote = Helper.NewQuote(NewClient);
			testQuote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").AddRateLine("WAR", FlatCalculator.Code);
			testQuote.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX").AddRateLine("WAR", FlatCalculator.Code);

			AssertEquals("Is Locked", false, testQuote.TH_IsLocked);
			AssertEquals("Accepted Date", ZDateTime.Empty, testQuote.TH_Accepted);
			ClientRate acceptedRate;
			testQuote.TryAcceptQuote(out acceptedRate);
			var airRateEntries = acceptedRate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR);
			var lclRateEntries = acceptedRate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.LCL);

			AssertEquals("Is Locked", true, testQuote.TH_IsLocked);
			AssertEquals("Accepted Date", ZDateTime.Today, testQuote.TH_Accepted);

			AssertEquals("Air Rate Entries", 1, airRateEntries.Count);
			AssertEquals("LCL Rate Entries", 1, lclRateEntries.Count);

			AssertEquals("AIR 0 Rate Lines", 2, airRateEntries[0].RateLines.Count);
			AssertEquals("LCL 0 Rate Lines", 2, lclRateEntries[0].RateLines.Count);
		}

		public void TestAcceptQuote_ClearSignatories()
		{
			var testQuote = Helper.NewQuote(NewClient);
			testQuote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").AddRateLine("WAR", FlatCalculator.Code);
			testQuote.TH_GS_NKFirstSignatory = "A";
			testQuote.TH_GS_NKSecondSignatory = "B";

			ClientRate acceptedRate;
			testQuote.TryAcceptQuote(out acceptedRate);
			AssertEquals("Signatories cleared as these should only be used for Quotes.", "", acceptedRate.TH_GS_NKFirstSignatory);
			AssertEquals("Signatories cleared as these should only be used for Quotes.", "", acceptedRate.TH_GS_NKSecondSignatory);

			AssertEquals("Signatories remain on the quote.", "A", testQuote.TH_GS_NKFirstSignatory);
			AssertEquals("Signatories remain on the quote.", "B", testQuote.TH_GS_NKSecondSignatory);
		}

		public void TestAcceptQuote_RatesAlreadyExistsForClient()
		{
			var testRate = Helper.NewClientRate(NewClient);
			var entry1 = testRate.AddRateEntry("AIR", "LSE", "USLAX", "AUSYD");
			entry1.TI_RateStartDate = ZDate.Today.AddDays(-15);
			entry1.TI_RateEndDate = ZDate.Today.AddDays(165);
			var entry2 = testRate.AddRateEntry("LCL", "LCL", "USLAX", "AUSYD");
			entry2.TI_RateStartDate = ZDate.Today.AddDays(-15);
			entry2.TI_RateEndDate = ZDate.Today.AddDays(165);

			var testQuote = Helper.NewQuote(NewClient);
			testQuote.AddRateEntry("AIR", "LSE", "USLAX", "AUSYD");
			testQuote.AddRateEntry("LCL", "LCL", "USSFO", "AUSYD");
			testQuote.AddRateEntry("FCL", "SEA", "USLAX", "AUSYD", "", "20GP");

			testQuote.TryAcceptQuote(out ClientRate existingRate);
			var airRateEntries = existingRate.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection;
			var lclRateEntries = existingRate.EntryCollections[RatingConstants.RateCategory.LCL].LazyLoadingCollection;
			var fclRateEntries = existingRate.EntryCollections[RatingConstants.RateCategory.FCL].LazyLoadingCollection;

			AssertEquals("Is Locked", true, testQuote.TH_IsLocked);
			AssertEquals("Accepted Date", ZDateTime.Today, testQuote.TH_Accepted);

			AssertEquals("Air Rate Entries", 2, airRateEntries.Count);  // AIR already existed
			AssertEquals("LCL Rate Entries", 2, lclRateEntries.Count);  // LCL already existed
			AssertEquals("FCL Rate Entries", 1, fclRateEntries.Count);

			AssertEquals("Rate Start Date", ZDateTime.Today.AddDays(-15), airRateEntries[0].TI_RateStartDate);
			AssertEquals("Rate Start Date", ZDateTime.Today.AddDays(-15), lclRateEntries[0].TI_RateStartDate);

			AssertEquals("Air Rate End Date set to 1 day before new quote start date", ZDateTime.Today.AddDays(-1), airRateEntries[0].TI_RateEndDate);
			AssertEquals("LCL Rate End Date not changed as was not matching quote lcl entry", ZDateTime.Today.AddDays(165), lclRateEntries[0].TI_RateEndDate);

			// New Entries
			AssertEquals("Rate Start Date", testQuote.TH_QuoteDate, airRateEntries[1].TI_RateStartDate);
			AssertEquals("Rate End Date", testQuote.TH_QuoteEndDate, airRateEntries[1].TI_RateEndDate);

			AssertEquals("AIR 0 Rate Line Count", 1, airRateEntries[0].RateLines.Count);
			AssertEquals("AIR 1 Rate Line Count", 1, airRateEntries[1].RateLines.Count);
			AssertEquals("LCL 0 Rate Line Count", 1, lclRateEntries[0].RateLines.Count);
			AssertEquals("LCL 1 Rate Line Count", 1, lclRateEntries[1].RateLines.Count);
			AssertEquals("FCL 0 Rate Line Count", 1, fclRateEntries[0].RateLines.Count);
		}

		#endregion

		#region Cancel Quote

		public void TestCancelQuote()
		{
			var testQuote = Factory.New<Quote>();
			AssertEquals("Precondition: Not cancelled", false, testQuote.TH_IsCancelled);
			AssertEquals("Precondition: Editable", false, testQuote.TH_QuoteEndDateInfo.ReadOnly);

			testQuote.CancelQuote();
			AssertEquals("Cancelled", true, testQuote.TH_IsCancelled);
			AssertEquals("Readonly", true, testQuote.TH_QuoteEndDateInfo.ReadOnly);
		}

		public void TestQuotationCancelledEventLog()
		{
			var testQuote = Factory.New<Quote>();
			AssertEquals(0, testQuote.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.QuotationCancelled.Code)).Length);

			testQuote.CancelQuote();
			AssertEquals(1, testQuote.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.QuotationCancelled.Code)).Length);
		}

		public void TestCancelQuoteValidation()
		{
			RatingDataRegistry.Instance.IsQuoteCancellationReasonCodeRequired.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var quote = Factory.NewWithValidTestData<Quote>();
			Factory.Save();
			quote.CancelQuote();
			AssertEquals("Should has error", true, quote.TH_QuoteCancellationReasonInfo.HasErrors());
		}

		public void TestEndDateIsNotChangedWhenCancellingQuote()
		{
			var quote = Helper.NewQuote(Helper.NewOrgHeader());

			var endDate = ZDate.Today.AddMonths(1);
			quote.TH_QuoteEndDate = endDate;

			quote.CancelQuote();
			AssertEquals(endDate, quote.TH_QuoteEndDate);
		}

		#endregion

		#region Delete Quote

		public void TestCanDeleteQuote()
		{
			var acceptedQuote = GetAcceptedQuote();
			AssertEquals(false, acceptedQuote.CanDelete);
			AssertEquals("The selected quote has already been accepted and cannot be deleted.", acceptedQuote.ReasonForNotAbleToDelete);

			var clientAcceptedQuote = GetClientAcceptedQuote();
			AssertEquals(false, clientAcceptedQuote.CanDelete);
			AssertEquals("The selected quote has already been accepted by the client and cannot be deleted.", clientAcceptedQuote.ReasonForNotAbleToDelete);

			var activeQuote = GetActiveQuote();
			AssertEquals(true, activeQuote.CanDelete);
			AssertNull(activeQuote.ReasonForNotAbleToDelete);
		}

		#endregion

		public void TestQuoteLogs()
		{
			var quote = Factory.New<Quote>();
			AssertEquals(typeof(QuoteLogs), quote.Logs.GetType());
		}

		[ExpectNoExceptions]
		public void TestQuoteLogs_DataRefreshBusDoesNotLoadQuotedBookingLogs()
		{
			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.SpotQuote, Factory);
			var logParent = (IStmALogParent)quotedBooking.Quote;
			logParent.Logs.AddNew(Events.QuotationAccepted);
			Assert("Prerequisite - DataRefreshBus won't add elements to a collection that hasn't been loaded", logParent.Logs.GetAllLogs().IsLoaded);
			Factory.Save();

			var factory = new BusinessObjectFactory();
			quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().InitializeFrom(quotedBooking.Quote.PK, ZGuid.Empty, factory);
			logParent = (IStmALogParent)quotedBooking;
			logParent.Logs.AddNew(Events.ExWorks);

			factory.Save();
		}

		#region CFX Set On Rating Header

		public void TestCFXSetOnRatingHeader()
		{
			var testClient = Factory.New<OrgHeader>();
			testClient.OH_FullName = "Test Client";
			testClient.MainAddress.OA_Address1 = "184 Bourke Road";
			testClient.MainAddress.OA_City = "Alexandria";
			testClient.OH_RL_NKClosestPort = "AUSYD";

			testClient.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "IMP", "AIR", 10m);
			testClient.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "IMP", "SEA", 20m);
			testClient.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "EXP", "AIR", 30m);
			testClient.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "EXP", "SEA", 40m);

			var testQuote = Factory.New<Quote>();
			testQuote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "USLAX");
			testQuote.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.AIR, "AUSYD", "USLAX");
			testQuote.TH_OH = testClient.PK;
			AssertEquals("Import Air CFX", 10M, testQuote.TH_AirCFX);
			AssertEquals("Import Sea CFX", 20M, testQuote.TH_SeaCFX);
			AssertEquals("Export Air CFX", 30M, testQuote.TH_ExportAirCFX);
			AssertEquals("Export Sea CFX", 40M, testQuote.TH_ExportSeaCFX);

			testQuote.TH_AirCFX = 50M;
			testQuote.TH_SeaCFX = 60M;
			testQuote.TH_ExportAirCFX = 70M;
			testQuote.TH_ExportSeaCFX = 80M;

			Factory.Save();

			AssertEquals("Import Air CFX", 10M, testClient.CompanyData.AccCFXConfigurations.GetRecord("SHP", OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Air).JCF_CFXPercentage);
			AssertEquals("Import Sea CFX", 20M, testClient.CompanyData.AccCFXConfigurations.GetRecord("SHP", OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea).JCF_CFXPercentage);
			AssertEquals("Export Air CFX", 30M, testClient.CompanyData.AccCFXConfigurations.GetRecord("SHP", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air).JCF_CFXPercentage);
			AssertEquals("Export Sea CFX", 40M, testClient.CompanyData.AccCFXConfigurations.GetRecord("SHP", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea).JCF_CFXPercentage);

			AssertEquals("Import Air CFX", 50M, testQuote.TH_AirCFX);
			AssertEquals("Import Sea CFX", 60M, testQuote.TH_SeaCFX);
			AssertEquals("Export Air CFX", 70M, testQuote.TH_ExportAirCFX);
			AssertEquals("Export Sea CFX", 80M, testQuote.TH_ExportSeaCFX);

			testQuote.TryAcceptQuote(out var clientRate);

			AssertEquals("Import Air CFX", 50M, testClient.CompanyData.AccCFXConfigurations.GetRecord("SHP", OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Air).JCF_CFXPercentage);
			AssertEquals("Import Sea CFX", 60M, testClient.CompanyData.AccCFXConfigurations.GetRecord("SHP", OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea).JCF_CFXPercentage);
			AssertEquals("Export Air CFX", 70M, testClient.CompanyData.AccCFXConfigurations.GetRecord("SHP", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air).JCF_CFXPercentage);
			AssertEquals("Export Sea CFX", 80M, testClient.CompanyData.AccCFXConfigurations.GetRecord("SHP", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea).JCF_CFXPercentage);
		}

		#endregion

		#region Amended Quote Cancellation

		public void TestAmendedQuotationIsCancelled() => TestAmendedQuoteCancellation(isOneOffQuote: false, expectedIsCancelled: false);

		public void TestAmendedOneOffQuoteIsNotCancelled() => TestAmendedQuoteCancellation(isOneOffQuote: true, expectedIsCancelled: false);

		void TestAmendedQuoteCancellation(bool isOneOffQuote, bool expectedIsCancelled)
		{
			var originalQuote = Helper.NewQuote(NewClient);
			originalQuote.AddRateEntry("AIR", "LSE", "USLAX", "AUSYD");
			originalQuote.TH_OneTimeQuote = isOneOffQuote;
			Factory.Save();

			originalQuote.AmendmentCopy = true;
			originalQuote.SameClientCopy = true;

			((ITemplateCopyable)originalQuote).TemplateCopy();

			originalQuote.SameClientCopy = false;
			originalQuote.AmendmentCopy = false;

			Assert(!originalQuote.TH_IsCancelled);

			Factory.Save();

			AssertEquals(isOneOffQuote, !originalQuote.TH_IsCancelled);
		}

		#endregion

		#region Test Clone

		public void TestClone()
		{
			var testQuote = Factory.New<Quote>();
			testQuote.AddRateEntry("AIR");
			testQuote.Notes.AddNew();

			var clonedQuote = (Quote)testQuote.Clone();

			Assert("Different Objects", testQuote != clonedQuote);
			AssertEquals("No child Rate Entry", 0, clonedQuote.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR).Count);
			AssertEquals("1 copied note present", 1, clonedQuote.Notes.GetAllNotes().Count);
		}

		#endregion

		#region HasMinimumCostMarkupNotMetWarning

		public void TestHasMinimumCostMarkupNotMetWarning()
		{
			Env.Registry.Rating.MinimumMarkUpPercentages = "AUSYD|5;USLAX|5";

			var testQuote = Factory.New<Quote>();
			var entry = testQuote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");

			entry.RateLines[0].TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;
			entry.RateLines[0].InitializeCalculator();

			AssertEquals(5m, entry.RateLines[0].MinimumMarkupPercentage());
			Assert(testQuote.HasMinimumCostMarkupNotMetWarning());
			((CompanyTariffOrCostBasedCalculator)entry.RateLines[0].Calculator).Percent = 5m;
			Assert(!testQuote.HasMinimumCostMarkupNotMetWarning());
			((CompanyTariffOrCostBasedCalculator)entry.RateLines[0].Calculator).Percent = 3m;
			Assert(testQuote.HasMinimumCostMarkupNotMetWarning());
			((CompanyTariffOrCostBasedCalculator)entry.RateLines[0].Calculator).Percent = 6m;
			Assert(!testQuote.HasMinimumCostMarkupNotMetWarning());
		}

		#endregion

		#region Selected and Available Pages

		public void TestSelectedAvailablePages()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());

			AssertContainsExactElementsInAnyOrder("TestQuote.SelectedPages",
				new string[]
				{
					"Cover Page",
					"Forwarding Standard Pricing Page",
					"Shipping Standard Pricing Page",
					"CFS Rates Pricing Page",
					"Warehouse Rates Pricing Page",
					"Transport Rates Pricing Page",
					"Acceptance Page",
					"Contact Details",
					"Published Agents",
				},
				Array.ConvertAll(testQuote.SelectedPages.ToArray<RateAttachment>(), (a) => a.TA_RateAttachmentName.ToString()));

			AssertContainsExactElementsInAnyOrder("TestQuote.AvailablePages",
				new string[]
				{
					"Index Page",
					"One Off Pricing Page",
					"One Off Multi Carriers Pricing Page",
					"Forwarding Landscape Pricing Page",
					"Forwarding Compact Pricing Page",
					"Shipping Landscape Pricing Page",
					"Shipping Compact Pricing Page",
					"Shipping Detention Pricing Page",
				},
				Array.ConvertAll(testQuote.AvailablePages.ToArray<RateAttachmentSet>(), (a) => a.TS_AttachmentName.ToString()));

			Env.Registry.Rating.SetQuoteTermsAndConditionsPages(new Image[] { new Bitmap(100, 100), null, null, new Bitmap(101, 101), null, null, null, new Bitmap(102, 102), null, new Bitmap(103, 103) });

			testQuote = Helper.NewQuote(Helper.NewOrgHeader());

			AssertContainsExactElementsInAnyOrder("TestQuote.SelectedPages",
				new string[]
				{
					"Cover Page",
					"Forwarding Standard Pricing Page",
					"Shipping Standard Pricing Page",
					"CFS Rates Pricing Page",
					"Warehouse Rates Pricing Page",
					"Transport Rates Pricing Page",
					"Acceptance Page",
					"Contact Details",
					"Published Agents",
					"Trailing Page 1",
					"Trailing Page 4",
					"Trailing Page 8",
					"Trailing Page 10",
				},
				Array.ConvertAll(testQuote.SelectedPages.ToArray<RateAttachment>(), (a) => a.TA_RateAttachmentName.ToString()));

			AssertContainsExactElementsInAnyOrder("TestQuote.AvailablePages",
				new string[]
				{
					"Index Page",
					"One Off Pricing Page",
					"One Off Multi Carriers Pricing Page",
					"Forwarding Landscape Pricing Page",
					"Forwarding Compact Pricing Page",
					"Shipping Landscape Pricing Page",
					"Shipping Compact Pricing Page",
					"Shipping Detention Pricing Page",
				},
				Array.ConvertAll(testQuote.AvailablePages.ToArray<RateAttachmentSet>(), (a) => a.TS_AttachmentName.ToString()));

			Assert(testQuote.SelectedPages[9].IsImage);
			AssertEquals(100, testQuote.SelectedPages[9].Image.Width);
			Assert(testQuote.SelectedPages[10].IsImage);
			AssertEquals(101, testQuote.SelectedPages[10].Image.Width);
			Assert(testQuote.SelectedPages[11].IsImage);
			AssertEquals(102, testQuote.SelectedPages[11].Image.Width);
			Assert(testQuote.SelectedPages[12].IsImage);
			AssertEquals(103, testQuote.SelectedPages[12].Image.Width);
		}

		#endregion

		#region CoverPageText

		public void TestCoverPageText()
		{
			Env.Registry.Rating.QuoteCoverPageTextOneOffNew = "111";
			Env.Registry.Rating.QuoteCoverPageTextNew = "222";
			Env.Registry.Rating.QuoteCoverPageTextOneOffExisting = "333";
			Env.Registry.Rating.QuoteCoverPageTextExisting = "444";

			var quote1 = Helper.NewQuote(Helper.NewOrgHeader());
			AssertEquals("222", quote1.CoverPageText);

			Factory.Save();

			var quote2 = Helper.NewQuote(quote1.Header);
			AssertEquals("444", quote2.CoverPageText);

			var quote3 = Helper.NewQuote(quote1.Header);
			quote3.TH_OneTimeQuote = true;
			AssertEquals("333", quote3.CoverPageText);

			var quote4 = Helper.NewQuote(Helper.NewOrgHeader());
			quote4.TH_OneTimeQuote = true;
			AssertEquals("111", quote4.CoverPageText);
		}

		#endregion

		public void TestDoNotPrintDocumentsWithIncorrectDataContexts()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());

			var supporter = (Quote.QuoteDocumentSupporter)((IDocumentSupportable)testQuote).DocumentSupporter;

			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = "Test Quote Document";

			var template = Factory.New<StmTemplate>();
			template.SO_DataContext = ".InvalidDataContext";

			var templatePivot = Factory.New<StmMenuTemplatePivot>();
			templatePivot.SI_SU = menu.PK;
			templatePivot.SI_SO = template.PK;

			Factory.Save();

			AssertEquals("The data context .InvalidDataContext is invalid. Please check your template, amend the data context, reload the template and try again.", supporter.GetDataStateBeforeRun(menu).ErrorMessage);

			template.SO_DataContext = "Quotation";
			Factory.Save();

			AssertNotEquals("The data context Quotation is invalid. Please check your template, amend the data context, reload the template and try again.", supporter.GetDataStateBeforeRun(menu).ErrorMessage);
		}

		#region Printing Empty Quotes

		public void TestPrintEmptyQuote()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			testQuote.NoTradeLanesToPrint += new EventHandler(TestQuote_NoTradeLanesToPrint);

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = testQuote.PK;
			job.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_JH = job.PK;

			Factory.Save();

			var supporter = (RatingHeaderDocumentSupporter)((IDocumentSupportable)testQuote).DocumentSupporter;
			NoTradeLanesToPrintCalled = false;
			var task = supporter.BuildPrintTask(null);
			AssertNull(task);
			Assert(NoTradeLanesToPrintCalled);

			testQuote.TH_OneTimeQuote = true;
			var oneOffQuote = testQuote.CurrentOneOffQuote;
			oneOffQuote.TT_TransportMode = Constants.TransportModes.Air;
			oneOffQuote.TT_ContainerMode = Constants.ContainerModes.Loose;
			oneOffQuote.TT_RL_NKReceivalLocation = "AUSYD";
			oneOffQuote.TT_RL_NKDeliveryLocation = "USLAX";
			oneOffQuote.TT_ActualWeight = 100;
			oneOffQuote.TT_UnitOfWeight = RatingConstants.Units.KG;

			NoTradeLanesToPrintCalled = false;
			task = supporter.BuildPrintTask(null);
			AssertNotNull(task);
			Assert(!NoTradeLanesToPrintCalled);
		}

		void TestQuote_NoTradeLanesToPrint(object sender, EventArgs e)
		{
			NoTradeLanesToPrintCalled = true;
		}

		bool NoTradeLanesToPrintCalled;

		#endregion

		#region Signatures

		public void TestSignatures()
		{
			var org1 = Helper.NewOrgHeader();
			var org2 = Helper.NewOrgHeader();

			var rep = Factory.New<GlbStaff>();
			rep.GS_FullName = "Test Rep";
			rep.GS_Code = "TR";
			org2.StaffAssignments.OverallSalesRep = rep.GS_Code;

			var testQuote = Factory.New<Quote>();
			AssertEquals("", testQuote.TH_GS_NKFirstSignatory);
			AssertEquals("", testQuote.TH_GS_NKSecondSignatory);

			testQuote.TH_OH = org1.PK;
			AssertEquals(GlbStaff.CurrentUser.GS_Code, testQuote.TH_GS_NKFirstSignatory);
			AssertEquals("", testQuote.TH_GS_NKSecondSignatory);

			testQuote.TH_GS_NKSecondSignatory = rep.GS_Code;
			AssertEquals(rep.GS_Code, testQuote.TH_GS_NKSecondSignatory);

			testQuote.TH_OH = org2.PK;
			AssertEquals(rep.GS_Code, testQuote.TH_GS_NKFirstSignatory);
			AssertEquals("", testQuote.TH_GS_NKSecondSignatory);
		}

		#endregion

		#region ModifyingCompanyUpdatesDependentFields

		public void TestModifyingCompanyUpdatesDependentFields()
		{
			var salesRep1 = Factory.New<GlbStaff>();
			salesRep1.GS_LoginName = "SALESREP1";
			salesRep1.GS_Code = "SP1";

			var salesRep2 = Factory.New<GlbStaff>();
			salesRep2.GS_LoginName = "SALESREP2";
			salesRep2.GS_Code = "SP2";

			var testOrg = Helper.NewOrgHeader();

			var salesRepAssignmentCurrentCompany = testOrg.StaffAssignments.AddNew();
			salesRepAssignmentCurrentCompany.O8_GC = GlbCompany.CurrentCompany.PK;
			salesRepAssignmentCurrentCompany.O8_GS_NKPersonResponsible = salesRep1.GS_Code;
			salesRepAssignmentCurrentCompany.O8_Role = StaffAssignmentRoles.Codes.SalesRep;

			var headerCompanyDataForCurrentCompany = testOrg.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany);
			headerCompanyDataForCurrentCompany.AccCFXConfigurations.SetUplifts("ALL", "IMP", "AIR", 10m);
			headerCompanyDataForCurrentCompany.AccCFXConfigurations.SetUplifts("ALL", "IMP", "SEA", 20m);
			headerCompanyDataForCurrentCompany.AccCFXConfigurations.SetUplifts("ALL", "EXP", "AIR", 30m);
			headerCompanyDataForCurrentCompany.AccCFXConfigurations.SetUplifts("ALL", "EXP", "SEA", 40m);

			var otherCompany = Factory.New<GlbCompany>();
			otherCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.HongKong;
			otherCompany.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.HongKong;

			Assert("Currency of Other Company must differ from Currency of CurrentCompany", otherCompany.GC_RX_NKLocalCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			Factory.Save();

			var otherBranch = otherCompany.Branches.AddNew();

			var salesRepAssignmentOtherCompany = testOrg.StaffAssignments.AddNew();
			salesRepAssignmentOtherCompany.O8_GC = otherCompany.PK;
			salesRepAssignmentOtherCompany.O8_GS_NKPersonResponsible = salesRep2.GS_Code;
			salesRepAssignmentOtherCompany.O8_Role = StaffAssignmentRoles.Codes.SalesRep;

			var headerCompanyDataForOtherCompany = testOrg.GetCompanyDataForGlbCompany(otherCompany);
			headerCompanyDataForOtherCompany.AccCFXConfigurations.SetUplifts("ALL", "IMP", "AIR", 50m);
			headerCompanyDataForOtherCompany.AccCFXConfigurations.SetUplifts("ALL", "IMP", "SEA", 60m);
			headerCompanyDataForOtherCompany.AccCFXConfigurations.SetUplifts("ALL", "EXP", "AIR", 70m);
			headerCompanyDataForOtherCompany.AccCFXConfigurations.SetUplifts("ALL", "EXP", "SEA", 80m);

			Factory.Save();

			var testQuote = Helper.NewQuote(testOrg);
			testQuote.TH_OneTimeQuote = ZBool.True;
			AssertEquals("Quote should contain a OneOffQuote", 1, testQuote.OneOffQuote.Count);
			AssertEquals("Quote should contain a OneOffQuote", testQuote.OneOffQuote[0], testQuote.CurrentOneOffQuote);
			var oneOffQuote = testQuote.CurrentOneOffQuote;
			AssertNotNull("OneOffQuote should not be null", oneOffQuote);

			AssertEquals("SalesRepresentative should be SalesRep1", salesRep1.GS_Code, testQuote.HeaderStaffAssignments.OverallSalesRep);

			AssertEquals("TH_AirCFX Company1", 10M, testQuote.TH_AirCFX);
			AssertEquals("TH_SeaCFX Company1", 20M, testQuote.TH_SeaCFX);
			AssertEquals("TH_ExportAirCFX Company1", 30M, testQuote.TH_ExportAirCFX);
			AssertEquals("TH_ExportSeaCFX Company1", 40M, testQuote.TH_ExportSeaCFX);

			AssertEquals("FirstSignatory should be SalesRep1", salesRep1.GS_Code, testQuote.TH_GS_NKFirstSignatory);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, otherBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				testQuote.TH_GC = otherCompany.PK;
				AssertEquals("SalesRepresentative should be SalesRep2", salesRep2.GS_Code, testQuote.HeaderStaffAssignments.OverallSalesRep);

				AssertEquals("TH_AirCFX should be using Company2 OrgCompanyData, 10M indicates it is using Company1", 50M, testQuote.TH_AirCFX);
				AssertEquals("TH_SeaCFX should be using Company2 OrgCompanyData, 20M indicates it is using Company1", 60M, testQuote.TH_SeaCFX);
				AssertEquals("TH_ExportAirCFX should be using Company2 OrgCompanyData, 30M indicates it is using Company1", 70M, testQuote.TH_ExportAirCFX);
				AssertEquals("TH_ExportSeaCFX should be using Company2 OrgCompanyData, 40M indicates it is using Company1", 80M, testQuote.TH_ExportSeaCFX);

				AssertEquals("FirstSignatory should be SalesRep2", salesRep2.GS_Code, testQuote.TH_GS_NKFirstSignatory);
			}
		}
		#endregion

		#region FirstMatchingEntryForOneOffQuote

		public void TestFirstMatchingEntryForOneOffQuote_Courier()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = testQuote.PK;
			job.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_JH = job.PK;

			Factory.Save();

			testQuote.TH_OneTimeQuote = true;
			var oneOffQuote = testQuote.CurrentOneOffQuote;
			oneOffQuote.TT_TransportMode = Constants.TransportModes.Courier;
			oneOffQuote.TT_ContainerMode = Constants.RateMode.COU;
			oneOffQuote.TT_RL_NKReceivalLocation = "AUSYD";
			oneOffQuote.TT_RL_NKDeliveryLocation = "USLAX";
			oneOffQuote.TT_ActualWeight = 100;
			oneOffQuote.TT_UnitOfWeight = RatingConstants.Units.KG;

			var entry = testQuote.FirstMatchingEntryForOneOffQuote;
			AssertNotNull("FirstMatchingEntryForOneOffQuote", entry);
			AssertEquals(RatingConstants.RateCategory.AIR, entry.TI_RateCategory);
			AssertEquals(Core.Constants.RateMode.COU, entry.TI_Mode);
		}

		#endregion

		#region IJobHeaderParent Members

		public void TestIJobHeaderParent_AllowInvoiceDeletion()
		{
			var jobStorage = Factory.New<Quote>() as IJobHeaderParent;
			Assert(jobStorage.AllowInvoiceDeletion);
		}

		#endregion

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

			var quote = Factory.New<Quote>();
			((IImportParentRelatedActivityInfoOnNew)quote).ImportParentInfo(inquiry, new ImportRelatedActivityNoDecisionFactory());

			AssertEquals(org.PK, quote.TH_OH);
		}

		#endregion

		#region TestCopyQuotationForSameClientWhenCalculatorInOrigionIsCTZ

		[TestDate(2013, 11, 11)]
		public void TestCopyQuotationForSameClientWhenCalcInORGIsCTZ()
		{
			Helper.CreateRateTransportZoneSet(NewClient, Core.Constants.CountryCodes.Australia, zoneNames: new ZString[] { "CARTZ 1", "CARTZ 2", "CARTZ 3" });
			Factory.Save();

			var originalQuote = Helper.NewQuote(NewClient);
			originalQuote.TH_QuoteDate = new ZDate(2013, 10, 1);
			originalQuote.TH_QuoteEndDate = new ZDate(2013, 11, 1);
			originalQuote.SameClientCopy = true;

			var entry = originalQuote.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, Core.Constants.CountryCodes.Australia, "");
			var line = entry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);
			var calculator = line.GetCalculator<CartageZoneDistanceCalculator>();

			AssertEquals("Has 4 Zones including Standard Zone", 4, calculator.CartageZones.Count);

			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 10m, calculator.CartageZones[1].PK);
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0m, 60m, calculator.CartageZones[2].PK);
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Minus, 10m, 5m, calculator.CartageZones[2].PK);
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 10m, 4m, calculator.CartageZones[2].PK);
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.MIN, 0m, 80m, calculator.CartageZones[3].PK);
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Minus, 20m, 7m, calculator.CartageZones[3].PK);
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 20m, 6m, calculator.CartageZones[3].PK);

			var newQuote = (Quote)((ITemplateCopyable)originalQuote).TemplateCopy();
			var newOrgRateEntries = (RateEntryCollection)newQuote.EntryCollections["ORG"];
			AssertEquals("Has 1 ORG Entry", 1, newOrgRateEntries.Count);

			var newOrgRateEntry = newOrgRateEntries[0];
			AssertEquals("Has 1 Rate Line", 1, newOrgRateEntry.RateLines.Count);

			var newRateLine = newOrgRateEntry.RateLines[0];
			AssertEquals(typeof(CartageZoneDistanceCalculator), newRateLine.Calculator.GetType());

			var newCalculator = newRateLine.GetCalculator<CartageZoneDistanceCalculator>();
			var zonesCount = newCalculator.CartageZones.Count;
			AssertEquals("Has 4 Zones", 4, zonesCount);
			AssertEquals(calculator.CartageZones[0].ZoneRateLineItems.Count, newCalculator.CartageZones[0].ZoneRateLineItems.Count);
			AssertEquals(calculator.CartageZones[1].ZoneRateLineItems.Count, newCalculator.CartageZones[1].ZoneRateLineItems.Count);
			AssertEquals(calculator.CartageZones[2].ZoneRateLineItems.Count, newCalculator.CartageZones[2].ZoneRateLineItems.Count);
			AssertEquals(calculator.CartageZones[3].ZoneRateLineItems.Count, newCalculator.CartageZones[3].ZoneRateLineItems.Count);
		}

		#endregion

		#region Job Header Deactivation Tests

		public void TestJobHeaderIsNotDeactivatedWhenFactoryHasInvoicingPlugInGUIBusinessContext()
		{
			AssertJobHeaderDeactivation(true);
		}

		public void TestJobHeaderIsDeactivatedWhenFactoryDoesNotHaveInvoicingPlugInGUIBusinessContext()
		{
			AssertJobHeaderDeactivation(false);
		}

		void AssertJobHeaderDeactivation(bool hasInvoicingPlugInGUIContext)
		{
			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			var jobLoader = new JobHeader.Loader(quote);
			var job = jobLoader.TryCreate();
			Factory.Save();

			quote.IsCancelled = true;
			if (hasInvoicingPlugInGUIContext)
			{
				Factory.SetContext(BusinessContext.InvoicingPlugInGUI);
			}

			var assertionMessage1 = string.Format("quote {0} have InvoicingPluginGUI Business Context", hasInvoicingPlugInGUIContext ? "should" : "should not");
			var assertionMessage2 = string.Format("Job {0} be deactivated by OnSaving method", hasInvoicingPlugInGUIContext ? "should not" : "should");

			Assert("Deactivating quote, IsCancelled flag should be set to true", quote.IsCancelled);
			Assert("Deactivating quote, IsCancelledInfo should have changes", quote.IsCancelledHasChanged);
			AssertEquals(assertionMessage1, hasInvoicingPlugInGUIContext, quote.HasContext(BusinessContext.InvoicingPlugInGUI));
			Assert("Job is not yet deactivated", !job.IsCancelled);

			Factory.Save();

			AssertEquals(assertionMessage2, !hasInvoicingPlugInGUIContext, job.IsCancelled);
		}

		#endregion

		#region Can Save with No Local Client Tests

		public void TestNormalOrganisation_HasChargesNoLocalClient_CanSave()
		{
			var testQuote = GetQuote(true);
			Factory.Save();

			var job = new Job.Loader(testQuote).TryCreateWithoutMutexForTestOnly();
			job.JH_TH_NKQuoteNumber = testQuote.TH_QuoteNumber;
			Assert(job.OneOffQuote == testQuote);
			Factory.Save();

			Assert(testQuote.TH_OneTimeQuote);
			Assert(job.JH_TH_NKQuoteNumber == testQuote.TH_QuoteNumber);

			Assert(job.LocalChargesPK.IsEmpty);
			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_JH = job.PK;

			Assert(job.Charges.Count > 0);
			job.RunPreSaveValidation();
			Factory.Save();

			Assert(job.JH_OA_LocalChargesAddrInfo.Notifications.IsNullOrEmpty());
			AssertApproved(testQuote, false);
			AssertMessages("Should be Dialog", "WishToApproveWhenSaving");
		}

		public void TestNormalOrganisation_HasChargesNoLocalClient_CantApprove()
		{
			var testQuote = GetQuote(true);
			Factory.Save();

			var job = new Job.Loader(testQuote).TryCreateWithoutMutexForTestOnly();
			job.JH_TH_NKQuoteNumber = testQuote.TH_QuoteNumber;
			Assert(job.OneOffQuote == testQuote);

			Assert(job.LocalChargesPK.IsEmpty);

			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_JH = job.PK;

			Assert(job.Charges.Count > 0);
			job.RunPreSaveValidation();
			Factory.Save();

			Assert(job.JH_OA_LocalChargesAddrInfo.Notifications.IsNullOrEmpty());
			AssertApproved(testQuote, false);

			var actionResult = testQuote.InternalApproveQuote();
			Factory.Save();
			AssertEquals("Should NOT be approved", false, actionResult);
			AssertApproved(testQuote, false);

			AssertMessages("Should be Dialog", "WishToApproveWhenSaving", "MissingLocalClientMessage");
		}

		public void TestNormalOrganisation_HasChargesHasLocalClient_CanApprove()
		{
			var testQuote = GetQuote(true);
			Factory.Save();

			var job = new Job.Loader(testQuote).TryCreateWithoutMutexForTestOnly();
			job.JH_TH_NKQuoteNumber = testQuote.TH_QuoteNumber;
			Assert(job.OneOffQuote == testQuote);

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			job.LocalChargesPK = localClient.PK;

			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_JH = job.PK;

			Assert(job.Charges.Count > 0);
			job.RunPreSaveValidation();
			Factory.Save();
			AssertApproved(testQuote, false);

			confirmDialog = true;
			var actionResult = testQuote.InternalApproveQuote();
			Factory.Save();
			AssertEquals("Should be approved", true, actionResult);
			AssertApproved(testQuote, true);
			AssertMessages("Should be Dialog", "WishToApproveWhenSaving", "WishToApprove");
		}

		public void TestNormalOrganisation_NoChargesNoLocalClient_CanApprove()
		{
			var testQuote = GetQuote(true);
			Factory.Save();

			var job = new Job.Loader(testQuote).TryCreateWithoutMutexForTestOnly();
			Assert(job.LocalChargesPK.IsEmpty);

			Assert(job.Charges.Count == 0);
			AssertApproved(testQuote, false);
			job.RunPreSaveValidation();
			Factory.Save();
			AssertApproved(testQuote, false);

			confirmDialog = true;
			var actionResult = testQuote.InternalApproveQuote();
			Factory.Save();
			AssertEquals("Should be approved", true, actionResult);
			AssertApproved(testQuote, true);
			AssertMessages("Should be Dialog", "WishToApproveWhenSaving", "WishToApprove");
		}
		public void TestNormalOrganisation_HasChargesHasOverseasAgent_CanApprove()
		{
			var testQuote = GetQuote(true);
			Factory.Save();

			var job = new Job.Loader(testQuote).TryCreateWithoutMutexForTestOnly();
			job.JH_TH_NKQuoteNumber = testQuote.TH_QuoteNumber;
			Assert(job.OneOffQuote == testQuote);

			testQuote.CurrentOneOffQuote.TT_OrgRole = Core.Constants.OrgRoles.OverseasAgent;

			var overseasAgent = Factory.NewWithValidTestData<OrgHeader>();
			job.AgentCollectPK = overseasAgent.PK;

			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_JH = job.PK;

			Assert(job.Charges.Count > 0);
			job.RunPreSaveValidation();
			Factory.Save();
			AssertApproved(testQuote, false);

			confirmDialog = true;
			var actionResult = testQuote.InternalApproveQuote();
			Factory.Save();
			AssertEquals("Should be approved", true, actionResult);
			AssertApproved(testQuote, true);
			AssertMessages("Should be Dialog", "WishToApproveWhenSaving", "WishToApprove");
		}

		public void TestNormalOrganisation_NoChargesNoOverseasAgent_CanApprove()
		{
			var testQuote = GetQuote(true);
			Factory.Save();

			var job = new Job.Loader(testQuote).TryCreateWithoutMutexForTestOnly();
			Assert(job.LocalChargesPK.IsEmpty);

			testQuote.CurrentOneOffQuote.TT_OrgRole = Core.Constants.OrgRoles.OverseasAgent;

			Assert(job.Charges.Count == 0);
			AssertApproved(testQuote, false);
			job.RunPreSaveValidation();
			Factory.Save();
			AssertApproved(testQuote, false);

			confirmDialog = true;
			var actionResult = testQuote.InternalApproveQuote();
			Factory.Save();
			AssertEquals("Should be approved", true, actionResult);
			AssertApproved(testQuote, true);
			AssertMessages("Should be Dialog", "WishToApproveWhenSaving", "WishToApprove");
		}

		public void TestNormalOrganisation_HasChargesNoOverseasAgent_CanSave()
		{
			var testQuote = GetQuote(true);
			Factory.Save();

			var job = new Job.Loader(testQuote).TryCreateWithoutMutexForTestOnly();
			job.JH_TH_NKQuoteNumber = testQuote.TH_QuoteNumber;
			Assert(job.OneOffQuote == testQuote);
			Factory.Save();

			testQuote.CurrentOneOffQuote.TT_OrgRole = Core.Constants.OrgRoles.OverseasAgent;

			Assert(testQuote.TH_OneTimeQuote);
			Assert(job.JH_TH_NKQuoteNumber == testQuote.TH_QuoteNumber);

			Assert(job.LocalChargesPK.IsEmpty);
			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_JH = job.PK;

			Assert(job.Charges.Count > 0);
			job.RunPreSaveValidation();
			Factory.Save();

			Assert(job.JH_OA_LocalChargesAddrInfo.Notifications.IsNullOrEmpty());
			AssertApproved(testQuote, false);
			AssertMessages("Should be Dialog", "WishToApproveWhenSaving");
		}

		public void TestNormalOrganisation_HasChargesNoOverseasAgent_CantApprove()
		{
			var testQuote = GetQuote(true);
			Factory.Save();

			var job = new Job.Loader(testQuote).TryCreateWithoutMutexForTestOnly();
			job.JH_TH_NKQuoteNumber = testQuote.TH_QuoteNumber;
			Assert(job.OneOffQuote == testQuote);

			testQuote.CurrentOneOffQuote.TT_OrgRole = Core.Constants.OrgRoles.OverseasAgent;

			Assert(job.AgentCollectPK.IsEmpty);

			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_JH = job.PK;

			Assert(job.Charges.Count > 0);
			job.RunPreSaveValidation();
			Factory.Save();

			Assert(job.JH_OA_AgentCollectAddrInfo.Notifications.IsNullOrEmpty());
			AssertApproved(testQuote, false);

			var actionResult = testQuote.InternalApproveQuote();
			Factory.Save();
			AssertEquals("Should NOT be approved", false, actionResult);
			AssertApproved(testQuote, false);

			AssertMessages("Should be Dialog", "WishToApproveWhenSaving", "MissingOverseasAgentMessage");
		}

		public void TestInternalApprovalOverride_HasChargesNoLocalClient_CanSave()
		{
			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			TestNormalOrganisation_HasChargesNoLocalClient_CanSave();
		}
		public void TestInternalApprovalOverride_HasChargesNoLocalClient_CantApprove()
		{
			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			TestNormalOrganisation_HasChargesNoLocalClient_CantApprove();
		}
		public void TestInternalApprovalOverride_HasChargesHasLocalClient_CanApprove()
		{
			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			TestNormalOrganisation_HasChargesHasLocalClient_CanApprove();
		}
		public void TestInternalApprovalOverride_NoChargesNoLocalClient_CanApprove()
		{
			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			TestNormalOrganisation_NoChargesNoLocalClient_CanApprove();
		}
		public void TestInternalApprovalOverride_HasChargesNoOverseasAgent_CanSave()
		{
			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			TestNormalOrganisation_HasChargesNoOverseasAgent_CanSave();
		}
		public void TestInternalApprovalOverride_HasChargesNoOverseasAgent_CantApprove()
		{
			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			TestNormalOrganisation_HasChargesNoOverseasAgent_CantApprove();
		}
		public void TestInternalApprovalOverride_HasChargesHasOverseasAgent_CanApprove()
		{
			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			TestNormalOrganisation_HasChargesHasOverseasAgent_CanApprove();
		}
		public void TestInternalApprovalOverride_NoChargesNoOverseasAgent_CanApprove()
		{
			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			TestNormalOrganisation_NoChargesNoOverseasAgent_CanApprove();
		}

		#endregion

		public void TestNoExceptionThrownWhenTH_GCIsSetToEmpty()
		{
			// Why this test? please take a look at: Enterprise.Tracking.Business.Quotations.Testing.TrackingQuoteTest.TestNewPropertiesForWeb()
			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			AssertNoExceptionThrown(() => quote.TH_GC = ZGuid.Empty);
		}

		public void TestOnQuoteSaved()
		{
			var quote = Factory.New<Quote>();

			var quoteSavedCalled = false;
			var hasSaveSucceeded = false;
			quote.OnQuoteSaved += (sender, args) =>
			{
				quoteSavedCalled = true;
				hasSaveSucceeded = ((Quote.SavedEventArgs)args).HasSaveSucceeded;
			};

			Factory.Save();
			Assert("OnQuoteSaved is called", quoteSavedCalled);
			Assert("Save succeeded", hasSaveSucceeded);

			quoteSavedCalled = false;
			Factory.Saving += delegate
			{ throw new ZCannotSaveException("Something went wrong.", "Cannot save Quote"); };

			try
			{
				Factory.Save();
				Fail("Should have thrown exception");
			}
			catch (Exception)
			{
				Assert("OnQuoteSaved is called", quoteSavedCalled);
				AssertEquals("Save failed", false, hasSaveSucceeded);
			}
		}

		public void TestHasNonZeroSellAmt()
		{
			var quote = Helper.NewQuote(NewClient);
			quote.TH_OneTimeQuote = true;
			quote.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Air;
			quote.CurrentOneOffQuote.TT_ContainerMode = Constants.RateMode.LSE;
			quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");

			var job = new Job.Loader(quote).TryCreate();
			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_OH_SellAccount = NewClient.PK;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OH_CostAccount = TransportProvider1.PK;
			charge.JR_RX_NKCostCurrency = "USD";
			charge.JR_OSCostAmt = 10000;
			charge.JR_OSSellAmt = 0;
			Factory.Save();

			Assert(!quote.HasNonZeroSellAmt);

			charge.JR_OSSellAmt = 5000;
			Factory.Save();

			Assert(quote.HasNonZeroSellAmt);

			charge.JR_OSSellAmt = -1;
			Factory.Save();

			Assert(quote.HasNonZeroSellAmt);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Globals.IsUserInteractive = true;
		}

		Quote GetNewQuote()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var quote = Factory.NewWithValidTestData<Quote>();
			quote.TH_OH = orgHeader.PK;

			return quote;
		}

		Quote GetClientAcceptedQuote()
		{
			Quote quote = GetNewQuote();
			quote.TH_ClientAccepted = ZDateTime.Today;

			Factory.Save();

			return quote;
		}

		Quote GetActiveQuote()
		{
			bool oldValue = DataRegistryRating.Instance.QuoteRequireInternalApproval.Value;
			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Quote quote = GetNewQuote();
			quote.ShowApprovalDialog += new EventHandler<Quote.ApprovalDialogEventArgs>(delegate(object sender, Quote.ApprovalDialogEventArgs e)
			{ e.Cancel = true; });

			Factory.Save();
			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldValue);

			return quote;
		}

		Quote GetAcceptedQuote()
		{
			Quote quote = GetNewQuote();
			quote.TH_Accepted = ZDateTime.Today;

			Factory.Save();

			return quote;
		}
	}
}
