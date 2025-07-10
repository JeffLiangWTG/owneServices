using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.DeTariffs;

public sealed class DeTariffParser
{
	readonly string extractionDirectory;
	readonly string outputFilePath;
	DateTime lastModified = DateTime.MinValue;
	Dictionary<string, TariffNationalCode> nationalCodesBySid;
	Dictionary<string, TariffVatCode> vatCodesBySid;
	Dictionary<string, TariffNationalCodeDescription> descriptionsBySid;

	public DeTariffParser(string extractionDirectory, string outputFilePath)
	{
		this.extractionDirectory = extractionDirectory;
		this.outputFilePath = outputFilePath;
	}

	XmlSourceFiles GetInputXmlFileNames()
	{
		return new XmlSourceFiles()
		{
			NationalCodesFileName = Directory.EnumerateFiles(extractionDirectory, "XD*_N42000_*.xml").Single(),
			NationalDescriptionsFileName =
				Directory.EnumerateFiles(extractionDirectory, "XD*_T40010_*.xml").Single(),
			VatCodesFileName = Directory.EnumerateFiles(extractionDirectory, "XD*_N42020_*.xml").Single(),
			UpdateFileNames = Directory.EnumerateFiles(extractionDirectory, "XD????????.xml").OrderBy(e => e).ToArray(),
		};
	}

	public void ConvertToXMLFile()
	{
		var xmlSourceFiles = GetInputXmlFileNames();

		ProcessBaseline(xmlSourceFiles);

		ProcessUpdates(xmlSourceFiles);

		JoinData();

		UpdateHierarchicalFallbackData();

		var result = ConvertToCodeList();

		var writerConfiguration = Helper.GetRefCusTariffWriterConfiguration();
		Helper.ExportToXMLFile("DE Import Tariffs", outputFilePath, writerConfiguration, lastModified, result);
	}

	void UpdateHierarchicalFallbackData()
	{
		var lookup = nationalCodesBySid.Values.ToLookup(e => e.FullTariffCode);

		foreach (var code in nationalCodesBySid.Values.OrderByDescending(e => e.Level).SkipWhile(e => e.Level == 5))
		{
			foreach (var upperLevelCode in code.GetUpperLevelCodes())
			{
				var codes = lookup[upperLevelCode].SelectMany(e => e.VatCode);
				code.VatCode.AddRange(codes);
				if (code.VatCode.Count != 0)
				{
					break;
				}
			}
		}
	}


	IEnumerable<RefCusTariff> ConvertToCodeList()
	{
		return nationalCodesBySid.Values
			.Where(e => e.IsWl80)
			.GroupBy(e => e.TariffCode)
			.Select(tariffNationalCodes => new RefCusTariff()
			{
				ZZ1_TariffCode = tariffNationalCodes.Key,
				RefCusTariffNationalCodes = GetNationalCodes(tariffNationalCodes)
					.Where(e => e.ZZW_EndDate > lastModified && !string.IsNullOrEmpty(e.ZZW_Description))
					.OrderBy(e => e.ZZW_NationalCode)
					.ToArray(),
			})
			.Where(e => e.RefCusTariffNationalCodes?.Length > 0)
			.OrderBy(e => e.ZZ1_TariffCode);
	}

	void JoinData()
	{
		foreach (var vat in vatCodesBySid.Values)
		{
			if (nationalCodesBySid.TryGetValue(vat.Sid_Wn, out var nationalCode))
				nationalCode.VatCode.Add(vat);
		}

		var nationalCodesBySidWn = nationalCodesBySid.Values.ToLookup(e => e.Sid_Wn);
		foreach (var description in descriptionsBySid.Values)
		{
			foreach (var code in nationalCodesBySidWn[description.Sid_Wn])
			{
				code.Description.Add(description);
			}
		}
	}

