using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.RatingTests.Testing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.QuotedBookings.GUI.Test.QuotedBookingFormTest;
using static Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement;

namespace Enterprise.Rating.GUI.Testing
{
	public class QuoteTest : BaseRatingIntegrationTest
	{
		#region TestQuotationFinalisedPrinted

		public void TestQuotationFinalisedPrinted()
		{
			Quote testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			testQuote.NoTradeLanesToPrint += new EventHandler(TestQuote_NoTradeLanesToPrint);

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = testQuote.PK;
			job.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_JH = job.PK;

			Factory.Save();

			RatingHeaderDocumentSupporter supporter = (RatingHeaderDocumentSupporter)((IDocumentSupportable)testQuote).DocumentSupporter;
			NoTradeLanesToPrintCalled = false;
			PrintTask task = supporter.BuildPrintTask(null);
			AssertNull(task);
			Assert(NoTradeLanesToPrintCalled);

			testQuote.TH_OneTimeQuote = true;
			RateOneOffShipment oneOffQuote = testQuote.CurrentOneOffQuote;
			oneOffQuote.TT_TransportMode = TransportModes.Air;
			oneOffQuote.TT_ContainerMode = ContainerModes.Loose;
			oneOffQuote.TT_RL_NKReceivalLocation = "AUSYD";
			oneOffQuote.TT_RL_NKDeliveryLocation = "USLAX";
			oneOffQuote.TT_ActualWeight = 100;
			oneOffQuote.TT_UnitOfWeight = QuantityUnit.KG;
			AssertEquals(0, testQuote.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.QuotationCancelled.Code)).Length);

