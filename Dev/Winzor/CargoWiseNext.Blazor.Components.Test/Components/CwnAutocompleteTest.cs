using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CargoWiseNext.Blazor.Components.Test.Components;

public class CwnAutocompleteTest : BunitTestContext
{
	[SetUp]
	public override void Setup()
	{
		base.Setup();
		JSInterop.SetupVoid("cwnAutocomplete.scrollElementIntoView", _ => true).SetVoidResult();
	}

	[Test]
	public void TestCwnAutocomplete_DoesShowPlainInputElement_WhenNofocus()
	{
		var cut = RenderComponent<CwnAutocomplete>();

		Assert.Multiple(() =>
		{
			var div = cut.Find("div.cwn-input.cwn-autocomplete__input");
			var input = div.QuerySelector("input.cwn-input__input");
			Assert.That(input!.GetAttribute("type"), Is.EqualTo("text"));
			Assert.That(input.GetAttribute("maxlength"), Is.EqualTo("250"));
		});
	}

	[Test]
	public void TestCwnAutocomplete_DoesDisplaySearchText_WhenSearchTextParameterSet()
	{
		var cut = RenderComponent<CwnAutocomplete>(parameter => parameter
			.Add(p => p.SearchText, "Test"));

		Assert.Multiple(() =>
		{
			var uut = cut.Find("input");
			Assert.That(uut.GetAttribute("value"), Is.EqualTo("Test"));
		});
	}

	[Test]
	public void TestCwnAutocomplete_DoesApplyStylingToInputElement_WhenStylingParametersSet()
	{
		var cut = RenderComponent<CwnAutocomplete>(parameters => parameters
			.Add(p => p.ClassInput, "custom-class")
			.Add(p => p.StyleInput, "color: blue;"));

		Assert.Multiple(() =>
		{
			var uut = cut.Find(".cwn-input");
			Assert.That(uut.GetAttribute("class"), Does.Contain("custom-class"));
			Assert.That(uut.GetAttribute("style"), Does.Contain("color: blue;"));
		});
	}

	[Test]
	public void TestCwnAutocomplete_DoesDisplayPlaceholderText_WhenPlaceholderParameterSet()
	{
		var cut = RenderComponent<CwnAutocomplete>(parameters => parameters
			.Add(p => p.Placeholder, "Test Placeholder..."));

		var uut = cut.Find("input");

		Assert.That(uut.GetAttribute("placeholder"), Is.EqualTo("Test Placeholder..."));
	}

	[Test] public void TestCwnAutocomplete_DoesApplyMaxInputLength_WhenMaxLengthParameterSet()
	{
		var cut = RenderComponent<CwnAutocomplete>(parameters => parameters
			.Add(p => p.MaxLength, 20));

		var uut = cut.Find("input");

		Assert.That(uut.GetAttribute("maxlength"), Is.EqualTo("20"));
	}

	[Test]
	public void TestCwnAutocomplete_DoesApplyIconsToElements_WhenIconParametersSet()
	{
		var cut = RenderComponent<CwnAutocomplete>(parameters => parameters
			.Add(p => p.StartIcon, Icon.ArrowRight)
			.Add(p => p.EndIcon, Icon.Search)
			.Add(p => p.IconSize, Size.XXLarge));

		Assert.Multiple(() =>
		{
			var icons = cut.FindAll(".cwn-icon");

			Assert.That(icons, Has.Count.EqualTo(2));
			Assert.That(icons[0].GetAttribute("class"), Does.Contain("cwn-icon--arrow-right"));
			Assert.That(icons[1].GetAttribute("class"), Does.Contain("cwn-icon--search"));

			Assert.That(cut.FindAll(".cwn-icon--2xlarge"), Has.Count.EqualTo(2));
		});
	}

	[Test]
	public void TestOnFocus_DoesShowNoResults_WhenNoItems()
	{
		var cut = RenderComponent<CwnAutocomplete>(parameters => parameters
			.Add(p => p.NoResultsText, "Empty list")
			.Add(p => p.Items, []));

		var input = cut.Find("input");
		input.Focus();

		Assert.Multiple(() =>
		{
			Assert.That(cut.Find(".cwn-autocomplete__no-results").TextContent, Is.EqualTo("Empty list"));
			Assert.That(cut.FindAll(".cwn-autocomplete__item"), Is.Empty);
		});
	}

