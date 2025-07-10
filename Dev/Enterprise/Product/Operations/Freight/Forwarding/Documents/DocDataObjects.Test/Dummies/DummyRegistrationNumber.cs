using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class DummyRegistrationNumber : IRegistrationNumber
	{
		public ICodeDescription Type { get; set; }
		public ICountry CountryOfIssue { get; set; }
		public ZString Value { get; set; }
	}
}