	void ProcessUpdates(XmlSourceFiles xmlSourceFiles)
	{
		var updates = xmlSourceFiles.UpdateFileNames.SelectMany(GetXmlUpdateElements);

		foreach (var o in updates)
		{
			switch (o)
			{
				case TariffNationalCode { UpdateType: DeTariffsConstants.InsertFlag or DeTariffsConstants.UpdateFlag } nationalCode:
					nationalCodesBySid[nationalCode.Sid] = nationalCode;
					break;
				case TariffNationalCode { UpdateType: DeTariffsConstants.DeleteFlag } nationalCode:
					nationalCodesBySid.Remove(nationalCode.Sid);
					break;
				case TariffNationalCodeDescription { UpdateType: DeTariffsConstants.InsertFlag or DeTariffsConstants.UpdateFlag } nationalCodeDescription:
					descriptionsBySid[nationalCodeDescription.Sid] = nationalCodeDescription;
					break;
				case TariffNationalCodeDescription { UpdateType: DeTariffsConstants.DeleteFlag } nationalCodeDescription:
					descriptionsBySid.Remove(nationalCodeDescription.Sid);
					break;
				case TariffVatCode { UpdateType: DeTariffsConstants.InsertFlag or DeTariffsConstants.UpdateFlag } vat:
					vatCodesBySid[vat.Sid] = vat;
					break;
				case TariffVatCode { UpdateType: DeTariffsConstants.DeleteFlag } vat:
					vatCodesBySid.Remove(vat.Sid);
					break;
			}
		}
	}

	void ProcessBaseline(XmlSourceFiles xmlSourceFiles)
	{
		nationalCodesBySid = ToDictionaryKeepLatest(ParseAsNationalCodes(GetXmlElements(xmlSourceFiles.NationalCodesFileName)), e => e.Sid);

		vatCodesBySid = ToDictionaryKeepLatest(ParseAsVatCode(GetXmlElements(xmlSourceFiles.VatCodesFileName)), e => e.Sid);

		descriptionsBySid = ToDictionaryKeepLatest(ParseAsNationalCodesDescription(GetXmlElements(xmlSourceFiles.NationalDescriptionsFileName)), e => e.Sid);
		return;

		static Dictionary<TKey, TValue> ToDictionaryKeepLatest<TKey, TValue>(IEnumerable<TValue> source, Func<TValue, TKey> keySelector)
		{
			var dictionary = new Dictionary<TKey, TValue>();
			foreach (var value in source)
			{
				dictionary[keySelector(value)] = value;
			}

			return dictionary;
		}
	}

	static IEnumerable<RefCusTariffNationalCode> GetNationalCodes(IEnumerable<TariffNationalCode> nationalCodes)
	{
		foreach (var codes in nationalCodes.GroupBy(e => e.NationalCode))
		{
			foreach (var range in codes)
			{
				var nationalCodeStartDate = range.StartDate;
				var nationalCodeEndDate = range.EndDate;

				var ranges = new List<RefCusTariffNationalCode>
				{
					new()
					{
						ZZW_NationalCode = range.NationalCode,
						ZZW_ZZF_NKTaxOrFeeCode = null,
						ZZW_StartDate = nationalCodeStartDate,
						ZZW_EndDate = nationalCodeEndDate,
					},
				};

				PartitionByVatChange(range, ranges);

				PartitionByDescriptionChange(range, ranges);

				foreach (var code in ranges)
				{
					DefaultVatCode(code);
					CleanupDescription(code, range.Text);
				}

				foreach (var completeRange in DeduplicateContinuousRangesWithoutChange(ranges))
				{
					ClampMinDate(completeRange);
					yield return completeRange;
				}
			}
		}
	}

	static void ClampMinDate(RefCusTariffNationalCode code)
	{
		if (code.ZZW_StartDate < DeTariffsConstants.MinDate)
		{
			code.ZZW_StartDate = DeTariffsConstants.MinDate;
		}
		if (code.ZZW_EndDate < DeTariffsConstants.MinDate)
		{
			code.ZZW_EndDate = DeTariffsConstants.MinDate;
		}
	}

	static void CleanupDescription(RefCusTariffNationalCode code, string nationalCodeText)
	{
		code.ZZW_Description ??= string.Empty;
		nationalCodeText ??= string.Empty;
		var sb = new StringBuilder(code.ZZW_Description.Length + nationalCodeText.Length);

		var cleanedPart1 = CleanSpan(code.ZZW_Description);
		var cleanedPart2 = CleanSpan(nationalCodeText);

		sb.Append(cleanedPart1);
		if (!cleanedPart1.IsEmpty && !cleanedPart2.IsEmpty)
		{
			sb.Append(", ");
		}
		sb.Append(cleanedPart2);
		code.ZZW_Description = sb.ToString();

		return;

		static ReadOnlySpan<char> CleanSpan(ReadOnlySpan<char> span)
		{
			var result = new StringBuilder(span.Length);
			bool inSpace = false;
			int i = 0;

			while (i < span.Length)
			{
				if (char.IsWhiteSpace(span[i]))
				{
					if (!inSpace)
					{
						result.Append(span[i]);
						inSpace = true;
					}
					i++;
				}
				else if (span.Slice(i).StartsWith("<P>"))
				{
					i += 3;
				}
				else
				{
					result.Append(span[i]);
					inSpace = false;
					i++;
				}
			}

			return result.ToString().AsSpan().Trim().Trim(",");
		}
	}

