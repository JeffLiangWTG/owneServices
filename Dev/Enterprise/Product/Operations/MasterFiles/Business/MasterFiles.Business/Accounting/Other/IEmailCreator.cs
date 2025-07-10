using CargoWise.Integration;

namespace Enterprise.MasterFiles.Business
{
	public interface IEmailCreator
	{
		EmailSendResult Create(ITransactionParticipant factory);
	}
}
