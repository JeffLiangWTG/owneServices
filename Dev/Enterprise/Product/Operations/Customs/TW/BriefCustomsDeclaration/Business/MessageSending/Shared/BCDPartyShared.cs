using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public abstract class BCDPartyShared : IPartyDetails, IAddress
	{
		protected abstract ZString GetIDCore();

		protected abstract ZString GetNameCore();

		protected abstract ZString GetChineseNameCore();

		protected abstract ZString RegNoType { get; }

		protected abstract ZString GetCustomsControlIDCore();

		protected abstract ZString GetLineCore();

		protected abstract ZString GetChineseLineCore();

		protected abstract ZString GetCountryCodeCore();

		ZString IPartyDetails.ID => GetIDCore();

		ZString IPartyDetails.Name => GetNameCore();

		ZString IPartyDetails.ChineseName => GetChineseNameCore();

		ZString IPartyDetails.TypeCode => OrgHeaderHelper.GetPartyIdentifierCode(RegNoType);

		ZString IPartyDetails.CustomsControlID => GetCustomsControlIDCore();

		ZString IPartyDetails.PaymentOnAccountBusinessID => ZString.Empty;

		ZString IPartyDetails.RoleCode => ZString.Empty;

		ZString IPartyDetails.SubBoxID => ZString.Empty;

		IAddress IPartyDetails.Address => this;

		ILPCOAuthorizedParty IPartyDetails.LPCOAuthorizedParty => null;

		IEnumerable<ICommunication> IPartyDetails.Communications => Enumerable.Empty<ICommunication>();

		ZString IPartyDetails.ContactName => ZString.Empty;

		ZString IPartyDetails.OwnerName => ZString.Empty;

		ZString IPartyDetails.MainManufacturer => ZString.Empty;

		ZString IPartyDetails.UndertakeCode => ZString.Empty;

		IEnumerable<IAdditionalInformation> IPartyDetails.AdditionalInformations => Enumerable.Empty<IAdditionalInformation>();

		ZString IAddress.Line => GetLineCore();

		ZString IAddress.ChineseLine => GetChineseLineCore();

		ZString IAddress.CountryCode => GetCountryCodeCore();

		ZString IAddress.CountrySubDivisionID => ZString.Empty;

		ZString IAddress.CountrySubDivisionName => ZString.Empty;

		protected ZString GetLine(ZString postCode, RefCountry country, ZString state, ZString city, ZString street1, ZString street2)
		{
			var lineBuilder = new ZStringBuilder();
			lineBuilder.AppendIfNotEmpty(street1);
			lineBuilder.AppendIfNotEmpty(street2);
			lineBuilder.AppendIfNotEmpty(city);
			if (IsStateRequired(country))
			{
				lineBuilder.AppendIfNotEmpty(state);
			}
			lineBuilder.AppendIfNotEmpty(postCode);
			if (country?.RN_DescMultilingual.GetLocalizedValue(Core.SharedConstants.Languages.English) is ZString countryEnglishDesc)
			{
				lineBuilder.AppendIfNotEmpty(countryEnglishDesc);
			}
			return lineBuilder.ToStringWithDelimiterBetweenAppends(" ");
		}

		protected ZString GetChineseLine(ZString postCode, RefCountry country, ZString localState, ZString localCity, ZString localStreet1, ZString localStreet2)
		{
			var chineseLineBuilder = new ZStringBuilder();
			var countryName = country.GetChineseCountryName();
			chineseLineBuilder.AppendIfNotEmpty(postCode);
			chineseLineBuilder.AppendIfNotEmpty(countryName);
			if (IsStateRequired(country))
			{
				chineseLineBuilder.AppendIfNotEmpty(localState);
			}
			chineseLineBuilder.AppendIfNotEmpty(localCity);
			chineseLineBuilder.AppendIfNotEmpty(localStreet1);
			chineseLineBuilder.AppendIfNotEmpty(localStreet2);
			return chineseLineBuilder.ToString();
		}

		bool IsStateRequired(RefCountry country)
		{
			return (country?.RN_Code ?? ZString.Empty) != Core.Constants.CountryCodes.Taiwan;
		}
	}
}
