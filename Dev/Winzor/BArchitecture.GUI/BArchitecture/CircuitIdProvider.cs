using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BArchitecture;

public interface ICircuitIdProvider
{
	string CircuitId { get; }
}

public class CircuitIdProvider : CircuitHandler, ICircuitIdProvider
{
	public string CircuitId { get; private set; } = string.Empty;
	public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
	{
		CircuitId = circuit.Id;
		return Task.CompletedTask;
	}
}