	static void DefaultVatCode(RefCusTariffNationalCode code)
	{
		if (string.IsNullOrEmpty(code.ZZW_ZZF_NKTaxOrFeeCode))
		{
			code.ZZW_ZZF_NKTaxOrFeeCode = DeTariffsConstants.DefaultVatCode;
		}
	}

	static IEnumerable<RefCusTariffNationalCode> DeduplicateContinuousRangesWithoutChange(IEnumerable<RefCusTariffNationalCode> ranges)
	{
		ranges = ranges.OrderBy(e => e.ZZW_StartDate);

		var slidingWindow = ranges.Zip(ranges.Skip(1).Append(null));


		foreach (var (current, next) in slidingWindow)
		{
			if (next is not null &&
				current.ZZW_ZZF_NKTaxOrFeeCode == next.ZZW_ZZF_NKTaxOrFeeCode &&
				current.ZZW_Description == next.ZZW_Description &&
				current.ZZW_EndDate.AddDays(1) == next.ZZW_StartDate)
			{
				next.ZZW_StartDate = current.ZZW_StartDate;
			}
			else
			{
				yield return current;
			}
		}
	}

	static void PartitionByDescriptionChange(TariffNationalCode tariffNationalCode, List<RefCusTariffNationalCode> ranges)
	{
		foreach (var description in tariffNationalCode.Description.OrderBy(e => e.StartDate).ThenBy(e => e.EndDate))
		{
			var rangeContainingStart = ranges
				.FirstOrDefault(r => description.StartDate > r.ZZW_StartDate && description.StartDate.AddDays(1) < r.ZZW_EndDate);

			if (rangeContainingStart is not null)
			{
				ranges.Add(new RefCusTariffNationalCode
				{
					ZZW_NationalCode = tariffNationalCode.NationalCode,
					ZZW_ZZF_NKTaxOrFeeCode = rangeContainingStart.ZZW_ZZF_NKTaxOrFeeCode,
					ZZW_StartDate = description.StartDate,
					ZZW_EndDate = rangeContainingStart.ZZW_EndDate,
					ZZW_Description = description.Description,
				});
				rangeContainingStart.ZZW_EndDate = description.StartDate;
			}

			var rangeContainingEnd = ranges.FirstOrDefault(r =>
				description.EndDate > r.ZZW_StartDate.AddDays(1) && description.EndDate < r.ZZW_EndDate);
			if (rangeContainingEnd is not null)
			{
				rangeContainingEnd.ZZW_Description = description.Description;
				ranges.Add(new RefCusTariffNationalCode
				{
					ZZW_NationalCode = tariffNationalCode.NationalCode,
					ZZW_ZZF_NKTaxOrFeeCode = rangeContainingEnd.ZZW_ZZF_NKTaxOrFeeCode,
					ZZW_StartDate = description.EndDate.AddDays(1),
					ZZW_EndDate = rangeContainingEnd.ZZW_EndDate,
				});
				rangeContainingEnd.ZZW_EndDate = description.EndDate;
			}

			foreach (var rangeWithin in ranges.Where(r =>
						description.StartDate <= r.ZZW_StartDate && description.EndDate >= r.ZZW_EndDate))
			{
				rangeWithin.ZZW_Description = description.Description;
			}
		}
	}

