using System.Collections.Generic;
using System.Linq;
using System.Text;
using WiseRates.Api.Model;

namespace Enterprise.Rating.Business
{
	public static class BookingTermsConverter
	{
		public static string Convert(Dictionary<string, BookingTerms> bookingTerms)
		{
			var collectionToPopulate = new List<(string title, string value)>();

			foreach (var item in bookingTerms)
			{
				if (item.Value?.Items?.Any() == true)
				{
					var header = (item.Key, string.Empty);
					collectionToPopulate.Add(header);

					foreach (var term in item.Value.Items)
					{
						collectionToPopulate.Add((term.Name, $"{term.Currency} {term.Fee}")); // Just a template, no content.
					}

					collectionToPopulate.Add((string.Empty, string.Empty));
				}
			}

			if (collectionToPopulate.Any())
			{
				var firstColumnWidth = -1 * collectionToPopulate.Select(c => c.title.Length).Max();

				var builder = new StringBuilder();
				foreach (var (title, value) in collectionToPopulate)
				{
					builder.AppendLine($"{string.Format($"{{0,{firstColumnWidth}}}", title)} {value}"); // Just a template, no content.
				}

				return builder.ToString();
			}

			return string.Empty;
		}
	}
}
