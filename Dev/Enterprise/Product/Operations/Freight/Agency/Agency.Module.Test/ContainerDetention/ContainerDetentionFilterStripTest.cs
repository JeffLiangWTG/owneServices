using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.RevenueRecognitionLookups;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(ContainerDetentionFilterStrip))]
	internal class ContainerDetentionFilterStripTest : FilterStripBusinessObjectTestCase
	{
		public void TestBaseFilter()
		{
			SetUpInvoices(2);
			GlbCompany otherCompany = Factory.New<GlbCompany>();
			otherCompany.GC_Code = "XXX";
			GlbBranch otherBranch = otherCompany.Branches.AddNew();
			otherBranch.GB_Code = "XXX";
			detention[1].NC_GC = otherCompany.PK;
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[ContainerDetentionFilterStrip.Descriptions.OwningCompany];
			filter.Property = ZGuid.Empty;
			Env.Security.AgencyContainerDetentionViewInterCompany.IsAllowed = false;
			Asserter.AssertMatches("Empty - Dissalowed", FilterStrip.Filter, detention[0]);
			Env.Security.AgencyContainerDetentionViewInterCompany.IsAllowed = true;
			Asserter.AssertMatches("Empty", FilterStrip.Filter, detention[0], detention[1]);
			filter.Property = otherCompany.PK;
			asserter.AssertMatches("Other Company", filterStrip.Filter, detention[1]);
		}

		public void TestOwningCompanySettings_Authorised()
		{
			Env.Security.AgencyContainerDetentionViewInterCompany.IsAllowed = true;
			GlbCompany otherCompany = Factory.New<GlbCompany>();
			otherCompany.GC_Code = "XXX";
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[ContainerDetentionFilterStrip.Descriptions.OwningCompany];
			AssertEquals("Default", GlbCompany.CurrentCompany.PK, filter.DefaultProperty);
			filter.Property = ZGuid.Empty;
			AssertNoNotifications(filter.PropertyInfo);
			filter.Property = GlbCompany.CurrentCompany.PK;
			AssertNoNotifications(filter.PropertyInfo);
			filter.Property = otherCompany.PK;
			AssertNoNotifications(filter.PropertyInfo);
		}

		public void TestOwningCompanySettings_NotAuthorised()
		{
			const string error = "You are not authorized to view container detention jobs from other companies.";
			Env.Security.AgencyContainerDetentionViewInterCompany.IsAllowed = false;
			GlbCompany otherCompany = Factory.New<GlbCompany>();
			otherCompany.GC_Code = "XXX";
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[ContainerDetentionFilterStrip.Descriptions.OwningCompany];
			AssertEquals("Default", GlbCompany.CurrentCompany.PK, filter.DefaultProperty);
			filter.Property = ZGuid.Empty;
			AssertHasError(filter.PropertyInfo, error);
			filter.Property = GlbCompany.CurrentCompany.PK;
			AssertNoNotifications(filter.PropertyInfo);
			filter.Property = otherCompany.PK;
			AssertHasError(filter.PropertyInfo, error);
		}

		public void TestJobNumberFilter()
		{
			SetUpInvoices(2);
			detention[0].NC_JobNumber = "DI00000101";
			detention[1].NC_JobNumber = "DI00000102";
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[ContainerDetentionFilterStrip.Descriptions.JobNumber];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, detention);
			filter.Property = "DI00000101";
			Asserter.AssertMatches("DI00000101", filter, detention[0]);
		}

		public void TestBillNumberFilter()
		{
			SetUpInvoices(2);
			shipment[0].JS_HouseBill = "Bill1";
			shipment[1].JS_HouseBill = "Bill2";
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[ContainerDetentionFilterStrip.Descriptions.BillNumber];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, detention);
			filter.Property = "Bill1";
			Asserter.AssertMatches("Bill1", filter, detention[0]);
		}

		public void TestShipmentNumberFilter()
		{
			SetUpInvoices(2);
			shipment[0].JS_UniqueConsignRef = "V1";
			shipment[1].JS_UniqueConsignRef = "V2";
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[ContainerDetentionFilterStrip.Descriptions.ShipmentNumber];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, detention);
			filter.Property = "V1";
			Asserter.AssertMatches("V1", filter, detention[0]);
		}

		public void TestContainerNumberFilter()
		{
			SetUpInvoices(2);
			stock[0].R6_ContainerNum = "Container1";
			stock[1].R6_ContainerNum = "Container2";
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[ContainerDetentionFilterStrip.Descriptions.ContainerNumber];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, detention);
			filter.Property = "Container1";
			Asserter.AssertMatches("Container1", filter, detention[0]);
		}

		public void TestVoyageVesselFilter()
		{
			SetUpInvoices(3);
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "Vessel1";
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "Vessel2Test";
			voyage[0].JV_RV_NKVessel = vessel1.RV_FK;
			voyage[0].JV_VoyageFlight = "Voyage1";
			voyage[1].JV_RV_NKVessel = vessel2.RV_FK;
			voyage[1].JV_VoyageFlight = "Voyage2";
			voyage[2].JV_RV_NKVessel = "";
			voyage[2].JV_VoyageFlight = "";
			Factory.Save();
			var filter = (VoyageVesselModuleFilter)FilterStrip[ContainerDetentionFilterStrip.Descriptions.VoyageVessel];
			filter.VoyageFlightNo = "";
			filter.Vessel = "";
			Asserter.AssertMatches("Empty Filter", filter, detention);
			filter.VoyageFlightNo = "Voyage1";
			Asserter.AssertMatches("Voyage1", filter, detention[0]);
			filter.VoyageFlightNo = "";
			filter.Vessel = "Vessel2";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			Asserter.AssertMatches("Vessel2Test", filter, detention[1]);
			filter.VoyageFlightNo = "";
			filter.Vessel = "";
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			Asserter.AssertMatches("IsBlank Correct", filter, detention[2]);
		}

		public void TestLocalClientFilter()
		{
			SetUpInvoices(2);
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[ContainerDetentionFilterStrip.Descriptions.Client];
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty Filter", filter, detention);
			filter.Property = client[0].PK;
			Asserter.AssertMatches("Client1", filter, detention[0]);
		}

		public void TestPrincipalFilter()
		{
			SetUpInvoices(2);
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[ContainerDetentionFilterStrip.Descriptions.Principal];
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty Filter", filter, detention);
			filter.Property = principal[0].PK;
			Asserter.AssertMatches("Principal1", filter, detention[0]);
		}

		public void TestLoadDischargeFilter()
		{
			SetUpInvoices(2);
			voyage[0].Origins[0].JA_RL_NKPortOfLoading = "SGSIN";
			voyage[0].Destinations[0].JB_RL_NKPortOfDischarge = "AUSYD";
			voyage[0].GenerateSailings();
			shipment[0].JS_JX = voyage[0].Sailings[0].PK;
			voyage[1].Origins[0].JA_RL_NKPortOfLoading = "NLAMS";
			voyage[1].Destinations[0].JB_RL_NKPortOfDischarge = "AUBNE";
			voyage[1].GenerateSailings();
			shipment[1].JS_JX = voyage[1].Sailings[0].PK;
			Factory.Save();
			ModuleLocationFilter filter = (ModuleLocationFilter)FilterStrip[ContainerDetentionFilterStrip.Descriptions.LoadDischarge];
			filter.Property1 = "";
			filter.Property2 = "";
			Asserter.AssertMatches("Empty Filter", filter, detention);
			filter.Property1 = "SGSIN";
			Asserter.AssertMatches("Load", filter, detention[0]);
			filter.Property1 = "";
			filter.Property2 = "AUBNE";
			Asserter.AssertMatches("Discharge", filter, detention[1]);
		}

		public void TestOriginDestinationFilter()
		{
			SetUpInvoices(2);
			shipment[0].JS_RL_NKOrigin = "SGSIN";
			shipment[0].JS_RL_NKDestination = "AUSYD";
			shipment[1].JS_RL_NKOrigin = "NLAMS";
			shipment[1].JS_RL_NKDestination = "AUBNE";
			Factory.Save();
			ModuleLocationFilter filter = (ModuleLocationFilter)FilterStrip[ContainerDetentionFilterStrip.Descriptions.OriginDestination];
			filter.Property1 = "";
			filter.Property2 = "";
			Asserter.AssertMatches("Empty Filter", filter, detention);
			filter.Property1 = "SGSIN";
			Asserter.AssertMatches("Load", filter, detention[0]);
			filter.Property1 = "";
			filter.Property2 = "AUBNE";
			Asserter.AssertMatches("Discharge", filter, detention[1]);
		}

		public void TestDetentionStatusFilter()
		{
			SetUpInvoices(7);
			// 0 - no header
			// 1 - no charges
			// 2 - charge without transaction line
			// 3 - wip charge
			// 4 - posted charge
			// 5 - mixed revinue/wip charges
			// 6 - mixed revinue/no-transactin-line charge
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "BLAT";
			header[0].Delete();
			AddCharge(header[2], null);
			AddCharge(header[3], TransactionLineTypes.Revenue);
			AddCharge(header[4], TransactionLineTypes.WIP);
			AddCharge(header[5], TransactionLineTypes.Revenue);
			AddCharge(header[5], TransactionLineTypes.WIP);
			AddCharge(header[6], TransactionLineTypes.Revenue);
			AddCharge(header[6], null);
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[ContainerDetentionFilterStrip.Descriptions.DetentionStatus];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter.Query, detention);
			filter.Property = DetentionInvoiceStatus.Codes.NotPosted;
			Asserter.AssertMatches("Not Posted", filter.Query, detention[0], detention[1], detention[2], detention[4]);
			filter.Property = DetentionInvoiceStatus.Codes.PartiallyPosted;
			Asserter.AssertMatches("Partially Posted", filter.Query, detention[5], detention[6]);
			filter.Property = DetentionInvoiceStatus.Codes.Posted;
			Asserter.AssertMatches("Posted", filter.Query, detention[3]);
			AssertEquals("Must filter job headers on parent table code", true, filter.Query.LiteralTextSqlFormatted.Contains("JH_ParentTableCode = 'NC'"));
		}

		public void TestDetentionTypeFilter()
		{
			SetUpInvoices(2);
			detention[0].NC_DetentionType = DetentionInvoiceType.Codes.Import;
			detention[1].NC_DetentionType = DetentionInvoiceType.Codes.Export;
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[ContainerDetentionFilterStrip.Descriptions.DetentionType];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter.Query, detention);
			filter.Property = DetentionInvoiceType.Codes.Import;
			Asserter.AssertMatches("Import", filter.Query, detention[0]);
			filter.Property = DetentionInvoiceType.Codes.Export;
			Asserter.AssertMatches("Export", filter.Query, detention[1]);
		}

		public void TestContainersAttachedFilter()
		{
			SetUpInvoices(2);
			Factory.Save();
			ModuleFlagsFilter filter = (ModuleFlagsFilter)FilterStrip[ContainerDetentionFilterStrip.Descriptions.ContainersAttached];
			filter["Containers Attached"] = false;
			Asserter.AssertMatches("No Containers attached", filter.Query);
			filter["Containers Attached"] = true;
			Asserter.AssertMatches("Containers attached", filter.Query, detention);
			detention[0].Movements[0].Delete();
			Factory.Save();
			filter["Containers Attached"] = false;
			Asserter.AssertMatches("No Containers (1)", filter.Query, detention[0]);
			filter["Containers Attached"] = true;
			Asserter.AssertMatches("Containers attached (1)", filter.Query, detention[1]);
			detention[1].Movements[0].Delete();
			Factory.Save();
			filter["Containers Attached"] = false;
			Asserter.AssertMatches("No Containers (2)", filter.Query, detention);
			filter["Containers Attached"] = true;
			Asserter.AssertMatches("Containers attached (2)", filter.Query);
		}

		public void TestNoChargesFilter()
		{
			SetUpInvoices(3);
			var randomDetatchedHeader = Factory.NewJobForTesting<JobHeader>();
			randomDetatchedHeader.JH_ParentID = ZGuid.Empty;
			randomDetatchedHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			randomDetatchedHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			randomDetatchedHeader.JH_JobNum = "detatched";
			header[0].Delete();
			AddCharge(randomDetatchedHeader, TransactionLineTypes.WIP, 10m);
			AddCharge(header[2], TransactionLineTypes.WIP, 20m);
			Factory.Save();
			ModuleFlagsFilter filter = (ModuleFlagsFilter)FilterStrip["Invoiced / Charges"];
			filter["No Charges"] = false;
			Asserter.AssertMatches("Empty", filter.Query, detention[0], detention[1], detention[2]);
			filter["No Charges"] = true;
			Asserter.AssertMatches("No Charges", filter.Query, detention[1]);
		}

		public void TestInvoiceStatusFilter()
		{
			SetUpInvoices(2);
			header[0].JH_Status = "WRK";
			header[1].JH_Status = "INV";
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip["Invoice Status"];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter.Query, detention);
			filter.Property = "INV";
			Asserter.AssertMatches("INV", filter.Query, detention[1]);
		}

		#region Billing Filters
		public void TestAPInvoiceNumberFilter()
		{
			SetUpInvoices(2);
			AccTransactionHeader newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			newInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			newInvoice.AH_Ledger = LedgerTypes.AccountsPayable;
			newInvoice.AH_TransactionType = TransactionTypes.Invoice;
			newInvoice.AH_TransactionNum = "00001001";
			AccTransactionLines newInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			newInvoiceLine.AL_AH = newInvoice.PK;
			newInvoiceLine.AL_JH = header[0].PK;
			newInvoiceLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			Factory.Save();
			ModuleNumberFilter filter = (ModuleNumberFilter)FilterStrip[FilterStrip.AccountingFilterStrip.APInvoiceNumber];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, detention);
			filter.Property = "00001001";
			Asserter.AssertMatches("00001001", filter, detention[0]);
			filter.Property = "00001002";
			Asserter.AssertMatches("00001002", filter);
		}

		public void TestARTransactionFilter()
		{
			SetUpInvoices(2);
			AccTransactionHeader newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			newInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			newInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			newInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			newInvoice.AH_TransactionNum = "00001001";
			AccTransactionLines newInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			newInvoiceLine.AL_AH = newInvoice.PK;
			newInvoiceLine.AL_JH = header[0].PK;
			newInvoiceLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			Factory.Save();
			ModuleNumberFilter filter = (ModuleNumberFilter)FilterStrip[FilterStrip.AccountingFilterStrip.ARTransactionNumber];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, detention);
			filter.Property = "00001001";
			Asserter.AssertMatches("00001001", filter, detention[0]);
			filter.Property = "00001002";
			Asserter.AssertMatches("00001002", filter);
		}

		public void TestProfitLossReasonFilterWithOperators()
		{
			SetUpInvoices(3);

			header[0].JH_ProfitLossReasonCode = "ND1";
			header[1].JH_ProfitLossReasonCode = "CD1";
			header[2].JH_ProfitLossReasonCode = string.Empty;

			Factory.Save();

			var profitLossReasonFilter = (ModuleTextFilter)FilterStrip["Profit/Loss Reason"];
			profitLossReasonFilter.IsActive = true;

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			profitLossReasonFilter.Property = "ND1";

			Asserter.AssertMatches("Has a Match for detention[0].", profitLossReasonFilter, detention[0]);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			profitLossReasonFilter.Property = "N";

			Asserter.AssertMatches("Has a Match for detention[0].", profitLossReasonFilter, detention[0]);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			profitLossReasonFilter.Property = "D";

			Asserter.AssertMatches("Has a Match for detention[0] and detention[1]", profitLossReasonFilter, detention[0], detention[1]);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			profitLossReasonFilter.Property = "N";

			Asserter.AssertMatches("Has a Match for detention[1] and detention[2]", profitLossReasonFilter, detention[1], detention[2]);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			profitLossReasonFilter.Property = "N";

			Asserter.AssertMatches("Has a Match for detention[1] and detention[2]", profitLossReasonFilter, detention[1], detention[2]);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			profitLossReasonFilter.Property = "ND1";

			Asserter.AssertMatches("Has a Match for detention[1] and detention[2]", profitLossReasonFilter, detention[1], detention[2]);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			profitLossReasonFilter.Property = "";

			Asserter.AssertMatches("Has a Match for detention[2]", profitLossReasonFilter, detention[2]);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			profitLossReasonFilter.Property = "ND1";

			Asserter.AssertMatches("Has a Match for detention[0] and detention[1]", profitLossReasonFilter, detention[0], detention[1]);
		}

		#endregion
		#region Implementation
		FilterStripAsserter<ContainerDetention> Asserter
		{
			get
			{
				return asserter ?? (asserter = new FilterStripAsserter<ContainerDetention>(Factory, (d) => d.NC_JobNumber));
			}
		}

		FilterStripAsserter<ContainerDetention> asserter;
		void AddCharge(JobHeader header, string lineType, decimal lineAmount = 0m)
		{
			JobCharge charge = (JobCharge)((IBusinessObjectCollection)header["Charges"]).AddNew();
			charge.JR_AC = ChargeCode.PK;
			if (!string.IsNullOrEmpty(lineType))
			{
				AccTransactionHeader invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
				invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
				invoice.AH_TransactionType = TransactionTypes.Invoice;
				AccTransactionLines line = Factory.NewWithValidTestData<AccTransactionLines>();
				line.AL_LineType = lineType;
				line.AL_AH = invoice.PK;
				line.AL_AC = ChargeCode.PK;
				line.AL_GE = header.JH_GE;
				line.AL_GB = header.JH_GB;
				line.AL_JH = header.PK;
				line.AL_RevRecognitionType = RecognitionDateOptionCodes.Immediate;
				line.AL_ExchangeRate = 1m;
				line.AL_RX_NKTransactionCurrency = "AUD";
				line.AL_LineAmount = lineAmount;
				line.AL_OSAmount = lineAmount;
				if (lineType == TransactionLineTypes.WIP || lineType == TransactionLineTypes.Revenue)
				{
					charge.JR_AL_ARLine = line.PK;
				}

				if (lineType == TransactionLineTypes.Accrual || lineType == TransactionLineTypes.Cost)
				{
					charge.JR_AL_APLine = line.PK;
				}
			}

			charge.SetAmountsFromLinkedLinesForTests();
		}

		void SetUpInvoices(int detentionCount)
		{
			client = new OrgHeader[detentionCount];
			principal = new OrgHeader[detentionCount];
			shipment = new AgencyShipment[detentionCount];
			container = new AgencyShipmentContainer[detentionCount];
			detention = new ContainerDetention[detentionCount];
			header = new JobHeader[detentionCount];
			stock = new RefContainerStock[detentionCount];
			voyage = new JobVoyage[detentionCount];
			RefContainer containerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			for (int i = 0; i < detentionCount; i++)
			{
				stock[i] = Factory.New<RefContainerStock>();
				stock[i].R6_ContainerNum = "C" + i;
				stock[i].R6_RC = containerType.PK;
				voyage[i] = Factory.New<JobVoyage>();
				voyage[i].Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyage[i].Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUMEL";
				voyage[i].GenerateSailings();
				ContainerMovement movement = stock[i].Movements.AddNew();
				movement.E9_JV = voyage[i].PK;
				client[i] = Factory.NewWithValidTestData<OrgHeader>();
				principal[i] = Factory.NewWithValidTestData<OrgHeader>();
				shipment[i] = Factory.New<AgencyShipment>();
				shipment[i].JS_JX = voyage[i].Sailings[0].PK;
				container[i] = shipment[i].RealContainers.AddNew();
				container[i].JC_ContainerNum = stock[i].R6_ContainerNum;
				detention[i] = Factory.New<ContainerDetention>();
				detention[i].NC_JobNumber = "Detention" + i;
				detention[i].NC_OH_Client = client[i].PK;
				detention[i].NC_OH_Principal = principal[i].PK;
				detention[i].Movements.Add(movement);
				header[i] = new JobHeader.Loader(detention[i]).TryLoadOrCreate();
				header[i].JH_GB = GlbBranch.CurrentBranch.PK;
				header[i].JH_GE = GlbDepartment.CurrentDepartment.PK;
				Asserter.AddToScope(detention[i]);
			}
		}

		AgencyShipment[] shipment;
		AgencyShipmentContainer[] container;
		OrgHeader[] client;
		OrgHeader[] principal;
		ContainerDetention[] detention;
		JobHeader[] header;
		RefContainerStock[] stock;
		JobVoyage[] voyage;
		AccChargeCode ChargeCode
		{
			get
			{
				return chargeCode ?? (chargeCode = Factory.NewWithValidTestData<AccChargeCode>());
			}
		}

		AccChargeCode chargeCode;
		ContainerDetentionFilterStrip FilterStrip
		{
			get
			{
				return filterStrip ?? (filterStrip = new ContainerDetentionFilterStrip());
			}
		}

		ContainerDetentionFilterStrip filterStrip;
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ContainerDetentionFilterStrip();
		}
		#endregion
	}
}
