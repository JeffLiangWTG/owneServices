using System.Collections;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class AdditionalInfoLookups(AdditionalInfo parent) : EU.ExitControl.Business.AdditionalInfoLookups(parent)
{
	public override CodeDescriptionPairList SubTypeList
	{
		get
		{
			if (Parent.Parent is CusExitReport cusExitReport && !cusExitReport.CER_Calc_Discrepancies)
			{
				return Factory.GetCachedValue("PL.ExitControl.AdditionalInfoLookups.SubTypeList.NoDiscrepancies", () =>
				{
					var result = new CodeDescriptionPairList();
					foreach (var codeDescription in new AdditionalInfoSubTypeList().Cast<CodeDescriptionPair>().Where
						(x => x.Code == AdditionalInfoSubTypeList.Codes.AdditionalInformation))
					{
						result.Add(codeDescription);
					}
					return result;
				});
			}
			else
			{
				return Factory.GetCachedValue("PL.ExitControl.AdditionalInfoLookups.SubTypeList.Discrepancies", () =>
				{
					var result = new CodeDescriptionPairList();
					foreach (var codeDescription in new AdditionalInfoSubTypeList().Cast<CodeDescriptionPair>().Where
						(x => x.Code == AdditionalInfoSubTypeList.Codes.AdditionalInformation || x.Code == AdditionalInfoSubTypeList.Codes.TransportDocument))
					{
						result.Add(codeDescription);
					}

					return result;
				});
			}
		}
	}

	public override ICollection CodeList =>
		Parent.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation
			? ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Poland, UniversalReferenceConstants.RefCusCodeListType.Code.ExportAdditionalInformation, ZDate.Today)
			: base.CodeList;
}
