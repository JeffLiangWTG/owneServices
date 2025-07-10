using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Microsoft.AspNetCore.Components;
using Moq;
using NUnit.Framework;
using WinzorFramework;

namespace Enterprise.Winzor.Architecture.Test;

class ExceptionReporterUIHooksTest
{
	[Test]
	public async Task OnUnhandledExceptionDuringDatabaseUpgradeShouldBeSwallowed()
	{
		using var ctx = new EnterpriseTestContext();
		// Render a form incase it's needed by the ServerInitiatedCallbackContext.
		var rendered = await ctx.RenderFormAsync(() => new Form());

		IDisposable productRegistrationSubstitute = null;
		IDisposable isUnitTestingProductionFunctionality = null;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			// Simulate a database upgrade when trying to get the error report ID.
			var mockProductRegistration = new Mock<IProductRegistration>();
			mockProductRegistration.Setup(x => x.Key).Throws(new DatabaseUpgradeInProgressException());
			productRegistrationSubstitute = ObjectFactory.Substitute(mockProductRegistration.Object);

			// Simulate a production environment to avoid test specific exception handling.
			isUnitTestingProductionFunctionality = Globals.SetIsUnitTestingProductionFunctionality();
			ExceptionReporter.Instance.TestingDoReportException.Value = true;

			Application.ThreadException += ThrowExceptionOnThreadException;
		});

		try
		{
			Assert.That(async () => await ctx.WinzorDispatcher.InvokeAsync(() =>
			{
				// Run inside a ServerInitiatedCallbackContext to emulate code running in a WinForms timer callback.
				var context = new ServerInitiatedCallbackContext();
				using (ctx.WinzorDispatcher.WithContext(context))
				{
					// Attempt to report an unhandled exception. We cannot gracefully handle this exception due to the simulated database upgrade.
					// We need to ensure that this doesn't rethrow an exception, as that will result in an unobserved task exceptions being reported.
					Application.OnThreadException(new InvalidOperationException("This is an exception!"));
				}
			}), Throws.Nothing);
		}
		finally
		{
			await ctx.WinzorDispatcher.InvokeAsync(() =>
			{
				// Reset all the test specific settings.
				Application.ThreadException -= ThrowExceptionOnThreadException;
				productRegistrationSubstitute.Dispose();
				isUnitTestingProductionFunctionality.Dispose();
				ExceptionReporter.Instance.TestingDoReportException.ResetValue();
			});
		}
	}

	void ThrowExceptionOnThreadException(object sender, ThreadExceptionEventArgs e)
	{
		throw new Exception("New exception");
	}

	[Test]
	public async Task TestFormCallbackExceptionsAreHandled()
	{
		Form form = null;
		ExceptionReporter.Instance.Enable();
		ExceptionReporterTestListener.Instance.Clear();
		using var ctx = new EnterpriseTestContext();
		await ctx.RenderFormAsync(() => {
			form = new Form();
			return form;
		});

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.Close();
			throw new Exception("FAIL");
		});

		Assert.That(ExceptionReporterTestListener.Instance.Count, Is.EqualTo(1));
		Assert.That(ExceptionReporterTestListener.Instance[0].Message, Is.EqualTo("FAIL"));
		ExceptionReporterTestListener.Instance.Clear();
	}
}
