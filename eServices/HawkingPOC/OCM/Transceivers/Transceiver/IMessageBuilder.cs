using OcmPoc.Infrastructure.MessageInterfaces.Documents;

namespace OcmPoc.Transceivers
{
	public interface IMessageBuilder
	{
		Message Build(string name, byte[] content);
	}
}
