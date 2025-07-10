using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Module;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using AddInfoFilterRepository = Enterprise.Customs.Business.AddInfoFilterRepository;

namespace Enterprise.Customs.ZA.Module
{
	public class JobDeclarationFilterBusinessObject : Customs.Module.JobDeclarationFilterBusinessObject, Integration.Customs.ZA.IJobDeclarationFilterBusinessObject
	{
		public new JobDeclarationFilterLookups Lookups
		{
			get { return (JobDeclarationFilterLookups)base.Lookups; }
		}

		protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups()
		{
			return new JobDeclarationFilterLookups(this);
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();

			var serialNumberFilter = result.AddTextFilter(DeclarationFilterConstants.NumberFilterTypes.SerialNumber, GetSerialNumberFilter);
			serialNumberFilter.MaxLength = CusEntryHeaderSchema.CH_BGMReference.MaxLength;
			serialNumberFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|SerialNumber", DeclarationFilterConstants.NumberFilterTypes.SerialNumber);

			//VIN is a filter on an addoncolumn attached to an invoice line. Other comparisons are not clear on their meaning. For example.
			//What does 'is blank' mean?
			//Does it mean that a declaration exists with invoices and invoice lines and all the VIN on the invoice lines are blank
			//Does it mean that a declaration exists with invoices and no invoice lines and therefore VIN is blank
			//Does it mean that a declaration exists with no invoices  and therefore VIN is blank
			//Does it mean that a declaration exists with invoices and invoice lines and at least one of the invoice lines has an empty VIN
			var vINfilter = result.AddTextFilterForExactComparison(DeclarationFilterConstants.NumberFilterTypes.VIN, GetVINQuery);
			vINfilter.ComparisonOperator_List.AddPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains);
			vINfilter.ComparisonOperator_List.AddPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith);
			vINfilter.MaxLength = CusVehicle.Schema.CVH_VehicleIdentificationNumberMaxLength;
			vINfilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|VIN", DeclarationFilterConstants.NumberFilterTypes.VIN);

			var previousMRNFilter = new AddInfoModuleTextFilter(DeclarationFilterConstants.PreviousMRN, GetPreviousMRNQuery);
			previousMRNFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|PreviousMRN", DeclarationFilterConstants.PreviousMRN);
			result.AddFilter(previousMRNFilter);

			var procedureCodesFilter = new ProcedureCodesModuleFilter(DeclarationFilterConstants.CPCAndPPC, GetProcedureCodesQuery);
			procedureCodesFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|CPCAndPPC", DeclarationFilterConstants.CPCAndPPC);
			result.AddFilter(procedureCodesFilter);

			var entryInstructionSubGroup = new EntryInstructionCaseSubGroup();
			var caseNumberFilter = result.AddTextFilter(DeclarationFilterConstants.NumberFilterTypes.CaseNumber, GetCaseNumberFilter);
			caseNumberFilter.SubGroup = entryInstructionSubGroup;
			caseNumberFilter.Category = FilterCategories.NumbersAndReferences;
			caseNumberFilter.MaxLength = CusCodeDataSchema.CY_Data.MaxLength;
			caseNumberFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|CaseNumber", DeclarationFilterConstants.NumberFilterTypes.CaseNumber);

			var supportingDocStatusFilter = result.AddTextFilter(DeclarationFilterConstants.StatusFilterTypes.SupportingDocumentStatus, GetSupportingDocumentStatusFilter, DocumentStatusCodes);
			supportingDocStatusFilter.SubGroup = entryInstructionSubGroup;
			supportingDocStatusFilter.Category = FilterCategories.StatusAndFlags;
			supportingDocStatusFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|SupportingDocStatus", DeclarationFilterConstants.StatusFilterTypes.SupportingDocumentStatus);

			var entryNumberFilter = result.AddNumberFilter(DeclarationFilterConstants.UniqueConsignmentReference, GetCusEntryNumberUCRFilter);
			entryNumberFilter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;
			entryNumberFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|UCR", DeclarationFilterConstants.UniqueConsignmentReference);