	[Test]
	public void TestOnFocus_DoesShowItemsAndHighlightTheFirst_WhenFocus()
	{
		var selectedItemCallbackCount = 0;
		var cut = RenderComponent<CwnAutocomplete>(parameters => parameters
			.Add(p => p.Items, ["John Howard", "Julia Gillard", "Robert Menzies"])
			.Add(p => p.OnItemSelected, EventCallback.Factory.Create(this, (string _) => Interlocked.Increment(ref selectedItemCallbackCount))));

		var input = cut.Find("input");
		input.Focus();

		Assert.Multiple(() =>
		{
			Assert.That(cut.FindAll(".cwn-autocomplete__no-results"), Is.Empty);
			Assert.That(cut.FindAll(".cwn-autocomplete__list"), Has.Count.EqualTo(1));
			Assert.That(cut.Find(".cwn-list-item--selected").TextContent, Is.EqualTo("John Howard"));
			Assert.That(selectedItemCallbackCount, Is.Zero);
		});
	}

	[Test]
	public void TestOnFocus_DoesFilterAndHighlightTheFirstMatch_WhenSearchTextEntered()
	{
		var selectedItemCallbackCount = 0;
		string[] testItems = ["John Howard", "Julia Gillard", "Robert Menzies"];
		var cut = RenderComponent<CwnAutocomplete>(parameters => parameters
			.Add(p => p.OnItemSelected,
				EventCallback.Factory.Create(this,
					(string _) => Interlocked.Increment(ref selectedItemCallbackCount)))
			.Add(p => p.Items, testItems)
			.Add(p => p.SearchText, "ar"));

		var input = cut.Find("input");
		input.Focus();

		var uut = cut.FindAll(".cwn-autocomplete__item span");
		Assert.Multiple(() =>
		{
			Assert.That(uut, Has.Count.EqualTo(2));
			Assert.That(() => uut[0].MarkupMatches("<span>John How<strong>ar</strong>d</span>"), Throws.Nothing);
			Assert.That(() => uut[1].MarkupMatches("<span>Julia Gill<strong>ar</strong>d</span>"), Throws.Nothing);
			Assert.That(cut.Find(".cwn-list-item--selected").TextContent, Is.EqualTo(testItems[0]));
			Assert.That(selectedItemCallbackCount, Is.Zero);
		});
	}

	[Test]
	public void Test_DoesApplyStylingToListItems_WhenStylingParametersSet()
	{
		var cut = RenderComponent<CwnAutocomplete>(parameters => parameters
			.Add(p => p.Items, ["John Howard", "Julia Gillard", "Robert Menzies"])
			.Add(p => p.ClassList, "custom-class")
			.Add(p => p.StyleList, "color: blue;"));

		var input = cut.Find("input");
		input.Focus();

		var uut = cut.Find(".cwn-autocomplete__list");
		Assert.Multiple(() =>
		{
			Assert.That(uut.GetAttribute("class"), Does.Contain("custom-class"));
			Assert.That(uut.GetAttribute("style"), Does.Contain("color: blue;"));
		});
	}

	[Test] public void TestMouseDown_DoesDisplaySelectedValueAndPublishOnItemSelected()
	{
		string[] items = ["John Howard", "Julia Gillard", "Robert Menzies"];
		var selectedItem = string.Empty;
		var cut = RenderComponent<CwnAutocomplete>(parameters => parameters
			.Add(p => p.Items, items)
			.Add(p => p.OnItemSelected, EventCallback.Factory.Create(this, (string item) => selectedItem = item)));

		var input = cut.Find("input");

		Assert.Multiple(() =>
		{
			for (var itemIndexToClick = 0; itemIndexToClick < items.Length; itemIndexToClick++)
			{
				input.Focus();

				var expectedItem = items[itemIndexToClick];
				var autoCompleteItems = cut.FindAll(".cwn-autocomplete__item");
				autoCompleteItems[itemIndexToClick].MouseDown();

				Assert.That(cut.FindAll(".cwn-autocomplete__item"), Is.Empty);
				Assert.That(input.GetAttribute("value"), Is.EqualTo(expectedItem));
				Assert.That(selectedItem, Is.EqualTo(expectedItem));

				input.Input(string.Empty);
			}
		});
	}

	[TestCase("ArrowUp")]
	[TestCase("ArrowDown")]
	public void TestArrowKeyInput_DoesNothing_WhenNoItems(string key)
	{
		var selectedItemCallbackCount = 0;
		var cut = RenderComponent<CwnAutocomplete>(parameters => parameters
			.Add(p => p.Items, [])
			.Add(p => p.OnItemSelected, EventCallback.Factory.Create(this, (string _) => Interlocked.Increment(ref selectedItemCallbackCount))));

		var input = cut.Find("input");
		input.Focus();

		input.KeyDown(key);

		Assert.Multiple(() =>
		{
			Assert.That(cut.FindAll(".cwn-list-item--selected"), Is.Empty);
			Assert.That(selectedItemCallbackCount, Is.Zero);
		});
	}

