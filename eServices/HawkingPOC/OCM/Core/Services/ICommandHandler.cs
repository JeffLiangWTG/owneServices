using System.Threading.Tasks;

namespace OcmPoc.Core.Services
{
	public interface ICommandHandler<T>
    {
		Task HandleAsyc(T command);
    }
}
