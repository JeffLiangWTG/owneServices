using Common.Logging;

namespace CargoWise.eHub.Products.JPCustoms.Client
{
	public interface IMailClientConfiguration
	{
		string Server { get; }
		int Port { get; }
		ILog Logger { get; }
	}
}
