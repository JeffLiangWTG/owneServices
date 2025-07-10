using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;
using Microsoft.Playwright;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

internal class ZTextBoxBaseContextMenuManagerTest
{
	[Test, WithPlaywrightPage, WithTransaction]
	public async Task ZRichTextBoxInsertTemplate()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		BoundZRichTextBoxFormForTest form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			dummy.Z0_VarBinaryMax = ZBlob.FromUTF8("ABC");
			factory.Save();

			form = new BoundZRichTextBoxFormForTest(dummy, "Z0_VarBinaryMax");
			return form;
		});

		var editor = page.Locator(".richtextbox__editoranchor");
		await editor.WaitForAsync();
		await Assertions.Expect(editor).ToHaveCountAsync(1);
		Assert.That(() => editor.TextContentAsync(), Is.EqualTo("ABC").After(2000, 100));

		await editor.ClearAsync();

		const string templateText = "Template Content";
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.TextBox.contextMenuManager.InitializeContextMenu();
			var template = form.TextBox.contextMenuManager.TextTemplatesFactory.New();
			template.S8_Description = "Template 1";
			template.S8_TemplateText = templateText;
			template.Factory.Save();

			form.TextBox.RichEdit.ContextMenuStrip.Show();
			var templateMenu = (ToolStripMenuItem)form.TextBox.RichEdit.ContextMenuStrip.Items[0];
			templateMenu.DropDown.Show();
			templateMenu.DropDown.Items[0].PerformClick();
		});
		await editor.BlurAsync();
		Assert.That(() => form.TextBox.RichEdit.Text, Is.EqualTo(templateText).After(2000, 100));
		Assert.That(() => editor.TextContentAsync(), Is.EqualTo(templateText).After(2000, 100));
	}

	class BoundZRichTextBoxFormForTest : ZForm
	{
		public BoundZRichTextBoxFormForTest(BusinessObject bzo, string bindingMember)
			: base(bzo)
		{
			TextBox = new ZRichTextBox();
			BindingSource.SetBindingMember(TextBox, bindingMember);
			Controls.Add(TextBox);
		}

		public readonly ZRichTextBox TextBox;
	}
}
