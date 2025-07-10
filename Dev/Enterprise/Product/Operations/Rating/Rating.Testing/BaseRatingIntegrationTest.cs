using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Packing.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Test;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.Registry.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;
using Charge = Enterprise.Accounting.Business.JobInvoicing.Charge;
using ChargeType = WiseRates.Api.Model.ChargeType;
using ExchangeRate = Enterprise.Accounting.Business.JobInvoicing.ExchangeRate;
using Rate = WiseRates.Api.Model.Rate;
using RatesServiceCharge = WiseRates.Api.Model.Charge;
using RefChargeCode = WiseRates.Api.Model.RefChargeCode;

namespace Enterprise.RatingTests.Testing
{
	public class BaseRatingIntegrationTest : RatingTestCase
	{
		protected void AutoCostAndAssert(string message
			, Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>> expectedInvoicingCharges
			, IEnumerable<AssertionCost> expectedCosts
			, IGenericJobCostPlugIn costsSupporter
			, bool autorateRevenue = true
			, bool autorateCosts = true
			, bool deleteExistingCosts = true
			, string[] expectedWarnings = null
			, string[] expectedErrors = null
			, IDialogService dialogService = null
			)
		{
			DataRegistryRating.Instance.AllowSavingOfAutoRatingLogNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			UnitTestUserNotification.Instance.ClearMessages();

			if (deleteExistingCosts)
			{
				DeleteExistingCosts(costsSupporter.CostSupporter.PK);
			}

			var businessEntity = costsSupporter as IBusiness;

			using (var form = new ZForm(businessEntity))
			{
				form.PlugIns.Add(ControllerIDs.Apportionment);
				form.DisplayMode = ODisplayMode.Edit;

				using (var plugin = (ApportionmentPlugin)form.PlugIns.Instances[0])
				{
					string menuName;
					if (autorateRevenue && autorateCosts)
					{
						menuName = "Autorate Costs and Revenue";
					}
					else if (autorateCosts)
					{
						menuName = "Autorate Costs";
					}
					else
					{
						throw new InvalidOperationException("No menu item exist for revenue only.");
					}

					plugin.DialogService_ForTestOnly = dialogService;
					var item = plugin.TopLevelMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == menuName);
					item.PerformClick();

					businessEntity.Factory.Save();

					var autoRatingSummary = GetAutoRatingSummary(message, costsSupporter as IStmNoteParent, expectedWarnings, expectedErrors);

					AssertCosts(autoRatingSummary, costsSupporter, expectedCosts);
					AssertCharges(autoRatingSummary, expectedInvoicingCharges);
				}
			}
		}

		protected void AssertCosts(string message, IGenericJobCostPlugIn costsSupporter, IEnumerable<AssertionCost> expectedCosts)
		{
			if (expectedCosts != null)
			{
				var costs = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, costsSupporter.CostSupporter.PK));

				var comparisonColumns = expectedCosts.SelectMany(x => x.ComparisonColumns).Distinct().ToList();
				var customComparisonColumns = expectedCosts.SelectMany(x => x.CustomComparisonColumns).Distinct().ToList();

