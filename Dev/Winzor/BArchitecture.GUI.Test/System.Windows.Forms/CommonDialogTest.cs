using System.Threading.Tasks;
using Bunit;
using NUnit.Framework;
using WinzorTestFramework;

namespace System.Windows.Forms;
internal class CommonDialogTest
{
	[Test]
	public async Task CommonDialogShouldShowNormally()
	{
		using var ctx = new WinzorTestContext();
		CommonDialogForTest dialog = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var button = new Button() { Text = "Open Dialog" };
			button.Click += (s, e) =>
			{
				dialog = new CommonDialogForTest();
				dialog.ShowDialog();
			};
			form.Controls.Add(button);
			return form;
		});
		await rendered.Find("button").ClickAsync(new WebMouseEventArgs());
		Assert.That(dialog, Is.Not.Null);
		Assert.That(dialog.DialogResult, Is.EqualTo(DialogResult.OK));
	}

	[Test]
	public async Task CommonDialogCanceledAfterThrowException()
	{
		using var ctx = new WinzorTestContext();
		ctx.ThreadExceptionExceptionRaised += _ => true;

		CommonDialogForTest dialog = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var button = new Button() { Text = "Open Dialog" };
			button.Click += (s, e) =>
			{
				dialog = new CommonDialogForTest(true);
				dialog.ShowDialog();
			};
			form.Controls.Add(button);
			return form;
		});
		await rendered.Find("button").ClickAsync(new WebMouseEventArgs());
		Assert.That(dialog, Is.Not.Null);
		Assert.That(dialog.DialogResult, Is.EqualTo(DialogResult.Cancel));
	}

	class CommonDialogForTest : CommonDialog
	{
		public CommonDialogForTest(bool throwException = false)
		{
			this.throwException = throwException;
		}

		public new DialogResult DialogResult
		{
			get => base.DialogResult;
			set => base.DialogResult = value;
		}

		public TaskCompletionSource RunDialogTCS = new TaskCompletionSource();

		readonly bool throwException;

		protected override Task RunDialogAsync(Form contextForm)
		{
			DialogResult = DialogResult.Cancel;
			if (throwException)
			{
				throw new Exception("Test exception");
			}
			DialogResult = DialogResult.OK;
			return Task.CompletedTask;
		}
	}
}
