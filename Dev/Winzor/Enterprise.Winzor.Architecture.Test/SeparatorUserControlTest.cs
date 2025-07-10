using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngleSharp.Html.Dom;
using Bunit;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Balloons;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using static WTG.PlaywrightTesting.PlaywrightTestContext;
using Res = CargoWiseOne.ResourceStrings.Res;

namespace Enterprise.Winzor.Architecture.Test;

class SeparatorUserControlTest
{
	[Test, WithPlaywrightPage]
	public async Task SeparatorWithLinesCheck()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var separator = new SeparatorUserControl();
			separator.CaptionResourceString = Res.GetData("4d319211-496e-42b5-8c25-7bcc9f19fcbb", "Hello world");
			form.Controls.Add(separator);
			return form;
		});

		var separatorLine = page.Locator(".separator .separator__line").First;
		var separatorSpan = page.Locator(".separator span");
		var textContent = await separatorSpan.InnerTextAsync();
		var lineHeight = (await separatorLine.BoundingBoxAsync()).Height;
		Assert.That(lineHeight,Is.EqualTo(2.0));
		Assert.That(textContent, Is.EqualTo("Hello world"));
	}
}

