namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public interface IPathwayAttribute
	{
		string Description { get; }
		Condicao ConditionToProceedFormula { get; }
		string NKQuestionChild { get; }
		string NKQuestionStartDateChild { get; }
		string StartDate { get; }
		string EndDate { get; }
		bool AllowMultipleAnswers { get; }
		bool IsAnswerMandatory { get; }
	}
}
