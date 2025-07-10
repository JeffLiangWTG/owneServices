using System.Collections.Generic;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class TariffTributary
	{
		public string dataGeracao { get; set; }
		public List<TratamentosTributariosImportacao> tratamentosTributariosImportacao { get; set; }
	}

}
