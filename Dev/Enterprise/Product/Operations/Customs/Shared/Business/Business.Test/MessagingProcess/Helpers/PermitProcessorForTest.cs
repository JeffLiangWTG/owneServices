using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business.BusinessObjects.CusPermit;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.Testing.MessagingProcess.Helpers
{
	internal class PermitProcessorForTest : ICusPermitCusDecProcessor<EDIMessage>
	{
		public ZString[] ErrorsForTest { get; set; } = Array.Empty<ZString>();
		public IEnumerable<ZString> ErrorList => ErrorsForTest;

		public bool AddPermitRecordsAndLockMutexIfNeededCalled { get; set; }
		public void AddPermitRecordsAndLockMutexIfNeeded()
		{
			AddPermitRecordsAndLockMutexIfNeededCalled = true;
		}

		public bool AddPermitTransactionsCalled { get; set; }
		public void AddPermitTransactions(EDIMessage parentBizO, Func<EDIMessage, ZString> getPermitAppId, string status = "PND")
		{
			AddPermitTransactionsCalled = true;
		}

		public bool UnlockPermitMutexesCalled { get; set; }
		public void UnlockPermitMutexes()
		{
			UnlockPermitMutexesCalled = true;
		}
	}
}
