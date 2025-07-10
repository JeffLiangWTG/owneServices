using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Accounting.ProcessLogging;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AccDraftInvoiceProcessingErrorLoggerTest : TestCaseWithFactory
	{
		[TestDate]
		public void TestLog()
		{
			var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			draftInvoice.AIH_RX_NKTransactionCurrency = "AUD";
			draftInvoice.AIH_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			AssertEquals("PreCondition, no error log in DB before calling Log method"
				, 0
				, Factory.Load<AccDraftInvoiceProcessingErrorLog>(GetQueryForError(draftInvoice.PK.ToGuid(), AccDraftInvoiceProcessingErrorCodes.CreditorNotProvided)).Length
			);

			TestDateAttribute.Date = new DateTime(2025, 06, 03, 0, 0, 0, DateTimeKind.Utc);
			Logger.Log(draftInvoice, CreateError(AccDraftInvoiceProcessingErrorCodes.CreditorNotProvided, "DummyMsg1"));
			AssertErrorLogInDB(draftInvoice.PK.ToGuid(), AccDraftInvoiceProcessingErrorCodes.CreditorNotProvided
				, "DummyMsg1", "2025-06-03 00:00:00", 1);

			SetErrorBeFixed(draftInvoice.PK.ToGuid(), AccDraftInvoiceProcessingErrorCodes.CreditorNotProvided);

			TestDateAttribute.Date = new DateTime(2025, 06, 03, 05, 10, 00, DateTimeKind.Utc);
			Logger.Log(draftInvoice, CreateError(AccDraftInvoiceProcessingErrorCodes.CreditorNotProvided, "DummyMsg2"));
			AssertErrorLogInDB(draftInvoice.PK.ToGuid(), AccDraftInvoiceProcessingErrorCodes.CreditorNotProvided
				, "DummyMsg2", "2025-06-03 05:10:00", 2);

			void AssertErrorLogInDB(Guid parentId, string errorCode, string expectedDesc, string expectedLastReportTime, int expectedFailCount)
			{
				var errorsInDB = Factory.Load<AccDraftInvoiceProcessingErrorLog>(GetQueryForError(parentId, errorCode));
				AssertEquals("PreCondition, only one result for each error code in same draft invoice.", 1, errorsInDB.Length);

				var errorInDB = errorsInDB.First();
				AssertEquals("AIL_Description", expectedDesc, errorInDB.AIL_Description);
				AssertEquals("AIL_FailCount", expectedFailCount, errorInDB.AIL_FailCount);
				AssertEquals("AIL_LastReportedTimeUtc", expectedLastReportTime, errorInDB.AIL_LastReportedTimeUtc.ToString("yyyy-MM-dd HH:mm:ss"));
				AssertEquals("AIL_FixedDateTimeUtc", ZDateTime.Empty, errorInDB.AIL_FixedDateTimeUtc);
			}
		}

		void SetErrorBeFixed(Guid parentId, string errorCode)
		{
			var errorsInDB = Factory.Load<AccDraftInvoiceProcessingErrorLog>(GetQueryForError(parentId, errorCode));
			errorsInDB.First().AIL_FixedDateTimeUtc = ZDateTime.UtcNow;

			Factory.Save();

			AssertNotEquals("PreCondition, FixedDate is set successfully."
				, ZDateTime.Empty
				, Factory.Load<AccDraftInvoiceProcessingErrorLog>(GetQueryForError(parentId, errorCode)).First().AIL_FixedDateTimeUtc
			);
		}

		ZDBOnlyQuery GetQueryForError(Guid parentId, string errorCode)
		{
			var query = new ZDBOnlyQuery(typeof(AccDraftInvoiceProcessingErrorLog))
			{
				IgnoreDbQueryCache = true,
				ReLoadExistingRows = true,
			};
			query.AddToFilter(AccDraftInvoiceProcessingErrorLogSchema.AIL_AIH_DraftInvoice, parentId);
			query.AddToFilter(AccDraftInvoiceProcessingErrorLogSchema.AIL_Code, errorCode);

			return query;
		}

		ILogableError CreateError(string code, string desc)
		{
			var mockedError = new Mock<ILogableError>();
			mockedError.Setup(x => x.Code).Returns(code);
			mockedError.Setup(x => x.Message).Returns(desc);

			return mockedError.Object;
		}

		IAccProcessLogger Logger => (logger ??= new AccDraftInvoiceProcessingErrorLogger(Factory));
		AccDraftInvoiceProcessingErrorLogger logger;
	}
}
