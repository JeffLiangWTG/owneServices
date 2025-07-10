using CargoWise.Blazor.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WinzorFramework;

namespace Enterprise.Winzor.Architecture.Test;

public static class EnterpriseTestContextExtension
{
	public static EnterpriseTestContext ConfigureEntryPointPool(this EnterpriseTestContext ctx, EntryPointPoolOptions entryPointPoolOptions)
	{
		ctx.Services.AddSingleton(Options.Create(entryPointPoolOptions));
		ctx.Services.AddSingleton<IFormOpener, PoolingFormOpener>();

		return ctx;
	}
}
