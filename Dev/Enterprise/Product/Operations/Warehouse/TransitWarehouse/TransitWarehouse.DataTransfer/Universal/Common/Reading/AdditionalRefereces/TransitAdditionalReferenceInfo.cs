using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	[XsdSchema(Placement.Outer)]
	public class TransitAdditionalReferenceInfo : IDataObject
	{
		[Mandatory, CandidateKey]
		public ZString? Type { get; set; }
		public ZString? Value { get; set; }
		public ZString? Category { get; set; }
		public ZString? CountryCode { get; set; }
		public ZString? SourceType { get; set; }
	}
}
