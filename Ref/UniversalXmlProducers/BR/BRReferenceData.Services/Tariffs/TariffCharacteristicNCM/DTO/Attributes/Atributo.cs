using System.Collections.Generic;
namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class Atributo : IPathwayAttribute
	{
		public string codigo { get; set; }
		public string nome { get; set; }
		public string definicao { get; set; }
		public string nomeApresentacao { get; set; }
		public string orientacaoPreenchimento { get; set; }
		public string formaPreenchimento { get; set; }
		public string dataInicioVigencia { get; set; }
		public string dataFimVigencia { get; set; }
		public string modalidade { get; set; }
		public int tamanhoMaximo { get; set; }
		public int casasDecimais { get; set; }
		public bool obrigatorio { get; set; }
		public bool multivalorado { get; set; }
		public List<Dominio> dominio { get; set; }
		public List<Objetivo> objetivos { get; set; }
		public List<string> orgaos { get; set; }
		public bool atributoCondicionante { get; set; }
		public List<Condicionado> condicionados { get; set; }
		public List<Atributo> listaSubatributos { get; set; }
		public string Description => nomeApresentacao;
		public Condicao ConditionToProceedFormula => null;
		public string NKQuestionChild => codigo;
		public string NKQuestionStartDateChild => dataInicioVigencia;
		public string StartDate => dataInicioVigencia;
		public string EndDate => dataFimVigencia;
		public bool AllowMultipleAnswers => multivalorado;
		public bool IsAnswerMandatory => obrigatorio;
	}
}
