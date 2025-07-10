using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusInBondMoveLineItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestParent()
		{
			AssertEquals("Parent", line, line.Validation.Parent);
		}

		public void TestCheckBI_QuantityUQ()
		{
			line.BI_QuantityUQ = "XXX";
			AssertHasMessageErrorContaining(line.BI_QuantityUQInfo, ListValidation.InvalidCodeMessageError);
			line.BI_QuantityUQ = "PKG";
			AssertNoMessageErrorContaining(line.BI_QuantityUQInfo, ListValidation.InvalidCodeMessageError);
			var targetInfo = line.BI_QuantityUQInfo;
			line.BI_Quantity = 0;
			line.BI_QuantityUQ = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			line.BI_Quantity = 10;
			line.BI_QuantityUQ = ZString.Empty;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			line.BI_Quantity = -1;
			line.BI_QuantityUQ = Weight.Kilograms;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			line.BI_Quantity = 10;
			line.BI_QuantityUQ = Weight.Kilograms;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBI_Quantity()
		{
			var targetInfo = line.BI_QuantityInfo;
			line.BI_QuantityUQ = ZString.Empty;
			line.BI_Quantity = 0;
			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeZero);
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
			});
			line.BI_QuantityUQ = Weight.Kilograms;
			line.BI_Quantity = 0;
			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeZero);
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
			});
			line.BI_Quantity = -1;
			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeZero);
				AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
			});
			line.BI_Quantity = 1;
			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeZero);
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
			});
		}

		public void TestCheckBI_Description()
		{
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.SEA;
			line.BI_Description = ZString.Empty;
			AssertNoMessageErrorContaining(line.BI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AIR;
			line.Validation.ValidateBI_Description();
			AssertHasMessageErrorContaining(line.BI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			line.BI_Description = "Test";
			AssertNoMessageErrorContaining(line.BI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			line = moveDetail.CusInBondMoveLineItemCollection.AddNew();
		}

		public void TestCheckBI_PackagingDescription()
		{
			line.TW_IsCoPackaged = false;
			line.BI_PackagingDescription = ZString.Empty;
			line.Validation.ValidateAll();
			AssertNoMessageErrorContaining(line.BI_PackagingDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			line.BI_PackagingDescription = "XXX";
			AssertNoMessageErrorContaining(line.BI_PackagingDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			line.TW_IsCoPackaged = true;
			line.BI_PackagingDescription = ZString.Empty;
			AssertHasMessageErrorContaining(line.BI_PackagingDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			line.BI_PackagingDescription = "XXX";
			AssertNoMessageErrorContaining(line.BI_PackagingDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		CusInBondMoveLineItem line;
		CusInBondHeader header;
	}
}
