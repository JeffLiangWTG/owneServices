using AngleSharp.Css.Dom;
using CargoWise.NetworkVisualisation.GUI.Components;
using Enterprise.Winzor.Architecture.Test;
using WTG.PlaywrightTesting;

namespace NetworkVisualisation.GUI.Winzor.Test.Components;

public class DynamicTextAreaTest : BunitTestContext
{
	[Test]
	public void TextAreaParametersAssignedCorrectly()
	{
		var component = RenderComponent<DynamicTextArea>(parameters =>
		{
			parameters.Add(x => x.Class, "test-class")
				.Add(x => x.Style, "color: cyan;")
				.Add(x => x.Placeholder, "test-placeholder")
				.Add(x => x.Value, "test-value")
				.Add(x => x.AriaLabel, "test-arialabel")
				.Add(x => x.Readonly, true)
				.Add(x => x.Maxlength, 500);
		});

		var textAreaWrapper = component.Find(".textarea__resizable-wrapper");
		var textArea = component.Find(".textarea__resizable");

		Assert.That(textAreaWrapper.ClassList, Does.Contain("test-class"));
		Assert.That(textAreaWrapper.GetStyle().GetColor(), Does.Contain("rgba(0, 255, 255, 1)"));
		Assert.That(textAreaWrapper.GetAttribute("data-value"), Is.EqualTo("test-value"));

		Assert.That(textArea.GetAttribute("value"), Is.EqualTo("test-value"));
		Assert.That(textArea.GetAttribute("placeholder"), Is.EqualTo("test-placeholder"));
		Assert.That(textArea.GetAttribute("aria-label"), Is.EqualTo("test-arialabel"));
		Assert.That(textArea.GetAttribute("readonly"), Is.Not.Null);
		Assert.That(textArea.GetAttribute("maxlength"), Is.EqualTo("500"));
	}

	[Test]
	public async Task TextAreaReadOnlyDoesNotUpdateValueAsync()
	{
		var counter = 0;
		var component = RenderComponent<DynamicTextArea>(parameters =>
		{
			parameters.Add(x => x.Class, "test-class")
				.Add(x => x.Value, "test-value")
				.Add(x => x.Readonly, true)
				.Add(x => x.ValueChanged, (_) => counter++);
		});

		var textAreaWrapper = component.Find(".textarea__resizable-wrapper");
		var textArea = component.Find(".textarea__resizable");

		await textArea.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = "new-text" });
		Assert.That(counter, Is.EqualTo(0));
	}

	[Test]
	public async Task TextAreaMaxlengthDoesNotSetValueLongerAsync()
	{
		var counter = 0;
		var component = RenderComponent<DynamicTextArea>(parameters =>
		{
			parameters.Add(x => x.Class, "test-class")
				.Add(x => x.Value, "abc")
				.Add(x => x.Maxlength, 3)
				.Add(x => x.ValueChanged, (_) => counter++);
		});

		var textAreaWrapper = component.Find(".textarea__resizable-wrapper");
		var textArea = component.Find(".textarea__resizable");

		await textArea.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = "new-text" });
		Assert.That(counter, Is.EqualTo(0));
	}

	[Test, WithPlaywrightPage]
	public async Task TextAreaResizedAccordingToContentOnInputAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var (page, component) = await ctx.LoadComponentOnFormAsync<DynamicTextArea>(parameters =>
			parameters
				.Add(x => x.Style, "width: 50px;")
		);

		var textArea = await page.WaitForSelectorAsync(".textarea__resizable");
		Assert.That(textArea, Is.Not.Null);

		Assert.That(async () => await textArea!.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('height')"), Is.EqualTo("13.4375px"));
		for (var i = 0; i < 50; i++)
		{
			await textArea!.PressAsync("a");
		}
		Assert.That(async () => await textArea!.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('height')"), Is.EqualTo("94.0625px"));

		for (var i = 0; i < 25; i++)
		{
			await textArea!.PressAsync("Backspace");
		}
		Assert.That(async () => await textArea!.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('height')"), Is.EqualTo("53.75px"));

		for (var i = 0; i < 25; i++)
		{
			await textArea!.PressAsync("Backspace");
		}
		Assert.That(async () => await textArea!.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('height')"), Is.EqualTo("13.4375px"));
	}

	[Test, WithPlaywrightPage]
	public async Task TextAreaSizeCalculatedCorrectlyOnRenderAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var (page, component) = await ctx.LoadComponentOnFormAsync<DynamicTextArea>(parameters =>
			parameters
				.Add(x => x.Style, "width: 50px;")
				.Add(x => x.Value, "aaaaaaaaaaaaaaaaaaaaaaaaa")
		);

		var textArea = await page.WaitForSelectorAsync(".textarea__resizable");
		Assert.That(textArea, Is.Not.Null);

		Assert.That(async () => await textArea!.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('height')"), Is.EqualTo("53.75px"));
	}

	[Test, WithPlaywrightPage]
	public async Task TextAreaSizeCalculatedCorrectlyOnValueChangeFromServerAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var (page, component) = await ctx.LoadComponentOnFormAsync<DynamicTextArea>(parameters =>
			parameters
				.Add(x => x.Style, "width: 50px;")
		);

		var textArea = await page.WaitForSelectorAsync(".textarea__resizable");
		Assert.That(textArea, Is.Not.Null);

		Assert.That(async () => await textArea!.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('height')"), Is.EqualTo("13.4375px"));

		await component.UpdateParametersAsync(parameters =>
			parameters
				.Add(x => x.Value, "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")
		);

		Assert.That(async () => await textArea!.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('height')"), Is.EqualTo("94.0625px"));

		await component.UpdateParametersAsync(parameters =>
			parameters
				.Add(x => x.Value, "aaaaaaaaaaaaaaaaaaaaaaaaa")
		);

		Assert.That(async () => await textArea!.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('height')"), Is.EqualTo("53.75px"));
	}

	[Test, WithPlaywrightPage]
	public async Task TextAreaHeightAdjustedOnWidthChangeAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var (page, component) = await ctx.LoadComponentOnFormAsync<DynamicTextArea>(parameters =>
			parameters
				.Add(x => x.Style, "width: 50px;")
				.Add(x => x.Value, "aaaaaaaaaaaaaaaaaaaaaaaaa")
		);

		var textArea = await page.WaitForSelectorAsync(".textarea__resizable");
		Assert.That(textArea, Is.Not.Null);

		Assert.That(async () => await textArea!.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('height')"), Is.EqualTo("53.75px"));

		await component.UpdateParametersAsync(parameters =>
			parameters
				.Add(x => x.Style, "width: 25px;")
		);

		Assert.That(async () => await textArea!.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('height')"), Is.EqualTo("94.0625px"));

		await component.UpdateParametersAsync(parameters =>
			parameters
				.Add(x => x.Style, "width: 150px;")
		);

		Assert.That(async () => await textArea!.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('height')"), Is.EqualTo("13.4375px"));
	}
}
