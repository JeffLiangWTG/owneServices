using System;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	[TestedType(typeof(NatureAndQtyOfGoods))]
	[TestsSubclassesOf(typeof(NatureAndQtyOfGoods))]
	public class NatureAndQtyOfGoodsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCreateInstance()
		{
			AssertExceptionThrown(typeof(NullReferenceException), () => new NatureAndQtyOfGoods(null));

			ExportAWBRateLine line = Factory.New<ExportAWBRateLine>();
			NatureAndQtyOfGoods natureAndQtyOfGoods = new NatureAndQtyOfGoods(line);

			AssertEquals(line, natureAndQtyOfGoods.ParentRateLine);
			AssertEquals(line.Factory, natureAndQtyOfGoods.Factory);
		}

		public virtual void TestFont()
		{
			Font font = NatureAndQtyOfGoods.Font;
			AssertNotNull(font);
			AssertEquals(font, NatureAndQtyOfGoods.Font);
		}

		public virtual void TestText()
		{
			NatureAndQtyOfGoods natureAndQtyOfGoods = (NatureAndQtyOfGoods)GetNewBusinessObject();
			AssertEquals(ZString.Empty, natureAndQtyOfGoods.Text);

			natureAndQtyOfGoods.Text = "AAA";
			AssertEquals("AAA", natureAndQtyOfGoods.Text);
		}

		public virtual void TestTextSize()
		{
			NatureAndQtyOfGoods natureAndQtyOfGoods = (NatureAndQtyOfGoods)GetNewBusinessObject();
			AssertEquals("prerequisite", ZString.Empty, natureAndQtyOfGoods.Text);
			AssertEquals(0, natureAndQtyOfGoods.TextSize);

			natureAndQtyOfGoods.Text = "III";
			int textSize1 = natureAndQtyOfGoods.TextSize;

			natureAndQtyOfGoods.Text = "WWW";
			int textSize2 = natureAndQtyOfGoods.TextSize;

			Assert("'WWW' should be bigger than 'III'", textSize2 > textSize1);
		}

		public void TestValidation()
		{
			NatureAndQtyOfGoods natureAndQtyOfGoods = (NatureAndQtyOfGoods)GetNewBusinessObject();
			AssertNotNull(natureAndQtyOfGoods.Validation);
			AssertEquals(ExpectedValidationType, natureAndQtyOfGoods.Validation.GetType());
		}

		public virtual Type ExpectedValidationType
		{
			get { return typeof(NatureAndQtyOfGoodsValidation); }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NatureAndQtyOfGoods(Factory.New<ExportAWBRateLine>());
		}
	}
}
