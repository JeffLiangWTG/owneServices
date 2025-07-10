using System.Collections.Concurrent;
using System.Timers;

namespace WinzorFramework.JSInterop;

public enum ResultOfGetDownloadObject
{
	Succeed,
	NotExist
}

public interface IDownloadObject : IDisposable
{
	// Download file name
	string Name { get; }
	Task DownloadAsync(Stream body, CancellationToken cts);
}

/**
 * An abstract IDownloadObject implementation, 
 */
public abstract class AbsDownloadObject : IDownloadObject
{
	// true if the managed/unmanaged resource has been disposed
	protected bool disposedValue;

	public abstract string Name { get; }

	/**
	 * Download by stream copy, override OpenInStream if you want to provide a custom stream
	 */
	public virtual async Task DownloadAsync(Stream body, CancellationToken cts)
	{
		try
		{
			await using Stream inStream = await OpenInStreamAsync();
			await inStream.CopyToAsync(body, cts);
		}
		finally
		{
			Dispose();
		}
		return;
	}

	protected abstract Task<Stream> OpenInStreamAsync();

	/**
	 * Override and copy this if you want to dispose managed/unmanged resource.
	 */
	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			disposedValue = true;
		}
	}
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}

public interface IDownloadObjectManager
{
	ResultOfGetDownloadObject GetAndRemoveDownloadObject(string objectId, out IDownloadObject? downloadObject);
	string? AddDownloadObject(IDownloadObject downloadObject);
}

public class DownloadObjectManager : IDownloadObjectManager, IDisposable
{
	bool _disposedValue;

	readonly ConcurrentDictionary<string, IDownloadObject> _dictId2WaitingObjects = new ConcurrentDictionary<string, IDownloadObject>();

	public bool IsWaitingObjectsEmpty { get => _dictId2WaitingObjects.IsEmpty; }

	// the defer time is 2m, if a download object is not downloaded in 2m, the object will be removed and disposed.
	readonly System.Timers.Timer _deferResourceReleaseTimer;

	public bool IsDeferTimerEnabled { get => _deferResourceReleaseTimer.Enabled; }

	public DownloadObjectManager(double deferResourceReleaseTime = 1000 * 120)
	{
		_deferResourceReleaseTimer =
			new System.Timers.Timer() { AutoReset = true, Interval = deferResourceReleaseTime };
		_deferResourceReleaseTimer.Elapsed += OnDeferResourceReleaseTimerElapsed;
	}
	void DeferResourceRelease()
	{
		_deferResourceReleaseTimer.Stop();
		_deferResourceReleaseTimer.Start();
	}

	void OnDeferResourceReleaseTimerElapsed(object? sender, ElapsedEventArgs e)
	{
		_deferResourceReleaseTimer.Stop();
		ReleaseWaitingObjects();
	}

	void ReleaseWaitingObjects()
	{
		// Release the unused objects 
		foreach (var (objectId, downloadObject) in _dictId2WaitingObjects)
		{
			downloadObject.Dispose();
		}

		_dictId2WaitingObjects.Clear();
	}

	public string? AddDownloadObject(IDownloadObject downloadObject)
	{
		var objectId = Guid.NewGuid().ToString();
		if (_dictId2WaitingObjects.TryAdd(objectId, downloadObject))
		{
			DeferResourceRelease();
			return objectId;
		}
		else
		{
			return null;
		}
	}

	public ResultOfGetDownloadObject GetAndRemoveDownloadObject(string objectId, out IDownloadObject? downloadObject)
	{
		if (_dictId2WaitingObjects.TryRemove(objectId, out var removed))
		{
			DeferResourceRelease();
			downloadObject = removed;
			return ResultOfGetDownloadObject.Succeed;
		}
		else
		{
			downloadObject = default(IDownloadObject);
			return ResultOfGetDownloadObject.NotExist;
		}
	}
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			_deferResourceReleaseTimer.Stop();
			ReleaseWaitingObjects();
			_disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
