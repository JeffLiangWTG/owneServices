using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	class ConsolCreationTemplateUserControlTest : TestCase
	{
		public void TestGetList()
		{
			using (var findBox = new ConsolCreationTemplateUserControl())
			{
				AssertNull(findBox.List);
			}
		}

		public void TestGetDefaultFilters()
		{
			using (var findBox = new ConsolCreationTemplateUserControl())
			{
				AssertNull(findBox.List);
			}
		}
	}
}
