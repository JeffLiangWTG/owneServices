using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsProductStyleSize))]
	class WhsProductStyleSizeTestCase : WhsEnvBusinessObjectTestCase
	{
		#region Related

		public void TestProductStyle()
		{
			var productStyle = Factory.New<WhsProductStyle>();
			var productStyleSize = Factory.New<WhsProductStyleSize>();
			productStyleSize.WSZ_WST_ProductStyle = productStyle.PK;
			AssertNotNull(productStyleSize.ProductStyle);
			AssertEquals(productStyle, productStyleSize.ProductStyle);
		}

		#endregion

		#region TestDeleteSequencesSizes

		public void TestDeleteSequencesSizes()
		{
			var style = Helper.CreateProductStyle();

			var s1 = style.Sizes.AddNew();
			var s2 = style.Sizes.AddNew();
			var s3 = style.Sizes.AddNew();
			AssertEquals(1, (ZInt)s1.WSZ_Sequence);
			AssertEquals(2, (ZInt)s2.WSZ_Sequence);
			AssertEquals(3, (ZInt)s3.WSZ_Sequence);

			s2.Delete();
			AssertEquals(1, (ZInt)s1.WSZ_Sequence);
			AssertEquals(true, s2.IsDeleted);
			AssertEquals(2, (ZInt)s3.WSZ_Sequence);
		}

		#endregion

		#region TestDeletingStyleDoesNotSequencesSizes

		public void TestDeletingStyleDoesNotSequencesSizes()
		{
			var style = Helper.CreateProductStyle();
			var s1 = style.Sizes.AddNew();
			var s2 = style.Sizes.AddNew();
			var s3 = style.Sizes.AddNew();
			AssertEquals(1, (ZInt)s1.WSZ_Sequence);
			AssertEquals(2, (ZInt)s2.WSZ_Sequence);
			AssertEquals(3, (ZInt)s3.WSZ_Sequence);

			s1.WSZ_SequenceInfo.ValueChanged += delegate
			{ Fail("Should not have changed any sequence"); };
			s2.WSZ_SequenceInfo.ValueChanged += delegate
			{ Fail("Should not have changed any sequence"); };
			s3.WSZ_SequenceInfo.ValueChanged += delegate
			{ Fail("Should not have changed any sequence"); };

			style.Delete();
			AssertEquals(true, s1.IsDeleted);
			AssertEquals(true, s2.IsDeleted);
			AssertEquals(true, s3.IsDeleted);
		}

		#endregion

		#region TestCanDelete

		public void TestCanDelete()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = part.PK.ToString().Replace("-", "");
			part.OP_WSC_WhsProductStyleColour = ProductStyleColour.PK;
			part.OP_WSZ_WhsProductStyleSize = ProductStyleSize1.PK;
			AssertEquals(true, ProductStyleSize1.CanDelete);

			Factory.Save();
			AssertEquals(false, ProductStyleSize1.CanDelete);
		}

		#endregion

		#region TestReasonForNotAbleToDelete

		public void TestReasonForNotAbleToDelete()
		{
			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "123";
			part1.OP_Desc = "OneTwoThree";
			part1.OP_WSC_WhsProductStyleColour = ProductStyleColour.PK;
			part1.OP_WSZ_WhsProductStyleSize = ProductStyleSize1.PK;
			Factory.Save();
			AssertEquals(@"This Size cannot be deleted as it is being used on the following Products:
123 - OneTwoThree", ProductStyleSize1.ReasonForNotAbleToDelete);

			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "456";
			part2.OP_Desc = "FourFiveSix";
			part2.OP_WSC_WhsProductStyleColour = ProductStyleColour.PK;
			part2.OP_WSZ_WhsProductStyleSize = ProductStyleSize2.PK;
			Factory.Save();
			AssertEquals(@"This Size cannot be deleted as it is being used on the following Products:
456 - FourFiveSix", ProductStyleSize2.ReasonForNotAbleToDelete);

			var productStyleSize = Factory.New<WhsProductStyleSize>();
			ProductStyleSize2.WSZ_Sequence = 1;
			AssertEquals("", productStyleSize.ReasonForNotAbleToDelete);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var owner = Helper.CreateClient("O1");
			var style = Helper.CreateProductStyle("S1", "Style1", owner.PK);
			var colour = style.Colours.AddNew();
			colour.WSC_Code = "ABC";
			colour.WSC_Description = "D";

			var size = style.Sizes.AddNew();
			size.WSZ_Size = "1";
			size.WSZ_Sequence = 1;
			return size;
		}

		#endregion

		#region Implementation

		protected WhsProductStyleColour ProductStyleColour
		{
			get
			{
				if (productStyleColour == null)
				{
					var owner = Helper.CreateClient("O1");
					var productStyle = Helper.CreateProductStyle("S1", "Style1", owner.PK);
					productStyleColour = productStyle.Colours.AddNew();
					productStyleColour.WSC_Code = "ABC";
					productStyleColour.WSC_Description = "D";

					ProductStyleSize1 = productStyle.Sizes.AddNew();
					ProductStyleSize1.WSZ_Size = "1";
					ProductStyleSize1.WSZ_Sequence = 1;

					ProductStyleSize2 = productStyle.Sizes.AddNew();
					ProductStyleSize2.WSZ_Size = "2";
					ProductStyleSize2.WSZ_Sequence = 2;
				}

				return productStyleColour;
			}
		}

		WhsProductStyleSize ProductStyleSize1 { get; set; }

		WhsProductStyleSize ProductStyleSize2 { get; set; }

		WhsProductStyleColour productStyleColour;

		#endregion
	}
}
