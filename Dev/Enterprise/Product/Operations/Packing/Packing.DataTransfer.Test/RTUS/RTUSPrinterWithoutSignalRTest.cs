using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Packing.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.RTUS.Interface;

namespace Enterprise.Packing.DataTransfer.Testing
{
	class RTUSPrinterWithoutSignalRTest : PackingTestCaseWithFactory
	{
		[TestDate(2020, 1, 1, 10, 10, 10)]
		[TestUtcOffset(10, 0, 0)]
		public void TestPrint()
		{
			var printQueue = Factory.New<IStmPrintQueue>();
			Factory.Save();

			AssertEquals(true, new RTUSPrinterWithoutSignalR(null).Print(FileType.PDF, new[] { (byte)0xA, (byte)0xB }, printQueue.PK.ToGuid()));

			var query = new ZQuery(StmPrintJobSchema.SP_SQ, printQueue.PK);
			var otherFactory = new BusinessObjectFactory();
			var printJob = otherFactory.Load<IStmPrintJob>(query).Single();
			AssertEquals(nameof(printJob.SP_Copies), (ZShort)1, printJob.SP_Copies);
			AssertArrayEqualsByElements(nameof(printJob.SP_CustomProperties), new[] { (byte)0xA, (byte)0xB }, printJob.SP_CustomProperties);
			AssertEquals(nameof(printJob.SP_EmailAttachments), "Label.PDF", printJob.SP_EmailAttachments);
			AssertEquals(nameof(printJob.SP_JobType), "PRN", printJob.SP_JobType);
			AssertEquals(nameof(printJob.SP_RunDateTime), new ZDateTime(2020, 1, 1, 10, 10, 10), printJob.SP_RunDateTime);

			var deliveryGroup = otherFactory.Load<IStmDeliveryGroup>(printJob.SP_SB_DeliveryGroup);
			AssertEquals(nameof(deliveryGroup.SB_IsProcessed), true, deliveryGroup.SB_IsProcessed);
		}

		public void TestPrint_WithHandledException()
		{
			Exception thrownException = null;

			AssertEquals(false, new RTUSPrinterWithoutSignalR(OnError).Print(FileType.PDF, new[] { (byte)0xA, (byte)0xB }, Guid.NewGuid()));
			AssertContains("The INSERT statement conflicted with the FOREIGN KEY constraint \"StmPrintJob__FK2_StmPrintQueue_CRR_120N\".", thrownException.Message);

			void OnError(Exception exception)
			{
				thrownException = exception;
			}
		}

		public void TestPrint_WithUnhandledException()
		{
			try
			{
				new RTUSPrinterWithoutSignalR(null).Print(FileType.PDF, new[] { (byte)0xA, (byte)0xB }, Guid.NewGuid());
				Fail("Exception should be thrown.");
			}
			catch (ZSaveException ex)
			{
				AssertContains("The INSERT statement conflicted with the FOREIGN KEY constraint \"StmPrintJob__FK2_StmPrintQueue_CRR_120N\".", ex.Message);
			}
		}

		public void TestPrint_WithNameAndServer()
		{
			AssertExceptionThrown(typeof(NotSupportedException), "Non-SignalR printer must use printer PK.",
				() => new RTUSPrinterWithoutSignalR(null).Print(FileType.PDF, new[] { (byte)0xA, (byte)0xB }, "PRINTER", "SERVER"));
		}
	}
}
