using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Business
{
	public class CMRConsignmentAddressFormatter : ICMRAddressFormatter
	{
		public ZString GetFullAddress(IDocAddress address)
		{
			if (address == null)
			{
				return ZString.Empty;
			}

			var builder = new ZStringBuilder();
			builder.AppendIfNotEmpty(address.E2_CompanyName.ToUpperInvariant());
			builder.AppendIfNotEmpty(address.E2_Address1.ToUpperInvariant());
			builder.AppendIfNotEmpty(address.E2_Address2.ToUpperInvariant());

			var cityAndCountryBuilder = new ZStringBuilder();
			cityAndCountryBuilder.AppendIfNotEmpty(address.E2_City.ToUpperInvariant());
			cityAndCountryBuilder.AppendIfNotEmpty(address.E2_Postcode.ToUpperInvariant());
			cityAndCountryBuilder.AppendIfNotEmpty(address.CountryDescription.ToUpperInvariant());

			var cityAndCountryString = (ZString)cityAndCountryBuilder.ToStringWithDelimiterBetweenAppends(CMRConsignmentNoteConstants.Delimiters.DashBetweenSpaces);
			builder.AppendIfNotEmpty(cityAndCountryString);

			return builder.ToStringWithNewLineBetweenAppends();
		}
	}
}
