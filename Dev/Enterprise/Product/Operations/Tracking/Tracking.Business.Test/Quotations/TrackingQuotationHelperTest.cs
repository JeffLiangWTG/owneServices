using System;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Utilities.Environment;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	public class TrackingQuotationHelperTest : TestCaseWithFactory
	{
		#region TestConstructor

		[ExpectNoExceptions]
		public void TestConstructor()
		{
			TrackingQuotationHelper helper = new TrackingQuotationHelper(Factory, WebTestHelper.TestSiteUser.LoggedInOrgContact, SpotQuote);
		}

		#endregion

		#region TestDeliver_NullRef

		public void TestDeliver_TaskIsNull()
		{
			SpotQuote.Quote.OneOffQuote.RemoveAndDeleteAll();
			SpotQuote.Quote.SelectedPages.RemoveAndDeleteAll();
			SpotQuote.Quote.WorkflowItems.RemoveAndDeleteAll();
			ViewRelatedActivityPivot.DeleteAllPivots(SpotQuote.Quote);

			var test = new TrackingQuotationHelper(Factory, WebTestHelper.TestSiteUser.LoggedInOrgContact, SpotQuote);

			MethodInfo method = typeof(TrackingQuotationHelper).GetMethod("Deliver",
				BindingFlags.NonPublic | BindingFlags.Instance);
			AssertNotNull(method);
			AssertNoExceptionThrown(() => method.Invoke(test, null));
		}

		public void TestDeliver_DocSupporterIsNull()
		{
			var test = new TrackingQuotationHelper(Factory, WebTestHelper.TestSiteUser.LoggedInOrgContact, SpotQuote);

			var docSupporter = ((IDocumentSupportable)SpotQuote.Quote).DocumentSupporter;

			DocumentZQuery filter = new DocumentZQuery(docSupporter.BusinessContext, Constants.MenuNameConstantsForPrinting.QuotationPack);
			var result = Factory.Load<DocumentCommand>(filter);
			foreach (var documentCommand in result)
			{
				documentCommand.Delete();
			}

			AssertNoExceptionThrown(() => test.Finalise());
		}

		#endregion

		#region TestQuoteKey

		public void TestQuoteKey()
		{
			AssertEquals("QuoteKey", SpotQuote.Quote.PK, QuotationHelper.QuoteKey);
		}

		#endregion

		#region TestQuotationPrefix

		public void TestQuotationPrefix()
		{
			AssertEquals("QuotationPrefix", SpotQuote.Quote.QuoteNumberWithoutAmendmentSuffix, QuotationHelper.QuotationPrefix);
		}

		#endregion

		#region TestHasFreightCharges

		public void TestHasFreightCharges()
		{
			//get { return ((Quote.QuoteDocumentSupporter)((IDocumentSupportable)Quote).DocumentSupporter).ValidOneOffQuoteChargesExist; }

			QuotationHelper = new TrackingQuotationHelper(Factory, WebTestHelper.TestSiteUser.LoggedInOrgContact, SpotQuote);

			AssertEquals("Charges should not be calculated at this point", ZDecimal.Zero, QuotationHelper.TotalSellAmount);
			AssertEquals("No Charges should be calculated", 0, QuotationHelper.JobCharges.Count);
			AssertEquals("No Freight Charges should be found", false, QuotationHelper.HasFreightCharges);

			Assert("Preview should have been successful", QuotationHelper.Preview());
			AssertEquals("Quote should have charges calculated", 1, QuotationHelper.JobCharges.Count);
			AssertEquals("TotalAmount", 100m, QuotationHelper.TotalSellAmount);
			Assert("Freight Charges should be found", QuotationHelper.HasFreightCharges);

			ZGuid chargeCode = QuotationHelper.JobCharges[0].JR_AC;
			SpotQuote.Quote.CurrentOneOffQuote.TT_IncoTerm = ZString.Empty;
			Assert("IncoTerm is empty", SpotQuote.Quote.CurrentOneOffQuote.TT_IncoTerm.IsEmpty);
			AssertEquals("Charge code should match", Env.Registry.FreightChargeCode, chargeCode.ToGuid());

			Env.Registry.FreightChargeCode = ZGuid.NewZGuid().ToGuid();
			AssertEquals("HasFreightCharges should be True because IncoTerm is empty", true, QuotationHelper.HasFreightCharges);

			SpotQuote.Quote.CurrentOneOffQuote.TT_IncoTerm = Constants.IncoTerms.ExWorks;
			AssertEquals("HasFreightCharges should be False", false, QuotationHelper.HasFreightCharges);
		}

		#endregion

		#region TestFinalise

		public void TestFinalise()
		{
			AssertQuoteInitialState(SpotQuote);
			AssertFinalise(SpotQuote);
		}

		#endregion

		#region TestPreview

		public void TestPreview()
		{
			QuotationHelper = new TrackingQuotationHelper(Factory, WebTestHelper.TestSiteUser.LoggedInOrgContact, SpotQuote);
			QuotationHelper.QuoteUpdated += new EventHandler(QuotationHelper_QuoteUpdated);

			AssertEquals("Charges should not be calculated at this point", ZDecimal.Zero, QuotationHelper.TotalSellAmount);
			AssertEquals("No Charges should be calculated", 0, QuotationHelper.JobCharges.Count);
			AssertEquals("No Emails should be waiting to be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			Assert("Quotation Updated Handler should be false", !QuoteUpdatedHandlerCalled);

			bool result = QuotationHelper.Preview();
			AssertEquals("Preview should have been successful", true, result);
			AssertEquals("Quote should not be finalised", false, SpotQuote.Quote.TH_IsLocked);
			AssertEquals("Quote should be active", Quote.QuoteStatusOptions.Active, SpotQuote.Quote.QuoteStatus);
			AssertEquals("Quote should have charges calculated", 1, QuotationHelper.JobCharges.Count);
			AssertEquals("TotalAmount", 100m, QuotationHelper.TotalSellAmount);
			Assert("Quote Updated Handler should have been called", QuoteUpdatedHandlerCalled);

			SpotQuote.Volume = 100m;
			QuoteUpdatedHandlerCalled = false;
			QuotationHelper.Preview();
			AssertEquals("Quote should not be finalised", false, SpotQuote.Quote.TH_IsLocked);
			AssertEquals("Quote should have charges calculated", 1, QuotationHelper.JobCharges.Count);
			AssertEquals("TotalAmount should have been updated", 500m, QuotationHelper.TotalSellAmount);
			Assert("Quote Updated Handler should have been called", QuoteUpdatedHandlerCalled);

			SpotQuote.Volume = 50m;
			QuoteUpdatedHandlerCalled = false;
			QuotationHelper.Finalise();
			AssertEquals("Quote should be finalised", true, SpotQuote.Quote.TH_IsLocked);
			AssertEquals("Quote should have charges calculated", 1, QuotationHelper.JobCharges.Count);
			AssertEquals("Total Amount should have been updated", 250m, QuotationHelper.TotalSellAmount);
			Assert("Quote Updated Handler should have been called", QuoteUpdatedHandlerCalled);

			SpotQuote.Volume = 100m;
			QuoteUpdatedHandlerCalled = false;
			QuotationHelper.Preview();
			AssertEquals("Quote should still be locked", true, SpotQuote.Quote.TH_IsLocked);
			AssertEquals("Quote should still have charges calculated", 1, QuotationHelper.JobCharges.Count);
			AssertEquals("Total Amount should not have been updated", 250m, QuotationHelper.TotalSellAmount);
			Assert("Quote Updated Handler should not have been called", !QuoteUpdatedHandlerCalled);
		}

		#endregion

		#region TestPreviewDoesNothingForEmptyQuotation

		public void TestPreviewDoesNothingForEmptyQuotation()
		{
			SpotQuote.Origin = "USNYC";
			QuotationHelper.QuoteUpdated += new EventHandler(QuotationHelper_QuoteUpdated);

			AssertEquals("Charges should not be calculated at this point", ZDecimal.Zero, QuotationHelper.TotalSellAmount);
			AssertEquals("No Charges should be calculated", 0, QuotationHelper.JobCharges.Count);
			AssertEquals("No Emails should be waiting to be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			Assert("Quotation Updated Handler should be false", !QuoteUpdatedHandlerCalled);
			QuotationHelper.Preview();
			AssertEquals("Quote should not be finalised", false, SpotQuote.Quote.TH_IsLocked);
			AssertEquals("Quote should not have charges calculated", 0, QuotationHelper.JobCharges.Count);
			Assert("Quote Updated Handler should not have been called", !QuoteUpdatedHandlerCalled);
		}
		#endregion

		#region TestPreviewWillDeleteJobIfAnyJobExistsFromBefore
		public void TestPreviewWillDeleteJobIfAnyJobExistsFromBefore()
		{
			SpotQuote.TryLoadOrCreateJob();
			JobHeader job = SpotQuote.Job;

			bool result = QuotationHelper.Preview();
			Assert("Preview should be successful", result);
			AssertEquals("Job existing from before was deleted", true, job.IsDeleted);
			AssertNotNull("New job was created during preview", SpotQuote.Job);
		}

		#endregion

		#region TestPreviewCoreWillClearJobDataIfAnyJobExistsFromBefore
		public void TestPreviewCoreWillClearJobDataIfAnyJobExistsFromBefore()
		{
			QuotationHelper.Preview();
			var job = SpotQuote.Job;
			AssertNotNull("A job was created during preview", job);

			job.JH_Description = "Test";
			AssertEquals("The description of job is Test", "Test", job.JH_Description);

			QuotationHelper.Preview();
			AssertEquals("The description of job is empty because  the job has been deactivated and activated", string.Empty, job.JH_Description);
			AssertNotNull("New job was created during preview Core", SpotQuote.Job);
		}

		#endregion

		#region TestCurrentBranchFallback

		[ExpectNoExceptions()]
		public void TestCurrentBranchFallbackWhenPreviewing()
		{
			var branchRule = GetBranchDefaultOrderRule();
			using (AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, branchRule))
			{
				QuotationHelper.Preview();
				AssertEquals(GlbBranch.CurrentBranch.PK, QuotationHelper.SpotQuote.Job.JH_GB);
			}
		}

		[ExpectNoExceptions()]
		public void TestCurrentBranchFallbackWhenFinalising()
		{
			var branchRule = GetBranchDefaultOrderRule();
			using (AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, branchRule))
			{
				QuotationHelper.Finalise();
				AssertEquals(GlbBranch.CurrentBranch.PK, QuotationHelper.SpotQuote.Job.JH_GB);
			}
		}

		IJobBranchDefaultOrderRule GetBranchDefaultOrderRule()
		{
			return new JobBranchDefaultOrderRule()
			{
				DefaultToBlank = 1,
				DefaultToBranchOfOrganisation = 0,
				DefaultToBranchRelatedToPortOrWarehouseBranch = 0,
				DefaultToLoginUserDefault = 0
			};
		}

		#endregion

		#region TestPreviewAfterFinalise

		public void TestPreviewAfterFinalise()
		{
			TestFinalise();
			bool result = QuotationHelper.Preview();
			Assert("Preview should be successful", result);
		}

		public void TestMultipleFinalise()
		{
			TestFinalise();
			QuotationHelper.QuoteUpdated += new EventHandler(QuotationHelper_QuoteUpdated);
			QuoteUpdatedHandlerCalled = false;
			bool result = QuotationHelper.Finalise();
			Assert("Finalise should be successful", result);
			AssertEquals("QuoteUpdatedHandler should have been called", true, QuoteUpdatedHandlerCalled);
		}

		void AssertQuoteInitialState(QuotedBooking spotQuote)
		{
			AssertEquals("Charges should not be calculated at this point", 0m, QuotationHelper.TotalSellAmount);
			AssertEquals("No Charges should be calculated", 0, QuotationHelper.JobCharges.Count);
			AssertEquals("No Emails should be waiting to be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Quotation should not have any eDocs", 0, ((IDocManagerSupport)spotQuote.Quote).DocManagerInfo.Documents.Count);
		}

		void AssertFinalise(QuotedBooking spotQuote)
		{
			var emailFormat = new EmailFormat();
			emailFormat.EmailSubjectFields.RemoveAndDeleteAll();
			emailFormat.EmailSubjectFields.Add(new EmailSubjectField("1", Constants.EmailFormat.EmailFieldCodes.CompanyBrandName));
			emailFormat.EmailSubjectFields.Add(new EmailSubjectField("2", Constants.EmailFormat.EmailFieldCodes.DocumentName));

			DocumentsDataRegistry.Instance.EmailFormat.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, emailFormat);

			QuotationHelper.QuoteUpdated += new EventHandler(QuotationHelper_QuoteUpdated);
			bool result = QuotationHelper.Finalise();
			AssertEquals("Finalise should have been successful", true, result);
			AssertEquals("Quote should be locked", true, spotQuote.Quote.TH_IsLocked);
			Assert("Quote Updated Handler should have been called", QuoteUpdatedHandlerCalled);
			AssertEquals("Charges should be calculated", 1, ((Job)spotQuote.Job).Charges.Count);
			AssertEquals("Total Amount", 100m, QuotationHelper.TotalSellAmount);

			AssertEquals("Notification Email should be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef notificationEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertNotNull("Notification Email", notificationEmail);
			string expectedEmailSubject = String.Format("Web Spot Quote {0} for client {1}", spotQuote.Quote.TH_QuoteNumber, spotQuote.Quote.Header.OH_FullName);
			AssertEquals("Notification Email Subject", expectedEmailSubject, notificationEmail.Subject);
			AssertEquals("Notification Email should have only been sent to Sales Rep", 1, notificationEmail.Recipients.Count);
			AssertEquals("Notification Email should be sent to Default Sales Rep", "SalesRep@QuotationCo", notificationEmail.Recipients[0]);

			StmPrintJob printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, SQLComparisonOperator.Equal, spotQuote.Quote.PK));
			AssertNotNull("Expected print job to be scheduled", printJob);
			AssertEquals("Print job should be email", "EML", printJob.SP_JobType);
			AssertEquals("Attachment should be PDF", "PDF", printJob.SP_EmailAttachmentFormat);
			AssertEquals("Email Destination", "test@cargowise.com", printJob.SP_Destination);
			AssertEquals("Email Subject Line", String.Format("{0} - Quotation - {1} - {2}", spotQuote.Quote.Company.GC_Name, spotQuote.Quote.TH_QuoteNumber, spotQuote.Quote.Header.OH_FullName).TrimEnd(), printJob.SP_EmailSubjectLine);
		}

		#endregion

		#region TestPreviewOnEmptyQuote

		[ExpectNoExceptions]
		public void TestPreviewOnEmptyQuote()
		{
			SpotQuote.Mode = "";
			SpotQuote.Origin = "";
			SpotQuote.Destination = "";
			QuotationHelper.Preview();

			Assert("Should indicate error on TransportMode", SpotQuote.ModeInfo.HasErrors());
			Assert("Should indicate error on Origin", SpotQuote.OriginInfo.HasErrors());
			Assert("Should indicate error on Destination", SpotQuote.DestinationInfo.HasErrors());
		}

		#endregion

		#region TestPreviewWithRateBasedOnZipCodes

		public void TestPreviewWithRateBasedOnZipCodes()
		{
			RefDomesticCartageZone zone1 = Factory.New<RefDomesticCartageZone>();
			zone1.F1_CityTownPostCode = "90028";
			zone1.F1_Zone = "A";
			zone1.F1_Distance = 1;
			zone1.F1_RL_NKLoco = "USBUR";

			RefDomesticCartageZone zone2 = Factory.New<RefDomesticCartageZone>();
			zone2.F1_CityTownPostCode = "21001";
			zone2.F1_Zone = "C";
			zone2.F1_Distance = 1;
			zone2.F1_RL_NKLoco = "USBWI";

			Rate.AIRRateEntriesForBinding[0].TI_OriginLRC = "USBUR";
			Rate.AIRRateEntriesForBinding[0].TI_CartagePickupAddressPostCode = "90028";
			Rate.AIRRateEntriesForBinding[0].TI_DestinationLRC = "USBWI";
			Rate.AIRRateEntriesForBinding[0].TI_CartageDeliveryAddressPostCode = "21001";

			Factory.Save();

			QuotedBooking spotQuote1 = GetQuotedBookingForTestPreviewWithRateBasedOnZipCodes();
			spotQuote1.QuotedBookingNumber = "Q0000001";
			TrackingQuotationHelper helper1 = new TrackingQuotationHelper(Factory, WebTestHelper.TestSiteUser.LoggedInOrgContact, spotQuote1);
			Assert("Preview is not available if GetQuotesBasedOnPostalCodes = false (default value)", !helper1.Preview());

			WebDataRegistry.Instance.GetQuotesBasedOnPostalCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			QuotedBooking spotQuote2 = GetQuotedBookingForTestPreviewWithRateBasedOnZipCodes();
			spotQuote2.QuotedBookingNumber = "Q0000002";
			TrackingQuotationHelper helper2 = new TrackingQuotationHelper(Factory, WebTestHelper.TestSiteUser.LoggedInOrgContact, spotQuote2);
			Assert("Preview is available if GetQuotesBasedOnPostalCodes = true", helper2.Preview());
		}

		QuotedBooking GetQuotedBookingForTestPreviewWithRateBasedOnZipCodes()
		{
			QuotedBooking spotQuote = TrackingQuotedBooking.GetNewQuotation(Factory, WebTestHelper.TestSiteUser);
			spotQuote.Mode = "LSE";

			spotQuote.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			spotQuote.ConsignorDocumentaryAddress.E2_City = "";
			spotQuote.ConsignorDocumentaryAddress.E2_State = "";
			spotQuote.ConsignorDocumentaryAddress.E2_RN_NKCountryCode = "";
			spotQuote.ConsignorDocumentaryAddress.E2_Postcode = "90028";
			spotQuote.Origin = "USBUR";

			spotQuote.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			spotQuote.ConsigneeDocumentaryAddress.E2_City = "";
			spotQuote.ConsigneeDocumentaryAddress.E2_State = "";
			spotQuote.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode = "";
			spotQuote.ConsigneeDocumentaryAddress.E2_Postcode = "21001";
			spotQuote.Destination = "USBWI";

			spotQuote.VolumeUnit = Constants.Volume.Litre;
			spotQuote.Volume = 20m;

			return spotQuote;
		}

		#endregion

		#region TestPreviewWithoutRates

		public void TestPreviewWithoutRates()
		{
			Rate.Delete();

			Assert("Preview failed if no rates exist", !QuotationHelper.Preview());

			WebDataRegistry.Instance.SaveQuotesWithoutRates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Assert("Preview is available if SaveQuotesWithoutRates = true", QuotationHelper.Preview());
		}

		#endregion

		#region TestPreviewNegativeCharges

		public void TestPreviewNegativeCharges()
		{
			var freightCharge = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
			var destinationCharge = SetupChargeCode("STGTST", "Storage Fee", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination);

			var carrier = SetupCarrier("Carrier");
			SetupRate(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "HKHKG", "AUSYD", freightCharge, 100m, carrier);
			SetupRate(RatingConstants.RateCategory.DST, Constants.RateMode.LSE, string.Empty, "AUSYD", destinationCharge, -100m); // is this setting both CostAmt and SellAmt?

			Assert("Preview is available if negative charges exist", QuotationHelper.Preview());
		}

		#endregion

		#region TestPreviewNoCost

		public void TestPreviewNoCost()
		{
			var freightCharge = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
			var destinationCharge = SetupChargeCode("STGTST", "Storage Fee", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination);

			var carrier = SetupCarrier("Carrier");
			var rate = SetupRate(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "HKHKG", "AUSYD", freightCharge, 100m, carrier);
			rate.RateLines[0].ChargeCode.AC_MarginPercentage = 0;

			Assert("Preview is available if negative charges exist", QuotationHelper.Preview());
		}

		#endregion

		#region TestFinalizeWithoutRates

		public void TestFinalizeWithoutRates()
		{
			Rate.Delete();

			Assert("Finalize failed if no rates exist", !QuotationHelper.Finalise());

			WebDataRegistry.Instance.SaveQuotesWithoutRates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Assert("Finalize is available if SaveQuotesWithoutRates = true", QuotationHelper.Finalise());
		}

		#endregion

		#region TestFinalizeNegativeCharges

		public void TestFinalizeNegativeCharges()
		{
			var freightCharge = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
			var destinationCharge = SetupChargeCode("STGTST", "Storage Fee", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination);

			var carrier = SetupCarrier("Carrier");
			SetupRate(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "HKHKG", "AUSYD", freightCharge, 100m, carrier);
			SetupRate(RatingConstants.RateCategory.DST, Constants.RateMode.LSE, string.Empty, "AUSYD", destinationCharge, -100m);

			Assert("Finalize is available if negative charges exist", QuotationHelper.Finalise());
		}

		#endregion

		#region TestFinalizeNoCost

		public void TestFinalizeNoCost()
		{
			var freightCharge = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
			var destinationCharge = SetupChargeCode("STGTST", "Storage Fee", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination);

			var carrier = SetupCarrier("Carrier");
			var rate = SetupRate(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "HKHKG", "AUSYD", freightCharge, 100m, carrier);
			rate.RateLines[0].ChargeCode.AC_MarginPercentage = 0;

			Assert("Finalize is available if there is no cost", QuotationHelper.Finalise());
		}

		#endregion

		#region TestFinalizeWithoutSiteUser

		public void TestFinalizeWithoutSiteUser()
		{
			var loggedInOrg = WebTestHelper.TestOrg;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_Code = "CONSIGNEE";

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_Code = "CONSIGNOR";

			var clientRate = AddClientRateForTestThirdParty(loggedInOrg, consignee, consignor, GlbCompany.CurrentCompany, Rate, (ZDecimal)5m);
			var quotedBooking = GetQuotedBookingForTestThirdParty(consignee, consignor);
			var quotationHelper = new TrackingQuotationHelper(Factory, null, quotedBooking);
			quotationHelper.SpotQuote.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			quotationHelper.SpotQuote.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			AssertNoExceptionThrown("Finalize failed ", () => { quotationHelper.Finalise(); });
		}

		#endregion

		#region TestPreviewWithMultipleTransportModesInComparisonMode

		public void TestPreviewWithMultipleTransportModesInComparisonMode()
		{
			var airLseModeIndex = -1;
			var seaLseModeIndex = -1;
			for (var i = 0; i < SpotQuote.ComparisonModes.Count; i++)
			{
				if (SpotQuote.ComparisonModes[i].Code == Constants.ContainerModes.Loose)
				{
					airLseModeIndex = i;
				}
				if (SpotQuote.ComparisonModes[i].Code == Constants.ContainerModes.LCL)
				{
					seaLseModeIndex = i;
				}
			}

			AssertNotEquals(airLseModeIndex, -1);
			AssertNotEquals(seaLseModeIndex, -1);

			SpotQuote.IsCompareMode = true;
			SpotQuote.ComparisonModes[airLseModeIndex].Bool = true;
			SpotQuote.ComparisonModes[seaLseModeIndex].Bool = true;

			var newRateEntry = Rate.AddRateEntry("LCL", Constants.ContainerModes.LCL, "HKHKG", "AUSYD");
			newRateEntry.RateLines[0].TL_RateCalculator = UnitCalculator.Code;
			newRateEntry.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;
			newRateEntry.RateLines[0].TL_WeightVolume = Constants.Volume.Litre;

			WebDataRegistry.Instance.SaveQuotesWithoutRates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert("Preview fails if more than one rate entry exists", !QuotationHelper.Preview());
			AssertEquals("Comparison Quote Results", 2, SpotQuote.ComparisonQuoteResults.Count);

			WebDataRegistry.Instance.SaveQuotesWithoutRates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert("Preview fails if more than one rate entry exists", !QuotationHelper.Preview());
			AssertEquals("Comparison Quote Results", 2, SpotQuote.ComparisonQuoteResults.Count);

			newRateEntry.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)0m;
			Assert("Preview is available if only one non zero rate entry exists", QuotationHelper.Preview());

			newRateEntry.Delete();
			Assert("Preview is available if only one rate entry exists", QuotationHelper.Preview());
		}

		#endregion

		#region TestCharges

		public void TestDestinationChargeWithSingleCarriersFreightCharge()
		{
			var freightCharge = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
			var destinationCharge = SetupChargeCode("STGTST", "Storage Fee", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination);

			var carrier = SetupCarrier("Carrier");
			SetupRate(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "USORD", "USLAX", freightCharge, 100m, carrier);

			SetupRate(RatingConstants.RateCategory.DST, Constants.RateMode.LSE, string.Empty, "USLAX", destinationCharge, 30m);

			Factory.Save();

			var spotQuote = CreateSpotQuote();
			var quotationHelper = new TrackingQuotationHelper(Factory, WebTestHelper.TestSiteUser.LoggedInOrgContact, spotQuote);
			spotQuote.Origin = "USORD";
			spotQuote.Destination = "USLAX";
			spotQuote.Mode = Constants.RateMode.LSE;
			spotQuote.Weight = 10m;
			spotQuote.WeightUnit = Constants.Weight.Kilograms;
			spotQuote.VolumeUnit = Constants.Volume.Litre;
			spotQuote.Volume = 0m;

			using (spotQuote.GetValidationSuspender())
			using (DataRegistryRating.Instance.ProposeSimilarRates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert("Preview should return true", quotationHelper.Preview());
				var comparisonQuotes = spotQuote.ComparisonQuoteResults.Cast<ComparisonQuoteResult>();
				AssertEquals("Should return 1 result", 1, comparisonQuotes.Count());

				var quote = comparisonQuotes.First();
				AssertNotNull(quote);
				Assert(quote.Selected == ZBool.True);

				AssertEquals(1300m, quote.Charges.TotalOSSellAmount);
				var quoteCharges = quote.Charges.Cast<ComparisonQuoteCharge>();
				AssertEquals(2, quoteCharges.Count());
				Assert(quoteCharges.Any(c => c.Description == freightCharge.AC_Desc && c.OSSellAmount == 1000m));
				Assert(quoteCharges.Any(c => c.Description == destinationCharge.AC_Desc && c.OSSellAmount == 300m));
			}
		}

		public void TestDestinationChargeWithMultipleCarriersFreightCharges()
		{
			var freightCharge = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
			var destinationCharge = SetupChargeCode("STGTST", "Storage Fee", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination);

			var carrier1 = SetupCarrier("Carrier1");
			SetupRate(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "USORD", "USLAX", freightCharge, 100m, carrier1);

			var carrier2 = SetupCarrier("Carrier2");
			SetupRate(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "USORD", "USLAX", freightCharge, 150m, carrier2);

			SetupRate(RatingConstants.RateCategory.DST, Constants.RateMode.LSE, string.Empty, "USLAX", destinationCharge, 30m);

			Factory.Save();

			var spotQuote = CreateSpotQuote();
			var quotationHelper = new TrackingQuotationHelper(Factory, WebTestHelper.TestSiteUser.LoggedInOrgContact, spotQuote);
			spotQuote.Origin = "USORD";
			spotQuote.Destination = "USLAX";
			spotQuote.Mode = Constants.RateMode.LSE;
			spotQuote.Weight = 10m;
			spotQuote.WeightUnit = Constants.Weight.Kilograms;
			spotQuote.VolumeUnit = Constants.Volume.Litre;
			spotQuote.Volume = 0m;

			using (spotQuote.GetValidationSuspender())
			using (DataRegistryRating.Instance.ProposeSimilarRates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert("Preview should return false", !quotationHelper.Preview());
				var comparisonQuotes = spotQuote.ComparisonQuoteResults.Cast<ComparisonQuoteResult>();
				AssertEquals("Should return 2 comparison results", 2, comparisonQuotes.Count());

				var carrier1Quote = comparisonQuotes.FirstOrDefault(q => q.CarrierPK == carrier1.PK);
				AssertNotNull(carrier1Quote);
				Assert(carrier1Quote.Selected == ZBool.False);

				var carrier2Quote = comparisonQuotes.FirstOrDefault(q => q.CarrierPK == carrier2.PK);
				AssertNotNull(carrier2Quote);
				Assert(carrier2Quote.Selected == ZBool.False);

				AssertEquals(1300m, carrier1Quote.Charges.TotalOSSellAmount);
				var carrier1Charges = carrier1Quote.Charges.Cast<ComparisonQuoteCharge>();
				AssertEquals(2, carrier1Charges.Count());
				Assert(carrier1Charges.Any(c => c.Description == freightCharge.AC_Desc && c.OSSellAmount == 1000m));
				Assert(carrier1Charges.Any(c => c.Description == destinationCharge.AC_Desc && c.OSSellAmount == 300m));

				AssertEquals(1800m, carrier2Quote.Charges.TotalOSSellAmount);
				var carrier2Charges = carrier2Quote.Charges.Cast<ComparisonQuoteCharge>();
				AssertEquals(2, carrier2Charges.Count());
				Assert(carrier2Charges.Any(c => c.Description == freightCharge.AC_Desc && c.OSSellAmount == 1500m));
				Assert(carrier2Charges.Any(c => c.Description == destinationCharge.AC_Desc && c.OSSellAmount == 300m));
			}
		}

		public void TestFreightChargeWithMultipleCarriersFreightCharges()
		{
			var freightCharge = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
			var destinationCharge = SetupChargeCode("STGTST", "Storage Fee", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination);

			var carrier1 = SetupCarrier("Carrier1");
			SetupRate(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "USORD", "USLAX", freightCharge, 100m, carrier1);

			var carrier2 = SetupCarrier("Carrier2");
			SetupRate(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "USORD", "USLAX", freightCharge, 150m, carrier2);

			SetupRate(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "USORD", "USLAX", freightCharge, 200m);
			SetupRate(RatingConstants.RateCategory.DST, Constants.RateMode.LSE, string.Empty, "USLAX", destinationCharge, 30m);

			Factory.Save();

			var spotQuote = CreateSpotQuote();
			var quotationHelper = new TrackingQuotationHelper(Factory, WebTestHelper.TestSiteUser.LoggedInOrgContact, spotQuote);
			spotQuote.Origin = "USORD";
			spotQuote.Destination = "USLAX";
			spotQuote.Mode = Constants.RateMode.LSE;
			spotQuote.Weight = 10m;
			spotQuote.WeightUnit = Constants.Weight.Kilograms;
			spotQuote.VolumeUnit = Constants.Volume.Litre;
			spotQuote.Volume = 0m;

			using (spotQuote.GetValidationSuspender())
			using (DataRegistryRating.Instance.ProposeSimilarRates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert("Preview should return true", quotationHelper.Preview());
				var comparisonQuotes = spotQuote.ComparisonQuoteResults.Cast<ComparisonQuoteResult>();
				AssertEquals("Should return 1 result", 1, comparisonQuotes.Count());

				var quote = comparisonQuotes.First();
				AssertNotNull(quote);
				Assert(quote.Selected == ZBool.True);

				AssertEquals(2300m, quote.Charges.TotalOSSellAmount);
				var quoteCharges = quote.Charges.Cast<ComparisonQuoteCharge>();
				AssertEquals(2, quoteCharges.Count());
				Assert(quoteCharges.Any(c => c.Description == freightCharge.AC_Desc && c.OSSellAmount == 2000m));
				Assert(quoteCharges.Any(c => c.Description == destinationCharge.AC_Desc && c.OSSellAmount == 300m));
			}
		}

		OrgHeader SetupCarrier(string code)
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsAirLine = true;
			carrier.OH_IsShippingProvider = true;
			carrier.OH_Code = code;

			return carrier;
		}

		RateEntry SetupRate(string category, string mode, string origin, string destination, AccChargeCode charge, ZDecimal value, OrgHeader carrier = null)
		{
			var entry = Rate.AddRateEntry(category, mode, origin, destination);
			entry.TI_OH_TransportProvider = carrier?.PK ?? ZGuid.Empty;
			entry.TI_RX_NKCurrency = "USD";

			if (entry.RateLines.Count == 0)
			{
				entry.RateLines.AddNew();
			}

			entry.RateLines[0].TL_AC = charge.PK;
			entry.RateLines[0].TL_RateCalculator = UnitCalculator.Code;
			entry.RateLines[0].RateLineItems[0].TM_Value = value;
			entry.RateLines[0].TL_WeightVolume = Constants.Weight.Kilograms;

			return entry;
		}

		AccChargeCode SetupChargeCode(ZString code, ZString description, ZString calculator, string chargeGroup = null)
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeType = Constants.ChargeType.Margin;
			chargeCode.AC_MarginPercentage = 100m;
			chargeCode.AC_Code = code;
			chargeCode.AC_Desc = description;
			chargeCode.AC_RateCalculator = calculator;
			chargeCode.AC_ChargeGroup = chargeGroup ?? ChargeCodeGroupList.Codes.Freight;
			chargeCode.AC_ShowOnQuotation = true;
			chargeCode.AC_SuppressOnQuoteIfZero = true;
			chargeCode.SetGLAccountDataForTesting();

			return chargeCode;
		}

		#endregion

		#region TestPreviewDoesNotChangeIncoTerms

		public void TestPreviewDoesNotChangeIncoTerms()
		{
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "C1";
			consignee.OH_IsConsignee = true;

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "C2";
			consignor.OH_IsConsignor = true;

			SpotQuote.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			SpotQuote.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			SpotQuote.Origin = "USORD";
			SpotQuote.Destination = "AUSYD";

			OrgSupplierBuyerLink link = SpotQuote.Consignor.SupplierLinks.AddNew(SpotQuote.Consignee);
			link.OL_RN_NKImporterCountry = Constants.CountryCodes.UnitedStates;
			OrgSupBuyLinkTrnMode mode = link.OrgSupBuyLinkTrnModes.AddNew();
			mode.PF_TransportMode = Constants.TransportModes.Air;
			mode.PF_ContainerMode = Constants.ContainerModes.Loose;
			mode.PF_IncoTerm = Constants.IncoTerms.CostAndFreight;

			SpotQuote.TransportMode = Constants.TransportModes.Air;
			SpotQuote.PaymentTerms = Constants.IncoTerms.CarriagePaidTo;

			WebDataRegistry.Instance.SaveQuotesWithoutRates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert("Preview should return true", QuotationHelper.Preview());

			AssertEquals(Constants.TransportModes.Air, SpotQuote.TransportMode);
			AssertEquals(Constants.IncoTerms.CarriagePaidTo, SpotQuote.PaymentTerms);
		}

		#endregion

		#region TestThirdPartyQuote

		public void TestThirdPartyQuote()
		{
			var loggedInOrg = WebTestHelper.TestOrg;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_Code = "CONSIGNEE";

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_Code = "CONSIGNOR";

			AddClientRateForTestThirdParty(loggedInOrg, consignee, consignor, GlbCompany.CurrentCompany, Rate, (ZDecimal)5m);
			var quotedBooking = GetQuotedBookingForTestThirdParty(consignee, consignor);
			var quotationHelper = new TrackingQuotationHelper(Factory, WebTestHelper.TestSiteUser.LoggedInOrgContact, quotedBooking);
			quotationHelper.SpotQuote.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			quotationHelper.SpotQuote.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			Assert("Preview should return true", quotationHelper.Preview());
			AssertEquals("The Local Client should be the same as the Logged In Client", quotedBooking.Job.LocalChargesPK, loggedInOrg.PK);
			AssertEquals("The Total Sell Amount was not calculated correctly", quotationHelper.TotalSellAmount, (ZDecimal)100m);

			using (var quoteBranch = new WebLoginBranch(Company2.Branches[0]))
			{
				loggedInOrg.OH_IsDebtor = true; // the org should also be a debtor in this new login company
				AddClientRateForTestThirdParty(loggedInOrg, consignee, consignor, Company2, Rate2, (ZDecimal)6m);
			}

			quotedBooking = GetQuotedBookingForTestThirdParty(consignee, consignor);
			quotedBooking.Quote.TH_GC = Company2.PK;

			quotationHelper = new TrackingQuotationHelper(Factory, WebTestHelper.TestSiteUser.LoggedInOrgContact, quotedBooking);

			Assert("Preview should return true", quotationHelper.Preview());
			AssertEquals("The Total Sell Amount was not calculated correctly", quotationHelper.TotalSellAmount, (ZDecimal)120m);
		}

		QuotedBooking GetQuotedBookingForTestThirdParty(OrgHeader consignee, OrgHeader consignor)
		{
			var spotQuote = TrackingQuotedBooking.GetNewQuotation(Factory, WebTestHelper.TestSiteUser);

			spotQuote.Mode = "LSE";
			spotQuote.TransportMode = Constants.TransportModes.Air;
			spotQuote.PaymentTerms = Constants.IncoTerms.CostAndFreight;

			spotQuote.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			spotQuote.Origin = "USCHI";

			spotQuote.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			spotQuote.Destination = "HKHKG";

			spotQuote.VolumeUnit = Constants.Volume.Litre;
			spotQuote.Volume = 20m;

			return spotQuote;
		}

		ClientRate AddClientRateForTestThirdParty(OrgHeader testOrg, OrgHeader consignee, OrgHeader consignor, GlbCompany company, ClientRate rate, ZDecimal calculator)
		{
			var clientRate = rate;
			clientRate.TH_OH = testOrg.PK;
			clientRate.TH_GC = company.PK;

			var entry = clientRate.AddRateEntry("AIR", "LSE", "USCHI", "HKHKG");
			var line = entry.RateLines[0];
			line.TL_RateCalculator = UnitCalculator.Code;
			line.Calculator[Calculator.Items.Operator.UNT] = calculator;
			line.TL_WeightVolume = Constants.Volume.Litre;

			clientRate.AIRRateEntriesForBinding[0].TI_OH_Consignee = consignee.PK;
			clientRate.AIRRateEntriesForBinding[0].TI_OH_Consignor = consignor.PK;
			Factory.Save();

			return clientRate;
		}

		#endregion

		public void TestPreview_JobHasChargesAndRateLineHasCuerencyWithoutExchangeRate_AddEmptyExchangeRateToJob()
		{
			var uah = RefCurrency.LoadFromCurrencyCode(Factory, Constants.CurrencyCodes.Ukraine);
			CreateExchangeRate(uah, 0.0m, Constants.ExchangeRateTypes.Code.BuyRate, GlbCompany.CurrentCompany);
			CreateExchangeRate(uah, 0.0m, Constants.ExchangeRateTypes.Code.SellRate, GlbCompany.CurrentCompany);

			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_IsDebtor = true;
			client.CompanyData.OB_RX_NKARDDefltCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = client.PK;

			var entry = clientRate.AddRateEntry("AIR", "LSE", "UAIEV", "AUSYD");
			var line = entry.RateLines[0];
			line.TL_RateCalculator = MinimumCalculator.Code;
			line.TL_WeightVolume = Constants.Volume.Litre;
			line.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)5m;

			var spotQuote = TrackingQuotedBooking.GetNewQuotation(Factory, WebTestHelper.TestSiteUser);
			spotQuote.ClientPK = client.PK;
			spotQuote.Mode = "LSE";
			spotQuote.Origin = "UAIEV";
			spotQuote.Destination = "AUSYD";
			spotQuote.VolumeUnit = Constants.Volume.Litre;
			spotQuote.Volume = 20m;

			Factory.Save();

			spotQuote.TryLoadOrCreateJob();
			var jobHeader = spotQuote.Job;
			jobHeader.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var job = new Job.Loader(spotQuote).Load();
			var charge = job.Charges.AddNew();
			charge.JR_AC = line.TL_AC;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_LocalSellAmt = 5m;
			var invoiceLine = (ARInvoiceLine)Factory.NewWithValidTestData<ARInvoice>().Lines.AddNew();
			invoiceLine.AL_LineType = TransactionLineTypes.Revenue;
			invoiceLine.AL_OSAmount = charge.JR_LocalSellAmt;
			invoiceLine.AL_LineAmount = charge.JR_LocalSellAmt;
			invoiceLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			charge.JR_AL_ARLine = invoiceLine.PK;

			Factory.Save();

			var helperToTest = new TrackingQuotationHelper(Factory, WebTestHelper.TestSiteUser.LoggedInOrgContact, spotQuote);
			helperToTest.Preview();

			var uahCurrency = (new RefCurrencyCollection(Factory)).FirstOrDefault(c => c.RX_Code == "UAH");
			AssertNotNull("Should add missing exchange rate to job", uahCurrency);
			AssertEquals("Sell rate should be empty", 0m, uahCurrency.CurrentSellRate);
			AssertEquals("Buy rate should be empty", 0m, uahCurrency.CurrentBuyRate);
		}

		#region Setup & Implementation

		protected override void SetUp()
		{
			base.SetUp();

			Company2 = new BusinessObjectFactory().NewWithValidTestData<GlbCompany>();
			Company2.Branches.Add(Company2.Factory.NewWithValidTestData<GlbBranch>());
			Company2.Factory.Save();
			Company2 = Factory.Load<GlbCompany>(Company2.PK);

			RefCurrency hKD = RefCurrency.LoadFromCurrencyCode(Factory, Constants.CurrencyCodes.HongKong);
			CreateExchangeRate(hKD, 0.8m, Constants.ExchangeRateTypes.Code.BuyRate, GlbCompany.CurrentCompany);
			CreateExchangeRate(hKD, 0.8m, Constants.ExchangeRateTypes.Code.SellRate, GlbCompany.CurrentCompany);

			RefCurrency uSD = RefCurrency.LoadFromCurrencyCode(Factory, Constants.CurrencyCodes.UnitedStates);
			CreateExchangeRate(uSD, 0.7m, Constants.ExchangeRateTypes.Code.BuyRate, GlbCompany.CurrentCompany);
			CreateExchangeRate(uSD, 0.7m, Constants.ExchangeRateTypes.Code.SellRate, GlbCompany.CurrentCompany);

			CreateExchangeRate(hKD, 0.8m, Constants.ExchangeRateTypes.Code.BuyRate, Company2);
			CreateExchangeRate(hKD, 0.8m, Constants.ExchangeRateTypes.Code.SellRate, Company2);

			CreateExchangeRate(uSD, 0.7m, Constants.ExchangeRateTypes.Code.BuyRate, Company2);
			CreateExchangeRate(uSD, 0.7m, Constants.ExchangeRateTypes.Code.SellRate, Company2);

			Factory.Save();
			ExchangeRateReader.GetReaderInstance().ClearCache();

			AssertEquals("Precondition: HKD Exchanges Rates were set up correctly", 0.8m,
			Env.CurrentCompany.ExchangeRate.TodaysRate(hKD.RX_Code, ExchangeRateType.Sell));

			AssertEquals("Precondition: USD Exchanges Rates were set up correctly", 0.7m,
			Env.CurrentCompany.ExchangeRate.TodaysRate(uSD.RX_Code, ExchangeRateType.Sell));

			WebTestHelper = new TestHelper(Factory);

			WebTestHelper.TestOrg.FillWithValidTestData();
			WebTestHelper.TestOrg.OH_IsDebtor = true;
			WebTestHelper.TestOrg.OH_IsConsignor = true;
			WebTestHelper.TestOrg.CompanyData.OB_RX_NKARDDefltCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			Rate = CreateClientRate(WebTestHelper.TestOrg);
			using (var quoteBranch = new WebLoginBranch(Company2.Branches[0]))
			{
				Rate2 = CreateClientRate(WebTestHelper.TestOrg);
			}

			WebTestHelper.TestSiteUser.Login(WebTestHelper.TestOrg.OH_Code, WebTestHelper.TestContact.OC_Email, WebTestHelper.TestContact.PasswordForTesting);
			AssertEquals("Precondition: TestSiteUser should be logged in", true, WebTestHelper.TestSiteUser.IsLoggedIn);

			SpotQuote = CreateSpotQuote();
			QuotationHelper = new TrackingQuotationHelper(Factory, WebTestHelper.TestSiteUser.LoggedInOrgContact, SpotQuote);

			QuoteUpdatedHandlerCalled = false;
		}

		RefExchangeRate CreateExchangeRate(RefCurrency currency, ZDecimal rate, ZString type, GlbCompany company)
		{
			RefExchangeRate exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
			exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
			exchangeRate.RE_ExRateType = type;
			exchangeRate.RE_SellRate = rate;
			exchangeRate.RE_RX_NKExCurrency = currency.RX_Code;
			exchangeRate.RE_GC = company.PK;
			return exchangeRate;
		}

		ClientRate Rate;
		QuotedBooking SpotQuote;
		TestHelper WebTestHelper;
		TrackingQuotationHelper QuotationHelper;
		GlbCompany Company2;
		ClientRate Rate2;

		ClientRate CreateClientRate(OrgHeader testOrg)
		{
			ClientRate clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = testOrg.PK;

			RateEntry entry = clientRate.AddRateEntry("AIR", "LSE", "HKHKG", "AUSYD");
			RateLine line = entry.RateLines[0];
			line.TL_RateCalculator = UnitCalculator.Code;
			line.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;
			line.TL_WeightVolume = Constants.Volume.Litre;
			Factory.Save();

			return clientRate;
		}

		QuotedBooking CreateSpotQuote()
		{
			var quotationRep = Factory.NewWithValidTestData<GlbStaff>();
			quotationRep.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			quotationRep.GS_EmailAddress = "SalesRep@QuotationCo";
			RatingDataRegistry.Instance.DefaultSalesRepresentative.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, quotationRep.PK.ToGuid());

			Factory.Save();

			var fSpotQuote = TrackingQuotedBooking.GetNewQuotation(Factory, WebTestHelper.TestSiteUser);
			fSpotQuote.ClientPK = WebTestHelper.TestOrg.PK;
			fSpotQuote.Mode = "LSE";
			fSpotQuote.Origin = "HKHKG";
			fSpotQuote.Destination = "AUSYD";
			fSpotQuote.VolumeUnit = Constants.Volume.Litre;
			fSpotQuote.Volume = 20m;

			JobHeader job = new Job.Loader(fSpotQuote).Load();
			AssertNull("Precondition: Job should not be created by the test. This should be created automatically when previewing or saving the quote.", job);

			return fSpotQuote;
		}

		#endregion

		#region QuoteUpdated Event Handler

		void QuotationHelper_QuoteUpdated(object sender, EventArgs e)
		{
			QuoteUpdatedHandlerCalled = true;
		}
		bool QuoteUpdatedHandlerCalled;

		#endregion

	}
}
