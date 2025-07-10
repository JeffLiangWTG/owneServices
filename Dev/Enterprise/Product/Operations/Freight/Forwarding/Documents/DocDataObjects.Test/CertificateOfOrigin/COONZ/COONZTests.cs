using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.CertificateOfOrigin.Testing
{
	[TestedType(typeof(COONZ))]
	sealed class COONZTests : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new COONZ("ForwardingShipment", "C00001015")
			{
				LineItems = new[]
				{
					new COONZLineItem("222a4468-853c-4bcb-baa1-e4b14216dc95")
					{
						Quantity = new Measurement()
						{
							Value = 22,
							Unit = new DummyCodeDescription
							{
								Code = Core.Constants.Weight.Kilograms
							}
						},
						MarksAndNumbers = "marks & numbers",
						GoodsDescription = "goods description",
						PackageCount = 1,
						Origin = "New Zealand",
						OriginCode = "NZ"
					}
				}
			};
		}
	}
}
