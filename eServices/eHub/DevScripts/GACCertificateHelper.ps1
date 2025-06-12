<#
.SYNOPSIS
    Export GAC and certificate info on local\remote computer into text file.
.DESCRIPTION
    .
.EXAMPLE
    C:\PS> GetGACCertificateInfo
    Export GAC and certificate info on local computer
.EXAMPLE
    C:\PS> GetGACCertificateInfo -ServerName server1 -Username user1 -Password password1 -Location 'GAC,cert:\LocalMachine\Root,cert:\CurrentUser\My'
    Export certificates of specific stores on remote computer
#>
Function GetGACCertificateInfo {
  [CmdletBinding()]
  param
  (
    [string]$ServerName = 'localhost',
    [string]$Username,
    [string]$Password,
    [string]$Location = 'All'
  )

  process {
    $dateStr = Get-Date -format "yyyyMMddHHmmss"
    if ($Location -ne "All") {
      $paths = $Location.Split(",")
    }
    else {
      $paths = "GAC", "cert:\"
    }

    foreach ($path in $paths) {
      If ($path -eq "GAC") {
        $scriptBlock = { & 'C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\gacutil.exe' -l }
      }
      Else {
        $arguments = $path
        $scriptBlock = { Param ($certificatePath) Get-ChildItem -Path $certificatePath -Recurse | Sort-Object -Property Subject }
      }

      If (($ServerName -ne 'localhost') -and ($ServerName -ne '')) {
        $securePassword = convertto-securestring -AsPlainText -Force -String $Password
        $credential = new-object -typename System.Management.Automation.PSCredential -argumentlist $Username, $securePassword
        $result = Invoke-Command -ComputerName $ServerName -ScriptBlock $scriptBlock -ArgumentList $arguments -Credential $credential
      }
      Else {
        $result = Invoke-Command -ScriptBlock $scriptBlock -ArgumentList $arguments
      }

      If ($path -eq "GAC") {
        $result = $result | Where-Object { $_ -match ".*processorArchitecture.*" } | Sort-Object
      }

      $outDir = ".\$([System.Net.Dns]::GetHostByName($ServerName).HostName)\$dateStr"
      If (!(Test-Path $outDir)) {
        New-Item -ItemType Directory -Force -Path $outDir
      }

      $outputFilePath =  "$outDir\$($path.ToString().Replace("cert:\", "CertStore").Replace("\", "-")).txt"
      $result | Out-File -FilePath $outputFilePath
    }
  }
}