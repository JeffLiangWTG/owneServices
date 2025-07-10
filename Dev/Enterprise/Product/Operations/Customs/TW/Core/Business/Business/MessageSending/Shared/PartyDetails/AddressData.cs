using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class AddressData
	{
		readonly bool hideENAddr;
		readonly bool hideZHTddr;

		public AddressData(JobDocAddress jobDocAddress, params ZString[] languages) : this(jobDocAddress, false, false, languages) { }

		public AddressData(JobDocAddress jobDocAddress, bool hideENAddr, bool hideZHTddr, params ZString[] languages) : this(jobDocAddress?.Address, hideENAddr, hideZHTddr, languages)
		{
			this.jobDocAddress = jobDocAddress;
		}

		public AddressData(OrgHeader header, JobDocAddress jobDocAddress, params ZString[] languages) : this(jobDocAddress?.Address ?? header.MainAddress, false, false, languages)
		{
			this.jobDocAddress = jobDocAddress;
		}

		public AddressData(OrgHeader header, params ZString[] languages) : this(header.MainAddress, false, false, languages) { }

		public AddressData(OrgAddress address, bool hideENAddr, bool hideZHTddr, params ZString[] languages)
		{
			currentAddress = address;
			languages = Argument.NotNull(languages, nameof(languages));
			orgAddress = (currentAddress != null && languages.Contains(currentAddress.Language)) ? currentAddress : null;
			translatedAddress = GetTranslatedAddress(languages);
			this.hideENAddr = hideENAddr;
			this.hideZHTddr = hideZHTddr;
			this.languages = languages;
		}

		protected virtual bool ShouldPopulateEnglishAddressLine
		{
			get
			{
				var result = ZBool.False;
				if (hasEffectiveAddress)
				{
					result = !Address1.IsEmpty && !hideENAddr;
				}
				return result;
			}
		}

		protected virtual bool ShouldPopulateChineseAddressLine
		{
			get
			{
				var result = ZBool.False;
				if (hasEffectiveAddress)
				{
					result = !Address1.IsEmpty && !hideZHTddr;
				}
				return result;
			}
		}

		IEffectiveAddress EffectiveAddress => fEffectiveAddress ?? (fEffectiveAddress = GetEffectiveAddress());
		IEffectiveAddress fEffectiveAddress;

		protected ZBool hasEffectiveAddress => EffectiveAddress != null;

		IEffectiveAddress GetEffectiveAddress()
		{
			IEffectiveAddress effectiveAddress = null;
			if (IsJobDocAddressOverride)
			{
				effectiveAddress = new JobDocAddressEffectiveAddress(jobDocAddress);
			}
			else if (orgAddress != null)
			{
				effectiveAddress = new OrgAddressEffectiveAddress(orgAddress);
			}
			else if (translatedAddress != null)
			{
				effectiveAddress = new OrgTranslatedAddressEffectiveAddress(translatedAddress);
			}
			return effectiveAddress;
		}

		#region Address
		readonly OrgAddress currentAddress;

		readonly OrgAddress orgAddress;

		readonly OrgTranslatedAddress translatedAddress;

		readonly JobDocAddress jobDocAddress;

		readonly ZString[] languages;

		protected ZBool IsJobDocAddressOverride => jobDocAddress != null && jobDocAddress.E2_AddressOverride;

		OrgTranslatedAddress GetTranslatedAddress(params ZString[] languages)
		{
			OrgTranslatedAddress result = null;
			foreach (var language in languages)
			{
				result = currentAddress?.GetTranslatedAddressInSpecificLanguage(language);
				if (result != null)
				{
					break;
				}
			}
			return result;
		}
		#endregion

		#region Properties
		public ZString CompanyName => EffectiveAddress?.CompanyName ?? ZString.Empty;

		public ZString Address1 => EffectiveAddress?.Address1 ?? ZString.Empty;

		public ZString Address2 => EffectiveAddress?.Address2 ?? ZString.Empty;

		public ZString AdditionalAddressInformation => EffectiveAddress?.AdditionalAddressInformation ?? ZString.Empty;

		public ZString City => EffectiveAddress?.City ?? ZString.Empty;

		public ZString StateCode => EffectiveAddress?.StateCode ?? ZString.Empty;

		public ZString StateDescription
		{
			get
			{
				var name = ZString.Empty;
				foreach (var language in languages)
				{
					name = EffectiveAddress?.GetStateDescription(language) ?? ZString.Empty;
					if (!name.IsEmpty)
					{
						break;
					}
				}
				return name;
			}
		}

		public ZString CountryCode => EffectiveAddress?.CountryCode ?? ZString.Empty;

		public ZString CountryName
		{
			get
			{
				var name = ZString.Empty;
				foreach (var language in languages)
				{
					name = EffectiveAddress?.GetCountryName(language) ?? ZString.Empty;
					if (!name.IsEmpty)
					{
						break;
					}
				}
				return name;
			}
		}

		public ZString Postcode => EffectiveAddress?.Postcode ?? ZString.Empty;

		public ZString Phone => EffectiveAddress?.Phone ?? ZString.Empty;

		public ZString EMail => EffectiveAddress?.EMail ?? ZString.Empty;

		public ZString EnglishAddressFormat => GetEnglishAddressFormatCore();
		protected virtual ZString GetEnglishAddressFormatCore()
		{
			var result = ZString.Empty;
			if (ShouldPopulateEnglishAddressLine)
			{
				var stringBuilder = new ZStringBuilder();
				stringBuilder.AppendIfNotEmpty(Address1);
				stringBuilder.AppendIfNotEmpty(Address2);
				stringBuilder.AppendIfNotEmpty(AdditionalAddressInformation);
				stringBuilder.AppendIfNotEmpty(City);
				if (CountryCode != Core.Constants.CountryCodes.Taiwan)
				{
					stringBuilder.AppendIfNotEmpty(StateDescription);
				}
				stringBuilder.AppendIfNotEmpty(Postcode);
				stringBuilder.AppendIfNotEmpty(CountryName);
				result = stringBuilder.ToStringWithDelimiterBetweenAppends(" ").ToUpperInvariant();
			}
			return result;
		}

		public ZString ChineseTraditionalAddressFormat => GetChineseTraditionalAddressFormatCore();
		protected virtual ZString GetChineseTraditionalAddressFormatCore()
		{
			var result = ZString.Empty;
			if (ShouldPopulateChineseAddressLine)
			{
				var stringBuilder = new ZStringBuilder();
				stringBuilder.AppendIfNotEmpty(Postcode);
				if (CountryCode != Core.Constants.CountryCodes.Taiwan)
				{
					stringBuilder.Append(CountryName);
					stringBuilder.AppendIfNotEmpty(StateDescription);
				}
				stringBuilder.AppendIfNotEmpty(City);
				stringBuilder.AppendIfNotEmpty(Address1);
				stringBuilder.AppendIfNotEmpty(Address2);
				stringBuilder.AppendIfNotEmpty(AdditionalAddressInformation);
				result = stringBuilder.ToString();
			}
			return result;
		}
		#endregion
	}
}
