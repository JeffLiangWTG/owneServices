using System.Drawing;
using System.Threading.Tasks;
using Bunit;
using Enterprise.BufferManagement.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WinzorFramework;

namespace Enterprise.Winzor.Architecture.Test;

class DirectionalLabelTest
{
	[Test]
	public async Task LabelDisplayVertical()
	{
		using var ctx = new EnterpriseTestContext();
		ZForm form = null;
		var rendered = await ctx.RenderFormAsync(() => 
		{
			form = new ZForm();
			form.Size = new Size(300, 300);
			var verticalLabel = new DirectionalLabel(true);
			verticalLabel.Text = "this is a vertical";
			var horizontalLabel = new DirectionalLabel(false);
			horizontalLabel.Text = "this is a horizontal";
			form.Controls.Add(verticalLabel);
			form.Controls.Add(horizontalLabel);
			return form;
		}, OpenFormAction.BlockUntilShown);

		var directionalLabel = rendered.FindAll(".label__text");

		Assert.That(directionalLabel[0].GetAttribute("style"), Does.Contain("transform: rotate(180deg);writing-mode: vertical"));

		Assert.That(directionalLabel[1].GetAttribute("style"), Does.Not.Contain("transform: rotate(180deg);writing-mode: vertical"));
	}

	[Test]
	public async Task LabelDoesNotWrap()
	{
		using var ctx = new EnterpriseTestContext();
		ZForm form = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new ZForm();
			form.Size = new Size(300, 300);
			var horizontalLabel = new DirectionalLabel(false);
			horizontalLabel.Text = "this is a horizontal";
			form.Controls.Add(horizontalLabel);
			return form;
		}, OpenFormAction.BlockUntilShown);

		var directionalLabel = rendered.FindAll(".label__text");

		Assert.That(directionalLabel[0].GetAttribute("style"), Does.Contain("white-space: nowrap"));
	}

	[Test]
	public async Task LabelInheritsCssStyles()
	{
		using var ctx = new EnterpriseTestContext();
		ZForm form = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new ZForm();
			form.Size = new Size(300, 300);
			var verticalLabel = new DirectionalLabel(true);
			verticalLabel.Text = "this is a vertical label with default text align";

			var verticalLabelLeftText = new DirectionalLabel(true);
			verticalLabelLeftText.Text = "this is a vertical left text";
			verticalLabelLeftText.TextAlign = ContentAlignment.TopLeft;

			var verticalLabelCenterText = new DirectionalLabel(true);
			verticalLabelCenterText.Text = "this is a vertical center text";
			verticalLabelCenterText.TextAlign = ContentAlignment.MiddleCenter;

			form.Controls.Add(verticalLabel);
			form.Controls.Add(verticalLabelLeftText);
			form.Controls.Add(verticalLabelCenterText);
			return form;
		}, OpenFormAction.BlockUntilShown);

		var directionalLabel = rendered.FindAll(".label__text");

		Assert.That(directionalLabel[0].GetAttribute("style"), Does.Contain("text-align: center; top: 0; right: 0; height: 90%;"));
		Assert.That(directionalLabel[1].GetAttribute("style"), Does.Contain("text-align: left; top: 0; right: 0; height: 90%;"));
		Assert.That(directionalLabel[2].GetAttribute("style"), Does.Contain("text-align: center; top: 0; right: 0; height: 90%;"));
	}
}
