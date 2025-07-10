using System.Threading.Tasks;
using Bunit;
using NUnit.Framework;
using WinzorTestFramework;

namespace System.Windows.Forms;
public class ErrorProviderTest
{
	[Test]
	public async Task ShowSymbolAndMessageWhenActive()
	{
		using var ctx = new WinzorTestContext();

		var warningMessage = "Warning-Test";

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var dataGrid = new DataGrid();
			var errorProvider = new ErrorProvider();
			errorProvider.NotificationType = "warning";
			errorProvider.SetError(dataGrid, warningMessage);
			dataGrid.SetErrorProvider(errorProvider);

			return dataGrid;
		});

		var warningSymbol = rendered.Find(".notification");
		Assert.That(warningSymbol, Is.Not.Null);

		var warningMessageFromDOM = warningSymbol.GetAttribute("title");
		Assert.That(warningMessageFromDOM, Is.EqualTo(warningMessage));
	}
}
