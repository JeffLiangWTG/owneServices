using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.ManualProcessor
{
	public class CACDocumentTypesParser

	{
		public CACDocumentTypesParser(string exportFilePath, DateTime publicationTime, string workingFileOrDirectory, StringBuilder errorBuilder)
		{
			_exportFilePath = exportFilePath;
			_publicationTime = publicationTime;
			_workingFileOrDirectory = workingFileOrDirectory;
			_errorBuilder = errorBuilder;

			_xmlWriter = new XmlWriter(XMLWriterHelper.GetRefCusCodelistConfiguration(Constants.CodeType.CADOC, true));
			_xmlWriter.SetDataSource(Constants.DataSource.DocumentTypes);
			_xmlWriter.SetUpdateType(UpdateType.Full);
		}

		readonly string _exportFilePath;
		readonly DateTime _publicationTime;
		readonly string _workingFileOrDirectory;
		readonly StringBuilder _errorBuilder;
		readonly IXmlWriter _xmlWriter;
		bool IsSuccessful = true;

		public void ParseCusCodeListIntoXML()
		{
			try
			{
				var refCusCodeList = GetCADOCRefCusCodeList();

				if (IsSuccessful && refCusCodeList.Any())
				{
					Console.WriteLine($"There will be {refCusCodeList.Count} records populated into the final XML");

					foreach (var codeList in refCusCodeList)
					{
						_xmlWriter.PopulateData(codeList);
					}
					_xmlWriter.SetPublicationTime(_publicationTime);
					_xmlWriter.SaveXml(_exportFilePath);

					Console.WriteLine($"Final XML populated {_exportFilePath}.");
				}
				else
				{
					_errorBuilder.AppendLine("Not populate successfully, please verify if the file contains any content or check the missing data.");
				}
			}
			catch (Exception ex)
			{
				_errorBuilder.AppendLine($"Error processing CA CDocument Types data :{System.Environment.NewLine}{ex.Message}");
			}
		}

		List<RefCusCodeList> GetCADOCRefCusCodeList()
		{
			var result = new List<RefCusCodeList>();

			int line = 1;
			var cACDocumentTypesList = CsvLoader.GetCADocumentTypesList(_workingFileOrDirectory);
			foreach (var caDocumentType in cACDocumentTypesList)
			{
				line++;
				var govAgencyIDCode = caDocumentType.GovAgencyIDCode;
				var code = caDocumentType.Code;
				var desc = caDocumentType.Desc;
				var isDefaultRefNum = caDocumentType.IsDefaultRefNum;

				if (!string.IsNullOrEmpty(govAgencyIDCode) && !string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(desc) && !string.IsNullOrEmpty(isDefaultRefNum))
				{
					if (code.Length == 4)
					{
						var refCusCodeListAttributeList = new List<RefCusCodeListAttribute>();
						var attributeForPGA = new RefCusCodeListAttribute()
						{
							ZZE_ZXE_NKName = Constants.CADocumentTypes.PGA_Name,
							ZZE_Value = govAgencyIDCode
						};
						refCusCodeListAttributeList.Add(attributeForPGA);

						if (code == Constants.CADocumentTypes.Code2006)
						{
							var attributeForCode2006GIP80 = new RefCusCodeListAttribute()
							{
								ZZE_ZXE_NKName = Constants.CADocumentTypes.ValuesAllowed_Name,
								ZZE_Value = Constants.CADocumentTypes.GIP80_Value
							};
							refCusCodeListAttributeList.Add(attributeForCode2006GIP80);

							var attributeForCode2006GIP81 = new RefCusCodeListAttribute()
							{
								ZZE_ZXE_NKName = Constants.CADocumentTypes.ValuesAllowed_Name,
								ZZE_Value = Constants.CADocumentTypes.GIP81_Value
							};
							refCusCodeListAttributeList.Add(attributeForCode2006GIP81);
						}
						else
						{
							if (isDefaultRefNum == Constants.CADocumentTypes.Yes)
							{
								var attributeForXXX = new RefCusCodeListAttribute()
								{
									ZZE_ZXE_NKName = Constants.CADocumentTypes.ValuesAllowed_Name,
									ZZE_Value = Constants.CADocumentTypes.XXX_Value
								};
								refCusCodeListAttributeList.Add(attributeForXXX);
							}
						}

						var refCusCodeList = new RefCusCodeList()
						{
							ZZD_ZZK_NKCodeType = Constants.CodeType.CADOC,
							ZZD_Code = code,
							ZZD_Description = desc,
							RefCusCodeListAttributes = refCusCodeListAttributeList.ToArray()
						};

						result.Add(refCusCodeList);
					}
				}
				else
				{
					IsSuccessful = false;
					_errorBuilder.AppendLine($"Missing data in line {line}.");
				}
				
			}

			return result;
		}
	}
}
