using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.Warehouse.Integration.Warehouse;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgPartBOM))]
	class OrgPartBOMTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			AssertEquals(1m, BOM.OE_ComponentQty);
		}

		public void TestOE_OP_Component()
		{
			var mainProduct = Factory.New<OrgSupplierPart>();
			BOM.OE_OP_MainProduct = mainProduct.PK;
			AssertEquals("", BOM.OE_F3_NKPackType);

			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.OP_StockKeepingUnit = "BAG";

			BOM.OE_OP_Component = part.PK;
			AssertEquals("BAG", BOM.OE_F3_NKPackType);
		}

		public void TestBOMDescription()
		{
			var mainProduct = Factory.New<OrgSupplierPart>();
			var bom = mainProduct.BillOfMaterials.AddNew();
			AssertEquals(" - ", bom.BOMDescription);

			bom.OE_F3_NKPackType = "UNT";
			AssertEquals(" - UNT", bom.BOMDescription);

			var component = Factory.New<OrgSupplierPart>();
			component.OP_PartNum = "PRODUCT1";
			bom.OE_OP_Component = component.PK;
			AssertEquals("PRODUCT1 - UNT", bom.BOMDescription);
		}

		public void TestCodeAndDescriptionPropertyAttributes()
		{
			AssertEquals(nameof(OrgPartBOM.BOMDescription), typeof(OrgPartBOM).GetCustomAttribute<CodePropertyAttribute>().PropertyName);
			AssertEquals(nameof(OrgPartBOM.BOMDescription), typeof(OrgPartBOM).GetCustomAttribute<DescriptionPropertyAttribute>().PropertyName);
		}

		public void TestComponentDescription()
		{
			var mainProduct = Factory.New<OrgSupplierPart>();
			BOM.OE_OP_MainProduct = mainProduct.PK;
			AssertEquals("", BOM.ComponentDescription);

			BOM.OE_OP_Component = Factory.New<OrgSupplierPart>().PK;
			AssertEquals("", BOM.ComponentDescription);

			BOM.Component.OP_Desc = "TEST PART";
			AssertEquals("TEST PART", BOM.ComponentDescription);
		}

		public void TestChildrenBomIndicator()
		{
			var mainProduct = Factory.New<OrgSupplierPart>();
			BOM.OE_OP_MainProduct = mainProduct.PK;
			AssertEquals("", BOM.ChildrenBomIndicator);
			// Add child to BOM
			BOM.OE_OP_Component = Factory.New<OrgSupplierPart>().PK;
			OrgPartBOM childBom = Factory.New<OrgPartBOM>();
			childBom.OE_OP_MainProduct = BOM.OE_OP_Component;
			childBom.OE_OP_Component = Factory.New<OrgSupplierPart>().PK;
			AssertEquals("+", BOM.ChildrenBomIndicator);
		}

		#region TestCanDelete

		public void TestCanDelete()
		{
			var mainProduct = Factory.NewWithValidTestData<OrgSupplierPart>();
			var component = Factory.NewWithValidTestData<OrgSupplierPart>();

			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				mockPickBomDetector.Setup(s => s.IsKitBuiltOnSalesOrder(Factory, mainProduct.PK)).Returns(true);
				var bom = Factory.New<OrgPartBOM>();
				bom.OE_OP_MainProduct = mainProduct.PK;
				bom.OE_OP_Component = component.PK;
				bom.OE_F3_NKPackType = "UNT";
				bom.OE_ComponentQty = 2;

				Assert("Should be able to delete because row is not saved yet.", bom.CanDelete);

				Factory.Save();
				Assert(bom.IsInDatabase);

				AssertEquals("Should not be able to delete because there are kits built for main product.", false, bom.CanDelete);
				AssertEquals(OrgPartBOMValidation.PickOnSalesOrderDetected, bom.ReasonForNotAbleToDelete);
			}
		}

		#endregion

		#region Implementation

		public OrgPartBOM BOM
		{
			get { return bom ?? (bom = Factory.New<OrgPartBOM>()); }
		}
		OrgPartBOM bom;

		#endregion
	}
}
