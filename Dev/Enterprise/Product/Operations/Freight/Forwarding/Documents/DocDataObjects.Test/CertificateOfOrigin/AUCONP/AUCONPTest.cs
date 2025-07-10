using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.CertificateOfOrigin.Testing
{
	[TestedType(typeof(AUCONP))]
	sealed class AUCONPTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new AUCONP("ForwardingShipment", "S00001015")
			{
				LineItems = new[]
				{
					new AUCONPLineItem("4ba71ab5-9981-415e-affc-9ac11a466977")
					{
						ItemNumber = 1,
						MarksAndNumbers = "marks & numbers",
						GoodsDescription = "goods description",
						PackageCount = 1,
						PackageType = new DummyCodeDescription()
						{
							Code = Core.Constants.PkgUnit.Box
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
