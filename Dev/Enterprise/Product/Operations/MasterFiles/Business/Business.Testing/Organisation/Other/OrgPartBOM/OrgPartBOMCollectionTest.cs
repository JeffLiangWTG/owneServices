using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgPartBOMCollection))]
	sealed class OrgPartBOMCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgPartBOMCollection>
	{
		#region TestIsReusable

		#region TestIsReusableComponent

		public void TestIsReusableComponent()
		{
			var mainPart = Factory.New<OrgSupplierPart>();

			var subPart1 = Factory.New<OrgSupplierPart>();
			var subPart2 = Factory.New<OrgSupplierPart>();
			var subPart3 = Factory.New<OrgSupplierPart>();

			CreatePartBOM(mainPart, subPart1, "UNT", true);
			CreatePartBOM(mainPart, subPart1, "BOX", false); // duplicate
			CreatePartBOM(mainPart, subPart2, "UNT", true);
			CreatePartBOM(mainPart, subPart3, "UNT", false);

			AssertEquals("Precondition:", 4, mainPart.BillOfMaterials.Count);
			AssertEquals("UNT subPart1 should be reusable.", true, mainPart.BillOfMaterials.IsReusableComponent(subPart1, "UNT"));
			AssertEquals("BOX subPart1 should not be reusable.", false, mainPart.BillOfMaterials.IsReusableComponent(subPart1, "BOX"));
			AssertEquals("UNT subPart2 should be reusable.", true, mainPart.BillOfMaterials.IsReusableComponent(subPart2, "UNT"));
			AssertEquals("UNT subPart3 should not be reusable.", false, mainPart.BillOfMaterials.IsReusableComponent(subPart3, "UNT"));
		}

		#endregion

		#region TestIsReusableComponent_ChildProductIsNotAChild

		[ExpectException(typeof(InvalidOperationException))]
		public void TestIsReusableComponent_ChildProductIsNotAChild()
		{
			var mainPart = Factory.New<OrgSupplierPart>();
			var subPart1 = Factory.New<OrgSupplierPart>();
			CreatePartBOM(mainPart, subPart1, "UNT", true);

			var unrelatedPart = Factory.New<OrgSupplierPart>();
			mainPart.BillOfMaterials.IsReusableComponent(unrelatedPart, "UNT");
		}

		#endregion

		#region TestIsReusableComponent_ChildWithTheStockUnitDoesNotExist

		[ExpectException(typeof(InvalidOperationException))]
		public void TestIsReusableComponent_ChildWithTheStockUnitDoesNotExist()
		{
			var mainPart = Factory.New<OrgSupplierPart>();
			var subPart1 = Factory.New<OrgSupplierPart>();
			CreatePartBOM(mainPart, subPart1, "UNT", true);
			mainPart.BillOfMaterials.IsReusableComponent(subPart1, "BOX");
		}

		#endregion

		#endregion

		#region TestFindByComponentPKandPackType

		#region FindByComponentPKandPackType_ThrowsExceptionWithNullArguments

		[ExpectException(typeof(ArgumentException))]
		public void FindByComponentPKandPackType_ThrowsExceptionWithNullPackType()
		{
			var mainPart = Factory.New<OrgSupplierPart>();
			var subPart1 = Factory.New<OrgSupplierPart>();
			CreatePartBOM(mainPart, subPart1, "UNT", true);

			mainPart.BillOfMaterials.FindByComponentPKandPackType(subPart1.PK, null);
		}

		#endregion

		#region FindByComponentPKandPackType_RetrivesCorrectProduct

		public void FindByComponentPKandPackType_RetrivesCorrectProduct()
		{
			var mainPart = Factory.New<OrgSupplierPart>();
			var childBomPart1 = Factory.New<OrgSupplierPart>();
			var childBomPart2 = Factory.New<OrgSupplierPart>();
			CreatePartBOM(mainPart, childBomPart1, "UNT", true);
			CreatePartBOM(mainPart, childBomPart1, "CTN", true);
			CreatePartBOM(mainPart, childBomPart2, "CTN", true);

			// Should retrieve the 2nd of the 3 bomParts
			var matchedPart1 = mainPart.BillOfMaterials.FindByComponentPKandPackType(childBomPart1.PK, "CTN");
			var matchedPart2 = mainPart.BillOfMaterials.FindByComponentPKandPackType(childBomPart2.PK, "CTN");
			var unmatchedPart = mainPart.BillOfMaterials.FindByComponentPKandPackType(childBomPart1.PK, "BOX");

			AssertEquals("Matched part should be packtype CTN.", "CTN", matchedPart1.OE_F3_NKPackType);
			AssertEquals("Matched part should be packtype CTN.", "CTN", matchedPart2.OE_F3_NKPackType);
			AssertEquals("Matched part should be childBomPart.", childBomPart1.PK, matchedPart1.OE_OP_Component);
			AssertEquals("Matched part should be childBomPart.", childBomPart2.PK, matchedPart2.OE_OP_Component);
			AssertNull("Should return null for unmatched part.", unmatchedPart);
		}

		#endregion

		#endregion

		#region TestFilterConstructor

		public void TestFilterConstructor()
		{
			var mainPart = Factory.New<OrgSupplierPart>();

			var subPart1 = Factory.New<OrgSupplierPart>();
			var subPart2 = Factory.New<OrgSupplierPart>();
			var subPart3 = Factory.New<OrgSupplierPart>();

			var bom1 = CreatePartBOM(mainPart, subPart1, "UNT");
			bom1.OE_ExcludeForVirtualWarehouse = true;
			var bom2 = CreatePartBOM(mainPart, subPart2, "UNT");
			var bom3 = CreatePartBOM(mainPart, subPart3, "UNT");
			bom3.OE_ExcludeForVirtualWarehouse = true;

			var collectionTrue = new OrgPartBOMCollection(mainPart, new ZQuery(OrgPartBOMSchema.OE_ExcludeForVirtualWarehouse, true));
			AssertEquals("Collection should contain BOM1 & BOM3.", true, collectionTrue.ContainsSameElementsInAnyOrder(new[] { bom1, bom3 }));

			var collectionFalse = new OrgPartBOMCollection(mainPart, new ZQuery(OrgPartBOMSchema.OE_ExcludeForVirtualWarehouse, false));
			AssertEquals("Collection should contain BOM2 only.", true, collectionFalse.ContainsSameElementsInAnyOrder(new[] { bom2 }));
		}

		#endregion

		#region Implementation

		protected override OrgPartBOMCollection GetCollectionToTest()
		{
			return new OrgPartBOMCollection(Part);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			OrgPartBOM bom = Factory.New<OrgPartBOM>();
			bom.OE_OP_MainProduct = Part.PK;
			return bom;
		}

		OrgPartBOM CreatePartBOM(OrgSupplierPart mainPart, OrgSupplierPart subPart, ZString stockUnits, bool isReusable = true)
		{
			var partBOM = Factory.New<OrgPartBOM>();
			partBOM.OE_OP_MainProduct = mainPart.PK;
			partBOM.OE_OP_Component = subPart.PK;
			partBOM.OE_F3_NKPackType = stockUnits;
			partBOM.OE_CanReuse = isReusable;

			return partBOM;
		}

		OrgSupplierPart Part
		{
			get { return part ?? (part = Factory.New<OrgSupplierPart>()); }
		}

		OrgSupplierPart part;

		#endregion
	}
}
