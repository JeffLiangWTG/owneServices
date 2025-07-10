using Enterprise.ZArchitecture.Business;

namespace Enterprise.Tracking.Business
{
	public interface IWebUserEditableNoteSupport
	{
		IStmNoteParent NotesParentBO { get; }
		WebUserEditableNote UserEditableNoteHelper { get; }
	}
}
