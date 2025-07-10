using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using Microsoft.VisualBasic.FileIO;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public abstract class Parser
	{
		protected Parser(DateTime publishDate, bool isImport)
		{
			this.publishDate = publishDate;
			IsImport = isImport;
			fileName = isImport ? Constants.FileNames.ImportTariffFileName : Constants.FileNames.ExportTariffFileName;
		}

		readonly DateTime publishDate;
		readonly string fileName;

		protected bool IsImport { get; }

		protected abstract string DataSource { get; }
		protected abstract string OutputXMLFileName { get; }
		protected abstract string EntityName { get; }

		public bool Parse(string path = "")
		{
			var csvFolderPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), AppConfig.NACCS.CodeLists.JPNACCSTariffPath);

			if (!string.IsNullOrWhiteSpace(path))
			{
				csvFolderPath = path;
			}

			var filePath = Path.Combine(csvFolderPath, fileName);
			var entities = CreateEntities(filePath);

			if (entities.Any())
			{
				ExportXml(entities);
				return true;
			}

			ErrorWriter.WriteError($"No {EntityName} is parsed");
			return false;
		}

		protected abstract IEnumerable<RefDataRepoModelEntityType> CreateEntities(string filePath);

		readonly static Dictionary<char, int> RomanMap = new Dictionary<char, int>()
		{
			{'I', 1},
			{'V', 5},
			{'X', 10},
			{'L', 50},
			{'C', 100},
			{'D', 500},
		};

		protected static int RomanToInteger(string roman)
		{
			var number = 0;
			var previousChar = roman[0];
			foreach (var currentChar in roman)
			{
				number += RomanMap[currentChar];
				if (RomanMap[previousChar] < RomanMap[currentChar])
				{
					number -= RomanMap[previousChar] * 2;
				}
				previousChar = currentChar;
			}
			return number;
		}

		public void ExportXml<T>(IEnumerable<T> groups)
		{
			var writer = new XmlWriter(GetXMLConfiguration());
			writer.SetDataSource(DataSource);
			writer.SetUpdateType(UpdateType.Full);
			writer.SetPublicationTime(publishDate);

			foreach (var group in groups)
			{
				writer.PopulateData(group);
			}

			writer.SaveXml(Path.Combine(AppConfig.Shared.OutputDirectory, OutputXMLFileName));
		}

		protected abstract XmlWriterConfiguration GetXMLConfiguration();
	}
}
