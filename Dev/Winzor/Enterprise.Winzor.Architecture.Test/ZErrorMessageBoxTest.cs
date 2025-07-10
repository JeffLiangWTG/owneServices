using System;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;

internal class ZErrorMessageBoxTest
{
	[Test, WithPlaywrightPage]
	public async Task ErrorMessageBoxNotOverflow()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			ZErrorMessageBox errorMessageBox = new ZErrorMessageBox(dummyBizo);

			return errorMessageBox;
		});

		var zErrorMessageBox = await page.WaitForSelectorAsync(".form");
		Assert.That(await zErrorMessageBox.EvaluateAsync<bool>("e => e.scrollHeight > e.clientHeight || e.scrollWidth > e.clientWidth"), Is.False);
	}
}
