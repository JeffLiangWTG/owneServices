using Enterprise.Security;

namespace Enterprise.Customs.Business.MessagingProcess
{
	public interface ISupportSecurityCheckpoints
	{
		SecurityCheckpoint SendMessagesSecurityCheckpoint { get; }
		SecurityCheckpoint SendMessagesWithErrorsSecurityCheckpoint { get; }
		SecurityCheckpoint SendMessagesWithErrorsOverrideSecurityCheckpoint { get; }
	}
}
