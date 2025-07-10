using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public abstract class RefCusCodeListParser<T> where T : ICodeList
	{
		public string ConvertCodeListToXML()
		{
			logBuilder = new StringBuilder();
			AddLog("Start processing.");

			var codeLists = GetCodeLists();
			var refCusCodeLists = new List<RefCusCodeList>();
			foreach (var codeList in codeLists)
			{
				var refCusCodeList = new RefCusCodeList()
				{
					ZZD_Code = codeList.Code,
					ZZD_Description = codeList.Description
				};

				if (codeList is IDateProvider dateProvider)
				{
					refCusCodeList.ZZD_StartDate = dateProvider.StartDate;
					refCusCodeList.ZZD_EndDate = dateProvider.EndDate;
				}

				if (codeList is ICodeListAttributesProvider codeListAttributesProvider)
				{
					var refCusCodeListAttributes = new List<RefCusCodeListAttribute>();
					if (codeListAttributesProvider.Attributes != null)
					{
						foreach (var attribute in codeListAttributesProvider.Attributes)
						{
							var refCusCodeListAttribute = new RefCusCodeListAttribute()
							{
								ZZE_ZXE_NKName = attribute.Name,
								ZZE_Value = attribute.Value
							};

							refCusCodeListAttributes.Add(refCusCodeListAttribute);
						}

						refCusCodeList.RefCusCodeListAttributes = refCusCodeListAttributes.ToArray();
					}
				}

				refCusCodeLists.Add(refCusCodeList);
			}

			try
			{
				var writer = new XmlWriter(XmlWriterConfiguration);
				writer.SetDataSource(DataSourse);
				writer.SetPublicationTime(PublicationDateTime);
				writer.SetUpdateType(UpdateType);

				var dirName = Path.GetDirectoryName(OutputFilePath);
				Directory.CreateDirectory(dirName);
				if (refCusCodeLists.Count > 0)
				{
					foreach (var code in refCusCodeLists)
					{
						writer.PopulateData(code);
					}
					writer.SaveXml(OutputFilePath);
					AddLog("XML file generation successful.");
				}
				else
				{
					AddLog("The number of RefCusCodeList is 0, unable to generate XML file.");
				}
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine("XML file generation failed: " + ex.ToString());
			}

			AddLog("Processing end...");
			var result = logBuilder.ToString();
			logBuilder = null;
			return result;
		}

		protected abstract XmlWriterConfiguration XmlWriterConfiguration { get; }
		protected abstract string DataSourse { get; }
		protected abstract DateTime PublicationDateTime { get; }
		protected abstract UpdateType UpdateType { get; }
		protected abstract string OutputFilePath { get; }

		protected abstract List<T> GetCodeLists();

		protected void AddLog(string log)
		{
			logBuilder.AppendLine(log);
		}
		StringBuilder logBuilder;
	}
}
