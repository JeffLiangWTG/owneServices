using System.Threading.Tasks;

namespace OcmPoc.Transceivers
{
	public interface IConnection
    {
		Task ConnectAsync();
		Task DisconnectAsync();
    }
}
