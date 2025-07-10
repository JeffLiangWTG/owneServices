using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.xTMessaging.Integration;

namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public class ILCustomsCodesProcessor : BaseCustomsResponseProcessor
	{
		public ILCustomsCodesProcessor(ILogger logger)
			: base(logger)
		{
			this.logger = logger;
		}

		protected override void StoreDataSet(DateTime publicationDate, string tableName, string dataSet)
		{
			var dataCollections = GetDataCollection(tableName, dataSet);

			foreach (var dataCollection in dataCollections)
			{
				StoreDataCollection(dataCollection.Key, publicationDate, dataCollection.Value);
			}
		}

		static RefCusCodeList GetElement(XElement codeElement)
			=> new RefCusCodeList()
			{
				ZZD_Code = codeElement.Element("ID").Value.Trim(),
				ZZD_Description = codeElement.Element("Name").Value.Trim(),
				ZZD_StartDate = UniversalDataHelper.MinimumDateTime,
				ZZD_EndDate = UniversalDataHelper.MaximumDateTime,
			};

		static IXmlWriterConfiguration GetXmlWriterConfiguration(string dataSourceName)
		{
			var xmlWriterConfiguration = new XmlWriterConfiguration();

			var codeList = new EntityTypeConfiguration<RefCusCodeList>(true);
			codeList.IncludeColumn(x => x.ZZD_Code, true);
			codeList.IncludeColumn(x => x.ZZD_Description, false);
			codeList.IncludeColumn(x => x.ZZD_StartDate, false);
			codeList.IncludeColumn(x => x.ZZD_EndDate, false);
			codeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, dataSourceName);
			codeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.Israel);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(codeList);

			return xmlWriterConfiguration;
		}

		static Dictionary<string, List<RefCusCodeList>> GetDataCollection(string tableName, string dataSet)
		{
			var result = new Dictionary<string, List<RefCusCodeList>>();

			using (StringReader sr = new StringReader(dataSet))
			{
				XDocument xdoc = XDocument.Load(sr);

				var codeList = xdoc.Root.Elements("Table");
				foreach (var codeElement in codeList)
				{
					var state = codeElement.Element("State").Value.Trim();
					if (state != UniversalDataHelper.Constants.ValidState)
					{
						continue;
					}

					var mappedCodeTypes = CustomTableNameToCodeTypeMapping.GetCodeType(tableName, codeElement);

					foreach (var dataSetName in mappedCodeTypes)
					{
						if (!result.ContainsKey(dataSetName))
						{
							result[dataSetName] = new List<RefCusCodeList>();
						}
						result[dataSetName].Add(GetElement(codeElement));
					}
				}
			}

			return result;
		}

		void StoreDataCollection(string dataSourceName, DateTime publicationDate, List<RefCusCodeList> dataCollection)
		{
			RefDataRepoFileWriter<RefCusCodeList>.StoreDataCollection(dataSourceName, publicationDate, GetSafeOutputFilePath(dataSourceName), GetXmlWriterConfiguration(dataSourceName), dataCollection);
			logger.Log(LogType.Information, $"Processing {dataSourceName} done.");
		}

		readonly ILogger logger;
	}
}
