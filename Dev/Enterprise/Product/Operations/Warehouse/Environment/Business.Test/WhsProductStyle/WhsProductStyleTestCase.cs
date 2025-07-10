using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsProductStyle))]
	class WhsProductStyleTestCase : WhsEnvBusinessObjectTestCase
	{
		#region Colours

		public void TestColours()
		{
			AssertEquals(0, ProductStyle.Colours.Count);
			AssertEquals(true, ProductStyle.IsRegisteredEditableChildObject(ProductStyle.Colours));

			ProductStyle.Colours.AddNew();
			AssertEquals(1, ProductStyle.Colours.Count);
		}

		#endregion

		#region Classifications

		public void TestClassifications()
		{
			AssertEquals(0, ProductStyle.Classifications.Count);
			AssertEquals(true, ProductStyle.IsRegisteredEditableChildObject(ProductStyle.Classifications));

			ProductStyle.Classifications.AddNew();
			AssertEquals(1, ProductStyle.Classifications.Count);
		}

		#endregion

		#region Sizes

		public void TestSizes()
		{
			AssertEquals(0, ProductStyle.Sizes.Count);
			AssertEquals(true, ProductStyle.IsRegisteredEditableChildObject(ProductStyle.Sizes));

			ProductStyle.Sizes.AddNew();
			AssertEquals(1, ProductStyle.Sizes.Count);
		}

		#endregion

		#region TestCanDelete

		public void TestCanDelete()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = part.PK.ToString().Replace("-", "");
			part.OP_WSC_WhsProductStyleColour = ProductStyleColour.PK;
			part.OP_WSZ_WhsProductStyleSize = ProductStyleSize1.PK;
			AssertEquals(true, ProductStyle.CanDelete);

			Factory.Save();
			AssertEquals(false, ProductStyle.CanDelete);
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
			AssertEquals(@"This Product Style cannot be deleted as it is being used on the following Products:
123 - OneTwoThree", ProductStyle.ReasonForNotAbleToDelete);

			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "456";
			part2.OP_Desc = "FourFiveSix";
			part2.OP_WSC_WhsProductStyleColour = ProductStyleColour.PK;
			part2.OP_WSZ_WhsProductStyleSize = ProductStyleSize2.PK;
			Factory.Save();
			AssertEquals(@"This Product Style cannot be deleted as it is being used on the following Products:
123 - OneTwoThree
456 - FourFiveSix", ProductStyle.ReasonForNotAbleToDelete);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			var productStyle = Factory.New<WhsProductStyle>();
			AssertEquals("Product Style", productStyle.HumanReadableName);

			productStyle.WST_Code = "S1";
			AssertEquals("Product Style S1", productStyle.HumanReadableName);
		}

		#endregion

		#region Implementation

		protected WhsProductStyle ProductStyle
		{
			get { return productStyle ?? (productStyle = Helper.CreateProductStyle("S1", "Style1", Helper.CreateClient("O1").PK)); }
		}

		WhsProductStyle productStyle;

		protected WhsProductStyleColour ProductStyleColour
		{
			get
			{
				if (productStyleColour == null)
				{
					productStyleColour = ProductStyle.Colours.AddNew();
					productStyleColour.WSC_Code = "ABC";
					productStyleColour.WSC_Description = "D";

					productStyleSize1 = ProductStyle.Sizes.AddNew();
					productStyleSize1.WSZ_Size = "1";
					productStyleSize1.WSZ_Sequence = 1;

					productStyleSize2 = ProductStyle.Sizes.AddNew();
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
