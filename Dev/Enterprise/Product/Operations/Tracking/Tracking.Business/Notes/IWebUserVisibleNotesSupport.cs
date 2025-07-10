using Enterprise.ZArchitecture.Business;

namespace Enterprise.Tracking.Business
{
	public interface IWebUserVisibleNotesSupport
	{
		IStmNoteParent NotesParentBO { get; }
		WebUserVisibleNotes NotesHelper { get; }
		bool ShowAgentNotes { get; }
	}
}
