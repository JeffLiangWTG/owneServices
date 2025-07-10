using System;
using System.Data;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GUI.Testing
{
	public partial class TestRigEDIMessageCreationUserControl : UserControl
	{
		public TestRigEDIMessageCreationUserControl()
		{
			InitializeComponent();
			InterchangeCustomsRerenceTextBox.Text = "CUSTOMSTEST";
			InterchangeCW1ReferenceTextBox.Text = GlbCompany.CurrentCompany?.LicenceKeyIdentifier ?? "";
		}

		void CreateButton_Click(object sender, EventArgs e)
		{
			if (Globals.IsUserInteractive)
			{
				var applicationCode = new ZString(ApplicationCodeTextBox.Text.ToUpperInvariant()).Left(3);
				var messageType = new ZString(MessageTypeTextBox.Text.ToUpperInvariant()).Left(3);
				var messageText = MessageTextTextBox.Text;
				if (applicationCode.IsEmpty || messageType.IsEmpty || string.IsNullOrEmpty(messageText))
				{
					Globals.Message.ShowError("Please make sure that both Application Code, Message Type and Message Text are filled in.");
					return;
				}
				var createBothMessageAndInterchange = CreateBothMessageAndInterchangeRadioButton.Checked;
				var createMessage = createBothMessageAndInterchange || CreateMessageRadioButton.Checked;
				var createInterchange = createBothMessageAndInterchange || CreateInterchangeRadioButton.Checked;
				var createBothOutgoingAndIncoming = CreateBothOutgoingAndIncomingRadioButton.Checked;
				var createOutgoing = createBothOutgoingAndIncoming || CreateOutgoingRadioButton.Checked;
				var createIncoming = createBothOutgoingAndIncoming || CreateIncomingRadioButton.Checked;
				var branchPKText = UseJobBranchCheckBox.Checked
					? "JE_GB"
					: "ISNULL((SELECT TOP 1 GB_PK FROM dbo.GlbBranch WHERE JE_GC = GB_GC AND JE_GB != GB_PK AND GB_IsActive = 1), JE_GB)";
				var sqlText = $@"
SET NOCOUNT ON;
DECLARE @TotalJobs BIGINT = 0;

BEGIN TRANSACTION;
BEGIN TRY
	DECLARE @DeclarationTableVar TABLE (ID BIGINT IDENTITY(1, 1), JE_PK UNIQUEIDENTIFIER, JE_GB UNIQUEIDENTIFIER, OutgoingEI_PK UNIQUEIDENTIFIER, OutgoingEM_PK UNIQUEIDENTIFIER, IncomingEI_PK UNIQUEIDENTIFIER, IncomingEM_PK UNIQUEIDENTIFIER);

	INSERT @DeclarationTableVar (JE_PK, JE_GB, OutgoingEI_PK, OutgoingEM_PK, IncomingEI_PK, IncomingEM_PK)
	SELECT JE_PK, {branchPKText} AS JE_GB, NEWID(), NEWID(), NEWID(), NEWID()
	FROM dbo.JobDeclaration
	WHERE (@FromCreateDate IS NULL OR JE_SystemCreateTimeUtc >= @FromCreateDate) AND (@ToCreateDate IS NULL OR JE_SystemCreateTimeUtc < @ToCreateDate)

	DECLARE @TotalNoOfMsg BIGINT = @@ROWCOUNT * @NoOfMsgPerJob;
	IF (@TotalNoOfMsg > 100000)
	BEGIN
		DECLARE @ErrorMessage VARCHAR(MAX) = 'Exceed the upper limit of how many EDIMessage can be created (Limit: 100000, Attempt:' + CAST(@TotalNoOfMsg AS VARCHAR(15)) + ').'
		RAISERROR(@ErrorMessage, 16, 1);
	END

	DECLARE @NoOfMsgCount int = 0, @EM_GE UNIQUEIDENTIFIER = (SELECT TOP 1 GE_PK FROM dbo.GlbDepartment WHERE GE_CustomsBrokerage = 1),
		@OutgoingInterchangeStatus CHAR(3) = CASE WHEN @CreateIncoming = 1 THEN 'SNT' ELSE @InterchangeStatus END,
		@OutgoingMessageStatus CHAR(3) = CASE WHEN @CreateIncoming = 1 OR @CreateInterchange = 1 THEN 'SNT' ELSE @MessageStatus END,
		@IncomingInterchangeStatus CHAR(3) = CASE WHEN @CreateMessage = 1 THEN 'RCV' ELSE @InterchangeStatus END, @IncomingMessageStatus CHAR(3) = @MessageStatus;

	WHILE (@NoOfMsgCount < @NoOfMsgPerJob)
	BEGIN
		SET @NoOfMsgCount = @NoOfMsgCount + 1;

		IF (@CreateInterchange = 1)
		BEGIN
			IF (@CreateOutgoing = 1)
			BEGIN
				INSERT EDIInterchange (EI_PK, EI_ReceiveTransmit, EI_ApplicationCode, EI_InterchangeType, EI_From, EI_To, EI_GB, EI_Status, EI_BodyText, EI_SystemCreateTimeUtc, EI_SystemCreateUser, EI_SystemLastEditTimeUtc, EI_SystemLastEditUser, EI_InterchangeNum, EI_SessionGUID, EI_TransportType, EI_Priority)
				SELECT OutgoingEI_PK, 'TRX', @ApplicationCode, @MessageType, @CW1Reference, @CustomsReference, JE_GB, @OutgoingInterchangeStatus, @MessageText, DATEADD(SECOND, -1 * (@TotalNoOfMsg - (@NoOfMsgCount * ID)), GETUTCDATE()), @UserCode, DATEADD(SECOND, -1 * (@TotalNoOfMsg - (@NoOfMsgCount * ID)), GETUTCDATE()), @UserCode, @ApplicationCode + CAST(OutgoingEI_PK AS VARCHAR(36)), OutgoingEI_PK, @TransportType, 'HGH'
				FROM @DeclarationTableVar;
			END
			IF (@CreateIncoming = 1)
			BEGIN
				INSERT EDIInterchange (EI_PK, EI_ReceiveTransmit, EI_ApplicationCode, EI_InterchangeType, EI_From, EI_To, EI_GB, EI_Status, EI_BodyText, EI_SystemCreateTimeUtc, EI_SystemCreateUser, EI_SystemLastEditTimeUtc, EI_SystemLastEditUser, EI_InterchangeNum, EI_SessionGUID, EI_TransportType, EI_Priority)
				SELECT IncomingEI_PK, 'RCV', @ApplicationCode, @MessageType, @CustomsReference, @CW1Reference, JE_GB, @IncomingInterchangeStatus, @MessageText, DATEADD(SECOND, -1 * (@TotalNoOfMsg - (@NoOfMsgCount * ID)), GETUTCDATE()), @UserCode, DATEADD(SECOND, -1 * (@TotalNoOfMsg - (@NoOfMsgCount * ID)), GETUTCDATE()), @UserCode, @ApplicationCode + CAST(IncomingEI_PK AS VARCHAR(36)), OutgoingEI_PK, @TransportType, 'HGH'
				FROM @DeclarationTableVar;
			END
		END

		IF (@CreateMessage = 1)
		BEGIN
			IF (@CreateOutgoing = 1)
			BEGIN
				INSERT EDIMessage (EM_PK, EM_ReceiveTransmit, EM_ApplicationCode, EM_MessageType, EM_GE, EM_LinkTable, EM_LinkUniqueID, EM_GB, EM_Status, EM_MessageText, EM_SystemCreateTimeUtc, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser, EM_MessageNum, EM_EI)
				SELECT OutgoingEM_PK, 'TRX', @ApplicationCode, @MessageType, @EM_GE, 'JobDeclaration', JE_PK, JE_GB, @OutgoingMessageStatus, @MessageText, DATEADD(SECOND, -1 * (@TotalNoOfMsg - (@NoOfMsgCount * ID)), GETUTCDATE()), @UserCode, DATEADD(SECOND, -1 * (@TotalNoOfMsg - (@NoOfMsgCount * ID)), GETUTCDATE()), @UserCode, @ApplicationCode + REPLACE(CAST(OutgoingEM_PK AS VARCHAR(36)), '-', ''), CASE WHEN @CreateInterchange = 1 THEN OutgoingEI_PK ELSE NULL END
				FROM @DeclarationTableVar;
			END
			IF (@CreateIncoming = 1)
			BEGIN
				INSERT EDIMessage (EM_PK, EM_ReceiveTransmit, EM_ApplicationCode, EM_MessageType, EM_GE, EM_GB, EM_Status, EM_MessageText, EM_SystemCreateTimeUtc, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser, EM_MessageNum, EM_EI)
				SELECT IncomingEM_PK, 'RCV', @ApplicationCode, @MessageType, @EM_GE, JE_GB, @IncomingMessageStatus, @MessageText, DATEADD(SECOND, -1 * (@TotalNoOfMsg - (@NoOfMsgCount * ID)), GETUTCDATE()), @UserCode, DATEADD(SECOND, -1 * (@TotalNoOfMsg - (@NoOfMsgCount * ID)), GETUTCDATE()), @UserCode, @ApplicationCode + REPLACE(CAST(OutgoingEM_PK  AS VARCHAR(36)), '-', ''), CASE WHEN @CreateInterchange = 1 THEN IncomingEI_PK ELSE NULL END
				FROM @DeclarationTableVar;
			END
		END
	END

	COMMIT TRANSACTION;
	SELECT @TotalJobs = COUNT(*) FROM @DeclarationTableVar
END TRY
BEGIN CATCH
	IF (@@TRANCOUNT > 0)
	BEGIN
		ROLLBACK TRANSACTION;
		THROW;
	END
END CATCH
SELECT @TotalJobs
";
				using (var cmd = Db.Connection.Command(sqlText))
				{
					cmd.AddParameter("@ApplicationCode", SqlDbType.Char, applicationCode.ToString());
					cmd.AddParameter("@MessageType", SqlDbType.Char, messageType.ToString());
					cmd.AddParameter("@UserCode", SqlDbType.VarChar, GlbStaff.CurrentUser.GS_Code.ToString());
					cmd.AddParameter("@FromCreateDate", SqlDbType.DateTime, CreationFromDateEdit.DateTimeValue.IsValid ? CreationFromDateEdit.DateTimeValue : DBNull.Value);
					cmd.AddParameter("@ToCreateDate", SqlDbType.DateTime, CreationToDateEdit.DateTimeValue.IsValid ? CreationToDateEdit.DateTimeValue : DBNull.Value);
					cmd.AddParameter("@NoOfMsgPerJob", SqlDbType.Int, (int)new ZDecimal(NoOfMsgPerJobCalcEdit.CalcValue).ToZInt());
					cmd.AddParameter("@CustomsReference", SqlDbType.VarChar, InterchangeCustomsRerenceTextBox.Text);
					cmd.AddParameter("@CW1Reference", SqlDbType.VarChar, InterchangeCW1ReferenceTextBox.Text);
					cmd.AddParameter("@CreateOutgoing", SqlDbType.Bit, createOutgoing);
					cmd.AddParameter("@CreateIncoming", SqlDbType.Bit, createIncoming);
					cmd.AddParameter("@CreateInterchange", SqlDbType.Bit, createInterchange);
					cmd.AddParameter("@CreateMessage", SqlDbType.Bit, createMessage);
					cmd.AddParameter("@InterchangeStatus", SqlDbType.Char, InterchangeStatusTextBox.Text);
					cmd.AddParameter("@MessageStatus", SqlDbType.Char, MessageStatusTextBox.Text);
					cmd.AddParameter("@MessageText", SqlDbType.VarChar, messageText);
					cmd.AddParameter("@TransportType", SqlDbType.VarChar, TransportTypeTextBox.Text);
					var totalJobs = (long)cmd.ExecuteScalar();
					Globals.Message.ShowInformation($"Create data for {totalJobs} jobs.");
				}
			}
		}
	}
}
