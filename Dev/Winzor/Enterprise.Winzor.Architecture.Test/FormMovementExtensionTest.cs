using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using FormBorderStyle = CargoWise.Blazor.Client.Integration.Messaging.FormBorderStyle;
using FormStartPosition = CargoWise.Blazor.Client.Integration.Messaging.FormStartPosition;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;

class FormMovementExtensionTest
{
	[TestCase(true)]
	[TestCase(false)]
	[TestCase(null)]
	public async Task MoveFormByMouseDragSetsDisableBorderlessWindow(bool? moveFormByMouseDrag)
	{
		using var ctx = new EnterpriseTestContext();

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var form = new Form();

			if (moveFormByMouseDrag != null)
			{
				form.MoveFormByMouseDrag(moveFormByMouseDrag.Value);
			}

			Assert.That(form.DisableBorderlessWindow, Is.EqualTo(moveFormByMouseDrag ?? false));
		});
	}

	[TestCase(true, FormBorderStyle.FixedSingle)]
	[TestCase(false, FormBorderStyle.None)]
	public async Task UnitTestForModalWindowIsNotBorderlessIfDraggable(bool moveFormByMouseDrag, FormBorderStyle expectedStyle)
	{
		using var ctx = new EnterpriseTestContext();
		var loadRequestSent = new TaskCompletionSource<WindowStyleOptions>();
		ctx.MockCargoWiseClientServices.WindowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				EnterpriseTestSetup.WinzorDispatcher.FormInstanceRegister.Lookup(createWindowOptions.Uri).Dispose();
				loadRequestSent.SetResult(windowStyleOptions);
			});
		var rendered = await ctx.RenderFormAsync(() => new Form());
		var mainForm = rendered.GetForm();

		await mainForm.InvokeWinzorDispatcherAsync(() =>
		{
			var popupForm = new Form
			{
				DisableBorderlessWindow = moveFormByMouseDrag,
				FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
			};
			popupForm.ShowDialog();
		});

		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		var windowOptions = await loadRequestSent.Task;
		Assert.That(windowOptions.FormBorderStyle, Is.EqualTo(expectedStyle));
	}

	[TestCase("EnterpriseInformationForm")]
	[TestCase("SetSQLPasswordForm")]
	public async Task FunctionalTestForModalWindowIsNotBorderlessIfDraggable(string popupName)
	{
		using var ctx = new EnterpriseTestContext();
		var loadRequestSent = new TaskCompletionSource<WindowStyleOptions>();
		ctx.MockCargoWiseClientServices.WindowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				EnterpriseTestSetup.WinzorDispatcher.FormInstanceRegister.Lookup(createWindowOptions.Uri).Dispose();
				loadRequestSent.SetResult(windowStyleOptions);
			});
		var rendered = await ctx.RenderFormAsync(() => new Form());
		var mainForm = rendered.GetForm();

		await mainForm.InvokeWinzorDispatcherAsync(() =>
		{
			Form popupForm = popupName switch
			{
				nameof(EnterpriseInformationForm) => new EnterpriseInformationForm(),
				nameof(SetSQLPasswordForm) => new SetSQLPasswordForm(new BusinessObjectFactory().New<GlbStaff>()),
				_ => throw new ArgumentException("Invalid popup name", nameof(popupName))
			};
			popupForm.ShowDialog();
		});

		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		var windowOptions = await loadRequestSent.Task;
		Assert.That(windowOptions.FormBorderStyle, Is.EqualTo(FormBorderStyle.FixedSingle));
	}

	#region Support Functions

	async Task<IPage> ClickButtonAndGetPopup(IElementHandle openButtonEle, IPage parentPage)
	{
		var popupPage = await parentPage.RunAndWaitForPopupAsync(async () =>
		{
			await openButtonEle.ClickAsync();
		});
		await popupPage.WaitForSelectorAsync("button");
		return popupPage;
	}

	#endregion
}
