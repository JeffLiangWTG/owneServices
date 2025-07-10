using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsProductStyleClassification))]
	class WhsProductStyleClassificationTestCase : WhsEnvBusinessObjectTestCase
	{
		#region Related

		public void TestProductStyle()
		{
			var productStyle = Factory.New<WhsProductStyle>();
			var classification = Factory.NewWithValidTestData<WhsProductStyleClassification>();
			classification.WSS_WST_ProductStyle = productStyle.PK;
			AssertNotNull(classification.ProductStyle);
			AssertEquals(productStyle, classification.ProductStyle);
		}

		#endregion

		#region TestCanDelete

		public void TestCanDelete()
		{
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			owner.OH_Code = "Org1";
			var productStyle = Helper.CreateProductStyle("S1", "Style1", owner.PK);
			var colour = Factory.NewWithValidTestData<WhsProductStyleColour>();
			var classification = Factory.NewWithValidTestData<WhsProductStyleClassification>();
			var productStyleSize1 = productStyle.Sizes.AddNew();
			productStyleSize1.WSZ_Size = "1";
			productStyleSize1.WSZ_Sequence = 1;

			colour.WSC_WST_ProductStyle = productStyle.PK;
			classification.WSS_WST_ProductStyle = productStyle.PK;

			AssertEquals(true, classification.CanDelete);

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "123";
			part.OP_Desc = "OneTwoThree";
			part.OP_WSC_WhsProductStyleColour = colour.PK;
			part.OP_WSS_WhsProductStyleClassification = classification.PK;
			part.OP_WSZ_WhsProductStyleSize = productStyleSize1.PK;
			AssertEquals(true, classification.CanDelete);

			Factory.Save();
			AssertEquals(false, classification.CanDelete);
		}

		#endregion

		#region TestReasonForNotAbleToDelete

		public void TestReasonForNotAbleToDelete()
		{
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			owner.OH_Code = "Org1";
			var productStyle = Helper.CreateProductStyle("S1", "Style1", owner.PK);
			var colour = Factory.NewWithValidTestData<WhsProductStyleColour>();
			var classification = Factory.NewWithValidTestData<WhsProductStyleClassification>();
			colour.WSC_WST_ProductStyle = productStyle.PK;
			classification.WSS_WST_ProductStyle = productStyle.PK;
			var productStyleSize1 = productStyle.Sizes.AddNew();
			productStyleSize1.WSZ_Size = "1";
			productStyleSize1.WSZ_Sequence = 1;

			var productStyleSize2 = productStyle.Sizes.AddNew();
			productStyleSize2.WSZ_Size = "2";
			productStyleSize2.WSZ_Sequence = 2;

			AssertEquals(true, classification.CanDelete);

			AssertEquals("", classification.ReasonForNotAbleToDelete);

			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "123";
			part1.OP_Desc = "OneTwoThree";
			part1.OP_WSC_WhsProductStyleColour = colour.PK;
			part1.OP_WSS_WhsProductStyleClassification = classification.PK;
			part1.OP_WSZ_WhsProductStyleSize = productStyleSize1.PK;
			Factory.Save();
			AssertEquals(@"This Classification cannot be deleted as it is being used on the following Products:
123 - OneTwoThree", classification.ReasonForNotAbleToDelete);

			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "456";
			part2.OP_Desc = "FourFiveSix";
			part2.OP_WSC_WhsProductStyleColour = colour.PK;
			part2.OP_WSS_WhsProductStyleClassification = classification.PK;
			part2.OP_WSZ_WhsProductStyleSize = productStyleSize2.PK;
			Factory.Save();
			AssertEquals(@"This Classification cannot be deleted as it is being used on the following Products:
123 - OneTwoThree
456 - FourFiveSix", classification.ReasonForNotAbleToDelete);
		}

		#endregion
	}
}
