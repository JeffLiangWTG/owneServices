using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.MasterFiles.Business;

public interface IAddressesValidationManager
{
	Task ValidateAsync(CancellationTokenSource cancellationTokenSource);
	void Validate();
}
