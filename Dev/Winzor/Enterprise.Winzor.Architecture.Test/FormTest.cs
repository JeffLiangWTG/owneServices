using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.Async;
using CargoWise.Blazor.Client.Integration;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using FormBorderStyle = CargoWise.Blazor.Client.Integration.Messaging.FormBorderStyle;
using FormStartPosition = CargoWise.Blazor.Client.Integration.Messaging.FormStartPosition;

namespace Enterprise.Winzor.Architecture.Test;

public class FormTest
{
	Mock<IWindowService> windowService;
	CargoWiseClientServices clientServices;

	[SetUp]
	public void Setup()
	{
		windowService = new Mock<IWindowService>();
		clientServices = MockCargoWiseClientServices.MakeMock(windowService: windowService.Object);
	}

	[Test]
	public async Task ShowTransientForm()
	{
		using var ctx = new EnterpriseTestContext();
		Form newForm = null;
		var loadRequestSent = new TaskCompletionSource();
		string loadRequestUrl = null;
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestUrl = createWindowOptions.Uri.ToString();
				loadRequestSent.SetResult();
			});
		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var button = new Button();
			button.Click += Button_Click;
			form.Controls.Add(button);
			return form;
		}, clientServices);
		renderedForm.Find("button").Click();
		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);

		using var ctx2 = new EnterpriseTestContext();
		var renderedNewForm = ctx2.RenderEntryPointComponent(loadRequestUrl);
		renderedNewForm.WaitForState(() => renderedNewForm.Instance.Form != null);
		Assert.That(renderedNewForm.Instance.Form, Is.SameAs(newForm));
		windowService.Verify(c => c.RegisterCloseListenerAsync(It.IsAny<Func<Task>>()), Times.Once);
		void Button_Click(object sender, EventArgs e)
		{
			newForm = new Form();
			newForm.Show();
		}
	}

	[Test, WithTransaction]
	public async Task SetCrashRecoveryUrlInLocalStorage()
	{
		using var ctx = new EnterpriseTestContext();
		var count = 0;
		var mockJS = new Mock<IJSRuntime>();
		var clientServices = MockCargoWiseClientServices.MakeMock(jsRuntime: mockJS.Object);
		await ctx.RenderFormAsync(() => Initialization.MainFormInstance, clientServices);
		mockJS.Verify(js => js.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.Is<object[]>(args => args.Length == 2 && args[0].Equals(LocalStorageItemKeys.CrashRecoveryUrl))), Times.Exactly(count));

		var form = default(ZForm);
		await ctx.RenderFormAsync(() =>
		{
			form = new ZForm();
			WinzorDispatcher.Current.FormInstanceRegister.Add(TestNavigationManager.BaseServerUri, form);
			return form;
		}, clientServices);
		mockJS.Verify(js => js.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.Is<object[]>(args => args.Length == 2 && args[0].Equals(LocalStorageItemKeys.CrashRecoveryUrl))), Times.Exactly(++count));
		await form.InvokeWinzorDispatcherAsync(() => form.DisplayMode = ZArchitecture.Core.ODisplayMode.Edit);
		mockJS.Verify(js => js.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.Is<object[]>(args => args.Length == 2 && args[0].Equals(LocalStorageItemKeys.CrashRecoveryUrl))), Times.Exactly(++count));
		// Close or Dispose all can trigger register the form instance from active forms and set crash recovery url, invalid the second call.
		await form.InvokeWinzorDispatcherAsync(() => form.Close());
		mockJS.Verify(js => js.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.Is<object[]>(args => args.Length == 2 && args[0].Equals(LocalStorageItemKeys.CrashRecoveryUrl))), Times.Exactly(++count));
		await form.InvokeWinzorDispatcherAsync(() => form.Dispose());
		mockJS.Verify(js => js.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.Is<object[]>(args => args.Length == 2 && args[0].Equals(LocalStorageItemKeys.CrashRecoveryUrl))), Times.Exactly(count));
	}

	[Test]
	public async Task RegisteredFormRemovedAfterDispose()
	{
		using var ctx = new EnterpriseTestContext();
		var uri = default(Uri);
		Form form = null;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			uri = ctx.WinzorDispatcher.FormInstanceRegister.Add(TestNavigationManager.BaseServerUri, form = new Form());
		});
		var rendered = await ctx.RenderFormAsync(() => form);
		await form.InvokeWinzorDispatcherAsync(form.Close);
		Assert.That(() => ctx.WinzorDispatcher.FormInstanceRegister.Lookup(uri), Is.Null.After(1000, 1000));
	}

	[Test]
	public async Task DisposeAndFinalizeWillNotThrowException()
	{
		using var ctx = new EnterpriseTestContext();
		Uri uri = default(Uri);
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			uri = ctx.WinzorDispatcher.FormInstanceRegister.Add(TestNavigationManager.BaseServerUri, new Form());
		});

		_ = ctx.RenderEntryPointComponent(uri.ToString());

		Assert.DoesNotThrow(() =>
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
		});
	}

	[Test]
	[WithTransaction]
	public async Task ShowEditForm()
	{
		using var ctx = new EnterpriseTestContext();
		Form newForm = null;
		var buttonClickCts = new TaskCompletionSource();
		var loadRequestSent = new TaskCompletionSource();
		string loadRequestUrl = null;
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestUrl = createWindowOptions.Uri.ToString();
				loadRequestSent.SetResult();
			});
		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var button = new Button();
			button.Click += Button_Click;
			form.Controls.Add(button);
			return form;
		}, clientServices);
		renderedForm.Find("button").Click();
		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(10)), Is.True);
		Assert.That(loadRequestUrl, Does.Contain("ShowEditForm"));

		using var ctx2 = new EnterpriseTestContext();
		var renderedNewForm = ctx2.RenderEntryPointComponent(loadRequestUrl);
		renderedNewForm.WaitForState(() => renderedNewForm.Instance.Form != null);
		Assert.That(await buttonClickCts.Task.WithTimeout(TimeSpan.FromSeconds(10)), Is.True);
		Assert.That(renderedNewForm.Instance.Form, Is.SameAs(newForm));

		void Button_Click(object sender, EventArgs e)
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			factory.Save();
			newForm = (Form)new DummyController().ShowEditForm(dummy);
			buttonClickCts.SetResult();
		}
	}

	[Test]
	public async Task ShowDialog()
	{
		using var ctx = new EnterpriseTestContext();
		Form newForm = null;
		var dialogCallerTcs = new TaskCompletionSource();
		var loadRequestSent = new TaskCompletionSource();
		string loadRequestUrl = null;
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestUrl = createWindowOptions.Uri.ToString();
				loadRequestSent.SetResult();
			});
		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var button = new Button();
			button.Click += Button_Click;
			form.Controls.Add(button);
			return form;
		}, clientServices);
		renderedForm.Find("button").Click();
		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);

		using var ctx2 = new EnterpriseTestContext();
		var renderedNewForm = ctx2.RenderEntryPointComponent(loadRequestUrl);
		renderedNewForm.WaitForState(() => renderedNewForm.Instance.Form != null);
		Assert.That(renderedNewForm.Instance.Form, Is.SameAs(newForm));

		Assert.That(await dialogCallerTcs.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.False);
		renderedNewForm.Instance.Form.Dispose();
		Assert.That(await dialogCallerTcs.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);

		void Button_Click(object sender, EventArgs e)
		{
			newForm = new Form();
			newForm.ShowDialog();
			dialogCallerTcs.SetResult();
		}
	}

	[Test]
	public async Task FormConstructorModal([Values] bool isModal)
	{
		using var ctx = new EnterpriseTestContext();
		Form newForm = null;
		var isModalWindow = false;
		var loadRequestSent = new TaskCompletionSource();
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				isModalWindow = showWindowOptions.Modal;
				loadRequestSent.SetResult();
			});
		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var button = new Button();
			button.Click += Button_Click;
			form.Controls.Add(button);
			return form;
		}, clientServices);
		renderedForm.Find("button").Click();
		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
		Assert.That(newForm, Is.Not.Null);
		Assert.That(newForm.Modal, Is.EqualTo(isModal));
		Assert.That(isModalWindow, Is.EqualTo(isModal));

		void Button_Click(object sender, EventArgs e)
		{
			newForm = new Form();
			if (isModal)
			{
				newForm.ShowDialog();
			}
			else
			{
				newForm.Show();
			}
		}
	}

	[Test]
	public async Task FormShowDialogOwnerShouldSetOwnerCorrectly()
	{
		using var ctx = new EnterpriseTestContext();
		Form originalForm = null;
		Form messageBoxForm = null;
		var dialogCallerTcs = new TaskCompletionSource();
		var loadRequestSent = new TaskCompletionSource();

		string loadRequestUrl = null;
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestUrl = createWindowOptions.Uri.ToString();
				loadRequestSent.SetResult();
			});
		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			originalForm = new Form();
			var button = new Button();
			button.Click += Button_Click;
			originalForm.Controls.Add(button);
			return originalForm;
		}, clientServices);
		renderedForm.Find("button").Click();

		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);

		Assert.That(messageBoxForm.Owner, Is.EqualTo(originalForm));

		void Button_Click(object sender, EventArgs e)
		{
			messageBoxForm = new Form();
			messageBoxForm.ShowDialog(originalForm);
			dialogCallerTcs.SetResult();
		}
	}

	[Test, WithPlaywrightPage]
	public async Task FormShowDialogActiveFormShouldBeModalForm()
	{
		using var ctx = new EnterpriseTestContext();
		Form originalForm = null;
		Form messageBoxForm = null;
		var dialogCallerTcs = new TaskCompletionSource();
		var loadRequestSent = new TaskCompletionSource();

		string loadRequestUrl = null;
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestUrl = createWindowOptions.Uri.ToString();
				loadRequestSent.SetResult();
			});
		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			originalForm = new Form() { Text = "Parent Form", Width = 500, Height = 500 };
			var button = new Button();
			button.Click += Button_Click;
			originalForm.Controls.Add(button);
			return originalForm;
		}, clientServices);
		renderedForm.Find("button").Click();

		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
		Assert.That(originalForm.Focused, Is.False);
		Assert.That(messageBoxForm.Modal, Is.True);

		await originalForm.InvokeWinzorDispatcherAsync(() => originalForm.Focus());
		Assert.That(originalForm.Focused, Is.False);

		void Button_Click(object sender, EventArgs e)
		{
			messageBoxForm = new Form() { Text = "Modal Form", Width = 100, Height = 100 };
			messageBoxForm.ShowDialog(originalForm);
			dialogCallerTcs.SetResult();
		}
	}

	[Test]
	public async Task ShowDialogFromShowDialog()
	{
		using var ctx = new EnterpriseTestContext();
		Form newForm1 = null;
		Form newForm2 = null;
		var dialogCaller1Tcs = new TaskCompletionSource();
		var dialogCaller2Tcs = new TaskCompletionSource();
		var loadRequestSent = new TaskCompletionSource();
		string loadRequestUrl = null;
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestUrl = createWindowOptions.Uri.ToString();
				loadRequestSent.SetResult();
			});
		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var button = new Button();
			button.Click += Button1_Click;
			form.Controls.Add(button);
			return form;
		}, clientServices);
		renderedForm.Find("button").Click();
		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);

		using var ctx2 = new EnterpriseTestContext();
		var renderedNewForm1 = ctx2.RenderEntryPointComponent(loadRequestUrl, windowService.Object);
		renderedNewForm1.WaitForState(() => renderedNewForm1.Instance.Form != null);
		Assert.That(renderedNewForm1.Instance.Form, Is.SameAs(newForm1));
		Assert.That(await dialogCaller1Tcs.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.False);

		loadRequestSent = new TaskCompletionSource();
		renderedNewForm1.Find("button").Click();
		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
		using var ctx3 = new EnterpriseTestContext();
		var renderedNewForm2 = ctx3.RenderEntryPointComponent(loadRequestUrl, windowService.Object);
		renderedNewForm2.WaitForState(() => renderedNewForm2.Instance.Form != null);
		Assert.That(renderedNewForm2.Instance.Form, Is.SameAs(newForm2));
		Assert.That(await dialogCaller1Tcs.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.False);
		Assert.That(await dialogCaller2Tcs.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.False);

		renderedNewForm2.Instance.Form.Dispose();
		Assert.That(await dialogCaller1Tcs.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.False);
		Assert.That(await dialogCaller2Tcs.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);

		renderedNewForm1.Instance.Form.Dispose();
		Assert.That(await dialogCaller1Tcs.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);

		void Button1_Click(object sender, EventArgs e)
		{
			newForm1 = new Form();
			var button = new Button();
			button.Click += Button2_Click;
			newForm1.Controls.Add(button);
			newForm1.ShowDialog();
			dialogCaller1Tcs.SetResult();
		}

		void Button2_Click(object sender, EventArgs e)
		{
			newForm2 = new Form();
			newForm2.ShowDialog();
			dialogCaller2Tcs.SetResult();
		}
	}

	[Test]
	public async Task ChildFormShowAfterParentFormClose()
	{
		using var ctx = new EnterpriseTestContext();
		Form newForm1 = null;
		Form newForm2 = null;
		var windowService1 = new Mock<IWindowService>();
		var loadRequestSent = new TaskCompletionSource();

		string loadRequestUrl = null;
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestUrl = createWindowOptions.Uri.ToString();
				loadRequestSent.SetResult();
			});

		// Form0
		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var button = new Button();
			button.Click += Button1_Click;
			form.Controls.Add(button);
			return form;
		}, clientServices);

		// Form0 button click
		renderedForm.Find("button").Click();
		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(2)), Is.True);

		// Form1
		using var ctx1 = new EnterpriseTestContext();
		var renderedNewForm1 = ctx1.RenderEntryPointComponent(loadRequestUrl, windowService1.Object);
		renderedNewForm1.WaitForState(() => renderedNewForm1.Instance.Form != null);

		// Form1 button click
		loadRequestSent = new TaskCompletionSource();
		renderedNewForm1.Find("button").Click();
		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(2)), Is.True);

		// Form2
		using var ctx2 = new EnterpriseTestContext();
		var renderedNewForm2 = ctx2.RenderEntryPointComponent(loadRequestUrl, windowService1.Object);
		renderedNewForm2.WaitForState(() => renderedNewForm2.Instance.Form != null);

		// Summary
		windowService1.Verify(w => w.RequestCloseAsync(), Times.Once);
		windowService.Verify(w => w.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()), Times.Exactly(2));
		windowService1.Verify(w => w.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()), Times.Never);

		void Button1_Click(object sender, EventArgs e)
		{
			newForm1 = new Form();
			var button = new Button();
			button.Click += Button2_Click;
			newForm1.Controls.Add(button);
			newForm1.ShowDialog();
		}

		void Button2_Click(object sender, EventArgs e)
		{
			var senderButton = (Button)sender;
			var parentForm = (Form)senderButton.Parent;
			Assert.That(parentForm, Is.SameAs(newForm1));
			parentForm.Close();
			parentForm.Dispose();
			newForm2 = new Form();
			newForm2.ShowDialog();
		}
	}

	[Test]
	public async Task ChildFormShowAfterParentFormClose_OpenFormsEmpty()
	{
		using var ctx = new EnterpriseTestContext();
		Form parentForm = null;
		Form childForm = null;
		var loadRequestSent = new TaskCompletionSource();

		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) => loadRequestSent.SetResult());

		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			parentForm = new Form();
			var button = new Button();
			button.Click += (sender, e) =>
			{
				childForm = new Form();
				childForm.ShowDialog();
			};
			parentForm.Controls.Add(button);
			return parentForm;
		}, clientServices);

		Assert.That(() => Application.OpenForms.Contains(parentForm), Is.True.After(2000, 100));
		renderedForm.Find("button").Click();
		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);

		var closeForms = Application.OpenForms.GetSnapshot()
			.Select(f => f.InvokeWinzorDispatcherAsync(f.Close))
			.ToArray();
		await Task.WhenAll(closeForms);

		Assert.That(() => Application.OpenForms.Any(), Is.False.After(2000, 100));

		windowService.Verify(w => w.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()), Times.Once());

		childForm.Dispose();
		parentForm.Dispose();
	}

	[TestCase(false)]
	[TestCase(true)]
	public async Task ShowDialogFromOnShown(bool isDispose)
	{
		ErrorReporter.Clear();

		using var ctx = new EnterpriseTestContext();
		Form newForm = null;
		var dialogCallerTcs = new TaskCompletionSource();
		var windowService = new Mock<IWindowService>();
		var loadRequestSent = new TaskCompletionSource();
		string loadRequestUrl = null;
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestUrl = createWindowOptions.Uri.ToString();
				loadRequestSent.SetResult();
			});
		var renderedForm = await ctx.RenderEntryPointComponent(() =>
		{
			var form = new Form();
			form.Shown += Form_Shown;
			return form;
		}, windowService.Object);
		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);

		using var ctx2 = new EnterpriseTestContext();
		var renderedNewForm = ctx2.RenderEntryPointComponent(loadRequestUrl);
		renderedNewForm.WaitForState(() => renderedNewForm.Instance.Form != null);
		Assert.That(renderedNewForm.Instance.Form, Is.SameAs(newForm));

		Assert.That(await dialogCallerTcs.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.False);
		if (isDispose)
		{
			renderedNewForm.Instance.Form.Dispose();
		}
		else
		{
			await renderedNewForm.Instance.Form.InvokeWinzorDispatcherAsync(() => renderedNewForm.Instance.Form.Close());
		}

		Assert.That(await dialogCallerTcs.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);

		Assert.That(await renderedForm.Instance.LoadFormTask.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
		Assert.That(ErrorReporter.LastExceptionReported, Is.Null);

		void Form_Shown(object sender, EventArgs e)
		{
			newForm = new Form();
			newForm.ShowDialog();
			dialogCallerTcs.SetResult();
		}
	}

	[Test]
	public async Task ShowDialogFromOnShownAndCallRefreshOnHandleCreated()
	{
		using var ctx = new EnterpriseTestContext();
		Form newForm = null;
		var dialogCallerTcs = new TaskCompletionSource();
		var windowService = new Mock<IWindowService>();
		var loadRequestSent = new TaskCompletionSource();
		string loadRequestUrl = null;
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestUrl = createWindowOptions.Uri.ToString();
				loadRequestSent.SetResult();
			});
		var renderedForm = await ctx.RenderEntryPointComponent(() =>
		{
			var form = new Form();
			form.Shown += Form_Shown;
			form.HandleCreated += Form_HandleCreated;
			return form;
		}, windowService.Object);
		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);

		using var ctx2 = new EnterpriseTestContext();
		var renderedNewForm = ctx2.RenderEntryPointComponent(loadRequestUrl);
		renderedNewForm.WaitForState(() => renderedNewForm.Instance.Form != null);
		Assert.That(renderedNewForm.Instance.Form, Is.SameAs(newForm));

		Assert.That(await dialogCallerTcs.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.False);
		renderedNewForm.Instance.Form.Dispose();
		Assert.That(await dialogCallerTcs.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);

		void Form_Shown(object sender, EventArgs e)
		{
			newForm = new Form();
			newForm.ShowDialog();
			dialogCallerTcs.SetResult();
		}

		void Form_HandleCreated(object sender, EventArgs e)
		{
			((Form)sender).Refresh();
		}
	}

	[Test, WithPlaywrightPage]
	public async Task FormShowLabelForInMemoryAppServer()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(new Label() { Text = "Hello World" });
			return form;
		});

		await page.WaitForSelectorAsync(".label");
		var content = await page.ContentAsync();
		Assert.That(content, Does.Contain("Hello World"));
	}

	[Test]
	public async Task CloseFormShouldRequestClosePage()
	{
		using var ctx = new EnterpriseTestContext();
		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var button = new Button();
			button.Click += (_, _) => form.Close();
			form.Controls.Add(button);
			return form;
		}, clientServices);
		renderedForm.Find("button").Click();

		windowService.Verify(w => w.RegisterCloseListenerAsync(It.IsAny<Func<Task>>()), Times.Once);
		windowService.Verify(w => w.RequestCloseAsync(), Times.Once);
	}

	class FormForTest : Form
	{
		public event EventHandler Initialized;

		public FormForTest() : base()
		{
		}

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
			Initialized.Invoke(this, EventArgs.Empty);
		}
	}

	[Test, WithPlaywrightPage]
	public async Task FormDisposedShouldNotShowMainMenu()
	{
		var mockCargoWiseServiceProvider = new MockCargoWiseClientSeviceProvider();
		var tcs = new TaskCompletionSource<bool>();
		var showMenuInvocatonCount = 0;
		mockCargoWiseServiceProvider.MockMenuDisplayer.Setup(m => m.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()))
			.ReturnsAsync(MenuShowResultCode.Shown)
			.Callback(() =>
			{
				showMenuInvocatonCount += 1;
			});

		await using var ctx = new InMemoryAppServerTestContext(mockCargoWiseServiceProvider);
		FormForTest form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new FormForTest() { Text = "Form Title" };
			form.Menu = new MainMenu();
			form.Menu.MenuItems.Add(new MenuItem { Text = "TestMenuItem1" });
			form.Menu.MenuItems.Add(new MenuItem { Text = "TestMenuItem2" });
			form.Initialized += (_, _) => tcs.SetResult(true);
			return form;
		});

		await page.WaitForFunctionAsync("document.title === 'Form Title'");
		await tcs.Task.WithTimeout(TimeSpan.FromSeconds(60));

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.Dispose();
			form.Menu.MenuItems[0].Dispose();
		});

		Assert.That(showMenuInvocatonCount, Is.EqualTo(1));
	}

	[Test]
	public async Task CloseFormShouldCallOnClosingAndOnFormClosing()
	{
		using var ctx = new EnterpriseTestContext();
		Form form = null;
		var onClosingTriggered = new TaskCompletionSource();
		var onFormClosingTriggered = new TaskCompletionSource();
		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			form.Closing += OnClosing;
			form.FormClosing += OnFormClosing;
			var button = new Button();
			button.Click += CloseForm;
			form.Controls.Add(button);
			return form;
		});

		renderedForm.Find("button").Click();
		Assert.That(await onClosingTriggered.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
		Assert.That(await onFormClosingTriggered.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);

		void OnClosing(object sender, EventArgs e) => onClosingTriggered.SetResult();
		void OnFormClosing(object sender, EventArgs e) => onFormClosingTriggered.SetResult();
		void CloseForm(object sender, EventArgs e) => form.Close();
	}

	[TestCase(DialogResult.None, DialogResult.Cancel)]
	[TestCase(DialogResult.OK, DialogResult.OK)]
	[TestCase(DialogResult.Cancel, DialogResult.Cancel)]
	[TestCase(DialogResult.Abort, DialogResult.Abort)]
	[TestCase(DialogResult.Retry, DialogResult.Retry)]
	[TestCase(DialogResult.Ignore, DialogResult.Ignore)]
	[TestCase(DialogResult.Yes, DialogResult.Yes)]
	[TestCase(DialogResult.No, DialogResult.No)]
	public async Task ClickModalButtonShouldSetButtonDialogAndCloseModal(DialogResult modalButtonDialogResult, DialogResult expectedModalFormDialogResult)
	{
		using var ctx = new EnterpriseTestContext();
		Form modalForm = null;
		var dialogCallerTcs = new TaskCompletionSource();
		var modalFormClosedCallerTcs = new TaskCompletionSource();
		var windowService = ctx.MockCargoWiseClientServices.WindowService;
		var loadRequestSent = new TaskCompletionSource();
		string loadRequestUrl = null;
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestUrl = createWindowOptions.Uri.ToString();
				loadRequestSent.SetResult();
			});
		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var button = new Button();
			button.Click += Button_Click;
			form.Controls.Add(button);
			return form;
		});
		renderedForm.Find("button").Click();
		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);

		using var ctx2 = new EnterpriseTestContext();
		var renderedNewForm = ctx2.RenderEntryPointComponent(loadRequestUrl);
		renderedNewForm.WaitForState(() => renderedNewForm.Instance.Form != null);
		Assert.That(renderedNewForm.Instance.Form, Is.SameAs(modalForm));
		Assert.That(renderedNewForm.Instance.Form.DialogResult, Is.EqualTo(DialogResult.None));

		Assert.That(await dialogCallerTcs.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.False);
		Assert.That(await modalFormClosedCallerTcs.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.False);
		renderedNewForm.Find("button").Click();
		Assert.That(await dialogCallerTcs.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
		Assert.That(renderedNewForm.Instance.Form.DialogResult, Is.EqualTo(DialogResult.Yes));
		Assert.That(await modalFormClosedCallerTcs.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);

		void Button_Click(object sender, EventArgs e)
		{
			modalForm = new Form();
			var modalButton = new Button() { DialogResult = DialogResult.Yes };
			modalForm.Controls.Add(modalButton);
			modalForm.Closed += (_, _) =>
			{
				modalFormClosedCallerTcs.SetResult();
			};
			modalForm.ShowDialog();
			dialogCallerTcs.SetResult();
		}
	}

	[Test]
	public async Task ClickModalExitButtonShouldSetDialogResultCancelAndCloseModal()
	{
		using var ctx = new EnterpriseTestContext();
		Form modalForm = null;
		var dialogCallerTcs = new TaskCompletionSource();
		var modalFormClosedCallerTcs = new TaskCompletionSource();
		var loadRequestSent = new TaskCompletionSource();
		string loadRequestUrl = null;
		ctx.MockCargoWiseClientServices.WindowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestUrl = createWindowOptions.Uri.ToString();
				loadRequestSent.SetResult();
			});

		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var button = new Button();
			button.Click += Button_Click;
			form.Controls.Add(button);
			return form;
		});
		renderedForm.Find("button").Click();
		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);

		using var ctx2 = new EnterpriseTestContext();
		var renderedNewForm = ctx2.RenderEntryPointComponent(loadRequestUrl);
		renderedNewForm.WaitForState(() => renderedNewForm.Instance.Form != null);
		Assert.That(renderedNewForm.Instance.Form, Is.SameAs(modalForm));
		Assert.That(renderedNewForm.Instance.Form.DialogResult, Is.EqualTo(DialogResult.None));

		Assert.That(await dialogCallerTcs.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.False);
		Assert.That(await modalFormClosedCallerTcs.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.False);
		await renderedNewForm.Instance.Form.InvokeWinzorDispatcherAsync(() => renderedNewForm.Instance.Form.Close());
		Assert.That(await dialogCallerTcs.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
		Assert.That(renderedNewForm.Instance.Form.DialogResult, Is.EqualTo(DialogResult.Cancel));
		Assert.That(await modalFormClosedCallerTcs.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);

		void Button_Click(object sender, EventArgs e)
		{
			modalForm = new Form();
			modalForm.Closed += ModalForm_Closed;
			modalForm.ShowDialog();
			dialogCallerTcs.SetResult();
		}

		void ModalForm_Closed(object sender, EventArgs e)
		{
			modalFormClosedCallerTcs.SetResult();
		}
	}

	[Test]
	public async Task ResizeFormShouldCallWindowService([Values] bool clientWindowOpenSent)
	{
		using var ctx = new EnterpriseTestContext();
		Form newForm = null;
		var renderedForm = await ctx.RenderFormAsync(() => newForm = new Form() { MinimumSize = new Size(10, 20) }, clientServices, clientWindowOpenSent: clientWindowOpenSent);

		await newForm.InvokeWinzorDispatcherAsync(() => newForm.Size = new Size(333, 999));

		windowService.Verify(c => c.RequestUpdateWindowStyleAsync(It.Is<WindowStyleOptions>(o => o.PreferredSizeUnscaled == new Size(333, 999) && o.PreferredMinimumSizeUnscaled == new Size(10, 20))), Times.Exactly(clientWindowOpenSent ? 1 : 0));
	}

	[Test]
	public async Task FormActivateShouldCallWindowService()
	{
		using var ctx = new EnterpriseTestContext();
		Form newForm = null;
		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			newForm = new Form();
			return newForm;
		}, clientServices);

		await newForm.InvokeWinzorDispatcherAsync(() => newForm.Activate());

		windowService.Verify(c => c.FormActivateAsync(), Times.Once);
	}

	[Test]
	public async Task AppResizeShouldNotInformReadyAsync()
	{
		using var ctx = new EnterpriseTestContext();
		Form newForm = null;
		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			newForm = new Form();
			return newForm;
		}, clientServices);

		windowService.Invocations.Clear();

		await newForm.OnBrowserSizeChangedAsync(200, 300);

		windowService.Verify(c => c.RequestUpdateWindowStyleAsync(It.IsAny<WindowStyleOptions>()), Times.Never);
	}

	[Test]
	public async Task ReopenChildFormWithoutException()
	{
		using var ctx = new EnterpriseTestContext();
		Form newForm = null;

		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var showButton = new Button();
			showButton.Click += Button_Click;
			var closeButton = new Button();
			closeButton.Click += CloseButton_Click;
			form.Controls.Add(showButton);
			form.Controls.Add(closeButton);
			return form;
		});

		renderedForm.FindAll("button")[0].Click();
		await Task.Delay(200);

		Assert.That(newForm, Is.Not.Null);

		renderedForm.FindAll("button")[1].Click();
		await Task.Delay(200);

		Assert.DoesNotThrow(() => renderedForm.FindAll("button")[0].Click());

		void Button_Click(object sender, EventArgs e)
		{
			newForm = new Form();
			newForm.ShowDialog();
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			newForm.Close();
		}
	}

	[Test]
	public async Task UpdateFormStyleShouldCallWindowService([Values] bool clientWindowOpenSent)
	{
		using var ctx = new EnterpriseTestContext();
		Form newForm = null;
		var renderedForm = await ctx.RenderFormAsync(() => newForm = new Form() { MinimumSize = new Size(200, 200) }, clientServices);

		await newForm.InvokeWinzorDispatcherAsync(() =>
		{
			newForm.ClientWindowOpenSent = clientWindowOpenSent;

			newForm.MaximizeBox = false;
			newForm.MinimizeBox = false;
			newForm.ControlBox = false;
			newForm.TopMost = true;
			newForm.FormBorderStyle = (System.Windows.Forms.FormBorderStyle)FormBorderStyle.None;
		});

		// MaximizeBox
		windowService.Verify(c => c.RequestUpdateWindowStyleAsync(It.Is<WindowStyleOptions>(o => o.MaximizeBox == false)), clientWindowOpenSent ? Times.Once : Times.Never);
		windowService.Verify(c => c.RequestUpdateWindowStyleAsync(It.Is<WindowStyleOptions>(o => o.MaximizeBox == true)), Times.Never);

		// MinimizeBox
		windowService.Verify(c => c.RequestUpdateWindowStyleAsync(It.Is<WindowStyleOptions>(o => o.MinimizeBox == false)), clientWindowOpenSent ? Times.Once : Times.Never);
		windowService.Verify(c => c.RequestUpdateWindowStyleAsync(It.Is<WindowStyleOptions>(o => o.MinimizeBox == true)), Times.Never);

		// ControlBox
		windowService.Verify(c => c.RequestUpdateWindowStyleAsync(It.Is<WindowStyleOptions>(o => o.ControlBox == false)), clientWindowOpenSent ? Times.Once : Times.Never);
		windowService.Verify(c => c.RequestUpdateWindowStyleAsync(It.Is<WindowStyleOptions>(o => o.ControlBox == true)), Times.Never);

		// TopMost
		windowService.Verify(c => c.RequestUpdateWindowStyleAsync(It.Is<WindowStyleOptions>(o => o.TopMost == true)), clientWindowOpenSent ? Times.Once : Times.Never);
		windowService.Verify(c => c.RequestUpdateWindowStyleAsync(It.Is<WindowStyleOptions>(o => o.TopMost == false)), Times.Never);

		// FormBorderStyle
		windowService.Verify(c => c.RequestUpdateWindowStyleAsync(It.Is<WindowStyleOptions>(o => o.FormBorderStyle == FormBorderStyle.None)), clientWindowOpenSent ? Times.Once : Times.Never);
		windowService.Verify(c => c.RequestUpdateWindowStyleAsync(It.Is<WindowStyleOptions>(o => o.FormBorderStyle != null && o.FormBorderStyle != FormBorderStyle.None)), Times.Never);

		// AutoSizeMode
		windowService.Invocations.Clear();
		await newForm.InvokeWinzorDispatcherAsync(() =>
		{
			newForm.AutoSizeMode = AutoSizeMode.GrowAndShrink;
		});
		windowService.Verify(c => c.RequestUpdateWindowStyleAsync(It.Is<WindowStyleOptions>(o => o == newForm.GenerateWindowStyleOptions())), clientWindowOpenSent ? Times.Once : Times.Never);
	}

	[Test]
	public async Task UpdateWindowStateShouldCallWindowService()
	{
		using var ctx = new EnterpriseTestContext();
		Form newForm = null;
		var renderedForm = await ctx.RenderFormAsync(() => newForm = new Form() { MinimumSize = new Size(200, 200) }, clientServices);

		await newForm.InvokeWinzorDispatcherAsync(() => newForm.WindowState = System.Windows.Forms.FormWindowState.Maximized);

		windowService.Verify(c => c.UpdateWindowStateAsync(CargoWise.Blazor.Client.Integration.Messaging.FormWindowState.Maximized), Times.Once);
	}

	[Test]
	public async Task UpdateFormWindowStateShouldCallResizeInTestContext()
	{
		using var ctx = new EnterpriseTestContext();
		using var dispatcherContext = new CargoWiseTestWinzorDispatcherContext(ctx);

		Form form = null;
		var isFormResizeCalled = false;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			using var c = ctx.WinzorDispatcher.WithContext(dispatcherContext);

			form = new Form();
			form.Resize += (sender, e) =>
			{
				isFormResizeCalled = true;
			};

			form.Show();
			form.WindowState = System.Windows.Forms.FormWindowState.Maximized;
		});

		Assert.That(isFormResizeCalled, Is.True);
	}

	[Test]
	public async Task MinimumSizeChangedShouldCallWindowService([Values] bool clientWindowOpenSent)
	{
		using var ctx = new EnterpriseTestContext();
		Form newForm = null;
		var renderedForm = await ctx.RenderFormAsync(() => newForm = new Form() { Size = new Size(500, 500) }, clientServices, clientWindowOpenSent: clientWindowOpenSent);

		await newForm.InvokeWinzorDispatcherAsync(() => newForm.MinimumSize = new Size(333, 333));

		windowService.Verify(c => c.RequestUpdateWindowStyleAsync(It.Is<WindowStyleOptions>(o => o.PreferredMinimumSizeUnscaled == new Size(333, 333))), Times.Exactly(clientWindowOpenSent ? 1 : 0));
	}

	[Test]
	public async Task AfterMinimumSizeChangedFormSizeShouldGreaterThanOrEqualToMinimumSize()
	{
		using var ctx = new EnterpriseTestContext();
		Form newForm = null;
		var renderedForm = await ctx.RenderFormAsync(() => newForm = new Form() { Size = new Size(500, 500) });

		await newForm.InvokeWinzorDispatcherAsync(() => newForm.MinimumSize = new Size(666, 666));
		Assert.That(newForm.Size.Width, Is.GreaterThanOrEqualTo(newForm.MinimumSize.Width));
		Assert.That(newForm.Size.Height, Is.GreaterThanOrEqualTo(newForm.MinimumSize.Height));

		await newForm.InvokeWinzorDispatcherAsync(() => newForm.Size = new Size(222, 222));
		Assert.That(newForm.Size.Width, Is.GreaterThanOrEqualTo(newForm.MinimumSize.Width));
		Assert.That(newForm.Size.Height, Is.GreaterThanOrEqualTo(newForm.MinimumSize.Height));
	}

	[Test]
	public async Task MaximumSizeChangedShouldCallWindowService([Values] bool clientWindowOpenSent)
	{
		using var ctx = new EnterpriseTestContext();
		Form newForm = null;
		var renderedForm = await ctx.RenderFormAsync(() => newForm = new Form() { Size = new Size(500, 500) }, clientServices, clientWindowOpenSent: clientWindowOpenSent);
		await newForm.InvokeWinzorDispatcherAsync(() => newForm.MaximumSize = new Size(999, 999));

		windowService.Verify(c => c.RequestUpdateWindowStyleAsync(It.Is<WindowStyleOptions>(o => o.PreferredMaximumSizeUnscaled == new Size(999, 999))), Times.Exactly(clientWindowOpenSent ? 1 : 0));
	}

	[Test]
	public async Task AfterMaximumSizeChangedFormSizeShouldNotGreaterThanMaximumSize()
	{
		using var ctx = new EnterpriseTestContext();
		Form newForm = null;
		var renderedForm = await ctx.RenderFormAsync(() => newForm = new Form() { Size = new Size(2000, 2000) });

		await newForm.InvokeWinzorDispatcherAsync(() => newForm.MaximumSize = new Size(999, 999));
		Assert.That(newForm.Size.Width, Is.Not.GreaterThan(newForm.MaximumSize.Width));
		Assert.That(newForm.Size.Height, Is.Not.GreaterThan(newForm.MaximumSize.Height));

		await newForm.InvokeWinzorDispatcherAsync(() => newForm.Size = new Size(1111, 1111));
		Assert.That(newForm.Size.Width, Is.Not.GreaterThan(newForm.MaximumSize.Width));
		Assert.That(newForm.Size.Height, Is.Not.GreaterThan(newForm.MaximumSize.Height));
	}

	[Test]
	public async Task SetCursorCurrentOnNewForm()
	{
		using var ctx = new EnterpriseTestContext();
		Form form = null;
		Form newForm = null;
		var loadRequestSent = new TaskCompletionSource();
		string loadRequestUrl = null;
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestUrl = createWindowOptions.Uri.ToString();
				loadRequestSent.SetResult();
			});
		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			form.Cursor = Cursors.Hand;
			var button = new Button();
			button.Click += (s, e) =>
			{
				newForm = new Form();
				newForm.Cursor = Cursors.Hand;
				newForm.Show();
			};
			form.Controls.Add(button);
			return form;
		}, clientServices);
		renderedForm.Find("button").Click();
		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);

		using var ctx2 = new EnterpriseTestContext();
		var renderedNewForm = ctx2.RenderEntryPointComponent(loadRequestUrl);
		renderedNewForm.WaitForState(() => renderedNewForm.Instance.Form != null);

		Assert.That(renderedForm.Find(".form").Attributes["style"].Value, Does.Contain("--current-cursor:pointer"));
		Assert.That(renderedNewForm.Find(".form").Attributes["style"].Value, Does.Contain("--current-cursor:pointer"));

		await newForm.InvokeWinzorDispatcherAsync(() => newForm.Cursor = Cursors.Help);
		await form.InvokeWinzorDispatcherAsync(() => form.Cursor = Cursors.Help);

		Assert.That(renderedForm.Find(".form").Attributes["style"].Value, Does.Contain("--current-cursor:help"));
		Assert.That(renderedNewForm.Find(".form").Attributes["style"].Value, Does.Contain("--current-cursor:help"));
	}

	static IEnumerable<TestCaseData> TestCursors => CursorCases.TestCursors;

	[TestCaseSource(nameof(TestCursors))]
	public async Task SetCursorOnCursorCurrent(Cursor cursor, string style)
	{
		Form form = null;
		using var ctx = new EnterpriseTestContext();
		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			return form;
		});

		Assert.That(renderedForm.Find(".form").Attributes["style"].Value, Does.Not.Contain($"--current-cursor:{style};"));

		await form.InvokeWinzorDispatcherAsync(() => form.Cursor = cursor);

		if (cursor != null && cursor != Cursors.Default)
		{
			Assert.That(renderedForm.Find(".form").Attributes["style"].Value, Does.Contain($"--current-cursor:{style};"));
		}
		else
		{
			Assert.That(renderedForm.Find(".form").Attributes["style"].Value, Does.Not.Contain($"--current-cursor"));
		}
	}

	[Test, WithTransaction, WithPlaywrightPage]
	public async Task ReloadForm()
	{
		var mockCargoWiseServiceProvider = new MockCargoWiseClientSeviceProvider();
		await using var ctx = new InMemoryAppServerTestContext(mockCargoWiseServiceProvider);
		var mockContext = new Mock<IWinzorDispatcherContext>();
		mockContext.Setup(m => m.OpenForm(It.IsAny<Form>())).Returns(OpenFormAction.None);
		Form form = null;
		Form reloadForm = null;
		var reloadRequestSent = new TaskCompletionSource();
		var page = await ctx.LoadFormAsync(() =>
		{
			using var c = ctx.WinzorDispatcher.WithContext(mockContext.Object);
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			factory.Save();
			form = (Form)new DummyController().ShowEditForm(dummy);
			return form;
		});

		var textBoxInput = page.Locator(".textbox:nth-child(3)");
		await textBoxInput.WaitForAsync();
		Assert.That(await textBoxInput.InputValueAsync(), Is.EqualTo("DEFAULT"));
		await textBoxInput.FillAsync("FOO");
		await page.Mouse.ClickAsync(1, 1);
		Assert.That(await textBoxInput.InputValueAsync(), Is.EqualTo("FOO"));

		var reloadPage = await ctx.LoadFormAsync(() =>
		{
			using var c = ctx.WinzorDispatcher.WithContext(mockContext.Object);
			reloadForm = ((ZForm)form).ReloadForm();
			reloadRequestSent.SetResult();
			return reloadForm;
		});
		Assert.That(await reloadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(2)), Is.True);
		Assert.That(form.IsDisposed, Is.True);
		Assert.That(form.Visible, Is.False);
		Assert.That(reloadForm.Visible, Is.True);
		mockCargoWiseServiceProvider.MockWindowService.Verify(s => s.RequestCloseAsync(), Times.Once);
		mockContext.Verify(s => s.OpenForm(It.IsAny<Form>()), Times.Exactly(2));

		textBoxInput = reloadPage.Locator(".textbox:nth-child(3)");
		await textBoxInput.WaitForAsync();
		Assert.That(await textBoxInput.InputValueAsync(), Is.EqualTo("DEFAULT"));
	}

	[Test, WithTransaction, WithPlaywrightPage]
	public async Task ReloadFormClosesOriginalForm()
	{
		var mockCargoWiseServiceProvider = new MockCargoWiseClientSeviceProvider();
		await using var ctx = new InMemoryAppServerTestContext(mockCargoWiseServiceProvider);
		var mockContext = Mock.Of<IWinzorDispatcherContext>();
		Form form = null;
		var formClosed = false;
		var reloadRequestSent = new TaskCompletionSource();
		var page = await ctx.LoadFormAsync(() =>
		{
			using var c = ctx.WinzorDispatcher.WithContext(mockContext);
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			factory.Save();
			form = (Form)new DummyController().ShowEditForm(dummy);
			form.Closed += (s, e) => formClosed = true;
			return form;
		});

		await page.Locator(".textbox").First.WaitForAsync();
		await ctx.LoadFormAsync(() =>
		{
			using var c = ctx.WinzorDispatcher.WithContext(mockContext);
			var reloadForm = ((ZForm)form).ReloadForm();
			reloadRequestSent.SetResult();
			return reloadForm;
		});

		Assert.That(await reloadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(2)), Is.True);
		Assert.That(formClosed, Is.True);
		mockCargoWiseServiceProvider.MockWindowService.Verify(s => s.RequestCloseAsync(), Times.Once);
	}

	[Test, WithTransaction, WithPlaywrightPage]
	public async Task ReloadFormShowsMenu()
	{
		var mockCargoWiseServiceProvider = new MockCargoWiseClientSeviceProvider();

		var showMenuInvocatonCount = 0;
		var firedTwice = new TaskCompletionSource();
		mockCargoWiseServiceProvider.MockMenuDisplayer.Setup(m => m.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()))
			.ReturnsAsync(MenuShowResultCode.Shown)
			.Callback(() =>
			{
				if (++showMenuInvocatonCount == 2)
				{
					firedTwice.SetResult();
				}
			});

		await using var ctx = new InMemoryAppServerTestContext(mockCargoWiseServiceProvider);
		Form form = null;
		var mockContext = Mock.Of<IWinzorDispatcherContext>();
		var page = await ctx.LoadFormAsync(() =>
		{
			using var c = ctx.WinzorDispatcher.WithContext(mockContext);
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			factory.Save();
			form = (Form)new DummyController().ShowEditForm(dummy);
			return form;
		});
		await page.Locator(".textbox").First.WaitForAsync();
		var reloadRequestSent = new TaskCompletionSource();
		var reloadPage = await ctx.LoadFormAsync(() =>
		{
			using var c = ctx.WinzorDispatcher.WithContext(mockContext);
			var reloadForm = ((ZForm)form).ReloadForm();
			reloadRequestSent.SetResult();
			return reloadForm;
		});
		Assert.That(await reloadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(2)), Is.True);
		var hit = await firedTwice.Task.WithTimeout(TimeSpan.FromSeconds(1));
		Assert.That(hit, Is.True);
		Assert.That(showMenuInvocatonCount, Is.EqualTo(2));
	}

	[WithTransaction, WithPlaywrightPage]
	[TestCase(DialogResult.Yes, "FOO")]
	[TestCase(DialogResult.No, "DEFAULT")]
	public async Task ReloadFormShouldShowConfirmDialog(DialogResult dialogResult, string expect)
	{
		var mockCargoWiseServiceProvider = new MockCargoWiseClientSeviceProvider();
		await using var ctx = new InMemoryAppServerTestContext(mockCargoWiseServiceProvider);
		Form form = null;
		var mockContext = Mock.Of<IWinzorDispatcherContext>();
		var page = await ctx.LoadFormAsync(() =>
		{
			using var c = ctx.WinzorDispatcher.WithContext(mockContext);
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			factory.Save();
			form = (Form)new DummyController().ShowEditForm(dummy);
			return form;
		});

		var textBoxInput1 = page.Locator(".textbox:nth-child(3)");
		await textBoxInput1.WaitForAsync();
		Assert.That(await textBoxInput1.InputValueAsync(), Is.EqualTo("DEFAULT"));

		await textBoxInput1.FillAsync("FOO");

		var textBoxInput2 = page.Locator(".textbox:nth-child(2)");
		await textBoxInput2.ClickAsync();
		await page.WaitForFunctionAsync("document.querySelectorAll('.button__button--with-image-and-text')[1].disabled===false");
		Assert.That(actual: await textBoxInput1.InputValueAsync(), Is.EqualTo("FOO"));

		UnitTestUserNotification.Instance.AddAnswer(dialogResult);
		var reloadRequestSent = new TaskCompletionSource();
		var reloadPage = await ctx.LoadFormAsync(() =>
		{
			using var c = ctx.WinzorDispatcher.WithContext(mockContext);
			var reloadForm = ((ZForm)form).ReloadForm();
			reloadRequestSent.SetResult();
			return reloadForm;
		});
		Assert.That(await reloadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(2)), Is.True);
		textBoxInput1 = reloadPage.Locator(".textbox:nth-child(3)");
		await textBoxInput1.WaitForAsync();
		Assert.That(actual: await textBoxInput1.InputValueAsync(), Is.EqualTo(expect));
	}

	[Test, WithTransaction, WithPlaywrightPage]
	public async Task ReloadFormShouldCancelWhenUserClickCancel()
	{
		var mockCargoWiseServiceProvider = new MockCargoWiseClientSeviceProvider();
		await using var ctx = new InMemoryAppServerTestContext(mockCargoWiseServiceProvider);
		Form form = null;
		var formClosed = false;
		var mockContext = Mock.Of<IWinzorDispatcherContext>();
		var page = await ctx.LoadFormAsync(() =>
		{
			using var c = ctx.WinzorDispatcher.WithContext(mockContext);
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			factory.Save();
			form = (Form)new DummyController().ShowEditForm(dummy);
			form.Closed += (s, e) => formClosed = true;
			return form;
		});

		var textBoxInput1 = page.Locator(".textbox:nth-child(3)");
		Assert.That(async() => await textBoxInput1.InputValueAsync(), Is.EqualTo("DEFAULT").After(1000, 100));

		await textBoxInput1.FillAsync("FOO");

		var textBoxInput2 = page.Locator(".textbox:nth-child(2)");
		await textBoxInput2.ClickAsync();
		await page.WaitForFunctionAsync("document.querySelectorAll('.button__button--with-image-and-text')[1].disabled===false");
		UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var reloadForm = ((ZForm)form).ReloadForm();
		});

		mockCargoWiseServiceProvider.MockWindowService.Verify(s => s.RequestCloseAsync(), Times.Never);
		Assert.That(formClosed, Is.False);
	}

	[Test, WithTransaction, WithPlaywrightPage]
	public async Task ReloadFormsReopensTheCurrentTab()
	{
		var mockCargoWiseServiceProvider = new MockCargoWiseClientSeviceProvider();
		await using var ctx = new InMemoryAppServerTestContext(mockCargoWiseServiceProvider);
		Form form = null;
		DummyController controller = null;
		DummyBusinessObject dummy = null;
		var mockContext = Mock.Of<IWinzorDispatcherContext>();
		var page = await ctx.LoadFormAsync(() =>
		{
			using var c = ctx.WinzorDispatcher.WithContext(mockContext);
			var factory = new BusinessObjectFactory();
			dummy = factory.New<DummyBusinessObject>();
			factory.Save();
			controller = new DummyController();
			form = (Form)controller.ShowEditForm(dummy);
			return form;
		});

		var tab1 = page.Locator(".tabcontrol__button:nth-child(2)");
		await tab1.ClickAsync();
		Assert.That(() => ((TabControl)form.Controls[2]).SelectedIndex, Is.EqualTo(1).After(1000, 200));

		var reloadRequestSent = new TaskCompletionSource();
		Form reloadForm = null;
		var reloadPage = await ctx.LoadFormAsync(() =>
		{
			using var c = ctx.WinzorDispatcher.WithContext(mockContext);
			reloadForm = ((ZForm)form).ReloadForm();
			reloadRequestSent.SetResult();
			return reloadForm;
		});
		Assert.That(await reloadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(2)), Is.True);
		var tab2 = reloadPage.Locator(".tabcontrol > .tabcontrol__navigation > .tabcontrol__tabs > .active");
		await tab2.WaitForAsync();
		Form newform = controller.GetOpenedForm(dummy);
		foreach (Control control in newform.Controls)
		{
			if (control is TabControl tb)
			{
				Assert.That(tb.SelectedIndex, Is.EqualTo(1));
			}
		}
		mockCargoWiseServiceProvider.MockWindowService.Verify(s => s.RequestCloseAsync(), Times.Once);
	}

	[Test]
	public async Task TestHide()
	{
		using var ctx = new EnterpriseTestContext();
		Form newForm = null;
		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			newForm = new Form();
			return newForm;
		}, clientServices);

		await newForm.InvokeWinzorDispatcherAsync(() => newForm.Hide());

		windowService.Verify(c => c.RequestHideWindowAsync(), Times.Once);
	}

	[Test]
	public async Task TestShowHidden()
	{
		using var ctx = new EnterpriseTestContext();
		Form newForm = null;
		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			newForm = new Form();
			return newForm;
		}, clientServices);

		await newForm.InvokeWinzorDispatcherAsync(() =>
		{
			newForm.Hide();
			newForm.Show();
		});

		windowService.Verify(c => c.RequestShowWindowAsync(It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()), Times.Once);
	}

	[Test]
	public async Task TestCloseHidden()
	{
		using var ctx = new EnterpriseTestContext();
		Form newForm = null;
		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			newForm = new Form();
			return newForm;
		}, clientServices);

		await newForm.InvokeWinzorDispatcherAsync(() => newForm.Hide());
		await newForm.InvokeWinzorDispatcherAsync(() => newForm.Close());

		windowService.Verify(c => c.RequestHideWindowAsync(), Times.Once);
		windowService.Verify(c => c.RequestCloseAsync(), Times.Once);
	}

	[Test]
	public async Task ShowFormCorrectlyFromTaskCallback()
	{
		using var ctx = new EnterpriseTestContext();
		var loadRequestSent = new TaskCompletionSource();
		string loadRequestUrl = null;
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestUrl = createWindowOptions.Uri.ToString();
				loadRequestSent.SetResult();
			});

		Form newForm1 = null;
		Form newForm2 = null;
		Form newForm3 = null;

		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var button = new Button();
			button.Click += ButtonOnFormClick;
			form.Controls.Add(button);

			return form;
		}, clientServices);

		loadRequestSent = new TaskCompletionSource();
		renderedForm.Find("button").Click();
		await loadRequestSent.Task;
		using var ctxForNewForm1 = new EnterpriseTestContext();
		var renderedNewForm1 = ctxForNewForm1.RenderEntryPointComponent(loadRequestUrl, windowService.Object);
		renderedNewForm1.WaitForState(() => renderedNewForm1.Instance.Form != null);
		Assert.That(renderedNewForm1.Instance.Form, Is.SameAs(newForm1));

		loadRequestSent = new TaskCompletionSource();
		renderedNewForm1.Find("button").Click();
		await loadRequestSent.Task;
		using var ctxForNewForm2 = new EnterpriseTestContext();
		var renderedNewForm2 = ctxForNewForm2.RenderEntryPointComponent(loadRequestUrl, windowService.Object);
		renderedNewForm2.WaitForState(() => renderedNewForm2.Instance.Form != null);
		Assert.That(renderedNewForm2.Instance.Form, Is.SameAs(newForm2));

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			newForm1.Close();

			var syncContext = SynchronizationContext.Current;
			var timer = new System.Timers.Timer { Interval = 1000, AutoReset = false };
			timer.Elapsed += (_, _) =>
			{
				timer.Stop();

				syncContext.Post(_ =>
				{
					newForm3 = new Form();
					newForm3.Show();
				}, null);
			};

			loadRequestSent = new TaskCompletionSource();
			timer.Start();
		});

		await loadRequestSent.Task;
		using var ctxForNewForm3 = new EnterpriseTestContext();
		var renderedNewForm3 = ctxForNewForm3.RenderEntryPointComponent(loadRequestUrl, windowService.Object);
		renderedNewForm3.WaitForState(() => renderedNewForm3.Instance.Form != null);
		Assert.That(renderedNewForm3.Instance.Form, Is.SameAs(newForm3));

		void ButtonOnFormClick(object sender, EventArgs e)
		{
			newForm1 = new Form();
			var button = new Button();
			button.Click += ButtonOnNewForm1Click;
			newForm1.Controls.Add(button);
			newForm1.Show();
		}

		void ButtonOnNewForm1Click(object sender, EventArgs e)
		{
			newForm2 = new Form();
			newForm2.ShowDialog();
		}
	}

	[Test, WithTransaction, WithPlaywrightPage(Headless = false)]
	public async Task TestUnobservedJSDisconnectedExceptionNotReported()
	{
		var formRef = await ShowAndCloseForm();

		Assert.That(() => !formRef.TryGetTarget(out var form) || form.IsDisposed, Is.True.After(1000));
		for (var i = 0; i < 10; i++)
		{
			Thread.Sleep(100);
			GC.Collect();
			GC.WaitForPendingFinalizers();
			Assert.That(ErrorReporter.LastExceptionReported, Is.Null);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	async Task<WeakReference<Form>> ShowAndCloseForm()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var form = default(Form);
		var page = await ctx.LoadFormAsync(() =>
		{
			var mockContext = Mock.Of<IWinzorDispatcherContext>();
			using var c = ctx.WinzorDispatcher.WithContext(mockContext);
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			factory.Save();
			form = (Form)new DummyController().ShowEditForm(dummy);
			return form;
		});

		var textBoxInput = await page.WaitForSelectorAsync(".textbox");

		await form.CloseHandlerAsync();

		return new WeakReference<Form>(form);
	}

	[Test]
	public async Task RestoreBoundsReturnsBounds()
	{
		var expectedBounds = new Rectangle(10, 20, 123, 456);
		Form form = null;

		using var ctx = new EnterpriseTestContext();
		var uri = default(Uri);
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			form = new Form() { Size = expectedBounds.Size, Location = expectedBounds.Location };
			uri = ctx.WinzorDispatcher.FormInstanceRegister.Add(TestNavigationManager.BaseServerUri, form);
		});

		Assert.That(() => form, Is.Not.Null.After(5000, 100));
		Assert.That(form.Bounds, Is.EqualTo(expectedBounds), "Incorrect Bounds");
		Assert.That(form.RestoreBounds, Is.EqualTo(expectedBounds), "Incorrect RestoreBounds");
	}

	[Test, WithPlaywrightPage]
	public async Task KeepParentActiveForModelFormOnSeparateThread()
	{
		using var ctx = new EnterpriseTestContext();
		Form originalForm = null;
		Form messageBoxForm = null;
		bool modal = false;
		var dialogCallerTcs = new TaskCompletionSource();
		var loadRequestSent = new TaskCompletionSource();

		string loadRequestUrl = null;
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				modal = showWindowOptions.Modal;
				loadRequestUrl = createWindowOptions.Uri.ToString();
				loadRequestSent.SetResult();
			});
		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			originalForm = new Form() { Text = "Parent Form", Width = 500, Height = 500 };
			var button = new Button() { Text = "Button", Width = 70, Height = 50 };
			button.Click += Button_Click;
			originalForm.Controls.Add(button);
			return originalForm;
		}, clientServices);
		renderedForm.Find("button").Click();

		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
		Assert.That(messageBoxForm.Modal, Is.True);
		Assert.That(modal, Is.EqualTo(false));

		void Button_Click(object sender, EventArgs e)
		{
			_ = DefaultAsyncStrategy.Get().RunInAnotherWinformsThreadAsync(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					messageBoxForm = new Form() { Text = "Modal Form", Width = 100, Height = 100 };
					messageBoxForm.ShowDialog(originalForm);
				}
			}, threadName: "NewForm");
			dialogCallerTcs.SetResult();
		}
	}

	[Test, WithPlaywrightPage]
	public async Task DoesNotKeepParentActiveForModelFormOnSameThread()
	{
		using var ctx = new EnterpriseTestContext();
		Form originalForm = null;
		Form messageBoxForm = null;
		bool modal = false;
		var dialogCallerTcs = new TaskCompletionSource();
		var loadRequestSent = new TaskCompletionSource();

		string loadRequestUrl = null;
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				modal = showWindowOptions.Modal;
				loadRequestUrl = createWindowOptions.Uri.ToString();
				loadRequestSent.SetResult();
			});
		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			originalForm = new Form() { Text = "Parent Form", Width = 500, Height = 500 };
			var button = new Button() { Text = "Button", Width = 70, Height = 50 };
			button.Click += Button_Click;
			originalForm.Controls.Add(button);
			return originalForm;
		}, clientServices);
		renderedForm.Find("button").Click();

		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
		Assert.That(messageBoxForm.Modal, Is.True);
		Assert.That(modal, Is.EqualTo(true));

		void Button_Click(object sender, EventArgs e)
		{
			messageBoxForm = new Form() { Text = "Modal Form", Width = 100, Height = 100 };
			messageBoxForm.ShowDialog(originalForm);
			dialogCallerTcs.SetResult();
		}
	}

	[Test]
	public async Task FormCloseShouldInvokeCloseRequestReceivedAsync()
	{
		using var ctx = new EnterpriseTestContext();
		bool isCloseRequestReceivedAsyncCalled = false;
		Func<Task> invokeMethod = null;

		windowService.Setup(o => o.CloseRequestReceivedAsync()).Callback(() =>
		{
			isCloseRequestReceivedAsyncCalled = true;
		});

		windowService.Setup(o => o.RegisterCloseListenerAsync(It.IsAny<Func<Task>>()))
			.Callback<Func<Task>>((action) =>
			{
				invokeMethod = action;
			});

		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var button = new Button();
			form.Controls.Add(button);
			return form;
		}, clientServices);

		await invokeMethod();
		Assert.That(() => isCloseRequestReceivedAsyncCalled, Is.EqualTo(true));
	}

	[Test]
	public async Task PreloadClientEventServiceJSInterop()
	{
		using var ctx = new EnterpriseTestContext();
		var services = ctx.MockCargoWiseClientServices;
		var interop = new Mock<IClientEventServiceJSInterop>();
		interop.Setup(i => i.PreloadInterop());
		ctx.Services.AddScoped(_ => interop.Object);

		var rendered = await ctx.RenderFormAsync(() => new Form());

		interop.Verify(e => e.PreloadInterop(), Times.Once());
	}

	[Test]
	public async Task PreloadClipboardJSInterop()
	{
		using var ctx = new EnterpriseTestContext();
		var services = ctx.MockCargoWiseClientServices;
		var interop = new Mock<IClipboardJSInterop>();
		interop.Setup(i => i.PreloadInterop());
		ctx.Services.AddScoped(_ => interop.Object);

		var rendered = await ctx.RenderFormAsync(() => new Form());

		interop.Verify(e => e.PreloadInterop(), Times.Once());
	}

	[Test]
	public async Task PreloadFileServiceJSInterop()
	{
		using var ctx = new EnterpriseTestContext();
		var services = ctx.MockCargoWiseClientServices;
		var interop = new Mock<IFileServiceJSInterop>();
		interop.Setup(i => i.PreloadInterop());
		ctx.Services.AddScoped(_ => interop.Object);

		var rendered = await ctx.RenderFormAsync(() => new Form());

		interop.Verify(e => e.PreloadInterop(), Times.Once());
	}

	[Test]
	public async Task FormSendsRequestShowWindowAsyncWithCorrectArgumentsWhenRendered()
	{
		using var ctx = new EnterpriseTestContext();

		CreateWindowOptions receivedCreateWindowOptions = null;
		ShowWindowOptions receivedShowWindowOptions = null;
		WindowStyleOptions receivedWindowStyleOptions = null;
		var windowOpenAsync = new TaskCompletionSource();

		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				receivedCreateWindowOptions = createWindowOptions;
				receivedShowWindowOptions = showWindowOptions;
				receivedWindowStyleOptions = windowStyleOptions;

				windowOpenAsync.SetResult();
			});

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var mainForm = new Form();

			//This is the form being shown (that will trigger RequestShowWindowAsync when rendered).
			var formToOpen = new Form()
			{
				ControlBox = true,
				FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow,
				StartPosition = System.Windows.Forms.FormStartPosition.Manual,
				MaximizeBox = false,
				MinimizeBox = false,
				MinimumSize = new Size(1, 2),
				MaximumSize = new Size(999, 987),
				Size = new Size(123, 456),
			};

			_ = mainForm.InvokeWinzorDispatcherAsync(() => formToOpen.Show());

			return mainForm;
		}, clientServices);

		Assert.That(await windowOpenAsync.Task.WithTimeout(TimeSpan.FromSeconds(10)), "RequestShowWindowAsync has not been sent.");

		Assert.That(receivedCreateWindowOptions.Uri.ToString(), Does.StartWith("https://test.wisecloud.com/?Command=ShowTransientForm&ID="));

		Assert.That(receivedShowWindowOptions.Modal, Is.False);
		Assert.That(receivedShowWindowOptions.ParentWindowId, Is.Null);
		Assert.That(receivedShowWindowOptions.StartPosition, Is.EqualTo(FormStartPosition.Manual));

		Assert.That(receivedWindowStyleOptions.PreferredSizeUnscaled?.ToSize(), Is.EqualTo(new Size(123, 456)));
		Assert.That(receivedWindowStyleOptions.PreferredMinimumSizeUnscaled?.ToSize(), Is.EqualTo(new Size(1, 2)));
		Assert.That(receivedWindowStyleOptions.PreferredMaximumSizeUnscaled?.ToSize(), Is.EqualTo(new Size(999, 987)));
		Assert.That(receivedWindowStyleOptions.MinimizeBox, Is.False);
		Assert.That(receivedWindowStyleOptions.MaximizeBox, Is.False);
		Assert.That(receivedWindowStyleOptions.ControlBox, Is.True);
		Assert.That(receivedWindowStyleOptions.FormBorderStyle, Is.EqualTo(FormBorderStyle.SizableToolWindow));
		Assert.That(receivedWindowStyleOptions.TopMost, Is.False);
		Assert.That(receivedWindowStyleOptions.Enabled, Is.True);
	}

	[Test]
	public async Task FormDoesNotSendRedundantClientCallsWhenRendered()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderFormAsync(() => new Form(), clientServices);
		var parentForm = rendered.GetForm();

		rendered.WaitForState(() => rendered.RenderCount == 1);
		windowService.Verify(c => c.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()), Times.Never);

		windowService.Invocations.Clear();

		Form formToOpen = null;
		await ctx.WinzorDispatcher.InvokeAsync(() => formToOpen = new Form());

		await parentForm.InvokeWinzorDispatcherAsync(() => formToOpen.Show());

		windowService.Verify(c => c.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()), Times.Once);
		windowService.Verify(c => c.RequestUpdateWindowStyleAsync(It.IsAny<WindowStyleOptions>()), Times.Never);
	}

	[Test, WithTransaction, WithPlaywrightPage]
	public async Task ReloadFormShouldTriggerOpenFormWhenDisposeTriggersForceClose()
	{
		var mockCargoWiseServiceProvider = new MockCargoWiseClientSeviceProvider();
		await using var ctx = new InMemoryAppServerTestContext(mockCargoWiseServiceProvider);
		var mockContext = new Mock<IWinzorDispatcherContext>();
		mockContext.Setup(m => m.OpenForm(It.IsAny<Form>())).Returns(OpenFormAction.None);
		Form form = null;
		Form reloadForm = null;
		var reloadRequestSent = new TaskCompletionSource();
		var page = await ctx.LoadFormAsync(() =>
		{
			using var c = ctx.WinzorDispatcher.WithContext(mockContext.Object);
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			factory.Save();
			form = (Form)new DummyController().ShowEditForm(dummy);
			form.Disposed += (s, e) => form.Close(); //Happens When Edoc is added and modified
			return form;
		});

		var textBoxInput = page.Locator(".textbox:nth-child(3)");
		await textBoxInput.WaitForAsync();
		Assert.That(async () => await textBoxInput.InputValueAsync(), Is.EqualTo("DEFAULT").After(2000, 100));
		await textBoxInput.FillAsync("FOO");
		await page.Mouse.ClickAsync(1, 1);
		Assert.That(async () => await textBoxInput.InputValueAsync(), Is.EqualTo("FOO").After(2000, 100));

		var reloadPage = await ctx.LoadFormAsync(() =>
		{
			using var c = ctx.WinzorDispatcher.WithContext(mockContext.Object);
			reloadForm = ((ZForm)form).ReloadForm();
			reloadRequestSent.SetResult();
			return reloadForm;
		});
		Assert.That(await reloadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(2)), Is.True);
		Assert.That(reloadForm.Visible, Is.True);
		mockCargoWiseServiceProvider.MockWindowService.Verify(s => s.RequestCloseAsync(), Times.Once);
		mockContext.Verify(s => s.OpenForm(It.IsAny<Form>()), Times.Exactly(2));

		textBoxInput = reloadPage.Locator(".textbox:nth-child(3)");
		await textBoxInput.WaitForAsync();
		Assert.That(async () => await textBoxInput.InputValueAsync(), Is.EqualTo("DEFAULT").After(2000, 100));
	}
}