			var printIndicatorfilter = result.AddTextFilter(DeclarationFilterConstants.StatusFilterTypes.ReleasePrinterIndicator, GetRelPrintQuery, ReleasePrintIndicatorList);
			printIndicatorfilter.Category = FilterCategories.StatusAndFlags;
			printIndicatorfilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|RelPrintInd", DeclarationFilterConstants.StatusFilterTypes.ReleasePrinterIndicator);

			return result;
		}

		#region SubGroups

		internal class EntryInstructionCaseSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
				var entryInstructionSubQuery = new ZDBOnlySubQuery(typeof(CusEntryInstruction), CusEntryInstructionSchema.CEI_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
				var caseNumberSubQuery = new ZDBOnlySubQuery(typeof(CaseNumber), CusCodeDataSchema.CY_ParentID);
				caseNumberSubQuery.AddToFilter(filter);
				entryInstructionSubQuery.AddSubQuery(caseNumberSubQuery, JoinCondition.And);
				declarationQuery.AddSubQuery(entryInstructionSubQuery, JoinCondition.And);

				return declarationQuery;
			}
		}

		#endregion

		#region Filters

		ZQuery GetCusEntryNumberUCRFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var isNegative = comparisonOperator.IsNegativeSQLOperator();
			if (comparisonOperator == SpecialComparisonOperator.IsBlank || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				isNegative = comparisonOperator == SpecialComparisonOperator.IsBlank;
			}

			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			var entryNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, isNegative);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.UniqueConsignementReference);

			var entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE, isNegative);
			var entryHeaderSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryHeaderSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.UniqueConsignementReference);

			comparisonOperator = comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery();
			if (comparisonOperator == SpecialComparisonOperator.IsBlank || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				entryNumberQuery.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "{0} <> ''", CusEntryNumSchema.CE_EntryNum.Name), null);
				entryHeaderSubQuery.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "{0} <> ''", CusEntryNumSchema.CE_EntryNum.Name), null);
			}
			else
			{
				entryNumberQuery.AddToFilter_PossiblyCommaSeparated(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);
				entryHeaderSubQuery.AddToFilter_PossiblyCommaSeparated(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);
			}

			entryHeaderQuery.AddSubQuery(entryHeaderSubQuery, JoinCondition.And);
			entryNumberQuery.AddAsUnionQuery(entryHeaderQuery);
			result.AddSubQuery(entryNumberQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetCaseNumberFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(CusCodeDataSchema.CY_Data, comparisonOperator, value);
		}

		ZQuery GetSupportingDocumentStatusFilter(ZString value)
		{
			var genAddonQuery = new ZDBOnlyQuery(typeof(Customs.Business.CusCodeData));
			var cusCodeDataSubQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			cusCodeDataSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, value);
			cusCodeDataSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, "ZA_DocumentStatus");
			genAddonQuery.AddSubQuery(cusCodeDataSubQuery, JoinCondition.And);
			return genAddonQuery;
		}

		ZQuery GetSerialNumberFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery joinDeclarationAndCusEntryHeader = new ZDBOnlyQuery(typeof(JobDeclaration));
			ZDBOnlySubQuery filterOnBGMReference = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
			filterOnBGMReference.AddToFilter(CusEntryHeaderSchema.CH_BGMReference, comparisonOperator, value);
			joinDeclarationAndCusEntryHeader.AddSubQuery(filterOnBGMReference, JoinCondition.And);

			return joinDeclarationAndCusEntryHeader;
		}

		ZQuery GetVINQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			var cusVehicleQuery = new ZDBOnlySubQuery(typeof(CusVehicle), CusVehicleSchema.CVH_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
			cusVehicleQuery.AddToFilter(ModelViewColumnHelper.GetModuleFilterQuery(CusVehicle.Schema.PK, CusVehicleSchema.Constants.PK, CusVehicleSchema.Constants.TableName, CusVehicleSchema.Constants.CVH_VehicleIdentificationNumber, filterOperator, value));
			declarationQuery.AddSubQuery(cusVehicleQuery, JoinCondition.And);
			return declarationQuery;
		}

		ZQuery GetPreviousMRNQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			var filterOnInstructionPreviousMRN = new ZDBOnlySubQuery(typeof(CusEntryInstruction), CusEntryInstructionSchema.CEI_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
			filterOnInstructionPreviousMRN.AddToFilter(AddInfoFilterRepository.GetAddInfoQuery(filterOperator, value, CusEntryInstructionSchema.CEI_AddInfo, ZACusEntryInstructionSchema.CEI_PreviousMRN.Name.Substring(4)));
			result.AddSubQuery(filterOnInstructionPreviousMRN, JoinCondition.And);
			return result;
		}

		ZQuery GetRelPrintQuery(ZString value)
		{
			if (!value.IsEmpty)
			{
				var result = new ZDBOnlyQuery(typeof(JobDeclaration));
				var filterOnCusEntryHeaderPrintInd = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
				filterOnCusEntryHeaderPrintInd.AddToFilter(Customs.Business.AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.Equal, value, CusEntryHeaderSchema.CH_AddInfo, ZACusEntryHeaderSchema.CH_RelPrintInd.Name.Substring(3)));
				result.AddSubQuery(filterOnCusEntryHeaderPrintInd, JoinCondition.And);
				return result;
			}
			else
			{
				return new ZQuery();
			}
		}

		ZQuery GetProcedureCodesQuery(ZString cpc, ZString ppc)
		{
			ZQuery result = null;
			if (!cpc.IsEmpty)
			{
				var queryWithCPC = new ZDBOnlyQuery(typeof(JobDeclaration));
				var instructionCPCSubQuery = new ZDBOnlySubQuery(typeof(CusEntryInstruction), CusEntryInstructionSchema.CEI_JE);
				instructionCPCSubQuery.AddToFilter(CusEntryInstructionSchema.CEI_Style, cpc);
				if (!ppc.IsEmpty)
				{
					var invoiceLinePPCSubQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceLine), JobComInvoiceLineSchema.JI_CEI);
					AddPpcSubstringAsParameterToZquery(invoiceLinePPCSubQuery, ppc);
					instructionCPCSubQuery.AddSubQuery(invoiceLinePPCSubQuery, JoinCondition.And);
				}
				queryWithCPC.AddSubQuery(instructionCPCSubQuery, JoinCondition.And);
				result = queryWithCPC;
			}
			else if (!ppc.IsEmpty)
			{
				var zq = new ZQuery();
				AddPpcSubstringAsParameterToZquery(zq, ppc);
				result = GetCommercialInvoiceQuery(zq);
			}
			return result;
		}

		public static void AddPpcSubstringAsParameterToZquery(ZQuery q, ZString ppc)
		{
			q.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.CurrentCulture, "SUBSTRING({0}, 3, 2)=@ppc", JobComInvoiceLineSchema.JI_Procedure.Name), new ZSqlParameterCollection(ZSqlParameter.New("@ppc", ppc, JobComInvoiceLineSchema.JI_Procedure)));
		}

		public GenAddOnColumnQueryHelper InvoiceLineQueryHelper
		{
			get
			{
				if (invoiceLineQueryHelper == null)
				{
					var linkList = new List<GenAddOnColumnQueryHelper.ForeignKeyLink>
					{
						new GenAddOnColumnQueryHelper.ForeignKeyLink(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE),
						new GenAddOnColumnQueryHelper.ForeignKeyLink(typeof(JobComInvoiceLine), JobComInvoiceLineSchema.JI_JZ)
					};

					invoiceLineQueryHelper = new GenAddOnColumnQueryHelper(typeof(JobDeclaration), linkList);
				}

				return invoiceLineQueryHelper;
			}
		}
		GenAddOnColumnQueryHelper invoiceLineQueryHelper;

		Customs.Business.ModelViewColumnQueryHelper<JobComInvoiceLine> ModelViewColumnHelper
		{
			get
			{
				return modelViewColumnHelper ?? (modelViewColumnHelper = new Customs.Business.ModelViewColumnQueryHelper<JobComInvoiceLine>());
			}
		}
		Customs.Business.ModelViewColumnQueryHelper<JobComInvoiceLine> modelViewColumnHelper;

		#endregion

		CodeDescriptionPairList DocumentStatusCodes => Factory.GetCachedValue<DocumentStatusCodes>();
		protected CodeDescriptionPairList ReleasePrintIndicatorList => Factory.GetCachedValue<CustomsPrintIndicatorList>();
	}
}
