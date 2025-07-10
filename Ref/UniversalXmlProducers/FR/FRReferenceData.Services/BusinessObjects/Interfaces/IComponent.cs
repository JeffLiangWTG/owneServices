namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public interface IComponent
	{
		string Code { get; set; }				//Expression de droit : can be an agricultural component, a maximum, a percentage ...
		decimal? Amount { get; set; }				//Amount
		string Currency { get; set; }			//Currency
		string MeasurementCode { get; set; }    //Measurement Code
		string MeasurementCodeDescription { get; set; }
		string Qualifier { get; set; }			//UOM qualifier
		string QualifierDescription { get; set; }
	}
}
