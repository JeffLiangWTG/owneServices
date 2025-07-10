using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.RichEdit;
using NUnit.Framework;
using WinzorTestFramework;

namespace Enterprise.Winzor.Architecture.Test;

public class ZRichTextBoxPopupFormTest
{
	[Test]
	public async Task RichTextBoxPopupModalFormCallingFormClosedOnlyOnce()
	{
		using var ctx = new EnterpriseTestContext();
		ZRichTextBoxPopupForm richTextBoxPopupForm = null;
		int formClosedCount = 0;
		bool isClosing = false;
		ZForm form = null;
		using var dispatcherContext = new CargoWiseTestWinzorDispatcherContext(ctx);

		await ctx.RenderFormAsync(() => form = new ZForm() { Text = "Main Form" });

		var showDialogTask = ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			richTextBoxPopupForm = new ZRichTextBoxPopupForm(null);
			richTextBoxPopupForm.FormClosed += (s, e) =>
			{
				isClosing = richTextBoxPopupForm.IsClosing;
				formClosedCount++;
			};

			Assert.That(richTextBoxPopupForm.IsClosing, Is.EqualTo(false));
			using (ctx.WinzorDispatcher.WithContext(dispatcherContext))
			{
				richTextBoxPopupForm.ShowDialog(form);
			}
		});

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			richTextBoxPopupForm.Close();
		});

		await showDialogTask;
		Assert.That(formClosedCount, Is.EqualTo(1).After(1000));
		Assert.That(isClosing, Is.EqualTo(true));
	}

	[Test]
	public async Task RichTextBoxPopupModalFormClosingOnMnemonic()
	{
		using var ctx = new EnterpriseTestContext();

		ZRichTextBoxPopupForm richTextBoxPopupForm = null;
		int closedCalled = 0;
		bool isClosing = false;
		ZForm form = null;

		using var dispatcherContext = new CargoWiseTestWinzorDispatcherContext(ctx);

		await ctx.RenderFormAsync(() => form = new ZForm() { Text = "Main Form" });

		var popFormShownTCS = new TaskCompletionSource();
		var showDialogTask = ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			richTextBoxPopupForm = new ZRichTextBoxPopupForm(null);
			richTextBoxPopupForm.FormClosed += (s, e) =>
			{
				isClosing = richTextBoxPopupForm.IsClosing;
				closedCalled++;
			};
			richTextBoxPopupForm.Shown += (s, e) =>
			{
				popFormShownTCS.SetResult();
			};

			Assert.That(richTextBoxPopupForm.IsClosing, Is.EqualTo(false));
			using (ctx.WinzorDispatcher.WithContext(dispatcherContext))
			{
				richTextBoxPopupForm.ShowDialog(form);
			}
		});
		await popFormShownTCS.Task;

		Assert.That(richTextBoxPopupForm.IsClosing, Is.EqualTo(false));
		var renderedForm = dispatcherContext.Rendered;
		await renderedForm.KeyPressAsync(Keys.C, renderedForm.Find(".richtextbox"));

		Assert.That(closedCalled, Is.EqualTo(0).After(1000));
		Assert.That(isClosing, Is.EqualTo(false));

		await renderedForm.KeyPressAsync(Keys.Alt | Keys.C, renderedForm.Find(".richtextbox"));

		await showDialogTask;
		Assert.That(closedCalled, Is.EqualTo(1).After(1000));
		Assert.That(isClosing, Is.EqualTo(true));
	}

	[Test]
	public async Task RichEditOfRichEdit_IsFocused_WhenFormFirstShown()
	{
		using var ctx = new EnterpriseTestContext();
		ZRichTextBoxPopupForm richTextBoxPopupForm = null;
		await ctx.RenderFormAsync(() =>
		{
			richTextBoxPopupForm = new ZRichTextBoxPopupForm(null);
			ZFormModaliser.ShowDialogWithoutDispose(richTextBoxPopupForm);
			return richTextBoxPopupForm;
		});
		Assert.That(richTextBoxPopupForm.Controls.OfType<ZRichTextBox>().First().RichEdit.Focused, Is.True);
	}
}
