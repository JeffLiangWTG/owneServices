using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.Blazor.Common;
using Enterprise.ZArchitecture.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Enterprise.Winzor.Architecture;

/// <summary>
/// Maintains a pool of open windows
/// Should only be used by applications hosted in the client app
/// Improves performance by allowing operations that are vulnerable to high round trip latency to happen before the window is needed
/// such as: TCP socket negotiation, SSL handshakes, establishing the Blazor circuit
/// </summary>
public class EntryPointPool
{
	/// <summary>
	/// Reserved path where entry points enter the pool
	/// </summary>
	public const string EntryPointPoolPath = "/pool/";
	readonly EntryPointPoolOptions entryPointPoolOptions;
	readonly IOpeningFormQueue formChannel;
	readonly ILogger<EntryPointPool> logger;
	uint poolSize;
	int loadInFlight;

	/// <summary>
	/// The number of entry point components available in the pool
	/// </summary>
	public uint PoolSize => poolSize;

	/// <summary>
	/// ctor
	/// </summary>
	/// <param name="entryPointPoolOptions"></param>
	/// <param name="formChannel">The channel should be unbounded</param>
	/// <param name="logger"></param>
	public EntryPointPool(IOptions<EntryPointPoolOptions> entryPointPoolOptions, IOpeningFormQueue formChannel, ILogger<EntryPointPool> logger)
	{
		this.entryPointPoolOptions = entryPointPoolOptions.Value;
		this.formChannel = formChannel;
		this.logger = logger;
	}

	/// <summary>
	/// Used by <see cref="EntryPointComponent"/> when pooling is enabled.
	/// Waits for a form to be available and then returns it to the listener
	/// </summary>
	/// <returns>Form</returns>
	public async Task<Form> EnterAsync(Uri uri, IWindowService windowService)
	{
		Interlocked.Increment(ref poolSize);

		Form form;

		try
		{
			CheckPool(uri, windowService);
			form = await formChannel.ReadAsync();
		}
		finally
		{
			Interlocked.Decrement(ref poolSize);
			CheckPool(uri, windowService);
		}

		return form;
	}

	/// <summary>
	/// Checks if the pool has any space available and sends load requests to fill it
	/// This method does not guarantee that the pool will be filled
	/// Calls are ignored if the pool fill wait period has not expired since the last call
	/// </summary>
	/// <param name="baseUri"></param>
	/// <param name="windowService"></param>
	public void CheckPool(Uri baseUri, IWindowService windowService)
	{
		if (Interlocked.CompareExchange(ref loadInFlight, 1, 0) != 0)
		{
			return;
		}

		try
		{
			var poolSpace = Math.Max(entryPointPoolOptions.MinimumPoolSize - (long)poolSize, 0);

			for (var i = 0; i < poolSpace; i++)
			{
				var uri = new UriBuilder(baseUri)
				{
					// The client app activates requests to the same path
					// We add a Guid to the path here to ensure that a new window is created
					Path = $"{EntryPointPoolPath}{Guid.NewGuid()}",
				};

#pragma warning disable VSTHRD110 // We intentionally fire and forget here because we do NOT want to wait for a roundtrip to fill the pool to block the form load
				windowService.RequestCreateHiddenWindowAsync(new CreateWindowOptions { Uri = uri.Uri })
				.ContinueWith(
				(t, _) =>
				{
					if (t.Exception is not null)
					{
						logger.LogError(t.Exception, (NoResString)"An unhandled exception occurred while filling the pool");
					}
				},
				null,
				TaskScheduler.Default);
#pragma warning restore VSTHRD110 // Observe result of async calls
			}
		}
		finally
		{
#pragma warning disable VSTHRD110 // We intentionally fire and forget this delay to release to the caller while queuing an action to clear the loadInFlight variable after a timeout
			Task.Delay(entryPointPoolOptions.PoolFillWaitPeriod)
				.ContinueWith((_, _) => Interlocked.Exchange(ref loadInFlight, 0), null, TaskScheduler.Default);
#pragma warning restore VSTHRD110 // Because this call is not awaited, execution of the current method continues before the call is completed
		}
	}
}
