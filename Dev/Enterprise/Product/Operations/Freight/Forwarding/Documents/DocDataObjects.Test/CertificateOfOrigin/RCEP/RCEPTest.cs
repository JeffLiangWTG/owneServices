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
	[TestedType(typeof(RCEP))]
	sealed class RCEPTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new RCEP("ForwardingShipment", "S00001015")
			{
				DocSendingCollection = new DocDataObjects.DocSending.DocSendingBusinessObjectCollection(),
				LineItems = new[]
				{
					new RCEPLineItem("3921D79E-74A1-4952-B3AE-C9449FCA93B0")
					{
						ItemNumber = 1,
						MarksAndNumbers = "marks & numbers",
						GoodsDescription = "goods description",
						PackageCount = 1,
						PackageType = new DummyCodeDescription()
						{
							Code = Core.Constants.PkgUnit.Box
						},
						OriginCriterion = new CodeDescription(new OriginCriterionListRCEP())
						{
							Code = OriginCriterionListRCEP.Codes.WO
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
							Number = "INV0008",
							Date = new ZDateTime(2021, 11, 15),
							Amount = new Money
							{
								Amount = 10.00m,
								Currency = new DummyCodeDescription
								{
									Code = "AUD"
								}
							}
						}
					}
				}
			};
		}
	}
}
