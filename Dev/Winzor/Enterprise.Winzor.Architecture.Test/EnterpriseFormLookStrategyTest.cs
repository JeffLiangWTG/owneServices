using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

public class EnterpriseFormLookStrategyTest
{
	[TestCase(0, 0, -1, -1, FormStartPosition.CenterScreen, TestName = "{m}(CenterScreen)")]
	[TestCase(1, 0, 0, 0, FormStartPosition.Manual, TestName = "{m}(Manual_NonZeroLeft)")]
	[TestCase(0, 1, 0, 0, FormStartPosition.Manual, TestName = "{m}(Manual_NonZeroTop)")]
	[TestCase(0, 0, 5, 5, FormStartPosition.Manual, TestName = "{m}(Manual_SavedLayout)")] // saved layout as width and height exceed zero
	public async Task SetPositionAndSizeUpdatesStartPosition(int newLeft, int newTop, int newWidth, int newHeight, FormStartPosition expected)
	{
		using var ctx = new EnterpriseTestContext();
		Form form = null;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			return form = new Form() { StartPosition = FormStartPosition.Manual };
		});

		EnterpriseFormLookStrategy.SetPositionAndSize(form, newLeft, newTop, newWidth, newHeight);
		Assert.That(form.StartPosition, Is.EqualTo(expected));
	}
}
