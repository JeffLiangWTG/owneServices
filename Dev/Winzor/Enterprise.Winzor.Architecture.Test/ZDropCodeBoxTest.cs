using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Bunit;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using NUnit.Framework;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace Enterprise.Winzor.Architecture.Test;

public class ZDropCodeBoxTest
{
	[Test]
	public async Task TestLoadSuggestionsTrace()
	{
		var traces = new List<Activity>();
		using var tracerProvider = Sdk.CreateTracerProviderBuilder()
		.AddSource("WinzorFramework")
		.AddInMemoryExporter(traces)
		.Build();

		using var ctx = new EnterpriseTestContext();
		await ctx.RenderFormAsync(() =>
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(Guid.NewGuid(), $"Number_0", $"Description_0");
			var dropEdit = new ZDropEdit { List = list };

			var form = new WinzorTestForm();

			var codeBox = new ZDropCodeBoxWithExposed();
			codeBox.Parent = dropEdit;
			codeBox.PerformLoadSuggestions();
			form.Controls.Add(dropEdit);
			return form;
		});

		Assert.That(traces.Count, Is.GreaterThanOrEqualTo(1));
		Assert.That(traces.Any(trace => trace.DisplayName == "ZDropCodeBox.LoadSuggestions"), Is.True);

		Assert.That(traces.Any(trace => trace.TagObjects.Any(tag => tag.Key == "DropEditName" && tag.Value.ToString() == "ZDropEdit")));
		Assert.That(traces.Any(trace => trace.TagObjects.Any(tag => tag.Key == "ListCount" && tag.Value.ToString() == "1")));
	}

	[Test]
	public async Task TestOnInputChangedTrace()
	{
		var traces = new List<Activity>();
		using var tracerProvider = Sdk.CreateTracerProviderBuilder()
		.AddSource("WinzorFramework")
		.AddInMemoryExporter(traces)
		.Build();

		using var ctx = new EnterpriseTestContext();
		await ctx.RenderFormAsync(() =>
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(Guid.NewGuid(), $"Number_0", $"Description_0");
			var dropEdit = new ZDropEdit { List = list };

			var form = new WinzorTestForm();

			var codeBox = new ZDropCodeBoxWithExposed();
			codeBox.Parent = dropEdit;
			codeBox.PerformOnInputChanged("test");
			form.Controls.Add(dropEdit);
			return form;
		});

		Assert.That(traces.Count, Is.GreaterThanOrEqualTo(1));
		Assert.That(traces.Any(trace => trace.DisplayName == "ZDropCodeBox.OnInputChanged"), Is.True);
	}

	[Test]
	public async Task ResetSuggestionsToNull()
	{
		using var ctx = new EnterpriseTestContext();
		ZDropCodeBoxWithExposed codeBox = null;
		var list = new CodeDescriptionPairList();

		list.AddPair(Guid.NewGuid(), $"Number_0", $"Description_0");

		await ctx.RenderControlOnFormAsync(() =>
		{
			var dropEdit = new ZDropEdit { List = list };

			codeBox = new ZDropCodeBoxWithExposed();
			codeBox.Parent = dropEdit;

			return dropEdit;
		});

		Assert.That(codeBox.LoadedSuggestions.First(), Is.EqualTo("Number_0"));

		codeBox.ResetSuggestion();

		Assert.That(codeBox.LoadedSuggestions, Is.Null);
	}

	[Test]
	public async Task ChangeListShouldRefreshDropCodeSuggestions()
	{
		using var ctx = new EnterpriseTestContext();
		var list = new CodeDescriptionPairList();
		var list2 = new CodeDescriptionPairList();

		list.AddPair(Guid.NewGuid(), $"Number_1", $"Description_1");
		list2.AddPair(Guid.NewGuid(), $"Number_2", $"Description_2");

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var dropEdit = new ZDropEdit { List = list };
			dropEdit.CodeBox.Click += (s, e) => dropEdit.List = list2;
			return dropEdit;
		});

		Assert.That(rendered.FindAll("option").Single().Attributes["value"].Value, Is.EqualTo("Number_1"));

		await rendered.Find(".textbox").ClickAsync(new WebMouseEventArgs());

		Assert.That(rendered.FindAll("option").Single().Attributes["value"].Value, Is.EqualTo("Number_2"));
	}

	class ZDropCodeBoxWithExposed : ZDropCodeBox
	{
		public void PerformOnInputChanged(string value)
		{
			OnInputChanged(value);
		}

		public void PerformLoadSuggestions()
		{
			LoadSuggestions();
		}

		public List<string> LoadedSuggestions => Suggestions;
	}
}
