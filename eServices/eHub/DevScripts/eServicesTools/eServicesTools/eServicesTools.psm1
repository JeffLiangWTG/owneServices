Using Module eServices-Project

<#
.Synopsis
   Utilities to manipulate project files for common tasks performed in eServices.
.DESCRIPTION
   Automated tasks for eServices team. Includes:
   * Sign the project
   * Add CommonAssemblyInfo
   * Add Deployment
   * Add Logging
   * Rename project
   * Relocate project
   * Build dependencies
   * Find dependants
   * Add to TFS

.EXAMPLE
   PatchProject path\to\project.csproj -Sign
.EXAMPLE
   PatchProject path\to\project.csproj -OrganiseAssemblyInfo -InfoVersion 2015 -AssemblyVersion 3.0.0.0
.EXAMPLE
   PatchProject path\to\project.csproj -IncludeDeployment -DeploymentType WebService -ProfileNames Test1, Test2, Production1, Production2
.EXAMPLE
   PatchProject path\to\project.csproj -IncludeLogging
#>
function PatchProject
{
    [CmdletBinding()]
    Param
    (
        [Parameter(Mandatory=$True,
             ValueFromPipeline=$false,
             ValueFromPipelineByPropertyName=$True,
             HelpMessage='Path to settings file.')]
        [string]
        $Settings,

        [Parameter(Mandatory=$True,
             ValueFromPipeline=$True,
             ValueFromPipelineByPropertyName=$True,
             HelpMessage='Path to project file')]
        [string]
        $Path,

        [switch]
        $IncludeLogging,
        
        [switch]
        $Sign,
        
        [switch]
        $OrganiseAssemblyInfo,

        [ValidateScript({$OrganiseAssemblyInfo})]
        [string]
        $InfoVersion,

        [ValidateScript({$OrganiseAssemblyInfo})]
        [string]
        $AssemblyVersion,
        
        [switch]
        $IncludeDeployment,
		
        [ValidateScript({$IncludeDeployment})]
        [string[]]
        $ProfileNames,

        [ValidateScript({$IncludeDeployment})]
        [string]
        $DeploymentType,

        [switch]
        $Silent
    )

    Begin
    {
        $Project = [Project]::new($Path, $Settings)
        if (-not $Project.IsValid)
        {
            Write-Host -ForegroundColor Red "Invalid Project"
            break
        }
        if ($IncludeDeployment)
        {
            $DeploymentType = $Project.GetDeploymentType()
            if ($Project.GetDeploymentType() -eq "Inconclusive")
            {
                break
            }
        }
    }

    Process
    {
        if ($IncludeDeployment)
        {
            $Project.IncludeDeployment($DeploymentType)
			$ProfileNames | ForEach-Object {
				$Project.AddDeploymentProfile($_, $DeploymentType)
			}
        }
        if ($IncludeLogging)
        {
            $Project.IncludeLogging()
        }
        if ($OrganiseAssemblyInfo)
        {
            $Project.OrganiseAssemblyInfo($InfoVersion, $AssemblyVersion)
        }
        if ($Sign)
        {
            $Project.Sign()
        }
    }

    End
    {
    }
}

Export-ModuleMember -Function 'PatchProject'