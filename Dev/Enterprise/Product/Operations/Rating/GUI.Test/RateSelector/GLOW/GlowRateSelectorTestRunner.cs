#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.ZArchitecture.GUI.BrowserInterop;
using Moq;
using Newtonsoft.Json;

namespace Enterprise.Rating.GUI.Testing;

public class GlowRateSelectorTestRunner : IDisposable
{
	readonly Latch setupDoneLatch = new();
	readonly CancellationTokenSource cts = new();

	Mock<IBrowserInteropWindow> mockedBrowserWindow = null!;
	Mock<IMessageTransportLayer> mockedTransportLayer = null!;

	readonly Dictionary<string, Action<BrowserMessageEventArgs<string>>> browserWindowHandlers = [];
	readonly BlockingCollection<BrowserMessageEventArgs<string>> sentFromBrowser = [];
	readonly Queue<(string kind, object data)> sentToBrowser = new();

	public GlowRateSelectorResult? GlowRateSelectorResult { get; private set; }
	public IUpdateFilterData? UpdateFilterData { get; private set; }
	public CancellationToken CancellationToken => cts.Token;

	public void Run(RatingCriteria criteria, Action? action = null)
	{
		SetupBrowserWindow();
		SetupWindowFactory();

		var ratingContext = new Mock<IRatingContext>().Object;
		var rateSelector = new GlowRateSelector();

		Task.Run(() =>
		{
			try
			{
				using (Db.DisposableActionForDbConnection())
				{
					setupDoneLatch.Wait(CancellationToken);
					SendMessageFromBrowser(WebViewCommands.Ready, "");
					action?.Invoke();
				}
			}
			finally
			{
				SendMessageFromBrowser(WebViewCommands.SelectRow, "");
			}
		});

		GlowRateSelectorResult = rateSelector.ShowDialog(ratingContext, criteria);
	}

	public void SendMessageFromBrowser<T>(string kind, T payload) => sentFromBrowser.Add(new BrowserMessageEventArgs<string>
	{
		Kind = kind,
		Payload = JsonConvert.SerializeObject(payload)
	});

	public (string kind, object data)? GetLastMessageSentToBrowser() => sentToBrowser.Count > 0 ? sentToBrowser.Dequeue() : null;

	public void Dispose() => cts.Cancel();

	#region Mock

	void SetupBrowserWindow()
	{
		mockedTransportLayer = new Mock<IMessageTransportLayer>();
		mockedTransportLayer
			.Setup(m => m.SendToBrowserAsync(It.IsAny<string>(), It.IsAny<object>()))
			.Callback((string kind, object data) =>
			{
				if (data is IUpdateFilterData filterData)
				{
					UpdateFilterData = filterData;
				}
				else
				{
					sentToBrowser.Enqueue((kind, data));
				}
			});
		mockedTransportLayer
			.Setup(m => m.AddCommandHandler(It.IsAny<string>(), It.IsAny<Action<BrowserMessageEventArgs<string>>>()))
			.Callback(browserWindowHandlers.Add);

		mockedBrowserWindow = new Mock<IBrowserInteropWindow>();
		mockedBrowserWindow
			.Setup(b => b.ShowDialog())
			.Returns(() =>
			{
				setupDoneLatch.Release();
				foreach (var message in sentFromBrowser.GetConsumingEnumerable())
				{
					if (message.Kind == WebViewCommands.SelectRow)
					{
						return true;
					}
					if (browserWindowHandlers.TryGetValue(message.Kind, out var handler))
					{
						handler(message);
					}
				}
				return true;
			});
		mockedBrowserWindow.SetupGet(b => b.MessageTransportLayer).Returns(mockedTransportLayer.Object);
	}

	void SetupWindowFactory()
	{
		var mockBrowserInteropWindowFactory = new Mock<IBrowserInteropWindowFactory>();
		mockBrowserInteropWindowFactory.Setup(b => b.CreateBrowserInteropWindow(It.IsAny<string>(), It.IsAny<Uri>())).Returns(mockedBrowserWindow.Object);
		ObjectFactory.Substitute(mockBrowserInteropWindowFactory.Object);
	}

	#endregion
}

public static class PollingHelper
{
	public static T? PollUntilNotNull<T>(Func<T?> poll, CancellationToken cancellationToken, int pollMs = 100, int timeoutMs = 3000)
	{
		var timeoutTask = Task.Delay(timeoutMs, cancellationToken);

		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var value = poll();
			if (value is not null)
			{
				return value;
			}

			var completedTask = Task.WhenAny(timeoutTask, Task.Delay(pollMs, cancellationToken)).GetAwaiter().GetResult();
			if (completedTask == timeoutTask)
			{
				break;
			}
		}

		return default;
	}
}

/// <summary>
/// Single-use latch that allows a thread to wait until another thread signals release.
/// </summary>
public class Latch
{
	readonly TaskCompletionSource<bool> task = new();

	public void Wait(CancellationToken token)
	{
		var completedTask = Task.WaitAny(
			[
				task.Task,
				Task.Run(() =>
				{
					token.WaitHandle.WaitOne();
					token.ThrowIfCancellationRequested();
				})
			],
			token
		);

		if (completedTask == 1)
		{
			token.ThrowIfCancellationRequested();
		}
	}

	public void Release() => task.TrySetResult(true);
}
