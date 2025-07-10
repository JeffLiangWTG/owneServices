using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class ShortcutCreatorTest : ShortcutCreator
{
	[TestCase("TestFileName", "https://localhost:5000/?testurl", "TestFileName.url")]
	[TestCase("Test|<File>*Name", "https://localhost:5000/?testurl", "Test__File__Name.url")]
	public void ShortcutTestContent(string caption, string url, string exceptFileName)
	{
		var shortcut = new ShortcutCreatorForTest(exceptFileName);
		shortcut.CreateDesktopShortcut(caption, url);
	}

	class ShortcutCreatorForTest : ShortcutCreator
	{
		public ShortcutCreatorForTest(string fileName)
		{
			exceptFileName = fileName;
		}

		readonly string exceptFileName;

		protected override void SaveShortcutFile(string fileName, string content)
		{
			Assert.That(fileName, Is.EqualTo(exceptFileName));
			Assert.That(content, Does.Contain("[InternetShortcut]"));
			Assert.That(content, Does.Contain("URL="));
			Assert.That(content, Does.Contain("IconIndex="));
			Assert.That(content, Does.Contain("IconFile="));
		}
	}
}
