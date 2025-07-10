using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.CodeLists;
using Enterprise.Freight.Forwarding.Documents.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin
{
	[TestedType(typeof(IAECTA))]
	sealed class IAECTATest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new IAECTA("Forwarding Shipment", "S00001015")
			{
				LineItems = new[]
				{
					new IAECTALineItem("F5502572-0968-4C1C-AB89-EB5E90FA2EBA")
					{
						ItemNumber = 1,
						MarksAndNumbers = "marks & numbers",
						GoodsDescription = "goods description",
						PackageCount = 1,
						PackageType = new DummyCodeDescription()
						{
							Code = Core.Constants.PkgUnit.Box
						},
						OriginCriterion = new CodeDescription(new OriginCriterionListIAECTA())
						{
							Code = OriginCriterionListIAECTA.Codes.WO
						},
						Quantity = new Measurement()
						{
							Value = 22,
							Unit = new DummyCodeDescription
							{
								Code = Core.Constants.Weight.Kilograms
							}
						}
					}
				}
			};
		}
	}
}
