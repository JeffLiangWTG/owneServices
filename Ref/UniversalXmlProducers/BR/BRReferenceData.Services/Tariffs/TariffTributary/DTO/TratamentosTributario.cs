using System.Collections.Generic;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class TratamentosTributario
	{
		public Tributo tributo { get; set; }
		public Regime regime { get; set; }
		public FundamentoLegal fundamentoLegal { get; set; }
		public Vigencia vigencia { get; set; }
		public List<Mercadoria> mercadorias { get; set; }
	}

}
