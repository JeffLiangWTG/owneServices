using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BEReferenceData.Services
{
	public static class UCCCodeListHelper
	{
		public static IEnumerable<T> Merge<T>(IEnumerable<IEnumerable<T>> sourceLists, Func<T, object> getMergeKey, Func<IEnumerable<T>, T> getMerged) where T : Common.UniversalXmlWriter.RefDataRepoModelEntityType
		{
			return sourceLists.SelectMany(l => l).GroupBy(item => getMergeKey(item)).Select(group => getMerged(group));
		}

		public static RefCusCodeList MergeRefCusCodeList(IEnumerable<RefCusCodeList> list)
		{
			var result = list.FirstOrDefault();
			result.RefCusCodeListAttributes = Merge(
				list.Select(codeList => codeList.RefCusCodeListAttributes),
				attr => (attr.ZZE_ZXE_NKName, attr.ZZE_Value),
				dupAttrs => dupAttrs.FirstOrDefault()
				).ToArray();
			return result;
		}

		public static IExtractedUCCCodeList ExtractUCCCodeListFile(string zipPathName, IUCCCodeListDetails codeList, string destinationDir)
		{
			using (var archive = ZipFile.Open(zipPathName, ZipArchiveMode.Read))
			{
				if (archive.Entries.Any(e => e.Name.ToUpperInvariant().EndsWith(".XML", StringComparison.InvariantCulture)))
				{
					var entry = archive.Entries.First();
					var fileExtractedPath = Path.Combine(destinationDir, entry.Name);

					Directory.CreateDirectory(destinationDir);
					entry.ExtractToFile(fileExtractedPath, true);

					return new ExtractedUCCCodeListProvider(codeList.CodeType, fileExtractedPath, DateTime.Now, codeList.DataSource, codeList.AttributeValues);
				}
			}
			return null;
		}

		public static Dictionary<string, string> ExtractCodeListFiles(string zipPathName, string[] codeListNames, string destinationDir)
		{
			var result = new Dictionary<string, string>();

			using (var archive = ZipFile.Open(zipPathName, ZipArchiveMode.Read))
			{
				foreach (var code in codeListNames)
				{
					var entries = archive.Entries.Where(e => e.Name.ToUpper(CultureInfo.InvariantCulture).StartsWith(code, StringComparison.InvariantCulture) && e.Name.ToUpperInvariant().EndsWith(".ZIP", StringComparison.InvariantCulture)).ToList();

					if (entries.Count == 1)
					{
						var entry = entries[0];
						using (var zip = new ZipArchive(entry.Open()))
						{
							var nestedEntry = zip.Entries.First();
							var fileExtractedPath = Path.Combine(destinationDir, $"{code}_{nestedEntry.Name}");

							Directory.CreateDirectory(destinationDir);
							nestedEntry.ExtractToFile(fileExtractedPath, true);

							result[code] = fileExtractedPath;
						}
					}
					else if (entries.Count == 0)
					{
						throw new IOException($"Invalid zip file content: Code list zip file starting with '{code}' not found.");
					}
					else
					{
						throw new IOException($"Invalid zip file content: Code list zip file starting with '{code}' found more than one time.");
					}
				}
			}
			return result;
		}

		public static List<RefCusCodeList> ReadCodeListCodesXmlIntoResults(string xmlFileName, string codeType, List<(string attributeName, string attributeValue)> attributeValues)
		{
			var outputCodeList = new List<RefCusCodeList>();
			var inputDoc = new XDocument();

			using (var reader = File.OpenRead(xmlFileName))
			{
				inputDoc = XDocument.Load(reader);

				var entries = from e in inputDoc.Descendants()
							  where e.Name.LocalName == "RDEntry"
							  select e;

				foreach (var entry in entries)
				{
					var statusElem = from s in entry.Elements().Descendants()
									 where s.Name.LocalName == "state"
									 select s;
					var status = statusElem.First().Value;

					if (status == "valid")
					{
						var validFromElem = from v in entry.Elements().Descendants()
											where v.Name.LocalName == "activeFrom"
											select v;
						var validFrom = validFromElem.First().Value;

						var dataItemElem = from d in entry.Elements()
										   where d.Name.LocalName == "dataItem"
										   select d;
						var codeName = dataItemElem.First().Value;

						var descListElem = from l in entry.Elements()
										   where l.Name.LocalName == "LsdList"
										   select l;

						const string descElemName = "description";
						var descEnElem = from ds in descListElem.Elements()
										 where ds.Name.LocalName == descElemName && (string)ds.Attribute("lang") == "en"
										 select ds;
						var descEn = descEnElem.FirstOrDefault()?.Value;

						var descNlElem = from ds in descListElem.Elements()
										 where ds.Name.LocalName == descElemName && (string)ds.Attribute("lang") == "nl"
										 select ds;
						var descNl = descNlElem.FirstOrDefault()?.Value;

						var descFrElem = from ds in descListElem.Elements()
										 where ds.Name.LocalName == descElemName && (string)ds.Attribute("lang") == "fr"
										 select ds;
						var descFr = descFrElem.FirstOrDefault()?.Value;

						var descDeElem = from ds in descListElem.Elements()
										 where ds.Name.LocalName == descElemName && (string)ds.Attribute("lang") == "de"
										 select ds;
						var descDe = descDeElem.FirstOrDefault()?.Value;

						var nctsCode = new RefCusCodeList
						{
							ZZD_Code = codeName,
							ZZD_ZZK_NKCodeType = codeType,
							ZZD_ZZZ_NKDataGrouping = ServiceConstants.Common.LocalCountryCode,
							ZZD_StartDate = DateTime.ParseExact(validFrom, "yyyy-MM-dd", CultureInfo.InvariantCulture),
							ZZD_Description = descEn ?? descNl ?? descFr ?? descDe
						};

						var refCusCodeListLanguages = new List<RefCusCodeListLanguage>();
						if (!string.IsNullOrEmpty(descNl))
						{
							refCusCodeListLanguages.Add(
								new RefCusCodeListLanguage
								{
									ZXA_ZX6_NKLanguage = "NL",
									ZXA_Description = descNl
								});
						}

						if (!string.IsNullOrEmpty(descFr))
						{
							refCusCodeListLanguages.Add(
								new RefCusCodeListLanguage
								{
									ZXA_ZX6_NKLanguage = "FR",
									ZXA_Description = descFr
								});
						}

						if (!string.IsNullOrEmpty(descDe))
						{
							refCusCodeListLanguages.Add(
								new RefCusCodeListLanguage
								{
									ZXA_ZX6_NKLanguage = "DE",
									ZXA_Description = descDe
								});
						}
						nctsCode.RefCusCodeListLanguages = refCusCodeListLanguages.ToArray();

						nctsCode.RefCusCodeListAttributes = attributeValues.Select(av => new RefCusCodeListAttribute
						{
							ZZE_ZXE_NKName = av.attributeName,
							ZZE_Value = av.attributeValue
						}).ToArray();

						outputCodeList.Add(nctsCode);
					}
				}
			}

			return outputCodeList;
		}

		public static DateTime GetPublicationDateTime(IEnumerable<string> fileNames)
		{
			return fileNames.Select(x => GetPublicationDateTime(x, DateTime.Now)).Max();
		}

		static DateTime GetPublicationDateTime(string filename, DateTime fallbackDateTime)
		{
			var tokens = filename.Split('_');
			var result = fallbackDateTime;

			if (tokens != null && tokens.Length >= 4)
			{
				DateTime publicationDateTime;
				if (DateTime.TryParseExact(tokens[3], "[yyyy-MM-dd\\'H\\hm]", CultureInfo.InvariantCulture, DateTimeStyles.None, out publicationDateTime))
				{
					result = publicationDateTime;
				}
			}
			return result;
		}
	}
}
