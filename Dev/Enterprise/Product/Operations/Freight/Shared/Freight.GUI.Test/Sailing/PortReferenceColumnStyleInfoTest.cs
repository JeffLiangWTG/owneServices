using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class PortReferenceColumnStyleInfoTest : TestCase
	{
		public void TestColumnStyleType_AnyCondition_ReturnRequiredColumnStyle()
		{
			var styleInfoToTest = new PortReferenceColumnStyleInfo();

			var actualValue = styleInfoToTest.ColumnStyleType;
			AssertEquals("The column style is has an expected type", typeof(PortReferenceColumnStyle), actualValue);
		}
	}
}
