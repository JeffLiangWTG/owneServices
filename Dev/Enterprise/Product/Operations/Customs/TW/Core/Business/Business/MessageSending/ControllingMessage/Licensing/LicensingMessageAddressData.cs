using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class LicensingMessageAddressData : AddressData
	{
		readonly bool isCertificate15;

		public LicensingMessageAddressData(JobDocAddress jobDocAddress, bool isCertificate15, params ZString[] languages) : base(jobDocAddress: jobDocAddress, languages: languages)
		{
			this.isCertificate15 = isCertificate15;
		}

		protected override ZString GetEnglishAddressFormatCore()
		{
			var result = ZString.Empty;
			if (!Address1.IsEmpty)
			{
				var stringBuilder = new ZStringBuilder();
				stringBuilder.AppendIfNotEmpty(Address1);
				stringBuilder.AppendIfNotEmpty(Address2);
				if (IsJobDocAddressOverride)
				{
					stringBuilder.AppendIfNotEmpty(AdditionalAddressInformation);
				}
				stringBuilder.AppendIfNotEmpty(City);
				stringBuilder.AppendIfNotEmpty(StateDescription);
				stringBuilder.AppendIfNotEmpty(Postcode);
				stringBuilder.AppendIfNotEmpty(CountryName);
				result = stringBuilder.ToStringWithDelimiterBetweenAppends(" ").ToUpperInvariant();
			}
			return result;
		}

		protected override ZString GetChineseTraditionalAddressFormatCore()
		{
			var result = ZString.Empty;
			if (!Address1.IsEmpty)
			{
				var stringBuilder = new ZStringBuilder();
				if (!isCertificate15)
				{
					stringBuilder.AppendIfNotEmpty(Postcode);
					stringBuilder.AppendIfNotEmpty(CountryName);
				}
				stringBuilder.AppendIfNotEmpty(City);
				stringBuilder.AppendIfNotEmpty(Address1);
				stringBuilder.AppendIfNotEmpty(Address2);
				if (IsJobDocAddressOverride)
				{
					stringBuilder.AppendIfNotEmpty(AdditionalAddressInformation);
				}
				result = stringBuilder.ToString();
			}
			return result;
		}
	}
}
