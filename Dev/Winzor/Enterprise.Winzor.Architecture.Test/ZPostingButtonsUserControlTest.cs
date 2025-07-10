using System.Threading.Tasks;
using Bunit;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

public class ZPostingButtonsUserControlTest
{
	class TestingFormNotShowStatusBar : ZChildForm
	{
		protected override bool ShowStatusBar
		{
			get { return false; }
		}
	}

	[TestCase(325, 25, 29, TestName = "{m}_WithHieght")]
	public async Task ZPostingButtonsShouldSetOnTopOfStatusBar(int formHeight, int statusBarHeight, int postingButtonsHeight)
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() => {
			var zform = new ZForm();
			zform.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, formHeight, true);
			zform.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(635, statusBarHeight, true);

			var zPostingButtonsUserControl = new ZPostingButtonsUserControl();
			zPostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, postingButtonsHeight, true);

			zform.Controls.Add(zPostingButtonsUserControl);

			return zform;
		});

		var positionButtonsTop = formHeight - statusBarHeight - postingButtonsHeight;
		var postingButtons = rendered.Find("div:has(> .toolstrip)");
		Assert.That(postingButtons.GetAttribute("style"), Is.EqualTo($"position:absolute;width:300px;height:29px;top:{positionButtonsTop}px;left:0px;background-color:#DCE1E4FF;overflow:hidden;"));
	}

	[Test]
	public async Task ZPostingButtonsShouldNotOverwriteTopPosition()
	{
		var formHeight = 325;
		var statusBarHeight = 25;
		var postingButtonsHeight = 29;
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() => {
			var zform = new TestingFormNotShowStatusBar();
			zform.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, formHeight, true);

			var zPostingButtonsUserControl = new ZPostingButtonsUserControl();
			zPostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, postingButtonsHeight, true);

			zform.Controls.Add(zPostingButtonsUserControl);

			return zform;
		});

		var expectedButtonsTop = 0;
		var positionButtonsTop = formHeight - statusBarHeight - postingButtonsHeight;
		var postingButtons = rendered.Find("div:has(> .toolstrip)");
		Assert.That(postingButtons.GetAttribute("style"), Is.EqualTo($"position:absolute;width:300px;height:29px;top:{expectedButtonsTop}px;left:0px;background-color:#DCE1E4FF;overflow:hidden;"));
	}
}
