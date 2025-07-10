using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.TaiwanReferenceData.TurnkeyPlugInUpdateService;
using Exception = System.Exception;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public class TaiwanExciseTaxesUpdateInfo : RefDataUpdateInfo
	{
		public TaiwanExciseTaxesUpdateInfo(updateInfoBean info, string excelFilePath, string filename, DateTime publicationDateTime) : base(info, filename)
		{
			this.excelFilePath = excelFilePath;
			this.publicationDateTime = publicationDateTime;
		}

		readonly string excelFilePath;
		readonly DateTime publicationDateTime;

		protected override void Execute()
		{
			if (TryParseExciseTaxDataRowsFromExcel(excelFilePath, out var tariffList)
				&& TryParseExciseTaxRateDataRowsFromExcel(excelFilePath, out var rateList)
				&& TryParseExciseTaxTariffRelationshipDataRowsFromExcel(excelFilePath, out var relationshipList))
			{
				GenerateTaiwanExciseTaxesXml(tariffList, rateList, relationshipList);
			}
		}

		bool TryParseExciseTaxDataRowsFromExcel(string fileName, out List<ExciseTaxTariffDataRow> list)
		{
			try
			{
				var dataTable = ExcelHelper.ExcelToDataTable(fileName, 0, 0, true);
				list = new List<ExciseTaxTariffDataRow>();
				foreach (DataRow row in dataTable.Rows)
				{
					var exciseTax = ExciseTaxTariffDataRow.New(row);
					if (exciseTax != null)
					{
						list.Add(exciseTax);
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				ErrorWriter.WriteException(ex);
				list = null;
				return false;
			}
		}

		bool TryParseExciseTaxRateDataRowsFromExcel(string fileName, out List<ExciseTaxRateDataRow> list)
		{
			try
			{
				var dataTable = ExcelHelper.ExcelToDataTable(fileName, 1, 0, true);
				list = new List<ExciseTaxRateDataRow>();
				foreach (DataRow row in dataTable.Rows)
				{
					var exciseTaxRate = ExciseTaxRateDataRow.New(row);
					if (exciseTaxRate != null)
					{
						list.Add(exciseTaxRate);
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				ErrorWriter.WriteException(ex);
				list = null;
				return false;
			}
		}

		bool TryParseExciseTaxTariffRelationshipDataRowsFromExcel(string fileName, out List<ExciseTaxTariffRelationshipDataRow> list)
		{
			try
			{
				var dataTable = ExcelHelper.ExcelToDataTable(fileName, 2, 0, true);
				list = new List<ExciseTaxTariffRelationshipDataRow>();
				foreach (DataRow row in dataTable.Rows)
				{
					var exciseTaxRate = ExciseTaxTariffRelationshipDataRow.New(row);
					if (exciseTaxRate != null)
					{
						list.Add(exciseTaxRate);
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				ErrorWriter.WriteException(ex);
				list = null;
				return false;
			}
		}

		void GenerateTaiwanExciseTaxesXml(List<ExciseTaxTariffDataRow> tariffList, List<ExciseTaxRateDataRow> rateList, List<ExciseTaxTariffRelationshipDataRow> RelationshipList)
		{
			using (var xmlFile = File.Create(Filename))
			{
				var doc = new XDocument(new XDeclaration("1.0", "utf-8", null),
					new XElement("UniversalReferenceData",
						new XElement("DataSource", "TW Excise Taxes"),
						new XElement("PublicationTime", publicationDateTime.ToString("s", CultureInfo.InvariantCulture)),
						new XElement("UpdateType", "FULL"),
						new XElement("Schema",
				#region RefCusTariff
										new XElement("EntityType", new XAttribute("Name", "RefCusTariff"), new XAttribute("Data", true)
										,
											new XElement("Key",
												new XElement("PropertyRef", new XAttribute("Name", "ZZ1_TariffCode")),
												new XElement("PropertyRef", new XAttribute("Name", "ZZ1_ZZI_NKTariffType")),
												new XElement("PropertyRef", new XAttribute("Name", "ZZ1_ZZI_ZZZ_NKDataGrouping")),
												new XElement("PropertyRef", new XAttribute("Name", "ZZ1_ZZZ_NKDataGrouping"))),
											new XElement("Property", new XAttribute("Name", "RefCusRate"), new XAttribute("Type", "RefCusRate")),
											new XElement("Property", new XAttribute("Name", "RefCusTariffRelationship"), new XAttribute("Type", "RefCusTariffRelationship")),
											new XElement("Property", new XAttribute("Name", "RefCusTariffUOM"), new XAttribute("Type", "RefCusTariffUOM")),
											new XElement("Property", new XAttribute("Name", "ZZ1_Description"), new XAttribute("Type", "nvarchar")),
											new XElement("Property", new XAttribute("Name", "ZZ1_EndDate"), new XAttribute("Type", "datetime")),
											new XElement("Property", new XAttribute("Name", "ZZ1_StartDate"), new XAttribute("Type", "datetime")),
											new XElement("Property", new XAttribute("Name", "ZZ1_TariffCode"), new XAttribute("Type", "varchar"), new XAttribute("MaxLength", 35)),
											new XElement("Property", new XAttribute("Name", "ZZ1_ZZI_NKTariffType"), new XAttribute("Type", "varchar"), new XAttribute("MaxLength", 5)),
											new XElement("Property", new XAttribute("Name", "ZZ1_ZZI_ZZZ_NKDataGrouping"), new XAttribute("Type", "varchar"), new XAttribute("MaxLength", 3), new XAttribute("ConstantValue", "TW")),
											new XElement("Property", new XAttribute("Name", "ZZ1_ZZZ_NKDataGrouping"), new XAttribute("Type", "varchar"), new XAttribute("MaxLength", 3), new XAttribute("ConstantValue", "TW"))
											),
				#endregion
				#region RefCusRate
							new XElement("EntityType", new XAttribute("Name", "RefCusRate"), new XAttribute("Data", true)
							,
								new XElement("Key",
									new XElement("PropertyRef", new XAttribute("Name", "RefCusApplicability")),
									new XElement("PropertyRef", new XAttribute("Name", "ZZ2_ZY1_NKRateCode")),
									new XElement("PropertyRef", new XAttribute("Name", "ZZ2_ZY1_ZZR_NKRateType")),
									new XElement("PropertyRef", new XAttribute("Name", "ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping")),
									new XElement("PropertyRef", new XAttribute("Name", "ZZ2_ZZZ_NKDataGrouping"))),
								new XElement("Property", new XAttribute("Name", "RefCusApplicability"), new XAttribute("Type", "RefCusApplicability")),
								new XElement("Property", new XAttribute("Name", "ZZ2_EndDate"), new XAttribute("Type", "datetime")),
								new XElement("Property", new XAttribute("Name", "ZZ2_RateFormula"), new XAttribute("Type", "varchar"), new XAttribute("MaxLength", 500)),
								new XElement("Property", new XAttribute("Name", "ZZ2_RateFormulaDerivedFrom"), new XAttribute("Type", "nvarchar"), new XAttribute("MaxLength", 1000)),
								new XElement("Property", new XAttribute("Name", "ZZ2_StartDate"), new XAttribute("Type", "datetime")),
								new XElement("Property", new XAttribute("Name", "ZZ2_ZY1_NKRateCode"), new XAttribute("Type", "varchar"), new XAttribute("MaxLength", 5)),
								new XElement("Property", new XAttribute("Name", "ZZ2_ZY1_ZZR_NKRateType"), new XAttribute("Type", "varchar"), new XAttribute("MaxLength", 3)),
								new XElement("Property", new XAttribute("Name", "ZZ2_ZZZ_NKDataGrouping"), new XAttribute("Type", "varchar"), new XAttribute("MaxLength", 3), new XAttribute("ConstantValue", "TW"))
							),
				#endregion
				#region RefCusApplicability
							new XElement("EntityType", new XAttribute("Name", "RefCusApplicability"), new XAttribute("Data", true)
							,
								new XElement("Key",
									new XElement("PropertyRef", new XAttribute("Name", "ZZT_OrderNumber")),
									new XElement("PropertyRef", new XAttribute("Name", "ZZT_ZZA_NKTradeGroup")),
									new XElement("PropertyRef", new XAttribute("Name", "ZZT_ZZA_ZZZ_NKDataGrouping"))),
								new XElement("Property", new XAttribute("Name", "ZZT_EndDate"), new XAttribute("Type", "smalldatetime"), new XAttribute("ConstantValue", "2079-06-06 23:59:00")),
								new XElement("Property", new XAttribute("Name", "ZZT_OrderNumber"), new XAttribute("Type", "nvarchar"), new XAttribute("MaxLength", 15), new XAttribute("ConstantValue", "")),
								new XElement("Property", new XAttribute("Name", "ZZT_StartDate"), new XAttribute("Type", "smalldatetime"), new XAttribute("ConstantValue", "1900-01-01 00:00:00")),
								new XElement("Property", new XAttribute("Name", "ZZT_ZZA_NKTradeGroup"), new XAttribute("Type", "varchar"), new XAttribute("MaxLength", 35)),
								new XElement("Property", new XAttribute("Name", "ZZT_ZZA_ZZZ_NKDataGrouping"), new XAttribute("Type", "varchar"), new XAttribute("MaxLength", 3), new XAttribute("ConstantValue", "TW"))
								)
							,
				#endregion
				#region RefCusTariffRelationship
							new XElement("EntityType", new XAttribute("Name", "RefCusTariffRelationship"), new XAttribute("Data", true)
							,
								new XElement("Key",
									new XElement("PropertyRef", new XAttribute("Name", "ZZH_ZZI_NKTariffType")),
									new XElement("PropertyRef", new XAttribute("Name", "ZZH_TariffCode")),
									new XElement("PropertyRef", new XAttribute("Name", "ZZH_ZZI_ZZZ_NKDataGrouping"))),
								new XElement("Property", new XAttribute("Name", "ZZH_ZZI_NKTariffType"), new XAttribute("Type", "varchar"), new XAttribute("MaxLength", 5), new XAttribute("ConstantValue", "HSN")),
								new XElement("Property", new XAttribute("Name", "ZZH_TariffCode"), new XAttribute("Type", "varchar"), new XAttribute("MaxLength", 35)),
								new XElement("Property", new XAttribute("Name", "ZZH_ZZI_ZZZ_NKDataGrouping"), new XAttribute("Type", "varchar"), new XAttribute("MaxLength", 3), new XAttribute("ConstantValue", "TW"))
								)
							,
				#endregion
				#region RefCusTariffUOM
							new XElement("EntityType", new XAttribute("Name", "RefCusTariffUOM"), new XAttribute("Data", true)
							,
								new XElement("Key",
									new XElement("PropertyRef", new XAttribute("Name", "ZZ8_Type")),
									new XElement("PropertyRef", new XAttribute("Name", "ZZ8_ZZZ_NKDataGrouping"))),
								new XElement("Property", new XAttribute("Name", "ZZ8_Type"), new XAttribute("Type", "varchar"), new XAttribute("MaxLength", 3)),
								new XElement("Property", new XAttribute("Name", "ZZ8_UOM"), new XAttribute("Type", "varchar"), new XAttribute("MaxLength", 10)),
								new XElement("Property", new XAttribute("Name", "ZZ8_ZZZ_NKDataGrouping"), new XAttribute("Type", "varchar"), new XAttribute("MaxLength", 3), new XAttribute("ConstantValue", "TW"))
								)
				#endregion
				),
				from row in tariffList
				select new XElement("RefCusTariff",
					new XElement("ZZ1_ZZI_NKTariffType", row.ZZ1_ZZI_TariffType),
					new XElement("ZZ1_Description", row.ZZ1_Description),
					new XElement("ZZ1_EndDate", row.ZZ1_EndDate),
					new XElement("ZZ1_StartDate", row.ZZ1_StartDate),
					new XElement("ZZ1_TariffCode", row.ZZ1_TariffCode),
					CreateRefCusTariffUOM(row),
					CreateExciseTaxRate(row.ZZ1_TariffCode, row.ZZ1_ZZI_TariffType, rateList),
					CreateExciseTariffRelationships(row.ZZ1_TariffCode, row.ZZ1_ZZI_TariffType, RelationshipList)
				)));

				doc.Save(xmlFile);
			}
		}

		IEnumerable<XElement> CreateRefCusTariffUOM(ExciseTaxTariffDataRow tariffRow)
		{
			if (tariffRow.ExciseTaxTariffUOMDataRow is ExciseTaxTariffUOMDataRow tariffUOM)
			{
				yield return new XElement("RefCusTariffUOM",
					new XElement("ZZ8_Type", tariffUOM.ZZ8_Type),
					new XElement("ZZ8_UOM", tariffUOM.ZZ8_UOM)
					);
			}
		}

		IEnumerable<XElement> CreateExciseTaxRate(string tariffCode, string tariffType, List<ExciseTaxRateDataRow> rates)
		{
			var list = rates.Where(x => x.ZZ1_TariffCode == tariffCode && x.ZZ1_ZZI_TariffType == tariffType);
			foreach (var rate in list)
			{
				yield return new XElement("RefCusRate",
					new XElement("ZZ2_EndDate", rate.ZZ2_EndDate),
					new XElement("ZZ2_RateFormula", rate.ZZ2_RateFormula),
					new XElement("ZZ2_RateFormulaDerivedFrom", rate.ZZ2_RateFormulaDerivedFrom),
					new XElement("ZZ2_StartDate", rate.ZZ2_StartDate),
					new XElement("ZZ2_ZY1_NKRateCode", rate.ZZ2_ZY1_NKRateCode),
					new XElement("ZZ2_ZY1_ZZR_NKRateType", rate.ZZ2_ZY1_ZZR_NKRateType),
					new XElement("RefCusApplicability",
						new XElement("ZZT_ZZA_NKTradeGroup", rate.ZZT_ZZA_NKTradeGroup))
					);
			}
		}

		IEnumerable<XElement> CreateExciseTariffRelationships(string tariffCode, string tariffType, List<ExciseTaxTariffRelationshipDataRow> relationShips)
		{
			var list = relationShips.Where(x => x.ZZ1_TariffCode == tariffCode && x.ZZ1_ZZI_TariffType == tariffType);
			foreach (var relationShip in list)
			{
				yield return new XElement("RefCusTariffRelationship",
					new XElement("ZZH_TariffCode", relationShip.ZZH_TariffCode)
					);
			}
		}
	}
}
