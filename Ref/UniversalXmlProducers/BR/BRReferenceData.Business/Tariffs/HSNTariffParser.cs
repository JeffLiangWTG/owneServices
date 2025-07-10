using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class HSNTariffParser : BaseParser
	{
		public HSNTariffParser(string dataSource) : base(dataSource)
		{
		}

		public void ExportToXMLFile(Stream streamXls, Stream streamXml, string outputFileName, DateTime publicationTime)
		{
			Argument.NotNull(streamXml, nameof(streamXml));

			var refTariffsList = GetRefCusTariffLists(streamXls, streamXml);
			var writerConfiguration = Helper.GetRefCusTariffWriterConfiguration(Constants.TariffTypes.Codes.HSN);
			Helper.ExportToXMLFile(outputFileName, dataSource, publicationTime, writerConfiguration, refTariffsList);
		}

		ICollection<RefCusTariff> GetRefCusTariffLists(Stream streamXls, Stream streamXml)
		{
			Argument.NotNull(streamXls, nameof(streamXls));
			Argument.NotNull(streamXml, nameof(streamXml));

			var result = new HashSet<RefCusTariff>();
			var addedTariffs = new HashSet<string>();
			var mapCusTariff = new Dictionary<string, RefCusTariff>();
			var xls = new XlsFile(streamXls, true);
			var xml = XDocument.Load(streamXml);

			Contract.Assume(xls != null);
			Contract.Assume(xml != null);

			xls?.SetSheetSelected(1, true);
			var rowCount = xls?.GetRowCount(xls.ActiveSheet);
			for (var rowId = 2; rowId <= rowCount; rowId++)
			{
				var refCusTariff = new RefCusTariff
				{
					ZZ1_StartDate = DateTime.FromOADate(double.Parse(xls?.GetCellValue(rowId, 2).ToString(), CultureInfo.CurrentCulture)),
					ZZ1_TariffCode = xls?.GetCellValue(rowId, 1)?.ToString().Trim(),
					ZZ1_EndDate = GetDefaultEndDate(),
					ZZ1_ZZF_NKTaxOrFeeCode = "ICM",
				};
				mapCusTariff?.Add(refCusTariff.ZZ1_TariffCode, refCusTariff);
			}

			var tariffElements = xml.Root.Descendants(HSNTariffContants.TagCustomsTariff);

			foreach (XElement element in tariffElements)
			{
				string tariffCode = element.GetElementValueAsString(HSNTariffContants.TagCustomsTariffCode, 8);
				string tariffDescription = element.GetElementValueAsString(HSNTariffContants.TagCustomsTariffDescription, 500);

				if (tariffCode == null || tariffCode.Length < 8 || string.IsNullOrEmpty(tariffDescription))
				{
					continue;
				}

				if (!mapCusTariff.TryGetValue(tariffCode, out var refCusTariff))
				{
					refCusTariff = new RefCusTariff
					{
						ZZ1_StartDate = GetDefaultStartDate(),
						ZZ1_EndDate = GetDefaultEndDate()
					};
				}

				refCusTariff.ZZ1_Description = DescriptionCleaner(tariffDescription);
				refCusTariff.ZZ1_TariffCode = tariffCode;

				BuildZZ1_CompositeKeyOnZZ5(refCusTariff);
				if (addedTariffs.Add(tariffCode))
				{
					result?.Add(refCusTariff);
				}
			}
			return result;
		}

		static string DescriptionCleaner(string description)
		{
			return description?.Trim(new char[] { '-', ' ' });
		}

		Dictionary<string, int[]> GetSessions()
		{
			if (sessions == null)
			{
				sessions = new Dictionary<string, int[]>();
				sessions?.Add("01", new int[] { 1, 5 });
				sessions?.Add("02", new int[] { 6, 14 });
				sessions?.Add("03", new int[] { 15, 15 });
				sessions?.Add("04", new int[] { 16, 24 });
				sessions?.Add("05", new int[] { 25, 27 });
				sessions?.Add("06", new int[] { 28, 38 });
				sessions?.Add("07", new int[] { 39, 40 });
				sessions?.Add("08", new int[] { 41, 43 });
				sessions?.Add("09", new int[] { 44, 46 });
				sessions?.Add("10", new int[] { 47, 49 });
				sessions?.Add("11", new int[] { 50, 63 });
				sessions?.Add("12", new int[] { 64, 67 });
				sessions?.Add("13", new int[] { 68, 70 });
				sessions?.Add("14", new int[] { 71, 71 });
				sessions?.Add("15", new int[] { 72, 83 });
				sessions?.Add("16", new int[] { 84, 85 });
				sessions?.Add("17", new int[] { 86, 89 });
				sessions?.Add("18", new int[] { 90, 92 });
				sessions?.Add("19", new int[] { 93, 93 });
				sessions?.Add("20", new int[] { 94, 96 });
				sessions?.Add("21", new int[] { 97, 97 });
			}

			return sessions;
		}

		string GetNcmSession(string ncmCode)
		{
			int firstTwoCodes = int.Parse(ncmCode?.Substring(0, 2), CultureInfo.CurrentCulture);

			foreach (string session in GetSessions().Keys)
			{
				if (GetSessions().TryGetValue(session, out var range))
				{
					if (firstTwoCodes >= range[0] && firstTwoCodes <= range[1])
					{
						return session;
					}
				}
			}

			return null;
		}

		void BuildZZ1_CompositeKeyOnZZ5(RefCusTariff refCusTariff)
		{
			var tariffCode = refCusTariff.ZZ1_TariffCode;
			string compositeKey = $"{GetNcmSession(tariffCode)}.{tariffCode?.Substring(0, 2)}..{tariffCode?.Substring(2, 2)}.{tariffCode?.Substring(4, 1)}.{tariffCode?.Substring(5, 1)}.{tariffCode?.Substring(6, 1)}";

			if (tariffCode.Length == 8)
			{
				compositeKey += $".{tariffCode?.Substring(7, 1)}";
			}

			refCusTariff.ZZ1_CompositeKeyOnZZ5 = compositeKey;
		}

		static DateTime GetDefaultEndDate()
		{
			return DateTime.ParseExact($"01/01/2079T00:00:00", "dd/MM/yyyyThh:mm:ss", null);
		}

		public virtual DateTime GetDefaultStartDate()
		{
			return DateTime.ParseExact($"01/01/{DateTime.Today.Year}T00:00:00", "dd/MM/yyyyThh:mm:ss", null);
		}

		Dictionary<string, int[]> sessions;
	}
}