				AssertContainsExactElementsInAnyOrder(
					message,
					AssertionMock.Comparer,
					expectedCosts,
					costs.Select(x => new AssertionCost(x, comparisonColumns, customComparisonColumns)));
			}
		}

		protected void AssertCharges(string message, Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>> expectedInvoicingCharges)
		{
			if (expectedInvoicingCharges != null)
			{
				foreach (var pair in expectedInvoicingCharges)
				{
					var job = new JobHeader.Loader(pair.Key).Load() as Job;
					AssertCharges(message, pair.Value, job);
				}
			}
		}

		protected RatingResults AutorateAndAssert<T>(IEnumerable<AssertionCharge> expected
			, T jobParent
			, OrgHeader localClient
			, OrgHeader agent = null
			, Job job = null
			, bool autorateRevenue = true
			, bool autorateCosts = true
			, bool isConsolLevelChargeExcluded = false
			, bool standaloneShipmentOnly = false
			, string[] expectedWarnings = null
			, string[] expectedErrors = null
			, IAutoRatingGUIInteractor testInteractor = null
			, IRatingContext ratingContext = null
			, bool deleteJobCharges = true)
			where T : IJobHeaderParent, IBusiness
		{
			return AutorateAndAssert("", expected, jobParent, localClient, agent, job, autorateRevenue, autorateCosts, isConsolLevelChargeExcluded, standaloneShipmentOnly, expectedWarnings, expectedErrors, testInteractor, ratingContext, deleteJobCharges: deleteJobCharges);
		}

		protected RatingResults AutorateWithManualSelectAndAssert<T>(string message
			, IEnumerable<AssertionCharge> expected
			, T jobParent
			, OrgHeader localClient
			, OrgHeader agent = null
			, Job job = null
			, bool autorateRevenue = true
			, bool autorateCosts = true
			, bool isConsolLevelChargeExcluded = false
			, bool standaloneShipmentOnly = false
			, string[] expectedWarnings = null
			, string[] expectedErrors = null
			, IAutoRatingGUIInteractor testInteractor = null
			, IRatingContext ratingContext = null)
			where T : IJobHeaderParent, IBusiness
		{
			if (ratingContext == null)
			{
				var mockedInteractor = new Mock<IAutoRatingGUIInteractor>();
				ratingContext = RatingContext.CreateForManualSelect(mockedInteractor.Object, new Mock<IDialogService>().Object);
				mockedInteractor
					.Setup(x => x.SelectRate(ratingContext, It.IsAny<RatingCriteria>()))
					.Callback<IRatingContext, RatingCriteria>((context, ratingCriteria) => new RateSelectorCommand().SelectRate(context, ratingCriteria, null))
					.Returns((IEnumerable<AutoRateInfo>)null);
			}

			return AutorateAndAssert(message, expected, jobParent, localClient, agent, job, autorateRevenue, autorateCosts, isConsolLevelChargeExcluded, standaloneShipmentOnly, expectedWarnings, expectedErrors, testInteractor, ratingContext);
		}

		protected RatingResults AutorateAndAssert<T>(string message
			, IEnumerable<AssertionCharge> expected
			, T jobParent
			, OrgHeader localClient
			, OrgHeader agent = null
			, Job job = null
			, bool autorateRevenue = true
			, bool autorateCosts = true
			, bool isConsolLevelChargeExcluded = false
			, bool standaloneShipmentOnly = false
			, string[] expectedWarnings = null
			, string[] expectedErrors = null
			, IAutoRatingGUIInteractor testInteractor = null
			, IRatingContext ratingContext = null
			, bool shouldCheckErrorsAndWarningsInNoteText = true
			, bool deleteJobCharges = true)
			where T : IJobHeaderParent, IBusiness
		{
			RatingResults result;
			UnitTestUserNotification.Instance.ClearMessages();

			var createNewJob = job == null;

			using (job = job ?? new Job.Loader(jobParent).TryLoadOrCreateWithoutMutexForTestOnly())
			using (var plugin = new InvoicingPluginToFreight(jobParent))
			{
				if (createNewJob)
				{
					if (deleteJobCharges)
					{
						job.Charges.RemoveAndDeleteAll();
					}
					job.ExchangeRates.RemoveAndDeleteAll();

					if (localClient != null)
					{
						job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
					}

					if (agent != null)
					{
						job.JH_OA_AgentCollectAddr = agent.MainAddress.PK;
					}
				}

				var options = new AutoRateOptions
				(
					autoRateCost: autorateCosts,
					autoRateRevenue: autorateRevenue,
					excludeConsolLevelChargesOnCosting: isConsolLevelChargeExcluded,
					standaloneShipmentOnly: standaloneShipmentOnly
				);

				if (ratingContext != null)
				{
					result = plugin.ExecuteAutorating(ratingContext, options);
				}
				else if (testInteractor != null)
				{
					result = plugin.ExecuteAutorating(testInteractor, options);
				}
				else
				{
					result = plugin.ExecuteAutorating(options);
				}

				var autoRatingSummary = GetAutoRatingSummary(message, jobParent as IStmNoteParent, expectedWarnings, expectedErrors, shouldCheckErrorsAndWarningsInNoteText);

				AssertCharges(autoRatingSummary, expected, job);
			}

			return result;
		}

		protected RatingResults EnterpriseServiceAutorateAndAssert<T>(IEnumerable<AssertionCharge> expected
			, T jobParent
			, OrgHeader localClient
			, OrgHeader agent = null
			, Job job = null
			, bool autorateRevenue = true
			, bool autorateCosts = true
			, string[] expectedWarnings = null
			, string[] expectedErrors = null)
			where T : IJobHeaderParent, IBusiness
		{
			DataRegistryRating.Instance.AllowSavingOfAutoRatingLogNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			RatingResults result;
			UnitTestUserNotification.Instance.ClearMessages();

			var createNewJob = job == null;

			using (job = job ?? new Job.Loader(jobParent).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				if (createNewJob)
				{
					job.Charges.RemoveAndDeleteAll();
					job.ExchangeRates.RemoveAndDeleteAll();

					if (localClient != null)
					{
						job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
					}

					if (agent != null)
					{
						job.JH_OA_AgentCollectAddr = agent.MainAddress.PK;
					}
					Factory.Save();
				}

				result = ObjectFactory.Get<IAccountingRatingService>().AutoRateAndCreateJobHeader(jobParent.PK.ToGuid(), jobParent.TablePrefix(), Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK, localClient.PK.ToGuid(), autorateRevenue, autorateCosts);

				var autoRatingSummary = GetAutoRatingSummary("", jobParent as IStmNoteParent, expectedWarnings, expectedErrors);

				AssertCharges(autoRatingSummary, expected, job);
			}

			return result;
		}

		protected static string GetAutoRatingSummary(string message, IStmNoteParent noteParent, string[] expectedWarnings, string[] expectedErrors, bool shouldCheckAutoratingLog = true)
		{
			var messages = UnitTestUserNotification.Instance.PreviousMessages.ToArray();

			expectedWarnings = expectedWarnings ?? Array.Empty<string>();

			var msgBuilder = new ZStringBuilder();
			msgBuilder.AppendIfNotEmpty(message);
			msgBuilder.AppendLine();

			msgBuilder.Append("*** POP-UP ERRORS *******************************");
			AppendAndAssertMessages(msgBuilder, expectedErrors, messages.Where(x => x.WasError).Select(x => x.ToString().Trim()).ToArray(), true);

			msgBuilder.AppendLine();
			msgBuilder.Append("*** POP-UP NOTIFICATIONS ************************");
			AppendAndAssertMessages(msgBuilder, expectedWarnings, messages.Where(x => !x.WasError && !x.WasNone).Select(x => x.ToString().Trim()).ToArray(), false);

			msgBuilder.AppendLine();
			msgBuilder.Append("***** RATING ***** ENGINE ***** LOG *****");

			if (shouldCheckAutoratingLog)
			{
				var isExpectedToHaveThrownAnError = expectedErrors != null && expectedErrors.Any(x => x.Contains("Autorating has encountered an error") || x.Contains("Autorating cannot be run"));
				if (!isExpectedToHaveThrownAnError)
				{
					msgBuilder.Append(GetNoteTextForCurrentCompany(noteParent.Notes));
				}
			}

			return msgBuilder.ToStringWithNewLineBetweenAppends();
		}

		static void AppendAndAssertMessages(ZStringBuilder msgBuilder, string[] expectedMessages, string[] actualMessages, bool assertingErrors)
		{
			if (actualMessages.Length > 0)
			{
				msgBuilder.Append(new ZStringBuilder(actualMessages));
			}

			if (expectedMessages != null)
			{
				if (assertingErrors)
				{
					AssertContainsExactElementsInAnyOrder("Should have no unexpected errors", expectedMessages, actualMessages);
				}
				else
				{
					CombineAssertions(msgBuilder.ToStringWithNewLineBetweenAppends(), () =>
					{
						foreach (var expectedMessage in expectedMessages)
						{
							AssertCollectionContains("Expected warning/info not found in actual AutoRating results", expectedMessage, actualMessages);
						}
					});
				}
			}
		}

		protected void AssertCharges(IEnumerable<AssertionCharge> expected, Job job)
		{
			AssertCharges("", expected, job);
		}

		protected void AssertCharges(string message, IEnumerable<AssertionCharge> expected, Job job)
		{
			expected = expected ?? Enumerable.Empty<AssertionCharge>();
			var comparisonColumns = expected.SelectMany(x => x.ComparisonColumns).Distinct().ToList();
			var customComparisonColumns = expected.SelectMany(x => x.CustomComparisonColumns).Distinct().ToList();
			var actual = job.Charges.Cast<Charge>().Select(y => new AssertionCharge(y, comparisonColumns, customComparisonColumns));

			AssertContainsExactElementsInAnyOrder(
				message,
				AssertionMock.Comparer,
				expected,
				actual);
		}

		protected void DeleteExistingCosts(ZGuid parentId)
		{
			var costs = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, parentId));
			foreach (var cost in costs)
			{
				cost.Delete();
			}
		}

		protected void AssertAutoratingAuditLogNote(BusinessObject bizO, string expectedNote, string message = "")
		{
			var actualLog = GetNoteTextForCurrentCompany(bizO.GetNotes());
			AssertMultilineASCIIEquals(message, expectedNote, actualLog);
		}

		protected void AssertAutoratingAuditLogContains(BusinessObject bizO, string expectedNote, string message = "")
		{
			var actualLog = GetNoteTextForCurrentCompany(bizO.GetNotes());
			AssertContains(message, expectedNote, actualLog);
		}

		protected void AssertAutoratingAuditLogNotContains(BusinessObject bizO, string expectedNote, string message = "")
		{
			var actualLog = GetNoteTextForCurrentCompany(bizO.GetNotes());
			AssertNotContains(message, expectedNote, actualLog);
		}

		protected void AssertAutoratingAuditLogNoteContainsLines(BusinessObject bizO, string message, params string[] lines)
		{
			var actualLog = GetNoteTextForCurrentCompany(bizO.GetNotes());

			Assert("When checking that the autorating log has certain lines included, you MUST have some lines to check", lines.Length > 0);

			CombineAssertions(message + System.Environment.NewLine + actualLog, () =>
			{
				foreach (var line in lines)
				{
					AssertContains("Log doesn't contain matching line:" + line, line, actualLog);
				}
			});
		}

		static ZString GetNoteTextForCurrentCompany(Notes notes)
		{
			var result = notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description)
				.Where(n => n.ST_GC_RelatedCompany == GlbCompany.CurrentCompany.PK).ToArray();

			AssertEquals("Expected only a single autorating log note per company", 1, result.Length);

			return result[0].ST_NoteText;
		}

		protected void AssertOperationLogContainsLines(DummyOperationalActionSectionLog log, string message = "", params string[] lines)
		{
			Assert("Log should contain messages", log.messages.Count > 0);
			Assert("When checking that the operating log has certain lines included, you MUST have some lines to check", lines.Length > 0);

			var actualLog = log.MessagesString();

			CombineAssertions(message + System.Environment.NewLine + actualLog, () =>
			{
				foreach (var line in lines)
				{
					AssertContains("Log doesn't contain matching line:" + line, line, actualLog);
				}
			});
		}

		protected static void SetConsolCostDefaultApportionmentMethodFromCode(ZString code)
		{
			var newConfig = ConsolCostDefaultApportionmentMethodConfiguration.Create_ForTestOnly(code);
			AccountingConfigurationRegistry.Instance.ConsolCostDefaultApportionmentMethod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newConfig);
		}

		protected JobConsolCost CreateConsolCost(ForwardingConsol consol, AccChargeCode chargeCode, OrgHeader creditor = null)
		{
			ApportionmentListing listing = consol.GetApportionments();
			JobConsolCost cost = listing.CostsCollection.TryAddNew();
			var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			if (cost != null)
			{
				cost.E6_AC_ChargeCode = chargeCode.PK;
				if (creditor != null)
				{
					cost.E6_OH_Creditor = creditor.PK;
				}
				cost.E6_RX_NKCurrency = currency.RX_Code;
				cost.E6_ExchangeRate = 1m;
			}
			return cost;
		}

		#region CreateForTest

		protected ForwardingShipment CreateShipment(
			string transportMode = "SEA",
			string containerMode = "FCL",
			string origin = "UAIEV",
			string destination = "AUSYD")
		{
			var shipment = CreateForwardingShipment(transportMode, Consignor.PK, Consignee.PK, origin, destination, 0, 0);
			shipment.JS_PackingMode = containerMode;

			return shipment;
		}

		protected ForwardingConsol CreateConsol(
			string transportMode = "SEA",
			string containerMode = "FCL",
			string origin = "UAIEV",
			string destination = "AUSYD")
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = transportMode;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.Transports[0].JW_IsLinked = false;
			consol.Transports[0].CarrierPK = TransportProvider1.PK;
			consol.JK_OA_ShippingLineAddress = TransportProvider1.MainAddress.PK;
			consol.JK_ConsolMode = containerMode;
			consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			return consol;
		}

		internal ForwardingShipment CreateForwardingShipment(string transportMode, ZGuid consignorPK, ZGuid consigneePK, string origin, string destination, decimal weight, decimal volume = 0m, CommonConsol consol = null)
			=> CreateForwardingShipment
			(
				Factory,
				transportMode,
				packingMode: transportMode == TransportModes.Air ? ContainerModes.Loose : ContainerModes.LCL,
				consignorPK,
				consigneePK,
				origin,
				destination,
				weight,
				volume,
				consol
			);

		internal static ForwardingShipment CreateForwardingShipment(BusinessObjectFactory factory, string transportMode, string packingMode, ZGuid consignorPK, ZGuid consigneePK, string origin, string destination, decimal weight, decimal volume = 0m, CommonConsol consol = null)
		{
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();

			if (consol != null)
			{
				consol.Shipments.Add(shipment);
			}

			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment.JS_TransportMode = transportMode;
			shipment.JS_PackingMode = packingMode;
			shipment.ConsignorPK = consignorPK;
			shipment.ConsigneePK = consigneePK;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_ActualWeight = weight;
			shipment.JS_ActualVolume = volume;
			shipment.JS_UnitOfWeight = Weight.Kilograms;
			shipment.JS_UnitOfVolume = Volume.CubicMetres;

			return shipment;
		}

		internal static ForwardingConsol CreateForwardingConsol(string transportMode, string origin, string destination, OrgHeader transportProvider, ForwardingShipment shipment, string prepaidCollect = "")
		{
			var consol = shipment.Consols.AddNew();
			consol.JK_UniqueConsignRef = "C00023421";
			consol.JK_TransportMode = transportMode;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.Transports[0].JW_IsLinked = false;
			consol.Transports[0].CarrierPK = transportProvider.PK;
			consol.JK_OA_ShippingLineAddress = transportProvider.MainAddress.PK;
			consol.JK_PrepaidCollect = prepaidCollect;

			return consol;
		}

		protected Tuple<ForwardingConsol, ForwardingShipment, ForwardingContainer> CreateForwardingConsolWithShipmentAndContainer(string origin, string destination, OrgHeader transportProvider, string prepaidCollect)
		{
			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, origin, destination, 0m);
			shipment.JS_PackingMode = ContainerModes.FCL;

			var consol = CreateForwardingConsol(TransportModes.Sea, origin, destination, transportProvider, shipment, prepaidCollect);
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = transportProvider.MainAddress.PK;

			var container = consol.Containers.AddNew();

			return new Tuple<ForwardingConsol, ForwardingShipment, ForwardingContainer>(consol, shipment, container);
		}

		protected Job CreateJob(IJobHeaderParent bizO, ZString jobNumber, GlbBranch branch = null)
		{
			var result = Factory.NewJobForTesting<Job>();
			result.SuspendValidation();
			result.Parent = bizO;
			result.JH_JobNum = jobNumber;
			result.JH_GC = branch != null ? branch.GB_GC : GlbCompany.CurrentCompany.PK;
			result.JH_GB = branch != null ? branch.PK : GlbBranch.CurrentBranch.PK;
			result.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FIA")).PK;

			return result;
		}

		protected void PostRevenue(Charge charge)
		{
			var tl = charge.IsSavedByFactory
				? Factory.NewWithValidTestData<AccTransactionLines>()
				: charge.WIP;

			tl.AL_AH = Factory.NewWithValidTestData<AccTransactionHeader>().PK;
			tl.AL_LineType = TransactionLineTypes.Revenue;
			tl.AL_RX_NKTransactionCurrency = CurrencyCodes.Australia;
			tl.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			tl.AL_ReverseDate = ZDateTime.Now;

			charge.JR_AL_ARLine = tl.PK;

			Assert("Revenue is now posted", charge.IsRevenuePosted);
		}

		protected void PostCost(Charge charge)
		{
			var tl = charge.IsSavedByFactory
				? Factory.NewWithValidTestData<AccTransactionLines>()
				: charge.Accrual;

			tl.AL_AH = Factory.NewWithValidTestData<AccTransactionHeader>().PK;
			tl.AL_LineType = TransactionLineTypes.Cost;
			tl.AL_LineAmount = -charge.JR_LocalCostAmt;
			tl.AL_OSAmount = -charge.JR_OSCostAmt;
			tl.AL_RX_NKTransactionCurrency = CurrencyCodes.Australia;
			tl.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			tl.AL_ReverseDate = ZDateTime.Now;

			charge.JR_AL_APLine = tl.PK;

			Assert("Cost is now posted", charge.IsCostPosted);
		}
		public QuotedBooking CreateQuotedBooking(string transportMode, string containerMode, string paymentTerms, OrgHeader client, OrgHeader consignor, OrgHeader consignee, OrgHeader carrier, string origin, string destination, decimal weight, decimal volume, QuotedBookingState quotedBookingState = QuotedBookingState.AcceptedBookingWithQuote)
		{
			return CreateQuotedBooking(Factory, transportMode, containerMode, paymentTerms, client, consignor, consignee, carrier, origin, destination, weight, volume, quotedBookingState);
		}

		public static QuotedBooking CreateQuotedBooking(BusinessObjectFactory factory, string transportMode, string containerMode, string paymentTerms, OrgHeader client, OrgHeader consignor, OrgHeader consignee, OrgHeader carrier, string origin, string destination, decimal weight, decimal volume, QuotedBookingState quotedBookingState = QuotedBookingState.AcceptedBookingWithQuote)
		{
			var quotePK = quotedBookingState == QuotedBookingState.QuoteOnly || quotedBookingState == QuotedBookingState.AcceptedBookingWithQuote
				? QuotedBooking.CreateNewQuote(factory, QuotedBooking.QuoteState.ApprovedAndAccepted).PK
				: Guid.Empty;

			var bookingPK = quotedBookingState == QuotedBookingState.BookingOnly || quotedBookingState == QuotedBookingState.AcceptedBookingWithQuote
				? QuotedBooking.CreateNewBooking(factory).PK
				: Guid.Empty;

			var quotedBooking = QuotedBooking.New(quotePK, bookingPK, factory);
			if (quotedBooking.ClientDocAddress != null && client?.MainAddress != null)
			{
				quotedBooking.ClientDocAddress.E2_OA_Address = client.MainAddress.PK;
			}

			if (consignor != null)
			{
				if (!string.IsNullOrEmpty(origin))
				{
					consignor.OH_RL_NKClosestPort = origin;
				}

				if (quotedBooking.Booking != null)
				{
					quotedBooking.Booking.ConsignorPickupAddress.E2_OA_Address = consignor.MainAddress.PK;
				}

				quotedBooking.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			}

			if (consignee != null)
			{
				if (!string.IsNullOrEmpty(destination))
				{
					consignee.OH_RL_NKClosestPort = destination;
				}

				if (quotedBooking.Booking != null)
				{
					quotedBooking.Booking.ConsigneeDeliveryAddress.E2_OA_Address = consignee.MainAddress.PK;
				}

				quotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			}

			if (carrier != null)
			{
				quotedBooking.OH_Carrier = carrier.PK;
			}

			quotedBooking.TransportMode = transportMode;
			quotedBooking.ContainerMode = containerMode;
			quotedBooking.PaymentTerms = paymentTerms;
			quotedBooking.Origin = origin;
			quotedBooking.Destination = destination;
			quotedBooking.Weight = weight;
			quotedBooking.Volume = volume;

			return quotedBooking;
		}

		protected static void CreateInstructionPkgDivots(DtbBookingInstruction instruction, params PkgPackage[] packages)
		{
			foreach (var package in packages)
			{
				var divot = instruction.PackageDivots.AddNew();
				divot.KD_KP_Package = package.PK;
				divot.KD_Quantity = package.KP_PackageQty;
			}
		}

		protected static PkgPackage CreatePackage(DtbBooking booking, int quantity, string quantityUQ, RefCommodityCode commodity, decimal weight, string weightUQ, decimal volume, string volumeUQ)
		{
			var package = booking.AssignedPackages.AddNew();
			package.KP_PackageQty = quantity;
			package.KP_F3_NKPackType = quantityUQ;
			package.KP_Weight = weight;
			package.KP_WeightUQ = weightUQ;
			package.KP_Volume = volume;
			package.KP_VolumeUQ = volumeUQ;
			package.KP_RH_NKCommodityCode = commodity != null ? commodity.RH_Code : ZString.Empty;
			if (booking.ConsolidationSingleJob != null)
			{
				package.KP_KJ_ParentPackageJob = booking.ConsolidationSingleJob.PackageJob.PK;
			}

			return package;
		}

		protected ForwardingShipment GetShipmentWithMatchingCostAndClientRate(OrgHeader consignee, OrgHeader consignor, OrgHeader transportProvider, ZString origin, ZString destination, ZString chargeCode, ZDecimal clientBaseRate, ZDecimal costingBaseRate)
		{
			consignee.OH_IsDebtor = true;
			Helper.NewClientRateWithSingleRateLine(consignee, RatingConstants.RateCategory.LCL, RateMode.LCL, origin, destination, chargeCode, clientBaseRate);

			var costingRate = Helper.NewCosting(transportProvider);
			var costingEntry = costingRate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, origin, destination);
			costingEntry.RateLines.RemoveAndDeleteAll();
			var costingLine = costingEntry.AddRateLine(chargeCode, FlatCalculator.Code, "", CurrencyCodes.Australia);
			costingLine.GetCalculator<FlatCalculator>().BaseRate = costingBaseRate;

			var shipment = CreateForwardingShipment(TransportModes.Sea, consignor.PK, consignee.PK, origin, destination, 0m);
			var consol = CreateForwardingConsol(TransportModes.Sea, origin, destination, transportProvider, shipment);
			consol.CreditorPK = transportProvider.PK;

			return shipment;
		}

		protected void CreateRefCusRateCode(ZString code, ZString description, string country = "AU")
		{
			if (country == "AU")
			{
				CreateRefDataGrouping(country);
			}

			var rateTypePK = CreateRefCusRateType(description, description, country);
			var sqlText = $@"
SELECT TOP 1 {RefCusRateCodeSchema.Constants.PK}
FROM {RefCusRateCodeSchema.Constants.TableName}
WHERE {RefCusRateCodeSchema.Constants.ZY1_RateCode} = '{code}' AND {RefCusRateCodeSchema.Constants.ZY1_ZZR_RateType} = '{rateTypePK}'";

			var dynamicBOs = new DynamicBusinessObjectCollection(Factory);
			dynamicBOs.Load(sqlText);
			if (!(dynamicBOs.Count > 0))
			{
				var insertCusRateCodeData = $@"
INSERT INTO {RefCusRateCodeSchema.Constants.TableName}
	({RefCusRateCodeSchema.Constants.PK}, {RefCusRateCodeSchema.Constants.ZY1_RateCode}, {RefCusRateCodeSchema.Constants.ZY1_Description}, {RefCusRateCodeSchema.Constants.ZY1_ZZR_RateType})
	VALUES (NEWID(), '{code}', '{description}', '{rateTypePK}')";

				using (var command = CargoWise.Data.Db.Connection.Command(insertCusRateCodeData))
				{
					command.ExecuteNonQuery();
				}
				Factory.Save();
			}
		}

		protected void CreateRefDataGrouping(string countryCode, string parentPK = "")
		{
			var sqlText = $@"
SELECT TOP 1 {RefDataGroupingSchema.Constants.ZZZ_DataGrouping}
FROM {RefDataGroupingSchema.Constants.TableName}
WHERE {RefDataGroupingSchema.Constants.ZZZ_DataGrouping} = '{countryCode}'";

			var dynamicBOs = new DynamicBusinessObjectCollection(Factory);
			dynamicBOs.Load(sqlText);

			if (!(dynamicBOs.Count > 0))
			{
				var insertParentDataGroupingData = parentPK.IsNullOrEmpty()
					? $@"INSERT INTO {RefDataGroupingSchema.Constants.TableName}
	({RefDataGroupingSchema.Constants.PK}, {RefDataGroupingSchema.Constants.ZZZ_DataGrouping}, {RefDataGroupingSchema.Constants.ZZZ_Description})
	VALUES (NEWID(), '{countryCode}', 'TEST COUNTRY Desc')"
					: $@"INSERT INTO {RefDataGroupingSchema.Constants.TableName}
	({RefDataGroupingSchema.Constants.PK}, {RefDataGroupingSchema.Constants.ZZZ_DataGrouping}, {RefDataGroupingSchema.Constants.ZZZ_Description}, {RefDataGroupingSchema.Constants.ZZZ_ZZZ_Grouping})
	VALUES (NEWID(), '{countryCode}', 'TEST COUNTRY Desc', '{parentPK}')";

				using (var command = CargoWise.Data.Db.Connection.Command(insertParentDataGroupingData))
				{
					command.ExecuteNonQuery();
				}

				Factory.Save();
			}
		}

		ZString CreateRefCusRateType(ZString code, ZString decscription, ZString country)
		{
			var sqlText = $@"
SELECT TOP 1 {RefCusRateTypeSchema.Constants.PK}
FROM {RefCusRateTypeSchema.Constants.TableName}
WHERE {RefCusRateTypeSchema.Constants.ZZR_RateType} = '{code}'";

			var dynamicBOs = new DynamicBusinessObjectCollection(Factory);
			dynamicBOs.Load(sqlText);

			ZString rateTypePK;
			if (!(dynamicBOs.Count > 0))
			{
				rateTypePK = ZGuid.NewZGuid().ToString();

				var insertCusRateTypeData = $@"
INSERT INTO {RefCusRateTypeSchema.Constants.TableName}
	({RefCusRateTypeSchema.Constants.PK}, {RefCusRateTypeSchema.Constants.ZZR_RateType}, {RefCusRateTypeSchema.Constants.ZZR_Description}, {RefCusRateTypeSchema.Constants.ZZR_IsPayable}, {RefCusRateTypeSchema.Constants.ZZR_ZZZ_NKDataGrouping}, {RefCusRateTypeSchema.Constants.ZZR_CustomsValueFormula})
	Values ('{rateTypePK}', '{code}', '{decscription}', 1, '{country}', '')";

				using (var command = CargoWise.Data.Db.Connection.Command(insertCusRateTypeData))
				{
					command.ExecuteNonQuery();
				}

				Factory.Save();
			}
			else
			{
				rateTypePK = dynamicBOs.Select(x => new ZGuid(x[RefCusRateTypeSchema.Constants.PK])).FirstOrDefault().ToString();
			}

			return rateTypePK;
		}

		public GlbBranch CreateBranchProxy(OrgHeader orgProxy, string branchCode)
		{
			var currentCompany = Factory.Load<GlbCompany>(Env.CurrentCompanyPK);
			var newBranch = currentCompany.Branches.AddNew();
			newBranch.GB_Code = branchCode;
			newBranch.GB_BranchName = orgProxy.OH_Code + " Branch";
			newBranch.GB_OH_OrgProxy = orgProxy.PK;

			return newBranch;
		}

		protected RatingContext CreateRatingContextWithDialogService(ILogger logger, IDialogService dialogService, bool isManualRateSelect = true, CW1RatesProvider cw1RatesProvider = null, WiseRatesProvider wiseRatesProvider = null)
		{
			var loggerDecorator = new LoggerDecorator(logger);
			var factory = new ReadOnlyBusinessObjectFactory();
			return new RatingContext(loggerDecorator, factory, cw1RatesProvider, wiseRatesProvider, dialogService, isManualRateSelect);
		}

		protected OrgHeader NewCarrierCreditor(string code)
		{
			var orgHeader = Helper.NewOrgHeader();
			orgHeader.OH_Code = code;
			orgHeader.OH_IsCreditor = true;
			orgHeader.OH_IsShippingLine = true;
			orgHeader.OH_IsShippingProvider = true;

			return orgHeader;
		}

		protected OrgHeader NewAgencyCreditor(OrgHeader carrier)
		{
			var agency = Helper.NewOrgHeader();
			agency.OH_Code = "AGENCY";
			agency.OH_IsCreditor = true;

			if (carrier != null)
			{
				var appointedAgentPorted = carrier.CarrierAppointedAgentPorts_Agency.AddNew();
				appointedAgentPorted.O5_PortOrCountry = "AUSYD";
				appointedAgentPorted.O5_OA_AgentOfficeAddress = agency.MainAddress.PK;
			}

			return agency;
		}

		#endregion

		#region Create Rates Service for Test

		protected Rate CreateTestRate(ZString category, ZString mode, string origin, string destination, ZString serviceLevel, ZString commodityCode, ZString containerType, ZString carrier, ZString client, ZString controllingCustomer)
			=> Helper.CreateTestRate(category, mode, origin, destination, serviceLevel, commodityCode, containerType, carrier, client, controllingCustomer);

		protected void SetContainerQuality(Rate wiseRate, string quality)
			=> Helper.SetContainerQuality(wiseRate, quality);

		protected RefChargeCode[] GetChargeCodesFromRates(params Rate[] rates)
			=> Helper.GetChargeCodesFromRates(rates);

		protected RatesServiceCharge CreateCharge(string code, string unit = null, string currency = null)
			=> Helper.CreateCharge(code, unit, currency);

		protected RatesServiceCharge CreateFlatCharge(string code, string currency, decimal flatRate)
		{
			var charge = CreateCharge(code, null, currency);
			charge.FlatRate = flatRate;
			charge.PerUnitRate = null;
			return charge;
		}

		protected RatesServiceCharge CreatePerUnitCharge(string code, string unit, string currency, decimal perUnit)
			=> Helper.CreatePerUnitCharge(code, unit, currency, perUnit);

		protected RatesServiceCharge CreateInclusiveCharge(string code, string currency, string includedIntoChargeCode)
		{
			return CreateFlatCharge(code, currency, 0m).OfType(ChargeType.Included).IncludedIn(includedIntoChargeCode);
		}

		protected RatesServiceCharge CreateMinCharge(string code, string currency, string unit, decimal minRate)
		{
			var charge = CreateCharge(code, string.Empty, currency);
			charge.MinRate = minRate;
			charge.Unit = unit;
			charge.PerUnitRate = null;
			return charge;
		}

		protected RatesServiceCharge CreatePercentageCharge(string code, string currency, decimal percentage, string appliesToCode)
		{
			var charge = CreateCharge(code);
			charge.Percentage = percentage;
			charge.PercentageAppliesTo = appliesToCode;
			charge.Currency = currency;
			return charge;
		}

		protected RatesServiceCharge CreateMinimumOrPerUnitCharge(string code, string unit, string currency, decimal minRate, decimal perUnit)
		{
			var charge = CreateMinCharge(code, currency, unit, minRate);
			charge.PerUnitRate = perUnit;
			return charge;
		}

		protected RatesServiceCharge CreateFlatOrPerUnitCharge(string code, string unit, string currency, decimal flatRate, decimal perUnit)
		{
			var charge = CreateFlatCharge(code, currency, flatRate);
			charge.PerUnitRate = perUnit;
			charge.Unit = unit;
			return charge;
		}

		protected RatesServiceCharge CreateSlidingChargeWithBreak(string code, string unit, string currency, string breakOperator, int? breakValue, decimal? perUnitRate, decimal? minRate = null, decimal? maxRate = null, decimal? flatRate = null)
		{
			var charge = CreateCharge(code, unit, currency);
			charge.BreakUnit = unit;

			charge.BreakOperator = breakOperator;
			charge.Break = breakValue;
			charge.PerUnitRate = perUnitRate;
			charge.MinRate = minRate;
			charge.MaxRate = maxRate;
			charge.FlatRate = flatRate;

			return charge;
		}

		protected OrgPatternMatchOverride CreateOrgPatternMatchGuid(string type, ZGuid codeMappingOrg, string foreignCode, ZGuid localGuid)
		{
			var result = Factory.NewWithValidTestData<OrgPatternMatchOverride>();
			result.OO_OH = codeMappingOrg;
			result.OO_Relationship = type;
			result.OO_ForeignCode = foreignCode;
			result.OO_LocalGuid = localGuid;

			return result;
		}

		protected OrgPatternMatchOverride CreateOrgPatternMatchString(string type, ZGuid codeMappingOrg, string foreignCode, string localCode)
		{
			var result = Factory.NewWithValidTestData<OrgPatternMatchOverride>();
			result.OO_OH = codeMappingOrg;
			result.OO_Relationship = type;
			result.OO_ForeignCode = foreignCode;
			result.OO_LocalCode = localCode;

			return result;
		}

		#endregion

		protected IFeatureControlManager MockURSFeatureHelper(bool enabled)
		{
			var ursRule = new RatingFeatureHelper.Urs.UrsFeatureRule { Enabled = enabled, Url = "https://fcm.cargowise.com" };
			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out ursRule)).Returns(true);
			featureControlMock
				.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.UniversalRatesService, CancellationToken.None))
				.Returns(Task.FromResult(featureDataMock.Object));

			return featureControlMock.Object;
		}
	}

	public class AssertionCharge : AssertionMock
	{
		public AssertionCharge() { }

		public AssertionCharge(Charge charge, List<SchemaColumn> comparisonColumns, List<int> customComparisonColumns)
			: base(charge, comparisonColumns, customComparisonColumns) { }

		Charge Charge
		{
			get { return IsActualLine ? bizO as Charge : null; }
		}

		public enum AssertionChargeSchema
		{
			CostCalculationDescription = 1,
			RevenueCalculationDescription = 2,
			RelatedJobNumber = 3,
		}

		protected override string GetCustomColumnName(int key)
		{
			return ((AssertionChargeSchema)key).ToString();
		}

		protected override bool CompareCustomValue(int customValueKey, AssertionMock assertionMock, AssertionMock realCharge)
		{
			var assertionCharge = (AssertionCharge)assertionMock;
			var actualCharge = (AssertionCharge)realCharge;

			switch ((AssertionChargeSchema)customValueKey)
			{
				case AssertionChargeSchema.CostCalculationDescription:
					return CheckCalcDescriptions(assertionCharge.CostCalculationDescription, actualCharge.CostCalculationDescription);

				case AssertionChargeSchema.RevenueCalculationDescription:
					return CheckCalcDescriptions(assertionCharge.RevenueCalculationDescription, actualCharge.RevenueCalculationDescription);

				case AssertionChargeSchema.RelatedJobNumber:
					return assertionCharge.RelatedJobNumber == actualCharge.RelatedJobNumber;

				default:
					throw new InvalidOperationException($"Unexpected key encountered: {customValueKey}");
			}
		}

		protected override SchemaColumnCollection GetAllSchemaColumns()
		{
			return JobChargeSchema.All;
		}

		protected override void AppendCalculationDescriptions(ZStringBuilder result)
		{
			result.Append(GetValueDescriptionLine("CostCalculationDescription", CostCalculationDescription));
			result.Append(GetValueDescriptionLine("RevenueCalculationDescription", RevenueCalculationDescription));
		}

		protected override T GetValue<T>(SchemaColumn column)
		{
			if (IsActualLine)
			{
				switch (column.Name)
				{
					case AutoJobCharge.Schema.JR_AC:
						return (T)(Charge.ChargeCode.AC_Code as IZType);

					case AutoJobCharge.Schema.JR_OH_SellAccount:
						return (T)(Charge.SellAccount == null ? ZString.Empty : Charge.SellAccount.OH_Code as IZType);

					case AutoJobCharge.Schema.JR_OH_CostAccount:
						return (T)(Charge.CostAccount == null ? ZString.Empty : Charge.CostAccount.OH_Code as IZType);
				}
			}

			return base.GetValue<T>(column);
		}

		#region Properties

		public ZString ChargeCode
		{
			get { return GetValue<ZString>(JobChargeSchema.JR_AC); }
			set { SetValue(JobChargeSchema.JR_AC, value); }
		}

		public ZString SellAccountCode
		{
			get { return GetValue<ZString>(JobChargeSchema.JR_OH_SellAccount); }
			set { SetValue(JobChargeSchema.JR_OH_SellAccount, value); }
		}

		public ZString CostCalculationDescription
		{
			get { return IsActualLine ? Charge.CostCalculationDescription.ToAscii() : GetCustomValue<ZString>((int)AssertionChargeSchema.CostCalculationDescription); }
			set { SetCustomValue((int)AssertionChargeSchema.CostCalculationDescription, value); }
		}

		public ZString RevenueCalculationDescription
		{
			get { return IsActualLine ? Charge.RevenueCalculationDescription.ToAscii() : GetCustomValue<ZString>((int)AssertionChargeSchema.RevenueCalculationDescription); }
			set { SetCustomValue((int)AssertionChargeSchema.RevenueCalculationDescription, value); }
		}

		public ZDecimal JR_LocalSellAmt
		{
			get { return GetValue<ZDecimal>(JobChargeSchema.JR_LocalSellAmt); }
			set { SetValue(JobChargeSchema.JR_LocalSellAmt, value); }
		}

		public ZDecimal JR_LocalCostAmt
		{
			get { return GetValue<ZDecimal>(JobChargeSchema.JR_LocalCostAmt); }
			set { SetValue(JobChargeSchema.JR_LocalCostAmt, value); }
		}

		public ZBool JR_CostRated
		{
			get { return GetValue<ZBool>(JobChargeSchema.JR_CostRated); }
			set { SetValue(JobChargeSchema.JR_CostRated, value); }
		}

		public ZBool JR_SellRated
		{
			get { return GetValue<ZBool>(JobChargeSchema.JR_SellRated); }
			set { SetValue(JobChargeSchema.JR_SellRated, value); }
		}

		public ZDecimal JR_OSSellAmt
		{
			get { return GetValue<ZDecimal>(JobChargeSchema.JR_OSSellAmt); }
			set { SetValue(JobChargeSchema.JR_OSSellAmt, value); }
		}

		public ZDecimal JR_EstimatedRevenue
		{
			get { return GetValue<ZDecimal>(JobChargeSchema.JR_EstimatedRevenue); }
			set { SetValue(JobChargeSchema.JR_EstimatedRevenue, value); }
		}

		public ZDecimal JR_AgentDeclaredSellAmt
		{
			get { return GetValue<ZDecimal>(JobChargeSchema.JR_AgentDeclaredSellAmt); }
			set { SetValue(JobChargeSchema.JR_AgentDeclaredSellAmt, value); }
		}

		public ZString JR_RX_NKSellCurrency
		{
			get { return GetValue<ZString>(JobChargeSchema.JR_RX_NKSellCurrency); }
			set { SetValue(JobChargeSchema.JR_RX_NKSellCurrency, value); }
		}

		public ZDecimal JR_OSCostAmt
		{
			get { return GetValue<ZDecimal>(JobChargeSchema.JR_OSCostAmt); }
			set { SetValue(JobChargeSchema.JR_OSCostAmt, value); }
		}

		public ZDecimal JR_EstimatedCost
		{
			get { return GetValue<ZDecimal>(JobChargeSchema.JR_EstimatedCost); }
			set { SetValue(JobChargeSchema.JR_EstimatedCost, value); }
		}

		public ZDecimal JR_AgentDeclaredCostAmt
		{
			get { return GetValue<ZDecimal>(JobChargeSchema.JR_AgentDeclaredCostAmt); }
			set { SetValue(JobChargeSchema.JR_AgentDeclaredCostAmt, value); }
		}

		public ZString JR_RX_NKCostCurrency
		{
			get { return GetValue<ZString>(JobChargeSchema.JR_RX_NKCostCurrency); }
			set { SetValue(JobChargeSchema.JR_RX_NKCostCurrency, value); }
		}

		public ZString JR_Desc
		{
			get { return GetValue<ZString>(JobChargeSchema.JR_Desc); }
			set { SetValue(JobChargeSchema.JR_Desc, value); }
		}

		public ZBool JR_SellRatingOverride
		{
			get { return GetValue<ZBool>(JobChargeSchema.JR_SellRatingOverride); }
			set { SetValue(JobChargeSchema.JR_SellRatingOverride, value); }
		}

		public ZBool JR_CostRatingOverride
		{
			get { return GetValue<ZBool>(JobChargeSchema.JR_CostRatingOverride); }
			set { SetValue(JobChargeSchema.JR_CostRatingOverride, value); }
		}

		public ZString CostAccountCode
		{
			get { return GetValue<ZString>(JobChargeSchema.JR_OH_CostAccount); }
			set { SetValue(JobChargeSchema.JR_OH_CostAccount, value); }
		}

		public ZGuid JR_GB_InternalBranch
		{
			get { return GetValue<ZGuid>(JobChargeSchema.JR_GB_InternalBranch); }
			set { SetValue(JobChargeSchema.JR_GB_InternalBranch, value); }
		}

		public ZGuid JR_GE_InternalDept
		{
			get { return GetValue<ZGuid>(JobChargeSchema.JR_GE_InternalDept); }
			set { SetValue(JobChargeSchema.JR_GE_InternalDept, value); }
		}

		public ZGuid JR_JH_InternalJob
		{
			get { return GetValue<ZGuid>(JobChargeSchema.JR_JH_InternalJob); }
			set { SetValue(JobChargeSchema.JR_JH_InternalJob, value); }
		}

		public ZString JR_InvoiceType
		{
			get { return GetValue<ZString>(JobChargeSchema.JR_InvoiceType); }
			set { SetValue(JobChargeSchema.JR_InvoiceType, value); }
		}

		public ZString RelatedJobNumber
		{
			get { return IsActualLine ? Charge.JR_Calc_RelatedJobNumber : GetCustomValue<ZString>((int)AssertionChargeSchema.RelatedJobNumber); }
			set { SetCustomValue((int)AssertionChargeSchema.RelatedJobNumber, value); }
		}

		public ZString SellReferenceNumber
		{
			get { return GetValue<ZString>(JobChargeSchema.JR_SellReference); }
			set { SetValue(JobChargeSchema.JR_SellReference, value); }
		}

		public ZString CostReferenceNumber
		{
			get { return GetValue<ZString>(JobChargeSchema.JR_CostReference); }
			set { SetValue(JobChargeSchema.JR_CostReference, value); }
		}

		#endregion
	}

	public class AssertionCost : AssertionMock
	{
		public AssertionCost() { }

		public AssertionCost(JobConsolCost cost, List<SchemaColumn> comparisonColumns, List<int> customComparisonColumns)
			: base(cost, comparisonColumns, customComparisonColumns) { }

		public enum AssertionCostSchema
		{
			CostCalculationDescription = 1
		}

		protected override string GetCustomColumnName(int key)
		{
			return ((AssertionCostSchema)key).ToString();
		}

		JobConsolCost Cost
		{
			get { return IsActualLine ? bizO as JobConsolCost : null; }
		}

		protected override SchemaColumnCollection GetAllSchemaColumns()
		{
			return JobConsolCostSchema.All;
		}

		protected override void AppendCalculationDescriptions(ZStringBuilder result)
		{
			result.Append(GetValueDescriptionLine("CostCalculationDescription", CostCalculationDescription));
		}

		protected override T GetValue<T>(SchemaColumn column)
		{
			if (IsActualLine)
			{
				switch (column.Name)
				{
					case AutoJobConsolCost.Schema.E6_AC_ChargeCode:
						return (T)(Cost.ChargeCode.AC_Code as IZType);
				}
			}

			return base.GetValue<T>(column);
		}

		protected override bool CompareCustomValue(int customValueKey, AssertionMock assertionMock, AssertionMock realCharge)
		{
			var assertionCharge = (AssertionCost)assertionMock;
			var actualCharge = (AssertionCost)realCharge;

			switch ((AssertionCostSchema)customValueKey)
			{
				case AssertionCostSchema.CostCalculationDescription:
					return CheckCalcDescriptions(assertionCharge.CostCalculationDescription, actualCharge.CostCalculationDescription);

				default:
					throw new InvalidOperationException($"Unexpected key encountered: {customValueKey}");
			}
		}

		#region Properties

		public ZString E6_RatingBehaviour
		{
			get { return GetValue<ZString>(JobConsolCostSchema.E6_RatingBehaviour); }
			set { SetValue(JobConsolCostSchema.E6_RatingBehaviour, value); }
		}

		public ZString ChargeCode
		{
			get { return GetValue<ZString>(JobConsolCostSchema.E6_AC_ChargeCode); }
			set { SetValue(JobConsolCostSchema.E6_AC_ChargeCode, value); }
		}

		public ZString CostCalculationDescription
		{
			get { return IsActualLine ? (ZString)ORtfTextUtil.RtfToText(Cost.CostCalculationDescription.ToAscii()) : GetCustomValue<ZString>((int)AssertionCostSchema.CostCalculationDescription); }
			set { SetCustomValue((int)AssertionCostSchema.CostCalculationDescription, value); }
		}

		public ZDecimal E6_OSCostAmount
		{
			get { return GetValue<ZDecimal>(JobConsolCostSchema.E6_OSCostAmount); }
			set { SetValue(JobConsolCostSchema.E6_OSCostAmount, value); }
		}

		public ZDecimal E6_LocalCostAmount
		{
			get { return GetValue<ZDecimal>(JobConsolCostSchema.E6_LocalCostAmount); }
			set { SetValue(JobConsolCostSchema.E6_LocalCostAmount, value); }
		}

		public ZString E6_RX_NKCurrency
		{
			get { return GetValue<ZString>(JobConsolCostSchema.E6_RX_NKCurrency); }
			set { SetValue(JobConsolCostSchema.E6_RX_NKCurrency, value); }
		}

		public ZString E6_ApportionmentMethod
		{
			get { return GetValue<ZString>(JobConsolCostSchema.E6_ApportionmentMethod); }
			set { SetValue(JobConsolCostSchema.E6_ApportionmentMethod, value); }
		}

		public ZGuid E6_OH_Creditor
		{
			get { return GetValue<ZGuid>(JobConsolCostSchema.E6_OH_Creditor); }
			set { SetValue(JobConsolCostSchema.E6_OH_Creditor, value); }
		}

		#endregion
	}

	public abstract class AssertionMock
	{
		protected AssertionMock() { }

		protected AssertionMock(BusinessObject bizO, List<SchemaColumn> comparisonColumns, List<int> customComparisonColumns)
		{
			this.comparisonColumns = comparisonColumns;
			this.customComparisonColumns = customComparisonColumns;
			this.bizO = bizO;
		}

		readonly Dictionary<SchemaColumn, IZType> jobChargeValues = new Dictionary<SchemaColumn, IZType>();
		readonly Dictionary<int, IZType> customValues = new Dictionary<int, IZType>();

		protected abstract SchemaColumnCollection GetAllSchemaColumns();
		protected abstract void AppendCalculationDescriptions(ZStringBuilder result);

		protected bool IsActualLine { get { return bizO != null; } }

		protected BusinessObject bizO;

		public List<SchemaColumn> ComparisonColumns
		{
			get { return IsActualLine ? comparisonColumns : jobChargeValues.Keys.ToList(); }
		}
		readonly List<SchemaColumn> comparisonColumns;

		public List<int> CustomComparisonColumns
		{
			get { return IsActualLine ? customComparisonColumns : customValues.Keys.ToList(); }
		}
		readonly List<int> customComparisonColumns;

		public static IEqualityComparer<AssertionMock> Comparer
		{
			get { return new LambdaComparer<AssertionMock>(Compare, x => 0); }
		}

		static bool Compare(AssertionMock s1, AssertionMock s2)
		{
			var charge = s1.IsActualLine ? s1 : s2;
			var mockCharge = s1.IsActualLine ? s2 : s1;

			var result = mockCharge.jobChargeValues.All(x => mockCharge.GetValue<IZType>(x.Key).Equals(charge.GetValue<IZType>(x.Key)));

			foreach (var customValueKey in mockCharge.customValues.Keys)
			{
				result = result && mockCharge.CompareCustomValue(customValueKey, mockCharge, charge);
			}

			return result;
		}

		protected virtual bool CompareCustomValue(int customValueKey, AssertionMock assertionMock, AssertionMock realCharge)
		{
			return false;
		}

		protected static bool CheckCalcDescriptions(string assertionDescriptions, string actualDescriptions)
		{
			if (string.IsNullOrEmpty(assertionDescriptions))
			{
				return string.IsNullOrEmpty(actualDescriptions);
			}

			var assertionDescriptionsArray = assertionDescriptions
							 .Split(new[] { "\r\n", "\n\t" }, StringSplitOptions.RemoveEmptyEntries)
							 .Where(x => !string.IsNullOrEmpty(x.Trim()));

			foreach (var assertionDesc in assertionDescriptionsArray)
			{
				if (!actualDescriptions.Contains(assertionDesc))
				{
					return false;
				}
			}

			return true;
		}

		public override string ToString()
		{
			List<SchemaColumn> columns;
			if (IsActualLine)
			{
				columns = GetAllSchemaColumns()
										.Where(x => !x.IsPKColumn
													&& !x.Name.Contains(AuditDetailsColumns.SystemCreateTimeUtc)
													&& !x.Name.Contains(AuditDetailsColumns.SystemCreateUser)
													&& !x.Name.Contains(AuditDetailsColumns.SystemLastEditTimeUtc)
													&& !x.Name.Contains(AuditDetailsColumns.SystemLastEditUser)
													&& !x.Name.Contains("IsValid"))
										.OrderBy(x => !ComparisonColumns.Contains(x))
										.ToList();
			}
			else
			{
				columns = ComparisonColumns;
			}

			var result = new ZStringBuilder(columns.Select(x => GetValueDescriptionLine(x.Name, GetValue<IZType>(x))));

			if (IsActualLine)
			{
				AppendCalculationDescriptions(result);
			}
			else
			{
				foreach (var customValueKey in customValues.Keys)
				{
					result.Append(GetValueDescriptionLine(GetCustomColumnName(customValueKey), GetCustomValue<IZType>(customValueKey)));
				}
			}

			return result.ToStringWithNewLineBetweenAppends();
		}

		protected abstract string GetCustomColumnName(int key);

		protected static string GetValueDescriptionLine(string name, IZType value)
		{
			return string.Format("{0}:", name).PadRight(40) + value;
		}

		protected void SetValue(SchemaColumn column, IZType value)
		{
			if (IsActualLine)
			{
				throw new NotSupportedException("Actual line is supposed to be a read-only autorating result in this test");
			}

			jobChargeValues[column] = value;
		}

		protected virtual T GetValue<T>(SchemaColumn column) where T : IZType
		{
			return (T)(IsActualLine
							? bizO[column]
							: jobChargeValues.ContainsKey(column)
								? jobChargeValues[column]
								: null);
		}

		protected void SetCustomValue(int column, IZType value)
		{
			if (IsActualLine)
			{
				throw new NotSupportedException("Actual line is supposed to be a read-only autorating result in this test");
			}

			customValues[column] = value;
		}

		protected T GetCustomValue<T>(int column) where T : IZType
		{
			if (IsActualLine)
			{
				throw new NotSupportedException("Custom value getters for the actual line should be implemented in property getters");
			}

			return (T)(customValues.ContainsKey(column)
				? customValues[column]
				: null);
		}
	}

	public static class JobTestExtensions
	{
		public static ExchangeRate AddExRate(this Job job, string currencyCode, decimal rate)
		{
			var result = job.ExchangeRates.AddNew();
			result.JF_RX_NKRateCurrency = currencyCode;
			result.JF_BaseRate = rate;
			return result;
		}

		public static ExchangeRate AddExRate(this Job job, string currencyCode, decimal rate, ZGuid organizationPK, ExchangeRateOrgTypeEnum orgRole)
		{
			var result = job.AddExRate(currencyCode, rate);
			result.JF_OH_Org = organizationPK;
			result.OrgType = orgRole;
			return result;
		}

		public static void ApplyCustomQuickCalculator(this IQuickCalculatorCharge charge, IAutoRating hostRatingAdapter, ZDecimal costAmount, ZDecimal sellAmount)
		{
			var quickCalculator = new JobChargeQuickCalculateBusinessObject(hostRatingAdapter, charge);
			quickCalculator.QuantityDescription = "Custom";
			quickCalculator.Quantity = 1m;

			if (!costAmount.IsEmpty)
			{
				quickCalculator.UpdateCost = true;
				quickCalculator.CostRate = costAmount;
			}

			if (!sellAmount.IsEmpty)
			{
				quickCalculator.UpdateSell = true;
				quickCalculator.SellRate = sellAmount;
			}

			quickCalculator.SetCalculationResults();
		}
	}
}
