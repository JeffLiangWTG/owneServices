using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.CertificateOfOrigin.Testing
{
	[TestedType(typeof(IACEPALineItem))]
	sealed class IACEPALineItemTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new IACEPALineItem("2A44802E-D3B7-4171-BC00-135494CE3A34")
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
			};
		}
	}
}
