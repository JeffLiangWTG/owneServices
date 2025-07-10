using System.Threading.Tasks;

namespace System.Windows.Forms.Test
{
	public class KeyEventArgsTests
	{
		[TestCase(Keys.A)]
		[TestCase(Keys.Control | Keys.A)]
		[TestCase(Keys.Alt | Keys.A)]
		[TestCase(Keys.Shift | Keys.A)]
		[TestCase(Keys.Control)]
		[TestCase(Keys.Alt)]
		[TestCase(Keys.Shift)]
		[TestCase(Keys.Control | Keys.Alt | Keys.Shift | Keys.A)]
		[TestCase((Keys)(-1))]
		[TestCase((Keys)(0x5D))]
		[TestCase((Keys)(0xFF))]
		[TestCase(Keys.Control | Keys.Alt | Keys.Shift | (Keys)(0x5D))]
		public void Ctor_Keys(Keys keyData)
		{
			var e = new KeyEventArgs(keyData);
			Assert.That(e.KeyData, Is.EqualTo(keyData));
			Assert.That(e.Control, Is.EqualTo((keyData & Keys.Control) == Keys.Control));
			Assert.That(e.Alt, Is.EqualTo((keyData & Keys.Alt) == Keys.Alt));
			Assert.That(e.Shift, Is.EqualTo((keyData & Keys.Shift) == Keys.Shift));
			Assert.That(e.Modifiers, Is.EqualTo(keyData & Keys.Modifiers));
			Assert.That(e.KeyValue, Is.EqualTo((int)(keyData & Keys.KeyCode)));
			Assert.That(e.Handled, Is.False);
			Assert.That(e.SuppressKeyPress, Is.False);

			if (Enum.IsDefined(typeof(Keys), e.KeyValue))
			{
				Assert.That(e.KeyCode, Is.EqualTo((Keys)e.KeyValue));
			}
			else
			{
				Assert.That(e.KeyCode, Is.EqualTo(Keys.None));
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Handled_Set_GetReturnsExpected(bool value)
		{
			var e = new KeyEventArgs(Keys.A)
			{
				SuppressKeyPress = !value,
				Handled = value
			};
			Assert.That(e.Handled, Is.EqualTo(value));
			Assert.That(e.SuppressKeyPress, Is.EqualTo(!value));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void SuppressKeyPress_Set_GetReturnsExpected(bool value)
		{
			var e = new KeyEventArgs(Keys.A)
			{
				Handled = !value,
				SuppressKeyPress = value
			};
			Assert.That(e.SuppressKeyPress, Is.EqualTo(value));
			Assert.That(e.Handled, Is.EqualTo(value));
		}
	}
}
