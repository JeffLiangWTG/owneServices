using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class DocumentVisualizerTest
{
	[Test]
	public async Task DocumentShouldBeRenderedAsControls()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new ZForm();
			var panel = DocumentVisualizer.Testing.GUI.TestPageViews.TestTemplate();
			form.Controls.Add(panel);
			return form;
		});

		var labelList = rendered.FindAll(".label__text");
		Assert.That(labelList, Has.Count.EqualTo(6));
		Assert.That(labelList.Select(m => m.InnerHtml).ToList(), Is.EqualTo(new List<string>() {
			"document header", "Default", "Default",
			"First section header", "I am a child", "First section footer"
		}));

		var pictureList = rendered.FindAll(".picturebox");
		Assert.That(pictureList, Has.Count.EqualTo(1));
		var lineList = rendered.FindAll("[data-type=\"System.Windows.Forms.Control\"]");
		Assert.That(lineList, Has.Count.EqualTo(14));
	}

	[Test]
	public async Task EditableIndicatorShouldShowTopRight()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new ZForm();
			var control = new ZLabel()
			{
				AutoSize = false,
				Text = "LABEL",
				Width = 200,
				Height = 200,
				BackColor = System.Drawing.Color.White,
			};
			var editableIndicator = new EditableIndicator(control);
			form.Controls.Add(control);
			form.Controls.Add(editableIndicator);
			return form;
		});

		var indicator = rendered.Find("[data-type=\"Enterprise.DocumentVisualizer.GUI.EditableIndicator\"]");
		var style = indicator.GetAttribute("style");
		Assert.That(style, Is.EqualTo("position:absolute;width:5px;height:5px;top:-5px;left:195px;background-color:#FFFFFFFF;"));
	}
}
