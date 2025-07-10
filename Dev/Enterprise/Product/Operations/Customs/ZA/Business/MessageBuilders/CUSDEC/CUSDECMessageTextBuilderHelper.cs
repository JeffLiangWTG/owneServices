using System;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	internal static class CUSDECMessageBuilderHelper
	{
		public static string TranslateToEDIFACTMessageFunctionCode(this ZString input)
		{
			ZString result = input;
			switch (input.ToUpperInvariant())
			{
				case MessageSubTypeCodes.Codes.Original:
					result = MessageFunctionCodeList.Codes.Original;
					break;
				case MessageSubTypeCodes.Codes.Change:
					result = MessageFunctionCodeList.Codes.Change;
					break;
				case MessageSubTypeCodes.Codes.Cancellation:
					result = MessageFunctionCodeList.Codes.Cancellation;
					break;
				case MessageSubTypeCodes.Codes.Replace:
					result = MessageFunctionCodeList.Codes.Replace;
					break;
			}
			return result;
		}

		#region For IContainerDataProvider

		internal static ZString GetFormatContainerSealNumber(this IContainerInformation container) => string.Format(CultureInfo.InvariantCulture, "{0,-15}{1,-15}", container.FirstSealNumber.Left(15), container.SecondSealNumber.Left(15)).TrimEnd();

		#endregion

		#region General Helper

		internal static ZString ToCCYYMMDD(this ZDateTime input) => input.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

		internal static void BuildForMutualExclusiveValues(ZString value1, ZString value2, Action<ZString> action)
		{
			if (!value1.IsEmpty && value2.IsEmpty)
			{
				action(value1);
			}
			else if (value1.IsEmpty && !value2.IsEmpty)
			{
				action(value2);
			}
		}

		#endregion
	}

	internal class OrgHeaderAddressInformationWrapper : IAddressInformation
	{
		OrgHeaderAddressInformationWrapper(OrgHeader orgHeader, string orgCodeType, string organizationCodeQualifier, string organizationCodeCountry)
		{
			this.organizationCodeType = orgCodeType;
			this.organization = orgHeader;
			this.organizationQualifier = organizationCodeQualifier;
			this.organizationCodeCountry = organizationCodeCountry;
		}

		readonly OrgHeader organization;
		readonly ZString organizationQualifier;
		readonly string organizationCodeType;
		readonly ZString organizationCodeCountry;

		#region Static

		public static OrgHeaderAddressInformationWrapper Wrap(OrgHeader orgHeader, string orgCodeType, string organizationCodeQualifier = "", string organizationCodeCountry = Core.Constants.CountryCodes.SouthAfrica)
		{
			return (orgHeader != null) ? new OrgHeaderAddressInformationWrapper(orgHeader, orgCodeType, organizationCodeQualifier, organizationCodeCountry) : null;
		}

		public static OrgHeaderAddressInformationWrapper WrapDeclarant(OrgHeader declarant)
		{
			OrgHeaderAddressInformationWrapper result = null;
			if (declarant.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.TaxFileCode).IsEmpty)
			{
				var passport = OrgHelper.GetPassportCusCode(declarant);
				var idnumber = declarant.CustomsCodes.GetCustomsRegNo(OrgCusCode.SouthAfricaCodeTypes.IDNumber);
				if (!idnumber.IsEmpty)
				{
					result = Wrap(declarant, OrgCusCode.SouthAfricaCodeTypes.IDNumber, Edifact.D96B.Elements.CodeListQualifierList.CitizenIdentification.ToString());
				}
				else if (passport != null)
				{
					result = Wrap(declarant, OrgCusCode.CodeTypes.PassportID, Edifact.D96B.Elements.CodeListQualifierList.PassportNumber.ToString(), passport.OK_RN_NKCodeCountry);
				}
			}
			else
			{
				result = Wrap(declarant, OrgCusCode.CodeTypes.TaxFileCode, Edifact.D96B.Elements.CodeListQualifierList.TaxPartyIdentification.ToString());
			}
			return result;
		}

		#endregion

		#region Properties

		public ZString OrganizationCode => organizationCodeType == null ? ZString.Empty : organization.CustomsCodes.GetCustomsRegNo(organizationCodeType, organizationCodeCountry);

		public ZString OrganizationCodeQualifier => organizationQualifier;

		public ZString Name => organization.OH_FullName;

		public ZString Address => organization.MainAddress.AddressAsASingleLineWithoutCompanyName;

		public ZString City => organization.MainAddress.City;

		public ZString PostCode => organization.MainAddress.Postcode;

		public ZString VATRegistrationNo => organization.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.SouthAfrica);

		#endregion
	}
}
