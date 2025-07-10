using System.Linq;
using CargoWise.ComponentModel;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	class WhsProductStyleValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestWST_Code

		public void TestWST_Code()
		{
			var abcStyle = Helper.CreateProductStyle("ABC", "Aye Bee See");
			AssertNoErrors(abcStyle.WST_CodeInfo);

			var abcStyle2 = Helper.CreateProductStyle();
			abcStyle2.Validation.ValidateWST_Code();
			AssertHasError(abcStyle2.WST_CodeInfo, "Please enter a Product Style Code.");

			abcStyle2.WST_Code = "ABC";
			AssertHasError(abcStyle2.WST_CodeInfo, "Product Style 'Aye Bee See' is already using the Code 'ABC'.");

			abcStyle2.WST_Code = "DEF";
			AssertNoErrors(abcStyle2.WST_CodeInfo);
		}

		#endregion

		#region TestWST_Description

		public void TestWST_Description()
		{
			var abcStyle = Helper.CreateProductStyle();
			abcStyle.Validation.ValidateWST_Description();
			AssertHasError(abcStyle.WST_DescriptionInfo, "Please enter a Product Style Description.");

			abcStyle.WST_Description = "Hey";
			AssertNoErrors(abcStyle.WST_DescriptionInfo);
		}

		#endregion

		#region TestWST_OH_Owner

		public void TestWST_OH_Owner()
		{
			var client1 = Helper.CreateClient("O1");
			var client2 = Helper.CreateClient("O2");
			var style = Helper.CreateProductStyle("S1", "Style1", client1.PK);
			var colour = style.Colours.AddNew();
			colour.WSC_Code = "ABC";
			colour.WSC_Description = "D";

			var size1 = style.Sizes.AddNew();
			size1.WSZ_Size = "1";
			size1.WSZ_Sequence = 1;
			var size2 = style.Sizes.AddNew();
			size2.WSZ_Size = "2";
			size2.WSZ_Sequence = 1;
			AssertNoErrors(style.WST_OH_OwnerInfo);

			style.WST_OH_Owner = client2.PK;
			AssertNoErrors(style.WST_OH_OwnerInfo);
			style.WST_OH_Owner = client1.PK;
			AssertNoErrors(style.WST_OH_OwnerInfo);

			var part1 = Helper.CreateProduct("P1", client1);
			part1.OP_PartNum = "123";
			part1.OP_Desc = "OneTwoThree";
			part1.OP_WSC_WhsProductStyleColour = colour.PK;
			part1.OP_WSZ_WhsProductStyleSize = size1.PK;
			Factory.Save();

			style.WST_OH_Owner = client2.PK;
			AssertHasError(style.WST_OH_OwnerInfo, @"Owner cannot be changed as it is being used on the following products:
123 - OneTwoThree");
			style.WST_OH_Owner = client1.PK;
			AssertNoErrors(style.WST_OH_OwnerInfo);

			var part2 = Helper.CreateProduct("P1", client1);
			part2.OP_PartNum = "456";
			part2.OP_Desc = "FourFiveSix";
			part2.OP_WSC_WhsProductStyleColour = colour.PK;
			part2.OP_WSZ_WhsProductStyleSize = size2.PK;
			Factory.Save();

			style.WST_OH_Owner = client2.PK;
			AssertHasError(style.WST_OH_OwnerInfo, @"Owner cannot be changed as it is being used on the following products:
123 - OneTwoThree
456 - FourFiveSix");

			style.WST_OH_Owner = client1.PK;
			AssertNoErrors(style.WST_OH_OwnerInfo);

			var style2 = Factory.New<WhsProductStyle>();
			style2.WST_OH_Owner = client2.PK;
			AssertNoErrors(style2.WST_OH_OwnerInfo);

			style2.Colours.AddNew();
			style2.WST_OH_Owner = client1.PK;
			AssertNoErrors(style2.WST_OH_OwnerInfo);
		}

		#endregion

		#region TestValidateAtLeastOneColour

		public void TestValidateAtLeastOneColour()
		{
			var expectedErrorMessage = "At least One Color is required.";
			var style = Helper.CreateProductStyle();
			style.Validation.ValidateAll();
			style.Validation.ValidateAll();
			AssertHasRowError(style, expectedErrorMessage);
			AssertEquals("Should only have 1 row error", 1, style.RowErrors.Count(e => e.Message.Equals(expectedErrorMessage)));

			style.AddRowError("TestError");
			style.Colours.AddNew();
			style.Validation.ValidateAll();
			AssertNoRowError(style, expectedErrorMessage);
			AssertHasRowError("Should not clear Hack Error", style, "TestError");
		}

		#endregion

		#region TestValidateAtLeastOneSize

		public void TestValidateAtLeastOneSize()
		{
			var expectedErrorMessage = "At least One Size is required.";
			var style = Helper.CreateProductStyle();
			style.Validation.ValidateAll();
			style.Validation.ValidateAll();
			AssertHasRowError(style, expectedErrorMessage);
			AssertEquals("Should only have 1 row error", 1, style.RowErrors.Count(e => e.Message.Equals(expectedErrorMessage)));

			style.AddRowError("TestError");
			style.Sizes.AddNew();
			style.Validation.ValidateAll();
			AssertNoRowError(style, expectedErrorMessage);
			AssertHasRowError("Should not clear Hack Error", style, "TestError");
		}

		#endregion
	}
}
