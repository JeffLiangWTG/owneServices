using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.eTail.GUI.Testing.DeniedPartyScreening
{
	public class ZCustomZeroDisplayColumnStyleTest : TestCaseWithDummy
	{
		public void TestZCustomZeroDisplayColumnStyle_WhenValueIsZero_ShouldShowNA()
		{
			using (var style = new ZCustomZeroDisplayColumnStyleForTesting(new ZCustomZeroDisplayColumnStyleInfo()))
			{
				AssertEquals("N/A", style.GetFormatValueObjectForTest(Dummy, new ZInt(0)));
				AssertEquals("N/A", style.GetFormatValueObjectForTest(Dummy, new ZLong(0)));
				AssertEquals("N/A", style.GetFormatValueObjectForTest(Dummy, new ZShort(0)));
				AssertEquals("N/A", style.GetFormatValueObjectForTest(Dummy, new ZDecimal(0)));
				AssertEquals("N/A", style.GetFormatValueObjectForTest(Dummy, new ZByte(0)));
			}
		}

		public void TestZCustomZeroDisplayColumnStyle_WhenValueIsNotZero_ShouldShowTheNumber()
		{
			using (var style = new ZCustomZeroDisplayColumnStyleForTesting(new ZCustomZeroDisplayColumnStyleInfo()))
			{
				AssertEquals("85.00", style.GetFormatValueObjectForTest(Dummy, new ZInt(85)));
				AssertEquals("-25.00", style.GetFormatValueObjectForTest(Dummy, new ZInt(-25)));
				AssertEquals("123,456,789,000.00", style.GetFormatValueObjectForTest(Dummy, new ZLong(123456789000)));
				AssertEquals("64.00", style.GetFormatValueObjectForTest(Dummy, new ZShort(64)));
				AssertEquals("9.55", style.GetFormatValueObjectForTest(Dummy, new ZDecimal(9.55)));
				AssertEquals("5.00", style.GetFormatValueObjectForTest(Dummy, new ZByte(5)));
			}
		}
	}

	public class ZCustomZeroDisplayColumnStyleForTesting : ZCustomZeroDisplayColumnStyle
	{
		public ZCustomZeroDisplayColumnStyleForTesting(ZCustomZeroDisplayColumnStyleInfo columnInfo) : base(columnInfo)
		{
		}
		public string GetFormatValueObjectForTest(object source, object propertyValue)
		{
			return FormatValueObjectCore(source, propertyValue);
		}
	}
}
