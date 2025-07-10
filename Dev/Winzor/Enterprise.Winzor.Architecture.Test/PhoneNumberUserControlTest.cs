using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Css.Dom;
using Bunit;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WinzorTestFramework;

namespace Enterprise.Winzor.Architecture.Test;

internal class PhoneNumberUserControlTest
{
	#region BUnit

	[TestCase(true, TestName = "PhoneNumberUserControl_RenderContextMenu")]
	[TestCase(false, TestName = "PhoneNumberUserControl_DoNotRenderContextMenuIfBlank")]
	public async Task PhoneNumberUserControl_CheckForContextMenu(bool phoneNumberEntered)
	{
		using var ctx = new EnterpriseTestContext();
		var mockMenuDisplayer = new TestMenuDisplayer();
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: mockMenuDisplayer, jsRuntime: ctx.JSInterop.JSRuntime);

		PhoneNumberUserControl phoneNumberUserControl = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();

			var factory = new BusinessObjectFactory();
			var staff = factory.New<GlbStaff>();
			var phoneNumber = new PhoneNumber(staff.GS_MobilePhoneInfo, null, null);
			form.SetDataBinding(phoneNumber, "");

			if (phoneNumberEntered)
			{
				staff.GS_MobilePhone = "+61 4 1234 5678";
			}

			phoneNumberUserControl = new PhoneNumberUserControl();
			form.Controls.Add(phoneNumberUserControl);

			return form;
		}, clientServices);

		var button = rendered.Find(".button:nth-child(2)");
		await button.ClickAsync(new WebMouseEventArgs());

		Assert.That(mockMenuDisplayer.Menu.MenuItems.Any, Is.EqualTo(phoneNumberEntered));
	}

	[Test]
	public async Task PhoneNumberUserControl_ResetAppearanceWhenEmpty()
	{
		using var ctx = new EnterpriseTestContext();
		var mockMenuDisplayer = new TestMenuDisplayer();
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: mockMenuDisplayer, jsRuntime: ctx.JSInterop.JSRuntime);

		PhoneNumberUserControl phoneNumberUserControl = null;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();

			var factory = new BusinessObjectFactory();
			var staff = factory.New<GlbStaff>();
			var phoneNumber = new PhoneNumber(staff.GS_MobilePhoneInfo, null, null);
			form.SetDataBinding(phoneNumber, "");

			phoneNumberUserControl = new PhoneNumberUserControl();
			form.Controls.Add(phoneNumberUserControl);

			return form;
		}, clientServices);

		var button = rendered.Find(".button:nth-child(2)");
		var defaultImage = rendered.Find(".button:nth-child(2) div").GetStyle().GetPropertyValue("background-image");
		await button.ClickAsync(new WebMouseEventArgs());

		var currentImage = rendered.Find(".button:nth-child(2) div").GetStyle().GetPropertyValue("background-image");
		Assert.That(currentImage, Is.EqualTo(defaultImage));
	}

	#endregion
}
