using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Module;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Module
{
	public class EntryHeaderFilterBusinessObject : Customs.Module.EntryHeaderFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();

			var procedureCodesFilter = new ProcedureCodesModuleFilter(DeclarationFilterConstants.CPCAndPPC, GetProcedureCodesQuery);
			procedureCodesFilter.SubGroup = new ProcedureCodeSubGroup();
			procedureCodesFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|ProcedureCodes", DeclarationFilterConstants.CPCAndPPC);
			result.AddFilter(procedureCodesFilter);

			var entryNumberFilter = result.AddNumberFilter(DeclarationFilterConstants.UniqueConsignmentReference, GetUCRNumberQuery);
			entryNumberFilter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;
			entryNumberFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|UCRNumber", DeclarationFilterConstants.UniqueConsignmentReference);

			var printIndicatorfilter = result.AddTextFilter(DeclarationFilterConstants.StatusFilterTypes.ReleasePrinterIndicator, GetRelPrintQuery, ReleasePrintIndicatorList);
			printIndicatorfilter.Category = FilterCategories.StatusAndFlags;
			printIndicatorfilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|RelPrintInd", DeclarationFilterConstants.StatusFilterTypes.ReleasePrinterIndicator);

			var baseEntryNumberFilter = result[Constants.EntryNumber];
			baseEntryNumberFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|EntryNumberMRN", "Entry Number (MRN)");

			var referenceNumberFilter = result[Constants.ReferenceNumber];
			referenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|ReferenceNumberLRN", "Reference Number (LRN)");

			result.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.AcquitByDate, CusEntryHeaderSchema.CH_BondValidToDate).MultilingualDescription = ResString.GetMultilingualString("ZA|EntryHeaderFilter|BondValidToDate", DeclarationFilterConstants.DateFilterTypes.AcquitByDate);
			result.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.AcquittedDate, CusEntryHeaderSchema.CH_BondAcquittedDate).MultilingualDescription = ResString.GetMultilingualString("ZA|EntryHeaderFilter|AcquittedDate", DeclarationFilterConstants.DateFilterTypes.AcquittedDate);

			return result;
		}

		#region Filters

		ZQuery GetUCRNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var entryNumberQuery = GetEntryNumberQueryOfParentTableAndEntryTypeForEntryNum(comparisonOperator, value, CusEntryHeaderSchema.Constants.TableName, CusEntryNumberTypes.Standard.UniqueConsignementReference, Core.Constants.CountryCodes.SouthAfrica);

			var result = new ZDBOnlyQuery(typeof(CusEntryHeader));
			result.AddSubQuery(entryNumberQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetProcedureCodesQuery(ZString cpc, ZString ppc)
		{
			var result = new ZQuery();
			if (!cpc.IsEmpty || !ppc.IsEmpty)
			{
				if (!cpc.IsEmpty)
				{
					result.AddToFilter(CusEntryInstructionSchema.CEI_Style, cpc);
				}
				if (!ppc.IsEmpty)
				{
					JobDeclarationFilterBusinessObject.AddPpcSubstringAsParameterToZquery(result, ppc);
				}
			}
			return result;
		}

		ZQuery GetRelPrintQuery(ZString value)
		{
			var result = new ZQuery();

			if (!value.IsEmpty)
			{
				result = Customs.Business.AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.Equal, value, CusEntryHeaderSchema.CH_AddInfo, ZACusEntryHeaderSchema.CH_RelPrintInd.Name.Substring(3));
			}

			return result;
		}
		#endregion

		protected override EntryHeaderFilterLookups GetNewLookups() => new ZAFilterLookups(this);

		protected CodeDescriptionPairList ReleasePrintIndicatorList => Factory.GetCachedValue<CustomsPrintIndicatorList>();
	}

	class ZAFilterLookups : EntryHeaderFilterLookups
	{
		public ZAFilterLookups(EntryHeaderFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{ }

		protected override CodeDescriptionPairList GetMainMessageStatusList() => Factory.GetCachedValue<Common.ZA.ZAMessageStatusList>();
	}

	class ProcedureCodeSubGroup : ModuleFilterSubGroup
	{
		public override ZQuery GetSubQuery(ZQuery filter)
		{
			var parts = filter.GetCompositeParts();

			var cpc = parts.FirstOrDefault(x => x.Params.Any(y => y.SchemaColumn.Name == CusEntryInstruction.Schema.CEI_Style));
			var ppc = parts.FirstOrDefault(x => x.Params.Any(y => y.SchemaColumn.Name == JobComInvoiceLine.Schema.JI_Procedure));

			var result = new ZQuery();
			if (cpc != null || ppc != null)
			{
				var queryWithCPC = new ZDBOnlyQuery(typeof(CusEntryHeader));
				var instructionCPCSubQuery = new ZDBOnlySubQuery(typeof(CusEntryInstruction), CusEntryHeaderSchema.CH_CEI_Instruction);
				if (cpc != null && ppc == null)
				{
					instructionCPCSubQuery.AddToFilter(cpc);
				}
				if (ppc != null)
				{
					var invoiceLinePPCSubQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceLine), JobComInvoiceLineSchema.JI_CEI);
					invoiceLinePPCSubQuery.AddToFilter(ppc);
					instructionCPCSubQuery.AddSubQuery(invoiceLinePPCSubQuery, JoinCondition.And);
				}
				queryWithCPC.AddSubQuery(instructionCPCSubQuery, JoinCondition.And);
				result = queryWithCPC;
			}
			return result;
		}
	}
}
