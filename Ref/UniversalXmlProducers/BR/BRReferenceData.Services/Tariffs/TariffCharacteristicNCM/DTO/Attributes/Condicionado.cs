namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class Condicionado : IPathwayAttribute
	{
		public bool obrigatorio { get; set; }
		public bool multivalorado { get; set; }
		public string dataInicioVigencia { get; set; }
		public string dataFimVigencia { get; set; }
		public string descricaoCondicao { get; set; }
		public Condicao condicao { get; set; }
		public Atributo atributo { get; set; }
		public string Description => descricaoCondicao;
		public Condicao ConditionToProceedFormula => condicao;
		public string NKQuestionChild => atributo?.codigo;
		public string NKQuestionStartDateChild => atributo?.StartDate;
		public string StartDate => dataInicioVigencia;
		public string EndDate => dataFimVigencia;
		public bool AllowMultipleAnswers => multivalorado;
		public bool IsAnswerMandatory => obrigatorio;
	}
}
