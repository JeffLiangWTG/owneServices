using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business.BatchProcessor.Testing
{
	sealed class BaseCustomsStmProcessQueueBatchProcessorTest : TestCaseWithFactory
	{
		public void TestProcessQueuedItem()
		{
			var queuedItem = Factory.New<StmProcessQueue>();
			queuedItem.SW_ApplicationCode = "~T~";
			queuedItem.SW_JobTypeCode = "CUS";
			queuedItem.SW_ActionCode = "~A";
			queuedItem.SW_ReferenceID = Guid.NewGuid();
			queuedItem.SW_ReferenceTableCode = "Z0";
			queuedItem.SW_PostedTimeUtc = ZDateTime.Today;
			Factory.Save();

			var processor = new BaseCustomsStmProcessQueueProcessorForTesting(new LoggingInformation());
			processor.ExecuteBatch();

			var logs = new ZStringBuilder();
			var enumerator = processor.Logger.UserLogStrings.GetEnumerator();
			while (enumerator.MoveNext())
			{
				logs.Append(enumerator.Current.Trim());
			}

			AssertContains(@"Item has been processed.", logs.ToString());

			var newFactory = new BusinessObjectFactory();
			AssertNull(newFactory.Load<StmProcessQueue>(queuedItem.PK));
		}
	}
}
