using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public interface ITaskDetailsControl : IWorkflowItemsControl
	{
		ZRichTextBox NotesRichTextBox { get; }
	}
}
