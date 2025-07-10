using System.Text;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class TraceMessageWriterTest : TestCase
	{
		public void TestWriteMessage()
		{
			using (var textBox = new ZTextBox())
			{
				textBox.CharacterCasing = CharacterCasing.Normal;
				var expectedMessageBuilder = new StringBuilder();

				var writer = new TraceMessageWriter(textBox);

				writer.WriteMessage("Message 01");
				expectedMessageBuilder.Append("Message 01");
				AssertEquals(expectedMessageBuilder.ToString(), textBox.Text);

				writer.WriteMessage("Message 02");
				expectedMessageBuilder.Append("Message 02");
				AssertEquals(expectedMessageBuilder.ToString(), textBox.Text);

				writer.WriteMessage("Message 03");
				expectedMessageBuilder.Append("Message 03");
				AssertEquals(expectedMessageBuilder.ToString(), textBox.Text);
			}
		}
	}
}
