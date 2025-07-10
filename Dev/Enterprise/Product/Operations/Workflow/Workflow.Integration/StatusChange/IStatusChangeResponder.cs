using Enterprise.Integration;

namespace Enterprise.Workflow.Integration
{
	public interface IStatusChangeResponder
	{
		StatusChangeResponder Responder { get; }

		bool RespondsToStatusChange(IProcessTask task, string newStatus);

		(bool result, string reason) CanChangeStatus(IProcessTask task, string newStatus);

		StatusChangeResult RespondToChange(IProcessTask task, string newStatus);
	}
}
