using System.Threading.Tasks;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;
public class DataGridTextBoxTest
{
	[Test, WithPlaywrightPage]
	public async Task TestIsInEditOrNavigateModeAndGridValueWhenOnInputEvent()
	{
		await using var ctx = new InMemoryTestServerContext();
		DataGridTextBox dataGridTextBox = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var parentDataGrid = new DataGrid();
			dataGridTextBox = new DataGridTextBox();
			dataGridTextBox.SetDataGrid(parentDataGrid);
			var form = new Form();
			form.Controls.Add(dataGridTextBox);
			return form;
		});

		Assert.That(dataGridTextBox.IsInEditOrNavigateMode);
		await page.FocusAsync(".textbox");

		var str = "烧饼好吃";
		var dataGridTextBoxInput = page.Locator(".textbox");
		await dataGridTextBoxInput.FillAsync(str);

		for (int i = 0; i < 5; i++)
		{
			if (dataGridTextBox.Text.Equals(str))
			{
				break;
			}
			await Task.Delay(500);
		}

		Assert.That(!dataGridTextBox.IsInEditOrNavigateMode);
	}
}
