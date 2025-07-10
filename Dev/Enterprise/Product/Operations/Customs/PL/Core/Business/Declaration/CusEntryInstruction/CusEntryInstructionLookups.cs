using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.Declaration;

public class CusEntryInstructionLookups : EU.Business.Declaration.CusEntryInstructionLookups
{
	public CusEntryInstructionLookups(CusEntryInstruction cusEntryInstruction)
		: base(cusEntryInstruction)
	{
	}

	public CodeDescriptionPairList EadPrintOutList => Factory.GetCachedValue<EadPrintOutList>();

	public CodeDescriptionPairList TemporaryLocationCodeTypeList => Factory.GetCachedValue<TemporaryLocationCodeTypeList>();

	public CodeDescriptionPairList CPCList
	{
		get
		{
			var result = new CodeDescriptionPairList();
			if (Parent.JobDeclaration is JobDeclaration jobDeclaration)
			{
				var dataGroupingCode = jobDeclaration.GetDefaultDataGroupingCode();
				var dateOfValuation = jobDeclaration.DateOfValuation;
				var messageType = jobDeclaration.JE_MessageType;
				var shipmentType = GetShipmentType();
				result = Factory.GetCachedValue(string.Join("|", "PL.CusEntryInstructionLookups.CPCList", string.Join("_", dataGroupingCode, dateOfValuation, messageType)), () =>
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

	protected override CodeDescriptionPairList GetEntrySubstyleList(ICanBeImportOrExport parent)
	{
		var entrySubStyleList = Factory.GetCachedValue<EntrySubStyleList>();
		entrySubStyleList.Sort();
		return entrySubStyleList;
	}
}
