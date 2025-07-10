using System.Collections.Generic;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class Condition : ICondition
	{
		public Condition(string code, string taxCode, int? sequenceNumber, string description, string documentCode, string documentType, string actionCode, decimal? amount, string measurementCode, string measurementCodeDescription, string qualifier, string qualifierDescription, List<Component> components)
		{
			Code = code;
			TaxCode = taxCode;
			SequenceNumber = sequenceNumber;
			Description = description;
			DocumentCode = documentCode;
			DocumentType = documentType;
			ActionCode = actionCode;
			Amount = amount;
			MeasurementCode = measurementCode;
			MeasurementCodeDescription = measurementCodeDescription;
			Qualifier = qualifier;
			QualifierDescription = qualifierDescription;
			Components = components;
		}

		public string Code { get; set; }
		public string TaxCode { get; set; }
		public int? SequenceNumber { get; set; }
		public string Description { get; set; }
		public string DocumentCode { get; set; }
		public string DocumentType { get; set; }
		public string ActionCode { get; set; }
		public decimal? Amount { get; set; }
		public string MeasurementCode { get; set; }
		public string MeasurementCodeDescription { get; set; }
		public string Qualifier { get; set; }
		public string QualifierDescription { get; set; }
		public List<Component> Components { get; set; }
		public bool IsRateFormula { get; set; }

		public string Formula => formula ?? (formula = IsRateFormula ? formula = MeasureHelper.BuildFormulaFromComponents(Components, false, ActionCode) : string.Empty);
		string formula;

		public string PlainTextFormula => plainTextFormula ?? (plainTextFormula = IsRateFormula ? plainTextFormula = MeasureHelper.BuildFormulaFromComponents(Components, true, ActionCode) : string.Empty);
		string plainTextFormula;

		public const string dtpDocType = "SNR";
		public const string supDocType = "SUP";
		public const string informative = "INF";
	}
}
