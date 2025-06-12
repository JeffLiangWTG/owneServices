param (
   [Parameter(Mandatory=$true)][string]$filename,
   [Parameter(Mandatory=$false)][string]$sender = 'WTLDSGSGC',
   [Parameter(Mandatory=$false)][string]$recipient = 'WTLDSGSGC',
   [Parameter(Mandatory=$false)][string]$server = 'localhost',
   [Parameter(Mandatory=$false)][string]$emailsubject,
   [Parameter(Mandatory=$false)][string]$overridefileName
)

$ErrorActionPreference = "Stop"

if ($sender -eq $recipient) {
    echo "Please include either '-sender SENDER' or '-recipient RECIPIENT'"
}
else {
    $message = Get-Content $filename | Out-String

    $connectionString = "Server=$server;Database=eHubTransactions;Integrated Security=True;"
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()

    $command = $connection.CreateCommand()
    $command.CommandText = "USE eHubTransactions;
DECLARE @newContent nvarchar(max) = dbo.CompressAndEncode(@useContent)
DECLARE @newMessageTrackingID uniqueidentifier = newid();
DECLARE @PK uniqueidentifier = newid();
DECLARE @return_value int

EXEC    @return_value = [dbo].[InsertInbox]
        @InboxPK = @PK,
        @MessageTrackingID = @newMessageTrackingID,
        @EnvelopeTrackingID = '00000000-0000-0000-0000-000000000000',
        @SenderID = @useSender,
        @RecipientID = @useRecipient,
        @MessageType = '',
        @IsFlatFile = 0,
        @EmailSubject = @useEmailSubject,
        @FileName = @useFileName,
        @ApplicationCode = 'SYS',
        @Status = '0',
        @CurrentDateTimeUTC = '', -- This is just ignored anyway
        @Content = @newContent
SELECT 'Return Value' = @return_value"

    $command.Parameters.Add("@useContent", $message) | Out-Null
    $command.Parameters.Add("@useSender", $sender) | Out-Null
    $command.Parameters.Add("@useRecipient", $recipient) | Out-Null
    $command.Parameters.Add("@useEmailSubject", $emailsubject) | Out-Null
    $command.Parameters.Add("@useFileName", $overridefileName) | Out-Null
    $command.ExecuteNonQuery()

    $connection.Close()
}