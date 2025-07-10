using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.ZA;
using Enterprise.MasterFiles.Business;
using Enterprise.ReportTesting;

namespace Enterprise.Customs.ZA.Business.Report.Testing
{
	sealed class ZACustomsEntryHeaderReportTestHelper
	{
		internal static List<string> ParametersValuesList => new List<string>()
		{
			ReportFunctionalTestCase.QuoteParameter(GlbCompany.CurrentCompany.PK), // @CurrentCompany
			ReportFunctionalTestCase.QuoteParameter(ZAJobMessageTypeList.Codes.Import), // @ShipmentType
			ReportFunctionalTestCase.QuoteParameter(null), // @TransportMode
			ReportFunctionalTestCase.QuoteParameter(null), // @CustomsOffice
			ReportFunctionalTestCase.QuoteParameter(new DateTime(2023, 09, 01)), // @JobRegisteredOnFrom
			ReportFunctionalTestCase.QuoteParameter(null), // @JobRegisteredOnTo
			ReportFunctionalTestCase.QuoteParameter(null), // @EntrySubmittedDateFrom
			ReportFunctionalTestCase.QuoteParameter(null), // @EntrySubmittedDateTo
			ReportFunctionalTestCase.QuoteParameter(null), // @AssessmentDateFrom
			ReportFunctionalTestCase.QuoteParameter(null), // @AssessmentDateTo
			ReportFunctionalTestCase.QuoteParameter(null), // @ProcedureCode
			ReportFunctionalTestCase.QuoteParameter(null), // @EntryStatus
			ReportFunctionalTestCase.QuoteParameter(null), // @MessageStatus
			ReportFunctionalTestCase.QuoteParameter(null), // @ReleasePrintIndicator
			ReportFunctionalTestCase.QuoteParameter(null), // @EntryReleaseDateFrom
			ReportFunctionalTestCase.QuoteParameter(null), // @EntryReleaseDateTo
		};

		internal static List<ReportSchemaColumn> ExpectedColumnsInAnyOrder => new List<ReportSchemaColumn>()
		{
			new ReportSchemaColumn(typeof(DateTime), "AssessmentDate"),
			new ReportSchemaColumn(typeof(decimal), "BondAcquittalValue"),
			new ReportSchemaColumn(typeof(DateTime), "BondAcquittedDate"),
			new ReportSchemaColumn(typeof(string), "BondHolderCode"),
			new ReportSchemaColumn(typeof(string), "BondHolderName"),
			new ReportSchemaColumn(typeof(DateTime), "BondValidToDate"),
			new ReportSchemaColumn(typeof(Guid), "BranchPK"),
			new ReportSchemaColumn(typeof(string), "CountryOfDestination"),
			new ReportSchemaColumn(typeof(string), "CountryOfExit"),
			new ReportSchemaColumn(typeof(string), "CustomsOffice"),
			new ReportSchemaColumn(typeof(string), "EntryInstructionDescription"),
			new ReportSchemaColumn(typeof(string), "EntryNumber"),
			new ReportSchemaColumn(typeof(DateTime), "EntryReleaseDate"),
			new ReportSchemaColumn(typeof(string), "EntryStatus"),
			new ReportSchemaColumn(typeof(DateTime), "EntrySubmittedDate"),
			new ReportSchemaColumn(typeof(string), "FromWarehouseCode"),
			new ReportSchemaColumn(typeof(string), "FromWarehouseName"),
			new ReportSchemaColumn(typeof(string), "HouseBill"),
			new ReportSchemaColumn(typeof(string), "ImporterCode"),
			new ReportSchemaColumn(typeof(string), "ImporterName"),
			new ReportSchemaColumn(typeof(Guid), "ImporterPk"),
			new ReportSchemaColumn(typeof(string), "JobNumber"),
			new ReportSchemaColumn(typeof(DateTime), "JobRegisteredDate"),
			new ReportSchemaColumn(typeof(string), "LRN"),
			new ReportSchemaColumn(typeof(string), "MasterBill"),
			new ReportSchemaColumn(typeof(string), "NewWhOwnerCode"),
			new ReportSchemaColumn(typeof(string), "NewWhOwnerName"),
			new ReportSchemaColumn(typeof(int), "NoOfLines"),
			new ReportSchemaColumn(typeof(string), "NoOfPkgs"),
			new ReportSchemaColumn(typeof(string), "OfficeOfExit"),
			new ReportSchemaColumn(typeof(string), "PreviousMRN"),
			new ReportSchemaColumn(typeof(string), "ProcedureCode"),
			new ReportSchemaColumn(typeof(string), "RelPrintInd"),
			new ReportSchemaColumn(typeof(string), "RemoverCode"),
			new ReportSchemaColumn(typeof(string), "RemoverName"),
			new ReportSchemaColumn(typeof(string), "ShipmentType"),
			new ReportSchemaColumn(typeof(string), "SupplierCode"),
			new ReportSchemaColumn(typeof(string), "SupplierName"),
			new ReportSchemaColumn(typeof(Guid), "SupplierPk"),
			new ReportSchemaColumn(typeof(decimal), "TotalDuty"),
			new ReportSchemaColumn(typeof(decimal), "TotalPpPen"),
			new ReportSchemaColumn(typeof(decimal), "TotalS1p2b"),
			new ReportSchemaColumn(typeof(decimal), "TotalVat"),
			new ReportSchemaColumn(typeof(string), "ToWarehouseCode"),
			new ReportSchemaColumn(typeof(string), "ToWarehouseName"),
			new ReportSchemaColumn(typeof(string), "Transport"),
			new ReportSchemaColumn(typeof(string), "UcrNo"),
			new ReportSchemaColumn(typeof(string), "VoyFlight"),
			new ReportSchemaColumn(typeof(DateTime), "WarehouseReleaseDate"),
			new ReportSchemaColumn(typeof(string), "WarehouseTransactionStatus"),
		};

