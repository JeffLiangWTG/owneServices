using System.Threading.Tasks;
using NUnit.Framework;
using WinzorTestFramework;

namespace System.Windows.Forms;

class LinkCollectionTest
{
	[Test]
	public async Task TestAdd()
	{
		using var ctx = new WinzorTestContext();
		LinkLabel linkLabel = null;
		await ctx.RenderControlOnFormAsync(() => linkLabel = new LinkLabel() { Text = "Click here, here or here." });
		Assert.That(linkLabel.Links.LinksAdded, Is.False);

		await linkLabel.InvokeWinzorDispatcherAsync(() => linkLabel.Links.Add(new LinkLabel.Link(12, 4, "2")));
		Assert.That(linkLabel.Links.Count, Is.EqualTo(1));
		Assert.That(linkLabel.Links[0].LinkData, Is.EqualTo("2"));
		Assert.That(linkLabel.Links.LinksAdded, Is.True);

		await linkLabel.InvokeWinzorDispatcherAsync(() => linkLabel.Links.Add(6, 4, "1"));
		Assert.That(linkLabel.Links.Count, Is.EqualTo(2));
		Assert.That(linkLabel.Links[0].LinkData, Is.EqualTo("1"));

		await linkLabel.InvokeWinzorDispatcherAsync(() => linkLabel.Links.Add(0, 5));
		Assert.That(linkLabel.Links.Count, Is.EqualTo(3));
		Assert.That(linkLabel.Links[0].LinkData, Is.Null);
	}

	[Test]
	public async Task TestAddOverlappingLinks()
	{
		using var ctx = new WinzorTestContext();
		LinkLabel linkLabel = null;
		await ctx.RenderControlOnFormAsync(() =>
		{
			linkLabel = new LinkLabel() { Text = "Click here, here or here." };
			linkLabel.Links.Add(6, 4, "1");
			return linkLabel;
		});

		Assert.Throws<InvalidOperationException>(() => linkLabel.Links.Add(7, 4), "Overlapping link regions");
		Assert.Throws<InvalidOperationException>(() => linkLabel.Links.Add(8, 4, "2"), "Overlapping link regions");
		Assert.Throws<InvalidOperationException>(() => linkLabel.Links.Add(new LinkLabel.Link(9, 4, "2")), "Overlapping link regions");
	}

	[Test]
	public async Task TestContainsAndIndexOf()
	{
		using var ctx = new WinzorTestContext();
		LinkLabel linkLabel = null;
		LinkLabel.Link link = null;
		await ctx.RenderControlOnFormAsync(() =>
		{
			linkLabel = new LinkLabel() { Text = "Click here, here or here." };
			linkLabel.Links.Add(6, 4, "1");
			linkLabel.Links.Add(link = new LinkLabel.Link(12, 4, "2") { Name = "Link2" });
			return linkLabel;
		});

		Assert.That(linkLabel.Links.ContainsKey("Link2"), Is.True);
		Assert.That(linkLabel.Links.IndexOf(link), Is.EqualTo(1));
		Assert.That(linkLabel.Links.IndexOfKey("Link2"), Is.EqualTo(1));
	}

	[Test]
	public async Task TestClear()
	{
		using var ctx = new WinzorTestContext();
		LinkLabel linkLabel = null;
		await ctx.RenderControlOnFormAsync(() =>
		{
			linkLabel = new LinkLabel() { Text = "Click here, here or here." };
			linkLabel.Links.Add(6, 4, "1");
			linkLabel.Links.Add(12, 4, "2");
			linkLabel.Links.Add(20, 4, "3");
			return linkLabel;
		});

		Assert.That(linkLabel.Links.Count, Is.EqualTo(3));
		await linkLabel.InvokeWinzorDispatcherAsync(linkLabel.Links.Clear);
		Assert.That(linkLabel.Links.Count, Is.EqualTo(0));
		Assert.That(linkLabel.TabStop, Is.False);
	}

	[Test]
	public async Task TestRemove()
	{
		using var ctx = new WinzorTestContext();
		LinkLabel linkLabel = null;
		LinkLabel.Link link = null;
		await ctx.RenderControlOnFormAsync(() =>
		{
			linkLabel = new LinkLabel() { Text = "Click here, here or here." };
			linkLabel.Links.Add(6, 4, "1");
			linkLabel.Links.Add(link = new LinkLabel.Link(12, 4, "2"));
			linkLabel.Links.Add(new LinkLabel.Link(20, 4, "3") { Name = "Link3" });
			return linkLabel;
		});

		Assert.That(linkLabel.Links.Count, Is.EqualTo(3));
		Assert.That(linkLabel.TabStop, Is.True);

		await linkLabel.InvokeWinzorDispatcherAsync(() =>
		{
			linkLabel.Links.Remove(link);
			Assert.That(linkLabel.Links.Count, Is.EqualTo(2));

			linkLabel.Links.RemoveAt(0);
			Assert.That(linkLabel.Links.Count, Is.EqualTo(1));

			linkLabel.Links.RemoveByKey("Link3");
			Assert.That(linkLabel.Links.Count, Is.EqualTo(0));
			Assert.That(linkLabel.TabStop, Is.False);
		});
	}
}
