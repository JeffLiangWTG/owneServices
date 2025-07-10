$ErrorActionPreference = "Stop"

$TargetPath = ".\ThirdParty\playwright-browsers"

if(!(Test-Path "${TargetPath}")) {
	New-Item -Type dir ${TargetPath}
}

# TODO: Change this url to our team's url when it is created - eRequest CS01835107
# TODO: Upgrade Playwright to latest version when we maintain our own binaries
$remoteRepoBaseUrl = "https://proget.wtg.zone/endpoints/LogisticServices/content/Common"
$targetPathBaseUrl = [String]@(Resolve-Path $TargetPath -ErrorAction Stop)

# schema of each item in this array
# 0: file url on proget
# 1: target folder to download and extract the file
# 2: the hash of the zip file
$filesToCopy = @(
  @("$remoteRepoBaseUrl/Playwright/chrome-win-v1.42.zip", "$targetPathBaseUrl\chromium-1105", "0552d16bb7641ea927018c004ea3a1a65b03285f9f04ace0a7e1c77132467771"),
  @("$remoteRepoBaseUrl/Playwright/firefox-v1.42.zip", "$targetPathBaseUrl\firefox-1440", "6f1de2cca04c8a52b27d79082b926a7b15212f9ca7c2f40692613566de8b1f37")
)

function GetFileHash($filePath) {
    $fileStream = [System.IO.File]::OpenRead($filePath)

    try {
        # Get-FileHash is not available when calling Powershell from PSCore, which is
        # a common situation when using QGL CLI, so we'll roll our own implementation.
        $sha = [System.Security.Cryptography.SHA256]::Create()

        try {
            $hashBytes = $sha.ComputeHash($fileStream)
            $hashHexString = [BitConverter]::ToString($hashBytes).Replace("-", "")
            return $hashHexString
        }
        finally {
            $sha.Dispose()
        }
    }
    finally {
        $fileStream.Close()
    }
}

function DownloadAndVerifyHash($remoteRepository, $targetDir, $expectedHash) {
    if((Test-Path "${targetDir}")) {
        Write-Host "Skip downloading '$remoteRepository' because the directory '$targetDir' has already existed."
        Write-Host "To force downloading it again, manually remove the directory '$targetDir'"
        Write-Host
        return;
    }

    New-Item -Type dir ${targetDir}

    $maxRetries = 4
    $retryDelay = 30
    $targetFileName = "$targetDir\browser.zip"

    for ($attempt = 1; $attempt -le $maxRetries; $attempt++) {
        try {
            Write-Host "Downloading from $remoteRepository"
            Invoke-WebRequest $remoteRepository -OutFile $targetFileName
            Write-Host "Download Complete. Verifying Hash."
            $downloadFileHash = GetFileHash $targetFileName
            if ($downloadFileHash -ne $expectedHash)
            {
                Write-Error "Copied file hash '$downloadFileHash' does not match the one specified in call arguments ('$expectedHash')"
            }
            Write-Host "Hash Verified. OK."

            Expand-Archive -Path $targetFileName -DestinationPath $targetDir
            return
        }
        catch [System.IO.IOException] {
            if ($attempt -eq $maxRetries) {
                Write-Error "Unable to download from $remoteRepository"
                throw
            } else {
                Write-Host "Attempt $attempt failed. Retrying in $retryDelay seconds..."
                Start-Sleep -Seconds $retryDelay
                $retryDelay *= 2
            }
        }
    }
}

Foreach ($file in $filesToCopy) {
	DownloadAndVerifyHash $file[0] $file[1] $file[2]
}
