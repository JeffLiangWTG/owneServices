param (
   [Parameter(Mandatory=$true)][string]$filename,
   [Parameter(Mandatory=$false)][string]$sender = 'WTLDSGSGC',
   [Parameter(Mandatory=$false)][string]$recipient = 'WTLDSGSGC',
   [Parameter(Mandatory=$false)][string]$server = 'localhost',
   [Parameter(Mandatory=$false)][string]$emailsubject
)

$ErrorActionPreference = "Stop"

$message = Get-Content $filename | Out-String

# Open database
$connectionString = "Server=$server;Database=eHubTransactions;Integrated Security=True;"
$connection = New-Object System.Data.SqlClient.SqlConnection
$connection.ConnectionString = $connectionString
$connection.Open()

# Convert message to CompressAndEncode form.
# This needs to be preprocessed because the special characters would mess up the string literal, and we get around that via command parameters
$command = $connection.CreateCommand()
$command.CommandText = "SELECT dbo.CompressAndEncode(@useContent)"
$command.Parameters.Add("@useContent", $message) | Out-Null
$message = $command.ExecuteScalar();
$connection.Close()

# Generate INSERT query
$insertSQL = "USE eHubTransactions;
DECLARE @newContent nvarchar(max) = '@useContent';
DECLARE @newMessageTrackingID uniqueidentifier = newid();
DECLARE @PK uniqueidentifier = newid();
DECLARE @return_value int

EXEC    @return_value = [dbo].[InsertInbox]
	@InboxPK = @PK,
	@MessageTrackingID = @newMessageTrackingID,
	@EnvelopeTrackingID = '00000000-0000-0000-0000-000000000000',
	@SenderID = '@useSender',
	@RecipientID = '@useRecipient',
	@MessageType = '',
	@IsFlatFile = 0,
	@EmailSubject = '@useEmailSubject',
	@FileName = '',
	@ApplicationCode = 'SYS',
	@Status = '0',
	@CurrentDateTimeUTC = '', -- This is just ignored anyway
	@Content = @newContent
SELECT 'Return Value' = @return_value"

$insertSQL = $insertSQL.Replace('@useContent', $message)
$insertSQL = $insertSQL.Replace('@useSender', $sender)
$insertSQL = $insertSQL.Replace('@useRecipient', $recipient)
$insertSQL = $insertSQL.Replace('@useEmailSubject', $emailsubject)

$outFilename = 'InsertMessage_' + [io.path]::GetFileNameWithoutExtension($filename) + '.sql'

$insertSql | Out-File -File $outFileName
Write-Host 'The query to write the message to inbox has been written to:' $outFilename
