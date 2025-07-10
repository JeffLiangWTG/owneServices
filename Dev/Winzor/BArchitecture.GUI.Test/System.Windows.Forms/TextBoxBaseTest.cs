using System.Threading.Tasks;
using NUnit.Framework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;

namespace System.Windows.Forms;
public class TextBoxBaseTest
{
	[Test]
	public async Task ClearUndo_ResetUndoState()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new DefaultTextBoxBase() { Text = "a" });
		var textBox = rendered.GetControl<DefaultTextBoxBase>();
		textBox.ClearUndo();
		Assert.That(textBox.CanUndo, Is.EqualTo(false));
		Assert.That(textBox.PreText, Is.EqualTo(string.Empty));
	}

	[Test]
	public async Task OnHideSelectionChanged_Invoke_CallsHideSelectionChanged()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new DefaultTextBoxBase() { Text = "a", HideSelection = false });
		var textBox = rendered.GetControl<DefaultTextBoxBase>();
		var callCount = 0;

		EventHandler handler = (sender, e) =>
		{
			callCount++;
		};
		textBox.HideSelectionChanged += handler;
		textBox.HideSelection = true;
		Assert.That(callCount, Is.EqualTo(1));

		textBox.HideSelectionChanged -= handler;
		textBox.HideSelection = false;
		Assert.That(callCount, Is.EqualTo(1));
	}

	[Test]
	public async Task ToString_ReturnsExpected()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new DefaultTextBoxBase() { Text = "a" });
		var textBox = rendered.GetControl<DefaultTextBoxBase>();
		Assert.That(textBox.ToString(), Is.EqualTo("Text: a"));
	}

	[Test]
	public async Task ToString_ReturnsExpectedWithLongText()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new DefaultTextBoxBase() { Text = new string('a', 41) });
		var textBox = rendered.GetControl<DefaultTextBoxBase>();
		Assert.That(textBox.ToString(), Is.EqualTo("Text: aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa..."));
	}

	[Test]
	public async Task IsInputChar_ReturnsTrueWithNonMnemonic()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new DefaultTextBoxBase());
		var textBox = rendered.GetControl<DefaultTextBoxBase>();

		Assert.That(textBox.IsInputCharTest('C'), Is.True);
		Assert.That(textBox.IsInputCharTest('D'), Is.True);
		Assert.That(textBox.IsInputCharTest('R'), Is.True);
	}

	[Test]
	public async Task IsInputKey_ReturnsCorrectly()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new DefaultTextBoxBase());
		var textBox = rendered.GetControl<DefaultTextBoxBase>();

		Assert.That(textBox.IsInputKeyTest(Keys.C), Is.False);
		Assert.That(textBox.IsInputKeyTest(Keys.Alt | Keys.C), Is.False);
		Assert.That(textBox.IsInputKeyTest(Keys.Left), Is.True);
		Assert.That(textBox.IsInputKeyTest(Keys.Back), Is.True);
		Assert.That(textBox.IsInputKeyTest(Keys.Enter), Is.False);
	}

	class DefaultTextBoxBase : TextBoxBase
	{
		public override ITextBoxBaseJSInterop Interop => throw new NotImplementedException();

		public bool IsInputCharTest(char charCode)
		{
			return IsInputChar(charCode);
		}

		public bool IsInputKeyTest(Keys key)
		{
			return IsInputKey(key);
		}
	}
}
