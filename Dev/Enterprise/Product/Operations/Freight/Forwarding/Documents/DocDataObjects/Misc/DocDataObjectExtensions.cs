using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public static class DocDataObjectExtensions
	{
		public static bool IsEmpty(this IAddress source)
		{
			return source == null
				|| string.IsNullOrWhiteSpace(source.CompanyName);
		}

		public static bool IsEmpty(this IUnloco source)
		{
			return source == null
				|| string.IsNullOrWhiteSpace(source.Code);
		}

		internal static bool IsEmpty(this TaxInfo source)
		{
			return source == null
				|| string.IsNullOrWhiteSpace(source.Code);
		}

		public static bool HasRegistrationNumber(this IAddress address, ZString type)
		{
			return address
				?.RegistrationNumbers
				?.Any(number => number.Type.Code == type) ?? false;
		}

		public static bool HasRegistrationNumber(this IAddress address, ZString countryOfIssueCode, ZString type)
		{
			return address
				?.RegistrationNumbers
				?.Any(number => number.CountryOfIssue.Code == countryOfIssueCode && number.Type.Code == type) ?? false;
		}

		public static ZString GetRegistrationNumber(this IAddress address, ZString type)
		{
			return address
				.RegistrationNumbers
				?.FirstOrDefault(number => number.Type != null && number.Type.Code == type)
				?.Value ?? ZString.Empty;
		}

		public static ZString GetRegistrationNumber(this IAddress address, ZString countryOfIssueCode, ZString type)
		{
			return address
				.RegistrationNumbers
				?.FirstOrDefault(number => number.CountryOfIssue != null
										&& number.CountryOfIssue.Code == countryOfIssueCode
										&& number.Type != null
										&& number.Type.Code == type)
				?.Value ?? ZString.Empty;
		}
	}
}
