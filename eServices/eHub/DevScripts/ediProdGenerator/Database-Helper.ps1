function DB-GenerateDatabase
{
  [CmdletBinding()]
  param
  (
    [Parameter(Mandatory=$True,
      ValueFromPipeline=$True,
      ValueFromPipelineByPropertyName=$True,
      HelpMessage='Script to generate database')]
    [Alias('ScriptFile')]
    [string]$script,
    
    [Parameter(Mandatory=$True,
      ValueFromPipeline=$False,
      ValueFromPipelineByPropertyName=$False,
      HelpMessage='Server to connect to')]
    [Alias('ServerInstance')]
    [string]$server,

    [Parameter(Mandatory=$True,
      ValueFromPipeline=$False,
      ValueFromPipelineByPropertyName=$False,
      HelpMessage='Database name to create')]
    [Alias('DatabaseName')]
    [object]$dbName
  )

  begin 
  {
  }

  process 
  {
    try
    {
      $xmlSchemaCollectionsOutput = [System.IO.Path]::GetTempFileName()
      $mainOutput = [System.IO.Path]::GetTempFileName()
      $masterConnection = "Data Source=$server;Initial Catalog=master;Integrated Security=True"
      $ediConnection = "Data Source=$server;Initial Catalog=$dbName;Integrated Security=True"

      [regex]$pattern = '(CREATE XML SCHEMA COLLECTION[\w\W]*?;)'

      $matches = $pattern.Matches(([System.IO.File]::ReadAllText($script))) | Set-Content $xmlSchemaCollectionsOutput
      
      $pattern.Replace(([System.IO.File]::ReadAllText($script)), '') | Set-Content $mainOutput
      
      $databaseName = "DBName=$dbName"
      Invoke-Sqlcmd -ConnectionString $masterConnection -Query "IF EXISTS(SELECT * FROM sys.databases WHERE name='`$(DBName)')
      BEGIN
          ALTER DATABASE `$(DBName) SET SINGLE_USER WITH ROLLBACK IMMEDIATE
          DROP DATABASE `$(DBName)
      END
      CREATE DATABASE `$(DBName)" -Variable $databaseName
      
      Invoke-Sqlcmd -ConnectionString $ediConnection -InputFile $xmlSchemaCollectionsOutput
      Invoke-Sqlcmd -ConnectionString $ediConnection -InputFile $mainOutput 

      Remove-Item -Path $xmlSchemaCollectionsOutput
      Remove-Item -Path $mainOutput
      Remove-Item -Path $script

      Write-Host "Created $dbName"
    }
    catch
    {
        Write-Host -ForegroundColor Red "Failed to create $dbName"
        Write-Host -ForegroundColor Red $_.Exception.Message
    }
  }
}


function DB-BulkCopy
{
  [CmdletBinding()]
  param
  (
    [Parameter(Mandatory=$True,
      ValueFromPipeline=$True,
      ValueFromPipelineByPropertyName=$True,
      HelpMessage='XML file to be inserted into database')]
    [Alias('XMLSource')]
    [string]$source,
    
    [Parameter(Mandatory=$True,
      ValueFromPipeline=$False,
      ValueFromPipelineByPropertyName=$False,
      HelpMessage='Server to connect to')]
    [Alias('ServerInstance')]
    [string]$server,

    [Parameter(Mandatory=$True,
      ValueFromPipeline=$False,
      ValueFromPipelineByPropertyName=$False,
      HelpMessage='Database name to create')]
    [Alias('DatabaseName')]
    [object]$dbName
  )

  begin 
  {
  }

  process 
  {
    $ediProdConnectionString = "Data Source=$server;Initial Catalog=$dbName;Integrated Security=True"
    $ds = new-object "System.Data.DataSet" "dsServers"
    $ds.ReadXml($source) | Out-Null

    $cn = new-object System.Data.SqlClient.SqlConnection($ediProdConnectionString);
    $cn.Open()

    foreach ($table in $ds.Tables)
    {
      $tableName = $table.TableName
      if (-not $populated.Contains($tableName))
      {
        $tableCount = Invoke-Sqlcmd -Query "SELECT Count(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '$tableName'" -ConnectionString $ediProdConnectionString
        if ($tableCount.Column1 -eq 1)
        {
          try
          {
            Write-Host "Populating $tableName ... "  -NoNewline
            $bc = new-object ("System.Data.SqlClient.SqlBulkCopy") $cn
            $table.Columns | ForEach-Object { $bc.ColumnMappings.Add($_.ColumnName, $_.ColumnName) } | Out-Null
            $bc.DestinationTableName = $tableName
            $bc.WriteToServer($table)
            $populated += ,$tableName
            Write-Host -ForegroundColor Green "Done"
          }
          catch
          {
            Write-Host -ForegroundColor Red $_.Exception
          }
        }
      }
    }

    $cn.Close()
  }
}