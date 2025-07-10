using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsProductStyleColour))]
	class WhsProductStyleColourTestCase : WhsEnvBusinessObjectTestCase
	{
		#region Related

		public void TestProductStyle()
		{
			var productStyle = Factory.New<WhsProductStyle>();
			ProductStyleColour.WSC_WST_ProductStyle = productStyle.PK;
			AssertNotNull(ProductStyleColour.ProductStyle);
			AssertEquals(productStyle, ProductStyleColour.ProductStyle);
		}

		#endregion

		#region TestCanDelete

		public void TestCanDelete()
		{
			AssertEquals(true, ProductStyleColour.CanDelete);

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = part.PK.ToString().Replace("-", "");
			part.OP_WSC_WhsProductStyleColour = ProductStyleColour.PK;
			part.OP_WSZ_WhsProductStyleSize = ProductStyleSize1.PK;
			AssertEquals(true, ProductStyleColour.CanDelete);

			Factory.Save();
			AssertEquals(false, ProductStyleColour.CanDelete);
		}

		#endregion

		#region TestReasonForNotAbleToDelete

		public void TestReasonForNotAbleToDelete()
		{
			AssertEquals("", ProductStyleColour.ReasonForNotAbleToDelete);

			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "123";
			part1.OP_Desc = "OneTwoThree";
			part1.OP_WSC_WhsProductStyleColour = ProductStyleColour.PK;
			part1.OP_WSZ_WhsProductStyleSize = ProductStyleSize1.PK;
			Factory.Save();
			AssertEquals(@"This Color cannot be deleted as it is being used on the following Products:
123 - OneTwoThree", ProductStyleColour.ReasonForNotAbleToDelete);

			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "456";
			part2.OP_Desc = "FourFiveSix";
			part2.OP_WSC_WhsProductStyleColour = ProductStyleColour.PK;
			part2.OP_WSZ_WhsProductStyleSize = ProductStyleSize2.PK;
			Factory.Save();
			AssertEquals(@"This Color cannot be deleted as it is being used on the following Products:
123 - OneTwoThree
456 - FourFiveSix", ProductStyleColour.ReasonForNotAbleToDelete);
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

					productStyleSize1 = productStyle.Sizes.AddNew();
					productStyleSize1.WSZ_Size = "1";
					productStyleSize1.WSZ_Sequence = 1;

					productStyleSize2 = productStyle.Sizes.AddNew();
					productStyleSize2.WSZ_Size = "2";
					productStyleSize2.WSZ_Sequence = 2;
				}

				return productStyleColour;
			}
		}

		WhsProductStyleSize ProductStyleSize1
		{
			get { return productStyleSize1; }
		}

		WhsProductStyleSize ProductStyleSize2
		{
			get { return productStyleSize2; }
		}

		WhsProductStyleColour productStyleColour;
		WhsProductStyleSize productStyleSize1;
		WhsProductStyleSize productStyleSize2;

		#endregion
	}
}
