using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.CodeLists;
using Enterprise.Freight.Forwarding.Documents.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin
{
	[TestedType(typeof(TAFTA))]
	sealed class TAFTATest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new TAFTA("ForwardingShipment", "S00001015")
			{
				LineItems = new[]
				{
					new TAFTALineItem("06070CD9-8450-45DF-87FF-CD3B1EF26AE7")
					{
						ItemNumber = 1,
						ProductNumber = "PN001",
						MarksAndNumbers = "marks & numbers",
						GoodsDescription = "goods description",
						PackageCount = 1,	
						PackageType = new DummyCodeDescription()
						{
							Code = Core.Constants.PkgUnit.Box
						},
						OriginCriterion = new CodeDescription(new OriginCriterionListTAFTA())
						{
							Code = OriginCriterionListTAFTA.Codes.WO
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
