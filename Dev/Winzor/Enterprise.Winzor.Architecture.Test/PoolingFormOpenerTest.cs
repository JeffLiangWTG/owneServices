using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;

namespace Enterprise.Winzor.Architecture.Test;

public class PoolingFormOpenerTest
{
	[Test]
	public void PoolingFormOpenerImplementsIFormOpener()
	{
		Assert.That(new PoolingFormOpener(new OpeningFormQueue()), Is.InstanceOf<IFormOpener>());
	}

	[Test]
	public async Task OpenFormAddsFormToRegister()
	{
		using var ctx = new EnterpriseTestContext();
		var register = new Mock<IFormInstanceRegister>();
		var poolingFormOpener = new PoolingFormOpener(new OpeningFormQueue());
		Form childForm = null;

		var form = (await ctx.RenderFormAsync(() => new Form())).GetForm();

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			childForm = new Form();
			var uri = register.Object.Add(form.CargoWiseClientServices.ServerBaseUri, childForm, out var isNewForm, out _);
			poolingFormOpener.OpenForm(WinzorDispatcher.Current.CurrentContext, uri, form, childForm);
		});

		register.Verify(r => r.Add(form.CargoWiseClientServices.ServerBaseUri, childForm, out It.Ref<bool>.IsAny, out It.Ref<bool>.IsAny));
	}

	[Test]
	public async Task OpenFormAddsFormToFormQueue()
	{
		using var ctx = new EnterpriseTestContext();
		var register = new RegisteredFormInstances();
		var queue = new OpeningFormQueue();
		var poolingFormOpener = new PoolingFormOpener(queue);
		Form childForm = null;

		var form = (await ctx.RenderFormAsync(() => new Form())).GetForm();

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			childForm = new Form();
			var uri = register.Add(form.CargoWiseClientServices.ServerBaseUri, childForm, out var isNewForm, out _);
			poolingFormOpener.OpenForm(WinzorDispatcher.Current.CurrentContext, uri, form, childForm);
		});

		var queuedForm = await queue.ReadAsync();

		Assert.That(queuedForm, Is.SameAs(childForm));
	}

	[Test]
	public async Task OpenFormAsyncAddsFormToRegister()
	{
		using var ctx = new EnterpriseTestContext();
		var register = new Mock<IFormInstanceRegister>();
		var poolingFormOpener = new PoolingFormOpener(new OpeningFormQueue());

		var form = (await ctx.RenderFormAsync(() => new Form())).GetForm();

		Uri uri = null;
		var isNewActiveForm = false;
		var isNewForm = false;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			uri = register.Object.Add(form.CargoWiseClientServices.ServerBaseUri, form, out isNewActiveForm, out isNewForm);
		});

		await poolingFormOpener.OpenFormAsync(uri, ctx.MockCargoWiseClientServices.WindowService.Object, form);

		register.Verify(r => r.Add(form.CargoWiseClientServices.ServerBaseUri, form, out isNewActiveForm, out isNewForm));
	}

	[Test]
	public async Task OpenFormAsyncAddsFormToFormQueue()
	{
		using var ctx = new EnterpriseTestContext();
		var register = new RegisteredFormInstances();
		var queue = new OpeningFormQueue();
		var poolingFormOpener = new PoolingFormOpener(queue);

		var form = (await ctx.RenderFormAsync(() => new Form())).GetForm();

		Uri uri = null;
		var isNewForm = false;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			uri = register.Add(form.CargoWiseClientServices.ServerBaseUri, form, out isNewForm, out _);
		});

		await poolingFormOpener.OpenFormAsync(uri, ctx.MockCargoWiseClientServices.WindowService.Object, form);

		var queuedForm = await queue.ReadAsync();

		Assert.That(queuedForm, Is.SameAs(form));
	}
}
