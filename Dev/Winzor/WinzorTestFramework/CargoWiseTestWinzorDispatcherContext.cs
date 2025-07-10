using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.Blazor.Client.Integration.Files;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.Blazor.Client.Integration.OIDCVerify;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using WinzorFramework;
using WinzorFramework.JSInterop;
using FormWindowState = CargoWise.Blazor.Client.Integration.Messaging.FormWindowState;

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
#pragma warning disable VSTHRD110 // Observe result of async calls

namespace WinzorTestFramework;

public class CargoWiseTestWinzorDispatcherContext : IWinzorDispatcherContext, IDisposable
{
	public CargoWiseTestWinzorDispatcherContext(WinzorTestContext winzorTestContext)
	{
		this.winzorTestContext = winzorTestContext;
		SetupMockClientServices(winzorTestContext.MockCargoWiseClientServices);
	}

	OpenFormAction IWinzorDispatcherContext.OpenForm(Form form)
	{
		RenderFormInTestContext(form);
		return OpenFormAction.BlockUntilShown;
	}

	void RenderFormInTestContext(Form form)
	{
		form.ReadyToRender(winzorTestContext.DefaultClientServices, Guid.NewGuid());

		if (form.IsHandleCreated) // Don't try and render if the form has detached from renderer
		{
			using var cts = new CancellationTokenSource();
			var renderTask = winzorTestContext.Renderer.Dispatcher.InvokeAsync(() =>
			{
				try
				{
					Rendered = winzorTestContext.RenderComponent<ControlProxyComponent>(ComponentParameter.CreateParameter(nameof(ControlProxyComponent.Control), form));
				}
				catch (ObjectDisposedException)
				{
				}
			}).ContinueWith(_ => cts.Cancel(), TaskScheduler.Default);
			WinzorDispatcher.Current.RunMessageLoop(cts);
			renderTask.GetAwaiter().GetResult();
		}
	}

