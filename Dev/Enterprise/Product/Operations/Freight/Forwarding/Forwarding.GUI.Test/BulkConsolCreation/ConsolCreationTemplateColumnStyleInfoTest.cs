using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	class ConsolCreationTemplateColumnStyleInfoTest : TestCase
	{
		public void TestColumnStyleType()
		{
			AssertEquals(typeof(ConsolCreationTemplateColumnStyle), new ConsolCreationTemplateColumnStyleInfo().ColumnStyleType);
		}
	}
}
