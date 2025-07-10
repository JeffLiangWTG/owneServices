using System;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class ExceptionReportingFormManagerTest
{
	[Test]
	public void FormNotShownWhenCalledFromNonWinzorThread()
	{
		var manager = new ExceptionReportingFormManager();
		ExceptionReportArgs sendErrorReportArgs = null;
		Assert.That(() => manager.ShowReportForm(new Exception("fail"), "whatever", null, null, true, a => sendErrorReportArgs = a), Throws.Nothing);
		Assert.That(sendErrorReportArgs, Is.Not.Null);
		Assert.That(sendErrorReportArgs.IsSlient, Is.True);
	}
}
