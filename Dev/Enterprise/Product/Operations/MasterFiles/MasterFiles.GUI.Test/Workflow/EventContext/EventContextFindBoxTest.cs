using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class EventContextFindBoxTest : TestCase
	{
		public void TestGetList()
		{
			using (var findBox = new EventContextFindBox())
			{
				AssertNull(findBox.List);
			}
		}
	}
}
