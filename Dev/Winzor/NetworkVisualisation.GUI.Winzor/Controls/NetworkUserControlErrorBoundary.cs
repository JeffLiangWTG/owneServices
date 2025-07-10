using System;
using System.Threading.Tasks;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CargoWise.NetworkVisualisation.GUI;
public class NetworkUserControlErrorBoundary : ErrorBoundaryBase
{
	protected override Task OnErrorAsync(Exception exception)
	{
		if (exception is TaskCanceledException)
		{
			ErrorReporter.ReportOnce(exception.Message, exception);
		}
		else
		{
			ExceptionReporter.Instance.ReportDeveloperException((NoResString)"Failed to load the NCN", exception);
		}
		return Task.CompletedTask;
	}

	protected override void BuildRenderTree(RenderTreeBuilder builder)
	{
		if (CurrentException is null || CurrentException is TaskCanceledException)
		{
			builder.AddContent(0, ChildContent);
		}
		else if (ErrorContent is not null)
		{
			builder.AddContent(1, ErrorContent(CurrentException));
		}
	}
}
