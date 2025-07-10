using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.Universal.ZZCustomsFunctionalityEffectiveDate;

namespace Enterprise.Customs.ZA.Business.Business.Utilities
{
	public static class TDTSegmentSplitHelper
	{
		public static ZBool IsTDTSegementSplitApplicable(JobDeclaration declaration)
		{
			return declaration != null
					&& (declaration.JE_MessageType == JobMessageTypeList.Codes.Import || declaration.JE_MessageType == JobMessageTypeList.Codes.Export)
					&& declaration.IsSea
					&& IsFunctionalityValid(Customs.Universal.Constants.FunctionalityTypes.TDTSegmentSplit, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today);
		}

#if DEBUG
		public static IDisposable TemporarilyEnableTDTSegmentSplit(bool value)
		{
			return ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.TDTSegmentSplit
				, Core.Constants.CountryCodes.SouthAfrica
				, ZDateTime.Today
				, value);
		}
#endif
	}
}
