using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public interface IDeclaration
	{
		ZGuid PK { get; }
		ZDate US_DutyCalcDate { get; }
		bool ShouldCalculateMPFAndDutyDate { get; }
		ZBool IsExWarehouse { get; }
		bool IsQuota { get; }
		bool IsACECargoCertificationMode { get; }
		bool IsBorderMovement { get; }
		ZDateTime US_ITDate { get; }
		ZDateTime US_EstimatedEntryDate { get; }
		ZDateTime US_PreliminaryStatementPrintDate { get; }
		ZDateTime US_EntryDate { get; }
		ZDateTime US_PresentationDate { get; }
		ZDateTime JE_EntryAuthorisationDate { get; }
	}
}
