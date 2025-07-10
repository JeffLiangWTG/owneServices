using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoWise.Winzor.Telemetry;

public interface IWinzorTelemetry
{
	ActivitySource? LoadingActivity { get; }
	ActivitySource? RenderingActivity { get; }
	ActivitySource? InternalActivity { get; }
}

public class WinzorTelemetry : IWinzorTelemetry, IDisposable
{
	public ActivitySource LoadingActivity { get; } = new ActivitySource("CargoWise.Winzor.Loading");
	public ActivitySource RenderingActivity { get; } = new ActivitySource("CargoWise.Winzor.Rendering");
	public ActivitySource InternalActivity { get; } = new ActivitySource("CargoWise.Winzor.Internal");

	public void Dispose()
	{
		LoadingActivity.Dispose();
		RenderingActivity.Dispose();
		InternalActivity.Dispose();
	}
}
