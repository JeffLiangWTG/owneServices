using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Enterprise.Winzor.Architecture;

/// <summary>
/// Interally maintains a FIFO queue of forms which are waiting to be attached to an <see cref="EntryPointComponent" />
/// Calling ReadAsync allows callers to wait for a form to become available if no forms are in the queue
/// </summary>
public class OpeningFormQueue : IOpeningFormQueue
{
	readonly Channel<Form> channel;

	public OpeningFormQueue()
	{
		channel = Channel.CreateUnbounded<Form>(
				new UnboundedChannelOptions
				{
					SingleWriter = false,
					SingleReader = false,
					// According to https://devblogs.microsoft.com/dotnet/an-introduction-to-system-threading-channels/
					// AllowSynchronousContinuations would allow 'callbacks' to be executed on the writing thread
					// As our Writer is on a WinzorDispatcher thread and our Reader is on a Blazor render thread
					// Allowing synchronous continuations could have unexpected effects such as deadlocking
					// Or actions executing in the wrong context.
					AllowSynchronousContinuations = false
				});
	}

	/// <summary>
	/// Read a form from the queue
	/// Multiple readers are allowed
	/// The reader will enter a queue if no form is available
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public ValueTask<Form> ReadAsync(CancellationToken cancellationToken = default)
	{
		return channel.Reader.ReadAsync(cancellationToken);
	}

	/// <summary>
	/// Adds a form to the queue
	/// </summary>
	/// <param name="form"></param>
	/// <exception cref="InvalidOperationException"></exception>
	public void Write(Form form)
	{
		var success = channel.Writer.TryWrite(form);

		if (!success)
		{
			throw new InvalidOperationException("Write should never fail because the channel should be unbounded");
		}
	}
}
