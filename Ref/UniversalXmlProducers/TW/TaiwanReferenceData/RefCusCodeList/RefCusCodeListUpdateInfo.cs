using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using HtmlAgilityPack;
using static CargoWise.RefDbRepo.TaiwanReferenceData.OpenXmlHelper;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public class RefCusCodeListUpdateInfo : RefDataUpdateInfo
	{
		public RefCusCodeListUpdateInfo(string codeType, string filename, DateTime publicationDateTime) : base(null, filename)
		{
			this.codeType = codeType;
			this.publicationDateTime = publicationDateTime;
		}

		public RefCusCodeListUpdateInfo(string codeType, string filename, DateTime publicationDateTime, string htmlString) : this(codeType, filename, publicationDateTime)
		{
			this.htmlString = htmlString;
		}

		public RefCusCodeListUpdateInfo(string codeType, string filename, byte[] downloadedData, DateTime publicationDateTime) : this(codeType, filename, publicationDateTime)
		{
			downloadedDatas = new List<byte[]>() { Argument.NotNull(downloadedData, nameof(downloadedData)) };
		}

		public RefCusCodeListUpdateInfo(string codeType, string filename, IEnumerable<byte[]> downloadedDatas, DateTime publicationDateTime) : this(codeType, filename, publicationDateTime)
		{
			this.downloadedDatas = Argument.NotNull(downloadedDatas, nameof(downloadedDatas));
		}

		readonly string codeType;
		readonly DateTime publicationDateTime;
		readonly IEnumerable<byte[]> downloadedDatas;
		readonly string htmlString;

		protected List<RefCusCodeList> RefCusCodeLists
		{
			get
			{
				if (refCusCodeLists == null)
				{
					refCusCodeLists = new List<RefCusCodeList>();
				}
				return refCusCodeLists;
			}
		}
		List<RefCusCodeList> refCusCodeLists;

		string DataSource
		{
			get
			{
				switch (codeType)
				{
					case CodeTypes.RejectionReason:
						return "Rejection Reason";
					case CodeTypes.RequiredFormalities:
						return "Required formalities";
					case CodeTypes.TaiwanCustomsRequirements:
						return "Taiwan Customs Requirements";
					case CodeTypes.TaiwanImportRegulations:
						return "Taiwan Import Regulations";
					case CodeTypes.TaiwanExportRegulations:
						return "Taiwan Export Regulations";
					case CodeTypes.TaiwanPackingHouse:
						return "Taiwan Packing House";
					case CodeTypes.TaiwanUnitsOfMeasurement:
						return "Taiwan Units of Measurement";
					case CodeTypes.TaiwanSCECA:
						return "TW Special Codes for Exemption of Controlling Agencies";
					case CodeTypes.TaiwanControllingAgency:
						return "Taiwan Control Agency";
					case CodeTypes.TaiwanCustomsOffices:
						return "Taiwan Customs Office";
					case CodeTypes.TaiwanAircraftPartCAACodeCategory:
						return "Taiwan Aircraft Part CAA Code Category";
					case CodeTypes.TaiwanAircraftPartCAACCode:
						return "Taiwan Aircraft Part CAAC Code";
					case CodeTypes.TaiwanCommodityInspection:
						return "TW Commodity Inspection";
					case CodeTypes.FacilityCode:
						return "TW Facility Code";
					case CodeTypes.TWCPT016RejectionReason:
						return "TWCPT_016_Rejection Reason";
					case CodeTypes.TWCPT_017_Error_DocumentOrRequiredFormalities:
						return "TWCPT_017_Error Document or Required Formalities";
					case CodeTypes.CPT018ResponseToWarehouse:
						return "TWCPT_018_Response to Warehouse/Carrier or Document Required Notice";
					default:
						return string.Empty;
				}
			}
		}

		public string ExcelPath
		{
			get
			{
				if (string.IsNullOrEmpty(excelPath))
				{
					switch (codeType)
					{
						case CodeTypes.RejectionReason:
							excelPath = "Rejection Reason.xlsx";
							break;
						case CodeTypes.RequiredFormalities:
							excelPath = "Required formalities.xlsx";
							break;
						case CodeTypes.TaiwanCustomsRequirements:
							excelPath = "Taiwan Customs Requirements.xlsx";
							break;
					}
				}
				return excelPath;
			}
			set
			{
				excelPath = value;
			}
		}
		string excelPath;

		string GetValueAsString(object obj)
		{
			return obj == null ? string.Empty : obj.ToString();
		}

		protected override void Execute()
		{
			XmlWriterConfiguration xmlWriterConfig = null;

			switch (codeType)
			{
				case CodeTypes.RejectionReason:
				case CodeTypes.RequiredFormalities:
					PopulateCusCodeListsForRejectionReasonOrRequiredFormalities();
					break;
				case CodeTypes.TaiwanCustomsRequirements:
					PopulateCusCodeListsForTaiwanCustomsRequirements();
					break;
				case CodeTypes.TaiwanPackingHouse:
					PopulateCusCodeListsForTaiwanPackingHouse();
					break;
				case CodeTypes.TaiwanUnitsOfMeasurement:
					PopulateCusCodeListsForTaiwanUnitsOfMeasurement();
					break;
				case CodeTypes.TaiwanControllingAgency:
					PopulateCusCodeListsForTaiwanControllingAgency();
					break;
				case CodeTypes.TaiwanCustomsOffices:
					PopulateCusCodeListsForTaiwanCustomsOffices();
					break;
				case CodeTypes.TaiwanAircraftPartCAACodeCategory:
					PopulateCusCodeListsForTaiwanAircraftPartCAACodeCategory();
					break;
				case CodeTypes.TaiwanAircraftPartCAACCode:
					PopulateCusCodeListsForTaiwanAircraftPartCAACCode();
					break;
				case CodeTypes.TaiwanSCECA:
					PopulateCusCodeListsForTaiwanSCECA();
					break;
				case CodeTypes.TaiwanCommodityInspection:
					foreach (var data in downloadedDatas)
					{
						PopulateCusCodeListsForTaiwanCommodityInspection(data);
					}
					break;
				case CodeTypes.FacilityCode:
					PopulateCusCodeListsForFacilityCode();
					xmlWriterConfig = GetXmlWriterConfigForFac();
					break;
				case CodeTypes.TaiwanExportRegulations:
					PopulateCusCodeListsForTaiwanExportRegulations();
					break;
				case CodeTypes.TaiwanImportRegulations:
					PopulateCusCodeListsForTaiwanImportRegulations();
					break;
				case CodeTypes.TWCPT016RejectionReason:
				case CodeTypes.TWCPT_017_Error_DocumentOrRequiredFormalities:
					PopulateCusCodeListsForCPT016_17_18();
					break;
				case CodeTypes.CPT018ResponseToWarehouse:
					PopulateCusCodeListsForCPT016_17_18(new Regex(@"^\d{3}$"));
					break;
			}

			xmlWriterConfig ??= GetRefCusCodeListWriterConfiguration();

			WriteXmlHelper.ExportToXMLFile(Filename, DataSource, publicationDateTime, refCusCodeLists, xmlWriterConfig);
		}

		void PopulateCusCodeListsForCPT016_17_18(Regex codeRegex = null)
		{
			codeRegex ??= new Regex(@"^[A-Z]{1}\d{2}$");
			foreach (var data in downloadedDatas)
			{
				var dataTables = OdfHelper.ExtractTableToDataTables(data);
				foreach (var dataTable in dataTables)
				{
					foreach (DataRow dataRow in dataTable.Rows)
					{
						var code = (string)dataRow[0];
						if (codeRegex.IsMatch(code))
						{
							var description = (string)dataRow[1];

							var refCusCodeList = new RefCusCodeList
							{
								ZZD_Code = code.Trim(),
								ZZD_Description = description.SafeSubstring(0, 2000),
							};
							RefCusCodeLists.Add(refCusCodeList);
						}
					}
				}
			}
		}

		void PopulateCusCodeListsForTaiwanSCECA()
		{
			var parser = new PDFParser();
			foreach (var data in downloadedDatas)
			{
				var list = parser.Parse(data);
				foreach (var row in list)
				{
					var attributes = new List<RefCusCodeListAttribute>();
					var code = row.code;
					if (code != string.Empty)
					{
						attributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.ControllingAgency, ZZE_Value = code.SafeSubstring(0, 2) });
					}

					var remark = row.remark;
					if (remark != string.Empty)
					{
						attributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Remarks, ZZE_Value = remark.SafeSubstring(0, 255) });
					}

					var source = row.source;
					if (source != string.Empty)
					{
						attributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Source, ZZE_Value = source.SafeSubstring(0, 255) });
					}
					var refCusCodeList = new RefCusCodeList
					{
						ZZD_Code = row.code.Trim(),
						ZZD_Description = row.scope.SafeSubstring(0, 2000),
						RefCusCodeListAttributes = attributes.ToArray()
					};
					RefCusCodeLists.Add(refCusCodeList);
				}
			}
		}

		void PopulateCusCodeListsForTaiwanExportRegulations()
		{
			var codeRegex = new Regex("^[A-Z0-9]{3}$");
			foreach (var data in downloadedDatas)
			{
				var dataTable = CsvFileHelper.GetDataTable(data);
				foreach (DataRow row in dataTable.Rows)
				{
					var code = GetValueAsString(row[0]);
					if (codeRegex.IsMatch(code))
					{
						var refCusCodeList = new RefCusCodeList
						{
							ZZD_Code = code.Trim(),
							ZZD_Description = GetValueAsString(row[1]).SafeSubstring(0, 2000),
						};
						RefCusCodeLists.Add(refCusCodeList);
					}
				}
			}
		}

		void PopulateCusCodeListsForTaiwanImportRegulations()
		{
			var codeRegex = new Regex("^[A-Z0-9]{3}$");
			foreach (var data in downloadedDatas)
			{
				var dataTable = CsvFileHelper.GetDataTable(data);
				foreach (DataRow row in dataTable.Rows)
				{
					var code = GetValueAsString(row[0]);
					if (codeRegex.IsMatch(code))
					{
						var refCusCodeList = new RefCusCodeList
						{
							ZZD_Code = code.Trim(),
							ZZD_Description = GetValueAsString(row[1]).SafeSubstring(0, 2000),
						};
						RefCusCodeLists.Add(refCusCodeList);
					}
				}
			}
		}

		public void PopulateCusCodeListsForFacilityCode()
		{
			new FacilityCodeAnalyzer().PopulateCusCodeListsForFacilityCode(downloadedDatas, RefCusCodeLists);
		}

		void PopulateCusCodeListsForTaiwanUnitsOfMeasurement()
		{
			foreach (var data in downloadedDatas)
			{
				var dataTable = PdfHelper.ExtractTableToDataTable(data);
				var regex = new Regex(AppConfig.CodeLists.TaiwanUnitsOfMeasurement.Regex.CodeRegex);
				var dictionary = new Dictionary<string, (bool IsUntded, string Description, string ChineseDescription)>();
				foreach (DataRow row in dataTable.Rows)
				{
					var match = regex.Match(GetValueAsString(row[0]));
					if (match.Success)
					{
						var code = match.Groups[1].Value;
						if (!dictionary.ContainsKey(code))
						{
							var isUntded = match.Groups[2].Value != "*";
							var chineseDescription = GetValueAsString(row[1]).Replace("\n", string.Empty);
							var description = GetValueAsString(row[2]).Replace("\n", string.Empty);
							dictionary.Add(code, (isUntded, description, chineseDescription));
						}
					}
				}

				foreach (var kvp in dictionary)
				{
					var refCusCodeList = new RefCusCodeList
					{
						ZZD_Code = kvp.Key.Trim(),
						ZZD_Description = kvp.Value.Description.SafeSubstring(0, 2000),
						RefCusCodeListLanguages = new RefCusCodeListLanguage[]
						{
							new RefCusCodeListLanguage() { ZXA_Description = kvp.Value.ChineseDescription }
						}
					};
					RefCusCodeLists.Add(refCusCodeList);
				}
			}
		}

		void PopulateCusCodeListsForTaiwanControllingAgency()
		{
			var codeRegex = new Regex("^[A-Z0-9]{2}$");
			var annotationRegex = new Regex("[(註].*[)]");
			var fullWidthColon = "：";
			var tempDictionary = new Dictionary<string, string>();
			foreach (var data in downloadedDatas)
			{
				var dataTables = OdfHelper.ExtractTableToDataTables(data);
				var textHNodeList = OdfHelper.ExtractTextHNodeList(data);
				foreach (var dataTable in dataTables)
				{
					foreach (DataRow dataRow in dataTable.Rows)
					{
						var codesSplitByComma = ((string)dataRow[0]).Split('、');
						var agencyName = (string)dataRow[1];
						foreach (var code in codesSplitByComma)
						{
							if (codeRegex.IsMatch(code))
							{
								var description = agencyName;
								if (annotationRegex.IsMatch(agencyName))
								{
									var textHNode = textHNodeList.Cast<XmlNode>().FirstOrDefault(c => c.InnerText.Split(fullWidthColon)[0] == code);
									var codeAndRemark = textHNode.InnerText.Split(fullWidthColon);
									if (codeAndRemark.Length > 1)
									{
										description = string.Format("{0}{1}{2}", annotationRegex.Replace(description, ""), fullWidthColon, codeAndRemark[1].Trim());
									}
								}
								if (tempDictionary.ContainsKey(code))
								{
									tempDictionary[code] += "；" + description;
								}
								else
								{
									tempDictionary.Add(code, description);
								}
							}
						}
					}
				}
			}
			tempDictionary.ToList().ForEach(x => RefCusCodeLists.Add(new RefCusCodeList() { ZZD_Code = x.Key.Trim(), ZZD_Description = x.Value.SafeSubstring(0, 2000) }));
		}

		void PopulateCusCodeListsForTaiwanCustomsOffices()
		{
			var codeRegex = new Regex("^[A-Z]{2}");
			foreach (var data in downloadedDatas)
			{
				var dataTables = OdfHelper.ExtractTableToDataTables(data);
				foreach (var dataTable in dataTables)
				{
					foreach (DataRow dataRow in dataTable.Rows)
					{
						var code = (string)dataRow[0];
						if (codeRegex.IsMatch(code))
						{
							var firstCode = code.Substring(0, 1);
							var description = (string)dataRow[1];
							var transportMode = string.Empty;
							switch (firstCode)
							{
								case "A":
								case "B":
								case "D":
									transportMode = description.Contains("機場") ? "AIR" : "SEA";
									break;
								case "C":
									transportMode = "AIR";
									break;
							}

							var refCusCodeList = new RefCusCodeList
							{
								ZZD_Code = code.Trim(),
								ZZD_Description = description.SafeSubstring(0, 2000),
								RefCusCodeOrAttributeTransportModes = new RefCusCodeOrAttributeTransportMode[]
								{
									new RefCusCodeOrAttributeTransportMode() { ZZU_TransportMode = transportMode },
								}
							};
							RefCusCodeLists.Add(refCusCodeList);
						}
					}
				}
			}
		}

		void PopulateCusCodeListsForTaiwanAircraftPartCAACodeCategory()
		{
			var htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(htmlString);
			var downloadFilebaseNodes = htmlDocument.DocumentNode.SelectNodes($"//a[@class='download-filebase']");
			var codeWithDescriptionRegex = new Regex("第(?<chineseNumber>.*)類：(.*)(.(?i)odt)");
			foreach (var node in downloadFilebaseNodes)
			{
				var title = node.Attributes["title"].Value;
				var match = codeWithDescriptionRegex.Match(title);
				if (match.Success)
				{
					var matchGroup = match.Groups;
					var chineseNumber = matchGroup["chineseNumber"].Value;
					var description = title.ToUpper().Replace(".ODT", "");
					var code = Utility.ParseCnToIntString(chineseNumber);
					if (RefCusCodeLists.All(c => c.ZZD_Code != code))
					{
						var refCusCodeList = new RefCusCodeList
						{
							ZZD_Code = code.Trim(),
							ZZD_Description = description.SafeSubstring(0, 2000),
						};
						RefCusCodeLists.Add(refCusCodeList);
					}
				}
			}
		}

		void PopulateCusCodeListsForTaiwanAircraftPartCAACCode()
		{
			var numberRegex = new Regex("^\\d");
			foreach (var data in downloadedDatas)
			{
				var dataTables = OdfHelper.ExtractTableToDataTables(data);
				foreach (var dataTable in dataTables)
				{
					foreach (DataRow dataRow in dataTable.Rows)
					{
						var category = (string)dataRow[0];
						var number = (string)dataRow[1];
						var code = $"{category}.{number}";
						if (numberRegex.IsMatch(category) && numberRegex.IsMatch(number) && RefCusCodeLists.All(c => c.ZZD_Code != code))
						{
							var attributes = new List<RefCusCodeListAttribute>();
							var languages = new List<RefCusCodeListLanguage>();
							var chineseDescription = (string)dataRow[2];
							var description = (string)dataRow[3];

							if (!string.IsNullOrEmpty(chineseDescription.Trim()))
							{
								languages.Add(new RefCusCodeListLanguage() { ZXA_Description = chineseDescription });
							}

							var refCusCodeList = new RefCusCodeList
							{
								ZZD_Code = code.Trim(),
								ZZD_Description = description,
								RefCusCodeListAttributes = attributes.ToArray(),
								RefCusCodeListLanguages = languages.ToArray(),
							};
							RefCusCodeLists.Add(refCusCodeList);
						}
					}
				}
			}
		}

		void PopulateCusCodeListsForTaiwanCommodityInspection(byte[] fileData)
		{
			var document = OpenXmlHelper.GetDocument(fileData);
			var body = document.Root.Element(Namespaces.Office + "body").Elements().First();
			var tables = body.Descendants(Namespaces.Table + "table");
			var codeRegex = new Regex("^([A-Z0-9]{4})([*]?)$");
			foreach (var table in tables)
			{
				var rows = table.Descendants(Namespaces.Table + "table-row");
				foreach (var row in rows)
				{
					var cells = row.Descendants(Namespaces.Table + "table-cell");
					var codeElem = cells.ElementAt(0).Elements(Namespaces.Text + "p").LastNotEmpty();
					if (codeElem == null)
						continue;

					var code = codeElem.Value;
					var match = codeRegex.Match(code);
					if (match.Success)
					{
						var zone = cells.ElementAt(1).Elements(Namespaces.Text + "p").LastNotEmpty().Value;

						var index = RefCusCodeLists.FindIndex(m => m.ZZD_Code == code);
						if (index < 0)
						{
							var refCusCodeList = new RefCusCodeList
							{
								ZZD_Code = code.Trim(),
								ZZD_Description = zone.SafeSubstring(0, 2000),
								RefCusCodeListAttributes = new RefCusCodeListAttribute[]
							{
								new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.CUSTOMSOFFICE, ZZE_Value = code.Substring(0, 2) },
							}
							};
							RefCusCodeLists.Add(refCusCodeList);
						}
						else
						{
							RefCusCodeLists[index].ZZD_Description += $";{zone}";
						}

					}
				}
			}
		}

		void PopulateCusCodeListsForTaiwanPackingHouse()
		{
			foreach (var data in downloadedDatas)
			{
				var dataTable = ExcelHelper.BytesToDataTable(ExcelFileFormat.Xlsx, data, 0, 0, true);
				var DataByCode = new Dictionary<string, (string Description, List<RefCusCodeListAttribute> Attributes)>();
				foreach (DataRow row in dataTable.Rows)
				{
					var code = GetValueAsString(row[6]);
					if (!string.IsNullOrEmpty(code))
					{
						if (DataByCode.ContainsKey(code))
						{
							DataByCode[code].Attributes.Add(new RefCusCodeListAttribute { ZZE_ZXE_NKName = AttributeNames.Tariff, ZZE_Value = GetValueAsString(row[0]) });
						}
						else
						{
							DataByCode[code] = (GetValueAsString(row[4]), new List<RefCusCodeListAttribute>
							{
								new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Tariff, ZZE_Value = GetValueAsString(row[0]) },
								new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.Country, ZZE_Value = GetValueAsString(row[2]) },
								new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.ForeignId, ZZE_Value = GetValueAsString(row[5]) },
							});
						}
					}
				}

				foreach (var pair in DataByCode)
				{
					var refCusCodeList = new RefCusCodeList
					{
						ZZD_Code = pair.Key.Trim(),
						ZZD_Description = pair.Value.Description.SafeSubstring(0, 2000),
						RefCusCodeListAttributes = pair.Value.Attributes.ToArray(),
					};
					RefCusCodeLists.Add(refCusCodeList);
				}
			}
		}

		void PopulateCusCodeListsForRejectionReasonOrRequiredFormalities()
		{
			var dataTable = ExcelHelper.ExcelToDataTable(ExcelPath, ExcelSheetIndex, 0, true);
			foreach (DataRow row in dataTable.Rows)
			{
				var code = GetValueAsString(row[0]);
				var description = GetValueAsString(row[1]);
				if (!string.IsNullOrEmpty(code))
				{
					RefCusCodeLists.Add(new RefCusCodeList() { ZZD_Code = code.Trim(), ZZD_Description = description.SafeSubstring(0, 2000) });
				}
			}
		}

		void PopulateCusCodeListsForTaiwanCustomsRequirements()
		{
			var dataTable = ExcelHelper.ExcelToDataTable(ExcelPath, ExcelSheetIndex, 0, true);
			foreach (DataRow row in dataTable.Rows)
			{
				var attributeName = GetValueAsString(row[0]);
				var code = GetValueAsString(row[1]);
				var description = GetValueAsString(row[2]);
				if (!string.IsNullOrEmpty(code))
				{
					var refCusCodeList = new RefCusCodeList
					{
						ZZD_Code = code.Trim(),
						ZZD_Description = description.SafeSubstring(0, 2000),
						RefCusCodeListAttributes = new RefCusCodeListAttribute[] { new RefCusCodeListAttribute() { ZZE_ZXE_NKName = attributeName, ZZE_Value = attributeName } }
					};
					RefCusCodeLists.Add(refCusCodeList);
				}
			}
		}

		int ExcelSheetIndex => 0;

		XmlWriterConfiguration GetRefCusCodeListWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var refCusCodeList = new EntityTypeConfiguration<RefCusCodeList>(true);
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			refCusCodeList.IncludeColumn(x => x.ZZD_Code, true);
			refCusCodeList.IncludeColumn(x => x.ZZD_Description, false);
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, DefaultStartDate);
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, DefaultEndDate);
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.DataGroupings.Taiwan);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeList);

			var includeAttributes = codeType == CodeTypes.TaiwanPackingHouse || codeType == CodeTypes.TaiwanCommodityInspection || codeType == CodeTypes.TaiwanSCECA;
			if (includeAttributes)
			{
				refCusCodeList.IncludeColumn(x => x.RefCusCodeListAttributes, false);
				var cusCodeListAttributeConfig = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
				cusCodeListAttributeConfig.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
				cusCodeListAttributeConfig.IncludeColumn(x => x.ZZE_Value, true);
				writerConfiguration.IncludeEntityTypeConfiguration(cusCodeListAttributeConfig);
			}

			var includeLanguages = codeType == CodeTypes.TaiwanUnitsOfMeasurement || codeType == CodeTypes.TaiwanAircraftPartCAACCode;
			if (includeLanguages)
			{
				refCusCodeList.IncludeColumn(x => x.RefCusCodeListLanguages, false);
				var cusCodeListLanguageConfig = new EntityTypeConfiguration<RefCusCodeListLanguage>(true);
				cusCodeListLanguageConfig.IncludeColumnWithConstantValue(x => x.ZXA_ZX6_NKLanguage, true, Constants.Languages.ChineseTraditional);
				cusCodeListLanguageConfig.IncludeColumn(x => x.ZXA_Description);
				writerConfiguration.IncludeEntityTypeConfiguration(cusCodeListLanguageConfig);
			}

			var includeAttributeTransportMode = codeType == CodeTypes.TaiwanCustomsOffices;
			if (includeAttributeTransportMode)
			{
				refCusCodeList.IncludeColumn(x => x.RefCusCodeOrAttributeTransportModes, false);
				var cusCodeOrAttributeTransportModeConfig = new EntityTypeConfiguration<RefCusCodeOrAttributeTransportMode>(true);
				cusCodeOrAttributeTransportModeConfig.IncludeColumn(x => x.ZZU_TransportMode, true);
				writerConfiguration.IncludeEntityTypeConfiguration(cusCodeOrAttributeTransportModeConfig);
			}

			return writerConfiguration;
		}

		static XmlWriterConfiguration GetXmlWriterConfigForFac()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var refCusCodeList = new EntityTypeConfiguration<RefCusCodeList>(true);
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.DataGroupings.Taiwan);
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, CodeTypes.FacilityCode);
			refCusCodeList.IncludeColumn(x => x.ZZD_Code, true);
			refCusCodeList.IncludeColumn(x => x.ZZD_Description, false);
			refCusCodeList.IncludeColumn(x => x.ZZD_StartDate, false);
			refCusCodeList.IncludeColumn(x => x.ZZD_EndDate, false);
			refCusCodeList.IncludeColumn(x => x.RefCusCodeListAttributes, false);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeList);

			var cusCodeListAttributeConfig = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			cusCodeListAttributeConfig.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			cusCodeListAttributeConfig.IncludeColumn(x => x.ZZE_Value, true);
			writerConfiguration.IncludeEntityTypeConfiguration(cusCodeListAttributeConfig);

			return writerConfiguration;
		}

		static readonly DateTime DefaultStartDate = new DateTime(1900, 01, 01, 0, 0, 0);
		static readonly DateTime DefaultEndDate = new DateTime(2079, 06, 06, 23, 59, 0);

		public static class CodeTypes
		{
			public const string RejectionReason = "TWRR";
			public const string RequiredFormalities = "TWRF";
			public const string TaiwanCustomsRequirements = "TWCR";
			public const string TaiwanImportRegulations = "TWIR";
			public const string TaiwanExportRegulations = "TWER";
			public const string TaiwanPackingHouse = "TWPKH";
			public const string TaiwanUnitsOfMeasurement = "TWCIU";
			public const string TaiwanSCECA = "SCECA";
			public const string TaiwanCommodityInspection = "ICI";
			public const string FacilityCode = "FAC";
			public const string TaiwanControllingAgency = "TWCA";
			public const string TaiwanCustomsOffices = "CUSOF";
			public const string TaiwanAircraftPartCAACodeCategory = "CAACC";
			public const string TaiwanAircraftPartCAACCode = "CAAC";
			public const string TaiwanExciseTaxes = "TWET";
			public const string TWCPT016RejectionReason = "TWREJ";
			public const string TWCPT_017_Error_DocumentOrRequiredFormalities = "TWREQ";
			public const string CPT018ResponseToWarehouse = "TWREW";
		}

		public static class AttributeNames
		{
			public const string Tariff = "Tariff";
			public const string Country = "Country";
			public const string ForeignId = "ForeignId";
			public const string ControllingAgency = "ControllingAgency";
			public const string CUSTOMSOFFICE = "CUSTOMSOFFICE";
			public const string Remarks = "Remarks";
			public const string Source = "Source";
		}
	}
}
