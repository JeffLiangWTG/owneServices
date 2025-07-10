using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class TariffCharacteristicNCMParser : BaseParser
	{
		public TariffCharacteristicNCMParser(string dataSource) : base(dataSource)
		{
		}

		public void ExportToXMLFile(IEnumerable<Atributo> attributes, IEnumerable<NCM> tariffs, string outputFileName, DateTime publicationTime, string characteristicType)
		{
			var refTariffList = GetRefCusTariffList(attributes, tariffs);
			var writerConfiguration = Helper.GetTariffBRCharacteristicConfiguration(characteristicType, true);
			Helper.ExportToXMLFile(outputFileName, dataSource, publicationTime, writerConfiguration, refTariffList);
		}

		static ICollection<RefCusTariff> GetRefCusTariffList(IEnumerable<Atributo> attributes, IEnumerable<NCM> tariffs)
		{
			var result = new HashSet<RefCusTariff>();

			Contract.Requires(attributes?.Any() ?? false);
			Contract.Requires(tariffs?.Any() ?? false);

			foreach (var group in tariffs.GroupBy(x => x.codigoNcm))
			{
				var ncmCode = group.Key;
				if (ncmCode != null && Regex.IsMatch(ncmCode, RatesUtils.NcmPattern))
				{
					var ncmAttributes = group.SelectMany(x => x.listaAtributos).DistinctBy(x => x.codigo);
					if (ncmAttributes.Any())
					{
						var tariffBRCharacteristics = new List<RefCusTariffBRCharacteristic>();
						foreach (var ncmAttribute in ncmAttributes)
						{
							var attribute = attributes.FirstOrDefault(a => a.codigo == ncmAttribute.codigo);
							if (!string.IsNullOrEmpty(attribute?.codigo))
							{
								var characteristic = GetRefCusTariffBRCharacteristic(ncmAttribute, attribute);
								if (!string.IsNullOrEmpty(characteristic.ZB1_Style))
								{
									if (attribute.dominio?.Any() ?? false)
									{
										characteristic.RefCusTariffBRCharacteristicValues = GetRefCusTariffBRCharacteristicValue(attribute.dominio).ToArray();
									}
									tariffBRCharacteristics.Add(characteristic);
								}
							}
						}
						if (tariffBRCharacteristics.Any())
						{
							var refCusTariff = new RefCusTariff()
							{
								ZZ1_TariffCode = ncmCode.KeepNumericsOnly(),
								RefCusTariffBRCharacteristics = tariffBRCharacteristics.ToArray()
							};
							result.Add(refCusTariff);
						}
					}
				}
			}
			return result;
		}

		static RefCusTariffBRCharacteristic GetRefCusTariffBRCharacteristic(Atributo ncmAttribute, Atributo attribute)
		{
			var characteristic = new RefCusTariffBRCharacteristic();

			characteristic.ZB1_Style = GetStyle(attribute.formaPreenchimento);
			if (!string.IsNullOrEmpty(characteristic.ZB1_Style) && !characteristic.ZB1_Style.Equals(Constants.StyleConstants.Compound, StringComparison.Ordinal))
			{
				characteristic.ZB1_Code = attribute.codigo;
				characteristic.ZB1_Text = attribute.nomeApresentacao;
				characteristic.ZB1_MaxLength = ((short)attribute.tamanhoMaximo);
				characteristic.ZB1_DecimalPlaces = (short)attribute.casasDecimais;

				if (DateTime.TryParseExact(ncmAttribute.dataInicioVigencia, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate))
				{
					characteristic.ZB1_StartDate = startDate;
				}
				if (DateTime.TryParseExact(attribute.dataFimVigencia, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
				{
					characteristic.ZB1_EndDate = endDate;
				}

				var modality = ncmAttribute.modalidade;
				characteristic.ZB1_IsImport = !string.IsNullOrEmpty(modality) && string.Equals(modality, "Importação", StringComparison.Ordinal);
				characteristic.ZB1_IsExport = !string.IsNullOrEmpty(modality) && string.Equals(modality, "Exportação", StringComparison.Ordinal);
				characteristic.ZB1_IsMandatory = ncmAttribute.obrigatorio;
				if (!attribute.atributoCondicionante)
				{
					characteristic.ZB1_IsConditioningAttribute = attribute.atributoCondicionante;
				}
				var fillingGuidance = attribute.orientacaoPreenchimento;

				if (!string.IsNullOrEmpty(fillingGuidance))
				{
					characteristic.RefCusTariffBRCharacteristicAttributes = GetRefCusTariffBRCharacteristicAttributes(fillingGuidance).ToArray();
				}
			}
			else
			{
				characteristic.ZB1_Style = string.Empty;
			}

			return characteristic;
		}

		static IEnumerable<RefCusTariffBRCharacteristicValue> GetRefCusTariffBRCharacteristicValue(List<Dominio> domainItems)
		{
			var tariffBRCharacteristicValues = new List<RefCusTariffBRCharacteristicValue>();
			foreach (var item in domainItems)
			{
				var characteristicValue = new RefCusTariffBRCharacteristicValue
				{
					ZB2_Value = item.codigo,
					ZB2_Description = item.descricao.Length > 2000 ? item.descricao.Substring(0, 2000) : item.descricao
				};
				tariffBRCharacteristicValues.Add(characteristicValue);
			}
			return tariffBRCharacteristicValues;
		}

		static IEnumerable<RefCusTariffBRCharacteristicAttribute> GetRefCusTariffBRCharacteristicAttributes(string element)
		{
			return new List<RefCusTariffBRCharacteristicAttribute>()
			{
				new RefCusTariffBRCharacteristicAttribute()
				{
					ZB3_Name = AttributeConstants.Caption,
					ZB3_Value = element
				},
			};
		}

		static string GetStyle(string value)
		{
			switch (value)
			{
				case Constants.VariableStyleConstants.StaticList:
					return Constants.StyleConstants.List;
				case Constants.VariableStyleConstants.Boolean:
					return Constants.StyleConstants.Boolean;
				case Constants.VariableStyleConstants.Text:
					return Constants.StyleConstants.Text;
				case Constants.VariableStyleConstants.NumberInteger:
				case Constants.VariableStyleConstants.NumberFloat:
					return Constants.StyleConstants.Number;
				case Constants.VariableStyleConstants.Compound:
					return Constants.StyleConstants.Compound;
				default:
					ParserErrorCollector.Instance.AppendLine($"Style not found: {value}");
					return string.Empty;
			}
		}

		static class AttributeConstants
		{
			public const string Caption = "Caption";
		}
	}
}
