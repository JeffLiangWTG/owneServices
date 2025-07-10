using System;
using Microsoft.AspNetCore.Components;

namespace WinzorTestFramework;

public class TestNavigationManager : NavigationManager
{
	public TestNavigationManager(string url)
	{
		Initialize(BaseServerUri.ToString(), url);
	}

	protected override void NavigateToCore(string uri, bool forceLoad)
	{
		throw new NotImplementedException();
	}

	public static Uri BaseServerUri = new Uri("https://test.wisecloud.com/");
	public static Uri PoolServerUri = new Uri("https://test.wisecloud.com/pool/");
}
