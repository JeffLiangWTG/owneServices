using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

class PLEntryMessageUserControlTest : TestCaseWithFactory
{
	public void TestMessageUserControl()
	{
		using (var control = new PLEntryMessageUserControl())
		{
			using (var messageUserControl = control.NewMessageUserControl)
			{
				AssertType<MessageUserControl>(messageUserControl);
			}
		}
	}

	public void TestAttachmentMessageUserControl()
	{
		using (var control = new PLEntryMessageUserControl())
		{
			AssertNotNull("AttachmentsTabPage", control.FindSingleOrDefault<ZTabPage>("AttachmentsTabPage"));
			AssertNotNull("AttachmentMessageUserControl", control.FindSingleOrDefault<AttachmentMessageUserControl>("AttachmentMessageUserControl"));
		}
	}
}
