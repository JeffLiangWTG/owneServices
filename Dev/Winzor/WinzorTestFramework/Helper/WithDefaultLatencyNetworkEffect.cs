using System;
using System.Threading.Tasks;
using WTG.ToxicNetworkEffect;

namespace WinzorTestFramework;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class WithDefaultLatencyNetworkEffect : WithLatencyNetworkEffect
{
	readonly static string DefaultProxyName = "test-proxy";
	readonly static string DefaultListen = "127.0.0.1:5001";
	public static string ServerBaseUrl => "http://" + DefaultListen;

	public static async Task DisconnectProxyAsync()
	{
		var proxy = await WithToxiProxy.GetProxy(DefaultProxyName);
		proxy.Enabled = false;
		await proxy.UpdateAsync();
	}

	public static async Task ReconnectProxyAsync()
	{
		var proxy = await WithToxiProxy.GetProxy(DefaultProxyName);
		proxy.Enabled = true;
		await proxy.UpdateAsync();
	}

	public WithDefaultLatencyNetworkEffect()
	{
		Listen = DefaultListen;
		Upstream = "127.0.0.1:5000";
		ProxyName = DefaultProxyName;
	}
}
