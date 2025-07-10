using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.CertificateOfOrigin.Testing
{
	[TestedType(typeof(IACEPA))]
	sealed class IACEPATest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new IACEPA("ForwardingShipment", "S00001015")
			{
				LineItems = new[]
				{
					new IACEPALineItem("6579AA4E-B8D6-47D6-B634-1A21C9D04709")
					{
						ItemNumber = 1,
						GoodsDescription = "goods description",
						PackageCount = 1,
						PackageType = new DummyCodeDescription()
						{
							Code = Core.Constants.PkgUnit.Box
						},
						OriginCriterion = new CodeDescription(new OriginCriterionListIACEPA())
						{
							Code = OriginCriterionListIACEPA.Codes.WO
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
