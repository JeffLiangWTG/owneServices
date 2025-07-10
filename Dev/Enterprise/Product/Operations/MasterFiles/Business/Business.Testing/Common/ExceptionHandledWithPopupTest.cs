using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class ExceptionHandledWithPopupTest : TestCase
	{
		public void TestImplementsIExceptionReporterExtender()
		{
			var exception = GetExceptionInstance();
			var exceptionReporterExtender = exception as IExceptionReporterExtender;
			AssertNotNull("Implements IExceptionReporterExtender", exceptionReporterExtender);
			Assert("Handled", exceptionReporterExtender.HandleException());
			AssertEquals("Message displayed to user", exception.Message, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		protected abstract ExceptionHandledWithPopup GetExceptionInstance();
	}
}