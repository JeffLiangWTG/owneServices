for ($i = 0; $i -lt 10000; $i++) {
    $hdrs = @{ "X-Sender" = "CCCCCC"; "X-Recipient" = "YYYYYY"; "X-TrackingId" = [Guid]::NewGuid().ToString() }
    $rsp = Invoke-WebRequest -Method Post -Uri http://localhost:7004/v1/messages/ -Headers $hdrs -InFile C:\eServices\HawkingPOC\CSI\Testing\TestFiles\MessageY1_100_AAAAAA.json
    "$i $($rsp.StatusDescription)"
}