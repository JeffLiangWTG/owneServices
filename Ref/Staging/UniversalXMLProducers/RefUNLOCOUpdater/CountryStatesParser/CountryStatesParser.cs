using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Odbc;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater
{
	public class CountryStatesParser
	{
		public CountryStatesParser(DateTime sourceDate, string outputFilePath, string mdbFilePath)
		{
			Argument.NotNullOrEmpty(outputFilePath, nameof(outputFilePath));
			Argument.NotNullOrEmpty(mdbFilePath, nameof(mdbFilePath));

			_sourceDate = sourceDate;
			_outputFilePath = outputFilePath;
			_mdbFilePath = mdbFilePath;
		}

		public void GenerateXml()
		{
			var configuration = GetWriterConfiguration();
			var writer = new XmlWriter(configuration);
			writer.SetDataSource("UN Country States");
			writer.SetPublicationTime(_sourceDate);
			writer.SetUpdateType(UpdateType.Full);

			var stateCodeDictionary = GetStateCodesDictionary();
			foreach (var record in GetCountryStates())
			{
				var countryState = new RefCountryStates()
				{
					RW_Code = record.Code,
					RW_RN_NKCountryCode = record.Country,
					RW_Description = record.Name.Length > 35 ? FormatDescription(record.Name) : record.Name,
					RW_IsActive = true
				};

				if (record.Country.Equals("IN", StringComparison.OrdinalIgnoreCase))
				{
					var description = countryState.RW_Description;
					countryState.RW_Description = stateCodeDictionary.ContainsKey(record.Code) ? $"{stateCodeDictionary[record.Code]} {description}" : description;
				}
				writer.PopulateData(countryState);
			}

			writer.SaveXml(_outputFilePath);
		}

		static string FormatDescription(string description)
		{
			Argument.NotNullOrEmpty(description, nameof(description));
			var result = description;
			if (result.Length > 35)
			{
				result = Regex.Match(description, TextInSquareBracketsRegex).Value;
				if (!string.IsNullOrEmpty(result) && !(result.Length > 35))
				{
					return result;
				}
				result = Regex.Match(description, TextBeforeLeftBracketRegex).Value;
				if (!string.IsNullOrEmpty(result) && !(result.Length > 35))
				{
					return result;
				}
			}
			result = Regex.Match(description, TextBeforeDashOrCommaRegex).Value;
			if (!string.IsNullOrEmpty(result))
			{
				return result;
			}
			return description.Length > 35 ? description.Substring(0, 35) : description;
		}

		const string TextInSquareBracketsRegex = @"(?<=\[).+?(?=\])";
		const string TextBeforeDashOrCommaRegex = @".+?(?=( -)|(,))";
		const string TextBeforeLeftBracketRegex = @".+?(?=(\s?\())";

		IEnumerable<CountryState> GetCountryStates()
		{
			var result = new List<CountryState>();
			using (var conn = OdbcConnectionHelper.GetOdbcConnection(_mdbFilePath))
			{
				conn.Open();
				using (var cmd = new OdbcCommand("Select SUCountry,SUCode,SUName From SubdivisionCodes", conn))
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.HasRows)
					{
						while (reader.Read())
						{
							result.Add(new CountryState()
							{
								Country = reader.GetString(0)?.Trim(), //SUCountry
								Code = reader.GetString(1)?.Trim(), //SUCode
								Name = reader.GetString(2)?.Trim() //SUName
							});
						}
						reader.NextResult();
					}
				}
			}
			return result;
		}

		static XmlWriterConfiguration GetWriterConfiguration()
		{
			var countryStatesConfiguration = new EntityTypeConfiguration<RefCountryStates>(true);
			countryStatesConfiguration.IncludeColumn(x => x.RW_Code, true);
			countryStatesConfiguration.IncludeColumn(x => x.RW_Description, false, IsDataValue.False);
			countryStatesConfiguration.IncludeColumn(x => x.RW_RN_NKCountryCode, true);
			countryStatesConfiguration.IncludeColumn(x => x.RW_IsActive, false, IsDataValue.False);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(countryStatesConfiguration);
			return writerConfiguration;
		}

		static Dictionary<string, string> GetStateCodesDictionary()
		{
			var stateCodesDictionary = new Dictionary<string, string>();
			var contents = File.ReadAllLines(ConfigurationProvider.StateCodeListFile);
			for (var line = 1; line < contents.Length; line++)
			{
				var content = contents[line];
				if (string.IsNullOrEmpty(content))
				{
					continue;
				}
				var codes = content.Split(',');
				var stateCode = codes[0];
				var numericCode = codes[1];
				stateCodesDictionary[stateCode] = numericCode;
			}

			return stateCodesDictionary;
		}

		readonly string _mdbFilePath;
		readonly DateTime _sourceDate;
		readonly string _outputFilePath;
	}
}
