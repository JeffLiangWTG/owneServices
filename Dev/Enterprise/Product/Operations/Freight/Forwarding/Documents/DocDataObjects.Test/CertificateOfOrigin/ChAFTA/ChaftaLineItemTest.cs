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
	[TestedType(typeof(ChaftaLineItem))]
	sealed class ChaftaLineItemTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ChaftaLineItem("7585f854-b675-4853-b2ab-e41f0eac611e")
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
					Number = "ABCD0008",
					Date = new ZDateTime(2020, 08, 02)
				}
			};
		}
	}
}
