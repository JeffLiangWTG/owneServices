using System.Collections;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.Declaration;

public class JobComInvoiceLineLookups : EU.Business.Declaration.JobComInvoiceLineLookups
{
	public JobComInvoiceLineLookups(JobComInvoiceLine parent)
		: base(parent)
	{
	}

	public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

	protected new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

	public override RefCusProcedureCollection CPCList
	{
		get
		{
			var procedureCode = Parent.EntryInstruction?.CEI_Procedure ?? ZString.Empty;
			var messageType = Parent.Declaration?.JE_MessageType ?? ZString.Empty;
			return !procedureCode.IsEmpty
				? RefCusProcedureCollection.LoadPreviousProceduresCodesForCountryShipmentTypeProcedureCode(Factory, GetDefaultDataGroupingCode(), messageType, procedureCode, GetDateOfValuation())
				: base.CPCList;
		}
	}

	public ZZRefCusCodeListCombinedCollection PLMarkModelCollection
	{
		get
		{
			var result = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, GetDefaultDataGroupingCode(), UniversalReferenceConstants.RefCusCodeListType.Codes.CarMarkModel, ZDate.Today);
			AddFilterBusinessObjectDefault(new FilterBusinessObjectDefault(Customs.Universal.Constants.ZZRefCusCodeListFilters.Description, "Property", InvoiceLine.JI_MarkModel));
			AddFilterBusinessObjectDefault(new FilterBusinessObjectDefault(Customs.Universal.Constants.ZZRefCusCodeListFilters.Code, "Property", InvoiceLine.MarkModelCode));
			return result;

			void AddFilterBusinessObjectDefault(FilterBusinessObjectDefault filterBusinessObjectDefault)
			{
				if (!filterBusinessObjectDefault.Value.IsEmpty)
				{
					result.FilterBusinessObjectDefaults.Add(filterBusinessObjectDefault);
				}
				else
				{
					if (result.FilterBusinessObjectDefaults.ContainsDefaultFor(filterBusinessObjectDefault.Key))
					{
						result.FilterBusinessObjectDefaults.Remove(filterBusinessObjectDefault.Key);
					}
				}
			}
		}
	}

	public CodeDescriptionPairList RequestedCustomsProcedureCodes
	{
		get
		{
			var result = new CodeDescriptionPairList();
			if (Parent.Declaration is JobDeclaration jobDeclaration)
			{
				var dataGroupingCode = jobDeclaration.GetDefaultDataGroupingCode();
				var dateOfValuation = GetDateOfValuation();
				var shipmentType = jobDeclaration.JE_MessageType;
				result = Factory.GetCachedValue(string.Join("|", "PL.JobComInvoiceLineLookups.RequestedCustomsProcedureCodes", string.Join("_", dataGroupingCode, dateOfValuation, shipmentType)), () =>
				{
					var list = new CodeDescriptionPairList();
					var procedures = PLRefCusProcedureCollection.LoadCustomsProcedureCodesForPoland(Factory, dataGroupingCode, shipmentType, dateOfValuation).DistinctBy(p => p.ZZ6_ProcedureCode).OrderBy(p => p.ZZ6_ProcedureCode);
					var codeList = ZZRefCusCodeListCombined.Loader.Load(Factory, dataGroupingCode, UniversalReferenceConstants.RefCusCodeListType.Codes.CustomsProcedures, dateOfValuation);
					foreach (var procedure in procedures)
					{
						var procedureCode = procedure.ZZ6_ProcedureCode;
						var codeListElement = codeList.FirstOrDefault(x => x.ZZD_Code == procedureCode);
						list.AddPair(procedureCode, codeListElement?.ZZD_Description);
					}
					list.Sort();
					return list;
				});
			}
			return result;
		}
	}

	public CodeDescriptionPairList PreviousCustomsProcedureCodes
	{
		get
		{
			var result = new CodeDescriptionPairList();
			var procedureCode = Parent.ProcedureCodeBase;
			if (!procedureCode.IsEmpty && Parent.Declaration is JobDeclaration jobDeclaration)
			{
				var dataGroupingCode = jobDeclaration.GetDefaultDataGroupingCode();
				var dateOfValuation = GetDateOfValuation();
				var shipmentType = jobDeclaration.JE_MessageType;
				result = Factory.GetCachedValue(string.Join("|", "PL.JobComInvoiceLineLookups.PreviousCustomsProcedureCodes", string.Join("_", dataGroupingCode, dateOfValuation, shipmentType, procedureCode)), () =>
				{
					var list = new CodeDescriptionPairList();
					var procedures = RefCusProcedureCollection.LoadPreviousProceduresCodesForCountryShipmentTypeProcedureCode(Factory, dataGroupingCode, shipmentType, procedureCode, dateOfValuation, true);
					var codeList = ZZRefCusCodeListCombined.Loader.Load(Factory, dataGroupingCode, UniversalReferenceConstants.RefCusCodeListType.Codes.CustomsProcedures, dateOfValuation);
					foreach (var procedure in procedures)
					{
						var previousProcedureCode = procedure.ZZ6_PreviousProcedureCode;
						var codeListElement = codeList.FirstOrDefault(x => x.ZZD_Code == previousProcedureCode);
						list.AddPair(previousProcedureCode, codeListElement?.ZZD_Description);
					}
					list.Sort();
					return list;
				});
			}
			return result;
		}
	}

	public CodeDescriptionPairList ConcessionCodes
	{
		get
		{
			var result = new CodeDescriptionPairList();
			var procedureCode = Parent.ProcedureCodeBase;
			var previousProcedureCode = Parent.PreviousProcedureCode;
			if (!procedureCode.IsEmpty && !previousProcedureCode.IsEmpty && Parent.Declaration is JobDeclaration jobDeclaration)
			{
				var dataGroupingCode = jobDeclaration.GetDefaultDataGroupingCode();
				var dateOfValuation = GetDateOfValuation();
				var shipmentType = jobDeclaration.JE_MessageType;
				result = Factory.GetCachedValue(string.Join("|", "PL.JobComInvoiceLineLookups.ConcessionCodes", string.Join("_", dataGroupingCode, dateOfValuation, shipmentType, procedureCode, previousProcedureCode)), () =>
				{
					var list = new CodeDescriptionPairList();
					var procedures = RefCusProcedureCollection.LoadConcessionsForCountryShipmentTypeProcedureCodePreviousProceduresCode(Factory, dataGroupingCode, shipmentType, procedureCode, previousProcedureCode, dateOfValuation);
					var codeList = ZZRefCusCodeListCombined.Loader.Load(Factory, dataGroupingCode, UniversalReferenceConstants.RefCusCodeListType.Codes.CustomsConcessions, dateOfValuation);
					foreach (var procedure in procedures)
					{
						var concession = procedure.ZZ6_Concession;
						var codeListElement = codeList.FirstOrDefault(x => x.ZZD_Code == concession);
						list.AddPair(concession, codeListElement?.ZZD_Description);
					}
					list.Sort();
					return list;
				});
			}
			return result;
		}
	}

	protected override ZDateTime GetDateOfValuation() => Parent.EffectiveAssessmentDate;

	protected override ICollection CountryOfOriginsCore() => new RefCountryCollection(Factory);

	protected override ZString CustomsUQListType => Parent.IsExport
		? UniversalReferenceConstants.RefCusCodeListType.Codes.ExportCustomsDeclarationUnitsOfQuantity
		: Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ;
}
