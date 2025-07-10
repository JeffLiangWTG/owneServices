using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit.Framework;
using WinzorTestFramework;

namespace WinzorFramework;
internal class RegisteredFormInstancesTest
{
	[Test]
	public async Task TestAddedFormIsDeregisteredOnClose()
	{
		using var ctx = new WinzorTestContext();

		var component1 = await ctx.RenderFormAsync(() => new Form());
		var form = component1.GetForm();

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var registeredFormInstances = new RegisteredFormInstances();
			var uri = registeredFormInstances.Add(form.CargoWiseClientServices.ServerBaseUri, form, out var isNewForm, out _);
			var lookupResult = registeredFormInstances.Lookup(uri);
			Assert.That(lookupResult, Is.EqualTo(form));
			form.Close();
			lookupResult = registeredFormInstances.Lookup(uri);
			Assert.That(lookupResult, Is.Null);
		});
	}
}
