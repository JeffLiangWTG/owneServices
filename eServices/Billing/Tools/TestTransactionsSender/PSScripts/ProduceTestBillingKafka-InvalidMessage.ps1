<#
Run this script tool to send an INVALID message to Test Billing Kafka.
The Kafka queue would be blocked for the partition that the message is distributed to.
#>

$root = "C:\Tools\kafka"
$bin = "\bin\windows"
$bootstrapServers = "1.test-1.kafka.wtg.ws:9093,2.test-1.kafka.wtg.ws:9093,3.test-1.kafka.wtg.ws:9093,4.test-1.kafka.wtg.ws:9093,5.test-1.kafka.wtg.ws:9093"
$clientProperties = "C:\wtg\eServices\Billing\Tools\TestTransactionsSender\PSScripts\client.properties"
$topic = "topic-au1-test-billing-test"
$message = "1:I am an INVALID message"

$message | & "$root$bin\kafka-console-producer.bat" `
    --broker-list $bootstrapServers `
	--topic $topic `
    --property parse.key=true  `
	--property key.separator=: `
    --producer.config $clientProperties

Write-Output "Message has been successfully produced to '$bootstrapServers'-'$topic': $message"
