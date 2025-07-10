using Bunit;
using CargoWiseNext.Blazor.Components.JsInterop;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CargoWiseNext.Blazor.Components.Test;

public class CwnDropDownListTest : BunitTestContext
{
	readonly SortedList<string, string> Items = new(new Dictionary<string, string>
	{
		{ "AAA", "AAA Item" },
		{ "BBB", "BBB Item" },
		{ "CCC", "CCC Item" }
	});

	[Test]
	public void CwnDropDownList_RenderTest()
	{
		//Setup
		Services.AddSingleton(new Mock<IDropDownListInterop>().Object);

		// Act
		var cut = RenderComponent<CwnDropDownList>();

		// Assert
		cut.MarkupMatches(@"
            <div class=""cwn-dropdownlist""   >
              <button class=""cwn-dropdownlist__button"" tabindex=""-1"">
                <span class=""cwn-dropdownlist-content"">
                  <input type=""text"" maxlength=""-1"" class=""cwn-dropdownlist__selected-key"" tabindex=""0"" value="""">
                  <span class=""cwn-dropdownlist__selected-value"" title=""""></span>
                </span>
              </button>
              <div class=""cwn-dropdownlist-container"">
                <div role=""group"" class=""cwn-button-group cwn-button-group--override-styles cwn-button-group--text cwn-button-group--size-medium cwn-button-group--horizontal cwn-dropdownlist__menu cwn-dropdownlist__menu--hidden""></div>
              </div>
            </div>");
	}

	[TestCase("AAA", "AAA Item")]
	[TestCase("BBB", "BBB Item")]
	[TestCase("CCC", "CCC Item")]
	public void CwnDropDownList_WhenValidSelectedCodeTest(string code, string value)
	{
		//Setup
		Services.AddSingleton(new Mock<IDropDownListInterop>().Object);

		// Act
		var cut = RenderComponent<CwnDropDownList>(parameters => parameters
			.Add(p => p.Items, Items)
			.Add(p => p.SelectedCode, code)
		);

		// Assert
		cut.Find(".cwn-dropdownlist__button").MarkupMatches(@$"
            <button class=""cwn-dropdownlist__button"" tabindex=""-1"">
              <span class=""cwn-dropdownlist-content"">
                <input type=""text"" maxlength=""-1"" class=""cwn-dropdownlist__selected-key"" tabindex=""0""  value=""{code}"">
                <span class=""cwn-dropdownlist__selected-value"" title=""{value}"">{value}</span>
              </span>
            </button>");
	}

	[Test]
	public void CwnDropDownList_WhenInvalidSelectedCodeTest()
	{
		//Setup
		Services.AddSingleton(new Mock<IDropDownListInterop>().Object);

		// Act
		var cut = RenderComponent<CwnDropDownList>(parameters => parameters
			.Add(p => p.Items, Items)
			.Add(p => p.SelectedCode, "DDD")
		);

		// Assert
		cut.Find(".cwn-dropdownlist__button").MarkupMatches(@"
            <button class=""cwn-dropdownlist__button""   tabindex=""-1"">
              <span class=""cwn-dropdownlist-content"">
                <input type=""text"" maxlength=""-1"" class=""cwn-dropdownlist__selected-key"" tabindex=""0""    value=""DDD""  >
                <span class=""cwn-dropdownlist__selected-value"" title=""""></span>
              </span>
            </button>");
	}

	[Test]
	public void CwnDropDownList_WhenItemsNotSet_ShouldNotThrowErrorTest()
	{
		//Setup
		Services.AddSingleton(new Mock<IDropDownListInterop>().Object);

		// Act
		var cut = RenderComponent<CwnDropDownList>(parameters => parameters
			.Add(p => p.SelectedCode, "AAA")
		);

		// Assert
		cut.Find(".cwn-dropdownlist__button").MarkupMatches(@"
            <button class=""cwn-dropdownlist__button""   tabindex=""-1"">
              <span class=""cwn-dropdownlist-content"">
                <input type=""text"" maxlength=""-1"" class=""cwn-dropdownlist__selected-key"" tabindex=""0""    value=""AAA""  >
                <span class=""cwn-dropdownlist__selected-value"" title=""""></span>
              </span>
            </button>");
	}

	[Test]
	public void CwnDropDownList_WhenDropdownVisibleTest()
	{
		//Setup
		Services.AddSingleton(new Mock<IDropDownListInterop>().Object);

		// Act
		var cut = RenderComponent<CwnDropDownList>(parameters => parameters
			.Add(p => p.Items, Items)
			.Add(p => p.SelectedCode, "BBB")
			.Add(p => p.IsDropdownVisible, true)
		);

		var dropdownMenu = cut.Find("div.cwn-dropdownlist__menu");

		// Assert
		dropdownMenu.MarkupMatches(@"
                     <div role=""group"" class=""cwn-button-group cwn-button-group--override-styles cwn-button-group--text cwn-button-group--size-medium cwn-button-group--horizontal cwn-dropdownlist__menu""   >
                       <div role=""option"" title=""AAA Item"" >
                         <button class=""cwn-dropdownlist__option"" tabindex=""-1""  >
                           <span class=""cwn-dropdownlist-content"">
                             <span class=""cwn-dropdownlist-content-key"">AAA</span>
                             <span class=""cwn-dropdownlist-content-value"">AAA Item</span>
                           </span>
                         </button>
                       </div>
                       <div role=""option"" title=""BBB Item"" aria-selected="""" >
                         <button class=""cwn-dropdownlist__option cwn-dropdownlist__option--selected"" tabindex=""-1""  >
                           <span class=""cwn-dropdownlist-content"">
                             <span class=""cwn-dropdownlist-content-key"">BBB</span>
                             <span class=""cwn-dropdownlist-content-value"">BBB Item</span>
                           </span>
                         </button>
                       </div>
                       <div role=""option"" title=""CCC Item"" >
                         <button class=""cwn-dropdownlist__option"" tabindex=""-1""  >
                           <span class=""cwn-dropdownlist-content"">
                             <span class=""cwn-dropdownlist-content-key"">CCC</span>
                             <span class=""cwn-dropdownlist-content-value"">CCC Item</span>
                           </span>
                         </button>
                       </div>
                     </div>");
	}

	[TestCase(0)]
	[TestCase(1)]
	[TestCase(100)]
	public void CwnDropDownList_MaximumInputLengthTest(int maximumInputLength)
	{
		//Setup
		Services.AddSingleton(new Mock<IDropDownListInterop>().Object);

		// Act
		var cut = RenderComponent<CwnDropDownList>(parameters => parameters
			.Add(p => p.Items, Items)
			.Add(p => p.MaximumInputLength, maximumInputLength)
		);

		// Assert
		cut.Find(".cwn-dropdownlist__selected-key").MarkupMatches(@$"
            <input type=""text"" maxlength=""{maximumInputLength}"" class=""cwn-dropdownlist__selected-key"" tabindex=""0"" value="""">");
	}
}
