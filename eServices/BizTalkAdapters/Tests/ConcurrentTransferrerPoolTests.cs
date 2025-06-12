using System;
using System.Threading;
using CargoWise.eHub.BizTalkAdapters.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.BizTalkAdapters.Tests
{
	[TestClass()]
	public class ConcurrentTransferrerPoolTests : TestBase
	{
		[TestMethod()]
		public void ConcurrentTransferrerPool_ExceptionHandling_DisposeTransferrer()
		{
			var location = MockRepository.GenerateMock<TransferrerProperties.Receive.Location>();
			var properties = new TransferrerProperties.Receive("test://TestURI");

			var mockTransferrerFactory = MockRepository.GenerateMock<FtpTransferrerFactory>();
			var mockTransferrer = MockRepository.GenerateMock<ITransferrer>();

			mockTransferrerFactory.Expect(x => x.CreateTransferrer()).Return(mockTransferrer).Repeat.Once();
			mockTransferrer.Expect(x => x.Open()).Repeat.Once();
			mockTransferrer.Expect(x => x.Close()).Repeat.Never();
			mockTransferrer.Expect(x => x.Dispose()).Repeat.Once();
			mockTransferrerFactory.Expect(x => x.CreateTransferrer()).Return(mockTransferrer).Repeat.Once();
			mockTransferrer.Expect(x => x.Dispose()).Repeat.Once();

			using (var pool = new ConcurrentTransferrerPool(mockTransferrerFactory, location, properties, new CancellationToken(false), null))
			{
				AssertException<Exception>("Test Exception", () =>
					{
						pool.InvokeActionWithTransferrer((t2) => { throw new Exception("Test Exception"); });
					});
				pool.InvokeActionWithTransferrer((t2) => { });
			}

			mockTransferrerFactory.VerifyAllExpectations();
			mockTransferrer.VerifyAllExpectations();
		}
	}
}