	[Test]
	public void TestArrowDown_DoesScrollThroughItems()
	{
		var selectedItemCallbackCount = 0;
		string[] items = ["John Howard", "Julia Gillard", "Robert Menzies"];
		var cut = RenderComponent<CwnAutocomplete>(parameters => parameters
			.Add(p => p.Items, items)
			.Add(p => p.OnItemSelected, EventCallback.Factory.Create(this, (string _) => Interlocked.Increment(ref selectedItemCallbackCount))));

		var input = cut.Find("input");
		input.Focus();

		Assert.Multiple(() =>
		{
			var expectedItem = items[0];
			Assert.That(cut.Find(".cwn-list-item--selected").TextContent, Is.EqualTo(expectedItem));

			for (var pressNumber = 1; pressNumber <= items.Length; pressNumber++)
			{
				input.KeyDown("ArrowDown");
				var expectedIndex = pressNumber % items.Length;
				expectedItem = items[expectedIndex];

				Assert.That(cut.Find(".cwn-list-item--selected").TextContent, Is.EqualTo(expectedItem));
				Assert.That(input.GetAttribute("value"), Is.EqualTo(string.Empty));
				Assert.That(cut.FindAll(".cwn-autocomplete__item"), Has.Count.EqualTo(items.Length));
			}

			Assert.That(selectedItemCallbackCount, Is.Zero);
		});
	}

	[Test]
	public void TestArrowDownAndEnter_DoesSelectHighlightedItem()
	{
		string[] items = ["John Howard", "Julia Gillard", "Robert Menzies"];
		string? itemNotified = null;
		var cut = RenderComponent<CwnAutocomplete>(parameters => parameters
			.Add(p => p.Items, items)
			.Add(p => p.OnItemSelected, EventCallback.Factory.Create<string>(this, item => itemNotified = item)));

		var input = cut.Find("input");

		Assert.Multiple(() =>
		{
			for (var numberOfPresses = 1; numberOfPresses <= items.Length; numberOfPresses++)
			{
				input.Focus();
				for (var pressNumber = 1; pressNumber <= numberOfPresses; pressNumber++)
				{
					input.KeyDown("ArrowDown");
				}
				input.KeyDown("Enter");

				var expectedIndex = numberOfPresses % items.Length;
				var expectedItem = items[expectedIndex];

				Assert.That(itemNotified, Is.EqualTo(expectedItem));
				Assert.That(input.GetAttribute("value"), Is.EqualTo(expectedItem));
				Assert.That(cut.FindAll(".cwn-autocomplete__item"), Is.Empty);

				cut.SetParametersAndRender(parameters => parameters.Add(p => p.SearchText, string.Empty));
			}
		});
	}

	[Test]
	public void TestArrowUp_DoesScrollThroughItems()
	{
		var selectedItemCallbackCount = 0;
		string[] items = ["John Howard", "Julia Gillard", "Robert Menzies"];
		var cut = RenderComponent<CwnAutocomplete>(parameters => parameters
			.Add(p => p.Items, items)
			.Add(p => p.OnItemSelected, EventCallback.Factory.Create(this, (string _) => Interlocked.Increment(ref selectedItemCallbackCount))));

		var input = cut.Find("input");
		input.Focus();

		Assert.Multiple(() =>
		{
			var expectedItem = items[0];
			Assert.That(cut.Find(".cwn-list-item--selected").TextContent, Is.EqualTo(expectedItem));

			for (var pressNumber = 1; pressNumber <= items.Length; pressNumber++)
			{
				input.KeyDown("ArrowUp");

				var expectedIndex = (items.Length - pressNumber) % items.Length;
				expectedItem = items[expectedIndex];

				Assert.That(cut.Find(".cwn-list-item--selected").TextContent, Is.EqualTo(expectedItem));
				Assert.That(input.GetAttribute("value"), Is.EqualTo(string.Empty));
				Assert.That(cut.FindAll(".cwn-autocomplete__item"), Has.Count.EqualTo(items.Length));
			}

			Assert.That(selectedItemCallbackCount, Is.Zero);
		});
	}

	[Test]
	public void TestArrowUpAndEnter_DoesSelectHighlightedItem()
	{
		string[] items = ["John Howard", "Julia Gillard", "Robert Menzies"];
		string? itemNotified = null;
		var cut = RenderComponent<CwnAutocomplete>(parameters => parameters
			.Add(p => p.Items, items)
			.Add(p => p.OnItemSelected, EventCallback.Factory.Create<string>(this, item => itemNotified = item)));

		var input = cut.Find("input");

		Assert.Multiple(() =>
		{
			for (var numberOfPresses = 1; numberOfPresses <= items.Length; numberOfPresses++)
			{
				input.Focus();
				for (var pressNumber = 1; pressNumber <= numberOfPresses; pressNumber++)
				{
					input.KeyDown("ArrowUp");
				}
				input.KeyDown("Enter");

				var expectedIndex = (items.Length - numberOfPresses) % items.Length;
				var expectedItem = items[expectedIndex];

				Assert.That(itemNotified, Is.EqualTo(expectedItem));
				Assert.That(input.GetAttribute("value"), Is.EqualTo(expectedItem));
				Assert.That(cut.FindAll(".cwn-autocomplete__item"), Is.Empty);

				cut.SetParametersAndRender(parameters => parameters.Add(p => p.SearchText, string.Empty));
			}
		});
	}

