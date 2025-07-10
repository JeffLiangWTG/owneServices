using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	[TestsSubclassesOf(typeof(NatureAndQtyOfGoodsValidation))]
	public class NatureAndQtyOfGoodsValidationTest : TestCaseWithFactory
	{
		public void TestCreateInstance()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => GetNewValidation(null));
			AssertNoExceptionThrown(() => GetNewValidation(GetNewParent()));
		}

		public void TestParent()
		{
			NatureAndQtyOfGoods parent = GetNewParent();
			NatureAndQtyOfGoodsValidation validation = GetNewValidation(parent);
			AssertEquals(parent, ReflectionUtil.GetPropertyValue(validation, "Parent"));
		}

		public void TestText()
		{
			AssertText();
		}

		protected virtual void AssertText()
		{
			NatureAndQtyOfGoods parent = GetNewParent();
			AssertNoWarning("TextInfo", parent.TextInfo, "The text may be too long to print correctly on the AWB");

			parent.Text = new string('W', 20);
			AssertHasWarning("TextInfo", parent.TextInfo, "The text may be too long to print correctly on the AWB");

			parent.Text = new string('I', 20);
			AssertNoWarning("TextInfo", parent.TextInfo, "The text may be too long to print correctly on the AWB");

			parent.Text = "XXX \u069A";
			AssertHasError("TextInfo", parent.TextInfo, "Text only accepts Western European languages characters.");

			parent.Text = "XXX";
			AssertNoError("TextInfo", parent.TextInfo, "Text only accepts Western European languages characters.");
		}

		public void TestText_ULD()
		{
			ExportAWBRateLine rateLine = Factory.New<ExportAWBRateLine>();
			NatureAndQtyOfGoods parent = new NatureAndQtyOfGoods(rateLine);
			parent.ParentRateLine.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber;

			parent.Text = "BLAHHH";
			AssertHasMessageError(parent.TextInfo, "This is not a valid ULD number.");

			parent.Text = "AAA1234500";
			AssertHasMessageError(parent.TextInfo, "This is not a valid ULD number.");

			parent.Text = "ABCD1234XX";
			AssertNoMessageError(parent.TextInfo, "This is not a valid ULD number.");

			parent.Text = "A123123X1";
			AssertNoMessageError(parent.TextInfo, "This is not a valid ULD number.");
		}

		public void TestText_HCC()
		{
			ExportAWBRateLine rateLine = Factory.New<ExportAWBRateLine>();
			NatureAndQtyOfGoods parent = new NatureAndQtyOfGoods(rateLine);
			parent.ParentRateLine.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode;
			var errorMessage = "Harmonized Commodity Code must be between 6 and 18 characters long.";

			parent.Text = "BLAH";
			AssertHasMessageError(parent.TextInfo, errorMessage);

			parent.Text = "CODE123456";
			AssertNoMessageError(parent.TextInfo, errorMessage);

			parent.Text = "123456789012345678";
			AssertNoMessageError(parent.TextInfo, errorMessage);

			parent.Text = "1234567890123456789";
			AssertHasMessageError(parent.TextInfo, errorMessage);
		}

		[ExpectNoExceptions]
		public void TestCheckTextShouldNotBeAccessingDeletedRateLinesException()
		{
			var rateLine = Factory.New<ExportAWBRateLine>();
			var parent = new NatureAndQtyOfGoods(rateLine);
			var validation = new NatureAndQtyOfGoodsValidation(parent);

			rateLine.Delete();
			validation.ValidateText();
		}

		protected virtual NatureAndQtyOfGoodsValidation GetNewValidation(NatureAndQtyOfGoods parent)
		{
			return new NatureAndQtyOfGoodsValidation(parent);
		}

		protected virtual NatureAndQtyOfGoods GetNewParent()
		{
			return new NatureAndQtyOfGoods(Factory.New<ExportAWBRateLine>());
		}
	}
}
