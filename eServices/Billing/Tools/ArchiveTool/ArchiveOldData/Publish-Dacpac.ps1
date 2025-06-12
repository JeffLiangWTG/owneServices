function Publish-Dacpac {
    param(
        [string]$serverName,
        [string]$databaseName,
        [string]$userName,
        [string]$password,
        [string]$dacpacPath
    )

    $sqlPackagePath = 'C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\SqlPackage.exe'

    $arguments = @(
        "/a:Publish",
        "/tsn:$serverName",
        "/tdn:$databaseName",
        "/tu:$userName",
        "/tp:$password",
        "/sf:$dacpacPath",
        "/tec:False"
    )

    & $sqlPackagePath $arguments
}