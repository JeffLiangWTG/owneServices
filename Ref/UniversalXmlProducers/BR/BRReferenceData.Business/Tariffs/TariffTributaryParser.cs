using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class TariffTributaryParser : BaseParser
	{
		public TariffTributaryParser(string dataSource) : base(dataSource)
		{
		}

		public void ExportToXMLFile(TariffTributary tariffTributary)
		{
			ValidateUnknownBlockName(tariffTributary);
			ValidateUnknownTaxCode(tariffTributary);
		}

		void ValidateUnknownBlockName(TariffTributary tariffTributary)
		{
			var unknownBlocksNames = tariffTributary.tratamentosTributariosImportacao.SelectMany(x => x.paisesBlocos.aplica.blocos.Where(bloco => bloco.nome != Constants.TariffTributary.CountryBlock.MERCOSUL).Select(b => b.nome)).ToList();
			var builder = new StringBuilder();
			if (unknownBlocksNames.Any())
			{
				foreach (var unknownBlock in unknownBlocksNames)
				{
					_ = dataSource;
					ParserErrorCollector.Instance.AppendLine($"Unknown block name founded ({unknownBlock})");
				}
			}
		}

		void ValidateUnknownTaxCode(TariffTributary tariffTributary)
		{
			var tributes = tariffTributary.tratamentosTributariosImportacao.SelectMany(x => x.tratamentosTributarios);
			var unknownTaxCodesNames = tributes.Where(x => !IsTaxCodeKnow(x.tributo.codigo)).Select(x => x.tributo.nome);
			var builder = new StringBuilder();
			if (unknownTaxCodesNames.Any())
			{
				foreach (var unknownTaxCode in unknownTaxCodesNames)
				{
					_ = dataSource;
					ParserErrorCollector.Instance.AppendLine($"Unknown tax code founded ({unknownTaxCode})");
				}
			}
		}

		static bool IsTaxCodeKnow(string code)
		{
			switch (code)
			{
				case "1":
				case "2":
				case "3":
				case "4":
				case "5":
				case "6":
				case "7":
					return true;
				default:
					return false;
			}
		}
	}
}
