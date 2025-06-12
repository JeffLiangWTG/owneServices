using System.IO;
using System.Threading.Tasks;

namespace OcmPoc.Transceivers
{
	public interface ISendingConnection : IConnection
	{
		Task SendAsync(string name, Stream content);
	}
}