		internal static JobDeclaration GetJobDeclaration(BusinessObjectFactory factory, string declarationReference, ZGuid importerPK, ZGuid supplierPK, int clusterKey = 1)
		{
			var dec = factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = declarationReference;
			dec.JE_GB = GlbCompany.CurrentCompany.PK;
			dec.JE_GB = GlbBranch.CurrentBranch.PK;
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			dec.JE_OH_Importer = importerPK;
			dec.JE_OH_Supplier = supplierPK;
			dec.JE_CustomsOffice = "JHB";
			dec.JE_MasterBill = "OOCL12345678";
			dec.JE_RL_NKPortOfLoading = "CATOR";
			dec.JE_RL_NKPortOfArrival = "ZAJNB";
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			dec.JE_VoyageFlightNo = "SA666";
			dec.JE_HouseBill = "S00049567";
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			dec.JE_ClusterKey = clusterKey;
			dec.JE_SystemCreateTimeUtc = new DateTime(2023, 9, 1, 0, 0, 0);
			return dec;
		}

		internal static CusEntryInstruction GetEntryInstruction(BusinessObjectFactory factory, ZGuid declarationPK, ZGuid bondHolderPK, ZGuid carrierPK, ZGuid ownerPK, ZGuid fromWarehouseAddressPK, ZGuid toWarehouseAddressPK, int clusterKey = 1)
		{
			var instruction = factory.New<CusEntryInstruction>();
			instruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._00;
			instruction.CEI_JE = declarationPK;
			instruction.CEI_OA_Warehouse = fromWarehouseAddressPK;
			instruction.CEI_Description = "Entry Instruction Description";
			instruction.CEI_CreditTerms = "NEP";
			instruction.CEI_PortOfExit = "CTN";
			instruction.CEI_PreviousMRN = "DFM201609225000601";
			instruction.CEI_UCROrderNumber = "CPC6211SEATEST";
			instruction.CEI_OH_BondHolder = bondHolderPK;
			instruction.CEI_OH_Carrier = carrierPK;
			instruction.CEI_OA_Warehouse2 = toWarehouseAddressPK;
			instruction.CEI_OH_Owner = ownerPK;
			instruction.CEI_DateForDuty = new DateTime(2016, 09, 22);
			instruction.CEI_ClusterKey = clusterKey;
			return instruction;
		}

		internal static CusEntryHeader GetCusEntryHeader(BusinessObjectFactory factory, ZGuid declarationPK, ZGuid instructionPK, int clusterKey = 1)
		{
			var entry = factory.New<CusEntryHeader>();
			entry.CH_MessageType = ZAJobMessageTypeList.Codes.Import;
			entry.CH_Status = ZAMessageStatusList.Codes.AwaitingResponse;
			entry.CH_EntryStatus = "1";
			entry.CH_BGMReference = "00505655JSA20170214004008";
			entry.CH_TotalPaid = 470.33f;
			entry.CH_HighestLineNumber = 1;
			entry.CH_JE = declarationPK;
			entry.CH_EntrySubmittedDate = new DateTime(2023, 9, 2, 0, 11, 0);
			entry.CH_EntryReleaseDate = new DateTime(2023, 9, 3, 0, 12, 0);
			entry.CH_CEI_Instruction = instructionPK;
			entry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardUpdated;
			entry.CH_WarehouseReleaseDate = new DateTime(2023, 9, 4, 0, 14, 0);
			entry.CH_BondAcquittedDate = (ZDate)new DateTime(2023, 9, 5);
			entry.CH_BondValidToDate = (ZDate)new DateTime(2023, 9, 6);
			entry.CH_ClusterKey = clusterKey;
			entry.CH_Packages = 5;
			entry.CH_RelPrintInd = "Y";
			return entry;
		}
	}
}
