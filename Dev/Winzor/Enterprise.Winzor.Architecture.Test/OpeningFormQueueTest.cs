using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;
class OpeningFormQueueTest
{
	[Test]
	public async Task WrittenItemsCanBeReadAsync()
	{
		var queue = new OpeningFormQueue();

		using var ctx = new EnterpriseTestContext();

		Form form = null;

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			form = new Form();
			queue.Write(form);
		});

		var readForm = await queue.ReadAsync();

		Assert.That(readForm, Is.Not.Null);
		Assert.That(readForm, Is.EqualTo(form));
	}
}
