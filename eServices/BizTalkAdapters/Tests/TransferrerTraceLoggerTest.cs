using System;
using CargoWise.eHub.BizTalkAdapters.Common;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.BizTalkAdapters.Tests
{
	[TestClass()]
	public class TransferrerTraceLoggerTest
	{
		const string LOG_PREFIX = "(TransferrerTraceLogger        ) ";

		[TestMethod()]
		public void TransferrerTraceLogger_WriteLineTest()
		{
			TransferrerTraceLogger target = new TransferrerTraceLogger();
			string message = "Just write this";
			var mockLogger = MockRepository.GenerateStrictMock<ILog>();
			mockLogger.Expect(l => l.IsDebugEnabled).Return(true).Repeat.Twice();
			mockLogger.Expect(l => l.Debug(LOG_PREFIX + message));
			target.Logger = mockLogger;

			target.WriteLine(message);

			mockLogger.VerifyAllExpectations();
		}

		[TestMethod()]
		public void TransferrerTraceLogger_WriteLineBlankTest()
		{
			TransferrerTraceLogger target = new TransferrerTraceLogger();
			var mockLogger = MockRepository.GenerateStrictMock<ILog>();
			mockLogger.Expect(l => l.IsDebugEnabled).Return(true).Repeat.Twice();
			mockLogger.Expect(l => l.Debug(LOG_PREFIX));
			target.Logger = mockLogger;

			target.WriteLine(String.Empty);

			mockLogger.VerifyAllExpectations();
		}

		[TestMethod()]
		public void TransferrerTraceLogger_WriteTest()
		{
			TransferrerTraceLogger target = new TransferrerTraceLogger();
			string message1 = "Write this";
			string message2 = "then this";
			var mockLogger = MockRepository.GenerateStrictMock<ILog>();
			mockLogger.Expect(l => l.IsDebugEnabled).Return(true).Repeat.Times(3);
			mockLogger.Expect(l => l.Debug(LOG_PREFIX + message1));
			mockLogger.Expect(l => l.Debug(LOG_PREFIX + message2));
			target.Logger = mockLogger;

			target.Write(message1 + Environment.NewLine + message2 + Environment.NewLine);

			mockLogger.VerifyAllExpectations();
		}
	}
}
