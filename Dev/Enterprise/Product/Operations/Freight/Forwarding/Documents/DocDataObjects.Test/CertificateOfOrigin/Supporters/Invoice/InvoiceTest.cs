using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice;
using Enterprise.Freight.Forwarding.Documents.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin.Supporters.Address
{
	[TestedType(typeof(Invoice))]
	class InvoiceTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new Invoice()
		{
			Number = "123",
			Date = ZDateTime.UtcNow,
			Amount = new Money()
			{
				Amount = 123.45m,
				Currency = new DummyCodeDescription()
				{
					Code = "USD"
				}
			}
		};
	}
}
