using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgInvoiceRollupOrGroup.Loader))]
	sealed class OrgInvoiceRollupOrGroupLoaderTest : LoaderTestCase
	{
		public void TestLoaderConstruction()
		{
			OrgHeader testHeader = Factory.NewWithValidTestData<OrgHeader>();
			GlbCompany testCompany = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch testBranch = testCompany.Branches.AddNew();
			GlbDepartment testDepartment = Factory.NewWithValidTestData<GlbDepartment>();

			OrgInvoiceRollupOrGroup.Loader testLoader = new OrgInvoiceRollupOrGroup.Loader(testHeader, testBranch, testDepartment);
			AssertNotNull(testLoader);
			AssertEquals("Factory", Factory, testLoader.FactoryInternal);
			AssertEquals("Org", testHeader, testLoader.OrgInternal);
			AssertEquals("Branch", testBranch, testLoader.BranchInternal);
			AssertEquals("Department", testDepartment, testLoader.DepartmentInternal);

			AccTransactionHeader testTransaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			testTransaction.AH_OH = testHeader.PK;
			testTransaction.AH_GB = testBranch.PK;
			testTransaction.AH_GE = testDepartment.PK;

			testLoader = new OrgInvoiceRollupOrGroup.Loader(testTransaction);
			AssertNotNull(testLoader);
			AssertEquals("Factory", Factory, testLoader.FactoryInternal);
			AssertEquals("Org", testHeader, testLoader.OrgInternal);
			AssertEquals("Branch", testBranch, testLoader.BranchInternal);
			AssertEquals("Department", testDepartment, testLoader.DepartmentInternal);
		}

		public void TestGetGroupOrSubTotalWithFallbacksToRegistry()
		{
			ZString sHP = JobInvoicingConsumerTypes.Shipment.Code;
			ZString sAB = OrgInvoiceRollupOrGroupLookups.JobType_List.ShipmentAndBrokerage.Code;
			ZString aLL = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;

			AssertGroupOrSubTotal("Registry Default", OrgConstants.GroupOrSubTotalCharges.Code.RollUp);

			// ENTERPRISE LEVEL REGISTRY SETTINGS					
			SetupEnterpriseRegistry(aLL, InvoiceRollupOrGroup.Schema.GroupOrSubTotal, OrgConstants.GroupOrSubTotalCharges.Code.Alphabetical);
			AssertGroupOrSubTotal("Enterprise Registry ALL Setting", OrgConstants.GroupOrSubTotalCharges.Code.Alphabetical);

			SetupEnterpriseRegistry(sAB, InvoiceRollupOrGroup.Schema.GroupOrSubTotal, OrgConstants.GroupOrSubTotalCharges.Code.RollUp);
			AssertGroupOrSubTotal("Enterprise Registry SAB Setting", OrgConstants.GroupOrSubTotalCharges.Code.RollUp);

			SetupEnterpriseRegistry(sHP, InvoiceRollupOrGroup.Schema.GroupOrSubTotal, OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence);
			AssertGroupOrSubTotal("Enterprise Registry SHP Setting", OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence);

			// COMPANY LEVEL REGISTRY SETTINGS
			SetupCompanyRegistry(aLL, InvoiceRollupOrGroup.Schema.GroupOrSubTotal, OrgConstants.GroupOrSubTotalCharges.Code.Sequence);
			AssertGroupOrSubTotal("Company Registry ALL Setting", OrgConstants.GroupOrSubTotalCharges.Code.Sequence);

			SetupCompanyRegistry(sAB, InvoiceRollupOrGroup.Schema.GroupOrSubTotal, OrgConstants.GroupOrSubTotalCharges.Code.SubTotal);
			AssertGroupOrSubTotal("Company Registry SAB Setting", OrgConstants.GroupOrSubTotalCharges.Code.SubTotal);

			SetupCompanyRegistry(sHP, InvoiceRollupOrGroup.Schema.GroupOrSubTotal, OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence);
			AssertGroupOrSubTotal("Company Registry SHP Setting", OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence);

			// BRANCH LEVEL REGISTRY SETTINGS
			SetupBranchRegistry(aLL, InvoiceRollupOrGroup.Schema.GroupOrSubTotal, OrgConstants.GroupOrSubTotalCharges.Code.SubTotal);
			AssertGroupOrSubTotal("Company Registry ALL Setting", OrgConstants.GroupOrSubTotalCharges.Code.SubTotal);

			SetupBranchRegistry(sAB, InvoiceRollupOrGroup.Schema.GroupOrSubTotal, OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence);
			AssertGroupOrSubTotal("Branch Registry SAB Setting", OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence);

			SetupBranchRegistry(sHP, InvoiceRollupOrGroup.Schema.GroupOrSubTotal, OrgConstants.GroupOrSubTotalCharges.Code.Sequence);
			AssertGroupOrSubTotal("Branch Registry SHP Setting", OrgConstants.GroupOrSubTotalCharges.Code.Sequence);

			// DEPARTMENT LEVEL REGISTRY SETTINGS
			SetupDepartmentRegistry(aLL, InvoiceRollupOrGroup.Schema.GroupOrSubTotal, OrgConstants.GroupOrSubTotalCharges.Code.RollUp);
			AssertGroupOrSubTotal("Department Registry ALL Setting", OrgConstants.GroupOrSubTotalCharges.Code.RollUp);

			SetupDepartmentRegistry(sAB, InvoiceRollupOrGroup.Schema.GroupOrSubTotal, OrgConstants.GroupOrSubTotalCharges.Code.Sequence);
			AssertGroupOrSubTotal("Department Registry SAB Setting", OrgConstants.GroupOrSubTotalCharges.Code.Sequence);

			SetupDepartmentRegistry(sHP, InvoiceRollupOrGroup.Schema.GroupOrSubTotal, OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence);
			AssertGroupOrSubTotal("Department Registry SHP Setting", OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence);

			// ORGANISATION SETTINGS
			SetupOrganisation(aLL, OrgInvoiceRollupOrGroup.Schema.PG_GroupOrSubTotal, OrgInvoiceRollupOrGroup.GroupOrSubtotalOptionDefaultCode);
			AssertGroupOrSubTotal("Organisation ALL Default Setting", OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence);

			SetupOrganisation(aLL, OrgInvoiceRollupOrGroup.Schema.PG_GroupOrSubTotal, OrgConstants.GroupOrSubTotalCharges.Code.Alphabetical);
			AssertGroupOrSubTotal("Organisation ALL Setting", OrgConstants.GroupOrSubTotalCharges.Code.Alphabetical);

			SetupOrganisation(sAB, OrgInvoiceRollupOrGroup.Schema.PG_GroupOrSubTotal, OrgInvoiceRollupOrGroup.GroupOrSubtotalOptionDefaultCode);
			AssertGroupOrSubTotal("Organisation SAB Default Setting", OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence);

			SetupOrganisation(sAB, OrgInvoiceRollupOrGroup.Schema.PG_GroupOrSubTotal, OrgConstants.GroupOrSubTotalCharges.Code.SubTotal);
			AssertGroupOrSubTotal("Organisation SAB Setting", OrgConstants.GroupOrSubTotalCharges.Code.SubTotal);

			SetupOrganisation(sHP, OrgInvoiceRollupOrGroup.Schema.PG_GroupOrSubTotal, OrgInvoiceRollupOrGroup.GroupOrSubtotalOptionDefaultCode);
			AssertGroupOrSubTotal("Organisation SHP Default Setting", OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence);

			SetupOrganisation(sHP, OrgInvoiceRollupOrGroup.Schema.PG_GroupOrSubTotal, OrgConstants.GroupOrSubTotalCharges.Code.Sequence);
			AssertGroupOrSubTotal("Organisation SHP Setting", OrgConstants.GroupOrSubTotalCharges.Code.Sequence);
		}

		public void TestGetGroupOrSubtotalStyleWithFallbacksToRegistry()
		{
			ZString sHP = JobInvoicingConsumerTypes.Shipment.Code;
			ZString sAB = OrgInvoiceRollupOrGroupLookups.JobType_List.ShipmentAndBrokerage.Code;
			ZString aLL = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;

			AssertGetGroupOrSubtotalStyle("Registry Default", OrgConstants.InvoiceLineGroupings.Code.None);

			// ENTERPRISE LEVEL REGISTRY SETTINGS					
			SetupEnterpriseRegistry(aLL, InvoiceRollupOrGroup.Schema.GroupOrSubtotalStyle, OrgConstants.InvoiceLineGroupings.Code.AEC);
			AssertGetGroupOrSubtotalStyle("Enterprise Registry ALL Setting", OrgConstants.InvoiceLineGroupings.Code.AEC);

			SetupEnterpriseRegistry(sAB, InvoiceRollupOrGroup.Schema.GroupOrSubtotalStyle, OrgConstants.InvoiceLineGroupings.Code.CCD);
			AssertGetGroupOrSubtotalStyle("Enterprise Registry SAB Setting", OrgConstants.InvoiceLineGroupings.Code.CCD);

			SetupEnterpriseRegistry(sHP, InvoiceRollupOrGroup.Schema.GroupOrSubtotalStyle, OrgConstants.InvoiceLineGroupings.Code.OFI);
			AssertGetGroupOrSubtotalStyle("Enterprise Registry SHP Setting", OrgConstants.InvoiceLineGroupings.Code.OFI);

			// COMPANY LEVEL REGISTRY SETTINGS
			SetupCompanyRegistry(aLL, InvoiceRollupOrGroup.Schema.GroupOrSubtotalStyle, OrgConstants.InvoiceLineGroupings.Code.CLC);
			AssertGetGroupOrSubtotalStyle("Company Registry ALL Setting", OrgConstants.InvoiceLineGroupings.Code.CLC);

			SetupCompanyRegistry(sAB, InvoiceRollupOrGroup.Schema.GroupOrSubtotalStyle, OrgConstants.InvoiceLineGroupings.Code.FandD);
			AssertGetGroupOrSubtotalStyle("Company Registry SAB Setting", OrgConstants.InvoiceLineGroupings.Code.FandD);

			SetupCompanyRegistry(sHP, InvoiceRollupOrGroup.Schema.GroupOrSubtotalStyle, OrgConstants.InvoiceLineGroupings.Code.FRT);
			AssertGetGroupOrSubtotalStyle("Company Registry SHP Setting", OrgConstants.InvoiceLineGroupings.Code.FRT);

			// BRANCH LEVEL REGISTRY SETTINGS
			SetupBranchRegistry(aLL, InvoiceRollupOrGroup.Schema.GroupOrSubtotalStyle, OrgConstants.InvoiceLineGroupings.Code.FandD);
			AssertGetGroupOrSubtotalStyle("Branch Registry ALL Setting", OrgConstants.InvoiceLineGroupings.Code.FandD);

			SetupBranchRegistry(sAB, InvoiceRollupOrGroup.Schema.GroupOrSubtotalStyle, OrgConstants.InvoiceLineGroupings.Code.OFI);
			AssertGetGroupOrSubtotalStyle("Branch Registry SAB Setting", OrgConstants.InvoiceLineGroupings.Code.OFI);

			SetupBranchRegistry(sHP, InvoiceRollupOrGroup.Schema.GroupOrSubtotalStyle, OrgConstants.InvoiceLineGroupings.Code.CCD);
			AssertGetGroupOrSubtotalStyle("Branch Registry SHP Setting", OrgConstants.InvoiceLineGroupings.Code.CCD);

			// DEPARTMENT LEVEL REGISTRY SETTINGS
			SetupDepartmentRegistry(aLL, InvoiceRollupOrGroup.Schema.GroupOrSubtotalStyle, OrgConstants.InvoiceLineGroupings.Code.OandF);
			AssertGetGroupOrSubtotalStyle("Department Registry ALL Setting", OrgConstants.InvoiceLineGroupings.Code.OandF);

			SetupDepartmentRegistry(sAB, InvoiceRollupOrGroup.Schema.GroupOrSubtotalStyle, OrgConstants.InvoiceLineGroupings.Code.OFD);
			AssertGetGroupOrSubtotalStyle("Department Registry SAB Setting", OrgConstants.InvoiceLineGroupings.Code.OFD);

			SetupDepartmentRegistry(sHP, InvoiceRollupOrGroup.Schema.GroupOrSubtotalStyle, OrgConstants.InvoiceLineGroupings.Code.AEC);
			AssertGetGroupOrSubtotalStyle("Department Registry SHP Setting", OrgConstants.InvoiceLineGroupings.Code.AEC);

			// ORGANISATION SETTINGS
			SetupOrganisation(aLL, OrgInvoiceRollupOrGroup.Schema.PG_GroupOrSubtotalStyle, OrgInvoiceRollupOrGroup.GroupOrSubtotalStyleOptionDefaultCode);
			AssertGetGroupOrSubtotalStyle("Organisation ALL Default Setting", OrgConstants.InvoiceLineGroupings.Code.AEC);

			SetupOrganisation(aLL, OrgInvoiceRollupOrGroup.Schema.PG_GroupOrSubtotalStyle, OrgConstants.InvoiceLineGroupings.Code.None);
			AssertGetGroupOrSubtotalStyle("Organisation ALL Setting", OrgConstants.InvoiceLineGroupings.Code.None);

			SetupOrganisation(sAB, OrgInvoiceRollupOrGroup.Schema.PG_GroupOrSubtotalStyle, OrgInvoiceRollupOrGroup.GroupOrSubtotalStyleOptionDefaultCode);
			AssertGetGroupOrSubtotalStyle("Organisation SAB Default Setting", OrgConstants.InvoiceLineGroupings.Code.AEC);

			SetupOrganisation(sAB, OrgInvoiceRollupOrGroup.Schema.PG_GroupOrSubtotalStyle, OrgConstants.InvoiceLineGroupings.Code.OandF);
			AssertGetGroupOrSubtotalStyle("Organisation SAB Setting", OrgConstants.InvoiceLineGroupings.Code.OandF);

			SetupOrganisation(sHP, OrgInvoiceRollupOrGroup.Schema.PG_GroupOrSubtotalStyle, OrgInvoiceRollupOrGroup.GroupOrSubtotalStyleOptionDefaultCode);
			AssertGetGroupOrSubtotalStyle("Organisation SHP Default Setting", OrgConstants.InvoiceLineGroupings.Code.AEC);

			SetupOrganisation(sHP, OrgInvoiceRollupOrGroup.Schema.PG_GroupOrSubtotalStyle, OrgConstants.InvoiceLineGroupings.Code.OFD);
			AssertGetGroupOrSubtotalStyle("Organisation SHP Setting", OrgConstants.InvoiceLineGroupings.Code.OFD);
		}

		public void TestGroupOrSubtotalStyleReturnsOnlyValidValues()
		{
			ZString aLL = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;

			var typesForTest =
					from CodeDescriptionPair pair in OrgInvoiceRollupOrGroupLookups.JobTypeFullList
					where
							pair.Code != JobInvoicingConsumerTypes.Shipment.Code &&
							pair.Code != JobInvoicingConsumerTypes.Brokerage.Code &&
							pair.Code != OrgInvoiceRollupOrGroupLookups.JobType_List.ShipmentAndBrokerage.Code &&
							pair.Code != OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code &&
							pair.Code != JobInvoicingConsumerTypes.AgencyBillOfLading.Code
					select pair.Code;

			foreach (ZString typeForTest in typesForTest)
			{
				SetupEnterpriseRegistry(aLL, InvoiceRollupOrGroup.Schema.GroupOrSubtotalStyle, OrgConstants.InvoiceLineGroupings.Code.AEC);
				SetupOrganisation(aLL, OrgInvoiceRollupOrGroup.Schema.PG_GroupOrSubtotalStyle, OrgInvoiceRollupOrGroup.GroupOrSubtotalStyleOptionDefaultCode);
				AssertGetGroupOrSubtotalStyle("JobType: MSC: Style must be NOG if default value from registry is not valid.", OrgConstants.InvoiceLineGroupings.Code.None, typeForTest);

				SetupEnterpriseRegistry(aLL, InvoiceRollupOrGroup.Schema.GroupOrSubtotalStyle, OrgConstants.InvoiceLineGroupings.Code.None);
				AssertGetGroupOrSubtotalStyle("JobType: MSC: Style must be from registry when value in Organisation set as DEF.", OrgConstants.InvoiceLineGroupings.Code.None, typeForTest);

				SetupEnterpriseRegistry(aLL, InvoiceRollupOrGroup.Schema.GroupOrSubtotalStyle, OrgConstants.InvoiceLineGroupings.Code.All);
				AssertGetGroupOrSubtotalStyle("JobType: MSC: Style must be NOG if default value from registry is not valid.",
						typeForTest == OrgInvoiceRollupOrGroupLookups.JobType_List.NonJobRelated.Code ? OrgConstants.InvoiceLineGroupings.Code.None : OrgConstants.InvoiceLineGroupings.Code.All,
						typeForTest);

				SetupEnterpriseRegistry(aLL, InvoiceRollupOrGroup.Schema.GroupOrSubtotalStyle, OrgConstants.InvoiceLineGroupings.Code.CCD);
				AssertGetGroupOrSubtotalStyle("JobType: MSC: Style must be from registry when value in Organisation set as DEF.", OrgConstants.InvoiceLineGroupings.Code.CCD, typeForTest);

				SetupEnterpriseRegistry(aLL, InvoiceRollupOrGroup.Schema.GroupOrSubtotalStyle, OrgConstants.InvoiceLineGroupings.Code.CCG);
				AssertGetGroupOrSubtotalStyle("JobType: MSC: Style must be from registry when value in Organisation set as DEF.", OrgConstants.InvoiceLineGroupings.Code.CCG, typeForTest);

				SetupEnterpriseRegistry(aLL, InvoiceRollupOrGroup.Schema.GroupOrSubtotalStyle, OrgConstants.InvoiceLineGroupings.Code.FRT);
				AssertGetGroupOrSubtotalStyle("JobType: MSC: Style must be NOG if default value from registry is not valid.", OrgConstants.InvoiceLineGroupings.Code.None, typeForTest);
			}
		}

		public void TestFindRollupOrGroupTakingTransportModeIntoAccount()
		{
			OrgInvoiceRollupOrGroup invoiceRollupOrGroup = RollupOrGroupCollection.AddNew();
			invoiceRollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
			invoiceRollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Sea;
			invoiceRollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.FRT;
			invoiceRollupOrGroup.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.None;
			invoiceRollupOrGroup.PG_InvoicePostingStyle = InvoicePostingOptionsList.Codes.FinalInvoiceOnly;

			invoiceRollupOrGroup = RollupOrGroupCollection.AddNew();
			invoiceRollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
			invoiceRollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			invoiceRollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.Alphabetical;
			invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.CLC;
			invoiceRollupOrGroup.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.Freight;
			invoiceRollupOrGroup.PG_InvoicePostingStyle = InvoicePostingOptionsList.Codes.DisbursementAndFinal;
			Factory.Save();

			AssertPostingStyle("SEA Transport Mode", OrgConstants.ModesForGroupOrSubTotal.Codes.Sea, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);
			AssertLineDisplayOption("SEA Transport Mode", OrgConstants.ModesForGroupOrSubTotal.Codes.Sea, InvoiceDescriptionOptionsList.Codes.None);
			AssertGroupOrSubTotal("SEA Transport Mode", OrgConstants.GroupOrSubTotalCharges.Code.RollUp, JobInvoicingConsumerTypes.Shipment.Code, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea);
			AssertGetGroupOrSubtotalStyle("SEA Transport Mode", OrgConstants.InvoiceLineGroupings.Code.FRT, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea, JobInvoicingConsumerTypes.Shipment.Code);

			AssertPostingStyle("AIR Transport Mode", InvoicePostingOptionsList.Codes.DisbursementAndFinal);
			AssertLineDisplayOption("AIR Transport Mode", InvoiceDescriptionOptionsList.Codes.Freight);
			AssertGroupOrSubTotal("AIR Transport Mode", OrgConstants.GroupOrSubTotalCharges.Code.Alphabetical, JobInvoicingConsumerTypes.Shipment.Code);
			AssertGetGroupOrSubtotalStyle("AIR Transport Mode", OrgConstants.InvoiceLineGroupings.Code.CLC, JobInvoicingConsumerTypes.Shipment.Code);
		}

		public void TestFallbacksToRegistryTakingTransportModeIntoAccount()
		{
			InvoiceRollupOrGroupCollection registryValue = OrganisationRegistry.Instance.InvoiceRollupOrGroup.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			InvoiceRollupOrGroup invoiceOrGroupSetting = registryValue.AddNew();
			invoiceOrGroupSetting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceOrGroupSetting.ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
			invoiceOrGroupSetting.TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Sea;
			invoiceOrGroupSetting.GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceOrGroupSetting.GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.FRT;
			invoiceOrGroupSetting.InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.None;
			invoiceOrGroupSetting.InvoicePostingStyle = InvoicePostingOptionsList.Codes.FinalInvoiceOnly;

			invoiceOrGroupSetting = registryValue.AddNew();
			invoiceOrGroupSetting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceOrGroupSetting.ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
			invoiceOrGroupSetting.TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			invoiceOrGroupSetting.GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.Alphabetical;
			invoiceOrGroupSetting.GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.CLC;
			invoiceOrGroupSetting.InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.Freight;
			invoiceOrGroupSetting.InvoicePostingStyle = InvoicePostingOptionsList.Codes.DisbursementAndFinal;
			OrganisationRegistry.Instance.InvoiceRollupOrGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			AssertPostingStyle("SEA Transport Mode", OrgConstants.ModesForGroupOrSubTotal.Codes.Sea, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);
			AssertLineDisplayOption("SEA Transport Mode", OrgConstants.ModesForGroupOrSubTotal.Codes.Sea, InvoiceDescriptionOptionsList.Codes.None);
			AssertGroupOrSubTotal("SEA Transport Mode", OrgConstants.GroupOrSubTotalCharges.Code.RollUp, JobInvoicingConsumerTypes.Shipment.Code, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea);
			AssertGetGroupOrSubtotalStyle("SEA Transport Mode", OrgConstants.InvoiceLineGroupings.Code.FRT, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea, JobInvoicingConsumerTypes.Shipment.Code);

			AssertPostingStyle("AIR Transport Mode", InvoicePostingOptionsList.Codes.DisbursementAndFinal);
			AssertLineDisplayOption("AIR Transport Mode", InvoiceDescriptionOptionsList.Codes.Freight);
			AssertGroupOrSubTotal("AIR Transport Mode", OrgConstants.GroupOrSubTotalCharges.Code.Alphabetical, JobInvoicingConsumerTypes.Shipment.Code);
			AssertGetGroupOrSubtotalStyle("AIR Transport Mode", OrgConstants.InvoiceLineGroupings.Code.CLC, JobInvoicingConsumerTypes.Shipment.Code);
		}

		public void TestGetInvoiceLineDisplayOptionWithFallbacksToRegistry()
		{
			ZString sHP = JobInvoicingConsumerTypes.Shipment.Code;
			ZString sAB = OrgInvoiceRollupOrGroupLookups.JobType_List.ShipmentAndBrokerage.Code;
			ZString aLL = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;

			AssertLineDisplayOption("Registry Default", InvoiceDescriptionOptionsList.Codes.None);

			// ENTERPRISE LEVEL REGISTRY SETTINGS
			SetupEnterpriseRegistry(aLL, InvoiceRollupOrGroup.Schema.InvoiceLineDisplayOption, InvoiceDescriptionOptionsList.Codes.All);
			AssertLineDisplayOption("Enterprise Registry ALL Setting", InvoiceDescriptionOptionsList.Codes.All);

			SetupEnterpriseRegistry(sAB, InvoiceRollupOrGroup.Schema.InvoiceLineDisplayOption, InvoiceDescriptionOptionsList.Codes.AllExRate);
			AssertLineDisplayOption("Enterprise Registry SAB Setting", InvoiceDescriptionOptionsList.Codes.AllExRate);

			SetupEnterpriseRegistry(sHP, InvoiceRollupOrGroup.Schema.InvoiceLineDisplayOption, InvoiceDescriptionOptionsList.Codes.Freight);
			AssertLineDisplayOption("Enterprise Registry SHP Setting", InvoiceDescriptionOptionsList.Codes.Freight);

			// COMPANY LEVEL REGISTRY SETTINGS
			SetupCompanyRegistry(aLL, InvoiceRollupOrGroup.Schema.InvoiceLineDisplayOption, InvoiceDescriptionOptionsList.Codes.FreightExRate);
			AssertLineDisplayOption("Company Registry ALL Setting", InvoiceDescriptionOptionsList.Codes.FreightExRate);

			SetupCompanyRegistry(sAB, InvoiceRollupOrGroup.Schema.InvoiceLineDisplayOption, InvoiceDescriptionOptionsList.Codes.FreightFOB);
			AssertLineDisplayOption("Company Registry SAB Setting", InvoiceDescriptionOptionsList.Codes.FreightFOB);

			SetupCompanyRegistry(sHP, InvoiceRollupOrGroup.Schema.InvoiceLineDisplayOption, InvoiceDescriptionOptionsList.Codes.FreightFOBExRate);
			AssertLineDisplayOption("Company Registry SHP Setting", InvoiceDescriptionOptionsList.Codes.FreightFOBExRate);

			// BRANCH LEVEL REGISTRY SETTINGS
			SetupBranchRegistry(aLL, InvoiceRollupOrGroup.Schema.InvoiceLineDisplayOption, InvoiceDescriptionOptionsList.Codes.FreightFOB);
			AssertLineDisplayOption("Branch Registry ALL Setting", InvoiceDescriptionOptionsList.Codes.FreightFOB);

			SetupBranchRegistry(sAB, InvoiceRollupOrGroup.Schema.InvoiceLineDisplayOption, InvoiceDescriptionOptionsList.Codes.Freight);
			AssertLineDisplayOption("Branch Registry SAB Setting", InvoiceDescriptionOptionsList.Codes.Freight);

			SetupBranchRegistry(sHP, InvoiceRollupOrGroup.Schema.InvoiceLineDisplayOption, InvoiceDescriptionOptionsList.Codes.NoneExRate);
			AssertLineDisplayOption("Branch Registry SHP Setting", InvoiceDescriptionOptionsList.Codes.NoneExRate);

			// DEPARTMENT LEVEL REGISTRY SETTINGS
			SetupDepartmentRegistry(aLL, InvoiceRollupOrGroup.Schema.InvoiceLineDisplayOption, InvoiceDescriptionOptionsList.Codes.FreightFOBExRate);
			AssertLineDisplayOption("Department Registry ALL Setting", InvoiceDescriptionOptionsList.Codes.FreightFOBExRate);

			SetupDepartmentRegistry(sAB, InvoiceRollupOrGroup.Schema.InvoiceLineDisplayOption, InvoiceDescriptionOptionsList.Codes.FreightExRate);
			AssertLineDisplayOption("Department Registry SAB Setting", InvoiceDescriptionOptionsList.Codes.FreightExRate);

			SetupDepartmentRegistry(sHP, InvoiceRollupOrGroup.Schema.InvoiceLineDisplayOption, InvoiceDescriptionOptionsList.Codes.Freight);
			AssertLineDisplayOption("Department Registry SHP Setting", InvoiceDescriptionOptionsList.Codes.Freight);

			// ORGANISATION SETTINGS
			SetupOrganisation(aLL, OrgInvoiceRollupOrGroup.Schema.PG_InvoiceLineDisplayOption, OrgInvoiceRollupOrGroup.InvoiceLineDisplayOptionDefaultCode);
			AssertLineDisplayOption("Organisation ALL Default Setting", InvoiceDescriptionOptionsList.Codes.Freight);

			SetupOrganisation(aLL, OrgInvoiceRollupOrGroup.Schema.PG_InvoiceLineDisplayOption, InvoiceDescriptionOptionsList.Codes.None);
			AssertLineDisplayOption("Organisation ALL Setting", InvoiceDescriptionOptionsList.Codes.None);

			SetupOrganisation(sAB, OrgInvoiceRollupOrGroup.Schema.PG_InvoiceLineDisplayOption, OrgInvoiceRollupOrGroup.InvoiceLineDisplayOptionDefaultCode);
			AssertLineDisplayOption("Organisation SAB Default Setting", InvoiceDescriptionOptionsList.Codes.Freight);

			SetupOrganisation(sAB, OrgInvoiceRollupOrGroup.Schema.PG_InvoiceLineDisplayOption, InvoiceDescriptionOptionsList.Codes.NoneExRate);
			AssertLineDisplayOption("Organisation SAB Setting", InvoiceDescriptionOptionsList.Codes.NoneExRate);

			SetupOrganisation(sHP, OrgInvoiceRollupOrGroup.Schema.PG_InvoiceLineDisplayOption, OrgInvoiceRollupOrGroup.InvoiceLineDisplayOptionDefaultCode);
			AssertLineDisplayOption("Organisation SHP Default Setting", InvoiceDescriptionOptionsList.Codes.Freight);

			SetupOrganisation(sHP, OrgInvoiceRollupOrGroup.Schema.PG_InvoiceLineDisplayOption, InvoiceDescriptionOptionsList.Codes.All);
			AssertLineDisplayOption("Organisation SHP Setting", InvoiceDescriptionOptionsList.Codes.All);
		}

		public void TestGetInvoicePostingStyleWithFallbacksToRegistry()
		{
			ZString sHP = JobInvoicingConsumerTypes.Shipment.Code;
			ZString sAB = OrgInvoiceRollupOrGroupLookups.JobType_List.ShipmentAndBrokerage.Code;
			ZString aLL = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;

			AssertPostingStyle("Registry Default", InvoicePostingOptionsList.Codes.FinalInvoiceOnly);

			// ENTERPRISE LEVEL REGISTRY SETTINGS					
			SetupEnterpriseRegistry(aLL, InvoiceRollupOrGroup.Schema.InvoicePostingStyle, InvoicePostingOptionsList.Codes.DisbursementAndFinal);
			AssertPostingStyle("Enterprise Registry ALL Setting", InvoicePostingOptionsList.Codes.DisbursementAndFinal);

			SetupEnterpriseRegistry(sAB, InvoiceRollupOrGroup.Schema.InvoicePostingStyle, InvoicePostingOptionsList.Codes.DisbursementForeignOnly);
			AssertPostingStyle("Enterprise Registry SAB Setting", InvoicePostingOptionsList.Codes.DisbursementForeignOnly);

			SetupEnterpriseRegistry(sHP, InvoiceRollupOrGroup.Schema.InvoicePostingStyle, InvoicePostingOptionsList.Codes.DisbursementFreightAndFinal);
			AssertPostingStyle("Enterprise Registry SHP Setting", InvoicePostingOptionsList.Codes.DisbursementFreightAndFinal);

			// COMPANY LEVEL REGISTRY SETTINGS
			SetupCompanyRegistry(aLL, InvoiceRollupOrGroup.Schema.InvoicePostingStyle, InvoicePostingOptionsList.Codes.DisbursementFreightForeignAndFinal);
			AssertPostingStyle("Company Registry ALL Setting", InvoicePostingOptionsList.Codes.DisbursementFreightForeignAndFinal);

			SetupCompanyRegistry(sAB, InvoiceRollupOrGroup.Schema.InvoicePostingStyle, InvoicePostingOptionsList.Codes.DisbursementInvoiceForeignAndFinal);
			AssertPostingStyle("Company Registry SAB Setting", InvoicePostingOptionsList.Codes.DisbursementInvoiceForeignAndFinal);

			SetupCompanyRegistry(sHP, InvoiceRollupOrGroup.Schema.InvoicePostingStyle, InvoicePostingOptionsList.Codes.DisbursementInvoiceOnly);
			AssertPostingStyle("Company Registry SHP Setting", InvoicePostingOptionsList.Codes.DisbursementInvoiceOnly);

			// BRANCH LEVEL REGISTRY SETTINGS
			SetupBranchRegistry(aLL, InvoiceRollupOrGroup.Schema.InvoicePostingStyle, InvoicePostingOptionsList.Codes.DisbursementAndFinal);
			AssertPostingStyle("Branch Registry ALL Setting", InvoicePostingOptionsList.Codes.DisbursementAndFinal);

			SetupBranchRegistry(sAB, InvoiceRollupOrGroup.Schema.InvoicePostingStyle, InvoicePostingOptionsList.Codes.DisbursementStandardAndFinal);
			AssertPostingStyle("Branch Registry SAB Setting", InvoicePostingOptionsList.Codes.DisbursementStandardAndFinal);

			SetupBranchRegistry(sHP, InvoiceRollupOrGroup.Schema.InvoicePostingStyle, InvoicePostingOptionsList.Codes.FreightAndFinal);
			AssertPostingStyle("Branch Registry SHP Setting", InvoicePostingOptionsList.Codes.FreightAndFinal);

			// DEPARTMENT LEVEL REGISTRY SETTINGS
			SetupDepartmentRegistry(aLL, InvoiceRollupOrGroup.Schema.InvoicePostingStyle, InvoicePostingOptionsList.Codes.InvoicePerTaxCode);
			AssertPostingStyle("Department Registry ALL Setting", InvoicePostingOptionsList.Codes.InvoicePerTaxCode);

			SetupDepartmentRegistry(sAB, InvoiceRollupOrGroup.Schema.InvoicePostingStyle, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);
			AssertPostingStyle("Department Registry SAB Setting", InvoicePostingOptionsList.Codes.FinalInvoiceOnly);

			SetupDepartmentRegistry(sHP, InvoiceRollupOrGroup.Schema.InvoicePostingStyle, InvoicePostingOptionsList.Codes.DisbursementFreightForeignAndFinal);
			AssertPostingStyle("Department Registry SHP Setting", InvoicePostingOptionsList.Codes.DisbursementFreightForeignAndFinal);

			// ORGANISATION SETTINGS
			SetupOrganisation(aLL, OrgInvoiceRollupOrGroup.Schema.PG_InvoicePostingStyle, OrgInvoiceRollupOrGroup.InvoicePostingOptionDefaultCode);
			AssertPostingStyle("Organisation ALL Default Setting", InvoicePostingOptionsList.Codes.DisbursementFreightForeignAndFinal);

			SetupOrganisation(aLL, OrgInvoiceRollupOrGroup.Schema.PG_InvoicePostingStyle, InvoicePostingOptionsList.Codes.DisbursementStandardAndFinal);
			AssertPostingStyle("Organisation ALL Setting", InvoicePostingOptionsList.Codes.DisbursementStandardAndFinal);

			SetupOrganisation(sAB, OrgInvoiceRollupOrGroup.Schema.PG_InvoicePostingStyle, OrgInvoiceRollupOrGroup.InvoicePostingOptionDefaultCode);
			AssertPostingStyle("Organisation SAB Default Setting", InvoicePostingOptionsList.Codes.DisbursementFreightForeignAndFinal);

			SetupOrganisation(sAB, OrgInvoiceRollupOrGroup.Schema.PG_InvoicePostingStyle, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);
			AssertPostingStyle("Organisation SAB Setting", InvoicePostingOptionsList.Codes.FinalInvoiceOnly);

			SetupOrganisation(sHP, OrgInvoiceRollupOrGroup.Schema.PG_InvoicePostingStyle, OrgInvoiceRollupOrGroup.InvoicePostingOptionDefaultCode);
			AssertPostingStyle("Organisation SHP Default Setting", InvoicePostingOptionsList.Codes.DisbursementFreightForeignAndFinal);

			SetupOrganisation(sHP, OrgInvoiceRollupOrGroup.Schema.PG_InvoicePostingStyle, InvoicePostingOptionsList.Codes.ForeignCurrencyInvoiceAndFinal);
			AssertPostingStyle("Organisation SHP Setting", InvoicePostingOptionsList.Codes.ForeignCurrencyInvoiceAndFinal);
		}

		public void TestGetInvoicePostingCurrencyWithFallbacksToRegistry()
		{
			ZString sHP = JobInvoicingConsumerTypes.Shipment.Code;
			ZString sAB = OrgInvoiceRollupOrGroupLookups.JobType_List.ShipmentAndBrokerage.Code;
			ZString aLL = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;

			AssertPostingStyle("Registry Default", InvoicePostingOptionsList.Codes.FinalInvoiceOnly);

			// ENTERPRISE LEVEL REGISTRY SETTINGS					
			SetupEnterpriseRegistry(aLL, InvoiceRollupOrGroup.Schema.InvoicePostingCurrency, "USD");
			AssertInvoicePostingCurrency("Enterprise Registry ALL Setting", "USD");

			SetupEnterpriseRegistry(sAB, InvoiceRollupOrGroup.Schema.InvoicePostingCurrency, "EUR");
			AssertInvoicePostingCurrency("Enterprise Registry SAB Setting", "EUR");

			SetupEnterpriseRegistry(sHP, InvoiceRollupOrGroup.Schema.InvoicePostingCurrency, "GBP");
			AssertInvoicePostingCurrency("Enterprise Registry SHP Setting", "GBP");

			// COMPANY LEVEL REGISTRY SETTINGS
			SetupCompanyRegistry(aLL, InvoiceRollupOrGroup.Schema.InvoicePostingCurrency, "NZD");
			AssertInvoicePostingCurrency("Company Registry ALL Setting", "NZD");

			SetupCompanyRegistry(sAB, InvoiceRollupOrGroup.Schema.InvoicePostingCurrency, "SGD");
			AssertInvoicePostingCurrency("Company Registry SAB Setting", "SGD");

			SetupCompanyRegistry(sHP, InvoiceRollupOrGroup.Schema.InvoicePostingCurrency, "JPY");
			AssertInvoicePostingCurrency("Company Registry SHP Setting", "JPY");

			// BRANCH LEVEL REGISTRY SETTINGS
			SetupBranchRegistry(aLL, InvoiceRollupOrGroup.Schema.InvoicePostingCurrency, "KRW");
			AssertInvoicePostingCurrency("Branch Registry ALL Setting", "KRW");

			SetupBranchRegistry(sAB, InvoiceRollupOrGroup.Schema.InvoicePostingCurrency, "IDR");
			AssertInvoicePostingCurrency("Branch Registry SAB Setting", "IDR");

			SetupBranchRegistry(sHP, InvoiceRollupOrGroup.Schema.InvoicePostingCurrency, "RUB");
			AssertInvoicePostingCurrency("Branch Registry SHP Setting", "RUB");

			// DEPARTMENT LEVEL REGISTRY SETTINGS
			SetupDepartmentRegistry(aLL, InvoiceRollupOrGroup.Schema.InvoicePostingCurrency, "BBD");
			AssertInvoicePostingCurrency("Department Registry ALL Setting", "BBD");

			SetupDepartmentRegistry(sAB, InvoiceRollupOrGroup.Schema.InvoicePostingCurrency, "CHF");
			AssertInvoicePostingCurrency("Department Registry SAB Setting", "CHF");

			SetupDepartmentRegistry(sHP, InvoiceRollupOrGroup.Schema.InvoicePostingCurrency, "BSD");
			AssertInvoicePostingCurrency("Department Registry SAB Setting", "BSD");

			// ORGANISATION SETTINGS
			var emptyRegistryValue = new InvoiceRollupOrGroupCollection();
			OrganisationRegistry.Instance.InvoiceRollupOrGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, emptyRegistryValue);
			OrganisationRegistry.Instance.InvoiceRollupOrGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, emptyRegistryValue);
			OrganisationRegistry.Instance.InvoiceRollupOrGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), emptyRegistryValue);

			SetupDepartmentRegistry(aLL, InvoiceRollupOrGroup.Schema.InvoicePostingCurrency, "BBD");
			SetupOrganisation(aLL, OrgInvoiceRollupOrGroup.Schema.PG_RX_NKInvoicePostingCurrency, "");
			AssertInvoicePostingCurrency("Fall back to Registry ALL Setting", "BBD");

			SetupOrganisation(aLL, OrgInvoiceRollupOrGroup.Schema.PG_RX_NKInvoicePostingCurrency, "BYR");
			AssertInvoicePostingCurrency("Organisation ALL Setting", "BYR");

			SetupOrganisation(aLL, OrgInvoiceRollupOrGroup.Schema.PG_RX_NKInvoicePostingCurrency, "CDF");
			AssertInvoicePostingCurrency("Organisation ALL Setting", "CDF");

			SetupDepartmentRegistry(sAB, InvoiceRollupOrGroup.Schema.InvoicePostingCurrency, "CHF");
			SetupOrganisation(sAB, OrgInvoiceRollupOrGroup.Schema.PG_RX_NKInvoicePostingCurrency, "");
			AssertInvoicePostingCurrency("Fall back to Registry SAB Setting", "CHF");

			SetupOrganisation(sAB, OrgInvoiceRollupOrGroup.Schema.PG_RX_NKInvoicePostingCurrency, "CNY");
			AssertInvoicePostingCurrency("Organisation ALL Setting", "CNY");

			SetupOrganisation(sAB, OrgInvoiceRollupOrGroup.Schema.PG_RX_NKInvoicePostingCurrency, "AWG");
			AssertInvoicePostingCurrency("Organisation SAB Setting", "AWG");

			SetupDepartmentRegistry(sHP, InvoiceRollupOrGroup.Schema.InvoicePostingCurrency, "BSD");
			SetupOrganisation(sHP, OrgInvoiceRollupOrGroup.Schema.PG_RX_NKInvoicePostingCurrency, "");
			AssertInvoicePostingCurrency("Fall back to Registry SAB Setting", "BSD");

			SetupOrganisation(sHP, OrgInvoiceRollupOrGroup.Schema.PG_RX_NKInvoicePostingCurrency, "BZD");
			AssertInvoicePostingCurrency("Organisation SAB Setting", "BZD");

			SetupOrganisation(sHP, OrgInvoiceRollupOrGroup.Schema.PG_RX_NKInvoicePostingCurrency, "COP");
			AssertInvoicePostingCurrency("Organisation SAB Setting", "COP");
		}
		public void TestFindRollupOrGroup()
		{
			OrgInvoiceRollupOrGroup invoiceRollupOrGroup = RollupOrGroupCollection.AddNew();
			invoiceRollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Export;
			invoiceRollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			invoiceRollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.OFD;

			invoiceRollupOrGroup = RollupOrGroupCollection.AddNew();
			invoiceRollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
			invoiceRollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Sea;
			invoiceRollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.All;

			invoiceRollupOrGroup = RollupOrGroupCollection.AddNew();
			invoiceRollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			invoiceRollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			invoiceRollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.SubTotal;
			invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.AEC;
			Factory.Save();

			OrgInvoiceRollupOrGroup foundRollOrGroup = new OrgInvoiceRollupOrGroup.Loader(RollupOrGroupCollection.Master.Header, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment).BaseLoadForTestOnly(OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, JobInvoicingConsumerTypes.Shipment.Code);
			AssertNotNull("Object was not found, should have found the OrgInvoiceRollupOrGroup that is SHP, EXP, AIR", foundRollOrGroup);
			AssertEquals("Should have an OFD GroupOrSubTotalStyle", OrgConstants.InvoiceLineGroupings.Code.OFD, foundRollOrGroup.PG_GroupOrSubtotalStyle);
			AssertEquals("Should be Rolled up", OrgConstants.GroupOrSubTotalCharges.Code.RollUp, foundRollOrGroup.PG_GroupOrSubTotal);

			foundRollOrGroup = new OrgInvoiceRollupOrGroup.Loader(RollupOrGroupCollection.Master.Header, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment).BaseLoadForTestOnly(OrgConstants.ServiceDirection.Code.Domestic, OrgConstants.ModesForGroupOrSubTotal.Codes.Rail, OrgConstants.ModesForGroupOrSubTotal.Codes.Rail, JobInvoicingConsumerTypes.Shipment.Code);
			AssertNotNull("Object was not found, should find closest match to SHP with all directions and modes");
			AssertEquals("Should have an AEC GroupOrSubTotalStyle", OrgConstants.InvoiceLineGroupings.Code.AEC, foundRollOrGroup.PG_GroupOrSubtotalStyle);
			AssertEquals("Should be Sub Total", OrgConstants.GroupOrSubTotalCharges.Code.SubTotal, foundRollOrGroup.PG_GroupOrSubTotal);

			foundRollOrGroup = new OrgInvoiceRollupOrGroup.Loader(RollupOrGroupCollection.Master.Header, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment).BaseLoadForTestOnly(OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea, OrgConstants.ModesForGroupOrSubTotal.Codes.FCL, JobInvoicingConsumerTypes.Brokerage.Code);
			AssertNull("No RollupOrGroup should be found, JobType does not match", foundRollOrGroup);

			invoiceRollupOrGroup = RollupOrGroupCollection.AddNew();
			invoiceRollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.CTOCusMAWB.Code;
			invoiceRollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.All;
			Factory.Save();

			foundRollOrGroup = new OrgInvoiceRollupOrGroup.Loader(RollupOrGroupCollection.Master.Header, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment).LoadForTestOnly(JobInvoicingConsumerTypes.CTOCusMAWB.Code);
			AssertNotNull("Object was not found, should match on the JobType", foundRollOrGroup);
			AssertEquals("Should have an ALL GroupOrSubTotalStyle", OrgConstants.InvoiceLineGroupings.Code.All, foundRollOrGroup.PG_GroupOrSubtotalStyle);
			AssertEquals("Should be RollUP", OrgConstants.GroupOrSubTotalCharges.Code.RollUp, foundRollOrGroup.PG_GroupOrSubTotal);
		}

		public void TestExactMatchOnJobType()
		{
			OrgInvoiceRollupOrGroup invoiceRollupOrGroup1 = RollupOrGroupCollection.AddNew();
			invoiceRollupOrGroup1.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceRollupOrGroup1.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Export;
			invoiceRollupOrGroup1.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			invoiceRollupOrGroup1.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceRollupOrGroup1.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.OFD;

			OrgInvoiceRollupOrGroup invoiceRollupOrGroup2 = RollupOrGroupCollection.AddNew();
			invoiceRollupOrGroup2.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.ShipmentAndBrokerage.Code;
			invoiceRollupOrGroup2.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Export;
			invoiceRollupOrGroup2.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			invoiceRollupOrGroup2.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceRollupOrGroup2.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.All;

			OrgInvoiceRollupOrGroup invoiceRollupOrGroup3 = RollupOrGroupCollection.AddNew();
			invoiceRollupOrGroup3.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			invoiceRollupOrGroup3.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			invoiceRollupOrGroup3.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			invoiceRollupOrGroup3.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.SubTotal;
			invoiceRollupOrGroup3.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.AEC;

			Factory.Save();

			AssertEquals(invoiceRollupOrGroup1.PK + " should be returned", invoiceRollupOrGroup1.PK, new OrgInvoiceRollupOrGroup.Loader(RollupOrGroupCollection.Master.Header, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment).BaseLoadForTestOnly(OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, JobInvoicingConsumerTypes.Shipment.Code).PK);
		}

		public void TestShipmentAndBrokerageJobTypeIsSelected()
		{
			OrgInvoiceRollupOrGroup invoiceRollupOrGroup1 = RollupOrGroupCollection.AddNew();
			invoiceRollupOrGroup1.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.ShipmentAndBrokerage.Code;
			invoiceRollupOrGroup1.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Export;
			invoiceRollupOrGroup1.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			invoiceRollupOrGroup1.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceRollupOrGroup1.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.All;

			OrgInvoiceRollupOrGroup invoiceRollupOrGroup2 = RollupOrGroupCollection.AddNew();
			invoiceRollupOrGroup2.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			invoiceRollupOrGroup2.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			invoiceRollupOrGroup2.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			invoiceRollupOrGroup2.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.SubTotal;
			invoiceRollupOrGroup2.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.AEC;
			Factory.Save();

			AssertEquals(invoiceRollupOrGroup1.PK + " should be returned", invoiceRollupOrGroup1.PK, new OrgInvoiceRollupOrGroup.Loader(RollupOrGroupCollection.Master.Header, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment).BaseLoadForTestOnly(OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, JobInvoicingConsumerTypes.Shipment.Code).PK);
			AssertEquals("Brokerage job should also match", invoiceRollupOrGroup1.PK, new OrgInvoiceRollupOrGroup.Loader(RollupOrGroupCollection.Master.Header, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment).BaseLoadForTestOnly(OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, JobInvoicingConsumerTypes.Brokerage.Code).PK);
		}

		public void TestAnyJobTypeIsSelected()
		{
			OrgInvoiceRollupOrGroup invoiceRollupOrGroup = RollupOrGroupCollection.AddNew();
			invoiceRollupOrGroup.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			invoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			invoiceRollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			invoiceRollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.SubTotal;
			invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.AEC;
			Factory.Save();

			AssertEquals(invoiceRollupOrGroup.PK + " should be returned", invoiceRollupOrGroup.PK, new OrgInvoiceRollupOrGroup.Loader(RollupOrGroupCollection.Master.Header, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment).BaseLoadForTestOnly(OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, JobInvoicingConsumerTypes.Shipment.Code).PK);
			AssertEquals("Brokerage job should also match", invoiceRollupOrGroup.PK, new OrgInvoiceRollupOrGroup.Loader(RollupOrGroupCollection.Master.Header, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment).BaseLoadForTestOnly(OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, JobInvoicingConsumerTypes.Brokerage.Code).PK);
			AssertEquals("AnyJob job should match", invoiceRollupOrGroup.PK, new OrgInvoiceRollupOrGroup.Loader(RollupOrGroupCollection.Master.Header, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment).BaseLoadForTestOnly(OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, JobInvoicingConsumerTypes.Brokerage.Code).PK);
		}

		public void TestExactMatchOnLCLOrFCLMode()
		{
			OrgInvoiceRollupOrGroup invoiceRollupOrGroup1 = RollupOrGroupCollection.AddNew();
			invoiceRollupOrGroup1.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceRollupOrGroup1.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Export;
			invoiceRollupOrGroup1.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.LCL;
			invoiceRollupOrGroup1.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceRollupOrGroup1.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.OFD;

			OrgInvoiceRollupOrGroup invoiceRollupOrGroup2 = RollupOrGroupCollection.AddNew();
			invoiceRollupOrGroup2.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceRollupOrGroup2.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Export;
			invoiceRollupOrGroup2.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Sea;
			invoiceRollupOrGroup2.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceRollupOrGroup2.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.OFD;

			OrgInvoiceRollupOrGroup invoiceRollupOrGroup3 = RollupOrGroupCollection.AddNew();
			invoiceRollupOrGroup3.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceRollupOrGroup3.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Export;
			invoiceRollupOrGroup3.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			invoiceRollupOrGroup3.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceRollupOrGroup3.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.OFD;
			Factory.Save();
			AssertEquals(invoiceRollupOrGroup1.PK + " should be returned", invoiceRollupOrGroup1.PK, new OrgInvoiceRollupOrGroup.Loader(RollupOrGroupCollection.Master.Header, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment).BaseLoadForTestOnly(OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea, OrgConstants.ModesForGroupOrSubTotal.Codes.LCL, JobInvoicingConsumerTypes.Shipment.Code).PK);

			invoiceRollupOrGroup1.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.FCL;
			Factory.Save();
			AssertEquals(invoiceRollupOrGroup1.PK + " should be returned", invoiceRollupOrGroup1.PK, new OrgInvoiceRollupOrGroup.Loader(RollupOrGroupCollection.Master.Header, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment).BaseLoadForTestOnly(OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea, OrgConstants.ModesForGroupOrSubTotal.Codes.FCL, JobInvoicingConsumerTypes.Shipment.Code).PK);
		}

		public void TestMatchOnSeaModeWhenFCLOrLCL()
		{
			OrgInvoiceRollupOrGroup invoiceRollupOrGroup1 = RollupOrGroupCollection.AddNew();
			invoiceRollupOrGroup1.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceRollupOrGroup1.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Export;
			invoiceRollupOrGroup1.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Sea;
			invoiceRollupOrGroup1.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceRollupOrGroup1.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.OFD;

			OrgInvoiceRollupOrGroup invoiceRollupOrGroup2 = RollupOrGroupCollection.AddNew();
			invoiceRollupOrGroup2.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceRollupOrGroup2.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Export;
			invoiceRollupOrGroup2.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			invoiceRollupOrGroup2.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceRollupOrGroup2.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.OFD;
			Factory.Save();
			AssertEquals(invoiceRollupOrGroup1.PK + " should be returned", invoiceRollupOrGroup1.PK, new OrgInvoiceRollupOrGroup.Loader(RollupOrGroupCollection.Master.Header, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment).BaseLoadForTestOnly(OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea, OrgConstants.ModesForGroupOrSubTotal.Codes.LCL, JobInvoicingConsumerTypes.Shipment.Code).PK);

			invoiceRollupOrGroup1.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.FCL;
			Factory.Save();
			AssertEquals(invoiceRollupOrGroup1.PK + " should be returned", invoiceRollupOrGroup1.PK, new OrgInvoiceRollupOrGroup.Loader(RollupOrGroupCollection.Master.Header, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment).BaseLoadForTestOnly(OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea, OrgConstants.ModesForGroupOrSubTotal.Codes.FCL, JobInvoicingConsumerTypes.Shipment.Code).PK);
		}

		public void TestMatchOnAllModeWhenFCLOrLCL()
		{
			OrgInvoiceRollupOrGroup invoiceRollupOrGroup = RollupOrGroupCollection.AddNew();
			invoiceRollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Export;
			invoiceRollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			invoiceRollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.OFD;
			Factory.Save();
			AssertEquals(invoiceRollupOrGroup.PK + " should be returned", invoiceRollupOrGroup.PK, new OrgInvoiceRollupOrGroup.Loader(RollupOrGroupCollection.Master.Header, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment).BaseLoadForTestOnly(OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea, OrgConstants.ModesForGroupOrSubTotal.Codes.LCL, JobInvoicingConsumerTypes.Shipment.Code).PK);

			invoiceRollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.FCL;
			Factory.Save();
			AssertEquals(invoiceRollupOrGroup.PK + " should be returned", invoiceRollupOrGroup.PK, new OrgInvoiceRollupOrGroup.Loader(RollupOrGroupCollection.Master.Header, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment).BaseLoadForTestOnly(OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea, OrgConstants.ModesForGroupOrSubTotal.Codes.FCL, JobInvoicingConsumerTypes.Shipment.Code).PK);
		}

		#region Implementation

		void SetupEnterpriseRegistry(string jobType, string propertyToSet, string value)
		{
			SetupRegistry(Guid.Empty, Guid.Empty, Guid.Empty, jobType, propertyToSet, value);
		}

		void SetupCompanyRegistry(string jobType, string propertyToSet, string value)
		{
			SetupRegistry(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, jobType, propertyToSet, value);
		}

		void SetupBranchRegistry(string jobType, string propertyToSet, string value)
		{
			SetupRegistry(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, jobType, propertyToSet, value);
		}

		void SetupDepartmentRegistry(string jobType, string propertyToSet, string value)
		{
			SetupRegistry(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), jobType, propertyToSet, value);
		}

		void SetupRegistry(Guid companyPK, Guid branchPK, Guid departmentPK, string jobType, string propertyToSet, string value)
		{
			InvoiceRollupOrGroupCollection registryValue = OrganisationRegistry.Instance.InvoiceRollupOrGroup.GetValueWithoutFallback(companyPK, branchPK, departmentPK);
			InvoiceRollupOrGroup invoiceOrGroupSetting = null;

			foreach (InvoiceRollupOrGroup registryRow in registryValue)
			{
				if (registryRow.JobType == jobType)
				{
					invoiceOrGroupSetting = registryRow;
					break;
				}
			}

			if (invoiceOrGroupSetting == null)
			{
				invoiceOrGroupSetting = registryValue.AddNew();
				invoiceOrGroupSetting.JobType = jobType;
				invoiceOrGroupSetting.ServiceDirection = OrgConstants.ServiceDirection.Code.All;
				invoiceOrGroupSetting.TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
				invoiceOrGroupSetting.GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
				invoiceOrGroupSetting.GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.None;
				invoiceOrGroupSetting.InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.None;
				invoiceOrGroupSetting.InvoicePostingStyle = InvoicePostingOptionsList.Codes.FinalInvoiceOnly;
				invoiceOrGroupSetting.InvoicePostingCurrency = ZString.Empty;   // Default value for the InvoicePostingCurrency
			}

			invoiceOrGroupSetting[propertyToSet] = value;

			OrganisationRegistry.Instance.InvoiceRollupOrGroup.SetValue(companyPK, branchPK, departmentPK, registryValue);
		}

		void SetupOrganisation(string jobType, string propertyToSet, string value)
		{
			OrgInvoiceRollupOrGroup invoiceOrGroupSetting = null;

			foreach (OrgInvoiceRollupOrGroup orgSetting in RollupOrGroupCollection)
			{
				if (orgSetting.PG_JobType == jobType)
				{
					invoiceOrGroupSetting = orgSetting;
					break;
				}
			}

			if (invoiceOrGroupSetting == null)
			{
				invoiceOrGroupSetting = RollupOrGroupCollection.AddNew();
			}

			invoiceOrGroupSetting.PG_JobType = jobType;
			invoiceOrGroupSetting.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			invoiceOrGroupSetting.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			invoiceOrGroupSetting.PG_GroupOrSubTotal = OrgInvoiceRollupOrGroup.GroupOrSubtotalOptionDefaultCode;
			invoiceOrGroupSetting.PG_GroupOrSubtotalStyle = OrgInvoiceRollupOrGroup.GroupOrSubtotalStyleOptionDefaultCode;
			invoiceOrGroupSetting.PG_InvoicePostingStyle = OrgInvoiceRollupOrGroup.InvoicePostingOptionDefaultCode;
			invoiceOrGroupSetting.PG_InvoiceLineDisplayOption = OrgInvoiceRollupOrGroup.InvoiceLineDisplayOptionDefaultCode;
			invoiceOrGroupSetting.PG_RX_NKInvoicePostingCurrency = ZString.Empty; // Default value for the InvoicePostingCurrency

			invoiceOrGroupSetting[propertyToSet] = value;
			invoiceOrGroupSetting.Factory.Save();
		}

		void AssertGroupOrSubTotal(ZString message, ZString expected)
		{
			AssertGroupOrSubTotal(message, expected, JobInvoicingConsumerTypes.Shipment.Code);
		}

		void AssertGroupOrSubTotal(ZString message, ZString expected, ZString jobType)
		{
			ZString actual = new OrgInvoiceRollupOrGroup.Loader(RollupOrGroupCollection.Master.Header, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment).GetGroupOrSubTotal(OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, jobType);
			AssertEquals("Group or Sub Total - " + message, expected, actual);
		}

		void AssertGroupOrSubTotal(ZString message, ZString expected, ZString jobType, ZString transportMode)
		{
			ZString actual = ZString.Empty;
			foreach (CodeDescriptionPair containerMode in ObjectFactory.Get<IFreightCodePairListProvider>().GetContainerModeList(transportMode))
			{
				actual = new OrgInvoiceRollupOrGroup.Loader(RollupOrGroupCollection.Master.Header, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment).GetGroupOrSubTotal(OrgConstants.ServiceDirection.Code.Import, transportMode, containerMode.Code, jobType);
				AssertEquals("Group or Sub Total - " + message, expected, actual);
			}
		}

		void AssertGetGroupOrSubtotalStyle(ZString message, ZString expected)
		{
			AssertGetGroupOrSubtotalStyle(message, expected, JobInvoicingConsumerTypes.Shipment.Code);
		}

		void AssertGetGroupOrSubtotalStyle(ZString message, ZString expected, ZString jobType)
		{
			ZString actual = new OrgInvoiceRollupOrGroup.Loader(RollupOrGroupCollection.Master.Header, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment).GetGroupOrSubtotalStyle(OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, jobType);
			AssertEquals("Group or Subtotal Style - " + message, expected, actual);
		}

		void AssertGetGroupOrSubtotalStyle(ZString message, ZString expected, ZString transportMode, ZString jobType)
		{
			ZString actual = ZString.Empty;
			foreach (CodeDescriptionPair containerMode in ObjectFactory.Get<IFreightCodePairListProvider>().GetContainerModeList(transportMode))
			{
				actual = new OrgInvoiceRollupOrGroup.Loader(RollupOrGroupCollection.Master.Header, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment).GetGroupOrSubtotalStyle(OrgConstants.ServiceDirection.Code.Import, transportMode, containerMode.Code, jobType);
				AssertEquals("Group or Subtotal Style - " + message, expected, actual);
			}
		}

		void AssertLineDisplayOption(ZString message, ZString expected)
		{
			ZString actual = new OrgInvoiceRollupOrGroup.Loader(RollupOrGroupCollection.Master.Header, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment).GetInvoiceLineDisplayOption(OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, JobInvoicingConsumerTypes.Shipment.Code);
			AssertEquals("Line Display Option - " + message, expected, actual);
		}

		void AssertLineDisplayOption(ZString message, ZString transportMode, ZString expected)
		{
			ZString actual = ZString.Empty;
			foreach (CodeDescriptionPair containerMode in ObjectFactory.Get<IFreightCodePairListProvider>().GetContainerModeList(transportMode))
			{
				actual = new OrgInvoiceRollupOrGroup.Loader(RollupOrGroupCollection.Master.Header, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment).GetInvoiceLineDisplayOption(OrgConstants.ServiceDirection.Code.Import, transportMode, containerMode.Code, JobInvoicingConsumerTypes.Shipment.Code);
				AssertEquals("Line Display Option - " + message, expected, actual);
			}
		}

		void AssertPostingStyle(ZString message, ZString expected)
		{
			ZString actual = new OrgInvoiceRollupOrGroup.Loader(RollupOrGroupCollection.Master.Header, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment).GetInvoicePostingStyle(OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, JobInvoicingConsumerTypes.Shipment.Code);
			AssertEquals("Invoice Posting Style - " + message, expected, actual);
		}

		void AssertPostingStyle(ZString message, ZString transportMode, ZString expected)
		{
			ZString actual = ZString.Empty;

			foreach (CodeDescriptionPair containerMode in ObjectFactory.Get<IFreightCodePairListProvider>().GetContainerModeList(transportMode))
			{
				actual = new OrgInvoiceRollupOrGroup.Loader(RollupOrGroupCollection.Master.Header, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment).GetInvoicePostingStyle(OrgConstants.ServiceDirection.Code.Import, transportMode, containerMode.Code, JobInvoicingConsumerTypes.Shipment.Code);
				AssertEquals("Invoice Posting Style - " + message, expected, actual);
			}
		}

		void AssertInvoicePostingCurrency(ZString message, ZString expected)
		{
			ZString actual = new OrgInvoiceRollupOrGroup.Loader(RollupOrGroupCollection.Master.Header, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment).GetInvoicePostingCurrency(OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, JobInvoicingConsumerTypes.Shipment.Code);
			AssertEquals("Invoice Posting Currency - " + message, expected, actual);
		}

		OrgInvoiceRollupOrGroupCollection RollupOrGroupCollection;

		protected override void SetUp()
		{
			base.SetUp();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			RollupOrGroupCollection = org.CompanyData.InvoiceRollupOrGroups;
			RollupOrGroupCollection.RemoveAndDeleteAll();
			Factory.Save();
			RollupOrGroupCollection.RemoveAndDeleteAll();
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			return new OrgInvoiceRollupOrGroup.Loader(org, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment);
		}

		#endregion
	}
}
