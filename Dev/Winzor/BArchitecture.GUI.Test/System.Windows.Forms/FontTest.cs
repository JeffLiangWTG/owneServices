using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace WinzorFramework.Test;

using static PlaywrightTestContext;
class FontTest
{
	[Test, WithPlaywrightPage]
	public async Task TahomaBoldShouldRenderAccurately()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Text = "Form Title" };
			var btnStaff = new Button { Text = "Staff", Font = new Font("Tahoma", 6F, FontStyle.Bold, GraphicsUnit.Point, 0, false) };

			form.Controls.Add(btnStaff);

			return form;
		});

		var btnStaffRendered = await page.WaitForSelectorAsync(".button div div");
		await btnStaffRendered.EvaluateAsync<string>("e => e.style.width = 'auto'");

		var actualWidth = await btnStaffRendered.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')");
		// actualWidth in testing reports 19.2969px - but 4 decimal places is too many to trust

		var roundedWidth = await btnStaffRendered.EvaluateAsync<string>($"Math.round(parseFloat('{actualWidth}') * 10) / 10");
		Assert.That(roundedWidth, Is.EqualTo("19.3"));
	}
}
