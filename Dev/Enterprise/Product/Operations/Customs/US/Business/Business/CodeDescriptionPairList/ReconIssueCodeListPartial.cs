
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	partial class ReconIssueCodeList : CodeDescriptionPairList,
		Integration.Customs.US.IReconIssueCodeDescriptionPairProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		#region ICodeDescriptionPairListProvider Members

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return this;
		}

		#endregion

		public static string ConvertToENSOtherIssueCode(string reconCode)
		{
			switch (reconCode)
			{
				case Codes.ValueRecon:
					return "001";
				case Codes.ClassRecon:
					return "002";
				case Codes._9802Recon:
					return "003";
				case Codes.ValueClassRecon:
					return "004";
				case Codes.Value9802Recon:
					return "005";
				case Codes.Class9802Recon:
					return "006";
				case Codes.ValueClass9802Recon:
					return "007";
				case Codes.NotApplicable:
					return ""; // NA still should not cause value to be put in message
				default:
					return "";
			}
		}

		public static string ConvertFromENSIssueCode(string ensIssueCode)
		{
			switch (ensIssueCode)
			{
				case "001":
					return Codes.ValueRecon;
				case "002":
					return Codes.ClassRecon;
				case "003":
					return Codes._9802Recon;
				case "004":
					return Codes.ValueClassRecon;
				case "005":
					return Codes.Value9802Recon;
				case "006":
					return Codes.Class9802Recon;
				case "007":
					return Codes.ValueClass9802Recon;
				case "008":
					return Codes.NotApplicable;
				default:
					return "";
			}
		}

		public static bool IsValidForConsumptionQuotaVisaEntryType(string reconReasonCode)
		{
			return reconReasonCode == ReconIssueCodeList.Codes.ValueRecon ||
					reconReasonCode == ReconIssueCodeList.Codes.Value9802Recon ||
					reconReasonCode == ReconIssueCodeList.Codes._9802Recon ||
					reconReasonCode == ReconIssueCodeList.Codes.NotApplicable;
		}

		public static ReconIssues GetReconIssuesValue(string code)
		{
			ReconIssues result = ReconIssues.None;

			if (code == ReconIssueCodeList.Codes._9802Recon
					|| code == ReconIssueCodeList.Codes.Class9802Recon
					|| code == ReconIssueCodeList.Codes.Value9802Recon
					|| code == ReconIssueCodeList.Codes.ValueClass9802Recon)
			{
				result = ReconIssues._98;
			}

			if (code == ReconIssueCodeList.Codes.ClassRecon
					|| code == ReconIssueCodeList.Codes.Class9802Recon
					|| code == ReconIssueCodeList.Codes.ValueClass9802Recon
					|| code == ReconIssueCodeList.Codes.ValueClassRecon)
			{
				result |= ReconIssues.CL;
			}

			if (code == ReconIssueCodeList.Codes.ValueRecon
					|| code == ReconIssueCodeList.Codes.Value9802Recon
					|| code == ReconIssueCodeList.Codes.ValueClass9802Recon
					|| code == ReconIssueCodeList.Codes.ValueClassRecon)
			{
				result |= ReconIssues.VL;
			}

			if (code == ReconIssueCodeList.Codes.NotApplicable)
			{
				result = ReconIssues.NA;
			}

			return result;
		}

		public static bool IsClassificationRecon(string reconReasonCode)
		{
			return reconReasonCode == ReconIssueCodeList.Codes.ClassRecon ||
					reconReasonCode == ReconIssueCodeList.Codes.Class9802Recon ||
					reconReasonCode == ReconIssueCodeList.Codes.ValueClassRecon ||
					reconReasonCode == ReconIssueCodeList.Codes.ValueClass9802Recon;
		}

		public static bool IsValueAndClassification(string reconReasonCode)
		{
			return reconReasonCode == ReconIssueCodeList.Codes.ValueClassRecon ||
					reconReasonCode == ReconIssueCodeList.Codes.ValueClass9802Recon;
		}

		public static bool NeedSendSPI(string reconReasonCode)
		{
			return reconReasonCode == ReconIssueCodeList.Codes.FTA;//8
		}

		public static bool NeedSendTariff(string reconReasonCode, bool hTSChangedDueToValue)
		{
			return
				(reconReasonCode == ReconIssueCodeList.Codes.ValueRecon && hTSChangedDueToValue) || //1
				reconReasonCode == ReconIssueCodeList.Codes.ClassRecon || //2
				reconReasonCode == ReconIssueCodeList.Codes.ValueClassRecon || //4
				(reconReasonCode == ReconIssueCodeList.Codes.Value9802Recon && hTSChangedDueToValue) || //5
				reconReasonCode == ReconIssueCodeList.Codes.Class9802Recon || //6
				reconReasonCode == ReconIssueCodeList.Codes.ValueClass9802Recon //7
					;
		}

		public static bool NeedSendHTSChangedDueToValue(string reconReasonCode)
		{
			return reconReasonCode == ReconIssueCodeList.Codes.ValueRecon || //1
						reconReasonCode == ReconIssueCodeList.Codes.ValueClassRecon || //4
						reconReasonCode == ReconIssueCodeList.Codes.Value9802Recon || //5
						reconReasonCode == ReconIssueCodeList.Codes.ValueClass9802Recon;//7
		}

		public static bool ExpectCustomsValueChanged(string reconReasonCode)
		{
			return reconReasonCode != ReconIssueCodeList.Codes.ClassRecon && //2
					reconReasonCode != ReconIssueCodeList.Codes.FTA;//8
		}

		public static bool IsAllowedChangeCustomsValues(string reconReasonCode)
		{
			return NeedSendHTSChangedDueToValue(reconReasonCode);
		}
	}
}
