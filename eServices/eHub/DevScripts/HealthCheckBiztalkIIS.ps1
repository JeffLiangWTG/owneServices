
Clear-Host
$BiztalkServerAddress = "SYDWP-SBTS-1.wisecloud.zone"
 
$BizTalkMgmtDB = [string]::Format("SERVER={0};DATABASE=BizTalkMgmtDb;Integrated Security=SSPI", $BiztalkServerAddress)

[void] [System.reflection.Assembly]::LoadWithPartialName("Microsoft.BizTalk.ExplorerOM")

$Catalog = New-Object Microsoft.BizTalk.ExplorerOM.BtsCatalogExplorer

$Catalog.ConnectionString = $BizTalkMgmtDB

$State = "Healthy"

$Description = [string]::Empty

$WCFAdapters = "WCF-BasicHttp", "WCF-CustomIsolated", "WCF-WebHttp"

foreach ($ReceivePort in $Catalog.ReceivePorts) {
    foreach ($ReceiveLocation in $ReceivePort.ReceiveLocations) {
        if (($WCFAdapters -contains $ReceiveLocation.TransportType.Name -or $ReceiveLocation.TransportType.Name.Equals("HTTP")) -and $ReceiveLocation.Address.Contains("/HealthCheck")){
            if ($ReceiveLocation.Enable -eq $false) {
                $State = "Critical"
                $Description = $Description + [string]::Format("ERROR: Health Check receive location is disabled. Address: {0}`n", $ReceiveLocation.Address)
                continue
            }

            $Protocol = "http"
            $Url = [String]::Format("{0}://{1}{2}" , $Protocol , $BiztalkServerAddress , $ReceiveLocation.Address)
            $Response = $null

            $HttpHeader = @{
                "Accept"="*/*"
                "Content-Type"="application/edifact"
                "Mime-Version"="1.0"
            }

            $HttpBody = "Health Check message"

            $WCFHeader = @{
                "Accept"="*/*"
                "Content-Type"="text/xml"
                "Mime-Version"="1.0"
            }

            $WCFBody = "<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
    <s:Header>
    </s:Header>
    <s:Body xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">    
    </s:Body>
</s:Envelope>"

            try {
                if ($ReceiveLocation.TransportType.Name.Equals("HTTP")) {
                    $Response = Invoke-WebRequest -URI $Url -Method Post -Headers $HttpHeader -Body $HttpBody
                }
                
                if ($WCFAdapters -contains $ReceiveLocation.TransportType.Name) {
                    $Response = Invoke-WebRequest -URI $Url -Method Post -Headers $WCFHeader -Body $WCFBody
                }
            }
            catch [System.Net.WebException]{
                $Response = $_.Exception.Response
            } catch {
                $State = "Critical"
                $Description = $Description + [string]::Format("ERROR: An exception was thrown during health check. Message: {0}`n", $_.Exception.Message)
            }
            
            if ($null -ne $Response -and [int]$Response.StatusCode -ne 200 -and [int]$Response.StatusCode -ne 202) {
                $State = "Critical"
                $Description = $Description + [string]::Format("ERROR: Receive response: {0} {1} after sending health check message to {2}`n", [int]$Response.StatusCode, $Response.StatusDescription, $ReceiveLocation.Address)
            }
        }
    }
}