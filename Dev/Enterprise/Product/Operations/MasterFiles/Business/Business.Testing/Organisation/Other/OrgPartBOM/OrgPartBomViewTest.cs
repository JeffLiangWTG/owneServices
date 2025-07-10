using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgPartBomView))]
	sealed class OrgPartBomViewTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return BOMView;
		}

		public void TestIOrgPartBOMView()
		{
			AssertEquals("TST", BOMView.ComponentCode);
			AssertEquals("TEST COMPONENT", BOMView.ComponentDescription);
			AssertEquals(1, BOMView.BOMLevel);
			AssertEquals("BAG", BOMView.ComponentPack);
			AssertEquals(new ZDecimal(2), BOMView.ComponentQty);
		}

		#region Implementation

		OrgPartBomView BOMView
		{
			get { return bomView ?? (bomView = new OrgPartBomView(BOM, 1)); }
		}
		OrgPartBomView bomView;

		OrgPartBOM BOM
		{
			get
			{
				if (fbom == null)
				{
					fbom = Factory.New<OrgPartBOM>();
					OrgSupplierPart part = Factory.New<OrgSupplierPart>();
					OrgSupplierPart subPart = Factory.New<OrgSupplierPart>();
					subPart.OP_Desc = "TEST COMPONENT";
					subPart.OP_PartNum = "TST";
					fbom.OE_OP_MainProduct = part.PK;
					fbom.OE_OP_Component = subPart.PK;
					fbom.OE_F3_NKPackType = "BAG";
					fbom.OE_ComponentQty = new ZDecimal(2);
					OrgSupplierPart byProductPart = Factory.New<OrgSupplierPart>();
					byProductPart.OP_PartNum = "By Product Part Num";
					byProductPart.OP_Desc = "By Product Description";
				}
				return fbom;
			}
		}
		OrgPartBOM fbom;

		#endregion
	}
}