			NoTradeLanesToPrintCalled = false;
			task = supporter.BuildPrintTask(null);
			AssertNotNull(task);
			Assert(!NoTradeLanesToPrintCalled);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			supporter.RunTask(task);
			AssertEquals(0, testQuote.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.QuotationCancelled.Code)).Length);
		}

		void TestQuote_NoTradeLanesToPrint(object sender, EventArgs e)
		{
			NoTradeLanesToPrintCalled = true;
		}

		bool NoTradeLanesToPrintCalled;

		#endregion

		#region Spot Quote Acception on a Forwarding Shipment

		public void TestAcceptingSpotQuoteOnForwardingShipment()
		{
			var spotQuote1 = CreateSpotQuoteWithJob(QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var spotQuote2 = CreateSpotQuoteWithJob(QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, NewClient.PK, "SGSIN", "AUMEL", 100m);
			var shipmentJob = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job;
			shipmentJob.JH_OA_LocalChargesAddr = NewClient.MainAddress.PK;
			Factory.Save();

			spotQuote1.CurrentOneOffQuote.TT_QuoteApprovedByManager = false;
			Factory.Save();

			AssertEquals("Pre-condition", false, spotQuote1.CurrentOneOffQuote.TT_QuoteApprovedByManager);
			AssertEquals("Pre-condition", true, spotQuote2.CurrentOneOffQuote.TT_QuoteApprovedByManager);

			using (_Rating.Start(new Mock<IAutoRatingGUIInteractor>().Object, isEqualization: false))
			{
				Mock.Get(_Rating.Interactor)
					.Setup(m => m.SelectQuote(It.IsAny<QuoteCollection>()))
					.Returns((QuoteCollection quotes) => (Quote)quotes.Single());

				var autoRater = new FreightAutoRater(new RatingContext());

				var ratingCriteria = new RatingCriteria(shipment.RatingAdapter, Factory);
				autoRater.AutoRate(ratingCriteria, CostSell.Revenue, true);

				Mock.Get(_Rating.Interactor)
					.Verify(m => m.SelectQuote(It.Is<QuoteCollection>(quotes => quotes.Single().PK == spotQuote2.PK)), Times.Once);
			}
		}

		Quote CreateSpotQuoteWithJob(QuotedBooking.QuoteState state)
		{
			var spotQuote = QuotedBooking.CreateNewQuote(Factory, state);
			spotQuote.QuotationClientAddress.OrganisationPK = NewClient.PK;
			AssertEquals("Pre-condition", true, spotQuote.TH_OneTimeQuote);

			var quotedBooking = QuotedBooking.New(spotQuote.PK, Guid.Empty, Factory);
			quotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address = Consignee.MainAddress.PK;
			quotedBooking.Mode = RateMode.LSE;
			quotedBooking.Origin = "SGSIN";
			quotedBooking.Destination = "AUMEL";
			quotedBooking.Weight = 150m;

			var job = CreateJob(spotQuote, spotQuote.TH_QuoteNumber);
			job.JH_OA_LocalChargesAddr = NewClient.MainAddress.PK;
			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_LocalSellAmt = 100m;

			return spotQuote;
		}

		#endregion

		#region Test Spot Quote Approval Limits

		public void TestSpotQuoteApprovalLimits_OnlyAboveRegistrySetting()
		{
			var collection = new PaymentThreeLevelAuthorisationSettingsCollection();
			AddRegistrySetting(collection, RangeCodes.Above, 0, AuthorisationRequirementCodes.FirstApprovalRequiredOnly);

			var user = Factory.NewWithValidTestData<GlbStaff>();
			RateSecurityTestHelper.CreateSecurityRight(Factory, user, Env.Security.OneOffQuoteApproveOneOffQuotes.Code, false);
			RateSecurityTestHelper.CreateSecurityRight(Factory, user, Env.Security.OneOffQuoteFirstLevelApproval.Code, true);
			Factory.Save();

			using (DataRegistryRating.Instance.SpotQuoteApprovalSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (DataRegistryRating.Instance.SpotQuoteRequireInternalApproval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				CombineAssertions(() =>
				{
					AssertCanApproveSpotQuoteForAmount(-1m, false, Env.Security.OneOffQuoteApproveOneOffQuotes);
					AssertCanApproveSpotQuoteForAmount(0, false, Env.Security.OneOffQuoteApproveOneOffQuotes);
					AssertCanApproveSpotQuoteForAmount(1m, true, Env.Security.OneOffQuoteFirstLevelApproval);
				});
			}
		}

		public void TestSpotQuoteApprovalLimits_OnlyUpToRegistrySetting()
		{
			var collection = new PaymentThreeLevelAuthorisationSettingsCollection();
			AddRegistrySetting(collection, RangeCodes.UpTo, 100m, AuthorisationRequirementCodes.FirstApprovalRequiredOnly);
			AddRegistrySetting(collection, RangeCodes.Above, 100m, AuthorisationRequirementCodes.SecondApprovalRequiredOnly);

			var user = Factory.NewWithValidTestData<GlbStaff>();
			RateSecurityTestHelper.CreateSecurityRight(Factory, user, Env.Security.OneOffQuoteApproveOneOffQuotes.Code, false);
			RateSecurityTestHelper.CreateSecurityRight(Factory, user, Env.Security.OneOffQuoteFirstLevelApproval.Code, true);
			Factory.Save();

			using (DataRegistryRating.Instance.SpotQuoteApprovalSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (DataRegistryRating.Instance.SpotQuoteRequireInternalApproval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				CombineAssertions(() =>
				{
					AssertCanApproveSpotQuoteForAmount(99m, true, Env.Security.OneOffQuoteFirstLevelApproval);
					AssertCanApproveSpotQuoteForAmount(100m, true, Env.Security.OneOffQuoteFirstLevelApproval);
					AssertCanApproveSpotQuoteForAmount(101m, false, Env.Security.OneOffQuoteApproveOneOffQuotes);
				});
			}
		}

		public void TestSpotQuoteApprovalLimits_UserWithGenericRights_Allowed()
		{
			var collection = SetUpSpotQuoteApprovalLimitsRegistryItems();

			var user = Factory.NewWithValidTestData<GlbStaff>();
			RateSecurityTestHelper.CreateSecurityRight(Factory, user, Env.Security.OneOffQuoteApproveOneOffQuotes.Code, true);
			Factory.Save();

			using (DataRegistryRating.Instance.SpotQuoteApprovalSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (DataRegistryRating.Instance.SpotQuoteRequireInternalApproval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				CombineAssertions(() =>
				{
					AssertCanApproveSpotQuoteForAmount(-1.1m, true, Env.Security.OneOffQuoteApproveOneOffQuotes);
					AssertCanApproveSpotQuoteForAmount(0, true, Env.Security.OneOffQuoteApproveOneOffQuotes);
					AssertCanApproveSpotQuoteForAmount(20m, true, Env.Security.None);
				});
			}
		}

		public void TestSpotQuoteApprovalLimits_UserWithGenericRights_Restricted()
		{
			var collection = SetUpSpotQuoteApprovalLimitsRegistryItems();

			var user = Factory.NewWithValidTestData<GlbStaff>();
			RateSecurityTestHelper.CreateSecurityRight(Factory, user, Env.Security.OneOffQuoteApproveOneOffQuotes.Code, false);
			Factory.Save();

			using (DataRegistryRating.Instance.SpotQuoteApprovalSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (DataRegistryRating.Instance.SpotQuoteRequireInternalApproval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				CombineAssertions(() =>
				{
					AssertCanApproveSpotQuoteForAmount(-1.1m, false, Env.Security.OneOffQuoteApproveOneOffQuotes);
					AssertCanApproveSpotQuoteForAmount(0, false, Env.Security.OneOffQuoteApproveOneOffQuotes);
					AssertCanApproveSpotQuoteForAmount(20m, true, Env.Security.None);
				});
			}
		}

		public void TestSpotQuoteApprovalLimits_UserWithLevel1ApprovalRights()
		{
			var collection = SetUpSpotQuoteApprovalLimitsRegistryItems();

			var user = Factory.NewWithValidTestData<GlbStaff>();
			RateSecurityTestHelper.CreateSecurityRight(Factory, user, Env.Security.OneOffQuoteFirstLevelApproval.Code, true);
			Factory.Save();

			using (DataRegistryRating.Instance.SpotQuoteApprovalSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (DataRegistryRating.Instance.SpotQuoteRequireInternalApproval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				CombineAssertions(() =>
				{
					AssertCanApproveSpotQuoteForAmount(20m, true, Env.Security.None);
					AssertCanApproveSpotQuoteForAmount(150m, true, Env.Security.OneOffQuoteFirstLevelApproval);
					AssertCanApproveSpotQuoteForAmount(2500m, false, Env.Security.OneOffQuoteSecondLevelApproval);
					AssertCanApproveSpotQuoteForAmount(7777m, false, Env.Security.OneOffQuoteThirdLevelApproval);
				});
			}
		}

		public void TestSpotQuoteApprovalLimits_UserWithLevel2ApprovalRights()
		{
			var collection = SetUpSpotQuoteApprovalLimitsRegistryItems();

			var user = Factory.NewWithValidTestData<GlbStaff>();
			RateSecurityTestHelper.CreateSecurityRight(Factory, user, Env.Security.OneOffQuoteSecondLevelApproval.Code, true);
			Factory.Save();

			using (DataRegistryRating.Instance.SpotQuoteApprovalSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (DataRegistryRating.Instance.SpotQuoteRequireInternalApproval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				CombineAssertions(() =>
				{
					AssertCanApproveSpotQuoteForAmount(20m, true, Env.Security.None);
					AssertCanApproveSpotQuoteForAmount(150m, false, Env.Security.OneOffQuoteFirstLevelApproval);
					AssertCanApproveSpotQuoteForAmount(2500m, true, Env.Security.OneOffQuoteSecondLevelApproval);
					AssertCanApproveSpotQuoteForAmount(7777m, false, Env.Security.OneOffQuoteThirdLevelApproval);
				});
			}
		}

		public void TestSpotQuoteApprovalLimits_UserWithLevel3ApprovalRights()
		{
			var collection = SetUpSpotQuoteApprovalLimitsRegistryItems();

			var user = Factory.NewWithValidTestData<GlbStaff>();
			RateSecurityTestHelper.CreateSecurityRight(Factory, user, Env.Security.OneOffQuoteThirdLevelApproval.Code, true);
			Factory.Save();

			using (DataRegistryRating.Instance.SpotQuoteApprovalSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (DataRegistryRating.Instance.SpotQuoteRequireInternalApproval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				CombineAssertions(() =>
				{
					AssertCanApproveSpotQuoteForAmount(20m, true, Env.Security.None);
					AssertCanApproveSpotQuoteForAmount(150m, false, Env.Security.OneOffQuoteFirstLevelApproval);
					AssertCanApproveSpotQuoteForAmount(2500m, false, Env.Security.OneOffQuoteSecondLevelApproval);
					AssertCanApproveSpotQuoteForAmount(7777m, true, Env.Security.OneOffQuoteThirdLevelApproval);
				});
			}
		}

		#region Spot Quote Approval Limits Set up

		PaymentThreeLevelAuthorisationSettingsCollection SetUpSpotQuoteApprovalLimitsRegistryItems()
		{
			Env.Security.CachingEnabled = false;
			var collection = new PaymentThreeLevelAuthorisationSettingsCollection();
			AddRegistrySetting(collection, RangeCodes.UpTo, 100m, AuthorisationRequirementCodes.NoApprovalRequired);
			AddRegistrySetting(collection, RangeCodes.UpTo, 1000m, AuthorisationRequirementCodes.FirstApprovalRequiredOnly);
			AddRegistrySetting(collection, RangeCodes.UpTo, 5000m, AuthorisationRequirementCodes.SecondApprovalRequiredOnly);
			AddRegistrySetting(collection, RangeCodes.Above, 5000m, AuthorisationRequirementCodes.ThirdApprovalRequiredOnly);

			return collection;
		}

		void AddRegistrySetting(PaymentThreeLevelAuthorisationSettingsCollection collection, ZString range, ZDecimal amount, ZString requirement)
		{
			var setting = collection.AddNew();
			setting.Range = range;
			setting.Amount = amount;
			setting.AuthorisationRequirement = requirement;
		}

		void AssertCanApproveSpotQuoteForAmount(decimal revenueAmount, bool expected, SecurityCheckpoint expectedSecuritySetting)
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			var spotQuote = quotedBooking.Quote;
			var quoteJob = CreateJob(spotQuote, spotQuote.TH_QuoteNumber);
			quoteJob.LocalChargesPK = NewClient.PK;
			var charge = quoteJob.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_LocalSellAmt = revenueAmount;

			spotQuote.InternalApproveQuote();

			var expectedAction = expected ? "allowed" : "restricted";
			var message = $"Spot Quote Approval for {revenueAmount} should have been {expectedAction} by {expectedSecuritySetting.Code}.";

			AssertEquals(message, expected, spotQuote.IsApproved);
		}

		#endregion

		#endregion

		public void TestWhenLoadingBWQWithFCLMode_ShouldNotRemoveLooseCargoOnRateOneOffShipment()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var bookingPK = QuotedBooking.CreateNewBooking(Factory).PK;
			var quotedBooking = QuotedBooking.New(quote.PK, bookingPK, Factory);
			quotedBooking.Mode = "FCL";
			var booking = quotedBooking.Booking;
			var outerPL = booking.OuterPackLines.AddNew();
			outerPL.JL_F3_NKPackType = "PLT";
			outerPL.JL_Height = 30;
			outerPL.JL_Width = 20;
			outerPL.JL_Length = 40;
			outerPL.JL_ActualWeight = 15;
			outerPL.JL_UnitOfDimension = Length.Centimetres;
			outerPL.JL_ActualWeightUQ = Weight.Kilotonnes;
			outerPL.JL_ActualVolumeUQ = Volume.CubicDecimetres;

			AssertEquals("There is no loose cargo data on Quote before copying.", 0, quote.CurrentOneOffQuote.LooseCargo.Count);
			// We will copy booking values to quote when we click Save on Booking With Quote Form.
			quotedBooking.CheckAndCopyBookingValuesToQuoteIfRequired();
			AssertEquals("Should copy loose cargo data from Booking to Quote.", 1, quote.CurrentOneOffQuote.LooseCargo.Count);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var rateOneOffShipment = newFactory.Load<RateOneOffShipment>(quotedBooking.Quote.CurrentOneOffQuote.PK);
			AssertEquals("Should keep loose cargo data in Quote, even it is with Mode FCL.", 1, rateOneOffShipment.LooseCargo.Count);
		}

		public void TestWhenLoadingOOQWithFCLModeAndHasLooseCargo_ShouldNotThrowException()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			quotedBooking.Mode = "FCL";
			var outerPL = quote.CurrentOneOffQuote.LooseCargo.AddNew();
			outerPL.TPL_F3_NKPackType = "PLT";
			outerPL.TPL_Height = 30;
			outerPL.TPL_Width = 20;
			outerPL.TPL_Length = 40;
			outerPL.TPL_DimensionUQ = Length.Centimetres;
			outerPL.TPL_WeightUQ = Weight.Kilotonnes;
			outerPL.TPL_VolumeUQ = Volume.CubicDecimetres;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			AssertNoExceptionThrown(() => newFactory.Load<RateOneOffShipment>(quotedBooking.Quote.CurrentOneOffQuote.PK));
		}

		#region Carrier On Consolidated Quick Booking

		public void TestCarrierOnConsolidatedQuickBooking()
		{
			var costing1 = Helper.NewCosting(TransportProvider1);
			var costEntry1 = costing1.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			costEntry1.TI_RX_NKCurrency = CurrencyCodes.Australia;
			costEntry1.TI_RC = GP40.PK;
			costEntry1.RateLines.RemoveAndDeleteAll();
			var costLine1 = costEntry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.M3);
			costLine1.GetCalculator<UnitCalculator>().PerUnit = 500m;

			var costing2 = Helper.NewCosting(TransportProvider2);
			var costEntry2 = costing2.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			costEntry2.TI_RX_NKCurrency = CurrencyCodes.Australia;
			costEntry2.TI_RC = GP40.PK;
			costEntry2.RateLines.RemoveAndDeleteAll();
			var costLine2 = costEntry2.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.M3);
			costLine2.GetCalculator<UnitCalculator>().PerUnit = 1000m;

			var quickBooking = CreateQuotedBooking(TransportModes.Sea, "FCL", ZString.Empty, Consignor, Consignor, Consignee, TransportProvider2, "AUSYD", "USLAX", 10000m, 20m, QuotedBookingState.AcceptedBookingWithQuote);
			var container = quickBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerNum = "TEST1111117";
			container.JC_RC = GP40.PK;

			Factory.Save();

			ForwardingConsol createdConsol;

			using (var bookingForm = new QuotedBookingFormForTest(quickBooking))
			{
				AssertEquals("Preconditions: No Consol expected to be on the booking", 0, quickBooking.Booking.Consols.Count);

				bookingForm.ConsolidateToNewConsol();

				bookingForm.PopupForm_ForTesting.BusinessEntity.Factory.Save();

				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				var sameBooking = Factory.Load<ForwardingShipment>(quickBooking.Booking.PK);
				AssertEquals("One Consol must have been created and attached to booking", 1, sameBooking.Consols.Count);
				createdConsol = sameBooking.Consols[0];
			}

			createdConsol.JK_OA_ShippingLineAddress = TransportProvider1.MainAddress.PK;
			createdConsol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;
			createdConsol.Transports.MostInterestingTransport.CreditorPK = ZGuid.Empty;

			Factory.Save();

			var shipment = createdConsol.Shipments[0];

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 10000m,
					CostCalculationDescription = "FRT: 20 Cubic Meter(s) @ AUD 500.00/M3"
				}
			};

			AutorateAndAssert(expectedCharges, shipment, Consignor, autorateCosts: true, autorateRevenue: false);
		}

		#endregion

		#region One Off Quote Consolidation

		public void TestWhenUsingConsolidateOnOneOffQuote_ContainersPopulatedInBookingWithQuote()
		{
			var quotedBooking = CreateQuotedBooking(TransportModes.Sea, "FCL", ZString.Empty, Consignor, Consignor, Consignee, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.QuoteOnly);

			var container = quotedBooking.Quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_RC = GP20.PK;
			container.TC_ContainerCount = 1;
			container = quotedBooking.Quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_RC = GP40.PK;
			container.TC_ContainerCount = 2;

			Factory.Save();

			var consolController = ZControllerFactory.Create(ControllerIDs.JobConsol);
			var viewQuotedBooking = consolController.Factory.Load<ViewQuotedBooking>(quotedBooking.Quote.PK);

			using (var bookingForm = new QuotedBookingFormForTest(quotedBooking))
			{
				bookingForm.ConsolidateToNewConsol();
				bookingForm.PopupForm_ForTesting.BusinessEntity.Factory.Save();

				ForwardingShipment sameBooking = consolController.Factory.Load<ForwardingShipment>(viewQuotedBooking.QuotedBooking.Booking.PK);
				AssertEquals("One Consol must have been created and attached to booking", 1, sameBooking.Consols.Count);
				AssertEquals("Consol should contain containers", 2, sameBooking.Consols[0].Containers.Count);
			}

			Assert("Quoted booking contains containers from OOQ", viewQuotedBooking.QuotedBooking.QuotedBookingContainers.Count == 2);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestConsolidateToNewConsol_ShowsDialog_ForEmptyExchangeRates()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var job = new Job.Loader(quotedBooking).TryCreate();
			var exRate = job.ExchangeRates.AddNew();
			exRate.JF_RX_NKRateCurrency = "CNY";

			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA")).PK;
			charge.JR_RX_NKSellCurrency = "CNY";
			charge.JR_OSSellAmt = 97m;

			quotedBooking.Factory.Save();

			using (var form = new QuotedBookingFormForTest(quotedBooking))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.ConsolidateToNewConsol();
				AssertEquals("Warning Message shown", "Exchange Rates could not be found in Maintain > Reference Files > Exchange Rates. Would you like to use the Exchange Rates on the One Off Quote/Booking with Quote? Click Yes to complete the consolidation operation using the Exchange Rates on the OOQ/BWQ. Click No to cancel the operation.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Custom Fields Test

		[TestedType(typeof(Quote))]
		class CustomFieldsTest : TestICustomFieldProvider
		{
			protected override BusinessObject GetBizo()
			{
				var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
				return quote;
			}
		}

		public void TestGetCustomBusinessObject()
		{
			CreateQuoteWorkflowWithCustomFields();

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);

			IDynamicBusinessObject dynamicBusinessObject = ((ICustomFieldProvider)quote).GetCustomBusinessObject();

			AssertContainsExactElementsInAnyOrder("expected workflow property aliases + infos",
					new[] { "__CUSTOM1__prop__ZString", "__CUSTOM1__prop__ZStringInfo", "__CUSTOM2__prop__ZInt", "__CUSTOM2__prop__ZIntInfo" }, dynamicBusinessObject.PropertyNames);

			Factory.Save();

			quote = new BusinessObjectFactory().Load<Quote>(quote.PK);
			dynamicBusinessObject = ((ICustomFieldProvider)quote).GetCustomBusinessObject();
			AssertEquals("different workflow match", 4, dynamicBusinessObject.PropertyNames.Length);
		}

		void CreateQuoteWorkflowWithCustomFields(string bookingType = "QTN")
		{
			ProcessTaskTemplate processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "QTN";

			GenCustomColumnDefinition customField1 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "custom1";
			customField1.XC_Type = AddOnColumnDataType.Codes.String;

			GenCustomColumnDefinition customField2 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField2.XC_Name = "custom2";
			customField2.XC_Type = AddOnColumnDataType.Codes.Integer;

			Factory.Save();
		}
		#endregion

		#region Universal Copy

		public void TestLooseCargoUniversalCopy()
		{
			var jobHeaderNode = CreateJobHeaderEntityTemplateNode("JobHeader");
			var relatedJobHeaderNode = new RelatedEntityCopyTemplateNode
			{
				Name = "JobHeader",
				InnerNode = jobHeaderNode,
				CopyMethod = RelatedEntityCopyMethod.Copy
			};

			var jobDocAddressNode = CreateJobDocAddressEntityTemplateNode("JobDocAddress");
			var templateJobDocAddressNode = new TemplateCopyTemplateNode
			{
				Name = "JobDocAddress",
				InnerNode = jobDocAddressNode
			};

			var relatedQuotationClientAddressNode = new RelatedEntityCopyTemplateNode
			{
				Name = "QuotationClientAddress",
				InnerNode = templateJobDocAddressNode,
				CopyMethod = RelatedEntityCopyMethod.Copy
			};

			var rateOneOffContainersNode = CreateRateOneOffContainersEntityTemplateNode("RateOneOffContainers");
			var templateRateOneOffContainersNode = new TemplateCopyTemplateNode
			{
				Name = "RateOneOffContainers",
				InnerNode = rateOneOffContainersNode
			};

			var collectionContainersNode = new CollectionCopyTemplateNode
			{
				Name = "Containers",
				ItemPropertyName = "TC_TT",
				InnerNode = templateRateOneOffContainersNode,
				ItemsTableName = RateOneOffContainersSchema.Constants.TableName,
				CopyMethod = CollectionCopyMethod.All
			};

			var rateOneOffPackLineNode = CreateRateOneOffPackLineEntityTemplateNode("RateOneOffPackLine");
			var templateRateOneOffPackLineNode = new TemplateCopyTemplateNode
			{
				Name = "RateOneOffPackLine",
				InnerNode = rateOneOffPackLineNode
			};

			var collectionLooseCargoNode = new CollectionCopyTemplateNode
			{
				Name = "LooseCargo",
				ItemPropertyName = "TPL_TT_RateOneOffShipment",
				InnerNode = templateRateOneOffPackLineNode,
				ItemsTableName = RateOneOffPackLineSchema.Constants.TableName,
				CopyMethod = CollectionCopyMethod.All
			};

			var rateOneOffShipmentNode = CreateRateOneOffShipmentEntityTemplateNode("RateOneOffShipment");
			rateOneOffShipmentNode.Nodes.Add(collectionLooseCargoNode);
			rateOneOffShipmentNode.Nodes.Add(collectionContainersNode);

			var collectionRateOneOffShipmentNode = new CollectionCopyTemplateNode
			{
				Name = "OneOffShipment",
				ItemPropertyName = "TT_TH",
				InnerNode = rateOneOffShipmentNode,
				ItemsTableName = RateOneOffShipmentSchema.Constants.TableName,
				CopyMethod = CollectionCopyMethod.All
			};

			var ratingHeaderNode = CreateRatingHeaderEntityTemplateNode("RatingHeader");
			ratingHeaderNode.Nodes.Add(collectionRateOneOffShipmentNode);
			ratingHeaderNode.Nodes.Add(relatedQuotationClientAddressNode);
			ratingHeaderNode.Nodes.Add(relatedJobHeaderNode);

			var quoteNode = new RelatedEntityCopyTemplateNode
			{
				Name = "Quote",
				InnerNode = ratingHeaderNode,
				CopyMethod = RelatedEntityCopyMethod.Copy
			};

			var quotedBookingNode = new EntityCopyTemplateNode { Name = "QuotedBooking" };
			quotedBookingNode.Nodes.Add(quoteNode);

			var copyTree = new CopyTemplateTree { Name = "QuotedBooking", InnerNode = quotedBookingNode };
			var quote = Factory.NewWithValidTestData<Quote>();
			quote.TH_OneTimeQuote = true;

			var loose = quote.CurrentOneOffQuote.LooseCargo.AddNew();
			loose.TPL_PackLineCount = 1;

			var viewQuotedBooking = Factory.New<ViewQuotedBooking>();
			viewQuotedBooking.VB_TH = quote.PK;
			viewQuotedBooking.QuotedBooking.Destination = "NZAKL";
			viewQuotedBooking.QuotedBooking.Origin = "AUSYD";
			viewQuotedBooking.QuotedBooking.Mode = "LCL";
			viewQuotedBooking.QuotedBooking.Weight = 0.5;
			viewQuotedBooking.QuotedBooking.Volume = 100;

			var orgHeader = Consignee;
			orgHeader.OH_FullName = "CONSIGNEE";
			orgHeader.OH_Code = "CON";
			orgHeader.OH_IsConsignee = true;
			quote.QuotationClientAddress.OrganisationPK = orgHeader.PK;
			quote.CurrentOneOffQuote.DeliveryDocAddress.OrganisationPK = orgHeader.PK;

			Factory.Save();

			var bizOCopyManager = new BusinessObjectCopyManager();
			var quotedBooking = (QuotedBooking)bizOCopyManager.Copy(viewQuotedBooking.QuotedBooking, copyTree).Object;

			using (var testForm = new ZForm(quotedBooking))
			{
				testForm.Show();
				testForm.FireSaveButton();
				Assert("ERROR: Saving Failed", !quotedBooking.HasErrors);
			}
		}

		EntityCopyTemplateNode CreateJobHeaderEntityTemplateNode(string name)
		{
			var jobHeaderNode = new EntityCopyTemplateNode { Name = name };
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_A_JCL, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_A_JOP, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_Description, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_ExcludeFromPeriodicRating, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_GB, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_GC, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_GE, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_GS_NKRepOps, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_GS_NKRepSales, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_HeaderType, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_HoldReason, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_IsProfitSharePosted, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_JobBufferPercentOverride, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_JobLocalReference, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_JobNum, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_JobPlannedStartDate, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_LocalChargesCFX, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_LocalClientInvoicingStyle, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_Name, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_OA_AgentCollectAddr, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_OA_LocalChargesAddr, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_OC_LocalBillingContact, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_PaymentCollectionStatus, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_ProfitLossReasonCode, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_ProfitShareInvoice, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_RatingHasBeenRun, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_RevenueRecognizedDate, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_SingleAgentsInvoicePerConsol, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_Status, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_TH_NKQuoteNumber, CopyMethod = CopyMethod.Copy });
			jobHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobHeaderSchema.Constants.JH_UniqueJobInvoiceNumber, CopyMethod = CopyMethod.Copy });

			return jobHeaderNode;
		}

		EntityCopyTemplateNode CreateJobDocAddressEntityTemplateNode(string name)
		{
			var jobDocAddressNode = new EntityCopyTemplateNode { Name = name };
			jobDocAddressNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobDocAddressSchema.Constants.E2_Address1, CopyMethod = CopyMethod.Copy });
			jobDocAddressNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobDocAddressSchema.Constants.E2_Address2, CopyMethod = CopyMethod.Copy });
			jobDocAddressNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobDocAddressSchema.Constants.E2_AddressMap, CopyMethod = CopyMethod.Copy });
			jobDocAddressNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobDocAddressSchema.Constants.E2_AddressOverride, CopyMethod = CopyMethod.Copy });
			jobDocAddressNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobDocAddressSchema.Constants.E2_AddressSequence, CopyMethod = CopyMethod.Copy });
			jobDocAddressNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobDocAddressSchema.Constants.E2_AddressType, CopyMethod = CopyMethod.Copy });
			jobDocAddressNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobDocAddressSchema.Constants.E2_City, CopyMethod = CopyMethod.Copy });
			jobDocAddressNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobDocAddressSchema.Constants.E2_CompanyName, CopyMethod = CopyMethod.Copy });
			jobDocAddressNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobDocAddressSchema.Constants.E2_Contact, CopyMethod = CopyMethod.Copy });
			jobDocAddressNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobDocAddressSchema.Constants.E2_Email, CopyMethod = CopyMethod.Copy });
			jobDocAddressNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobDocAddressSchema.Constants.E2_Fax, CopyMethod = CopyMethod.Copy });
			jobDocAddressNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobDocAddressSchema.Constants.E2_GovRegNum, CopyMethod = CopyMethod.Copy });
			jobDocAddressNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobDocAddressSchema.Constants.E2_GovRegNumType, CopyMethod = CopyMethod.Copy });
			jobDocAddressNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobDocAddressSchema.Constants.E2_IsResidential, CopyMethod = CopyMethod.Copy });
			jobDocAddressNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobDocAddressSchema.Constants.E2_Mobile, CopyMethod = CopyMethod.Copy });
			jobDocAddressNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobDocAddressSchema.Constants.E2_OA_Address, CopyMethod = CopyMethod.Copy });
			jobDocAddressNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobDocAddressSchema.Constants.E2_ParentTableCode, CopyMethod = CopyMethod.Copy });
			jobDocAddressNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobDocAddressSchema.Constants.E2_Phone, CopyMethod = CopyMethod.Copy });
			jobDocAddressNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobDocAddressSchema.Constants.E2_Postcode, CopyMethod = CopyMethod.Copy });
			jobDocAddressNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobDocAddressSchema.Constants.E2_RN_NKCountryCode, CopyMethod = CopyMethod.Copy });
			jobDocAddressNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobDocAddressSchema.Constants.E2_ScreeningStatus, CopyMethod = CopyMethod.Copy });
			jobDocAddressNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobDocAddressSchema.Constants.E2_State, CopyMethod = CopyMethod.Copy });
			jobDocAddressNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobDocAddressSchema.Constants.E2_ValidationStatus, CopyMethod = CopyMethod.Copy });

			return jobDocAddressNode;
		}

		EntityCopyTemplateNode CreateRateOneOffContainersEntityTemplateNode(string name)
		{
			var rateOneOffContainersNode = new EntityCopyTemplateNode { Name = name };
			rateOneOffContainersNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RateOneOffContainersSchema.Constants.TC_ContainerCount, CopyMethod = CopyMethod.Copy });
			rateOneOffContainersNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RateOneOffContainersSchema.Constants.TC_RC, CopyMethod = CopyMethod.Copy });
			return rateOneOffContainersNode;
		}

		EntityCopyTemplateNode CreateRateOneOffPackLineEntityTemplateNode(string name)
		{
			var rateOneOffPackLineNode = new EntityCopyTemplateNode { Name = name };
			rateOneOffPackLineNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RateOneOffPackLineSchema.Constants.TPL_PackLineCount, CopyMethod = CopyMethod.Copy });
			rateOneOffPackLineNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RateOneOffPackLineSchema.Constants.TPL_RC_RefContainer, CopyMethod = CopyMethod.Copy });
			rateOneOffPackLineNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RateOneOffPackLineSchema.Constants.TPL_F3_NKPackType, CopyMethod = CopyMethod.Copy });
			rateOneOffPackLineNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RateOneOffPackLineSchema.Constants.TPL_Height, CopyMethod = CopyMethod.Copy });
			rateOneOffPackLineNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RateOneOffPackLineSchema.Constants.TPL_Length, CopyMethod = CopyMethod.Copy });
			rateOneOffPackLineNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RateOneOffPackLineSchema.Constants.TPL_Width, CopyMethod = CopyMethod.Copy });
			rateOneOffPackLineNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RateOneOffPackLineSchema.Constants.TPL_DimensionUQ, CopyMethod = CopyMethod.Copy });
			rateOneOffPackLineNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RateOneOffPackLineSchema.Constants.TPL_Volume, CopyMethod = CopyMethod.Copy });
			rateOneOffPackLineNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RateOneOffPackLineSchema.Constants.TPL_VolumeUQ, CopyMethod = CopyMethod.Copy });
			rateOneOffPackLineNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RateOneOffPackLineSchema.Constants.TPL_Weight, CopyMethod = CopyMethod.Copy });
			rateOneOffPackLineNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RateOneOffPackLineSchema.Constants.TPL_WeightUQ, CopyMethod = CopyMethod.Copy });
			return rateOneOffPackLineNode;
		}

		EntityCopyTemplateNode CreateRateOneOffShipmentEntityTemplateNode(string name)
		{
			var rateOneOffShipmentNode = new EntityCopyTemplateNode { Name = name };
			rateOneOffShipmentNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RateOneOffShipmentSchema.Constants.TT_ActualVolume, CopyMethod = CopyMethod.Copy });
			rateOneOffShipmentNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RateOneOffShipmentSchema.Constants.TT_ActualWeight, CopyMethod = CopyMethod.Copy });
			rateOneOffShipmentNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RateOneOffShipmentSchema.Constants.TT_Chargeable, CopyMethod = CopyMethod.Copy });
			rateOneOffShipmentNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RateOneOffShipmentSchema.Constants.TT_RL_NKDeliveryLocation, CopyMethod = CopyMethod.Copy });
			rateOneOffShipmentNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RateOneOffShipmentSchema.Constants.TT_RL_NKReceivalLocation, CopyMethod = CopyMethod.Copy });
			rateOneOffShipmentNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RateOneOffShipmentSchema.Constants.TT_TransportMode, CopyMethod = CopyMethod.Copy });
			rateOneOffShipmentNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RateOneOffShipmentSchema.Constants.TT_ContainerMode, CopyMethod = CopyMethod.Copy });

			return rateOneOffShipmentNode;
		}

		EntityCopyTemplateNode CreateRatingHeaderEntityTemplateNode(string name)
		{
			var ratingHeaderNode = new EntityCopyTemplateNode { Name = name };
			ratingHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RatingHeaderSchema.Constants.TH_Accepted, CopyMethod = CopyMethod.Copy });
			ratingHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RatingHeaderSchema.Constants.TH_AirCFX, CopyMethod = CopyMethod.Copy });
			ratingHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RatingHeaderSchema.Constants.TH_ExportAirCFX, CopyMethod = CopyMethod.Copy });
			ratingHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RatingHeaderSchema.Constants.TH_ExportSeaCFX, CopyMethod = CopyMethod.Copy });
			ratingHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RatingHeaderSchema.Constants.TH_FollowUpDate, CopyMethod = CopyMethod.Copy });
			ratingHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RatingHeaderSchema.Constants.TH_GC, CopyMethod = CopyMethod.Copy });
			ratingHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RatingHeaderSchema.Constants.TH_GlobalRateDescription, CopyMethod = CopyMethod.Copy });
			ratingHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RatingHeaderSchema.Constants.TH_GlobalRateLevel, CopyMethod = CopyMethod.Copy });
			ratingHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RatingHeaderSchema.Constants.TH_GS_NKFirstSignatory, CopyMethod = CopyMethod.Copy });
			ratingHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RatingHeaderSchema.Constants.TH_GS_NKSecondSignatory, CopyMethod = CopyMethod.Copy });
			ratingHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RatingHeaderSchema.Constants.TH_IsLocked, CopyMethod = CopyMethod.Copy });
			ratingHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RatingHeaderSchema.Constants.TH_IsOneOffQuoteConsumed, CopyMethod = CopyMethod.Copy });
			ratingHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RatingHeaderSchema.Constants.TH_OH, CopyMethod = CopyMethod.Copy });
			ratingHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RatingHeaderSchema.Constants.TH_OneTimeQuote, CopyMethod = CopyMethod.Copy });
			ratingHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RatingHeaderSchema.Constants.TH_PrintInheritedDestinationCharges, CopyMethod = CopyMethod.Copy });
			ratingHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RatingHeaderSchema.Constants.TH_PrintInheritedOriginCharges, CopyMethod = CopyMethod.Copy });
			ratingHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RatingHeaderSchema.Constants.TH_PrintRateLevelDestinationCharges, CopyMethod = CopyMethod.Copy });
			ratingHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RatingHeaderSchema.Constants.TH_PrintRateLevelOriginCharges, CopyMethod = CopyMethod.Copy });
			ratingHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RatingHeaderSchema.Constants.TH_QuoteCancellationReason, CopyMethod = CopyMethod.Copy });
			ratingHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RatingHeaderSchema.Constants.TH_QuoteDate, CopyMethod = CopyMethod.Copy });
			ratingHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RatingHeaderSchema.Constants.TH_QuoteEndDate, CopyMethod = CopyMethod.Copy });
			ratingHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RatingHeaderSchema.Constants.TH_QuoteNumber, CopyMethod = CopyMethod.Copy });
			ratingHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RatingHeaderSchema.Constants.TH_RateType, CopyMethod = CopyMethod.Copy });
			ratingHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RatingHeaderSchema.Constants.TH_RX_NKSpotRateCurrency, CopyMethod = CopyMethod.Copy });
			ratingHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RatingHeaderSchema.Constants.TH_SeaCFX, CopyMethod = CopyMethod.Copy });
			ratingHeaderNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RatingHeaderSchema.Constants.TH_SpotRate, CopyMethod = CopyMethod.Copy });

			return ratingHeaderNode;
		}

		#endregion
	}
}
