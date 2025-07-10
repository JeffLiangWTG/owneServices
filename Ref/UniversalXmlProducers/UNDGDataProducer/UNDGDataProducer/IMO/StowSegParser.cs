using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public class StowSegParser : CsvDataParser<StowSegRecord>
	{
		const string RegexSGOrSW = @"(S[A-Z][0-9]{0,2}[a-z]{0,1} (Stow)?)";
		const string RegexSGG = @"(SG[0-9]{0,2} (Stow)?).*((SGG[0-9]{0,2}[a-z]?) - .*)";
		const string RegexH = @"(H[0-9]{1,3})";

		public StowSegParser()
		{
			HeaderMap = new Dictionary<string, int>()
			{
				{ nameof(StowSegRecord.DGLPhrase), 0 },
				{ nameof(StowSegRecord.DGLText), 1 }
			};
		}

		public override IDictionary<string, int> HeaderMap { get; }

		public override StowSegRecord ParseRecord(string[] rawDataRow)
		{
			return new StowSegRecord()
			{
				DGLPhrase = rawDataRow[HeaderMap[nameof(StowSegRecord.DGLPhrase)]],
				DGLText = rawDataRow[HeaderMap[nameof(StowSegRecord.DGLText)]]
			};
		}

		public IEnumerable<StowSegRecord> Parse(string filePath)
		{
			return ParseRecords(filePath, Constants.Encodings.IMOZipFile, true);
		}

		public static IEnumerable<UNDGCommonData> GetCommonData(StowSegRecord stowSegRecord)
		{
			var text = stowSegRecord.DGLText;
			if (text.StartsWith("SG", StringComparison.Ordinal) || text.StartsWith("SW", StringComparison.Ordinal))
			{
				if (text.Contains("SGG"))
				{
					var match = Regex.Match(text, RegexSGG);
					if (match.Success)
					{
						var sggText = match.Groups[3].Value;
						yield return new UNDGCommonData
						{
							DC_Descriptor = sggText,
							DC_Index = match.Groups[4].Value,
							DC_Language = "EN",
							DC_Type = "STS"
						};
						yield return new UNDGCommonData
						{
							DC_Descriptor = stowSegRecord.DGLText,
							DC_Index = stowSegRecord.DGLPhrase,
							DC_Language = "EN",
							DC_Type = "STS"
						};
					}
				}
				else
				{
					var match = Regex.Match(text, RegexSGOrSW);
					if (match.Success)
					{
						yield return new UNDGCommonData
						{
							DC_Descriptor = stowSegRecord.DGLText,
							DC_Index = stowSegRecord.DGLPhrase,
							DC_Language = "EN",
							DC_Type = "STS"
						};
					}
				}
			}
			else if (text.StartsWith("H", StringComparison.Ordinal))
			{
				var match = Regex.Match(text, RegexH);
				if (match.Success)
				{
					yield return new UNDGCommonData
					{
						DC_Descriptor = stowSegRecord.DGLText,
						DC_Index = stowSegRecord.DGLPhrase,
						DC_Language = "EN",
						DC_Type = "STS"
					};
				}
			}
			else
			{
				yield return new UNDGCommonData
				{
					DC_Descriptor = stowSegRecord.DGLText,
					DC_Index = stowSegRecord.DGLPhrase,
					DC_Language = "EN",
					DC_Type = "STS"
				};
			}
		}
	}
}
