using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class Base26NumberConverterTests : TestCaseWithFactory
	{
		public void TestConversionRange()
		{
			for (var expectedNum = 1; expectedNum < 100000; expectedNum++)
			{
				var text = Base26NumberConverter.GetLetterRepresentation(expectedNum);
				var actualNum = Base26NumberConverter.GetNumberRepresentation(text);
				AssertEquals(expectedNum.ToString(), expectedNum, actualNum);
			}
		}

		public void TestGetLetterRepresentationRange_Under()
		{
			AssertEquals(string.Empty, Base26NumberConverter.GetLetterRepresentation(0));
			AssertEquals(string.Empty, Base26NumberConverter.GetLetterRepresentation(-1));
		}

		public void TestGetLetterRepresentationRange_VeryBig()
		{
			AssertEquals("ZZZZ", Base26NumberConverter.GetLetterRepresentation(475254));
			AssertEquals(475254, Base26NumberConverter.GetNumberRepresentation("ZZZZ"));
			AssertEquals("ABKPT", Base26NumberConverter.GetLetterRepresentation(500000));
			AssertEquals(500000, Base26NumberConverter.GetNumberRepresentation("ABKPT"));
			AssertEquals("ZZZZZZ", Base26NumberConverter.GetLetterRepresentation(321272406));
			AssertEquals(321272406, Base26NumberConverter.GetNumberRepresentation("ZZZZZZ"));
		}

		public void TestGetLetterRepresentationRange_Over()
		{
			AssertExceptionThrown((Base26NumberConverter.MaxNumberRepresentation + 1).ToString(), typeof(ArgumentOutOfRangeException), () => Base26NumberConverter.GetLetterRepresentation(Base26NumberConverter.MaxNumberRepresentation + 1));
			AssertExceptionThrown("321272407", typeof(ArgumentOutOfRangeException), () => Base26NumberConverter.GetLetterRepresentation(321272407));
			AssertExceptionThrown("AAAAAAA", typeof(ArgumentOutOfRangeException), () => Base26NumberConverter.GetNumberRepresentation("AAAAAAA"));
		}

		public void TestBackwardCompatibility()
		{
			for (var i = 0; i < Enterprise.Accounting.Integration.CommonUtils.MaxNumberRepresentation; i++)
			{
				var expectedText = Enterprise.Accounting.Integration.CommonUtils.GetLetterRepresentation(i);
				var actualText = Base26NumberConverter.GetLetterRepresentation(i);
				AssertEquals(expectedText, actualText);

				var expectedNumber = Enterprise.Accounting.Integration.CommonUtils.GetNumberRepresentation(expectedText);
				var actualNumber = Base26NumberConverter.GetNumberRepresentation(expectedText);
				AssertEquals(expectedNumber, actualNumber);
			}
		}
	}
}
