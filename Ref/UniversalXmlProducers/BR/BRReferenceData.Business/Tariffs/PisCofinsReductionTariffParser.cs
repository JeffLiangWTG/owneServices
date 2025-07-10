using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class PisCofinsReductionTariffParser
	{
		public PisCofinsReductionTariffParser(string dataSource)
		{
			this.dataSource = dataSource;
		}

		readonly string dataSource;

		public void ExportToXMLFile(Stream pisCofinsInputFileStream, Stream ncmInputFileStream, string outputFileName, DateTime publicationTime)
		{
			var refTariffList = GetRefCusTariffList(pisCofinsInputFileStream, ncmInputFileStream);
			var writerConfiguration = Helper.GetChildTariffConfiguration(null, Constants.Groups.ALL, defaultZZ1_EndDate: true, hasAttributes: false, defaultZZ2_ZY1_ZZR_NKRateType: null, constantPreferenceDataGrouping: string.Empty, constantNKPreference: string.Empty);
			Helper.ExportToXMLFile(outputFileName, dataSource, publicationTime, writerConfiguration, refTariffList);
		}

		static ICollection<RefCusTariff> GetRefCusTariffList(Stream pisCofinsStreamXml, Stream ncmStreamXml)
		{
			Contract.Requires(pisCofinsStreamXml != null);
			Contract.Requires(ncmStreamXml != null);

			var result = new HashSet<RefCusTariff>();
			var pisCofinsXml = XDocument.Load(pisCofinsStreamXml);
			var ncmXml = XDocument.Load(ncmStreamXml);

			Contract.Requires(pisCofinsXml != null);
			Contract.Requires(ncmXml != null);

			var tariffDescriptions = GetTariffDescription(ncmXml);
			var pisCofinsElements = pisCofinsXml.Root?.Descendants("FundamentoLegalReducaoPisCofins");

			Contract.Assume(pisCofinsElements != null);

			foreach (var pisCofins in pisCofinsElements)
			{
				var tariffType = pisCofins.GetElementValueAsString("codigo", 2);
				var goods = pisCofins.Descendants("mercadoria");
				foreach (var merchandise in goods)
				{
					var tariffCode = merchandise.GetElementValueAsString("codigo", 15);
					if (tariffDescriptions.TryGetValue(tariffCode, out var description))
					{
						var startDate = merchandise.GetElementValueAsDateTime("inicioVigencia");
						var rate = decimal.TryParse(merchandise.GetElementValueAsString("percentual", 15), out var number) ? number : decimal.Zero;

						var refCusRateList = new List<RefCusRate>
					{
						new RefCusRate()
						{
							ZZ2_RateFormula = $"(VFD - (VFD * {rate / 100})) * {RateConstants.PisRate / 100}",
							ZZ2_RateFormulaDerivedFrom = $"Reduction Base of Calculation {rate:0.00} / Rate {RateConstants.PisRate}",
							ZZ2_StartDate = startDate,
							ZZ2_ZY1_ZZR_NKRateType = Constants.Rates.Types.PIS,
							ZZ2_ZY1_NKRateCode = Constants.Rates.Codes.PIS,
							RefCusApplicabilities = new RefCusApplicability[] { new RefCusApplicability() { ZZT_StartDate = startDate } }
						},
						new RefCusRate()
						{
							ZZ2_RateFormula = $"(VFD - (VFD * {rate / 100})) * {RateConstants.CofinsRate / 100}",
							ZZ2_RateFormulaDerivedFrom = $"Reduction Base of Calculation {rate:0.00} / Rate {RateConstants.CofinsRate}",
							ZZ2_StartDate = startDate,
							ZZ2_ZY1_ZZR_NKRateType = Constants.Rates.Types.COFINS,
							ZZ2_ZY1_NKRateCode = Constants.Rates.Codes.COFINS,
							RefCusApplicabilities = new RefCusApplicability[] { new RefCusApplicability() { ZZT_StartDate = startDate } }
						}
					};
						var refCusTariff = new RefCusTariff()
						{
							ZZ1_Description = description,
							ZZ1_TariffCode = tariffCode,
							ZZ1_StartDate = startDate,
							ZZ1_ZZI_NKTariffType = tariffType,
							RefCusRates = refCusRateList.ToArray(),
							RefCusTariffRelationships = new RefCusTariffRelationship[] { new RefCusTariffRelationship() { ZZH_TariffCode = tariffCode } }
						};
						result.Add(refCusTariff);
					}
				}
			}

			return result;
		}

		static IDictionary<string, string> GetTariffDescription(XDocument xml)
		{
			var result = new Dictionary<string, string>();

			var tariffElements = xml.Root.Descendants(HSNTariffContants.TagCustomsTariff);
			foreach (XElement element in tariffElements)
			{
				string tariffCode = element.GetElementValueAsString(HSNTariffContants.TagCustomsTariffCode, 8);
				string tariffDescription = element.GetElementValueAsString(HSNTariffContants.TagCustomsTariffDescription, 500);

				if (tariffCode == null || tariffCode.Length < 8 || string.IsNullOrEmpty(tariffDescription))
				{
					continue;
				}

				result.Add(tariffCode, DescriptionCleaner(tariffDescription));
			}
			return result;
		}

		static string DescriptionCleaner(string description)
		{
			return description?.Trim(new char[] { '-', ' ' });
		}

		static class RateConstants
		{
			public const decimal PisRate = 2.62m;
			public const decimal CofinsRate = 12.57m;
		}
	}
}
