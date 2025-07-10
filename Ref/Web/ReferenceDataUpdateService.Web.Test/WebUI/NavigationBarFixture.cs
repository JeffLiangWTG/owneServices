using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

namespace ReferenceDataUpdateService.Web.Test.WebUI
{
	[TestFixture]
	public class NavigationBarFixture : PlaywrightTest
	{
		/// <summary>
		/// There was a bug that required a double click to open the navigation dropdowns.
		/// This is caused by a conflict between bootstrap.bundle.min.js and bootstrap.min.js.
		/// If this test fails, check WI00848608 for more information.
		/// </summary>
		[Test]
		public async Task AllowSingleClickToOpenNavigationDropdown()
		{
			// sub menu to be hidden by default
			var globalCodesSubMenu = Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Global Codes" });
			var procedureCodesSubMenu = Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Procedure Codes" });
			await Assertions.Expect(globalCodesSubMenu).Not.ToBeVisibleAsync();
			await Assertions.Expect(procedureCodesSubMenu).Not.ToBeVisibleAsync();

			// require single click to open sub menu
			var customsMenu = Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Customs" }).Locator("visible=true");
			await customsMenu.ClickAsync();
			await Assertions.Expect(globalCodesSubMenu).ToBeVisibleAsync();
			await Assertions.Expect(procedureCodesSubMenu).ToBeVisibleAsync();

			// clicking on sub menu should navigate to the correct page
			await globalCodesSubMenu.ClickAsync();
			await Assertions.Expect(Page).ToHaveURLAsync("http://localhost:19488/RefCusCodeListUserViewSearchForm");
		}
	}
}
