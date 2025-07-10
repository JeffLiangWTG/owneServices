using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace CargoWiseNext.Blazor.Components
{
	public partial class CwnAutocomplete
	{
		int? selectedIndex;
		bool showList;
		readonly Dictionary<int, string> listItemIds = new();
		int? previousSelectedIndex;

		[Inject] public IJSRuntime? JsRuntime { get; set; }

		[Parameter] public IReadOnlyCollection<string> Items { get; set; } = [];
		[Parameter] public string? Placeholder { get; set; }
		[Parameter] public string? NoResultsText { get; set; }
		[Parameter] public Icon? StartIcon { get; set; }
		[Parameter] public Icon? EndIcon { get; set; }
		[Parameter] public Size IconSize { get; set; }
		[Parameter] public int MaxLength { get; set; } = 250;
		[Parameter] public bool Disabled { get; set; }
		[Parameter] public string? ClassInput { get; set; }
		[Parameter] public string? StyleInput { get; set; }
		[Parameter] public string? ClassList { get; set; }
		[Parameter] public string? StyleList { get; set; }
		[Parameter] public string SearchText { get; set; } = string.Empty;

		[Parameter] public EventCallback<string> OnItemSelected { get; set; }
		[Parameter] public EventCallback<bool> OnItemsVisibilityChanged { get; set; }

		string ClassnameInput => new CssBuilder()
			.AddClass("cwn-autocomplete__input")
			.AddClass(ClassInput)
			.Build();

		string? StylenameInput => new StyleBuilder()
			.AddStyle(StyleInput)
			.Build();

		string ClassnameList => new CssBuilder()
			.AddClass("cwn-autocomplete__list")
			.AddClass("cwn-scrollbar")
			.AddClass(ClassList)
			.Build();

		string? StylenameList => new StyleBuilder()
			.AddStyle(StyleList)
			.Build();

		bool ShowList
		{
			get => showList;
			set
			{
				if (showList != value)
				{
					showList = value;
					if (showList)
					{
						selectedIndex = showList && FilteredItems.Any() ? 0 : null;
						previousSelectedIndex = null;
					}
					OnItemsVisibilityChanged.InvokeAsync(value);
				}
			}
		}

		IEnumerable<string> FilteredItems => Items
			.Where(i => string.IsNullOrEmpty(i) is false)
			.Where(i => string.IsNullOrEmpty(SearchText) || i.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

		void HandleOnFocus() => ShowList = true;

		void OnFocusOut()
		{
			ShowList = false;
		}

		async Task ChooseItemAsync(int? index)
		{
			selectedIndex = index;
			if (SelectedIndexIsValid(selectedIndex, FilteredItems))
			{
				SearchText = FilteredItems.ElementAt(selectedIndex!.Value);
				await OnItemSelected.InvokeAsync(SearchText);
			}

			ShowList = false;
		}

		static bool SelectedIndexIsValid(int? selectedIndex, IEnumerable<string> filteredItems)
		{
			if (selectedIndex is null)
			{
				return false;
			}

			var filteredItemsCount = filteredItems.Count();
			return 0 <= selectedIndex && selectedIndex < filteredItemsCount;
		}

		static MarkupString HighlightText(string? text, string searchText)
		{
			if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(searchText))
			{
				return new MarkupString(text ?? string.Empty);
			}

			var newText = text.Replace(searchText, $"<strong>{searchText}</strong>", StringComparison.OrdinalIgnoreCase);

			return (MarkupString)$"<span>{newText}</span>";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage(category: "CargoWiseOne", checkId: "CW1161:Res.GetString Analyzer", Justification = "Key values are fixed, no need to localize.")]
		async Task HandleOnKeyDownAsync(KeyboardEventArgs e)
		{
			switch (e.Key)
			{
				case "Enter":
					await ChooseItemAsync(selectedIndex);
					break;
				case "Escape":
					ShowList = false;
					break;
				case "ArrowDown":
					NavigateList(1);
					break;
				case "ArrowUp":
					NavigateList(-1);
					break;
				case "Tab":
					break;
				default:
					if (ShowList is false)
					{
						ShowList = true;
					}
					break;
			}
		}

		void NavigateList(int direction)
		{
			var count = FilteredItems.Count();
			if (count == 0)
			{
				selectedIndex = null;
			}
			else
			{
				selectedIndex = (selectedIndex + direction + count) % count;
				StateHasChanged();
			}
		}

		string GetOrCreateListItemId(int index)
		{
			if (!listItemIds.TryGetValue(index, out var id))
			{
				id = $"cwn-autocomplete-item-{Guid.NewGuid()}";
				listItemIds[index] = id;
			}
			return id;
		}

		protected override async Task OnParametersSetAsync()
		{
			await base.OnParametersSetAsync();

			selectedIndex ??= FilteredItems.Any() ? 0 : null;
			previousSelectedIndex = null;
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			await base.OnAfterRenderAsync(firstRender);

			selectedIndex ??= FilteredItems.Any() ? 0 : null;
			await ScrollListAsync();
		}

		async Task ScrollListAsync()
		{
			if (ShowList && JsRuntime is not null && SelectedIndexIsValid(selectedIndex, FilteredItems) && selectedIndex != previousSelectedIndex)
			{
				if (listItemIds.TryGetValue(selectedIndex!.Value, out var id))
				{
					await JsRuntime.InvokeVoidAsync("cwnAutocomplete.scrollElementIntoView", id);
				}
				previousSelectedIndex = selectedIndex;
			}
		}
	}
}
