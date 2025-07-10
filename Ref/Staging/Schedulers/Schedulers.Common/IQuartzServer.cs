using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.Staging.Schedulers.Common
{
	public interface IQuartzServer
	{
		Task Initialize();

		Task Start();

		Task Stop();

		Task Pause();

		Task Resume();
	}
}
