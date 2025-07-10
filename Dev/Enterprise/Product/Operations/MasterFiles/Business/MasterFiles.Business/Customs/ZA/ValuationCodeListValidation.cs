using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Customs.ZA
{
	public static class ValuationCodeListValidation
	{
		public static void ValidateIndicator(ZString relatedInd, ZString valuationCode, ZPropertyInfo errorInfo)
		{
			if ((relatedInd == RelatedIndicatorList.Codes.Yes || relatedInd == RelatedIndicatorList.Codes.No) && valuationCode.IsEmpty)
			{
				errorInfo.AddMessageError(ValuationCodeRequired);
			}
			else if (relatedInd == RelatedIndicatorList.Codes.Exempt && !valuationCode.IsEmpty)
			{
				errorInfo.AddMessageError(ValuationCodeIsNowAllowed);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const message strings")]
		public const string ValuationCodeIsNowAllowed = @"A valuation code is not allowed when relationship indicator is E (Exempt).";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const message strings")]
		public const string ValuationCodeRequired = @"A valuation code is required when relationship indicator is not E (Exempt).";
	}
}
