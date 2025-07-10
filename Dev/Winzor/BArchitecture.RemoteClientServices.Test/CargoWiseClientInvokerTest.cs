using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using Enterprise.Winzor.Architecture.Test;
using NUnit.Framework;

namespace WinzorFramework.RemoteClientServices.Test
{
	class CargoWiseClientInvokerTest
	{
		static IEnumerable<TestCaseData> Actions
		{
			get
			{
				yield return new TestCaseData(() =>
				{
					var result = CargoWiseClientInvoker.Invoke(async (cts) => await Task.FromResult(true));
					Assert.That(result, Is.True);
				}) { TestName = "{m}_InvokeWithReturn" };

				yield return new TestCaseData(() =>
				{
					var result = false;
					CargoWiseClientInvoker.Invoke(async (cts) =>
					{
						result = true;
						await Task.CompletedTask;
					});

					Assert.That(result, Is.True);
				}) { TestName = "{m}_InvokeVoid" };
			}
		}

		[TestCaseSource(nameof(Actions))]
		public async Task RunInWinzorDispatcherSuccessAsync(Action action)
		{
			var form = default(Form);
			using var ctx = new EnterpriseTestContext();
			await ctx.RenderEntryPointComponent(() =>
			{
				form = new Form();
				return form;
			},
			ctx.DefaultClientServices.WindowService);

			var task = form.InvokeWinzorDispatcherAsync(() =>
			{
				Assert.That(() => action(), Throws.Nothing);
			});

			await Task.WhenAny(task, Task.Delay(2000));

			Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
		}

		[TestCaseSource(nameof(Actions))]
		public async Task RunInNonWinzorDispatcherFindsAnAvailableFormAsync(Action action)
		{
			var form = default(Form);
			using var ctx = new EnterpriseTestContext();
			await ctx.RenderEntryPointComponent(() =>
			{
				form = new Form();
				return form;
			},
			ctx.DefaultClientServices.WindowService);

			var innerTask = default(Task);
			var task = form.InvokeWinzorDispatcherAsync(() =>
			{
				innerTask = Task.Run(() =>
				{
					Assert.DoesNotThrow(() => action());
				});
			});

			while (innerTask == null)
			{
			}

			await Task.WhenAny(innerTask, Task.Delay(2000));
			Assert.That(innerTask.Status, Is.EqualTo(TaskStatus.RanToCompletion));

			await task;
			Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
		}

		[TestCaseSource(nameof(Actions))]
		public async Task RunInWinzorSynchronizationContextSuccessAsync(Action action)
		{
			var form = default(Form);
			var control = default(Control);
			using var ctx = new EnterpriseTestContext();
			await ctx.RenderEntryPointComponent(
				() =>
				{
					form = new Form();
					control = new Control();
					form.Controls.Add(control);
					return form;
				},
				ctx.DefaultClientServices.WindowService);

			var innerTask = default(Task);
			var task = form.InvokeWinzorDispatcherAsync(() =>
			{
				var context = SynchronizationContext.Current;
				innerTask = Task.Run(
					() =>
					{
						SynchronizationContext.SetSynchronizationContext(context);
						Assert.That(() => action(), Throws.Nothing);
					});
			});

			while (innerTask == null)
			{
			}

			await Task.WhenAny(innerTask, Task.Delay(2000));
			Assert.That(innerTask.Status, Is.EqualTo(TaskStatus.RanToCompletion));

			await task;
			Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
		}

		[Test]
		public async Task InvokeFromWinzorDispatcherDoesNotDeadlockAsync()
		{
			var form = default(Form);
			using var ctx = new EnterpriseTestContext();
			await ctx.RenderFormAsync(() =>
			{
				form = new Form();
				return form;
			});
			var dispatcherInvokeCts = new TaskCompletionSource();
			var clientInvokeCts = new TaskCompletionSource();
			var clientTask = form.InvokeWinzorDispatcherAsync(() =>
			{
				CargoWiseClientInvoker.Invoke(async _ =>
				{
					await dispatcherInvokeCts.Task;
					clientInvokeCts.SetResult();
				});
			});
			await form.InvokeWinzorDispatcherAsync(() =>
			{
				dispatcherInvokeCts.SetResult();
			});
			await clientInvokeCts.Task;
			await clientTask;
		}

		class DisposalTestForm : Form
		{
			public TaskCompletionSource<bool> TaskCompletionSource { get; }

			public DisposalTestForm(TaskCompletionSource<bool> taskCompletionSource)
			{
				TaskCompletionSource = taskCompletionSource;
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing && !IsDisposed)
				{
					CargoWiseClientInvoker.Invoke(async cws =>
					{
						await Task.Delay(500);
					});

					base.Dispose(disposing);

					TaskCompletionSource.SetResult(true);
				}
			}
		}

