using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.NL.GUI.Testing;

class EntryMessageUserControlTest : TestCaseWithFactory
{
	public void TestMessageUserControlType()
	{
		using (var entryMessageUserControl = new EntryMessageUserControlForTest())
		{
			using (var messageUserControl = entryMessageUserControl.GetMessageUserControl_Exposed())
			{
				AssertType(typeof(MessageUserControl), messageUserControl);
			}
		}
	}

	class EntryMessageUserControlForTest : EntryMessageUserControl
	{
		public BaseCustomsEntryUserControl GetMessageUserControl_Exposed() => base.GetMessageUserControl();
	}
}