	static void PartitionByVatChange(TariffNationalCode range, List<RefCusTariffNationalCode> ranges)
	{
		foreach (var vatCode in range.VatCode.OrderBy(e => e.StartDate).ThenBy(e => e.EndDate))
		{
			var rangeContainingStart = ranges
				.FirstOrDefault(r => vatCode.StartDate > r.ZZW_StartDate && vatCode.StartDate.AddDays(1) < r.ZZW_EndDate);

			if (rangeContainingStart is not null)
			{
				ranges.Add(new RefCusTariffNationalCode
				{
					ZZW_NationalCode = range.NationalCode,
					ZZW_Description = rangeContainingStart.ZZW_Description,
					ZZW_ZZF_NKTaxOrFeeCode = vatCode.VatCode,
					ZZW_StartDate = vatCode.StartDate,
					ZZW_EndDate = rangeContainingStart.ZZW_EndDate,
				});
				rangeContainingStart.ZZW_EndDate = vatCode.StartDate;
			}

			var rangeContainingEnd = ranges.FirstOrDefault(r =>
				vatCode.EndDate > r.ZZW_StartDate.AddDays(1) && vatCode.EndDate < r.ZZW_EndDate);

			if (rangeContainingEnd is not null)
			{
				rangeContainingEnd.ZZW_ZZF_NKTaxOrFeeCode = vatCode.VatCode;
				ranges.Add(new RefCusTariffNationalCode
				{
					ZZW_NationalCode = range.NationalCode,
					ZZW_Description = rangeContainingEnd.ZZW_Description,
					ZZW_StartDate = vatCode.EndDate.AddDays(1),
					ZZW_EndDate = rangeContainingEnd.ZZW_EndDate,
				});
				rangeContainingEnd.ZZW_EndDate = vatCode.EndDate;
			}

			foreach (var rangeWithin in ranges.Where(r =>
						vatCode.StartDate <= r.ZZW_StartDate && vatCode.EndDate >= r.ZZW_EndDate))
			{
				rangeWithin.ZZW_ZZF_NKTaxOrFeeCode = vatCode.VatCode;
			}
		}
	}

	static IEnumerable<TariffNationalCode> ParseAsNationalCodes(IEnumerable<XElement> elements)
	{
		foreach (var element in elements)
		{
			yield return TariffNationalCode.Parse(element);
		}
	}

	static IEnumerable<TariffNationalCodeDescription> ParseAsNationalCodesDescription(IEnumerable<XElement> elements)
	{
		foreach (var element in elements)
		{
			yield return TariffNationalCodeDescription.Parse(element);
		}
	}

	static IEnumerable<TariffVatCode> ParseAsVatCode(IEnumerable<XElement> elements)
	{
		foreach (var element in elements)
		{
			if(TariffVatCode.TryParse(element, out var vatCode))
			{
				yield return vatCode;
			}
		}
	}

	IEnumerable<XElement> GetXmlElements(string fileName)
	{
		var settings = new XmlReaderSettings();

		using var reader = XmlReader.Create(fileName, settings);

		foreach (var xElement in ReadEnumerable(reader))
		{
			yield return xElement;
		}
	}

	IEnumerable<XElement> ReadEnumerable(XmlReader reader)
	{
		reader.MoveToContent();

		while (reader.Read())
		{
			switch (reader.NodeType)
			{
				case XmlNodeType.Element when reader.Name == DeTariffsConstants.SetElementName:
				{
					var element = XNode.ReadFrom(reader) as XElement;
					yield return element;
					break;
				}
				case XmlNodeType.Element when reader.Name == DeTariffsConstants.CreationDateElementName:
				{
					var newLastModified = reader.ReadElementContentAsDateTime();
					lastModified = DateTime.Compare(lastModified, newLastModified) > 0 ? lastModified : newLastModified;
					break;
				}
			}
		}
	}

	IEnumerable<object> GetXmlUpdateElements(string fileName)
	{
		var settings = new XmlReaderSettings();

		using var reader = XmlReader.Create(fileName, settings);

		reader.MoveToContent();

		while (reader.Read())
		{
			if (reader.NodeType != XmlNodeType.Element)
			{
				continue;
			}

			switch (reader.Name)
			{
				case DeTariffsConstants.NationalCodeElementName:
					foreach (var tariffNationalCode in ParseAsNationalCodes(ReadEnumerable(reader.ReadSubtree())))
					{
						yield return tariffNationalCode;
					}

					break;
				case DeTariffsConstants.NationalCodeDescriptionElementName:
					foreach (var tariffNationalCodeDescription in ParseAsNationalCodesDescription(
								ReadEnumerable(reader.ReadSubtree())))
					{
						yield return tariffNationalCodeDescription;
					}

					break;
				case DeTariffsConstants.VatCodeElementName:
					foreach (var vatCode in ParseAsVatCode(ReadEnumerable(reader.ReadSubtree())))
					{
						yield return vatCode;
					}

					break;
				case DeTariffsConstants.CreationDateElementName:
					var newLastModified = reader.ReadElementContentAsDateTime();
					lastModified = DateTime.Compare(lastModified, newLastModified) > 0
						? lastModified
						: newLastModified;

					break;
			}
		}
	}
}
