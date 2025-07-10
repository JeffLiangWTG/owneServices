using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;

namespace System.Windows.Forms;
class NotRespondingOverlayComponentTest
{
	[Test]
	public async Task PreloadOverlayJSInterop()
	{
		using var ctx = new WinzorTestContext();
		var services = ctx.MockCargoWiseClientServices;
		var interop = new Mock<IOverlayJSInterop>();
		interop.Setup(i => i.PreloadInterop());
		ctx.Services.AddScoped(_ => interop.Object);

		var rendered = await ctx.RenderFormAsync(() => new Form());

		interop.Verify(e => e.PreloadInterop(), Times.Once());
	}
}
