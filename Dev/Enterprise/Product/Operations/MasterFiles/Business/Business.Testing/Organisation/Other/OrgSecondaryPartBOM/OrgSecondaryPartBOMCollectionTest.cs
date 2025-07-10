using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSecondaryPartBOMCollection))]
	class OrgSecondaryPartBOMCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgSecondaryPartBOMCollection>
	{
		public void TestProductConstructor_InvalidArgs()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new OrgSecondaryPartBOMCollection((OrgSupplierPart)null));
		}

		public void TestProductConstructor()
		{
			var mainProduct1 = Factory.New<OrgSupplierPart>();
			var mainProduct2 = Factory.New<OrgSupplierPart>();
			var collection1 = new OrgSecondaryPartBOMCollection(mainProduct1);
			var collection2 = new OrgSecondaryPartBOMCollection(mainProduct2);
			var secondaryPart1 = collection1.AddNew();
			AssertEquals(mainProduct1.PK, secondaryPart1.OSB_OP_MainProduct);
			AssertContainsExactElementsInAnyOrder(new[] { secondaryPart1 }, collection1);
			AssertEquals(0, collection2.Count);

			var secondaryPart2 = Factory.New<OrgSecondaryPartBOM>();
			secondaryPart2.OSB_OP_MainProduct = mainProduct2.PK;
			AssertContainsExactElementsInAnyOrder(new[] { secondaryPart1 }, collection1);
			AssertContainsExactElementsInAnyOrder(new[] { secondaryPart2 }, collection2);
		}
	}
}
