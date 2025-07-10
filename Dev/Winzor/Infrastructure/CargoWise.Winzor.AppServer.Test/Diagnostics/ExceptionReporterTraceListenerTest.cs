using System;
using System.Collections.Generic;
using System.Diagnostics;
using CargoWise.Winzor.AppServer.Diagnostics;
using NUnit.Framework;

namespace CargoWise.Winzor.AppServer.Test.Diagnostics;

class ExceptionReporterTraceListenerTest
{
	[Test]
	public void WhenPropagateErrorIsFalse()
	{
		using var uut = new ExceptionReporterTraceListener((_) => { }, propagateError: false);

		Assert.DoesNotThrow(() => uut.Fail("Message error"));
	}

	[Test]
	public void WhenPropagateErrorIsFalse_WhenDetailMessageIsInformed()
	{
		using var uut = new ExceptionReporterTraceListener((_) => { }, propagateError: false);

		Assert.DoesNotThrow(() => uut.Fail("Message error", detailMessage: "Detail error message"));
	}

	[Test]
	public void WhenPropagateErrorIsFalse_WhenReportHandlerThrows()
	{
		var reachHandler = false;
		Action<Exception> handler = (_) => {
			reachHandler = true;
			throw new Exception("Runtime error");
		};
		using var uut = new ExceptionReporterTraceListener(handler, propagateError: false);

		Assert.DoesNotThrow(() => uut.Fail("Message error", detailMessage: "Detail error message"));
		Assert.That(reachHandler, Is.True);
	}

	[Test]
	public void WhenPropagateErrorIsTrue()
	{
		Exception exceptionReported = null;
		using var uut = new ExceptionReporterTraceListener((ex) => { exceptionReported = ex; }, propagateError: true);

		var ex = Assert.Throws<Exception>(() => uut.Fail("Message error"));

		Assert.Multiple(() => {
			Assert.That(ex.Message, Is.EqualTo("Assert failure: 'Message error'"));
			Assert.That(ex.Data["message"], Is.EqualTo("Message error"));
			Assert.That(ex.Data["detailMessage"], Is.Empty);

			Assert.That(exceptionReported?.Message, Is.EqualTo("Assert failure: 'Message error'"));
			Assert.That(exceptionReported?.Data["message"], Is.EqualTo("Message error"));
			Assert.That(exceptionReported?.Data["detailMessage"], Is.Empty);
		});
	}

	[Test]
	public void WhenPropagateErrorIsTrue_WhenDetailMessageIsInformed()
	{
		Exception exceptionReported = null;
		using var uut = new ExceptionReporterTraceListener((ex) => { exceptionReported = ex; }, propagateError: true);

		var ex = Assert.Throws<Exception>(() => uut.Fail("Message error", "Detail error message"));

		Assert.Multiple(() => {
			Assert.That(ex.Message, Is.EqualTo("Assert failure: 'Message error'"));
			Assert.That(ex.Data["message"], Is.EqualTo("Message error"));
			Assert.That(ex.Data["detailMessage"], Is.EqualTo("Detail error message"));

			Assert.That(exceptionReported?.Message, Is.EqualTo("Assert failure: 'Message error'"));
			Assert.That(exceptionReported?.Data["message"], Is.EqualTo("Message error"));
			Assert.That(exceptionReported?.Data["detailMessage"], Is.EqualTo("Detail error message"));
		});
	}

	[Test]
	public void WhenPropagateErrorIsTrue_WhenReportHandlerThrows()
	{
		var reachHandler = false;
		Action<Exception> handler = (_) => {
			reachHandler = true;
			throw new Exception("Runtime error");
		};
		using var uut = new ExceptionReporterTraceListener(handler, propagateError: true);

		var ex = Assert.Throws<Exception>(() => uut.Fail("Message error"));

		Assert.Multiple(() => {
			Assert.That(ex.Message, Is.EqualTo("Assert failure: 'Message error'"));
			Assert.That(ex.Data["message"], Is.EqualTo("Message error"));
			Assert.That(ex.Data["detailMessage"], Is.Empty);

			Assert.That(reachHandler, Is.True);
		});
	}

	[Test]
	public void WhenPropagateErrorIsFalse_WhenDebugAssertCheckFails()
	{
		Exception actualEx = null!;
		Action<Exception> handler = (ex) => actualEx = ex;

		using var uut = new ExceptionReporterTraceListener(handler, propagateError: false);

		var currentListeners = new List<TraceListener>();
		for (var i = 0; i < Trace.Listeners.Count; i++)
		{
			currentListeners.Add(Trace.Listeners[i]);
		}

		try
		{
			Trace.Listeners.Clear();
			Trace.Listeners.Add(uut);

			Debug.Assert(false, "My message");

			Assert.That(actualEx, Is.Not.Null);

			Assert.Multiple(() => {
				Assert.That(actualEx.Message, Is.EqualTo("Assert failure: 'My message'"));
				Assert.That(actualEx.Data["message"], Is.EqualTo("My message"));
				Assert.That(actualEx.Data["detailMessage"], Is.Empty);
			});
		}
		finally
		{
			Trace.Listeners.Clear();
			foreach (var listener in currentListeners)
			{
				Trace.Listeners.Add(listener);
			}
		}
	}

	[Test]
	public void WhenPropagateErrorIsFalse_WhenTraceAssertCheckFails()
	{
		Exception actualEx = null!;
		Action<Exception> handler = (ex) => actualEx = ex;

		using var uut = new ExceptionReporterTraceListener(handler, propagateError: false);

		var currentListeners = new TraceListener[Trace.Listeners.Count];
		Trace.Listeners.CopyTo(currentListeners,0);

		try
		{
			Trace.Listeners.Clear();
			Trace.Listeners.Add(uut);

			Trace.Assert(false, "My message");

			Assert.That(actualEx, Is.Not.Null);

			Assert.Multiple(() => {
				Assert.That(actualEx.Message, Is.EqualTo("Assert failure: 'My message'"));
				Assert.That(actualEx.Data["message"], Is.EqualTo("My message"));
				Assert.That(actualEx.Data["detailMessage"], Is.Empty);
			});
		}
		finally
		{
			Trace.Listeners.Clear();
			Trace.Listeners.AddRange(currentListeners);
		}
	}
}
