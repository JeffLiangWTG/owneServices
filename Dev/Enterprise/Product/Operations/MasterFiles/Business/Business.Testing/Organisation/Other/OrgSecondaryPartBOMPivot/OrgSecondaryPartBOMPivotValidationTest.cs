using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	class OrgSecondaryPartBOMPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOPP_OE_Component()
		{
			var mainProduct = Factory.New<OrgSupplierPart>();
			var secondaryPart1 = mainProduct.SecondaryParts.AddNew();
			var componentPivot1 = secondaryPart1.ComponentUsages.AddNew();
			AssertNoErrors("Precondition", componentPivot1.OPP_OE_ComponentInfo);

			componentPivot1.Validation.ValidateOPP_OE_Component();
			AssertHasError(componentPivot1.OPP_OE_ComponentInfo, "Please enter a Component.");

			var bomComponent1 = Factory.New<OrgPartBOM>();
			componentPivot1.OPP_OE_Component = bomComponent1.PK;
			AssertNoErrors(componentPivot1.OPP_OE_ComponentInfo);

			var componentPivot2 = secondaryPart1.ComponentUsages.AddNew();
			componentPivot2.OPP_OE_Component = bomComponent1.PK;
			AssertHasError(componentPivot2.OPP_OE_ComponentInfo, "Cannot Select the same BOM Component twice.");

			var bomComponent2 = Factory.New<OrgPartBOM>();
			componentPivot2.OPP_OE_Component = bomComponent2.PK;
			AssertNoErrors(componentPivot2.OPP_OE_ComponentInfo);

			var secondaryPart2 = mainProduct.SecondaryParts.AddNew();
			var componentPivotOnSecondaryPart2 = secondaryPart2.ComponentUsages.AddNew();
			componentPivotOnSecondaryPart2.OPP_OE_Component = bomComponent1.PK;
			AssertNoErrors(componentPivotOnSecondaryPart2.OPP_OE_ComponentInfo);

			componentPivotOnSecondaryPart2.OPP_OE_Component = bomComponent2.PK;
			AssertNoErrors(componentPivotOnSecondaryPart2.OPP_OE_ComponentInfo);
		}

		public void TestCheckOPP_ComponentQuantity()
		{
			var part = Factory.New<OrgSupplierPart>();
			var secondaryPart = part.SecondaryParts.AddNew();
			var componentPivot = secondaryPart.ComponentUsages.AddNew();
			AssertNoErrors("Precondition", componentPivot.OPP_ComponentQuantityInfo);

			componentPivot.Validation.ValidateOPP_ComponentQuantity();
			AssertHasError(componentPivot.OPP_ComponentQuantityInfo, "Quantity Used cannot be zero.");

			componentPivot.OPP_ComponentQuantity = -1;
			AssertHasError(componentPivot.OPP_ComponentQuantityInfo, "Quantity Used cannot be negative.");

			componentPivot.OPP_ComponentQuantity = 1;
			AssertNoErrors(componentPivot.OPP_ComponentQuantityInfo);
		}

		public void TestCheckOPP_ComponentQuantity_MustNotBeGreaterThanTotalComponentQuantity()
		{
			var part = Factory.New<OrgSupplierPart>();
			var secondaryPart1 = part.SecondaryParts.AddNew();
			var componentPivot1 = secondaryPart1.ComponentUsages.AddNew();
			AssertNoErrors("Precondition", componentPivot1.OPP_ComponentQuantityInfo);

			var componentProduct = Factory.New<OrgSupplierPart>();
			var partUnit = componentProduct.PartUnits.AddNew();
			partUnit.OF_PackType = componentProduct.OP_StockKeepingUnit;
			partUnit.OF_ParentPackType = "BOX";
			partUnit.OF_QuantityInParent = 6m;

			var bomComponent1 = part.BillOfMaterials.AddNew();
			bomComponent1.OE_OP_Component = componentProduct.PK;
			bomComponent1.OE_F3_NKPackType = "BOX";
			bomComponent1.OE_ComponentQty = 2m;

			var bomComponent2 = part.BillOfMaterials.AddNew();
			bomComponent2.OE_OP_Component = Factory.New<OrgSupplierPart>().PK;
			bomComponent2.OE_ComponentQty = 9m;

			componentPivot1.OPP_OE_Component = bomComponent1.PK;
			componentPivot1.OPP_ComponentQuantity = 1m;
			AssertNoErrors(componentPivot1.OPP_ComponentQuantityInfo);

			componentPivot1.OPP_ComponentQuantity = 6m;
			AssertNoErrors(componentPivot1.OPP_ComponentQuantityInfo);

			componentPivot1.OPP_ComponentQuantity = 12m;
			AssertNoErrors(componentPivot1.OPP_ComponentQuantityInfo);

			componentPivot1.OPP_ComponentQuantity = 5m;
			AssertNoErrors(componentPivot1.OPP_ComponentQuantityInfo);

			componentPivot1.OPP_ComponentQuantity = 13m;
			AssertHasError(componentPivot1.OPP_ComponentQuantityInfo, "Cannot have Total Component Quantity used (13) greater than the Total Component Stock Quantity (12) on the BOM Component.");

			componentPivot1.OPP_ComponentQuantity = 4m;
			AssertNoErrors(componentPivot1.OPP_ComponentQuantityInfo);

			var secondaryPart2 = part.SecondaryParts.AddNew();
			var componentPivot2 = secondaryPart2.ComponentUsages.AddNew();
			componentPivot2.OPP_OE_Component = bomComponent1.PK;
			componentPivot2.OPP_ComponentQuantity = 7m;
			AssertNoErrors(componentPivot2.OPP_ComponentQuantityInfo);

			componentPivot2.OPP_ComponentQuantity = 9m;
			AssertHasError(componentPivot2.OPP_ComponentQuantityInfo, "Cannot have Total Component Quantity used (13) greater than the Total Component Stock Quantity (12) on the BOM Component.");

			componentPivot2.OPP_ComponentQuantity = 7m;
			componentPivot2.OPP_OE_Component = bomComponent2.PK;
			AssertNoErrors(componentPivot2.OPP_ComponentQuantityInfo);

			componentPivot2.OPP_ComponentQuantity = 9m;
			AssertNoErrors(componentPivot1.OPP_ComponentQuantityInfo);

			componentPivot2.OPP_ComponentQuantity = 10m;
			AssertHasError(componentPivot2.OPP_ComponentQuantityInfo, "Cannot have Total Component Quantity used (10) greater than the Total Component Stock Quantity (9) on the BOM Component.");

			componentPivot2.OPP_ComponentQuantity = 7m;
			AssertNoErrors(componentPivot1.OPP_ComponentQuantityInfo);
		}
	}
}
