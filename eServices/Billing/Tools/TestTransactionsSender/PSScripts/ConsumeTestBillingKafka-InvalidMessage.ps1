<#
Run this script tool to consume only when there are invalid message(s) from Test Billing Kafka.
#>

$root = "C:\Tools\kafka"
$bin = "\bin\windows"
$bootstrapServers = "1.test-1.kafka.wtg.ws:9093,2.test-1.kafka.wtg.ws:9093,3.test-1.kafka.wtg.ws:9093,4.test-1.kafka.wtg.ws:9093,5.test-1.kafka.wtg.ws:9093"
$clientProperties = "C:\wtg\eServices\Billing\Tools\TestTransactionsSender\PSScripts\client.properties"
$topic = "topic-au1-test-billing-test"
$groupId = "eServices-billing-transactions-consumer"
$maxMessageNumber = 1
$timeoutMs = 3000

& "$root$bin\kafka-console-consumer.bat" `
    --bootstrap-server $bootstrapServers --topic $topic --group $groupId `
    --from-beginning `
    --max-messages $maxMessageNumber `
    --timeout-ms $timeoutMs `
    --formatter kafka.tools.DefaultMessageFormatter `
    --property print.timestamp=true --property print.key=true --property print.value=true `
    --consumer.config $clientProperties

Write-Output "Message(s) have been successfully consumed from '$bootstrapServers'-'$topic' with '$groupId'."
