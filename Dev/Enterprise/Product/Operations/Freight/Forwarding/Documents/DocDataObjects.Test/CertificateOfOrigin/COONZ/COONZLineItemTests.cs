using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.CertificateOfOrigin.Testing
{
	[TestedType(typeof(DocDataObjects.CertificateOfOrigin.COONZLineItem))]
	sealed class COONZLineItemTests : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocDataObjects.CertificateOfOrigin.COONZLineItem("7585f854-b675-4853-b2ab-e41f0eac611e")
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
				GoodsDescription = "goods description"
			};
		}
	}
}
