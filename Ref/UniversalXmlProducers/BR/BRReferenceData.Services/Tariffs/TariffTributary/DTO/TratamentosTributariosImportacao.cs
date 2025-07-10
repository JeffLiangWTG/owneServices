using System.Collections.Generic;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class TratamentosTributariosImportacao
	{
		public Ncm ncm { get; set; }
		public PaisesBlocos paisesBlocos { get; set; }
		public List<TratamentosTributario> tratamentosTributarios { get; set; }
	}

}
