using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.NZ;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin.NZ
{
	[TestedType(typeof(NZCFTALineItem))]
	sealed class NZCFTALineItemTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new NZCFTALineItem("7585f854-b675-4853-b2ab-e41f0eac611e")
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
				OriginCriterion = new CodeDescription(new OriginCriterionListNZCFTA())
				{
					Code = OriginCriterionListNZCFTA.Codes.WO
				},
				Invoice = new Invoice()
				{
					Number = "INV0001",
					Date = new ZDateTime(2023, 07, 11),
					Amount = new Money
					{
						Amount = 10.00m,
						Currency = new DummyCodeDescription
						{
							Code = "AUD"
						}
					}
				}
			};
		}
	}
}
