using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ReportTesting;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.Report.Testing
{
	sealed class ZACustomsEntryLinesReportTests : ReportDataChecker
	{
		public ZACustomsEntryLinesReportTests()
		{
			reportDataObjectDetails = new ZACustomsEntryLinesReport_DataObjectDetails();
			var parameterHelper = new ReportParameterHelper(reportDataObjectDetails);
			parameterHelper.SetParameterValue("@CurrentCompany", GlbCompany.CurrentCompany.PK.ToString());
			parameterHelper.SetParameterValue("@ShipmentType", "IMP");
			scenario = new ReportTestScenario();
			scenario.ParametersValuesList = parameterHelper.GetParametersValuesList();
		}

		public void TestZACustomsEntryLinesReport()
		{
			var declarationList = PrepareDeclarations();
			Factory.Save();
			var scenarioList = new List<ReportTestScenario>()
			{ CreateTestScenario_FilterOn_CurrentCompany_and_ShipmentType(), CreateTestScenario_FilterOn_TransportMode(), CreateTestScenario_FilterOn_CustomsOffice(), CreateTestScenario_FilterOn_JobRegistrationDate(), CreateTestScenario_FilterOn_EntrySubmissionDate(), CreateTestScenario_FilterOn_AssessmentDate(), CreateTestScenario_FilterOn_CustomsProcedureCode(), CreateTestScenario_FilterOn_EntryStatus(), CreateTestScenario_FilterOn_MessageStatus(), CreateTestScenario_FilterOn_EntryReleaseDate(), CreateTestScenario_FilterOn_ImporterAndSupplier(), CreateTestScenario_FilterOn_AllFilterFields() };
			var allEntryLines = CollectAllEntryLines(declarationList);
			foreach (var scenario in scenarioList)
			{
				var expectedOutput = DetermineExpectedOutput(scenario, allEntryLines);
				var rowCount = expectedOutput.GetRowData().Count;
				Assert($"No expected data rows for scenario: {scenario.ScenarioName}", rowCount > 0);
				CheckReportData(scenario, expectedOutput);
			}
		}

		public void TestZACustomsEntryLinesReport_OverrideCustomsOffice()
		{
			var declarationList = PrepareDeclarations(true);
			Factory.Save();
			var scenarioList = new List<ReportTestScenario>() { CreateTestScenario_FilterOn_CustomsOffice("OV5") };
			var allEntryLines = CollectAllEntryLines(declarationList);
			foreach (var scenario in scenarioList)
			{
				var expectedOutput = DetermineExpectedOutput(scenario, allEntryLines, true);
				var rowCount = expectedOutput.GetRowData().Count;
				Assert($"No expected data rows for scenario: {scenario.ScenarioName}", rowCount > 0);
				CheckReportData(scenario, expectedOutput);
			}
		}

		ReportTestScenario CreateTestScenario_FilterOn_CurrentCompany_and_ShipmentType()
		{
			var parameterHelper = new ReportParameterHelper(reportDataObjectDetails);
			parameterHelper.SetParameterValue("@CurrentCompany", GlbCompany.CurrentCompany.PK.ToString());
			parameterHelper.SetParameterValue("@ShipmentType", "EXP");
			var scenario = new ReportTestScenario();
			scenario.ScenarioName = "Filter on current company and shipment type.";
			scenario.ParametersValuesList = parameterHelper.GetParametersValuesList();
			scenario.FiltersUsed = parameterHelper.GetFiltersUsed();
			scenario.ExpectedRowCount = 2;
			scenario.AddExpectedText(1, "[JobNumber]='B00001001'");
			scenario.AddExpectedText(1, "[ShipmentType]='EXP'");
			scenario.AddExpectedText(2, "[JobNumber]='B00001002'");
			scenario.AddExpectedText(2, "[ShipmentType]='EXP'");
			return scenario;
		}

		ReportTestScenario CreateTestScenario_FilterOn_TransportMode()
		{
			var parameterHelper = new ReportParameterHelper(reportDataObjectDetails);
			parameterHelper.SetParameterValue("@CurrentCompany", GlbCompany.CurrentCompany.PK.ToString());
			parameterHelper.SetParameterValue("@ShipmentType", "IMP");
			parameterHelper.SetParameterValue("@TransportMode", "AIR");
			var scenario = new ReportTestScenario();
			scenario.ScenarioName = "Filter on transport mode.";
			scenario.ParametersValuesList = parameterHelper.GetParametersValuesList();
			scenario.FiltersUsed = parameterHelper.GetFiltersUsed();
			scenario.ExpectedRowCount = 1;
			scenario.AddExpectedText(1, "[JobNumber]='B00001003'");
			scenario.AddExpectedText(1, "[TransportMode]='AIR'");
			return scenario;
		}

		ReportTestScenario CreateTestScenario_FilterOn_CustomsOffice(string customsOfficeOverride = "")
		{
			var customsOffice = string.IsNullOrEmpty(customsOfficeOverride) ? "OF5" : customsOfficeOverride;
			var parameterHelper = new ReportParameterHelper(reportDataObjectDetails);
			parameterHelper.SetParameterValue("@CurrentCompany", GlbCompany.CurrentCompany.PK.ToString());
			parameterHelper.SetParameterValue("@ShipmentType", "IMP");
			parameterHelper.SetParameterValue("@CustomsOffice", customsOffice);
			var scenario = new ReportTestScenario();
			scenario.ScenarioName = "Filter on customs office.";
			scenario.ParametersValuesList = parameterHelper.GetParametersValuesList();
			scenario.FiltersUsed = parameterHelper.GetFiltersUsed();
			scenario.ExpectedRowCount = 2;
			scenario.AddExpectedText(1, "[JobNumber]='B00001004'");
			scenario.AddExpectedText(1, $"[CustomsOffice]='{customsOffice}'");
			scenario.AddExpectedText(1, "[NoOfContainers]='1'");
			scenario.AddExpectedText(2, "[JobNumber]='B00001004'");
			scenario.AddExpectedText(2, $"[CustomsOffice]='{customsOffice}'");
			scenario.AddExpectedText(2, "[NoOfContainers]='0'");
			return scenario;
		}

		ReportTestScenario CreateTestScenario_FilterOn_JobRegistrationDate()
		{
			var parameterHelper = new ReportParameterHelper(reportDataObjectDetails);
			parameterHelper.SetParameterValue("@CurrentCompany", GlbCompany.CurrentCompany.PK.ToString());
			parameterHelper.SetParameterValue("@ShipmentType", "IMP");
			parameterHelper.SetParameterValue("@JobRegistedOnFrom", (new ZDateTime(2018, 12, 31)).ToISO8601String());
			parameterHelper.SetParameterValue("@JobRegistedOnTo", (new ZDateTime(2019, 4, 1)).ToISO8601String());
			var scenario = new ReportTestScenario();
			scenario.ScenarioName = "Filter on job registration date.";
			scenario.ParametersValuesList = parameterHelper.GetParametersValuesList();
			scenario.FiltersUsed = parameterHelper.GetFiltersUsed();
			scenario.ExpectedRowCount = 2;
			scenario.AddExpectedText(1, "[JobNumber]='B00001000'");
			scenario.AddExpectedText(1, "[JobRegisteredDate]='2019-03-31T00:00:00'");
			scenario.AddExpectedText(1, "[NoOfContainers]='2'");
			scenario.AddExpectedText(1, "[FirstContainerNo]='NNN-001'");
			scenario.AddExpectedText(1, "[ImporterCode]='IMP00,IMP03,IMP04'");
			scenario.AddExpectedText(1, "[SupplierCode]='SUP00,SUP03,SUP04'");
			scenario.AddExpectedText(2, "[JobNumber]='B00001000'");
			scenario.AddExpectedText(2, "[JobRegisteredDate]='2019-03-31T00:00:00'");
			scenario.AddExpectedText(2, "[NoOfContainers]='2'");
			scenario.AddExpectedText(2, "[FirstContainerNo]='NNN-002'");
			scenario.AddExpectedText(2, "[ImporterCode]='IMP00'");
			scenario.AddExpectedText(2, "[SupplierCode]='SUP00'");
			return scenario;
		}

		ReportTestScenario CreateTestScenario_FilterOn_EntrySubmissionDate()
		{
			var parameterHelper = new ReportParameterHelper(reportDataObjectDetails);
			parameterHelper.SetParameterValue("@CurrentCompany", GlbCompany.CurrentCompany.PK.ToString());
			parameterHelper.SetParameterValue("@ShipmentType", "EXP");
			parameterHelper.SetParameterValue("@EntrySubmittedDateFrom", (new ZDateTime(2017, 08, 08)).ToISO8601String());
			parameterHelper.SetParameterValue("@EntrySubmittedDateTo", (new ZDateTime(2017, 08, 11)).ToISO8601String());
			var scenario = new ReportTestScenario();
			scenario.ScenarioName = "Filter on entry submission date.";
			scenario.ParametersValuesList = parameterHelper.GetParametersValuesList();
			scenario.FiltersUsed = parameterHelper.GetFiltersUsed();
			scenario.ExpectedRowCount = 1;
			scenario.AddExpectedText(1, "[JobNumber]='B00001002'");
			scenario.AddExpectedText(1, "[EntrySubmittedDate]='2017-08-09T00:00:00'");
			return scenario;
		}

		ReportTestScenario CreateTestScenario_FilterOn_AssessmentDate()
		{
			var parameterHelper = new ReportParameterHelper(reportDataObjectDetails);
			parameterHelper.SetParameterValue("@CurrentCompany", GlbCompany.CurrentCompany.PK.ToString());
			parameterHelper.SetParameterValue("@ShipmentType", "IMP");
			parameterHelper.SetParameterValue("@AssessmentDateFrom", (new ZDateTime(2018, 01, 11)).ToISO8601String());
			parameterHelper.SetParameterValue("@AssessmentDateTo", (new ZDateTime(2018, 1, 13)).ToISO8601String());
			var scenario = new ReportTestScenario();
			scenario.ScenarioName = "Filter on assessment date.";
			scenario.ParametersValuesList = parameterHelper.GetParametersValuesList();
			scenario.FiltersUsed = parameterHelper.GetFiltersUsed();
			scenario.ExpectedRowCount = 1;
			scenario.AddExpectedText(1, "[JobNumber]='B00001003'");
			scenario.AddExpectedText(1, "[AssessmentDate]='2018-01-12T00:00:00'");
			return scenario;
		}

		ReportTestScenario CreateTestScenario_FilterOn_CustomsProcedureCode()
		{
			var parameterHelper = new ReportParameterHelper(reportDataObjectDetails);
			parameterHelper.SetParameterValue("@CurrentCompany", GlbCompany.CurrentCompany.PK.ToString());
			parameterHelper.SetParameterValue("@ShipmentType", "IMP");
			parameterHelper.SetParameterValue("@ProcedureCode", "A01");
			var scenario = new ReportTestScenario();
			scenario.ScenarioName = "Filter on Customs procedure code.";
			scenario.ParametersValuesList = parameterHelper.GetParametersValuesList();
			scenario.FiltersUsed = parameterHelper.GetFiltersUsed();
			scenario.ExpectedRowCount = 2;
			scenario.AddExpectedText(1, "[JobNumber]='B00001000'");
			scenario.AddExpectedText(1, "[CPC]='A01'");
			scenario.AddExpectedText(2, "[JobNumber]='B00001000'");
			scenario.AddExpectedText(2, "[CPC]='A01'");
			return scenario;
		}

		ReportTestScenario CreateTestScenario_FilterOn_EntryStatus()
		{
			var parameterHelper = new ReportParameterHelper(reportDataObjectDetails);
			parameterHelper.SetParameterValue("@CurrentCompany", GlbCompany.CurrentCompany.PK.ToString());
			parameterHelper.SetParameterValue("@ShipmentType", "IMP");
			parameterHelper.SetParameterValue("@EntryStatus", "5");
			var scenario = new ReportTestScenario();
			scenario.ScenarioName = "Filter on entry status.";
			scenario.ParametersValuesList = parameterHelper.GetParametersValuesList();
			scenario.FiltersUsed = parameterHelper.GetFiltersUsed();
			scenario.ExpectedRowCount = 2;
			scenario.AddExpectedText(1, "[JobNumber]='B00001004'");
			scenario.AddExpectedText(1, "[EntryStatus]='5'");
			scenario.AddExpectedText(1, "[CustomsOffice]='OF5'");
			scenario.AddExpectedText(2, "[JobNumber]='B00001004'");
			scenario.AddExpectedText(2, "[EntryStatus]='5'");
			scenario.AddExpectedText(2, "[CustomsOffice]='OF5'");
			return scenario;
		}

		ReportTestScenario CreateTestScenario_FilterOn_MessageStatus()
		{
			var parameterHelper = new ReportParameterHelper(reportDataObjectDetails);
			parameterHelper.SetParameterValue("@CurrentCompany", GlbCompany.CurrentCompany.PK.ToString());
			parameterHelper.SetParameterValue("@ShipmentType", "EXP");
			parameterHelper.SetParameterValue("@MessageStatus", "AWA");
			var scenario = new ReportTestScenario();
			scenario.ScenarioName = "Filter on message status.";
			scenario.ParametersValuesList = parameterHelper.GetParametersValuesList();
			scenario.FiltersUsed = parameterHelper.GetFiltersUsed();
			scenario.ExpectedRowCount = 1;
			scenario.AddExpectedText(1, "[JobNumber]='B00001001'");
			scenario.AddExpectedText(1, "[MessageStatus]='AWA'");
			scenario.AddExpectedText(1, "[CustomsOffice]='OF2'");
			scenario.AddExpectedText(1, "[CPC]='A02'");
			return scenario;
		}

		ReportTestScenario CreateTestScenario_FilterOn_EntryReleaseDate()
		{
			var parameterHelper = new ReportParameterHelper(reportDataObjectDetails);
			parameterHelper.SetParameterValue("@CurrentCompany", GlbCompany.CurrentCompany.PK.ToString());
			parameterHelper.SetParameterValue("@ShipmentType", "EXP");
			parameterHelper.SetParameterValue("@EntryReleaseDateFrom", (new ZDateTime(2019, 1, 8)).ToISO8601String());
			parameterHelper.SetParameterValue("@EntryReleaseDateTo", (new ZDateTime(2019, 1, 10)).ToISO8601String());
			var scenario = new ReportTestScenario();
			scenario.ScenarioName = "Filter on entry release date.";
			scenario.ParametersValuesList = parameterHelper.GetParametersValuesList();
			scenario.FiltersUsed = parameterHelper.GetFiltersUsed();
			scenario.ExpectedRowCount = 1;
			scenario.AddExpectedText(1, "[JobNumber]='B00001002'");
			scenario.AddExpectedText(1, "[EntryReleaseDate]='2019-01-09T00:00:00'");
			return scenario;
		}

		ReportTestScenario CreateTestScenario_FilterOn_ImporterAndSupplier()
		{
			var parameterHelper = new ReportParameterHelper(reportDataObjectDetails);
			parameterHelper.SetParameterValue("@CurrentCompany", GlbCompany.CurrentCompany.PK.ToString());
			parameterHelper.SetParameterValue("@ShipmentType", "IMP");
			var scenario = new ReportTestScenario();
			scenario.ScenarioName = "Filter on Importer and Supplier.";
			scenario.ParametersValuesList = parameterHelper.GetParametersValuesList();
			scenario.FiltersUsed = parameterHelper.GetFiltersUsed();
			scenario.FiltersUsed.Add("@SupplierPK", supplierForSearchPK.ToString());
			scenario.FiltersUsed.Add("@ImporterPK", importerForSearchPK.ToString());
			scenario.AdditionalWhereClause = $" where SupplierPK in ('{supplierForSearchPK.ToString()}') and ImporterPK in ('{importerForSearchPK.ToString()}')";
			scenario.ExpectedRowCount = 2;
			scenario.AddExpectedText(1, "[JobNumber]='B00001000'");
			scenario.AddExpectedText(1, "[ShipmentType]='IMP'");
			scenario.AddExpectedText(1, "[EntryLineNo]='1'");
			scenario.AddExpectedText(2, "[JobNumber]='B00001000'");
			scenario.AddExpectedText(2, "[ShipmentType]='IMP'");
			scenario.AddExpectedText(2, "[EntryLineNo]='2'");
			return scenario;
		}

		ReportTestScenario CreateTestScenario_FilterOn_AllFilterFields()
		{
			var parameterHelper = new ReportParameterHelper(reportDataObjectDetails);
			parameterHelper.SetParameterValue("@CurrentCompany", GlbCompany.CurrentCompany.PK.ToString());
			parameterHelper.SetParameterValue("@ShipmentType", "IMP");
			parameterHelper.SetParameterValue("@TransportMode", "AIR");
			parameterHelper.SetParameterValue("@CustomsOffice", "OF4");
			parameterHelper.SetParameterValue("@JobRegistedOnFrom", (new ZDateTime(2019, 4, 8)).ToISO8601String());
			parameterHelper.SetParameterValue("@JobRegistedOnTo", (new ZDateTime(2019, 4, 10)).ToISO8601String());
			parameterHelper.SetParameterValue("@EntrySubmittedDateFrom", (new ZDateTime(2017, 8, 11)).ToISO8601String());
			parameterHelper.SetParameterValue("@EntrySubmittedDateTo", (new ZDateTime(2017, 8, 13)).ToISO8601String());
			parameterHelper.SetParameterValue("@AssessmentDateFrom", (new ZDateTime(2018, 1, 11)).ToISO8601String());
			parameterHelper.SetParameterValue("@AssessmentDateTo", (new ZDateTime(2018, 1, 13)).ToISO8601String());
			parameterHelper.SetParameterValue("@ProcedureCode", "A04");
			parameterHelper.SetParameterValue("@EntryStatus", "4");
			parameterHelper.SetParameterValue("@MessageStatus", "CC");
			parameterHelper.SetParameterValue("@EntryReleaseDateFrom", (new ZDateTime(2019, 1, 11)).ToISO8601String());
			parameterHelper.SetParameterValue("@EntryReleaseDateTo", (new ZDateTime(2019, 1, 13)).ToISO8601String());
			var scenario = new ReportTestScenario();
			scenario.ScenarioName = "Filter on all filter fields.";
			scenario.ParametersValuesList = parameterHelper.GetParametersValuesList();
			scenario.FiltersUsed = parameterHelper.GetFiltersUsed();
			scenario.ExpectedRowCount = 1;
			scenario.AddExpectedText(1, "[JobNumber]='B00001003'");
			scenario.AddExpectedText(1, "[CustomsOffice]='OF4'");
			scenario.AddExpectedText(1, "[CPC]='A04'");
			scenario.AddExpectedText(1, "[EntryStatus]='4'");
			scenario.AddExpectedText(1, "[MessageStatus]='CC'");
			return scenario;
		}

		List<CusEntryLine> CollectAllEntryLines(List<JobDeclaration> declarationList)
		{
			return declarationList.SelectMany(x => x.CustomsEntryHeaders.Cast<CusEntryHeader>()).SelectMany(x => x.MergedLines).ToList();
		}

		ExpectedReportOutput DetermineExpectedOutput(ReportTestScenario scenario, List<CusEntryLine> allEntryLines, bool isCustomsOfficeOverride = false)
		{
			var filteredList = ApplyFiltersUsed(allEntryLines, scenario.FiltersUsed);
			var expectedOutput = new ExpectedReportOutput();
			filteredList.ForEach(x => expectedOutput.AddRow(GetRowText_from_EntryLine(x, isCustomsOfficeOverride)));
			return expectedOutput;
		}

		string GetRowText_from_EntryLine(CusEntryLine entryLine, bool isCustomsOfficeOverride = false)
		{
			var entry = entryLine.Header;
			var dec = entry.Declaration;
			var columnHelper = new ReportColumnHelper(reportDataObjectDetails.ExpectedColumnsInOrder);
			columnHelper.SetColumnValue("EntryHeaderPk", entry.PK);
			columnHelper.SetColumnValue("EntryReleaseDate", entry.CH_EntryReleaseDate);
			columnHelper.SetColumnValue("ShipmentType", entry.CH_MessageType);
			columnHelper.SetColumnValue("MessageStatus", entry.CH_Status);
			columnHelper.SetColumnValue("EntryStatus", entry.CH_EntryStatus);
			columnHelper.SetColumnValue("EntrySubmittedDate", entry.CH_EntrySubmittedDate);
			columnHelper.SetColumnValue("EntryLinePk", entryLine.PK);
			columnHelper.SetColumnValue("CustomsValue", entryLine.CL_CustomsValue);
			columnHelper.SetColumnValue("EntryLineNo", entryLine.CL_LineNumber);
			columnHelper.SetColumnValue("TariffCode", entryLine.CL_AdValoremTariff);
			var instr = entry.EntryInstruction;
			columnHelper.SetColumnValue("CPC", instr.CEI_Style);
			columnHelper.SetColumnValue("AssessmentDate", instr.CEI_DateForDuty);
			columnHelper.SetColumnValue("VesselName", dec.JE_VesselName);
			columnHelper.SetColumnValue("VoyFlight", dec.JE_VoyageFlightNo);
			columnHelper.SetColumnValue("ETA", dec.JE_DateOfArrival);
			columnHelper.SetColumnValue("JobNumber", dec.JE_DeclarationReference);
			columnHelper.SetColumnValue("TransportMode", dec.JE_TransportMode);
			columnHelper.SetColumnValue("ClientRef", dec.JE_OwnerRef);
			columnHelper.SetColumnValue("TransportDocNo", dec.JE_MasterBill);
			columnHelper.SetColumnValue("HouseBill", dec.JE_HouseBill);
			columnHelper.SetColumnValue("CustomsOffice", isCustomsOfficeOverride ? instr.CEI_CustomsOfficeOverride : dec.JE_CustomsOffice);
			columnHelper.SetColumnValue("JobRegisteredDate", dec.JE_SystemCreateTimeUtc);
			columnHelper.SetColumnValue("BranchPk", dec.JE_GB);
			var invoiceLines = GetInvoiceLinesForEntryLine(entryLine);
			columnHelper.SetColumnValue("StatsQty", invoiceLines.FindAll(x => !x.JI_CustomsQuantity.IsEmpty).Select(x => (decimal)x.JI_CustomsQuantity).Sum(), decimalPrecision: 6);
			columnHelper.SetColumnValue("AdditionalQty1", invoiceLines.FindAll(x => !x.JI_CustomsSecondQuantity.IsEmpty).Select(x => (decimal)x.JI_CustomsSecondQuantity).Sum(), decimalPrecision: 6);
			columnHelper.SetColumnValue("AdditionalQty2", invoiceLines.FindAll(x => !x.JI_CustomsThirdQuantity.IsEmpty).Select(x => (decimal)x.JI_CustomsThirdQuantity).Sum(), decimalPrecision: 6);
			columnHelper.SetColumnValue("StatsQtyUnit", FindFirstStringValue(invoiceLines, x => x.JI_CustomsUnitQty));
			columnHelper.SetColumnValue("OrderNumber", FindFirstStringValue(invoiceLines, x => x.JI_OrderNumber));
			columnHelper.SetColumnValue("ProductCode", FindFirstStringValue(invoiceLines, x => x.JI_PartNo));
			columnHelper.SetColumnValue("GoodsOrigin", FindFirstStringValue(invoiceLines, x => x.JI_CountryOfOrigin));
			columnHelper.SetColumnValue("AdditionalQty1Unit", FindFirstStringValue(invoiceLines, x => x.JI_CustomsSecondUnitQty));
			columnHelper.SetColumnValue("AdditionalQty2Unit", FindFirstStringValue(invoiceLines, x => x.JI_CustomsThirdUnitQty));
			columnHelper.SetColumnValue("EntryNumber", entry.MovementReferenceNumber);
			columnHelper.SetColumnValue("FirstContainerNo", GetFirstContainerNo(invoiceLines));
			columnHelper.SetColumnValue("NoOfContainers", GetNoOfContainers(invoiceLines));
			var importerCode = DetermineImporterCode(invoiceLines, dec);
			var supplierCode = DetermineSupplierCode(invoiceLines, dec);
			columnHelper.SetColumnValue("ImporterCode", importerCode);
			if (!dec.JE_OH_Importer.IsEmpty)
			{
				columnHelper.SetColumnValue("ImporterPK", dec.JE_OH_Importer.ToString());
			}

			columnHelper.SetColumnValue("SupplierCode", supplierCode);
			if (!dec.JE_OH_Supplier.IsEmpty)
			{
				columnHelper.SetColumnValue("SupplierPK", dec.JE_OH_Supplier.ToString());
			}

			return columnHelper.GetRowText();
		}

		string FindFirstStringValue(List<JobComInvoiceLine> invoiceLines, Func<JobComInvoiceLine, string> getValue)
		{
			string retValue = "";
			Action<JobComInvoiceLine> action = x =>
			{
				var theValue = getValue(x);
				if (!string.IsNullOrEmpty(theValue) && (string.IsNullOrEmpty(retValue)))
				{
					retValue = theValue;
				}
			};
			invoiceLines.ForEach(action);
			return retValue;
		}

		string GetFirstContainerNo(List<JobComInvoiceLine> invoiceLines)
		{
			var containerList = new List<string>();
			foreach (var invoiceLine in invoiceLines)
			{
				var query = new ZQuery(CusContainerInvoiceLinePivotSchema.C2_JI, invoiceLine.PK);
				var containerLinks = Factory.Load<CusContainerInvoiceLinePivot>(query);
				foreach (var containerLink in containerLinks)
				{
					var container = Factory.Load<CusContainer>(containerLink.C2_CO);
					var containerNo = container?.CO_ContainerNumber;
					if (!string.IsNullOrEmpty(containerNo))
					{
						if (!containerList.Contains(containerNo))
						{
							containerList.Add(containerNo);
						}
					}
				}
			}

			containerList.Sort();
			return containerList.Count > 0 ? containerList[0] : "";
		}

		int GetNoOfContainers(List<JobComInvoiceLine> invoiceLines)
		{
			var containerList = new List<string>();
			foreach (var invoiceLine in invoiceLines)
			{
				var query = new ZQuery(CusContainerInvoiceLinePivotSchema.C2_JI, invoiceLine.PK);
				var containerLinks = Factory.Load<CusContainerInvoiceLinePivot>(query);
				foreach (var containerLink in containerLinks)
				{
					var container = Factory.Load<CusContainer>(containerLink.C2_CO);
					var containerNo = container?.CO_ContainerNumber;
					if (!string.IsNullOrEmpty(containerNo))
					{
						if (!containerList.Contains(containerNo))
						{
							containerList.Add(containerNo);
						}
					}
				}
			}

			return containerList.Count;
		}

		string DetermineImporterCode(List<JobComInvoiceLine> invoiceLines, JobDeclaration dec)
		{
			Func<JobComInvoiceHeader, ZGuid> invoiceOrgPkToUse = x => x.JZ_OH_Consignee;
			var uniqueInvoiceIds = GetUniqueInvoiceIds(invoiceLines);
			var uniqueImporterCodes = GetUniqueOrgCodesFromInvoiceIds(uniqueInvoiceIds, invoiceOrgPkToUse);
			if (uniqueImporterCodes.Count > 0)
			{
				return ConvertToStringSegmentList(uniqueImporterCodes).Aggregate((x, y) => x + y);
			}

			Func<JobDeclaration, ZGuid> primary_OrgPk_ToUse = x => x.JE_OH_Consignee;
			Func<JobDeclaration, ZGuid> secondaryOrgPkToUse = x => x.JE_OH_Importer;
			return GetOrgCodeFromDeclaration(dec, primary_OrgPk_ToUse, secondaryOrgPkToUse);
		}

		string DetermineSupplierCode(List<JobComInvoiceLine> invoiceLines, JobDeclaration dec)
		{
			Func<JobComInvoiceHeader, ZGuid> invoiceOrgPkToUse = x => x.JZ_OH_Supplier;
			var uniqueInvoiceIds = GetUniqueInvoiceIds(invoiceLines);
			var uniqueSupplierCodes = GetUniqueOrgCodesFromInvoiceIds(uniqueInvoiceIds, invoiceOrgPkToUse);
			if (uniqueSupplierCodes.Count > 0)
			{
				return ConvertToStringSegmentList(uniqueSupplierCodes).Aggregate((x, y) => x + y);
			}

			Func<JobDeclaration, ZGuid> primary_OrgPk_ToUse = x => x.JE_OH_Supplier;
			Func<JobDeclaration, ZGuid> secondaryOrgPkToUse = x => x.JE_OH_Exporter;
			return GetOrgCodeFromDeclaration(dec, primary_OrgPk_ToUse, secondaryOrgPkToUse);
		}

		List<ZGuid> GetUniqueInvoiceIds(List<JobComInvoiceLine> invoiceLines)
		{
			var uniqueInvoiceIds = new List<ZGuid>();
			Action<JobComInvoiceLine> getUniqueInvoiceIds = x =>
			{
				var invoiceId = x.JI_JZ;
				if (!uniqueInvoiceIds.Contains(invoiceId))
				{
					uniqueInvoiceIds.Add(invoiceId);
				}
			};
			invoiceLines.ForEach(getUniqueInvoiceIds);
			return uniqueInvoiceIds;
		}

		List<string> GetUniqueOrgCodesFromInvoiceIds(List<ZGuid> uniqueInvoiceIds, Func<JobComInvoiceHeader, ZGuid> getTheOrgGuidToUse)
		{
			var uniqueOrgCodes = new List<string>();
			Action<ZGuid> populateUniqueOrgCodeList = x =>
			{
				var invoice = Factory.Load<JobComInvoiceHeader>(x);
				var orgPK = getTheOrgGuidToUse(invoice);
				if (!orgPK.IsEmpty)
				{
					var org = Factory.Load<OrgHeader>(orgPK);
					var orgCode = org.OH_Code;
					if (!uniqueOrgCodes.Contains(orgCode))
					{
						uniqueOrgCodes.Add(orgCode);
					}
				}
			};
			uniqueInvoiceIds.ForEach(populateUniqueOrgCodeList);
			return uniqueOrgCodes;
		}

		List<string> ConvertToStringSegmentList(List<string> values)
		{
			var segments = new List<string>();
			Action<string> populateSegments = x =>
			{
				if (segments.Count > 0)
				{
					segments.Add(",");
				}

				segments.Add(x);
			};
			values.ForEach(populateSegments);
			return segments;
		}

		string GetOrgCodeFromDeclaration(JobDeclaration dec, Func<JobDeclaration, ZGuid> primary_OrgPk_ToUse, Func<JobDeclaration, ZGuid> secondaryOrgPkToUse)
		{
			var pk1 = primary_OrgPk_ToUse(dec);
			if (!pk1.IsEmpty)
			{
				var org = Factory.Load<OrgHeader>(pk1);
				return org.OH_Code;
			}

			var pk2 = secondaryOrgPkToUse(dec);
			if (!pk2.IsEmpty)
			{
				var org = Factory.Load<OrgHeader>(pk2);
				return org.OH_Code;
			}

			return null;
		}

		List<JobComInvoiceLine> GetInvoiceLinesForEntryLine(CusEntryLine entryLine)
		{
			var query = new ZQuery(JobComInvoiceLineSchema.JI_CL, entryLine.PK);
			var invoiceLines = Factory.Load<JobComInvoiceLine>(query);
			return new List<JobComInvoiceLine>(invoiceLines);
		}

		List<CusEntryLine> ApplyFiltersUsed(List<CusEntryLine> allEntryLines, Dictionary<string, string> filtersUsed)
		{
			var entryLines = new List<CusEntryLine>(allEntryLines);
			var predicates = new List<Predicate<CusEntryLine>>();
			Action<string> compilePredicate = filterField =>
			{
				var filterValue = filtersUsed[filterField];
				switch (filterField)
				{
					case "@ShipmentType":
						predicates.Add(x => x.Header.Declaration.JE_MessageType == filterValue);
						break;
					case "@TransportMode":
						predicates.Add(x => x.Header.Declaration.JE_TransportMode == filterValue);
						break;
					case "@CustomsOffice":
						predicates.Add(x => x.Header.CustomsOffice == filterValue);
						break;
					case "@JobRegistedOnFrom":
						predicates.Add(x => x.Header.Declaration.JE_SystemCreateTimeUtc >= new ZDateTime(filterValue));
						break;
					case "@JobRegistedOnTo":
						predicates.Add(x => x.Header.Declaration.JE_SystemCreateTimeUtc <= new ZDateTime(filterValue));
						break;
					case "@EntrySubmittedDateFrom":
						predicates.Add(x => x.Header.CH_EntrySubmittedDate >= new ZDateTime(filterValue));
						break;
					case "@EntrySubmittedDateTo":
						predicates.Add(x => x.Header.CH_EntrySubmittedDate <= new ZDateTime(filterValue));
						break;
					case "@AssessmentDateFrom":
						predicates.Add(x => x.Header.EntryInstruction.AssessmentDate >= new ZDateTime(filterValue));
						break;
					case "@AssessmentDateTo":
						predicates.Add(x => x.Header.EntryInstruction.AssessmentDate <= new ZDateTime(filterValue));
						break;
					case "@ProcedureCode":
						predicates.Add(x => x.Header.EntryInstruction.CEI_Style == filterValue);
						break;
					case "@EntryStatus":
						predicates.Add(x => x.Header.CH_EntryStatus == filterValue);
						break;
					case "@MessageStatus":
						predicates.Add(x => x.Header.CH_Status == filterValue);
						break;
					case "@EntryReleaseDateFrom":
						predicates.Add(x => x.Header.CH_EntryReleaseDate >= new ZDateTime(filterValue));
						break;
					case "@EntryReleaseDateTo":
						predicates.Add(x => x.Header.CH_EntryReleaseDate <= new ZDateTime(filterValue));
						break;
					case "@ImporterPK":
						predicates.Add(x => x.Header.Declaration.JE_OH_Importer.ToString() == filterValue);
						break;
					case "@SupplierPK":
						predicates.Add(x => x.Header.Declaration.JE_OH_Supplier.ToString() == filterValue);
						break;
				}
			};
			filtersUsed.Keys.ToList().ForEach(compilePredicate);
			predicates.ForEach(x => entryLines = entryLines.FindAll(x));
			return entryLines;
		}

		List<JobDeclaration> PrepareDeclarations(bool isCustomsOfficeOverride = false)
		{
			var declarationList = new List<JobDeclaration>();
			#region Create Factories:
			var supplierFactory = new SupplierFactory(Factory);
			var importerFactory = new ImporterFactory(Factory);
			var decFactory = new DeclarationFactory(Factory);
			var entryFactory = new EntryHeaderFactory(Factory);
			var entryLineFactory = new EntryLineFactory(Factory);
			var invoiceFactory = new CommercialInvoiceHeaderFactory(Factory);
			var invoiceLineFactory = new CommercialInvoiceLineFactory(Factory);
			#endregion Create Factories
			#region Declaration 01:
			var invoice = invoiceFactory.CreateInvoice_with_Lines(4, invoiceLineFactory);
			invoice.JZ_OH_Supplier = supplierFactory.CreateSupplier().PK;
			invoice.JZ_OH_Consignee = importerFactory.CreateImporter().PK;
			var dec = decFactory.CreateDeclaration_with_CusContainers("SEA", "IMP", 4);
			supplierForSearchPK = supplierFactory.CreateSupplier().PK;
			dec.JE_OH_Supplier = supplierForSearchPK;
			dec.JE_OH_Exporter = supplierFactory.CreateSupplier().PK;
			dec.JE_OH_Consignee = importerFactory.CreateImporter().PK;
			importerForSearchPK = importerFactory.CreateImporter().PK;
			dec.JE_OH_Importer = importerForSearchPK;
			LinkContainerToInvoiceLine(invoice.JobComInvoiceLines[0], dec.CusContainers[0]);
			LinkContainerToInvoiceLine(invoice.JobComInvoiceLines[1], dec.CusContainers[1]);
			LinkContainerToInvoiceLine(invoice.JobComInvoiceLines[2], dec.CusContainers[2]);
			LinkContainerToInvoiceLine(invoice.JobComInvoiceLines[3], dec.CusContainers[3]);
			var entry = entryFactory.CreateEntry_with_Lines(dec, 2, entryLineFactory, isCustomsOfficeOverride ? "OV1" : string.Empty);
			var invoice02 = invoiceFactory.CreateInvoice_with_Lines(1, invoiceLineFactory);
			invoice02.JZ_OH_Supplier = supplierFactory.CreateSupplier().PK;
			invoice02.JZ_OH_Consignee = importerFactory.CreateImporter().PK;
			var invoice03 = invoiceFactory.CreateInvoice_with_Lines(1, invoiceLineFactory);
			invoice03.JZ_OH_Supplier = supplierFactory.CreateSupplier().PK;
			invoice03.JZ_OH_Consignee = importerFactory.CreateImporter().PK;
			var group01 = new List<JobComInvoiceLine>()
			{ invoice.JobComInvoiceLines[0], invoice.JobComInvoiceLines[2], invoice02.JobComInvoiceLines[0], invoice03.JobComInvoiceLines[0] };
			var group02 = new List<JobComInvoiceLine>()
			{ invoice.JobComInvoiceLines[1], invoice.JobComInvoiceLines[3] };
			MergeInvoiceLines(entry.MergedLines[0], group01);
			MergeInvoiceLines(entry.MergedLines[1], group02);
			declarationList.Add(dec);
			#endregion Declaration 01
			#region Declaration 02:
			invoice = invoiceFactory.CreateInvoice_with_Lines(1, invoiceLineFactory);
			dec = decFactory.CreateDeclaration_with_CusContainers("SEA", "EXP", 1);
			dec.JE_OH_Exporter = supplierFactory.CreateSupplier().PK;
			dec.JE_OH_Importer = importerFactory.CreateImporter().PK;
			LinkContainerToInvoiceLine(invoice.JobComInvoiceLines[0], dec.CusContainers[0]);
			entry = entryFactory.CreateEntry_with_Lines(dec, 1, entryLineFactory, isCustomsOfficeOverride ? "OV2" : string.Empty);
			invoice.JobComInvoiceLines[0].JI_CL = entry.MergedLines[0].PK;
			declarationList.Add(dec);
			#endregion Declaration 02
			#region Declaration 03:
			dec = decFactory.CreateDeclaration_with_CusContainers("SEA", "EXP", 1);
			dec.JE_OH_Supplier = supplierFactory.CreateSupplier().PK;
			dec.JE_OH_Consignee = importerFactory.CreateImporter().PK;
			invoice = invoiceFactory.CreateInvoice_with_Lines(2, invoiceLineFactory);
			LinkContainerToInvoiceLine(invoice.JobComInvoiceLines[0], dec.CusContainers[0]);
			LinkContainerToInvoiceLine(invoice.JobComInvoiceLines[1], dec.CusContainers[0]);
			entry = entryFactory.CreateEntry_with_Lines(dec, 1, entryLineFactory, isCustomsOfficeOverride ? "OV3" : string.Empty);
			invoice.JobComInvoiceLines[0].JI_CL = entry.MergedLines[0].PK;
			invoice.JobComInvoiceLines[1].JI_CL = entry.MergedLines[0].PK;
			declarationList.Add(dec);
			#endregion Declaration 03
			#region Declaration 04:
			invoice = invoiceFactory.CreateInvoice_with_Lines(3, invoiceLineFactory);
			dec = decFactory.CreateDeclaration_with_CusContainers("AIR", "IMP", 0);
			entry = entryFactory.CreateEntry_with_Lines(dec, 1, entryLineFactory, isCustomsOfficeOverride ? "OV4" : string.Empty);
			invoice.JobComInvoiceLines[0].JI_CL = entry.MergedLines[0].PK;
			invoice.JobComInvoiceLines[1].JI_CL = entry.MergedLines[0].PK;
			invoice.JobComInvoiceLines[2].JI_CL = entry.MergedLines[0].PK;
			declarationList.Add(dec);
			#endregion Declaration 04
			#region Declaration 05:
			dec = decFactory.CreateDeclaration_with_CusContainers("SEA", "IMP", 1);
			dec.JE_OH_Supplier = supplierFactory.CreateSupplier().PK;
			dec.JE_OH_Exporter = supplierFactory.CreateSupplier().PK;
			invoice = invoiceFactory.CreateInvoice_with_Lines(4, invoiceLineFactory);
			invoice.JZ_OH_Consignee = importerFactory.CreateImporter().PK;
			LinkContainerToInvoiceLine(invoice.JobComInvoiceLines[2], dec.CusContainers[0]);
			entry = entryFactory.CreateEntry_with_Lines(dec, 2, entryLineFactory, isCustomsOfficeOverride ? "OV5" : string.Empty);
			invoice.JobComInvoiceLines[0].JI_CL = entry.MergedLines[1].PK;
			invoice.JobComInvoiceLines[1].JI_CL = entry.MergedLines[1].PK;
			invoice.JobComInvoiceLines[2].JI_CL = entry.MergedLines[0].PK;
			invoice.JobComInvoiceLines[3].JI_CL = entry.MergedLines[0].PK;
			declarationList.Add(dec);
			#endregion Declaration 05
			return declarationList;
		}

		void LinkContainerToInvoiceLine(JobComInvoiceLine invLine, CusContainer container)
		{
			var conLink = Factory.New<CusContainerInvoiceLinePivot>();
			conLink.C2_JI = invLine.PK;
			conLink.C2_CO = container.PK;
		}

		void MergeInvoiceLines(CusEntryLine entryLine, List<JobComInvoiceLine> invoiceLines)
		{
			int total = invoiceLines.Count;
			var firstLine = invoiceLines[0];
			if (total > 1)
			{
				var tail = invoiceLines.Skip(1).ToList();
				tail.ForEach(invLine =>
				{
					invLine.JI_CustomsUnitQty = firstLine.JI_CustomsUnitQty;
					invLine.JI_OrderNumber = firstLine.JI_OrderNumber;
					invLine.JI_PartNo = firstLine.JI_PartNo;
					invLine.JI_CountryOfOrigin = firstLine.JI_CountryOfOrigin;
					invLine.JI_CustomsSecondUnitQty = firstLine.JI_CustomsSecondUnitQty;
					invLine.JI_CustomsThirdUnitQty = firstLine.JI_CustomsThirdUnitQty;
				});
			}

			invoiceLines.ForEach(x => x.JI_CL = entryLine.PK);
		}

		protected override void PrepareTestData()
		{
			var supplierFactory = new SupplierFactory(Factory);
			var importerFactory = new ImporterFactory(Factory);
			var decFactory = new DeclarationFactory(Factory);
			var entryFactory = new EntryHeaderFactory(Factory);
			var entryLineFactory = new EntryLineFactory(Factory);
			var invoiceFactory = new CommercialInvoiceHeaderFactory(Factory);
			var invoiceLineFactory = new CommercialInvoiceLineFactory(Factory);
			var dec = decFactory.CreateDeclaration_with_CusContainers("SEA", "IMP", 0);
			var entry = entryFactory.CreateEntry_with_Lines(dec, 1, entryLineFactory);
			var invoice = invoiceFactory.CreateInvoice_with_Lines(1, invoiceLineFactory);
			invoice.JZ_OH_Supplier = supplierFactory.CreateSupplier().PK;
			invoice.JZ_OH_Consignee = importerFactory.CreateImporter().PK;
			invoice.JobComInvoiceLines[0].JI_CL = entry.MergedLines[0].PK;
		}

		protected override void AssertTestResults(System.Data.DataTable results)
		{
			AssertEquals(1, results.Rows.Count);
			var row1 = FormatRowsValues(results.Rows[0], results);
			var expectedData = "[EntryReleaseDate]='2019-01-03T00:00:00'; [ShipmentType]='IMP'; [MessageStatus]='ACK'; [EntryStatus]='1'; [EntrySubmittedDate]='2017-08-03T00:00:00'; [CustomsValue]='5679.0000'; [EntryLineNo]='1'; [TariffCode]='8.8.8.1'; [CPC]='A01'; [AssessmentDate]='2018-01-03T00:00:00'; [VesselName]='BlueOcean1'; [VoyFlight]='BA001'; [ETA]='2019-10-26T00:00:00'; [JobNumber]='B00001000'; [TransportMode]='SEA'; [ClientRef]='ClientRef-1'; [TransportDocNo]='TxDoc-1'; [HouseBill]='HouseBill-1'; [CustomsOffice]='OF1'; [JobRegisteredDate]='2019-03-31T00:00:00'; [StatsQty]='1112.000000'; [AdditionalQty1]='2224.000000'; [AdditionalQty2]='3336.000000'; [StatsQtyUnit]='U1'; [OrderNumber]='Order-1'; [ProductCode]='PartNo-1'; [GoodsOrigin]='AE'; [AdditionalQty1Unit]='V1'; [AdditionalQty2Unit]='W1'; [EntryNumber]='MRN-01'; [FirstContainerNo]=''; [NoOfContainers]='0'; [ImporterCode]='IMP00'; [SupplierCode]='SUP00'";
			AssertContainsMoreHelpfully(expectedData, row1);
		}

		ZGuid importerForSearchPK;
		ZGuid supplierForSearchPK;
	}
}
