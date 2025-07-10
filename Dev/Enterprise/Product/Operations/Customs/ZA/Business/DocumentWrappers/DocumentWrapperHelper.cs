using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	public static class DocumentWrapperHelper
	{
		public static ZString TranslateDigitToLongHand(char charDigit)
		{
			var result = ZString.Empty;

			switch (charDigit)
			{
				case '0':
					result = "ZERO";
					break;
				case '1':
					result = "ONE";
					break;
				case '2':
					result = "TWO";
					break;
				case '3':
					result = "THREE";
					break;
				case '4':
					result = "FOUR";
					break;
				case '5':
					result = "FIVE";
					break;
				case '6':
					result = "SIX";
					break;
				case '7':
					result = "SEVEN";
					break;
				case '8':
					result = "EIGHT";
					break;
				case '9':
					result = "NINE";
					break;
				default:
					break;
			}
			return result;
		}

		public static ZString FormatTariffCode(ZString input)
		{
			var result = input;
			if (input.Length == 9)
			{
				result = ZString.Format("{0}.{1}.{2} ({3})", input.SubstringSafe(0, 4), input.SubstringSafe(4, 2), input.SubstringSafe(6, 2), input.SubstringSafe(8, 1));
			}
			else if (input.Length == 8)
			{
				result = ZString.Format("{0}.{1}.{2}", input.SubstringSafe(0, 4), input.SubstringSafe(4, 2), input.SubstringSafe(6, 2));
			}
			else if (input.Length == 6)
			{
				result = ZString.Format("{0}.{1}", input.SubstringSafe(0, 4), input.SubstringSafe(4, 2));
			}
			else if (input.Length == 4)
			{
				result = ZString.Format("{0}", input);
			}
			return result;
		}

		public static ZString FormatNonDutyTariffCode(ZString input)
		{
			var result = input;
			if (input.Length == 11)
			{
				result = ZString.Format("{0}.{1}.{2}.{3}.{4}", input.SubstringSafe(0, 3), input.SubstringSafe(3, 2), input.SubstringSafe(5, 2), input.SubstringSafe(7, 2), input.SubstringSafe(9, 3));
			}
			else if (input.Length == 9)
			{
				result = ZString.Format("{0}.{1}.{2}.{3}", input.SubstringSafe(0, 3), input.SubstringSafe(3, 2), input.SubstringSafe(5, 2), input.SubstringSafe(7, 2));
			}
			else if (input.Length == 7)
			{
				result = ZString.Format("{0}.{1}.{2}", input.SubstringSafe(0, 3), input.SubstringSafe(3, 2), input.SubstringSafe(5, 2));
			}
			return result;
		}

		public static CusEntryLine FindMatchingEntryLine(CusEntryHeader entryHeader, ILineLevelInformation messageLine)
		{
			if (entryHeader == null || messageLine == null)
			{
				return null;
			}

			ZString lineNumber = messageLine.LineNumber;
			ZString tariffCode = messageLine.TariffCode;

			return (CusEntryLine)entryHeader.MergedLines?.Cast<ILineLevelInformation>()?.FirstOrDefault(el => el.LineNumber == lineNumber && el.TariffCode == tariffCode);
		}

		public static IAddressInformation GetFallbackOrgAddressInfo(BusinessObjectFactory factory, IAddressInformation source, OrgHeader currentOrgHeader, string codeType)
		{
			if (codeType == null)
			{
				throw new ArgumentNullException(nameof(codeType), "Code type is required for selecting correct organization.");
			}

			if (source == null || source.OrganizationCode.IsEmpty || IsMatchingOrgCode(source, currentOrgHeader, codeType))
			{
				return OrgHeaderAddressInformationWrapper.Wrap(currentOrgHeader, codeType);
			}

			var org = OrgHeader.FindByOrgCusCode(factory, codeType, source.OrganizationCode, Core.Constants.CountryCodes.SouthAfrica);
			if (org != null)
			{
				return OrgHeaderAddressInformationWrapper.Wrap(org, codeType);
			}

			return null;
		}

		static bool IsMatchingOrgCode(IAddressInformation source, OrgHeader currentOrgHeader, string codeType)
		{
			if (source == null || source.OrganizationCode.IsEmpty)
			{
				return false;
			}
			if (currentOrgHeader == null)
			{
				return false;
			}
			ZString currentOrgCode = currentOrgHeader.CustomsCodes?.GetCustomsRegNo(codeType, Core.Constants.CountryCodes.SouthAfrica) ?? ZString.Empty;
			return currentOrgCode == source.OrganizationCode;
		}

		public static ZString GetUnregisteredTraderPrefixedNumber(IAddressInformation unregisteredTrader)
		{
			var result = ZString.Empty;

			if (unregisteredTrader != null && !unregisteredTrader.OrganizationCodeQualifier.IsEmpty && !unregisteredTrader.OrganizationCode.IsEmpty)
			{
				var prefix = ZString.Empty;

				if (unregisteredTrader.OrganizationCodeQualifier == Edifact.D96B.Elements.CodeListQualifierList.CitizenIdentification.ToString())
				{
					prefix = "IDN";
				}
				else if (unregisteredTrader.OrganizationCodeQualifier == Edifact.D96B.Elements.CodeListQualifierList.TaxPartyIdentification.ToString())
				{
					prefix = "TAX";
				}
				else if (unregisteredTrader.OrganizationCodeQualifier == Edifact.D96B.Elements.CodeListQualifierList.PassportNumber.ToString())
				{
					prefix = "PAS";
				}

				if (!prefix.IsEmpty)
				{
					result = FormattableString.Invariant($"{prefix}: {unregisteredTrader.OrganizationCode}");
				}
			}

			return result;
		}
	}
}
