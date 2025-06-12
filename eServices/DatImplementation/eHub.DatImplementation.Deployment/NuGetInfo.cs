using System;

namespace eHub.DatImplementation.Deployment
{
	public interface INugetInfo
	{
		string ApiKey { get; }
		string PackageSource { get; }
	}

	public class NugetInfo : INugetInfo
	{
		static readonly Lazy<NugetInfo> lazy = new Lazy<NugetInfo>(() => new NugetInfo());
		public static NugetInfo Instance { get; } = lazy.Value;

		public string ApiKey => "eda616ddcfaa33f539ae14d2cd74b8bc3e2b919b";
		public string PackageSource => "https://proget.wtg.zone/nuget/WTG-Internal/v3/index.json";
	}
}
