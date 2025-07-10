using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Common
{
	public static class CommonHelper
	{
		public static string CleanCommodityCode(string itemId)
		{
			if (itemId.Length > 3 && itemId.Length % 2 == 0)
			{
				while (itemId.Length > 3 && itemId.EndsWith("00", StringComparison.Ordinal))
				{
					itemId = itemId.Substring(0, itemId.Length - 2);
				}
			}

			return itemId;
		}

		public static string CleanHtmlTags(string description, bool multiLineOuput = true)
		{
			var tags = new Dictionary<string, string>
			{
				//Subscripts
				{ "<sub>0</sub>", "\u2080" },
				{ "<sub>1</sub>", "\u2081" },
				{ "<sub>2</sub>", "\u2082" },
				{ "<sub>3</sub>", "\u2083" },
				{ "<sub>4</sub>", "\u2084" },
				{ "<sub>5</sub>", "\u2085" },
				{ "<sub>6</sub>", "\u2086" },
				{ "<sub>7</sub>", "\u2087" },
				{ "<sub>8</sub>", "\u2088" },
				{ "<sub>9</sub>", "\u2089" },
				//Superscripts
				{ "<sup>0</sup>", "\u2070" },
				{ "<sup>1</sup>", "\u00B9" },
				{ "<sup>2</sup>", "\u00B2" },
				{ "<sup>3</sup>", "\u00B3" },
				{ "<sup>4</sup>", "\u2074" },
				{ "<sup>5</sup>", "\u2075" },
				{ "<sup>6</sup>", "\u2076" },
				{ "<sup>7</sup>", "\u2077" },
				{ "<sup>8</sup>", "\u2078" },
				{ "<sup>9</sup>", "\u2079" },
				{ "<sup>o</sup>", "\u00B0" }
			};

			if (multiLineOuput)
			{
				tags.Add("<br>", "\r\n");
				tags.Add("<p/>", "\r\n");
			}
			else
			{
				tags.Add("<br>", "; ");
				tags.Add("<p/>", "; ");
			}

			foreach (var tag in tags)
			{
				description = description.Replace(tag.Key, tag.Value);
			}

			return description;
		}

		public static DateTime CalcMinDate(DateTime? dateTime)
		{
			var dt = dateTime ?? DefaultValues.MinimumDateTime;

			dt = dt < DefaultValues.MinimumDateTime ? DefaultValues.MinimumDateTime : dt;

			return DropSeconds(dt);
		}

		public static DateTime CalcMaxDate(DateTime? dateTime)
		{
			var dt = dateTime ?? DefaultValues.MaximumDateTime;

			dt = dt > DefaultValues.MaximumDateTime ? DefaultValues.MaximumDateTime : dt;

			if (dt.TimeOfDay == TimeSpan.Zero && dt != DefaultValues.MinimumDateTime && dt > DateTime.MinValue)
			{
				dt = dt.AddMinutes(-1);
			}
			else
			{
				dt = DropSeconds(dt);
			}

			return dt;
		}

		public static DateTime DropSeconds(DateTime dateTime) => dateTime.AddSeconds(-dateTime.Second);

		public static class DefaultValues
		{
			public static DateTime MinimumDateTime => new DateTime(1900, 01, 01, 00, 00, 00);
			public static DateTime MaximumDateTime => new DateTime(2079, 06, 06, 23, 59, 00);
		}

		public static class MetaInfoOpTypes
		{
			public const string Created = "C";
			public const string Deleted = "D";
			public const string Updated = "U";
		}
	}
}
