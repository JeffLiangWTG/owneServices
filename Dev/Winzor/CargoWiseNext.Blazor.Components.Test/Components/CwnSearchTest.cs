using Bunit;

namespace CargoWiseNext.Blazor.Components.Test.Components
{
	public class CwnSearchTest : BunitTestContext
	{
		[Test]
		public void CwnSearchRenders()
		{
			// Act
			var cut = RenderComponent<CwnSearch>(
				parameters => parameters.Add(p => p.Shortcut, ["CTRL", "Q"]));

			// Assert
			cut.MarkupMatches(
				@"<div class=""cwn-search"">
				  <span class=""cwn-icon cwn-icon--search cwn-neutral cwn-icon--medium"" aria-hidden=""true"" role=""img""></span>
				  <input class=""cwn-text-field cwn-search--input"" type=""text"">
				  <div class=""cwn-search--shortcut"">
					  <div class=""cwn-search-shortcut"" style=""width:unset"">
						  <span>CTRL</span>
					  </div>
					  <div class=""cwn-search-shortcut"" style=""width:unset"">
						  <span>Q</span>
					  </div>
				  </div>
			  </div>");
		}

		[Test]
		public void ClickShouldFocusSearchInput()
		{
			// Act
			var cut = RenderComponent<CwnSearch>();
			var container = cut.Find("div");

			// Assert
			container.Click();
			JSInterop.VerifyFocusAsyncInvoke();
		}
	}
}
