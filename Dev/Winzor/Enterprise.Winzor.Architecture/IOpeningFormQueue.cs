using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Enterprise.Winzor.Architecture;
public interface IOpeningFormQueue
{
	ValueTask<Form> ReadAsync(CancellationToken cancellationToken = default);
	void Write(Form form);
}