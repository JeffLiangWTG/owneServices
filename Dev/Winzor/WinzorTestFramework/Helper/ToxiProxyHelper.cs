using System;
using WTG.PlaywrightTesting;

namespace WinzorTestFramework;

public static class ToxiProxyHelper
{
	public static string CustomReconnectionOptions(int maxRetries, TimeSpan retryInterval)
			=> $"window.winzorReconnectionOptions = {{ maxRetries: {maxRetries}, retryIntervalMilliseconds: {retryInterval.TotalMilliseconds} }}";

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
	public static void CleanupPageErrors()
	{
		// Because we are purposely disconnecting and reconnecting the page we need to clear errors
		// captured by the playwright test context that were caused by the network connection dropping
		WithPlaywrightPageAttribute.Context.ConsoleMessages.RemoveAll(error =>
			error.Text.Contains("WebSocket closed with status code: 1006") ||
			error.Text.Contains("Failed to load resource: net::ERR_CONNECTION_REFUSED") ||
			error.Text.Contains("Failed to complete negotiation with the server: TypeError: Failed to fetch")
		);

		WithPlaywrightPageAttribute.Context.PageErrors.RemoveAll(error =>
			error.Contains("Cannot send data if the connection is not in the 'Connected' State")
		);
	}
}

