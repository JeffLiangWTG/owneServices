function TFS-Connect 
{
  [CmdletBinding()]
  param
  (
    [Parameter(Mandatory=$True,
      ValueFromPipeline=$True,
      ValueFromPipelineByPropertyName=$True,
      HelpMessage='TFS Server to connect to')]
    [Alias('Server')]
    [string]$tfsServer
  )

  begin 
  {
    '.Client', '.VersionControl.Client', '.VersionControl.Common', '.WorkItemTracking.Client', '.Build.Client', '.Build.Common' |
    ForEach-Object {
      Add-Type -AssemblyName "Microsoft.TeamFoundation$_, Version=12.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
    }
  }

  process 
  {
    try
    {
      $uri = New-Object System.Uri -ArgumentList $tfsServer
      $credentialProvider = new-object Microsoft.TeamFoundation.Client.UICredentialsProvider
      $collection = [Microsoft.TeamFoundation.Client.TfsTeamProjectCollectionFactory]::GetTeamProjectCollection($uri, $credentialProvider)
      $collection.Authenticate()
 
      if ($collection.HasAuthenticated)
      {
        Write-Host 'Connected to TFS'
        $tfs = '' | Select-Object Collection, VersionControlServer, WorkItemStore, BuildServer, IdentityManagementService
        $tfs.VersionControlServer = $collection.GetService([Microsoft.TeamFoundation.VersionControl.Client.VersionControlServer])
        $tfs.WorkItemStore = $collection.GetService([Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItemStore])
        $tfs.BuildServer = $collection.GetService([Microsoft.TeamFoundation.Build.Client.IBuildServer])
        $tfs.IdentityManagementService = $collection.GetService([Microsoft.TeamFoundation.Framework.Client.IIdentityManagementService])
        $tfs.Collection = $collection
        return $tfs
      }
    }
    catch
    {
        Write-Host -ForegroundColor Red 'Failed to connect to TFS'
        Write-Host -ForegroundColor Red $_.Exception.Message
    }
  }
}

function TFS-Download
{
  [CmdletBinding()]
  param
  (
    [Parameter(Mandatory=$True,
      ValueFromPipeline=$True,
      ValueFromPipelineByPropertyName=$True,
      HelpMessage='TFS item to download from')]
    [Alias('ServerItem')]
    [string]$item,
    
    [Parameter(Mandatory=$False,
      ValueFromPipeline=$False,
      ValueFromPipelineByPropertyName=$False,
      HelpMessage='Local file to download to')]
    [Alias('LocalFile')]
    [string]$file,
        
    [Parameter(Mandatory=$True,
      ValueFromPipeline=$True,
      ValueFromPipelineByPropertyName=$True,
      HelpMessage='TFS server')]
    [Alias('TFSConnection')]
    [object]$tfs
  )

  begin 
  {
    '.Client', '.VersionControl.Client', '.VersionControl.Common', '.WorkItemTracking.Client', '.Build.Client', '.Build.Common' |
    ForEach-Object {
      Add-Type -AssemblyName "Microsoft.TeamFoundation$_, Version=12.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
    }
  }

  process 
  {
    try
    {
      if (!$file)
      {
        $file = [System.IO.Path]::GetTempFileName()
      }
      $tfs.VersionControlServer.DownloadFile($item, $file)
      return New-Object pscustomobject –property @{
        ServerItem = $item
        LocalFile = $file
      }
    }
    catch
    {
        Write-Host -ForegroundColor Red 'Failed to download ' $item
        Write-Host -ForegroundColor Red $_.Exception.Message
    }
  }
}

function TFS-Search
{
  [CmdletBinding()]
  param
  (
    [Parameter(Mandatory=$True,
      ValueFromPipeline=$False,
      ValueFromPipelineByPropertyName=$False,
      HelpMessage='TFS directory to search in')]
    [Alias('ServerDirectory')]
    [string]$directory,
    
    [Parameter(Mandatory=$True,
      ValueFromPipeline=$False,
      ValueFromPipelineByPropertyName=$False,
      HelpMessage='Pattern to match')]
    [Alias('Pattern')]
    [string]$searchPattern,
    
    [Parameter(Mandatory=$True,
      ValueFromPipeline=$False,
      ValueFromPipelineByPropertyName=$False,
      HelpMessage='Pattern to match')]
    [Alias('FilterOut')]
    [string]$excludePattern,
        
    [Parameter(Mandatory=$True,
      ValueFromPipeline=$False,
      ValueFromPipelineByPropertyName=$False,
      HelpMessage='TFS server')]
    [Alias('TFSConnection')]
    [object]$tfs
  )

  begin 
  {
    '.Client', '.VersionControl.Client', '.VersionControl.Common', '.WorkItemTracking.Client', '.Build.Client', '.Build.Common' |
    ForEach-Object {
      Add-Type -AssemblyName "Microsoft.TeamFoundation$_, Version=12.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
    }
  }

  process 
  {
    try
    {
      return $tfs.VersionControlServer.GetItems($directory, [Microsoft.TeamFoundation.VersionControl.Client.RecursionType]::Full) | 
        Select-Object -ExpandProperty Items | 
        Where-Object { $_.ServerItem -match $searchPattern } |
        Where-Object { $_.ServerItem -notmatch $excludePattern }
    }
    catch
    {
        Write-Host -ForegroundColor Red 'Failed to search ' $directory
        Write-Host -ForegroundColor Red $_.Exception.Message
    }
  }
}