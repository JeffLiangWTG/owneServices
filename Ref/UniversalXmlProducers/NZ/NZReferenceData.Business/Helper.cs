using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	public static class Helper
	{
		public static XmlWriter GenerateXmlWriter<T>(XmlWriterConfiguration configuration, UpdateType updateType, string dataSource, DateTime publicationDateTime, IEnumerable<T> list) where T : RefDataRepoModelEntityType
		{
			Argument.NotNull(configuration, nameof(configuration));
			Argument.NotNullOrEmpty(dataSource, nameof(dataSource));
			Argument.NotNull(list, nameof(list));

			var writer = new XmlWriter(configuration);
			writer.SetDataSource(dataSource);
			writer.SetPublicationTime(publicationDateTime);
			writer.SetUpdateType(updateType);

			foreach (var data in list)
			{
				writer.PopulateData(data);
			}

			return writer;
		}

		public static void ExportToXMLFile(XmlWriter writer, string outputFile)
		{
			Argument.NotNull(writer, nameof(writer));
			Argument.NotNullOrEmpty(outputFile, nameof(outputFile));

			var directoryName = Path.GetDirectoryName(outputFile);

			Directory.CreateDirectory(directoryName);
			writer.SaveXml(outputFile);
		}

		public static readonly Dictionary<int, string[]> SectionToChapters = new Dictionary<int, string[]>
		{
			{ 1, GetChapters(1, 5)},
			{ 2, GetChapters(6, 14)},
			{ 3, GetChapters(15 ,15)},
			{ 4, GetChapters(16, 24)},
			{ 5, GetChapters(25, 27)},
			{ 6, GetChapters(28, 38)},
			{ 7, GetChapters(39, 40)},
			{ 8, GetChapters(41, 43)},
			{ 9, GetChapters(44, 46)},
			{ 10, GetChapters(47, 49)},
			{ 11, GetChapters(50, 63)},
			{ 12, GetChapters(64, 67)},
			{ 13, GetChapters(68, 70)},
			{ 14, GetChapters(71, 71)},
			{ 15, GetChapters(72, 83)},
			{ 16, GetChapters(84, 85)},
			{ 17, GetChapters(86, 89)},
			{ 18, GetChapters(90, 92)},
			{ 19, GetChapters(93, 93)},
			{ 20, GetChapters(94, 96)},
			{ 21, GetChapters(97, 97)}
		};

		static string[] GetChapters(int start, int end)
		{
			return Enumerable.Range(start, end - start + 1).Select(x => x.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')).ToArray();
		}

		public static string GetFactorString(string factorString, bool needDivision, string lineString = null)
		{
			if (!decimal.TryParse(factorString, out var factor))
			{
				var errorMessageBuilder = new StringBuilder($"Unable to parse factor '{factorString}'.");
				if (lineString != null)
				{
					errorMessageBuilder.Append(CultureInfo.InvariantCulture, $" Line string: {lineString}");
				}
				throw new RefDataParseException(errorMessageBuilder.ToString());
			}

			if (needDivision)
			{
				factor /= 100;
			}

			return $"{factor:0.######}";
		}

		public static RefCusRate AddNewRefCusRate(
			this RefCusTariff tariff,
			DateTime startDate,
			DateTime endDate,
			string tradeGroup,
			string formula = null,
			string cusRateUom = null,
			bool isManual = false)
		{
			var rate = new RefCusRate
			{
				ZZ2_StartDate = startDate,
				ZZ2_EndDate = endDate,
				ZZ2_RateFormula = formula,
				ZZ2_ZZS_NKPreference = tradeGroup
			};

			if (!string.IsNullOrEmpty(cusRateUom))
			{
				rate.RefCusRateUOMs = [new RefCusRateUOM { ZXG_UOM = cusRateUom }];
			}

			var applicability = new RefCusApplicability
			{
				ZZT_StartDate = startDate,
				ZZT_EndDate = endDate,
				ZZT_ZZA_NKTradeGroup = tradeGroup
			};
			if (isManual)
			{
				applicability.ZZT_AdditionalCode = Constants.ApplicabilityAdditionalCodes.IsManual;
			}
			rate.RefCusApplicabilities = [applicability];

			tariff.RefCusRates = tariff.RefCusRates?.Append(rate).ToArray() ?? [rate];
			return rate;
		}

		public static void AddNewRefCusApplicability(this RefCusRate rate, DateTime startDate, DateTime endDate, string nkTradeGroup, bool isManual = false)
		{
			var newApplicability = new RefCusApplicability
			{
				ZZT_StartDate = startDate,
				ZZT_EndDate = endDate,
				ZZT_ZZA_NKTradeGroup = nkTradeGroup
			};

			if (isManual)
			{
				newApplicability.ZZT_AdditionalCode = Constants.ApplicabilityAdditionalCodes.IsManual;
			}

			if (rate.RefCusApplicabilities == null)
			{
				rate.RefCusApplicabilities = [newApplicability];
				return;
			}

			if (!rate.RefCusApplicabilities.Any(a =>
				    a.ZZT_StartDate == startDate &&
				    a.ZZT_EndDate == endDate &&
				    a.ZZT_ZZA_NKTradeGroup == nkTradeGroup))
			{
				rate.RefCusApplicabilities = rate.RefCusApplicabilities
					.Append(newApplicability)
					.OrderBy(r => r.ZZT_EndDate)
					.ToArray();
			}
		}
	}
}
