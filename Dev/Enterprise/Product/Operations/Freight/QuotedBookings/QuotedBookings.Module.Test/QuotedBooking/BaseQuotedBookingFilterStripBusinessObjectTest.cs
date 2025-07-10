using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.GUI;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Test;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Module.Test
{
	[TestedType(typeof(BaseQuotedBookingFilterStripBusinessObject))]
	public class BaseQuotedBookingFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Workflow

		public void TestWorkflowMilestoneFiltersPresent()
		{
			var milestoneFilter = GetNewFilterStripBusinessObject();
			AssertNotNull("You must use WorkflowFilterStripsHelper to add Workflow filter strips", milestoneFilter["Milestone Date"]);
		}

		public void TestWorkflowCustomFieldsFilters()
		{
			var filter = GetNewFilterStripBusinessObject();
			AssertNull(filter["custom text"]);
			AssertNull(filter["custom int"]);
			AssertNull(filter["custom decimal"]);
			AssertNull(filter["custom datetime"]);
			AssertNull(filter["custom shipment string"]);
			CreateQuotedBookingWorkflowWithCustomFields();
			filter = GetNewFilterStripBusinessObject();
			AssertNotNull(filter["custom text"]);
			AssertNotNull(filter["custom int"]);
			AssertNotNull(filter["custom decimal"]);
			AssertNotNull(filter["custom datetime"]);
			AssertNull(filter["custom shipment string"]);
			var workflowCustomFieldsFilter = (ModuleTextFilter)filter["custom text"];
			workflowCustomFieldsFilter.Property = "BOOM";
			workflowCustomFieldsFilter.IsActive = true;
			AssertNoExceptionThrown(() => Factory.Load<ViewQuotedBooking>(filter.Filter));
		}

		void CreateQuotedBookingWorkflowWithCustomFields()
		{
			ProcessTaskTemplate processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "QBK";
			GenCustomColumnDefinition customField1 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "custom text";
			customField1.XC_Type = AddOnColumnDataType.Codes.String;
			GenCustomColumnDefinition customField2 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField2.XC_Name = "custom int";
			customField2.XC_Type = AddOnColumnDataType.Codes.Integer;
			GenCustomColumnDefinition customField3 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField3.XC_Name = "custom decimal";
			customField3.XC_Type = AddOnColumnDataType.Codes.Decimal;
			GenCustomColumnDefinition customField4 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField4.XC_Name = "custom datetime";
			customField4.XC_Type = AddOnColumnDataType.Codes.Datetime;
			ProcessTaskTemplate processTaskTemplate2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate2.P0_ProcessType = "SHP";
			GenCustomColumnDefinition customField5 = processTaskTemplate2.GenCustomColumnDefinitions.AddNew();
			customField5.XC_Name = "custom shipment string";
			customField5.XC_Type = AddOnColumnDataType.Codes.String;
			Factory.Save();
			WorkflowCustomFieldsFilter.ClearCache();
		}

		#endregion

		#region Implementation

		#region Asserters

		protected void AssertBookingCollectionIsFiltered(FilterStripBusinessObject filter, params QuotedBooking[] expectedQuotedBookings)
		{
			AssertBookingCollectionIsFiltered(ZString.Empty, filter, expectedQuotedBookings);
		}

		protected void AssertBookingCollectionIsFiltered(ZString message, FilterStripBusinessObject filter, params QuotedBooking[] expectedQuotedBookings)
		{
			var collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			var expectedResults = expectedQuotedBookings.Select(quotedBooking => quotedBooking.PK);
			var actualResults = collection.Select(quotedBooking => quotedBooking.PK);
			AssertContainsExactElementsInAnyOrder(message, expectedResults, actualResults);
		}

		#endregion

		#region Additional Reference

		protected string GetAdditionalReferenceValue(ViewQuotedBooking booking, string country, string type)
		{
			CommonShipment shipment = booking.QuotedBooking.Booking;
			if (shipment != null)
			{
				foreach (CusEntryNumber number in shipment.Numbers)
				{
					if (number.CE_RN_NKCountryCode == country && number.CE_EntryType == type)
					{
						return number.CE_EntryNum;
					}
				}
			}

			return null;
		}

		protected void AddAdditionalReference(QuotedBooking booking, string country, string type, string number)
		{
			CusEntryNumber entryNumber = booking.Booking.Numbers.AddNew();
			entryNumber.CE_RN_NKCountryCode = country;
			entryNumber.CE_EntryType = type;
			entryNumber.CE_EntryNum = number;
		}

		#endregion

		#region Setup Related Parties

		protected OrgHeader GetOrgHeader(ZString fullName)
		{
			OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();
			result.OH_FullName = fullName;
			return result;
		}

		protected OrgRelatedParty GetOrgRelatedParty(OrgHeader parent, OrgHeader relatedParty)
		{
			OrgRelatedParty result = Factory.NewWithValidTestData<OrgRelatedParty>();
			result.PR_OH_Parent = parent.PK;
			result.PR_OH_RelatedParty = relatedParty.PK;
			result.PR_PartyType = ZString.Empty;
			result.PR_FreightDirection = ZString.Empty;
			result.PR_FreightTransportMode = ZString.Empty;
			result.PR_FreightContainerMode = ZString.Empty;
			return result;
		}

		protected ViewQuotedBooking GetQuotedBookingWithConsignor(ZString companyName)
		{
			ViewQuotedBooking result = GetQuotedBooking((++jobNumberIndex).ToString());
			QuotedBooking quotedbooking = result.QuotedBooking;
			JobDocAddress docAddress = quotedbooking.ConsignorDocumentaryAddress;
			docAddress.E2_OA_Address = GetOrgHeader(companyName).MainAddress.PK;
			return result;
		}

		protected ViewQuotedBooking GetQuotedBookingWithConsignee(ZString companyName)
		{
			ViewQuotedBooking result = GetQuotedBooking((++jobNumberIndex).ToString());
			QuotedBooking quotedbooking = result.QuotedBooking;
			JobDocAddress docAddress = quotedbooking.ConsigneeDocumentaryAddress;
			docAddress.E2_OA_Address = GetOrgHeader(companyName).MainAddress.PK;
			return result;
		}

		protected ViewQuotedBooking GetOneOffQuoteWithConsignee(OrgHeader company)
		{
			ViewQuotedBooking result = GetQuotedBooking((++jobNumberIndex).ToString());
			QuotedBooking quotedbooking = result.QuotedBooking;
			quotedbooking.Quote.CurrentOneOffQuote.DeliveryDocAddress.E2_OA_Address = company.MainAddress.PK;
			return result;
		}

		protected ViewQuotedBooking GetOneOffQuoteWithConsignor(OrgHeader company)
		{
			ViewQuotedBooking result = GetQuotedBooking((++jobNumberIndex).ToString());
			QuotedBooking quotedbooking = result.QuotedBooking;
			quotedbooking.Quote.CurrentOneOffQuote.PickUpDocAddress.E2_OA_Address = company.MainAddress.PK;
			return result;
		}

		protected ViewQuotedBooking GetQuotedBookingWithClient(ZString companyName)
		{
			ViewQuotedBooking result = GetQuotedBooking((++jobNumberIndex).ToString());
			result.QuotedBooking.ClientPK = GetOrgHeader(companyName).PK;
			return result;
		}

		protected ViewQuotedBooking GetQuotedBooking(ZString numberSuffix)
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.ClientPK = GetOrgHeader("Test Client").PK;
			quotedBooking.Mode = Core.Constants.RateMode.FCL;
			quotedBooking.TryLoadOrCreateJob();
			quotedBooking.Job.JH_GE = Env.CurrentDepartment.PK;
			quotedBooking.Job.JH_JobNum = "JobNumber" + numberSuffix;
			Factory.Save();
			ViewQuotedBooking result = Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, quote.PK));
			Asserter.AddToScope(result);
			return result;
		}

		int jobNumberIndex;
		protected FilterStripAsserter<ViewQuotedBooking> Asserter
		{
			get
			{
				return asserter ?? (asserter = new FilterStripAsserter<ViewQuotedBooking>(Factory, view =>
				{
					view.QuotedBooking.TryLoadOrCreateJob();
					return view.QuotedBooking.Job.JH_JobNum;
				}

				));
			}
		}

		FilterStripAsserter<ViewQuotedBooking> asserter;

		#endregion

		#region TestOrg

		public OrgHeader TestOrg
		{
			get
			{
				if (testOrg == null)
				{
					testOrg = Factory.NewWithValidTestData<OrgHeader>();
					testOrg.OH_FullName = "Test Client #1";
					testOrg.MainAddress.OA_Address1 = "101 Fake Street";
					testOrg.MainAddress.OA_City = "Sydney";
					testOrg.MainAddress.OA_State = "NSW";
					testOrg.MainAddress.OA_PostCode = "2000";
					testOrg.OH_RL_NKClosestPort = "AUSYD";
					testOrg.OH_Code = "TESTORG1";
				}

				return testOrg;
			}
		}

		OrgHeader testOrg;

		#endregion

		#region TestOrg2

		public OrgHeader TestOrg2
		{
			get
			{
				if (testOrg2 == null)
				{
					testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
					testOrg2.OH_FullName = "Test Client #2";
					testOrg2.MainAddress.OA_Address1 = "102 Fake Street";
					testOrg2.MainAddress.OA_City = "Sydney";
					testOrg2.MainAddress.OA_State = "NSW";
					testOrg2.MainAddress.OA_PostCode = "2000";
					testOrg2.OH_RL_NKClosestPort = "AUSYD";
					testOrg2.OH_Code = "TESTORG2";
				}

				return testOrg2;
			}
		}

		OrgHeader testOrg2;
		public OrgHeader TestOrg3
		{
			get
			{
				if (testOrg3 == null)
				{
					testOrg3 = Factory.NewWithValidTestData<OrgHeader>();
					testOrg3.OH_FullName = "Test Client #3";
					testOrg3.MainAddress.OA_Address1 = "102 Fake Street";
					testOrg3.MainAddress.OA_City = "Sydney";
					testOrg3.MainAddress.OA_State = "NSW";
					testOrg3.MainAddress.OA_PostCode = "2000";
					testOrg3.OH_RL_NKClosestPort = "AUSYD";
					testOrg3.OH_Code = "TESTORG3";
				}

				return testOrg3;
			}
		}

		OrgHeader testOrg3;

		#endregion

		#region Locations

		protected static ZString HomePort
		{
			get
			{
				if (!GlbBranch.CurrentBranch.GB_RL_NKHomePort.IsEmpty)
				{
					return GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				}

				return "AUSYD";
			}
		}

		protected static ZString OverseasPort
		{
			get
			{
				if (GlbBranch.CurrentBranch.GB_RL_NKHomePort != "SGSIN")
				{
					return "SGSIN";
				}
				else
				{
					return "USLAX";
				}
			}
		}

		#endregion

		#region Vessels and Voyage

		#region TestVessel1

		protected RefVessel TestVessel1
		{
			get
			{
				return testVessel1 ?? (testVessel1 = LoadOrCreateVessel("APL EMERALD", "8610033"));
			}
		}

		RefVessel testVessel1;

		#endregion

		#region TestVessel2

		protected RefVessel TestVessel2
		{
			get
			{
				return testVessel2 ?? (testVessel2 = LoadOrCreateVessel("The Black Pearl", "8610033"));
			}
		}

		RefVessel testVessel2;

		#endregion

		RefVessel LoadOrCreateVessel(string name, string lloydsNumber)
		{
			var vessel = RefVessel.LookupVesselByName(name, Factory).FirstOrDefault();
			if (vessel == null)
			{
				vessel = Factory.New<RefVessel>();
				vessel.RV_LloydsNumber = lloydsNumber;
				vessel.RV_Name = name;
			}

			return vessel;
		}

		protected JobVoyage CreateVoyage(ZString transportMode, RefVessel vessel, ZString voyageFlight)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = transportMode;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = voyageFlight;
			return voyage;
		}

		protected JobSailing GetOrCreateSailing(JobVoyage voyage, ZString load, ZString discharge)
		{
			if (voyage.Origins.GetOriginFromLoading(load) == null)
			{
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = load;
			}

			if (voyage.Destinations.GetDestinationFromDischarge(discharge) == null)
			{
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = discharge;
			}

			voyage.GenerateSailings();
			return voyage.Sailings.GetSailingFromLoadAndDischarge(load, discharge);
		}

		#endregion

		#region Quote and Booking Creators

		protected QuotedBooking CreateQuotedBookingWithSailing(JobSailing sailing)
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = Factory.New<ForwardingShipment>();
			booking.JS_IsBooking = true;
			booking.JS_IsForwardRegistered = ZBool.False;
			booking.JS_A_BKD = ZDateTime.Now;
			booking.JS_TransportMode = sailing.Voyage.JV_AirSeaRoad;
			booking.JS_RL_NKOrigin = sailing.Origin.JA_RL_NKPortOfLoading;
			booking.JS_RL_NKDestination = sailing.Destination.JB_RL_NKPortOfDischarge;
			booking.JS_JX = sailing.PK;
			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			quotedBooking.Mode = Core.Constants.RateMode.FCL;
			quotedBooking.TryLoadOrCreateJob();
			quotedBooking.Job.JH_GE = Env.CurrentDepartment.PK;
			return quotedBooking;
		}

		protected QuotedBooking CreateQuotedBooking(ZGuid orgPK)
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.ClientPK = orgPK;
			quotedBooking.Mode = Core.Constants.RateMode.FCL;
			quotedBooking.TryLoadOrCreateJob();
			quotedBooking.Job.JH_GE = Env.CurrentDepartment.PK;
			return quotedBooking;
		}

		protected QuotedBooking CreateQuotedBooking()
		{
			return CreateQuotedBooking(Factory.NewWithValidTestData<OrgHeader>().PK);
		}

		protected QuotedBooking CreateQuotedBookingWithIndirectLinkToPorts()
		{
			var result = CreateQuotedBooking();
			var consol = result.Booking.Consols.AddNew();
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			var transport = Factory.NewWithValidTestData<Transport>();
			consol.Transports.Add(transport);
			transport.JW_RL_NKLoadPort = HomePort;
			transport.JW_RL_NKDiscPort = OverseasPort;
			return result;
		}

		protected QuotedBooking CreateQuoteOnly(ZGuid? orgPK = null)
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			quotedBooking.ClientPK = orgPK ?? Factory.NewWithValidTestData<OrgHeader>().PK;
			quotedBooking.Mode = Core.Constants.RateMode.FCL;
			quotedBooking.TryLoadOrCreateJob();
			quotedBooking.Job.JH_GE = Env.CurrentDepartment.PK;
			return quotedBooking;
		}

		protected class CarrierCreditorPair
		{
			public ZGuid Carrier { get; set; } = ZGuid.Empty;
			public ZGuid Creditor { get; set; } = ZGuid.Empty;
		}

		protected QuotedBooking CreateQuoteOnlyWithPossibleCarrier(ZGuid orgPK, CarrierCreditorPair[] pairs)
		{
			var quotedBooking = CreateQuoteOnly(orgPK);
			foreach (var pair in pairs)
			{
				var possibleCarrier = quotedBooking.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
				possibleCarrier.TTC_OH_Carrier = pair.Carrier;
				possibleCarrier.TTC_OH_Creditor = pair.Creditor;
			}
			return quotedBooking;
		}

		protected QuotedBooking CreateQuoteOnly(QuotedBooking.QuoteState quoteState, ZGuid? orgPK = null)
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, quoteState);
			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			quotedBooking.ClientPK = orgPK ?? Factory.NewWithValidTestData<OrgHeader>().PK;
			quotedBooking.Mode = Core.Constants.RateMode.FCL;
			quotedBooking.TryLoadOrCreateJob();
			quotedBooking.Job.JH_GE = Env.CurrentDepartment.PK;
			return quotedBooking;
		}

		protected Quote CreateQuote()
		{
			return QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
		}

		protected QuotedBooking CreateBookingOnly()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			quotedBooking.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			quotedBooking.Mode = Core.Constants.RateMode.FCL;
			return quotedBooking;
		}

		#endregion

		#region ViewQuotedBooking

		protected ViewQuotedBooking GetView(QuotedBooking booking)
		{
			return Factory.Load<ViewQuotedBooking>(booking.ViewPK);
		}

		protected ViewQuotedBookingCollection FetchLastEditUserFilterResults(ZString login)
		{
			var filter = GetNewFilterStripBusinessObject();
			((ModuleNkFilter)filter["Last Edit User"]).Property = login;
			((ModuleNkFilter)filter["Last Edit User"]).IsActive = true;
			var collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			return collection;
		}

		#endregion

		#region Test Clients and Users

		protected GlbStaff CreateTestUser(ZString userCode, ZString userLogin)
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_Code = userCode;
			user.GS_LoginName = userLogin;
			return user;
		}

		protected Dictionary<string, QuotedBooking[]> GetClientNameData()
		{
			if (clientNameData != null)
			{
				return clientNameData;
			}

			clientNameData = new Dictionary<string, QuotedBooking[]>();
			var array = new QuotedBooking[4];
			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Job.LocalChargesPK = TestOrg.PK;
			quotedBooking1.Quote.TH_OH = Guid.Empty;
			array[0] = quotedBooking1;
			AssertEquals("Test Client #1", quotedBooking1.ClientFullName);
			var quotedBooking2 = CreateQuotedBooking();
			quotedBooking2.Job.LocalChargesPK = TestOrg.PK;
			quotedBooking2.Quote.TH_OH = TestOrg2.PK;
			array[1] = quotedBooking2;
			AssertEquals("Test Client #1", quotedBooking2.ClientFullName);
			var quotedBooking3 = CreateBookingOnly();
			quotedBooking3.TryLoadOrCreateJob();
			quotedBooking3.Job.LocalChargesPK = TestOrg.PK;
			array[2] = quotedBooking3;
			AssertEquals("Test Client #1", quotedBooking3.ClientFullName);
			var quotedBooking4 = CreateQuoteOnly();
			quotedBooking4.Quote.TH_OH = TestOrg.PK;
			array[3] = quotedBooking4;
			clientNameData.Add("Client1", array);
			AssertEquals("Test Client #1", quotedBooking4.ClientFullName);
			array = new QuotedBooking[4];
			var quotedBooking5 = CreateQuotedBooking();
			quotedBooking5.Job.LocalChargesPK = TestOrg2.PK;
			quotedBooking5.Quote.TH_OH = Guid.Empty;
			array[0] = quotedBooking5;
			AssertEquals("Test Client #2", quotedBooking5.ClientFullName);
			var quotedBooking6 = CreateQuotedBooking();
			quotedBooking6.Job.LocalChargesPK = TestOrg2.PK;
			quotedBooking6.Quote.TH_OH = TestOrg.PK;
			array[1] = quotedBooking6;
			AssertEquals("Test Client #2", quotedBooking6.ClientFullName);
			var quotedBooking7 = CreateBookingOnly();
			quotedBooking7.TryLoadOrCreateJob();
			quotedBooking7.Job.LocalChargesPK = TestOrg2.PK;
			array[2] = quotedBooking7;
			AssertEquals("Test Client #2", quotedBooking7.ClientFullName);
			var quotedBooking8 = CreateQuoteOnly();
			quotedBooking8.Quote.TH_OH = TestOrg2.PK;
			array[3] = quotedBooking8;
			clientNameData.Add("Client2", array);
			AssertEquals("Test Client #2", quotedBooking8.ClientFullName);
			array = new QuotedBooking[1];
			var quotedBooking9 = CreateQuotedBooking();
			quotedBooking9.Job.LocalChargesPK = Guid.Empty;
			quotedBooking9.Quote.TH_OH = Guid.Empty;
			array[0] = quotedBooking9;
			clientNameData.Add("Empty", array);
			AssertEquals(string.Empty, quotedBooking9.ClientFullName);
			Factory.Save();
			return clientNameData;
		}

		Dictionary<string, QuotedBooking[]> clientNameData;
		protected void CheckCollectionForUseClientNameFilter(string property, SQLComparisonOperator comparisonOperator, string message, IEnumerable<QuotedBooking> expected)
		{
			var filter = GetNewFilterStripBusinessObject();
			((ModuleTextFilter)filter["Client Name"]).IsActive = true;
			((ModuleTextFilter)filter["Client Name"]).Property = property;
			((ModuleTextFilter)filter["Client Name"]).SqlComparisonOperator = comparisonOperator;
			var collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			var result = collection.Select(c => c.QuotedBooking);
			AssertEquals(message, expected.Count(), result.Count());
			AssertContainsExactElementsInAnyOrder(message, expected, result);
		}

		#endregion

		#region Billing

		protected JobHeader AddJobForBusinessObject(BusinessObject parent)
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = parent.PK;
			job.JH_ParentTableCode = parent.TablePrefix;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			return job;
		}

		protected void AddTransactionLineToBusinessObject(JobHeader jobHeader, string ledgerType, string transactionNumber, string invoiceRef = "")
		{
			var newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			newInvoice.AH_Ledger = ledgerType;
			newInvoice.AH_TransactionType = TransactionTypes.Invoice;
			newInvoice.AH_TransactionNum = transactionNumber;
			newInvoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			newInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			var newInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			// AccTransactionLines.AL_LineType cannot be empty.
			newInvoiceLine.AL_LineType = TransactionLineTypes.WIP;
			newInvoiceLine.AL_AH = newInvoice.PK;
			newInvoiceLine.AL_JH = jobHeader.PK;
			newInvoiceLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			if (!string.IsNullOrEmpty(invoiceRef))
			{
				var charge = Factory.NewWithValidTestData<JobCharge>();
				charge.JR_JH = jobHeader.PK;
				charge.JR_CostReference = invoiceRef;
			}
		}

		#region Filters

		public void TestFilterDescriptions()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (CWNextFeatureTestHelper.EnableCWNext())
			{
				var expectedFilterDescriptions = GetExpectedFilterDescriptions();
				var actualFilterDescriptions = GetNewFilterStripBusinessObject()
					.ModuleFilters
					.ToSortedArrayWithIsExclusiveLast()
					.Select(f => f.Description.ToString())
					.ToArray();

				AssertContainsExactElementsInAnyOrder(expectedFilterDescriptions, actualFilterDescriptions);
			}
		}

		protected virtual string[] GetExpectedFilterDescriptions() => new[]
		{
			"Milestone Date",
			"Milestone Completed",
			"Next Milestone",
			"Last Completed Milestone",
			"Any Open Task Assigned To",
			"Next Task Assigned To",
			"Tasks",
			"Exceptions",
			"Milestones",
			"Triggers",
			"Custom SQL Filter",
			"Creating User",
			"Created Time",
			"Last Edit User",
			"Last Edit Time",
			"Created On Web/Internal"
		};

		#endregion

		#endregion

		#region Overrides

		protected FilterStripBusinessObject FilterStripBizO
		{
			get
			{
				return fFilterStripBizO ?? (fFilterStripBizO = GetNewFilterStripBusinessObject());
			}
		}

		FilterStripBusinessObject fFilterStripBizO;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			var filterStripBO = new BaseQuotedBookingFilterStripBusinessObject();
			filterStripBO.QueryObjectType = typeof(ViewQuotedBooking);
			return filterStripBO;
		}

		#endregion

		#endregion
	}
}
