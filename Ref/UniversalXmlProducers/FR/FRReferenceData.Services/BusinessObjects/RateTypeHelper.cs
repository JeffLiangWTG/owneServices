using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public static class RateTypeHelper
	{
		public static string GetRateType(string code)
		{
			var result = string.Empty;

			if (MeasureTypeConfiguration.TryGetValue(code, out var measureType))
			{
				result = measureType;
			}

			return result;
		}

		static ImmutableDictionary<string, string> MeasureTypeConfiguration => measureTypeConfiguration ?? (measureTypeConfiguration = GetMeasureTypeConfigurationFromSpreadSheet());
		static ImmutableDictionary<string, string> measureTypeConfiguration;

		static ImmutableDictionary<string, string> GetMeasureTypeConfigurationFromSpreadSheet()
		{
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"CargoWise.RefDbRepo.FRReferenceData.Services.Resources.{ApplicationConfig.Instance.RateTypeWithMeasureTypeFileName}");
			var workbook = new XlsFile(stream, false);
			workbook.ActiveSheetByName = "sheet1";

			var result = new Dictionary<string, string>();
			for (int row = 2; row <= workbook.RowCount; row++)
			{
				var overridenRateType = workbook.GetStringFromCell(row, 6);
				var rateType = overridenRateType.IsEmpty ? workbook.GetStringFromCell(row, 1) : overridenRateType;
				var rateCode = workbook.GetStringFromCell(row, 4);
				if (!result.ContainsKey(rateCode))
				{
					result.Add($"{rateCode}", rateType);
				}
			}
			return result.ToImmutableDictionary();
		}

		public static string GetRateTypeDescription(string rateType)
		{
			if (ZZR_Description_MaxLength is null)
			{
				var match = Regex.Match(typeof(RefCusRateType).GetProperty(nameof(RefCusRateType.ZZR_Description)).GetCustomAttribute<PropertySchemaAttribute>().PropertySchema, @"MaxLength=&quot;(\d*)&quot;");
				if (match.Success)
				{
					ZZR_Description_MaxLength = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
				}
			}

			if (rateTypeDescriptions.TryGetValue(rateType, out var description))
			{
				return ZZR_Description_MaxLength is int maxLength && description.Length > maxLength ? description.Substring(0, maxLength) : description;
			}
			return string.Empty;
		}
		static int? ZZR_Description_MaxLength;

		static readonly Dictionary<string, string> rateTypeDescriptions = new Dictionary<string, string>
		{
			{ "DEV", "Development Tax" },
			{ "RED", "Fees" },
			{ "EXC", "Excise" },
			{ "MSC", "Miscellaneous" },
			{ "OME", "Octroi de Mer Externe" },
			{ "OMR", "Octroi de Mer Régional" },
			{ "DTY", "Customs Duties" },
			{ "ADD", "Anti-Dumping Duties" },
			{ "AMC", "Mise à la Consommation Alcools (accises)" },
			{ "BNA", "Taxe sur les eaux et Boissons Non Alcooliques" },
			{ "CBE", "Contribution sur les boissons contenant de la caféine" },
			{ "CBS", "Contribution sur les boissons sucrées ou édulcorées" },
			{ "CMP", "Contribution sur métaux précieux, bijoux, objets d'art et d'antiquités" },
			{ "CSS", "Cotisation Sécurité Sociale (CMU)" },
			{ "OFI", "Taxe au profit de FranceAgriMer (ex-OFIMER)" },
			{ "PMX", "Taxe sur les boissons dites Prémix" },
			{ "RCA", "Redevance pour controles renforces - alimentation humaine" },
			{ "RCP", "Rémunération au profit du Comité Professionnel des Stocks Stratégiques Pétroliers (CPSSP)" },
			{ "RCR", "Redevance pour controles renforces - alimentation animale" },
			{ "RMA", "Redevances pour mesures d'urgence - alimentation humaine" },
			{ "ROC", "Taxe pour le développement des industries des matériaux de construction" },
			{ "RPH", "Redevance phytosanitaire" },
			{ "RSD", "Redevance sanitaire de découpage" },
			{ "RVT", "Redevance pour le contrôle vétérinaire" },
			{ "SOU", "Soulte. Rhums traditionnels originaire des DOM" },
			{ "TBO", "Taxe pour le développement des industries du bois" },
			{ "TCG", "Taxe affectée à l'importation sur les corps gras végétaux et animaux" },
			{ "TDA", "Taxe pour le développement des industries de l'ameublement" },
			{ "TDB", "Taxe pour le développement des industries de l'horlogerie, bijouterie, joaillerie et l'orfèvrerie" },
			{ "TDC", "Taxe pour le développement des industries du cuir, maroquinerie, ganterie et de la chaussure" },
			{ "TDF", "Taxe affectée au centre technique industriel de la fonderie (CTIF)" },
			{ "TDH", "Taxe pour le développement des industries de l'habillement" },
			{ "TGA", "Taxe générale sur les activités polluantes" },
			{ "TIC", "Taxe Intérieure de Consommation houilles, lignites et coke" },
			{ "TIM", "Taxe affectée perçue p/c CETIM Centre technique des industries de la mécanique et décolletage" },
			{ "TIP", "Taxe intérieure sur les produits pétroliers" },
			{ "TMC", "Accises Tabacs taxation Mise à la consommation" },
			{ "TMP", "Taxe forfaitaire sur métaux précieux, bijoux, objets d'art et d'antiquités" },
			{ "TPC", "Taxe affectée au Centre technique des industries de la transformation des papiers, cartons et celluloses CTCC" },
			{ "TPP", "Taxe affectée à l'importation sur les produits de la plasturgie" },
			{ "TSC", "Taxe spéciale de consommation" },
			{ "TVA", "TVA normale" },
			{ "TVB", "TVA canas généraux" },
			{ "TVF", "TVA valeur forfaitaire" }
		};
	}
}
