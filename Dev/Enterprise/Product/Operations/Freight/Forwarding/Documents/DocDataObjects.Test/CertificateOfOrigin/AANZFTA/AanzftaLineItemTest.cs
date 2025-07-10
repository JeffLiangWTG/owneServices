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
	[TestedType(typeof(AanzftaLineItem))]
	sealed class AanzftaLineItemTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new AanzftaLineItem("990134e1-1186-49c1-833b-5a72b92a210c")
			{
				ItemNumber = 1,
				MarksAndNumbers = "marks & numbers",
				GoodsDescription = "goods description",
				PackageCount = 1,
				PackageType = new DummyCodeDescription()
				{
					Code = Core.Constants.PkgUnit.Box
				},
				OriginCriterion = new CodeDescription(new OriginCriterionListAanzfta())
				{
					Code = OriginCriterionListAanzfta.Codes.WO
				},
				Quantity = new Measurement()
				{
					Value = 22,
					Unit = new DummyCodeDescription
					{
						Code = Core.Constants.Weight.Kilograms
					}
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
