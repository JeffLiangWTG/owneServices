using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Address
{
	[CodeProperty(nameof(Name)), DescriptionProperty(nameof(AddressLine1))]
	public sealed class AddressBusinessObject : NonPersistentBusinessObject
	{
		public AddressBusinessObject() { }

		public AddressBusinessObject(IAddress address, AddressType addressType)
		{
			Name = address.CompanyName;
			AddressLine1 = address.AddressLine1;
			AddressLine2 = address.AddressLine2;
			City = address.City;
			State = address.State;
			PostCode = address.Postcode;
			CountryName = address.Country.Name;
			AddressType = addressType;
		}

		public ZString Name { get; set; }
		public ZString AddressLine1 { get; set; }
		public ZString AddressLine2 { get; set; }
		public ZString City { get; set; }
		public ZString State { get; set; }
		public ZString PostCode { get; set; }
		public ZString CountryName { get; set; }
		public AddressType AddressType { get; set; }
	}

	public enum AddressType
	{
		EXPORTER,
		PRODUCER
	}
}
