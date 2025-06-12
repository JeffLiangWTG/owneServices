
Remove-Module eServicesTools -Force
Remove-Module eServices-DevEnv -Force
Remove-Module eServices-Project -Force
Remove-Module eServices-TFS -Force

Import-Module eServicesTools

$Projects = @(
    'D:\TFS\eServices_7\eHub\Portal\Portal\Portal.csproj'
)


$Projects | ForEach-Object { 
    PatchProject -Settings 'D:\tfs\eServices_7\eHub\DevScripts\eServicesTools\Samples\example.properties' `
                -Path $_ `
                -IncludeDeployment `
                -DeploymentType WebService `
                -ProfileNames Ehsan1
}

$Projects | ForEach-Object { 
    PatchProject -Settings 'D:\tfs\eServices_7\eHub\DevScripts\eServicesTools\Samples\example.properties' `
                -Path $_ `
                -Sign
}

$Projects | ForEach-Object { 
    PatchProject -Settings 'D:\tfs\eServices_7\eHub\DevScripts\eServicesTools\Samples\example.properties' `
                -Path $_ `
                -OrganiseAssemblyInfo `
                -InfoVersion 2015 `
                -AssemblyVersion 3.0.0.0
}

$Projects | ForEach-Object { 
    PatchProject -Settings 'D:\tfs\eServices_7\eHub\DevScripts\eServicesTools\Samples\example.properties' `
                -Path $_ `
                -IncludeLogging
}
