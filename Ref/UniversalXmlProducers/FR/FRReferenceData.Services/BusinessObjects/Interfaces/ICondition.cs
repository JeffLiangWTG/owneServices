using System.Collections.Generic;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public interface ICondition
	{
		string Code { get; set; }                   //Condition description code
		string TaxCode { get; set; }				//Condition tax code (for excise)
		int? SequenceNumber { get; set; }			//Condition Sequence Number
		string DocumentCode { get; set; }			//Document to show for the condition to apply
		string DocumentType { get; set; }			//0 =  document, 1 = disposition tarfaire particulière
		string ActionCode { get; set; }             //Action led by the condition
		decimal? Amount { get; set; }                 //Amount tied to the condition
		string MeasurementCode { get; set; }        //Unit of measure
		string MeasurementCodeDescription { get; set; }
		string Qualifier { get; set; }				//UOM qualifier
		string QualifierDescription { get; set; }
		List<Component> Components { get; set; }	//Calculation components
	}
}
