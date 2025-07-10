using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.NZ;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin.NZ
{
	[TestedType(typeof(NZCFTA))]
	sealed class NZCFTATest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new NZCFTA("ForwardingShipment", "C00001015")
			{
				DocSendingCollection = new DocDataObjects.DocSending.DocSendingBusinessObjectCollection(),
				LineItems = new[]
				{
					new NZCFTALineItem("222a4468-853c-4bcb-baa1-e4b14216dc95")
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
						}
					}
				}
			};
		}
	}
}
