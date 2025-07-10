using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Address;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin.Supporters.Address
{
	[TestedType(typeof(AddressBusinessObjectConfiguration))]
	sealed class AddressBusinessObjectConfigurationTest : NonPersistentBusinessObjectCollectionTestCase<AddressBusinessObjectConfiguration>
	{
		protected override AddressBusinessObjectConfiguration GetCollectionToTest()
			=> new AddressBusinessObjectConfiguration(certificateName: "NZCFTA", enableUnknown: true);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new AddressBusinessObject()
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
