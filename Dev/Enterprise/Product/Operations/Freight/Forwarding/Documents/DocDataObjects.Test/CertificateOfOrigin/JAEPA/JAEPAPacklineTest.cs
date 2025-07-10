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
	[TestedType(typeof(JAEPALineItem))]
	sealed class JAEPAPacklineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new JAEPALineItem("7193FA09-75CC-4B4B-B23B-8BAEAE639CC5")
			{
				ItemNumber = 1,
				MarksAndNumbers = "marks & numbers",
				GoodsDescription = "goods description",
				PackageCount = 1,
				PackageType = new DummyCodeDescription()
				{
					Code = Core.Constants.PkgUnit.Box
				},
				OriginCriterion = new CodeDescription(new OriginCriterionListJAEPA())
				{
					Code = OriginCriterionListJAEPA.Codes.WO
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
					Date = new ZDateTime(2023, 07, 11)
				}
			};
		}
	}
}
