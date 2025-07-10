using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	[Flags]
	public enum ReconIssues
	{
		None = 0, //Recon Issue is empty
		_98 = 1,
		CL = 2,
		VL = 4,
		NA = 8
	}

	public static class ReconIssueCalculator
	{
		public static void RecalculateIndicators(JobDeclaration declaration)
		{
			if (declaration.IsFormalImport && !declaration.US_FixRecon)
			{
				var otherReconIndicator = GetReconIssueCodeCalculated(declaration);
				if (!otherReconIndicator.IsEmpty && declaration.US_OtherReconIndicator != otherReconIndicator)
				{
					declaration.US_OtherReconIndicator = otherReconIndicator;
				}

				var naftaReconIndicator = GetNAFTACalculated(declaration);
				if (!naftaReconIndicator.IsEmpty && declaration.US_NAFTAReconIndicator != naftaReconIndicator)
				{
					declaration.US_NAFTAReconIndicator = naftaReconIndicator;
				}
			}
		}

		public static ZString GetReconIssueCodeCalculated(JobDeclaration declaration)
		{
			var reconModesToCheckAgainst = GetListOfIssuesToCheckAgainst(declaration);

			var all = ReconIssues._98 | ReconIssues.CL | ReconIssues.VL;
			var aggregatedReconIssues = ReconIssues.None;
			var result = string.Empty;
			var notApplicableSpecified = false;

			foreach (ReconIssues issueMode in reconModesToCheckAgainst)
			{
				if (issueMode == ReconIssues.NA)
				{
					notApplicableSpecified = true;
				}
				else
				{
					aggregatedReconIssues |= issueMode;

					if (aggregatedReconIssues == all)
					{
						break;
					}
				}
			}

			if (notApplicableSpecified && aggregatedReconIssues == ReconIssues.None && EntryTypeList.IsValidForRecon(declaration.US_EntryType))
			{
				result = ReconIssueCodeList.Codes.NotApplicable;
			}
			else
			{
				result = GetReconIssueCode(aggregatedReconIssues);
			}

			return result;
		}

		static string GetReconIssueCode(ReconIssues value)
		{
			switch ((int)value)
			{
				case 1:
					return ReconIssueCodeList.Codes._9802Recon;
				case 2:
					return ReconIssueCodeList.Codes.ClassRecon;
				case 3:
					return ReconIssueCodeList.Codes.Class9802Recon;
				case 4:
					return ReconIssueCodeList.Codes.ValueRecon;
				case 5:
					return ReconIssueCodeList.Codes.Value9802Recon;
				case 6:
					return ReconIssueCodeList.Codes.ValueClassRecon;
				case 7:
					return ReconIssueCodeList.Codes.ValueClass9802Recon;
				default:
					return "";
			}
		}
		public static ZBool GetNAFTACalculated(JobDeclaration declaration)
		{
			var indicatorsToCheckAgainst = GetNAFTAIndicToCheckAgainst(declaration);
			var result = false;
			foreach (var indicator in indicatorsToCheckAgainst)
			{
				result |= indicator;
				if (result)
				{
					break;
				}
			}
			return result;
		}

		internal static IEnumerable<ReconIssues> GetListOfIssuesToCheckAgainst(JobDeclaration declaration)
		{
			var reconIssueFromSupplierImporterLink = ReconIssues.None;
			var link = declaration.SupplierImporterLink;
			if (link != null)
			{
				reconIssueFromSupplierImporterLink = link.GetReconIssueCalculated();
			}

			if (reconIssueFromSupplierImporterLink == ReconIssues.None)
			{
				if (declaration.IORWrapper != null)
				{
					yield return declaration.IORWrapper.GetReconIssueCalculated();
				}
			}
			else
			{
				yield return reconIssueFromSupplierImporterLink;
			}

			foreach (var issue in declaration.Invoices.ReconIssues)
			{
				yield return issue;
			}

			foreach (var issue in declaration.InvoiceLines.ReconIssues)
			{
				yield return issue;
			}
		}

		internal static IEnumerable<ZBool> GetNAFTAIndicToCheckAgainst(JobDeclaration declaration)
		{
			var link = declaration.SupplierImporterLink;
			if (link != null)
			{
				var addInfo = link.GetAddInfo();
				if (addInfo != null)
				{
					yield return addInfo.ZO_NAFTAReconIndicator;
				}
			}

			if (declaration.IORWrapper != null)
			{
				yield return declaration.IORWrapper.ZO_NAFTAReconIndicator;
			}

			foreach (var indicator in declaration.Invoices.ReconNAFTAs)
			{
				yield return indicator;
			}
			foreach (var indicator in declaration.InvoiceLines.ReconNAFTAs)
			{
				yield return indicator;
			}
		}
	}
}
