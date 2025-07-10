using System.Drawing;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.MasterFiles.GUI;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using static WTG.PlaywrightTesting.PlaywrightTestContext;

namespace Enterprise.Winzor.Architecture.Test;

class ZDocAddressControlTest
{
	[Test, WithPlaywrightPage]
	public async Task DefaultGroupBoxShouldCoverConvertToOrganizationButton()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Size = new Size(600, 400) };
			var control = new ZDocAddressControl();
			form.Controls.Add(control);
			return form;
		});

		var defaultGroupBox = page.Locator("[data-name='DefaultGroupBox']");
		var convertToOrganizationButton = page.GetByTitle("Convert To Organization");

		await defaultGroupBox.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
		await convertToOrganizationButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

		var defaultGroupBoxBox = await defaultGroupBox.BoundingBoxAsync();
		var convertToOrganizationButtonBox = await convertToOrganizationButton.BoundingBoxAsync();

		Assert.That(defaultGroupBoxBox, Is.Not.Null, "Failed to retrieve bounding box for DefaultGroupBox.");
		Assert.That(convertToOrganizationButtonBox, Is.Not.Null, "Failed to retrieve bounding box for ConvertToOrganizationButton.");

		var defaultGroupBoxIndex = await defaultGroupBox.EvaluateAsync<int>("e => e.style.zIndex");
		var convertToOrganizationButtonIndex = await convertToOrganizationButton.EvaluateAsync<int>("e => e.style.zIndex");

		Assert.That(defaultGroupBoxIndex, Is.Not.EqualTo(-1));
		Assert.That(convertToOrganizationButtonIndex, Is.Not.EqualTo(-1));
		Assert.That(defaultGroupBoxIndex, Is.GreaterThan(convertToOrganizationButtonIndex));

		bool isCovering = defaultGroupBoxBox.X <= convertToOrganizationButtonBox.X &&
						  defaultGroupBoxBox.Y <= convertToOrganizationButtonBox.Y &&
						  defaultGroupBoxBox.X + defaultGroupBoxBox.Width >= convertToOrganizationButtonBox.X + convertToOrganizationButtonBox.Width &&
						  defaultGroupBoxBox.Y + defaultGroupBoxBox.Height >= convertToOrganizationButtonBox.Y + convertToOrganizationButtonBox.Height;

		Assert.That(isCovering, Is.True, "DefaultGroupBox does not fully cover ConvertToOrganizationButton.");
	}
}
