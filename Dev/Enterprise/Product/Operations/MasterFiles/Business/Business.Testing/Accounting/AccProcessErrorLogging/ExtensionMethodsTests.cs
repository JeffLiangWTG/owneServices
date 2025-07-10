using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Accounting.ProcessLogging;
using Moq;

namespace Enterprise.MasterFiles.Business.AccProcessErrorLogging.Testing
{
	public class ExtensionMethodsTests : TestCaseWithFactory
	{
		public void TestlogAccCriticalValidationException()
		{
			var criticalValidationException = new CannotSaveAfterCriticalErrorException("Forced Critical Validation Exception");
			var logged = false;
			var logger = new Mock<IAccProcessLogger>();
			logger.Setup(lg => lg.Log(It.IsAny<ISupportAccProcessLogging>(), It.IsAny<ILogableError>())).Callback(() => logged = true);

			var supporter = new Mock<ISupportAccProcessLogging>();
			supporter.Setup(s => s.Logger).Returns(logger.Object);
			supporter.Setup(s => s.ShouldLog).Returns(true);

			supporter.Object.LogAccCriticalValidationException(criticalValidationException);
			Assert("Should be logged", logged);
		}

		public void TestLogException()
		{
			var systemException = new InvalidCastException("Forced invalid cast exception");
			var logged = false;
			var logger = new Mock<IAccProcessLogger>();
			logger.Setup(lg => lg.Log(It.IsAny<ISupportAccProcessLogging>(), It.IsAny<ILogableError>())).Callback(() => logged = true);

			var supporter = new Mock<ISupportAccProcessLogging>();
			supporter.Setup(s => s.Logger).Returns(logger.Object);
			supporter.Setup(s => s.ShouldLog).Returns(true);

			supporter.Object.LogException(systemException);
			Assert("Should be logged", logged);
		}

		public void TestLogError()
		{
			var error = new LogableBusinessError("ZAA", "Test Error Message", "ZZZ", "");
			var logged = false;
			var logger = new Mock<IAccProcessLogger>();
			logger.Setup(lg => lg.Log(It.IsAny<ISupportAccProcessLogging>(), It.IsAny<ILogableError>())).Callback(() => logged = true);

			var supporter = new Mock<ISupportAccProcessLogging>();
			supporter.Setup(s => s.Logger).Returns(logger.Object);
			supporter.Setup(s => s.ShouldLog).Returns(true);

			supporter.Object.LogError(error);
			Assert("Should be logged", logged);
		}
	}
}
