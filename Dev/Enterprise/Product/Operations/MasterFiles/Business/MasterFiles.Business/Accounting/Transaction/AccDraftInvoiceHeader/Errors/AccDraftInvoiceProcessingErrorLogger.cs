using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Accounting.ProcessLogging;

namespace Enterprise.MasterFiles.Business
{
	public class AccDraftInvoiceProcessingErrorLogger : IAccProcessLogger
	{
		public AccDraftInvoiceProcessingErrorLogger(IDbConnected dbConnected)
		{
			DbConnected = dbConnected;
		}

		IDbConnected DbConnected { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL parameters")]
		void IAccProcessLogger.Log(ISupportAccProcessLogging parent, ILogableError error)
		{
			DbConnected.Connection.ExecuteNonQuery(LogSQL, command => {
				command.AddParameter("@draftInvoioceHeader", SqlDbType.UniqueIdentifier, parent.ParentId.ToGuid());
				command.AddParameter("@code", SqlDbType.Char, 3, error.Code);
				command.AddParameter("@desc", SqlDbType.NText, error.Message);
				command.AddParameter("@currentTime", SqlDbType.SmallDateTime, 128, ZDateTime.UtcNow);
				command.AddParameter("@editor", SqlDbType.NVarChar, 128, (string)GlbStaff.CurrentUser.GS_Code);
			});
		}

		const string LogSQL = @"
MERGE INTO AccDraftInvoiceProcessingErrorLog
USING (VALUES (@draftInvoioceHeader, @code, @desc, @currentTime, @editor)) AS src (AIH_PK, [Code], [Desc], [CurrentTime], [Editor])
ON AIL_AIH_DraftInvoice = src.[AIH_PK] AND AIL_Code = src.[Code]
WHEN MATCHED THEN
	UPDATE 
	SET 
		AIL_FailCount = AIL_FailCount + 1,
		AIL_Description = [Desc],
		AIL_FixedDateTimeUtc = NULL,
		AIL_LastReportedTimeUtc = [CurrentTime],
		AIL_SystemLastEditTimeUtc = [CurrentTime],
		AIL_SystemLastEditUser = [Editor]
WHEN NOT MATCHED BY TARGET THEN
	INSERT (
		AIL_PK
		, AIL_Code
		, AIL_Description
		, AIL_FixedDateTimeUtc
		, AIL_LastReportedTimeUtc
		, AIL_FailCount
		, AIL_AIH_DraftInvoice
		, AIL_SystemCreateTimeUtc
		, AIL_SystemCreateUser
		, AIL_SystemLastEditTimeUtc
		, AIL_SystemLastEditUser)
	VALUES (
		NEWID()
		, [Code]
		, [Desc]
		, NULL
		, [CurrentTime]
		, 1
		, [AIH_PK]
		, [CurrentTime]
		, [Editor]
		, [CurrentTime]
		, [Editor]);
";
	}
}
