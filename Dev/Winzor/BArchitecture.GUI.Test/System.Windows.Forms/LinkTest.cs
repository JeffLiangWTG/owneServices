using NUnit.Framework;

namespace System.Windows.Forms;

class LinkTest
{
	[Test]
	public void TestLinkName()
	{
		var link = new LinkLabel.Link() { Name = "Link1" };
		Assert.That(link.Name, Is.EqualTo("Link1"));

		link.Name = null;
		Assert.That(link.Name, Is.EqualTo(string.Empty));
	}
}
