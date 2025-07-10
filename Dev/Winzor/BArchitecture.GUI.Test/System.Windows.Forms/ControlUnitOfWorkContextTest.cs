using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;

namespace System.Windows.Forms;

class ControlUnitOfWorkContextTest
{
	[Test]
	public async Task TestCallStateHasChangedOnRequiredControlsAsyncShouldNotThrowNullReferenceException()
	{
		ControlForControlUnitOfWorkContextTest control = null;
		using var ctx = new WinzorTestContext();

		await ctx.RenderControlOnFormAsync(() => control = new ControlForControlUnitOfWorkContextTest());
		Assert.That(control.Proxy, Is.Not.Null);

		var proxy = control.Proxy;
		await control.InvokeWinzorDispatcherAsync(() => control.NotifyRenderRequired());
		control.Proxy = proxy;

		Assert.DoesNotThrowAsync(async () => await control.InvokeStateHasChangedAsync());
	}

	[Test]
	public async Task OpenForm_OpeningFormHasOwner_ShouldUseOwnerForm()
	{
		var mockFormOpener = new Mock<IFormOpener>();
		using var ctx = new WinzorTestContext(mockFormOpener.Object, Mock.Of<IFormInstanceRegister>());

		Exception threadException = null;
		ctx.ThreadExceptionExceptionRaised += (e) =>
		{
			threadException = e;
			return true;
		};

		var ownerForm = (await ctx.RenderFormAsync(() => new Form())).GetForm();
		Assert.That(ownerForm, Is.Not.Null);

		var contextControl = (await ctx.RenderControlOnFormAsync(() => new Button())).GetControl<Button>();
		Assert.That(contextControl, Is.Not.Null);

		await contextControl.InvokeWinzorDispatcherAsync(() =>
		{
			var form = new Form { Owner = ownerForm };
			WinzorDispatcher.Current.CurrentContext.OpenForm(form);
		});

		mockFormOpener.Verify(o => o.OpenForm(It.IsAny<IWinzorDispatcherContext>(), It.IsAny<Uri>(), ownerForm, It.IsAny<Form>()), Times.Once);
		Assert.That(threadException, Is.Null,
			"No exception should be thrown when opening a form with an owner.");
	}

	[Test]
	public async Task OpenForm_OpeningFormHasNoOwner_ShouldUseControlContextForm()
	{
		var mockFormOpener = new Mock<IFormOpener>();
		using var ctx = new WinzorTestContext(mockFormOpener.Object, Mock.Of<IFormInstanceRegister>());

		Exception threadException = null;
		ctx.ThreadExceptionExceptionRaised += (e) =>
		{
			threadException = e;
			return true;
		};

		var rendered = await ctx.RenderControlOnFormAsync(() => new Button());
		var contextControlForm = rendered.GetForm();
		var contextControl = rendered.GetControl<Button>();
		Assert.That(contextControl, Is.Not.Null);

		await contextControl.InvokeWinzorDispatcherAsync(() =>
		{
			var form = new Form { Owner = null };
			WinzorDispatcher.Current.CurrentContext.OpenForm(form);
		});

		mockFormOpener.Verify(o => o.OpenForm(It.IsAny<IWinzorDispatcherContext>(), It.IsAny<Uri>(), contextControlForm, It.IsAny<Form>()), Times.Once);
		Assert.That(threadException, Is.Null,
			"No exception should be thrown when opening using the control context form as a fallback.");
	}

