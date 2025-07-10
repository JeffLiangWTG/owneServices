using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice;
using Enterprise.Freight.Forwarding.Documents.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.CertificateOfOrigin.Testing
{
	[TestedType(typeof(Chafta))]
	sealed class ChaftaTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new Chafta("ForwardingShipment", "C00001015")
			{
				LineItems = new[]
				{
					new ChaftaLineItem("222a4468-853c-4bcb-baa1-e4b14216dc95")
					{
						Quantity = new Measurement()
						{
							Value = 22,
							Unit = new DummyCodeDescription
							{
								Code = Core.Constants.Weight.Kilograms
							}
						},
						ItemNumber = 789,
						MarksAndNumbers = "marks & numbers",
						GoodsDescription = "goods description",
						OriginCriterion = new CodeDescription(new OriginCriterionList())
						{
							Code = OriginCriterionList.Codes.WO
						},
						Invoice = new Invoice()
						{
							Number = "INV0001",
							Date = new ZDateTime(2023, 07, 11)
						}
					}
				}
			};
		}
	}
}
