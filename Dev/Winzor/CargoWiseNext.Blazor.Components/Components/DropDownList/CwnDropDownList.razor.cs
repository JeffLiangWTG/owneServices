using CargoWiseNext.Blazor.Components.JsInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CargoWiseNext.Blazor.Components;

public partial class CwnDropDownList : CwnComponentBase
{
	bool isInitialized;
	bool isDropDownListElementReferenceCaptured;
	bool isInputElementReferenceCaptured;

	[Inject]
	IDropDownListInterop? DropDownListInterop { get; set; }

	[Parameter]
	public bool IsDropdownVisible { get; set; }

	[Parameter]
	public int MaximumInputLength { get; set; } = -1;

	[Parameter]
	public SortedList<string, string> Items { get; set; } = [];

	[Parameter]
	public EventCallback<WebKeyboardEventArgs> OnKeyPress { get; set; }

	[Parameter]
	public EventCallback<ChangeEventArgs?> OnChange { get; set; }

	[Parameter]
	public EventCallback<string?> OnDropDownToggle { get; set; }

	[Parameter]
	public EventCallback OnFocusIn { get; set; }

	[Parameter]
	public string? SelectedCode { get; set; }

	readonly Dictionary<string, ElementReference> ItemsElementReferencesDict = new();

	ElementReference dropDownListElementReference;
	ElementReference DropDownListElementReference
	{
		set
		{
			dropDownListElementReference = value;
			isDropDownListElementReferenceCaptured = !value.Equals(default(ElementReference));
		}
	}

	ElementReference inputElementReference;
	ElementReference InputElementReference
	{
		get => inputElementReference;
		set
		{
			inputElementReference = value;
			isInputElementReferenceCaptured = !value.Equals(default(ElementReference));
		}
	}

	int SelectedIndex => Items?.Keys.ToList().IndexOf(SelectedCode ?? string.Empty) ?? -1;

	protected string Classname => new CssBuilder()
		.AddClass("cwn-dropdownlist")
		.AddClass(Class)
		.Build();

	protected string? Stylename => new StyleBuilder()
		.AddStyle(Style)
		.Build();

	string DropdownListClass => IsDropdownVisible
		? $"cwn-dropdownlist__menu"
		: $"cwn-dropdownlist__menu cwn-dropdownlist__menu--hidden";

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (isDropDownListElementReferenceCaptured && isInputElementReferenceCaptured
			&& DropDownListInterop != null && !isInitialized)
		{
			isInitialized = true;
			await DropDownListInterop.Initialize(
				SelectedCode ?? string.Empty, dropDownListElementReference, inputElementReference, this);
		}

		if (IsDropdownVisible)
		{
			await ScrollToAsync(SelectedIndex);
		}

		await base.OnAfterRenderAsync(firstRender);
	}

	[JSInvokable]
	public async Task SelectItemAsync(string itemKey)
	{
		SelectedCode = itemKey;
		await NotifyChangeAsync(new ChangeEventArgs { Value = itemKey });
	}

	public async ValueTask FocusAsync()
	{
		if (isInputElementReferenceCaptured)
		{
			await inputElementReference.FocusAsync();
		}
	}

	async Task OnFocusInAsync()
	{
		if (OnFocusIn.HasDelegate)
		{
			await OnFocusIn.InvokeAsync();
		}
	}

	async Task OnHandleButtonClickAsync(string itemKey)
	{
		if (string.IsNullOrEmpty(itemKey))
		{
			return;
		}

		if (IsDropdownVisible)
		{
			await OnToggleDropdownAsync();
		}

		await SelectItemAsync(itemKey);
	}

	async Task ScrollToAsync(int index)
	{
		if (DropDownListInterop is null || index < 0 || index >= Items.Count)
		{
			return;
		}

		if (ItemsElementReferencesDict.TryGetValue(Items.GetKeyAtIndex(index), out var elementReference))
		{
			await DropDownListInterop.ScrollToAsync(elementReference);
		}
	}

	async Task OnToggleDropdownAsync()
	{
		IsDropdownVisible = !IsDropdownVisible;
		await OnDropDownToggle.InvokeAsync(null);
		await InputElementReference.FocusAsync();
	}

	async Task OnKeyPressAsync(WebKeyboardEventArgs webKeyboardEventArgs)
	{
		if (OnKeyPress.HasDelegate)
		{
			await OnKeyPress.InvokeAsync(webKeyboardEventArgs);
		}
	}

	bool IsSelected(string code) => SelectedCode != null && SelectedCode.Equals(code);

	async Task NotifyChangeAsync(ChangeEventArgs e)
	{
		if (OnChange.HasDelegate)
		{
			await OnChange.InvokeAsync(e);
		}
	}

	string GetClass(string item)
	{
		var isSelected = SelectedIndex == Items.IndexOfKey(item);
		return new CssBuilder()
			.AddClass("cwn-dropdownlist__option")
			.AddClass("cwn-dropdownlist__option--selected", isSelected)
			.Build();
	}

	string GetItemValue(string code)
	{
		if (Items.TryGetValue(code, out var value))
		{
			return value;
		}

		return string.Empty;
	}
}
