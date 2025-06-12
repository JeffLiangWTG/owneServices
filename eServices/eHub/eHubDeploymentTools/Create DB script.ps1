# Configuration data
[string] $server   = "wg1-vsql-1";          # SQL Server Instance
[string] $database = "eHubTransactions";      # Database with the tables to script out.
[string] $outputPath  = "c:\eHubTransactions.sql";          # Path to export file

# Reference to SMO
[void][System.Reflection.Assembly]::LoadWithPartialName('Microsoft.SqlServer.SMO');

Write-Output ((Get-Date -format yyyy-MM-dd_HH-mm-ss) + ": Started ...");

$srv = New-Object Microsoft.SqlServer.Management.Smo.Server $server;
$db = $srv.Databases[$database];

# Configuration for scripting options: Which related objects should also be scripted?
$so = New-Object Microsoft.SqlServer.Management.Smo.ScriptingOptions;
$so.DriAll =            $false;
$so.DriClustered      = $true;
$so.DriDefaults       = $true;
$so.DriIndexes        = $true;
$so.DriNonClustered   = $true;
$so.DriPrimaryKey     = $true;
$so.DriUniqueKeys     = $true;

$so.AnsiFile          = $true;
$so.IncludeHeaders    = $false;
$so.Indexes           = $true;
$so.SchemaQualify     = $true;
$so.Triggers          = $false;
$so.XmlIndexes        = $false;

$outfile = New-Object System.IO.StreamWriter($outputPath);

# Loop over all tables.
foreach ($tbl in $db.Tables| where {$_.IsSystemObject -eq $false })
{
    $sb = New-Object System.Text.StringBuilder;
    # Loop through all generated partial scripts and add them.
    foreach ($script in $tbl.Script($so))
    {
        $void = $sb.AppendLine($script);
    }

    try
    {
        # Write script in text file.
        $outfile.WriteLine($sb.ToString());
        $outfile.WriteLine("GO");
        $outfile.WriteLine("");

        Write-Output ((Get-Date -format yyyy-MM-dd_HH-mm-ss) + ": Table $tbl.Name");
    }
    catch
    {
        Write-Output ($_.Exception.Message)
    }
}

# Loop over all foreign keys.
$so.DriAll =            $true;
foreach ($tb in $db.Tables| where {$_.IsSystemObject -eq $false })
{
    foreach ($item in $tb.ForeignKeys) 
    {
        $sb = New-Object System.Text.StringBuilder;
        # Loop through all generated partial scripts and add them.
        foreach ($script in $Item.Script($so))
        {
            $void = $sb.AppendLine($script);
        }

        try
        {
            # Write script in text file.
            $outfile.WriteLine($sb.ToString());
            $outfile.WriteLine("GO");
            $outfile.WriteLine("");

            Write-Output ((Get-Date -format yyyy-MM-dd_HH-mm-ss) + ": Index $item.Name");
        }
        catch
        {
            Write-Output ($_.Exception.Message)
        }
    }
}

# Loop over all views.
foreach ($vie in $db.Views | where {$_.IsSystemObject -eq $false })
{
    $sb = New-Object System.Text.StringBuilder;
    # Loop through all generated partial scripts and add them.
    foreach ($script in $vie.Script($so))
    {
        if( $script -ne "SET ANSI_NULLS ON" -and $script -ne "SET QUOTED_IDENTIFIER ON" )
        {
            $void = $sb.AppendLine($script);
        }
    }

    try
    {
        # Write script in text file.
        $outfile.WriteLine($sb.ToString());
        $outfile.WriteLine("GO");
        $outfile.WriteLine("");

        Write-Output ((Get-Date -format yyyy-MM-dd_HH-mm-ss) + ": View $vie.Name");
    }
    catch
    {
        Write-Output ($_.Exception.Message)
    }
}

# Loop over all SP.
foreach ($sp in $db.StoredProcedures | where {$_.IsSystemObject -eq $false })
{
    $sb = New-Object System.Text.StringBuilder;

    foreach ($script in $sp.Script($so))
    {
        if( $script -ne "SET ANSI_NULLS ON" -and $script -ne "SET QUOTED_IDENTIFIER ON" )
        {
          $void = $sb.AppendLine($script);
        }
    }

    try
    {
        # Write script in text file.
        $outfile.WriteLine($sb.ToString());
        $outfile.WriteLine("GO");
        $outfile.WriteLine("");
        
        Write-Output ((Get-Date -format yyyy-MM-dd_HH-mm-ss) + ": StoredProcedures $sp.Name");
    }
    catch
    {
        Write-Output ($_.Exception.Message)
    }
}

# Loop over all UserDefinedFunctions.
foreach ($udf in $db.UserDefinedFunctions | where {$_.IsSystemObject -eq $false })
{
    $sb = New-Object System.Text.StringBuilder;

    foreach ($script in $udf.Script($so))
    {
        if( $script -ne "SET ANSI_NULLS ON" -and $script -ne "SET QUOTED_IDENTIFIER ON" )
        {
          $void = $sb.AppendLine($script);
        }
    }

    try
    {
        # Write script in text file.
        $outfile.WriteLine($sb.ToString());
        $outfile.WriteLine("GO");
        $outfile.WriteLine("");
       
        Write-Output ((Get-Date -format yyyy-MM-dd_HH-mm-ss) + ": UserDefinedFunctions $udf.Name");
    }
    catch
    {
        Write-Output ($_.Exception.Message)
    }
}

# Loop over all Triggers.
foreach ($tb in $db.Tables| where {$_.IsSystemObject -eq $false })
{
    foreach ($tg in $tb.Triggers) 
    {
        $sb = New-Object System.Text.StringBuilder;
        # Loop through all generated partial scripts and add them.
        foreach ($script in $tg.Script($so))
        {
            $void = $sb.AppendLine($script);
        }

        try
        {
            # Write script in text file.
            $outfile.WriteLine($sb.ToString());
            $outfile.WriteLine("GO");
            $outfile.WriteLine("");

            Write-Output ((Get-Date -format yyyy-MM-dd_HH-mm-ss) + ": Trigger $tg.Name");
        }
        catch
        {
            Write-Output ($_.Exception.Message)
        }
    }
}

$outfile.Close();
$outfile.Dispose();
Write-Output ((Get-Date -format yyyy-MM-dd_HH-mm-ss) + ": Finished");
