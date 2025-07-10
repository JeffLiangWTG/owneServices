using System.Threading.Tasks;
using NUnit.Framework;
using WinzorTestFramework;

namespace System.Windows.Forms;

internal sealed class WinzorSpecificControlsCollectionTest
{
	[Test]
	public async Task ControlIsRemovedFromCurrentCollectionBeforeAdded()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var parent1 = new Control();
			var parent2 = new Control();
			var child = new Control();
			parent1.WinzorSpecificControls.Add(child);
			Assert.That(parent1.WinzorSpecificControls.Count, Is.EqualTo(1));
			Assert.That(parent2.WinzorSpecificControls.Count, Is.EqualTo(0));

			parent2.WinzorSpecificControls.Add(child);
			Assert.That(parent1.WinzorSpecificControls.Count, Is.EqualTo(0));
			Assert.That(parent2.WinzorSpecificControls.Count, Is.EqualTo(1));
		});
	}

	[Test]
	public async Task ControlCannotBeReaddedToCurrentCollection()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var parent = new Control();
			var child = new Control();
			parent.WinzorSpecificControls.Add(child);
			Assert.That(parent.WinzorSpecificControls.Count, Is.EqualTo(1));

			parent.WinzorSpecificControls.Add(child);
			Assert.That(parent.WinzorSpecificControls.Count, Is.EqualTo(1));
		});
	}

	[Test]
	public async Task AfterRemoveTriggeredWhenControlRemovedFrom()
	{
		var afterRemoveTriggered = false;
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var parent = new Control();
			var child = new Control();
			parent.WinzorSpecificControls.Add(child);
			Assert.That(parent.WinzorSpecificControls.Count, Is.EqualTo(1));

			parent.WinzorSpecificControls.AfterRemove += (Control obj) =>
			{
				afterRemoveTriggered = true;
			};

			parent.WinzorSpecificControls.Remove(child);
			Assert.That(parent.WinzorSpecificControls.Count, Is.EqualTo(0));
		});
		Assert.That(afterRemoveTriggered, Is.True);
	}
}
