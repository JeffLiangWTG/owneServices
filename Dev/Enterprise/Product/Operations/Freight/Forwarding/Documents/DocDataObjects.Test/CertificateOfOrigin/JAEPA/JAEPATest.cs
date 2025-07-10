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
	[TestedType(typeof(JAEPA))]
	sealed class JAEPATest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new JAEPA("ForwardingShipment", "S00001015")
			{
				LineItems = new[]
				{
					new JAEPALineItem("6579AA4E-B8D6-47D6-B634-1A21C9D04709")
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
					}
				}
			};
		}
	}
}
