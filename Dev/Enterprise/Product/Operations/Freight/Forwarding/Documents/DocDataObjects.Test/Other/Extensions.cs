using System.Linq;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	static class Extensions
	{
		const string newLine = "\r\n";

		public static string ToAssertString(this IAddress address)
		{
			if (address == null)
			{
				return string.Empty;
			}

			var addressDetails = $"{address.CompanyName}{newLine}{address.AddressLine1}{newLine}{address.AddressLine2}";
			var contactDetails = $"{address.Contact}{newLine}{address.Email}{newLine}{address.Phone}{newLine}{address.Fax}".Trim();

			return string.IsNullOrEmpty(contactDetails) ? addressDetails : addressDetails + newLine + contactDetails;
		}

		public static string ToAssertString(this IContainer container, int indentLevel = 0)
		{
			if (container == null)
			{
				return string.Empty;
			}

			var indent = new string(' ', indentLevel * 3);

			var res = string.Concat(indent, container.Number, "|", container.PackCount, "|", container.Type.Code, "|", container.Type?.Type?.Code);

			var packingLines = container
				.PackingLines
				.Select(p => p.ToAssertString(indentLevel + 1))
				.ToArray();

			return packingLines.Any()
				? string.Concat(res, newLine, string.Join(newLine, packingLines))
				: res;
		}

		public static string ToAssertString(this IPackingLine packingLine, int indentLevel = 0)
		{
			if (packingLine == null)
			{
				return string.Empty;
			}

			var indent = new string(' ', indentLevel * 3);

			var res = string.Concat(indent, $"{packingLine.Quantity} {packingLine.PackageType.Code}|{packingLine.GoodsDescription}|{packingLine.ExportReferenceNumber}");

			var dangerousGoods = packingLine
				.DangerousGoods
				.Select(dg => dg.ToAssertString(indentLevel + 1));

			if (dangerousGoods.Any())
			{
				res = string.Concat(res, newLine, string.Join(newLine, dangerousGoods));
			}

			var harmonizedCodes = packingLine
				.HarmonizedCodes
				.Select(hc => hc.ToAssertString(indentLevel + 1));

			if (harmonizedCodes.Any())
			{
				res = string.Concat(res, newLine, string.Join(newLine, harmonizedCodes));
			}

			return res;
		}

		public static string ToAssertString(this IDangerousGood dg, int indentLevel = 0)
		{
			if (dg == null)
			{
				return string.Empty;
			}

			var indent = new string(' ', indentLevel * 3);
			return string.Concat(indent, dg.ProperShippingName);
		}

		public static string ToAssertString(this IHarmonizedCode hc, int indentLevel = 0)
		{
			if (hc == null)
			{
				return string.Empty;
			}

			var indent = new string(' ', indentLevel * 3);
			return $"{indent}{hc.Country.Code}|{hc.Code}";
		}

		public static string ToAssertString(this IUnloco unloco)
		{
			return $"{unloco.Code}|{unloco.Name}";
		}

		public static string ToAssertString(this ICountry country)
		{
			return $"{country.Code}|{country.Name}";
		}

		public static string ToAssertString(this ICodeDescription codeDescription)
		{
			return $"{codeDescription.Code}|{codeDescription.Description}";
		}
	}
}
