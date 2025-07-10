using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Integration.Warehouse;
using Moq;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgPartBOMValidationTest : BusinessObjectValidationTestCase
	{
		#region TestComponentQty

		public void TestComponentQty()
		{
			var b = CreateNewWithPart(ParentPart, "B");
			b.OE_ComponentQty = 1;
			AssertEquals(false, b.OE_ComponentQtyInfo.HasErrors());

			b.OE_ComponentQty = 0;
			AssertEquals(true, b.OE_ComponentQtyInfo.HasErrors());

			b.OE_ComponentQty = -1;
			AssertEquals(true, b.OE_ComponentQtyInfo.HasErrors());

			b.OE_ComponentQty = 1;
			AssertEquals(false, b.OE_ComponentQtyInfo.HasErrors());
		}

		#endregion

		#region TestComponentPack

		public void TestComponentPack()
		{
			var b = CreateNewWithPart(ParentPart, "B");
			b.OE_F3_NKPackType = "XYZ";
			AssertEquals(true, b.OE_F3_NKPackTypeInfo.HasErrors());

			b.Component.OP_StockKeepingUnit = "BAG";
			b.OE_F3_NKPackType = "BAG";
			AssertEquals(false, b.OE_F3_NKPackTypeInfo.HasErrors());

			b.OE_F3_NKPackType = "";
			AssertEquals(true, b.OE_F3_NKPackTypeInfo.HasErrors());

			b.Component.OP_StockKeepingUnit = "CTN";
			b.OE_F3_NKPackType = "CTN";
			AssertEquals(false, b.OE_F3_NKPackTypeInfo.HasErrors());
		}

		#endregion

		#region Test Circular Reference Validation

		public void TestCircularReference()
		{
			//							A
			//						/	|	\
			//					B		C		D

			var b = CreateNewWithPart(ParentPart, "B");
			var c = CreateNewWithPart(ParentPart, "C");
			var d = CreateNewWithPart(ParentPart, "D");

			AssertEquals(false, b.OE_OP_ComponentInfo.HasErrors());
			AssertEquals(false, c.OE_OP_ComponentInfo.HasErrors());
			AssertEquals(false, d.OE_OP_ComponentInfo.HasErrors());

			//							A
			//						/	|	\
			//					B		C		D
			//				/		\
			//			E				F
			//		/		\
			//	G				H

			var e = CreateNewWithPart(b.Component, "E");
			var f = CreateNewWithPart(b.Component, "F");
			var g = CreateNewWithPart(e.Component, "G");
			var h = CreateNewWithPart(e.Component, "H");

			b.Validation.ValidateOE_OP_Component();
			AssertEquals(false, b.OE_OP_ComponentInfo.HasErrors());

			//							A
			//						/	|	\
			//					B		C		D
			//				/		\
			//			E				F
			//		/		\
			//	G				H
			//						\
			//							A (Circular)

			var circularPart = h.Component.BillOfMaterials.AddNew();
			circularPart.OE_OP_Component = ParentPart.PK;

			b.Validation.ValidateOE_OP_Component();
			AssertEquals(true, b.OE_OP_ComponentInfo.HasErrors());

			//							A
			//						/	|	\
			//					B		C		D
			//				/		\
			//			E				F
			//		/		\
			//	G				H
			//						\
			//							E (Circular)

			circularPart.OE_OP_Component = e.Component.PK;

			b.Validation.ValidateOE_OP_Component();
			AssertEquals(true, b.OE_OP_ComponentInfo.HasErrors());

			h.Component.BillOfMaterials.Delete(circularPart);

			//							A
			//						/	|	\
			//					B		C		D
			//				/		\
			//			E				F
			//		/		\				\
			//	G				H				E

			var noncircularPart = f.Component.BillOfMaterials.AddNew();
			noncircularPart.OE_OP_Component = e.Component.PK;

			b.Validation.ValidateOE_OP_Component();
			AssertEquals(false, b.OE_OP_ComponentInfo.HasErrors());
		}

		public void TestCircularReference1()
		{
			//							A
			//						/
			//					B	
			//				/
			//			A

			var b = CreateNewWithPart(ParentPart, "B");

			var circularPart = b.Component.BillOfMaterials.AddNew();
			circularPart.OE_OP_Component = ParentPart.PK;

			b.Validation.ValidateOE_OP_Component();
			AssertEquals(true, b.OE_OP_ComponentInfo.HasErrors());
		}

		public void TestCircularReference2()
		{
			//							A
			//						/
			//					B	
			//				/
			//			C
			//		/
			//	B

			var b = CreateNewWithPart(ParentPart, "B");
			var c = CreateNewWithPart(b.Component, "C");

			var circularPart = c.Component.BillOfMaterials.AddNew();
			circularPart.OE_OP_Component = b.Component.PK;

			b.Validation.ValidateOE_OP_Component();
			AssertEquals(true, b.OE_OP_ComponentInfo.HasErrors());
		}

		public void TestCircularReference3()
		{
			ParentPart.OP_PartNum = "A";
			//							A
			//						/	
			//					B		
			//				/		\
			//			E				F
			//		/		\				\
			//	G				H				E

			var b = CreateNewWithPart(ParentPart, "B");
			var e = CreateNewWithPart(b.Component, "E");
			var f = CreateNewWithPart(b.Component, "F");
			var g = CreateNewWithPart(e.Component, "G");
			var h = CreateNewWithPart(e.Component, "H");

			b.Validation.ValidateOE_OP_Component();
			AssertEquals(false, b.OE_OP_ComponentInfo.HasErrors());

			var circularPart = f.Component.BillOfMaterials.AddNew();
			circularPart.OE_OP_Component = e.Component.PK;

			b.Validation.ValidateOE_OP_Component();
			AssertEquals(false, b.OE_OP_ComponentInfo.HasErrors());
		}

		OrgPartBOM CreateNewWithPart(OrgSupplierPart part, string componentPartNum/*to help with debugging*/)
		{
			var result = part.BillOfMaterials.AddNew();
			result.OE_OP_Component = Factory.New<OrgSupplierPart>().PK;
			result.Component.OP_PartNum = componentPartNum;
			return result;
		}

		#endregion

		#region Component Validation

		public void TestIsMainProductHasAnyParentMainProductWithIsComponentPickedOnSalesOrder()
		{
			var mainProduct = CreateProduct("MP1", Constants.PkgUnit.Pallet);
			var bomComponentProduct1 = CreateProduct("BOM1", Constants.PkgUnit.Pallet);
			var bomComponentProduct2 = CreateProduct("BOM2", Constants.PkgUnit.Pallet);

			CreateProductBOM(mainProduct, bomComponentProduct1, 1m, Constants.PkgUnit.Box);
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			Factory.Save();

			var bomPartForBomProduct = CreateProductBOM(bomComponentProduct1, bomComponentProduct2, 1m, Constants.PkgUnit.Sheet);
			AssertHasError(bomPartForBomProduct.OE_OP_ComponentInfo, OrgPartBOMValidation.ParentHasIsPickOnOrderParent);
		}

		public void TestComponentHasBillOfMaterials()
		{
			var mainProduct = CreateProduct("MP1", Constants.PkgUnit.Pallet);
			var bomComponentProduct1 = CreateProduct("BOM1", Constants.PkgUnit.Pallet);
			var bomComponentProduct2 = CreateProduct("BOM2", Constants.PkgUnit.Pallet);
			CreateProductBOM(bomComponentProduct1, bomComponentProduct2, 1m, Constants.PkgUnit.Sheet);
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			Factory.Save();

			var bomPartForMainProduct = CreateProductBOM(mainProduct, bomComponentProduct1, 1m, Constants.PkgUnit.Box);
			AssertHasError(bomPartForMainProduct.OE_OP_ComponentInfo, OrgPartBOMValidation.CannotAddThisProductAsComponent);
		}

		public void TestNoOwnersExist()
		{
			var b = CreateNewWithPart(ParentPart, "B");
			b.Validation.ValidateOE_OP_Component();
			AssertEquals(false, b.OE_OP_ComponentInfo.HasErrors());
		}

		public void TestNoOwnersExist1()
		{
			var org = Factory.New<OrgHeader>();
			AddOrgPartRelation(ParentPart, OrgPartRelation.RelationshipTypes.Supplier, org);

			var b = CreateNewWithPart(ParentPart, "B");
			AddOrgPartRelation(b.Component, OrgPartRelation.RelationshipTypes.WarehouseConsignee, org);
			b.Validation.ValidateOE_OP_Component();
			AssertEquals(false, b.OE_OP_ComponentInfo.HasErrors());
		}

		public void TestCommonOwnersExist()
		{
			var org = Factory.New<OrgHeader>();
			AddOrgPartRelation(ParentPart, OrgPartRelation.RelationshipTypes.Owner, org);

			var b = CreateNewWithPart(ParentPart, "B");
			AddOrgPartRelation(b.Component, OrgPartRelation.RelationshipTypes.Owner, org);
			b.Validation.ValidateOE_OP_Component();
			AssertEquals(false, b.OE_OP_ComponentInfo.HasErrors());
		}

		public void TestCommonOwnersExist1()
		{
			var org = Factory.New<OrgHeader>();
			AddOrgPartRelation(ParentPart, OrgPartRelation.RelationshipTypes.Both, org);

			var b = CreateNewWithPart(ParentPart, "B");
			AddOrgPartRelation(b.Component, OrgPartRelation.RelationshipTypes.Both, org);
			b.Validation.ValidateOE_OP_Component();
			AssertEquals(false, b.OE_OP_ComponentInfo.HasErrors());
		}

		public void TestCommonOwnersExist2()
		{
			var org = Factory.New<OrgHeader>();
			AddOrgPartRelation(ParentPart, OrgPartRelation.RelationshipTypes.Supplier, Factory.New<OrgHeader>());
			AddOrgPartRelation(ParentPart, OrgPartRelation.RelationshipTypes.Owner, Factory.New<OrgHeader>());
			AddOrgPartRelation(ParentPart, OrgPartRelation.RelationshipTypes.Owner, org);

			var b = CreateNewWithPart(ParentPart, "B");
			AddOrgPartRelation(b.Component, OrgPartRelation.RelationshipTypes.Owner, Factory.New<OrgHeader>());
			AddOrgPartRelation(b.Component, OrgPartRelation.RelationshipTypes.Supplier, Factory.New<OrgHeader>());
			AddOrgPartRelation(b.Component, OrgPartRelation.RelationshipTypes.Owner, org);
			b.Validation.ValidateOE_OP_Component();
			AssertEquals(false, b.OE_OP_ComponentInfo.HasErrors());
		}

		public void TestNoCommonOwnersExist1()
		{
			var b = CreateNewWithPart(ParentPart, "B");
			AddOrgPartRelation(b.Component, OrgPartRelation.RelationshipTypes.Owner, Factory.New<OrgHeader>());
			b.Validation.ValidateOE_OP_Component();
			AssertEquals(true, b.OE_OP_ComponentInfo.HasErrors());

			AddOrgPartRelation(parentPart, OrgPartRelation.RelationshipTypes.Owner, Factory.New<OrgHeader>());
			b.Validation.ValidateOE_OP_Component();
			AssertEquals(true, b.OE_OP_ComponentInfo.HasErrors());
		}

		void AddOrgPartRelation(OrgSupplierPart part, string type, OrgHeader relatedOrg)
		{
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = type;
			relation.OU_OH = relatedOrg.PK;
		}

		#endregion

		#region Check same existance

		public void TestIsSameBOMExists()
		{
			//							A
			//						/	|	\
			//					B		C		D

			var b = CreateNewWithPart(ParentPart, "B");
			var c = CreateNewWithPart(ParentPart, "C");
			var d = CreateNewWithPart(ParentPart, "D");
			d.OE_F3_NKPackType = "UNT";
			b.OE_F3_NKPackType = "UNT";
			d.OE_ComponentQty = 1;
			b.OE_ComponentQty = 1;

			AssertEquals(false, b.OE_OP_ComponentInfo.HasErrors());
			AssertEquals(false, c.OE_OP_ComponentInfo.HasErrors());
			AssertEquals(false, d.OE_OP_ComponentInfo.HasErrors());

			//							A
			//						/	|	\
			//					B		C		B

			d.OE_OP_Component = b.OE_OP_Component;
			AssertEquals(true, d.OE_OP_ComponentInfo.HasErrors());
		}

		#endregion

		#region Changing BOM components picked on Sales Order

		#region CantAddBomForComponentPickedOnSalesOrder

		public void TestCantAddBomForComponentPickedOnSalesOrder()
		{
			var kit = CreateProduct("Table", Constants.PkgUnit.Unit);
			var component = CreateProduct("Leg", Constants.PkgUnit.Unit);

			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				mockPickBomDetector.Setup(s => s.IsKitBuiltOnSalesOrder(Factory, kit.PK)).Returns(false);
				var bomPart1 = CreateProductBOM(kit, component, 4m, Constants.PkgUnit.Unit);
				AssertNoErrors(bomPart1);
				component.BillOfMaterialsView.RemoveAll();

				mockPickBomDetector.Setup(s => s.IsKitBuiltOnSalesOrder(Factory, kit.PK)).Returns(true); // changing stub value
				var bomPart2 = CreateProductBOM(kit, component, 4m, Constants.PkgUnit.Unit);
				AssertHasErrors(OrgPartBOMValidation.PickOnSalesOrderDetected, bomPart2.OE_OP_ComponentInfo);
				AssertHasErrors(OrgPartBOMValidation.PickOnSalesOrderDetected, bomPart2.OE_ComponentQtyInfo);
				AssertHasErrors(OrgPartBOMValidation.PickOnSalesOrderDetected, bomPart2.OE_F3_NKPackTypeInfo);
			}
		}

		#endregion

		#region TestCantChangeBomForComponentPickedOnSalesOrder

		public void TestCantChangeBomForComponentPickedOnSalesOrder()
		{
			var kit = CreateProduct("Table", Constants.PkgUnit.Unit);
			var component1 = CreateProduct("Leg", Constants.PkgUnit.Unit);

			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				mockPickBomDetector.Setup(s => s.IsKitBuiltOnSalesOrder(Factory, kit.PK)).Returns(false);
				var bomPart = CreateProductBOM(kit, component1, 4m, Constants.PkgUnit.Unit);
				AssertNoErrors(bomPart);
				Factory.Save();

				mockPickBomDetector.Setup(s => s.IsKitBuiltOnSalesOrder(Factory, kit.PK)).Returns(true); // changing stub value
				var component2 = CreateProduct("TableTop", Constants.PkgUnit.Package);
				bomPart.OE_ComponentQty = 1;
				bomPart.OE_F3_NKPackType = Constants.PkgUnit.Package;
				bomPart.OE_OP_Component = component2.PK;

				AssertHasErrors(OrgPartBOMValidation.PickOnSalesOrderDetected, bomPart.OE_OP_ComponentInfo);
				AssertHasErrors(OrgPartBOMValidation.PickOnSalesOrderDetected, bomPart.OE_ComponentQtyInfo);
				AssertHasErrors(OrgPartBOMValidation.PickOnSalesOrderDetected, bomPart.OE_F3_NKPackTypeInfo);

				// return values back
				bomPart.OE_ComponentQty = 4m;
				bomPart.OE_F3_NKPackType = Constants.PkgUnit.Unit;
				bomPart.OE_OP_Component = component1.PK;

				AssertNoErrors(bomPart);
			}
		}

		#endregion

		#region TestCantDeleteBomComponentPickedOnSalesOrder

		public void TestCantDeleteBomComponentPickedOnSalesOrder()
		{
			var kit = CreateProduct("Table", Constants.PkgUnit.Unit);
			var component = CreateProduct("Leg", Constants.PkgUnit.Unit);

			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				mockPickBomDetector.Setup(s => s.IsKitBuiltOnSalesOrder(Factory, kit.PK)).Returns(false);
				var bomPart = CreateProductBOM(kit, component, 4m, Constants.PkgUnit.Unit);
				AssertNoErrors(bomPart);
				Factory.Save();

				Assert(bomPart.CanDelete);

				mockPickBomDetector.Setup(s => s.IsKitBuiltOnSalesOrder(Factory, kit.PK)).Returns(true); // changing stub value
				Assert("Should not be able to delete bom component if it was picked on sales order.", !bomPart.CanDelete);
				AssertEquals(OrgPartBOMValidation.PickOnSalesOrderDetected, bomPart.ReasonForNotAbleToDelete);
			}
		}

		#endregion

		#endregion

		#region TestNoUnitConversionsExist

		public void TestNoUnitConversionsExist()
		{
			var bomProduct = CreateProduct("B1", Constants.PkgUnit.Pallet);
			var componentProduct1 = CreateProduct("C1", Constants.PkgUnit.Unit);
			var componentProduct2 = CreateProduct("C2", Constants.PkgUnit.Unit);
			var bomPart1 = CreateProductBOM(bomProduct, componentProduct1, 1m, Constants.PkgUnit.Unit);
			var bomPart2 = CreateProductBOM(bomProduct, componentProduct2, 1m, Constants.PkgUnit.Unit);
			AssertNoErrors(bomPart1.OE_F3_NKPackTypeInfo);
			AssertNoErrors(bomPart2.OE_F3_NKPackTypeInfo);

			bomPart1.OE_F3_NKPackType = Constants.PkgUnit.Pallet;
			AssertHasError(bomPart1.OE_F3_NKPackTypeInfo, "Please specify a unit conversion for this pack type.");
			AssertNoErrors(bomPart2.OE_F3_NKPackTypeInfo);

			bomPart2.OE_F3_NKPackType = Constants.PkgUnit.Pallet;
			AssertHasError(bomPart1.OE_F3_NKPackTypeInfo, "Please specify a unit conversion for this pack type.");
			AssertHasError(bomPart2.OE_F3_NKPackTypeInfo, "Please specify a unit conversion for this pack type.");

			// setup unit conversion for the child component.
			var partUnits = componentProduct2.PartUnits;
			var partUnit = partUnits.AddNew();
			partUnit.OF_PackType = Constants.PkgUnit.Unit;
			partUnit.OF_QuantityInParent = 2;
			partUnit.OF_ParentPackType = Constants.PkgUnit.Pallet;
			bomPart2.OE_F3_NKPackType = Constants.PkgUnit.Unit; // to revalidate
			bomPart2.OE_F3_NKPackType = Constants.PkgUnit.Pallet;
			AssertHasError(bomPart1.OE_F3_NKPackTypeInfo, "Please specify a unit conversion for this pack type.");
			AssertNoErrors(bomPart2.OE_F3_NKPackTypeInfo);

			bomPart1.OE_OP_Component = ZGuid.Empty; // Invalid product
			bomPart1.OE_F3_NKPackType = Constants.PkgUnit.Sheet; // to revalidate
			AssertNoErrors(bomPart1.OE_F3_NKPackTypeInfo);
		}

		OrgSupplierPart CreateProduct(string partCode, string stockKeepingUnit)
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = partCode;
			part.OP_Desc = partCode;
			part.OP_StockKeepingUnit = stockKeepingUnit;
			return part;
		}

		OrgPartBOM CreateProductBOM(OrgSupplierPart part, OrgSupplierPart subPart, ZDecimal componentQty, ZString componentPack)
		{
			var bom = Factory.New<OrgPartBOM>();
			bom.OE_OP_MainProduct = part.PK;
			bom.OE_OP_Component = subPart.PK;
			bom.OE_ComponentQty = componentQty;
			bom.OE_F3_NKPackType = componentPack;
			return bom;
		}

		#endregion

		public void TestCheckOE_ExcludeForVirtualWarehouse()
		{
			var mainProduct = CreateProduct("MP1", Constants.PkgUnit.Pallet);
			var bomCompProduct1 = CreateProduct("BOM1", Constants.PkgUnit.Pallet);
			var bomCompProduct2 = CreateProduct("BOM2", Constants.PkgUnit.Pallet);
			var bomCompProduct3 = CreateProduct("BOM3", Constants.PkgUnit.Pallet);

			var bomPart1 = CreateProductBOM(mainProduct, bomCompProduct1, 1m, Constants.PkgUnit.Box);
			var bomPart2 = CreateProductBOM(mainProduct, bomCompProduct2, 1m, Constants.PkgUnit.Box);
			var bomPart3 = CreateProductBOM(mainProduct, bomCompProduct3, 1m, Constants.PkgUnit.Box);
			AssertNoErrors(bomPart1.OE_ExcludeForVirtualWarehouseInfo);
			AssertNoErrors(bomPart2.OE_ExcludeForVirtualWarehouseInfo);
			AssertNoErrors(bomPart3.OE_ExcludeForVirtualWarehouseInfo);

			bomPart1.OE_ExcludeForVirtualWarehouse = true;
			AssertNoErrors(bomPart1.OE_ExcludeForVirtualWarehouseInfo);
			AssertNoErrors(bomPart2.OE_ExcludeForVirtualWarehouseInfo);
			AssertNoErrors(bomPart3.OE_ExcludeForVirtualWarehouseInfo);

			bomPart2.OE_ExcludeForVirtualWarehouse = true;
			AssertNoErrors(bomPart1.OE_ExcludeForVirtualWarehouseInfo);
			AssertNoErrors(bomPart2.OE_ExcludeForVirtualWarehouseInfo);
			AssertNoErrors(bomPart3.OE_ExcludeForVirtualWarehouseInfo);

			bomPart3.OE_ExcludeForVirtualWarehouse = true;
			AssertHasError(bomPart1.OE_ExcludeForVirtualWarehouseInfo, "Cannot have all Components excluded in the BOM composition for Virtual Warehouse.");
			AssertHasError(bomPart2.OE_ExcludeForVirtualWarehouseInfo, "Cannot have all Components excluded in the BOM composition for Virtual Warehouse.");
			AssertHasError(bomPart3.OE_ExcludeForVirtualWarehouseInfo, "Cannot have all Components excluded in the BOM composition for Virtual Warehouse.");

			bomPart2.OE_ExcludeForVirtualWarehouse = false;
			AssertNoErrors(bomPart1.OE_ExcludeForVirtualWarehouseInfo);
			AssertNoErrors(bomPart2.OE_ExcludeForVirtualWarehouseInfo);
			AssertNoErrors(bomPart3.OE_ExcludeForVirtualWarehouseInfo);

			bomPart2.OE_ExcludeForVirtualWarehouse = true;
			AssertHasError(bomPart1.OE_ExcludeForVirtualWarehouseInfo, "Cannot have all Components excluded in the BOM composition for Virtual Warehouse.");
			AssertHasError(bomPart2.OE_ExcludeForVirtualWarehouseInfo, "Cannot have all Components excluded in the BOM composition for Virtual Warehouse.");
			AssertHasError(bomPart3.OE_ExcludeForVirtualWarehouseInfo, "Cannot have all Components excluded in the BOM composition for Virtual Warehouse.");
		}

		#region Implementation

		OrgSupplierPart ParentPart
		{
			get
			{
				if (parentPart == null)
				{
					parentPart = Factory.New<OrgSupplierPart>();
					parentPart.OP_PartNum = "A";
				}
				return parentPart;
			}
		}
		OrgSupplierPart parentPart;

		#endregion
	}
}
