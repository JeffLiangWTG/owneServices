using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.CodeLists;
using Enterprise.Freight.Forwarding.Documents.Testing;
using NUnit.Framework;
using COOUSDDO = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.COOUS;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin.COOUS
{
	[TestedType(typeof(COOUSDDO))]
	internal class COOUSTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new COOUSDDO("Forwarding Shipment", "S00001015")
			{
				LineItems = new[]
				{
					new COOUSLineItem("D4330FC6-30D6-4E12-9F91-595B81EA8B41")
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
							Code = string.Empty
						},
						Quantity = new Measurement()
						{
							Value = 22,
							Unit = new DummyCodeDescription
							{
								Code = Core.Constants.Weight.Kilograms
							}
						},
						Origin = "Australia",
						OriginCode = "AUS",
					}
				},
				ShipmentNumber = "S00001015"
			};
		}
	}
}
