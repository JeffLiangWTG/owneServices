Using Assembly Microsoft.TeamFoundation.Client
Using Assembly Microsoft.TeamFoundation.VersionControl.Client
Using Assembly Microsoft.TeamFoundation.VersionControl.Common
Using Assembly Microsoft.TeamFoundation.WorkItemTracking.Client
Using Assembly Microsoft.TeamFoundation.Build.Client
Using Assembly Microsoft.TeamFoundation.Build.Common

Using Namespace Microsoft.TeamFoundation.VersionControl.Client;
Using Namespace Microsoft.TeamFoundation.Client;
Using Namespace Microsoft.TeamFoundation.Build.Client;
Using Namespace Microsoft.TeamFoundation.WorkItemTracking.Client;
Using Namespace Microsoft.TeamFoundation.Framework.Client;
Using Namespace System.IO;

class TFS
{
    [Workspace] $WorkSpace
    hidden [VersionControlServer] $VersionControlServer
    hidden [WorkItemStore] $WorkItemStore
    hidden [IBuildServer] $BuildServer
    hidden [IIdentityManagementService] $IdentityManagementService
    hidden [TfsTeamProjectCollection] $Collection

    TFS ([string] $tfsServer, [string] $workspace)
    {
        
        try
        {
            $uri = New-Object System.Uri -ArgumentList $tfsServer
            $credentialProvider = new-object Microsoft.TeamFoundation.Client.UICredentialsProvider
            $this.Collection = [TfsTeamProjectCollectionFactory]::GetTeamProjectCollection($uri, $credentialProvider)
            $this.Collection.Authenticate()
 
            if ($this.Collection.HasAuthenticated)
            {
                $this.VersionControlServer = $this.Collection.GetService([VersionControlServer])
                $this.WorkItemStore = $this.Collection.GetService([WorkItemStore])
                $this.BuildServer = $this.Collection.GetService([IBuildServer])
                $this.IdentityManagementService = $this.Collection.GetService([IIdentityManagementService])
                $this.Collection = $this.Collection
                $this.WorkSpace = $this.VersionControlServer.GetWorkspace($workspace)
            }
        }
        catch
        {
            Write-Host -ForegroundColor Red 'Failed to connect to TFS'
            Write-Host -ForegroundColor Red $_.Exception.Message
        }
    }

    [string] GetLocalItem([string] $serverItem)
    {
        return $this.WorkSpace.GetLocalItemForServerItem($serverItem)
    }

    [string] GetServerItem([string] $localItem)
    {
        return $this.WorkSpace.GetServerItemForLocalItem($localItem)
    }

    [void] CheckOut([string] $item)
    {
        $added = $this.WorkSpace.GetPendingChanges() | Where-Object { 
            $_.IsAdd -and $_.LocalItem -eq $item 
        }
        if (-not ($this.VersionControlServer.ServerItemExists($item, [ItemType]::Any) -or $added -ne $null))
        {
            $this.Add($item)
        }
        elseif ($this.WorkSpace.GetPendingChanges($item).Count -eq 0)
        {
            $this.WorkSpace.PendEdit($item);
        }
    }

    hidden [void] Add([string] $item)
    {
        $this.WorkSpace.PendAdd($item);
    }
}