	[Test]
	public async Task OpenForm_OpeningFormHasOwnerWithNoCargoWiseClientServices_ShouldUseControlContextForm()
	{
		var mockFormOpener = new Mock<IFormOpener>();
		using var ctx = new WinzorTestContext(mockFormOpener.Object, Mock.Of<IFormInstanceRegister>());

		Exception threadException = null;
		ctx.ThreadExceptionExceptionRaised += (e) =>
		{
			threadException = e;
			return true;
		};

		// Render a form to ensure that the ControlUnitOfWorkContext prefers the control context form over a fallback form
		var otherForm = (await ctx.RenderFormAsync(() => new Form())).GetForm();
		Assert.That(otherForm, Is.Not.Null);

		Form ownerForm = null;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			ownerForm = new Form();
			Application.OpenForms.Add(ownerForm);
		});

		var rendered = await ctx.RenderControlOnFormAsync(() => new Button());
		var contextControlForm = rendered.GetForm();
		var contextControl = rendered.GetControl<Button>();
		Assert.That(contextControl, Is.Not.Null);

		Assert.That(ownerForm, Is.Not.Null);
		Assert.That(ownerForm.CargoWiseClientServices, Is.Null);
		Assert.That(Application.OpenForms, Does.Contain(ownerForm));

		await contextControl.InvokeWinzorDispatcherAsync(() =>
		{
			var form = new Form { Owner = ownerForm };
			WinzorDispatcher.Current.CurrentContext.OpenForm(form);
		});

		mockFormOpener.Verify(o => o.OpenForm(It.IsAny<IWinzorDispatcherContext>(), It.IsAny<Uri>(), contextControlForm, It.IsAny<Form>()), Times.Once);
		Assert.That(threadException, Is.Null,
			"No exception should be thrown when opening using the control context form as a fallback.");
	}

	[Test]
	public async Task OpenForm_OpeningFormHasNoOwnerAndControlHasNoForm_ShouldUseAnyAvailableForm()
	{
		var mockFormOpener = new Mock<IFormOpener>();
		using var ctx = new WinzorTestContext(mockFormOpener.Object, Mock.Of<IFormInstanceRegister>());

		Exception threadException = null;
		ctx.ThreadExceptionExceptionRaised += (e) =>
		{
			threadException = e;
			return true;
		};

		var otherForm = (await ctx.RenderFormAsync(() => new Form())).GetForm();
		Assert.That(otherForm, Is.Not.Null);

		Control control = null;
		await ctx.WinzorDispatcher.InvokeAsync(() => control = new Button());

		Assert.That(control, Is.Not.Null);
		await control.InvokeWinzorDispatcherAsync(() =>
		{
			WinzorDispatcher.Current.CurrentContext.OpenForm(new Form());
		});

		mockFormOpener.Verify(o => o.OpenForm(It.IsAny<IWinzorDispatcherContext>(), It.IsAny<Uri>(), otherForm, It.IsAny<Form>()), Times.Once);
		Assert.That(threadException, Is.Null,
			"No exception should be thrown when opening using another open form as a fallback.");
	}

	[Test]
	public async Task OpenForm_OpeningFormHasNoOwnerAndControlFormHasNoCargoWiseClientServices_ShouldUseAnyAvailableForm()
	{
		var mockFormOpener = new Mock<IFormOpener>();
		using var ctx = new WinzorTestContext(mockFormOpener.Object, Mock.Of<IFormInstanceRegister>());

		Exception threadException = null;
		ctx.ThreadExceptionExceptionRaised += (e) =>
		{
			threadException = e;
			return true;
		};

		var otherForm = (await ctx.RenderFormAsync(() => new Form())).GetForm();
		Assert.That(otherForm, Is.Not.Null);

		Form controlForm = null;
		Control control = null;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			control = new Button();
			controlForm = new Form();
			controlForm.Controls.Add(control);
			Application.OpenForms.Add(controlForm);
		});

		Assert.That(control, Is.Not.Null);
		Assert.That(controlForm, Is.Not.Null);
		Assert.That(controlForm.CargoWiseClientServices, Is.Null);
		Assert.That(Application.OpenForms, Does.Contain(controlForm));

		await control.InvokeWinzorDispatcherAsync(() =>
		{
			WinzorDispatcher.Current.CurrentContext.OpenForm(new Form());
		});

		mockFormOpener.Verify(o => o.OpenForm(It.IsAny<IWinzorDispatcherContext>(), It.IsAny<Uri>(), otherForm, It.IsAny<Form>()), Times.Once);
		Assert.That(threadException, Is.Null,
			"No exception should be thrown when opening using another open form as a fallback.");
	}

	[Test]
	public async Task OpenForm_OpeningFormHasNoOwnerAndControlHasNoFormAndNoAvailableOpenForms_ShouldThrowException()
	{
		var mockFormOpener = new Mock<IFormOpener>();
		using var ctx = new WinzorTestContext(mockFormOpener.Object, Mock.Of<IFormInstanceRegister>());

		Exception threadException = null;
		ctx.ThreadExceptionExceptionRaised += (e) =>
		{
			threadException = e;
			return true;
		};

		Control control = null;
		await ctx.WinzorDispatcher.InvokeAsync(() => control = new Button());

		Assert.That(control, Is.Not.Null);
		await control.InvokeWinzorDispatcherAsync(() =>
		{
			WinzorDispatcher.Current.CurrentContext.OpenForm(new Form());
		});

		Assert.That(threadException, Is.InstanceOf<InvalidOperationException>().With.Message.EqualTo("The context form does not have a CargoWiseClientServices instance"),
			"An exception should be thrown when attempting to open a form with no available form context");
	}

	class ControlForControlUnitOfWorkContextTest : ControlForTest
	{
		protected override bool RenderChainIsDisposedOrDisposing
		{
			get
			{
				Proxy = null;
				return false;
			}
		}
	}
}
