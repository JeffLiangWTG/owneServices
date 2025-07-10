using System.Linq;
using Bunit;
using NUnit.Framework;

namespace WinzorTestFramework;

public static class DataGridTestHelper
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
	public static void AssertGridSort(IRenderedFragment rendered, string[] headers, string[] cells)
	{
		for (var i = 0; i < headers.Length; i++)
		{
			var col = i + 1;
			if (!string.IsNullOrEmpty(headers[i]))
			{
				Assert.That(rendered.FindAll("th")[col].Attributes["class"].Value, Is.EqualTo(headers[i]));
				Assert.That(rendered.FindAll("th")[col].GetElementsByTagName("div")[0].GetAttribute("style"), Is.Empty);
			}
		}

		var editControl = rendered.FindAll("input").FirstOrDefault();
		var row = 0;
		for (var j = 0; j < cells.Length; j++)
		{
			if (j % headers.Length == 0)
			{
				row++;
			}
			var cell = rendered.FindAll("tr")[row].ChildNodes[j % headers.Length + 1];
			if (editControl is not null && cell.Contains(editControl))
			{
				Assert.That(editControl.GetAttribute("value"), Is.EqualTo(cells[j]));
			}
			else
			{
				Assert.That(cell.TextContent, Is.EqualTo(cells[j]));
			}
		}
	}
}
