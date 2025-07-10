using System.Collections.Generic;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class Aplica
	{
		public bool indicadorTodosNCMs { get; set; }
		public List<Ncm> ncms { get; set; }
		public bool indicadorTodosPaises { get; set; }
		public List<Paise> paises { get; set; }
		public List<Bloco> blocos { get; set; }
	}

}