		[Test]
		public async Task InvokeFromWinzorDispatcherDoesNotDeadlockOnFormDisposeAsync()
		{
			var workItemForm = default(Form);
			using var ctx = new EnterpriseTestContext();
			var tcs = new TaskCompletionSource<bool>();

			await ctx.RenderFormAsync(() =>
			{
				var mainForm = new Form();
				return mainForm;
			});

			await ctx.RenderFormAsync(() =>
			{
				workItemForm = new DisposalTestForm(tcs);
				return workItemForm;
			});

			await workItemForm.InvokeWinzorDispatcherAsync(() =>
			{
				workItemForm.Close();
			});

			await Assert.ThatAsync(async () => await tcs.Task, Is.True);
		}

		[Test]
		public async Task InvokeFromNonWinzorDispatcherThreadDoesNotInvokeWinzorDispatcherAsync()
		{
			var form = default(Form);
			var synchronizationContext = default(SynchronizationContext);
			using var ctx = new EnterpriseTestContext();
			await ctx.RenderFormAsync(() =>
			{
				form = new Form();
				synchronizationContext = SynchronizationContext.Current;
				return form;
			});
			var clientInvokeCts = new TaskCompletionSource();
			var dispatcherTask = form.InvokeWinzorDispatcherAsync(clientInvokeCts.Task.Wait);
			var thread = new Thread(() =>
			{
				SynchronizationContext.SetSynchronizationContext(synchronizationContext);
				CargoWiseClientInvoker.Invoke(_ =>
				{
					clientInvokeCts.SetResult();
					return Task.CompletedTask;
				});
			});
			thread.Start();
			thread.Join();
			await dispatcherTask;
		}

		[Test]
		public async Task InvokeFromContextControlRemovedFromFormAsync()
		{
			var form = default(Form);
			var button = default(Button);
			var invoked = false;
			using var ctx = new EnterpriseTestContext();
			var rendered = await ctx.RenderFormAsync(() =>
			{
				form = new Form();
				button = new Button();
				button.Text = "Click";
				button.Click += Button_Click;
				form.Controls.Add(button);
				return form;
			});

			using var ctx2 = new EnterpriseTestContext();
			await ctx2.RenderFormAsync(() => new Form());

			await form.InvokeWinzorDispatcherAsync(() => form.Activate());
			await rendered.Find("button").ClickAsync(new WebMouseEventArgs());

			Assert.That(invoked, Is.True);

			void Button_Click(object sender, EventArgs e)
			{
				form.Controls.Remove(button);
				CargoWiseClientInvoker.Invoke(_ =>
				{
					invoked = true;
					return Task.CompletedTask;
				});
			}
		}

		[Test]
		public async Task InvokeFromClosingFormAsync()
		{
			var form = default(Form);
			var button = default(Button);
			var invoked = false;
			using var ctx = new EnterpriseTestContext();
			var rendered = await ctx.RenderFormAsync(() =>
			{
				form = new Form();
				button = new Button();
				button.Text = "Click";
				button.Click += Button_Click;
				form.Controls.Add(button);
				return form;
			});

			using var ctx2 = new EnterpriseTestContext();
			await ctx2.RenderFormAsync(() => new Form());

			await form.InvokeWinzorDispatcherAsync(() => form.Activate());
			await rendered.Find("button").ClickAsync(new WebMouseEventArgs());

			Assert.That(invoked, Is.True);

			void Button_Click(object sender, EventArgs e)
			{
				form.Close();
				CargoWiseClientInvoker.Invoke(_ =>
				{
					invoked = true;
					return Task.CompletedTask;
				});
			}
		}

		[Test]
		public async Task InvokeFromClosingWithNoOpenedFormsAsync()
		{
			var form = default(Form);
			var button = default(Button);
			using var ctx = new EnterpriseTestContext();
			var rendered = await ctx.RenderFormAsync(() =>
			{
				form = new Form();
				button = new Button();
				button.Text = "Click";
				button.Click += Button_Click;
				form.Controls.Add(button);
				return form;
			});

			await rendered.Find("button").ClickAsync(new WebMouseEventArgs());

			void Button_Click(object sender, EventArgs e)
			{
				form.Close();
				try
				{
					CargoWiseClientInvoker.Invoke(_ =>
					{
						return Task.CompletedTask;
					});
				}
				catch (InvalidOperationException ex)
				{
					Assert.That(ex.Message, Is.EqualTo("Form is unavailable"));
				}
			}
		}

		[Test]
		public async Task TestInvokeCapturesAndThrowsExceptionAsync()
		{
			var form = default(Form);
			using var ctx = new EnterpriseTestContext();
			await ctx.RenderFormAsync(() =>
			{
				form = new Form();
				return form;
			});

			var exception = Assert.Throws<ArgumentException>(() => CargoWiseClientInvoker.Invoke(_ => throw new ArgumentException("test")));
			Assert.That(exception.Message, Is.EqualTo("test"));
		}
	}
}