	void SetupMockClientServices(MockCargoWiseClientServices mockClientServices)
	{
		mockClientServices.WindowService.Reset();
		var formRestoreSize = Size.Empty;
		mockClientServices.WindowService.Setup(o => o.UpdateWindowStateAsync(It.IsAny<FormWindowState>())).Callback<FormWindowState>((windowState) =>
		{
			// Try and find the form instance which is being updated - it will likely be the one with the matching window state
			var form = Application.OpenForms.First(f => f.WindowState == (System.Windows.Forms.FormWindowState)windowState);
			switch (windowState)
			{
				case FormWindowState.Maximized:
					{
						formRestoreSize = form.ClientSize;
						form.Size = new Size(3840, 2160);
						break;
					}
				case FormWindowState.Minimized:
					{
						formRestoreSize = form.ClientSize;
						form.ClientSize = Size.Empty; // to trigger the ClientSizeChanged event
						break;
					}
				case FormWindowState.Normal:
					{
						form.ClientSize = formRestoreSize; // to trigger the ClientSizeChanged event
						break;
					}
			}
		});
		mockClientServices.WindowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				var loadForm = WinzorDispatcher.Current.FormInstanceRegister.ClaimFormInstance(createWindowOptions.Uri);
				if (loadForm != null)
				{
					RenderFormInTestContext(loadForm);
				}
			});

		mockClientServices.ClientEventService.Reset();
		mockClientServices.ClientEventService.Setup(m => m.RegisterKeyEventListenerAsync(
			It.IsAny<Func<Task>>(),
			ClientKeyEvent.KeyPress,
			It.IsAny<ClientKeyEventData>(),
			It.IsAny<ElementReference>()
		)).Callback<Func<Task>, ClientKeyEvent, ClientKeyEventData, ElementReference>(
			(callback, _, eventData, elementRef) =>
			{
				SendKeys.KeyPressed += (_, args) =>
				{
					if (args.Key == eventData.key && args.Control.ElementReference.Equals(elementRef))
					{
						callback.Invoke();
						args.Handled = true;
					}
				};
			}
			);

		mockClientServices.FileService.Reset();
		mockClientServices.FileService
			.Setup(o => o.SaveFileToDirectoryAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>()))
			.ReturnsAsync((string fileName, IEnumerable<Dictionary<string, string[]>> fileTypes) =>
			{
				var fileReference = new Mock<IWritableBrowserFile>();
				fileReference
					.Setup(f => f.WriteAsync(It.IsAny<Stream>()))
					.Callback<Stream>(stream =>
					{
						using var fileStream = File.Create(fileName);
						stream.Flush();
						stream.Seek(0, SeekOrigin.Begin);
						stream.CopyTo(fileStream);
					});
				return fileReference.Object;
			});

		var mockRenderService = new Mock<IRenderService>();
		mockRenderService.Setup(o => o.ReplaceRenderedForm(It.IsAny<Form>())).Callback<Form>(f => ((IWinzorDispatcherContext)this).OpenForm(f));

		mockClientServices.ClientFileApi.Setup(s => s.GetFolderPathAsync(Environment.SpecialFolder.MyDocuments))
			.ReturnsAsync(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
		mockClientServices.ClientFileApi.Setup(s => s.WriteFileAsync(It.IsAny<string>(), It.IsAny<byte[]>()))
			.Callback(new InvocationAction(invocation =>
			{
				var fileName = (string)invocation.Arguments[0];
				var data = (byte[])invocation.Arguments[1];
				File.WriteAllBytes(fileName, data);
			}));
		mockClientServices.ClientFileApi.Setup(s => s.ReadFileAsync(It.IsAny<string>()))
			.ReturnsAsync((string path) => new MemoryStream(File.ReadAllBytes(path)));
	}

	public IRenderedComponent<ControlProxyComponent> Rendered { get; private set; }

	public void WaitForAllRenderTasks()
	{
		(renderTasks?.WaitAllAsync() ?? Task.CompletedTask).GetAwaiter().GetResult();
	}

	public void RegisterRenderTask(Task task)
	{
		renderTasks ??= new RenderTasks();
		renderTasks.Add(task);
	}

	void IWinzorDispatcherContext.NotifyRenderRequired(Control control)
	{
		// The concept of a Render in Winzor would be the equivalent of the WMPaint event in WinForms
		// This is triggered as a separate message on the message loop, meaning that it will both
		// 1. Be called after the current message has been processed
		// 2. Block any other messages from being processed until it has completed
		// 3. Not run at all (in a unit test) if Application.DoEvents (or some other mechanism to run the message loop) is called

		// Renders in Winzor are asynchronous, non blocking, and should be done on a separate render thread
		// However, for these unit tests we should try to recreate the WinForms structure as closely as possible

		// To achieve this, we will
		// 1) Not re-render controls immediately - instead we will queue a new WinzorDispatcher message to handle the re-render
		//		This will ensure that renders are only done when the unit test calls Application.DoEvents. If the test doesn't run the message loop,
		//		then the render will not be done at all (which is the expected behaviour)
		// 2) Ensure that the render is done on the WinzorDispatcher thread
		// 3) Ensure that the parts of the re-render which are normally done asynchronously block the WinzorDispatcher thread until completed

		// NOTE: This also has performance benefits which bring out unit tests inline with what is expected in the WinForms tests due to not calling paint/render code which doesn't ever run in WinForms tests

		// This method should only be called from the WinzorDispatcher thread
		// Not only is this a fair requirement but it also avoids needing to make this method thread-safe
		if (!WinzorDispatcher.IsCurrent)
		{
			return;
		}

		var winzorDispatcher = WinzorDispatcher.Current;

		if (control is IMocked)
		{
			// This control is a mock - we don't need to render it
			return;
		}

		if (controlsRequiringRender is null)
		{
			controlsRequiringRender = new HashSet<WeakReference<Control>>();

			// There is not a pending render task scheduled
			winzorDispatcher.InvokeAsync(() =>
			{
				using var ctx = winzorDispatcher.WithContext(this);

				var controlsToRenderLocal = Interlocked.Exchange(ref controlsRequiringRender, null);
				if (controlsToRenderLocal is null)
				{
					return;
				}

				var controlsToRender = controlsToRenderLocal
					.Select(c => c.TryGetTarget(out var control) ? control : null)
					.OfType<Control>()
					.ToHashSet();

				foreach (var controlToRender in controlsToRender)
				{
					if (controlToRender.IsDisposed || !controlToRender.HasRendered)
					{
						// No point in rerendering a control that doesn't need it
						continue;
					}

					// We will likely have a lot of nested controls that each need to be rendered.
					// As rendering the parent will rerender the children recursively we can skip
					// rendering a control directly if we know that it's parent will be rerendered.
					// This ends up being much faster as it saves us doing a lot of unnecessary renders.
					var parentIsRerendering = false;
					var parentControl = controlToRender.Parent;
					while (parentControl is not null)
					{
						if (controlsToRender.Contains(parentControl))
						{
							parentIsRerendering = true;
							break;
						}
						parentControl = parentControl.Parent;
					}

					if (parentIsRerendering)
					{
						continue;
					}

					controlToRender.OnBeforeRender();
					Task.Run(async () => await controlToRender.InvokeStateHasChangedAsync()).GetAwaiter().GetResult();
				}
			});
		}

		// We need to store a weak reference to control so that the unit test is still able to garbage collect the instance if desired
		controlsRequiringRender.Add(new WeakReference<Control>(control));
	}

	HashSet<WeakReference<Control>> controlsRequiringRender;

	void IWinzorDispatcherContext.OnEnterMessageLoop()
	{
	}

	public void InvokeRenderDispatcher(Func<Task> workItem)
	{
		RegisterRenderTask(winzorTestContext.Renderer.Dispatcher.InvokeAsync(workItem));
	}

	public void Dispose()
	{
		SendKeys.ClearKeyPressedHandlers();
	}

	readonly WinzorTestContext winzorTestContext;

	public Form Form => (Form)Rendered?.Instance?.Control;

	RenderTasks renderTasks;
}
