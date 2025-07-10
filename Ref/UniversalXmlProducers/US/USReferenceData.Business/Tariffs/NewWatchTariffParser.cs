using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public class NewWatchTariffParser
	{
		public NewWatchTariffParser(string inputFileFullName, string outputFileName, DateTime publicationTime)
		{
			this.inputFileFullName = inputFileFullName;
			this.outputFileName = outputFileName;
			this.publicationTime = publicationTime;
		}

		readonly string inputFileFullName;
		readonly string outputFileName;
		readonly DateTime publicationTime;

		const string attributeName = "TYPE";
		const string attributeValue = "301CN";
		const string desctriptionForNewNumber = "New Watch Rules";

		public string ParseToXMLFile()
		{
			var sBuilder = new StringBuilder();
			var watchTariffs = File.ReadLines(inputFileFullName);
			var refCusTarriffs = new List<RefCusTariff>();

			try
			{
				foreach (var tariff in watchTariffs)
				{
					var attrList = new List<RefCusTariffAttribute>();
					var attr = new RefCusTariffAttribute { ZZ3_Name = attributeName, ZZ3_Value = attributeValue };
					attrList.Add(attr);

					var refCusTariff = new RefCusTariff()
					{
						ZZ1_TariffCode = tariff.Replace(".", "").Trim(),
						ZZ1_Description = desctriptionForNewNumber,
						RefCusTariffAttributes = attrList.ToArray()
					};
					refCusTarriffs.Add(refCusTariff);
				}
				XmlWriterHelper.ExportToXMLFile("US New Watch Tariff Rule", outputFileName, XmlWriterHelper.GetNewWatchTariffWriterConfiguration(attributeName, attributeValue), publicationTime, refCusTarriffs, UpdateType.Partial);
			}
			catch
			{
				sBuilder.AppendLine("Error occured in ExportToXMLFile.");
			}

			return sBuilder.ToString();
		}
	}
}
