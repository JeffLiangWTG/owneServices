namespace eServices.Dms.Core.OpsPortal.Tests
{
	public static class SetupExtensions
	{
		public static void SetupQuickGrid(this BunitContext context)
		{
			context.Services.AddQuickGridEntityFrameworkAdapter();
			context.JSInterop.SetupModule("./_content/Microsoft.AspNetCore.Components.QuickGrid/QuickGrid.razor.js")
				.SetupModule("init", _ => true);
		}
	}
}
