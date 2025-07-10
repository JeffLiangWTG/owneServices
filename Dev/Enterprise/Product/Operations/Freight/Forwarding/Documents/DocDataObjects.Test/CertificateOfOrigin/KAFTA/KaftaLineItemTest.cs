using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.CodeLists;
using Enterprise.Freight.Forwarding.Documents.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin
{
	[TestedType(typeof(KAFTALineItem))]
	sealed class KaftaLineItemTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new KAFTALineItem("990134E1-1186-49C1-833B-5A72B92A210C")
			{
				ItemNumber = 1,
				MarksAndNumbers = "marks & numbers",
				GoodsDescription = "goods description",
				PackageCount = 1,
				PackageType = new DummyCodeDescription()
				{
					Code = Core.Constants.PkgUnit.Box
				},
				OriginCriterion = new CodeDescription(new OriginCriterionListKAFTA())
				{
					Code = OriginCriterionListKAFTA.Codes.WO
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
