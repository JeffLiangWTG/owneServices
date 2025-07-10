using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Address;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin.Supporters.Address
{
	[TestedType(typeof(AddressBusinessObject))]
	sealed class AddressBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new AddressBusinessObject
		{
			Name = "Name",
			AddressLine1 = "AddressLine1",
			AddressLine2 = "AddressLine2",
			City = "City",
			State = "State",
			PostCode = "12345",
			CountryName = "New Zealand"
		};
	}
}
