using System.Threading.Tasks;

namespace System.Windows.Forms.Test
{
	// NB: doesn't require thread affinity
	public class KeyPressEventArgsTests
	{
		[TestCase('\0')]
		[TestCase('a')]
		public async Task Ctor_Char(char keyChar)
		{
			using var ctx = new WinFormsTestContext();
			await ctx.WinzorDispatcher.InvokeAsync(() =>
			{
				var e = new KeyPressEventArgs(keyChar);
				Assert.That(e.KeyChar, Is.EqualTo(keyChar));
				Assert.That(e.Handled, Is.False);
			});
		}

		[TestCase('\0')]
		[TestCase('a')]
		public async Task KeyChar_Set_GetReturnsExpected(char value)
		{
			using var ctx = new WinFormsTestContext();
			await ctx.WinzorDispatcher.InvokeAsync(() =>
			{
				var e = new KeyPressEventArgs('b')
				{
					KeyChar = value
				};
				Assert.That(e.KeyChar, Is.EqualTo(value));
			});
		}

		[TestCase(true)]
		[TestCase(false)]
		public async Task Handled_Set_GetReturnsExpected(bool value)
		{
			using var ctx = new WinFormsTestContext();
			await ctx.WinzorDispatcher.InvokeAsync(() =>
			{
				var e = new KeyPressEventArgs('a')
				{
					Handled = value
				};
				Assert.That(e.Handled, Is.EqualTo(value));
			});
		}
	}
}
