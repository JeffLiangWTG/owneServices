using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace WinzorFramework.Enums;

class ElementEventHandlersTest
{
	[Test]
	public void ServerSideEventHandlersSizeIsLimited()
	{
		var eventHandlersSize = Enum.GetNames(typeof(ElementEventHandlers)).Length;
		Assert.That(eventHandlersSize, Is.LessThanOrEqualTo(32));
	}

	[Test, WithPlaywrightPage]
	public async Task ClientSideEventHandlersSizeIsLimited()
	{
		await using var ctx = new InMemoryTestServerContext();
		TextBox textBox = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(textBox = new TextBox());
			return form;
		});

		await page.WaitForLoadStateAsync();

		var eventHandlersSize = await page.EvaluateAsync<int>("Object.keys(window.elementEventHandlerTokens).length");
		Assert.That(eventHandlersSize, Is.LessThanOrEqualTo(32));
	}

	[Test, WithPlaywrightPage]
	public async Task EventHandlersShouldBeTheSame()
	{
		await using var ctx = new InMemoryTestServerContext();
		TextBox textBox = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(textBox = new TextBox());
			return form;
		});

		await page.WaitForLoadStateAsync();

		var clientSideHandlersJson = await page.EvaluateAsync("window.elementEventHandlerTokens");
		var clientSideHandlersDict = new Dictionary<string, int>();
		foreach (JsonProperty property in clientSideHandlersJson.Value.EnumerateObject())
		{
			if (property.Name != "$id")
			{
				clientSideHandlersDict[property.Name.ToLowerInvariant()] = property.Value.GetInt32();
			}
		}

		var clientSideHandlersSize = clientSideHandlersDict.Count;
		var serverSideHandlersSize = Enum.GetNames(typeof(ElementEventHandlers)).Length;
		Assert.That(clientSideHandlersSize, Is.EqualTo(serverSideHandlersSize));

		var serverSideEnumDict = Enum.GetValues(typeof(ElementEventHandlers))
							 .Cast<ElementEventHandlers>()
							 .ToDictionary(k => Enum.GetName(typeof(ElementEventHandlers), k).ToLowerInvariant(), v => (int)v);
		var allKVsAreMatched = !clientSideHandlersDict.Except(serverSideEnumDict).Any();
		Assert.That(allKVsAreMatched, Is.True);
	}

	[Test]
	public void TestEventHandlersIntegrity()
	{
		var expectedEnumValues = new Dictionary<string, int>
		{
			{ "None", 0 },
			{ "TextBoxDisableHomeEndKeyWhenSelectAll", 1 << 0 },
			{ "TextBoxStartTyping", 1 << 1 },
			{ "GridUpdateTextAreaHeightWhenFocusin", 1 << 2 }
		};

		var actualEnumValues = Enum.GetValues(typeof(ElementEventHandlers))
			.Cast<ElementEventHandlers>()
			.ToDictionary(k => Enum.GetName(typeof(ElementEventHandlers), k), v => (int)v);

		Assert.That(actualEnumValues, Is.EquivalentTo(expectedEnumValues));
	}
}
