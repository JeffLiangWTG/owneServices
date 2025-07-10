using System.Runtime.CompilerServices;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class Component : IComponent
	{
		public Component(string code, decimal? amount, string currency, string measurementCode, string measurementCodeDescription, string qualifier, string qualifierDescription)
		{
			this.Code = code;
			this.Amount = amount;
			this.Currency = currency;
			this.MeasurementCode = measurementCode;
			this.MeasurementCodeDescription = measurementCodeDescription;
			this.Qualifier = qualifier;
			this.QualifierDescription = qualifierDescription;
		}

		public string Code { get; set; }
		public decimal? Amount { get; set; }
		public string Currency { get; set; }
		public string MeasurementCode { get; set; }
		public string MeasurementCodeDescription { get; set; }
		public string Qualifier { get; set; }
		public string QualifierDescription { get; set; }
	}
}
