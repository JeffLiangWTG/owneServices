using NUnit.Framework;

namespace Enterprise.Freight.CFS.GUI
{
	sealed class DataMenuItemTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestDataMenuItemConstruct()
		{
			using (DataMenuItem testMenuItem = new DataMenuItem())
			{
			}
		}
	}
}
