using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class TariffCharacteristicNVEParser : BaseParser
	{
		public TariffCharacteristicNVEParser(string dataSource, string dataSourceNomenclature) : base(dataSource)
		{
			this.dataSourceNomenclature = dataSourceNomenclature;
		}

		readonly string dataSourceNomenclature;

		public void ExportToXMLFile(Stream inputFileStream, string outputFileName, string outputFileNameNomenclature, DateTime publicationTime)
		{
			var lists = GetRefCusTariffList(inputFileStream);
			var writerConfiguration = Helper.GetTariffBRCharacteristicConfiguration(Constants.RefCusTariffBRCharacteristicCodes.CharacteristicTypes.NVE, true, Constants.StyleConstants.List, true, true, false, true);
			var nomenclatureWriterConfiguration = Helper.GetNomenclatureBRCharacteristicConfiguration(Constants.RefCusTariffBRCharacteristicCodes.CharacteristicTypes.NVE, true, Constants.StyleConstants.List, true, true, false, true);

			Helper.ExportToXMLFile(outputFileName, dataSource, publicationTime, writerConfiguration, lists.Item1);
			Helper.ExportToXMLFile(outputFileNameNomenclature, dataSourceNomenclature, publicationTime, nomenclatureWriterConfiguration, lists.Item2);
		}

		(ICollection<RefCusTariff>, ICollection<RefCusNomenclatureGroup>) GetRefCusTariffList(Stream streamXml)
		{
			Contract.Requires(streamXml != null);

			var resultTariff = new List<RefCusTariff>();
			var resultNomenclature = new List<RefCusNomenclatureGroup>();
			var xml = XDocument.Load(streamXml);

			Contract.Requires(xml != null);

			var nves = xml.Descendants("Nve");
			foreach (XElement nve in nves)
			{
				ValidateLegalActNumber(nve);
				ValidateLegalActStartingDate(nve);
				var nveCode = nve.GetElementValueAsString("codigoNcm", 10);
				if (nveCode != null && nveCode.Length == 8)
				{
					AddNVE(resultTariff, nveCode, nve);
				}
				else
				{
					AddNVENomenclature(resultNomenclature, nveCode, nve);
				}
			}

			if (errorMessages.Length > 0)
			{
				throw new InvalidOperationException(errorMessages.ToString());
			}

			return (resultTariff, resultNomenclature);
		}

		static void AddNVE(ICollection<RefCusTariff> result, string nveCode, XElement nve)
		{
			var refCusTariff = result.Where(x => x.ZZ1_TariffCode == nveCode)?.FirstOrDefault();
			if (refCusTariff != null)
			{
				var code = nve.GetElementValueAsString("codigoAtributo", 15);
				var characteristics = refCusTariff.RefCusTariffBRCharacteristics.ToList();
				if (characteristics.Where(x => x.ZB1_Code == code).Any())
				{
					var values = characteristics.Where(x => x.ZB1_Code == code).FirstOrDefault().RefCusTariffBRCharacteristicValues.ToList();
					values.Add(GetRefCusTariffBRCharacteristicValue(nve));
					_ = characteristics.Where(x => x.ZB1_Code == code).Select(s => { s.RefCusTariffBRCharacteristicValues = values.ToArray(); return s; }).ToList();
				}
				else
				{
					characteristics.Add(GetRefCusTariffBRCharacteristic(nve));
				}
				_ = result.Where(x => x.ZZ1_TariffCode == nveCode).Select(s => { s.RefCusTariffBRCharacteristics = characteristics.ToArray(); return s; }).ToList();
			}
			else
			{
				result.Add(new RefCusTariff()
				{
					ZZ1_TariffCode = nveCode,
					RefCusTariffBRCharacteristics = new RefCusTariffBRCharacteristic[] { GetRefCusTariffBRCharacteristic(nve) }
				});
			}
		}

		static void AddNVENomenclature(ICollection<RefCusNomenclatureGroup> result, string nveCode, XElement nve)
		{
			var refCusNomenclatureGroup = result.Where(x => x.ZZ5_Value == nveCode)?.FirstOrDefault();
			if (refCusNomenclatureGroup != null)
			{
				var code = nve.GetElementValueAsString("codigoAtributo", 15);
				var characteristics = refCusNomenclatureGroup.RefCusTariffBRCharacteristics.ToList();
				if (characteristics.Where(x => x.ZB1_Code == code).Any())
				{
					var values = characteristics.Where(x => x.ZB1_Code == code).FirstOrDefault().RefCusTariffBRCharacteristicValues.ToList();
					values.Add(GetRefCusTariffBRCharacteristicValue(nve));
					_ = characteristics.Where(x => x.ZB1_Code == code).Select(s => { s.RefCusTariffBRCharacteristicValues = values.ToArray(); return s; }).ToList();
				}
				else
				{
					characteristics.Add(GetRefCusTariffBRCharacteristic(nve));
				}
				_ = result.Where(x => x.ZZ5_Value == nveCode).Select(s => { s.RefCusTariffBRCharacteristics = characteristics.ToArray(); return s; }).ToList();
			}
			else
			{
				refCusNomenclatureGroup = new RefCusNomenclatureGroup()
				{
					ZZ5_Value = nveCode,
					ZZ5_Description = nve.GetElementValueAsString("descricaoNcm", 0),
					RefCusTariffBRCharacteristics = new RefCusTariffBRCharacteristic[] { GetRefCusTariffBRCharacteristic(nve) }
				};
				RefCusNomenclatureGroupUtils.BuildZZ1_CompositeKeyOnZZ5(refCusNomenclatureGroup);
				result.Add(refCusNomenclatureGroup);
			}
		}

		static RefCusTariffBRCharacteristic GetRefCusTariffBRCharacteristic(XElement nve)
		{
			var characteristic = new RefCusTariffBRCharacteristic
			{
				ZB1_Code = nve.GetElementValueAsString("codigoAtributo", 15),
				ZB1_Text = nve.GetElementValueAsString("descricaoAtributo", 35),
				ZB1_IsExport = false,
				ZB1_IsImport = true,
				ZB1_IsMandatory = true,
				ZB1_IsConditioningAttribute = false,
				RefCusTariffBRCharacteristicAttributes = GetRefCusTariffBRCharacteristicAttributes(nve).ToArray(),
				RefCusTariffBRCharacteristicValues = new RefCusTariffBRCharacteristicValue[] { GetRefCusTariffBRCharacteristicValue(nve) }
			};

			if (DateTime.TryParseExact(nve.GetElementValueAsString("inicioVigenciaEspecificacao", 15), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate))
			{
				characteristic.ZB1_StartDate = startDate;
			}

			return characteristic;
		}

		static RefCusTariffBRCharacteristicValue GetRefCusTariffBRCharacteristicValue(XElement nve)
		{
			var tariffBRCharacteristicValue = new RefCusTariffBRCharacteristicValue()
			{
				ZB2_Value = nve.GetElementValueAsString("codigoEspecificacao", 10),
				ZB2_Description = nve.GetElementValueAsString("descricaoEspecificacao", 35).Trim()
			};
			return tariffBRCharacteristicValue;
		}

		static IEnumerable<RefCusTariffBRCharacteristicAttribute> GetRefCusTariffBRCharacteristicAttributes(XElement nve)
		{
			return new List<RefCusTariffBRCharacteristicAttribute>()
			{
				new RefCusTariffBRCharacteristicAttribute()
				{
					ZB3_Name = AttributeConstants.NumeroAtoLegal,
					ZB3_Value = nve.GetElementValueAsString(AttributeConstants.NumeroAtoLegal, 35)
				},
				new RefCusTariffBRCharacteristicAttribute()
				{
					ZB3_Name = AttributeConstants.TipoAtoLegal,
					ZB3_Value = nve.GetElementValueAsString(AttributeConstants.TipoAtoLegal, 35)
				},
				new RefCusTariffBRCharacteristicAttribute()
				{
					ZB3_Name = AttributeConstants.AnoAtoLegal,
					ZB3_Value = nve.GetElementValueAsString(AttributeConstants.AnoAtoLegal, 35)
				},
				new RefCusTariffBRCharacteristicAttribute()
				{
					ZB3_Name = AttributeConstants.OrgaoEmissorAtoLegal,
					ZB3_Value = nve.GetElementValueAsString(AttributeConstants.OrgaoEmissorAtoLegal, 35)
				}
			};
		}
		static class AttributeConstants
		{
			public const string NumeroAtoLegal = "numeroAtoLegal";
			public const string TipoAtoLegal = "tipoAtoLegal";
			public const string AnoAtoLegal = "anoAtoLegal";
			public const string OrgaoEmissorAtoLegal = "orgaoEmissorAtoLegal";
		}

		void ValidateLegalActStartingDate(XElement nve)
		{
			var ncmCode = nve.GetElementValueAsString("codigoNcm", 10);
			var attributeCode = nve.GetElementValueAsString("codigoAtributo", 15);
			var nveCode = nve.GetElementValueAsString("codigoEspecificacao", 10);
			var legalActStartDate = nve.GetElementValueAsString("inicioVigenciaEspecificacao", 15);
			var attributeDescription = nve.GetElementValueAsString("descricaoAtributo", 35).Trim();

			if (!legalActStartDates.ContainsKey(ncmCode + attributeCode + nveCode))
			{
				legalActStartDates.Add(ncmCode + attributeCode + nveCode, nve);
			}
			else
			{
				legalActStartDates.TryGetValue(ncmCode + attributeCode + nveCode, out var val);
				var legalActStartDate2 = val.GetElementValueAsString("inicioVigenciaEspecificacao", 15);
				var attributeDescription2 = val.GetElementValueAsString("descricaoAtributo", 35).Trim();
				if (legalActStartDate2 != legalActStartDate && attributeDescription.Trim() != attributeDescription2.Trim())
				{
					var errorMessage =
						$"Multiple Legal act start date founded for the same combination of (codigoNcm, codigoAtributo, codigoEspecificacao and descricaoAtributo) ({ncmCode}, {attributeCode}, {nveCode} and {attributeDescription}) legal Act start dates ( {legalActStartDate}, {legalActStartDate2} )";

					errorMessages.Append(errorMessage + "\n");
				}
				ValidateLegalActStartDateIsMinor(legalActStartDate, legalActStartDate2, ncmCode, attributeCode, nveCode, attributeDescription);
			}

		}

		void ValidateLegalActStartDateIsMinor(string sDate, string sDate2, string ncmCode, string attributeCode, string nveCode, string attributeDescription)
		{
			var bDate = DateTime.TryParseExact(sDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None,
				out DateTime date);
			var bStartDate = DateTime.TryParseExact(sDate2, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None,
				out DateTime startDate);
			if (bDate && bStartDate)
			{
				if (DateTime.Compare(startDate, date) > 0)
				{
					errorMessages.Append($"Generated Legal act of (codigoNcm, codigoAtributo, codigoEspecificacao and descricaoAtributo) ({ncmCode}, {attributeCode}, {nveCode} and {attributeDescription}) is not using the minor date founded on the xml minor date: {sDate}, actual date: {sDate2}" + "\n");
				}
			}
		}

		void ValidateLegalActNumber(XElement nve)
		{
			var legalActNumber = nve.GetElementValueAsString(AttributeConstants.NumeroAtoLegal, 35);
			var ncmCode = nve.GetElementValueAsString("codigoNcm", 10);
			var attributeCode = nve.GetElementValueAsString("codigoAtributo", 15);
			var nveCode = nve.GetElementValueAsString("codigoEspecificacao", 10);

			if (!legalActNumbers.ContainsKey(ncmCode + attributeCode + nveCode))
			{
				legalActNumbers.Add(ncmCode + attributeCode + nveCode, legalActNumber);
			}
			else
			{
				legalActNumbers.TryGetValue(ncmCode + attributeCode + nveCode, out var val);

				if (val != legalActNumber)
				{
					var errorMessage =
						$"Multiple Legal act number founded for the same combination of (codigoNcm, codigoAtributo and codigoEspecificacao) ({ncmCode}, {attributeCode} and {nveCode}) legal Act Numbers ( {legalActNumber}, {val} )";
					errorMessages.Append(errorMessage + "\n");
				}
			}
		}

		StringBuilder errorMessages = new StringBuilder();
		Dictionary<string, string> legalActNumbers = new Dictionary<string, string>();
		Dictionary<string, XElement> legalActStartDates = new Dictionary<string, XElement>();
	}
}