	[Test]
	public void TestEscape_DoesNotSelectAnyItem()
	{
		var selectedItemCallbackCount = 0;
		string[] items = ["John Howard", "Julia Gillard", "Robert Menzies"];
		var cut = RenderComponent<CwnAutocomplete>(parameters => parameters
			.Add(p => p.Items, items)
			.Add(p => p.OnItemSelected, EventCallback.Factory.Create(this, (string _) => Interlocked.Increment(ref selectedItemCallbackCount))));

		var input = cut.Find("input");
		input.Focus();

		Assert.That(cut.FindAll(".cwn-autocomplete__list"), Has.Count.EqualTo(1));

		input.KeyDown("Escape");

		Assert.Multiple(() =>
		{
			Assert.That(cut.FindAll(".cwn-autocomplete__list"), Is.Empty);
			Assert.That(selectedItemCallbackCount, Is.Zero);
		});
	}

	[Test]
	public void TestIsVisibleChange_DoesPublishOnItemsVisibilityChanged()
	{
		var selectedItemCallbackCount = 0;
		var isVisible = false;
		var cut = RenderComponent<CwnAutocomplete>(parameters => parameters
			.Add(p => p.Items, ["John Howard", "Julia Gillard", "Robert Menzies"])
			.Add(p => p.OnItemsVisibilityChanged, EventCallback.Factory.Create(this, (bool visible) => isVisible = visible))
			.Add(p => p.OnItemSelected, EventCallback.Factory.Create(this, (string _) => Interlocked.Increment(ref selectedItemCallbackCount))));

		var input = cut.Find("input");
		input.Focus();
		Assert.That(isVisible, Is.True);

		input.KeyDown("Escape");

		Assert.Multiple(() =>
		{
			Assert.That(isVisible, Is.False);
			Assert.That(selectedItemCallbackCount, Is.Zero);
		});
	}

	[Test]
	public void Test_DoesScrollHighlightedItemIntoView()
	{
		var items = Enumerable.Range(1, 50).Select(i => $"Item {i}").ToArray();
		var cut = RenderComponent<CwnAutocomplete>(parameters => parameters
			.Add(p => p.Items, items));

		var input = cut.Find("input");
		input.Focus();

		for (int i = 0; i < items.Length - 1; i++)
		{
			input.KeyDown("ArrowDown");
		}

		JSInterop.VerifyInvoke("cwnAutocomplete.scrollElementIntoView", calledTimes: items.Length);
	}

	[Test]
	public void TestCwnAutocomplete_DoesHighlightFirstItem_WhenConditionallyRendered()
	{
		var items = new[] { "John Howard", "Julia Gillard", "Robert Menzies" };
		var selectedItemCallbackCount = 0;
		var cut = RenderComponent<ConditionalParent>(parameters => parameters
			.Add(p => p.Items, items)
			.Add(p => p.Visible, false)
			.Add(p => p.OnItemSelected, EventCallback.Factory.Create(this, (string _) => Interlocked.Increment(ref selectedItemCallbackCount))));
		Assert.That(cut.FindAll("input"), Is.Empty);

		cut.SetParametersAndRender(p => p.Add(x => x.Visible, true));
		var input = cut.Find("input");
		input.Focus();

		Assert.Multiple(() =>
		{
			Assert.That(cut.Find(".cwn-list-item--selected").TextContent, Is.EqualTo(items[0]));
			Assert.That(selectedItemCallbackCount, Is.Zero);
		});
	}

	class ConditionalParent : ComponentBase
	{
		[Parameter] public bool Visible { get; set; }
		[Parameter] public string[] Items { get; set; } = [];
		[Parameter] public EventCallback<string> OnItemSelected { get; set; }

		protected override void BuildRenderTree(RenderTreeBuilder builder)
		{
			if (Visible)
			{
				builder.OpenComponent(0, typeof(CwnAutocomplete));
				builder.AddAttribute(1, "Items", Items);
				builder.AddAttribute(2, "OnItemSelected", OnItemSelected);
				builder.CloseComponent();
			}
		}
	}
}
