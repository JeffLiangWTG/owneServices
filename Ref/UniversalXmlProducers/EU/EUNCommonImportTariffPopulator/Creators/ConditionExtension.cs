using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public static class ConditionExtension
	{
		public static bool ShouldAllow(this measureCondition condition)
		{
			switch (condition.actionCode)
			{
				case "04":
				case "09":
				case "10":
				case "16":
					return false;
				case "24":
				case "28":
				case "29":
				case "36":
					return true;
				default:
					throw new InvalidOperationException($"Unknown actionCode :{condition.actionCode} for conditionCode {condition.conditionCodeId}");
			}
		}

		public static bool WithDutyAmount(this measureCondition condition)
		{
			return condition.dutyAmountSpecified && condition.dutyAmount > 0 && !string.IsNullOrEmpty(condition.measurementUnitCode);
		}

		public static bool WithCertificate(this measureCondition condition)
		{
			return !string.IsNullOrEmpty(condition.certificateCode) && !string.IsNullOrEmpty(condition.certificateType);
		}

		public static bool CertificateOfSUP(this measureCondition condition)
		{
			Argument.NotNullOrEmpty(condition.certificateCode, nameof(condition.certificateCode));
			Argument.NotNullOrEmpty(condition.certificateType, nameof(condition.certificateType));
			if (CertificateTypeArrayOfSUP.Contains(condition.certificateType))
			{
				return true;
			}
			if (condition.certificateType == "L" && condition.certificateCode != "136")
			{
				return true;
			}
			if (condition.certificateType == "N" && !CertificateCodeExceptionArrayOfSUPN.Contains(condition.certificateCode))
			{
				return true;
			}
			if (condition.certificateType == "Y" && CertificateCodeArrayOfSUPY.Contains(condition.certificateCode))
			{
				return true;
			}
			return false;
		}

		static string[] CertificateTypeArrayOfSUP = { "A", "B", "C", "E", "H", "Q", "Z" };
		static string[] CertificateCodeExceptionArrayOfSUPN = { "235", "271", "325", "380", "750", "787", "934", "935", "864" };
		static string[] CertificateCodeArrayOfSUPY = { "022", "023", "024", "025", "026", "027", "028", "029", "031", "040", "041", "042", "915", "919" };
	}
}
