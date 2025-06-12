scoW<!-- Markdown Cheat Sheet: https://www.markdownguide.org/cheat-sheet -->
# What is SCOM Alert?
System Center Operations Manager (SCOM) uses a single interface that shows state, health and performance information of computer systems. It also provides alerts generated according to some availability, performance, configuration or security situation being identified. When an interface had a problem, the related alerts went on and stay until someone turns it off. When an alert is on, there will be no alert until the current alert is going off.

# What should you do when having SCOM Alert incident?

#### 1) Install SCOM Monitoring (if you haven't installed before)

- Installing SCOM Console (for administration)
- Mount the SCOM 2012 R2 media – G:\SoftwareStore\Microsoft.com\System Center 2012 R2\System Centre Operations Manager 2012 R2
- Run Setup.exe
- Choose "Install"
- Tick "Operations Console"
- Complete the wizard
- Viewing Alerts
- Start Operations Console
- Server Name: sydco-scom-1.wtg.zone

If you don't have read permission to access, please create an Incident to grant the read access like
- CS00706257 - WISGLOSYD - sydco-scom-1.wtg.zone Insufficient Privileges
- Choose Monitoring -> Active Alerts from the tree
- Also, you can see our active alerts in (WiseTechGlobal - eHub > eHub Alerts) and (Microsoft BizTalk Server 2013R2 > Run-Time Component Views > Run-time alerts > all the view here)

#### 2) Viewing Message Logs

- Web Viewer; 
    - http://sydco-scom-1.wtg.zone/OperationsManager/default.aspx?ViewType=Operations_Overview&ViewID=Operations_Overview 

- Local Machine; 
    - Start menu -> Type 'Operations Console' -> Enter server name 'sydco-scom-1.wtg.zone' 
    
#### 3) View stuck / delayed messages on eHubAdmin

http://ehubadmin.wtg.zone/Messages?DateFilterType=WithinPeriod&PeriodType=Day&PeriodValue=1
- Refine the search with the 'Query' filters such as; 
    - Any Role and Client ID
        - Eg: SFS = SCHEDULED_FEED_SERVICE
        - Eg: Container Tracking = CONTAINER_TRACKING
    -  Data Range
- Retrieve the PK from the message that suits the search criteria

#### 3) Check if the alert is still on
- Active Alert is viewable if they are ON active. When the problem is resolved, you need to create a task in the incident for IS to turn that one OFF.
- If the alert is OFF, which means you cannot find the alert, just close the incident (current task if there are multiple tasks in the same incident) with the appropriate reason.
![SCOM Alerts are on!](http://tfs.wtg.zone:8080/tfs/CargoWise/_api/_versioncontrol/itemContent?repositoryId=&path=%24%2FeServices%2FDocumentation%2Fimages%2FSCOM.png&contentOnly=true)

#### 4) Investigate if it is still a problem with the alert service:

- If the alert is still on, you will need to investigate if it is still a problem by checking all the performance statistics in SCOM monitoring.(Please add more if you feel this isn't cleared enough)
    - If it isn't:
    	- Check the related statistics graphs to see if the filter is too strict or it is expected behavior such as weekend deployment which causes the small delay to delivery the message and it went back to 0 messages. Create a task for IS team to adjust the alert filter or turn off the alert. For example, the following SCOM spike showing a good flow of delivering messages:
![The spike always go back to 0 after delivering messages](http://tfs.wtg.zone:8080/tfs/CargoWise/_api/_versioncontrol/itemContent?repositoryId=&path=%24%2FeServices%2FDocumentation%2Fimages%2Fgood_SCOM_codition.png&contentOnly=true)
    - If it is:
        - Create work item explaining which is the error is for example "There are 46 Ocean messages are stucking in Inbox. Please prefer to instruction in http://tfs.wtg.zone:8080/tfs/CargoWise/eServices/_versionControl?path=$/eServices/Documentation/SCOM_Alert_Knowledge.md to solve the problem". For example, the SCOM spike will show there are always minimum 46 messages in Ocean's Queue graph:
![The spike stuck at 46](http://tfs.wtg.zone:8080/tfs/CargoWise/_api/_versioncontrol/itemContent?repositoryId=&path=%24%2FeServices%2FDocumentation%2Fimages%2FSCOM_Analysis.png&contentOnly=true)
- If the alert is off, you need to close your task and put a comment explaining the problem is solved and the alert was turnt off on DateTime.Now.

#### 5) Helpful SQL query to speed up the investigation:

Age: SELECT [eHubTransactions].[dbo].GetInboxMessageAge('{ID}')

Queue: SELECT [eHubTransactions].[dbo].GetInboxMessageCount('{ID}')

Response: SELECT NoResponseCount, ResponseCount FROM [eHubTracking].[dbo].GetResponseCountCheck('{ID}')


# What should you do when having SCOM Alert Work Item?
- Refer to section (3) before, check if the problem is still there.
- Check if your alert is inside [SCOM Alerts list](#scom-alerts-list) and follow the given solutions, if it isn't there, creating the content for that undocumented information.

# SCOM Alerts list

## Common Solutions

### Note to resubmit messages:
- Write a query to update status of Outbox Message or Inbox Message to 0 with specific PK to avoid any mistakes. For example:  

```
UPDATE [eHubTransactions].[dbo].eHubInboxMessage
SET EI_Status = 0
WHERE EI_PK IN ('0A18052A-60DD-4B10-98F0-25C28B87AF2D','E39E27C7-280A-4CE0-8A4C-B466CCB1C333','5651A2F0-75D3-4ECC-902C-A2F2DC14EC8B')
```

### Looking for any problem it might occurs during the whole proccess
- Refer the Biztalk integration diagram in https://wisetechglobal.sharepoint.com/Development/Development%20Team%20Workspace/Shared%20Documents/eServices/Architecture/BizTalk%20Integration%20Diagram.vsdx?d=wd70075c1dd3f4db2951a25db0eee2b7c to understand the whole process and look into each component to indentify if there is any potential risk/bug could happen and improve it(improve logging, improve logic). Please update this diagram if you have something to add up.

### Known Issues:

## eHub Performance

### CONTAINER_TRACKING

Time Added: 11/01/2018 23:34:48  
Path: ehubtransactions.db.wisegrid.net  
Alert Id: a9268095-f8d7-4b6a-b0c0-d8c9142f673c  
Rule Id: b808c153-9f34-9bd5-ded5-2915a15d7f77  
Priority: 1  
Severity: 2  
Occurrences: 0  
Entity Name: WTG.EHUB.AU.eHub  
State: Bad  
Description: Container Tracking messages through eHub are delayed by over 15 mins. Message age from eHub is 149001 sec, 4 messages in queue.  

#### Summary

There are messages in eHubInbox which are stuck from CONTAINER_TRACKING for more than 15 mins.  
Queries to trigger this alert from ehubtransactions.db.wisegrid.net server:  
```
SELECT [eHubTransactions].[dbo].GetInboxMessageAge('CONTAINER_TRACKING') -- Age
SELECT [eHubTransactions].[dbo].GetInboxMessageCount('CONTAINER_TRACKING') -- Queue
```
#### Causes

There are couple of known and unknown reasons for this issue; 
- For known issues, we already know that our CargoWise.eHub.Core.PipelineComponents.UpdateStatusEvent BAM activity task could fail to update the status when the server is restarted so there are multiple times that message is sent or failed but failed to be upgraded to final status(3,255). You will need to check log in \\sydwp-sbts-1.wisecloud.zone\Logs using username: sydwp-sbts-1.wisecloud.zone\eHubReader, password: 3hubRock$ if the message was delivered.
- For unknown issues, you will need to reproduce the issue in your local by copying WG1-VSQL-1 product sql data to your local and resubmit stuck messages by resetting  OutBox status to 0 if having outbox message, otherwise reset Inbox status to 0.

#### Useful Queries 

-
```
SELECT * FROM eHubInboxMessageExp
LEFT JOIN eHubOutboxMessageExp ON OI_EI_InboxPK = CAST(EI_PK AS varchar(36))
WHERE EI_PK IN ('0A18052A-60DD-4B10-98F0-25C28B87AF2D','E39E27C7-280A-4CE0-8A4C-B466CCB1C333','5651A2F0-75D3-4ECC-902C-A2F2DC14EC8B')
```

```
UPDATE [eHubTransactions].[dbo].eHubInboxMessage
SET EI_Status = 0
WHERE EI_PK IN ('0A18052A-60DD-4B10-98F0-25C28B87AF2D','E39E27C7-280A-4CE0-8A4C-B466CCB1C333','5651A2F0-75D3-4ECC-902C-A2F2DC14EC8B')
```

#### Solutions

- If it is a known issue, decide to use DeliveryNotification (please discuss with George or Geoff) in the new work item. 
- If it is an unknown issue, code review the whole process again to detect if having any hole then providing more logs or fixing the issue within this incident.
    - After this, find out if the message is deliveried by looking the logs and update it to the final status if it was attempted to update. If there is no information to follow up, please ask the responsible product manager if it is fine to resubmit the messages. If there is no responsible product manager or you cannot have the answer, please resubmit the message by providing the SQL script in "Prepare deployment request" task.
- Create the task at the end of the work item for IS team to turn off the alert.


### eHubAirService

Time Added: 11/01/2018 23:34:47  
Path: ehubtransactions.db.wisegrid.net  
Alert Id: f481385b-e371-4afe-be85-a1f084b2eedd  
Rule Id: 56977b7f-412f-af73-cce9-a785d98ddd77  
Priority: 1  
Severity: 2  
Occurrences: 0  
Entity Name: WTG.EHUB.AU.eHub  
State: Bad  
Description: AIR messages through eHub are delayed by over 15 mins. Message age from eHub is 162491 sec, 57 messages in queue.  

#### Summary

There are messages in eHubInbox which are stuck from CONTAINER_TRACKING for more than 15 mins.  
Queries to trigger this alert from ehubtransactions.db.wisegrid.net server:  
```
SELECT [eHubTransactions].[dbo].GetInboxMessageAge('eHubAirService') -- Age
SELECT [eHubTransactions].[dbo].GetInboxMessageCount('eHubAirService') -- Queue
```

#### Causes

There are couple of known and unknown reasons for this issue; 
- For known issues, we already that our CargoWise.eHub.Core.PipelineComponents.UpdateStatusEvent BAM activity task could fail to update the status when the server is restarted so there are multiple times that message are sent or failed but failed to be upgraded to final status(3,255). You will need to check log in \\sydwp-sbts-1.wisecloud.zone\Logs using username: sydwp-sbts-1.wisecloud.zone\eHubReader, password: 3hubRock$ if the message was delivered.
- For unknown issues, you will need to reproduce the issue in your local by copying WG1-VSQL-1 product sql data to your local and resubmit stucked messages by reset OutBox status to 0 if having outbox message, otherwise reset Inbox status to 0.

#### Solutions

- If it is a known issue, decide to use DeliveryNotification (please discuss with George or Geoff) in the new work item. 
- If it is an unknown issue, code review the whole process again to detect if having any hole then providing more logs or fixing the issue within this incident.- 
- Next, confirm if the message is delivered  by looking the logs and update it to the final status if it was attempted to update. 
    - If there is no information to follow up, please ask the responsible product manager to confirm that client has received or not and try to resubmit the messages if it is fine and the messages are being held < 1 week otherwise we should fail messages for avoiding any client's concern by providing the SQL script in "Prepare deployment request" task.
- Create the task at the end of the work item for IS team to turn off the alert.

### SCHEDULE_FEED_SERVICE

Time Added: 11/01/2018 23:34:48  
Path: ehubtransactions.db.wisegrid.net  
Alert Id: 75b60811-bd49-4e3f-9c3c-c5da6847d510  
Rule Id: bfd412d4-1637-e862-f52e-5441f7279731  
Priority: 1  
Severity: 2  
Occurrences: 0  
Entity Name: WTG.EHUB.AU.eHub  
State: Bad  
Description: SFS messages through eHub are delayed by over 15 mins. Message age from eHub is 148907 sec, 6 messages in queue.  

#### Summary

There are messages in eHubInbox which are stuck from CONTAINER_TRACKING for more than 15 mins.
Queries to trigger this alert from ehubtransactions.db.wisegrid.net server:  
```
SELECT [eHubTransactions].[dbo].GetInboxMessageAge('SCHEDULE_FEED_SERVICE') -- Age
SELECT [eHubTransactions].[dbo].GetInboxMessageCount('SCHEDULE_FEED_SERVICE') -- Queue
```

#### Causes

There are couple of known and unknown reasons for this issue; 
- For known issues, we already that our CargoWise.eHub.Core.PipelineComponents.UpdateStatusEvent BAM activity task could fail to update the status when the server is restarted so there are multiple times that message are sent or failed but failed to be upgraded to final status(3,255). You will need to check log in \\sydwp-sbts-1.wisecloud.zone\Logs using username: sydwp-sbts-1.wisecloud.zone\eHubReader, password: 3hubRock$ if the message was delivered.
- For unknown issues, you will need to reproduce the issue in your local by copying WG1-VSQL-1 product sql data to your local and resubmit stucked messages by reset OutBox status to 0 if having outbox message, otherwise reset Inbox status to 0.

#### Solutions

- For the deployment, we just need to requeue the stuck messages.
- If it is a known issue, decide to use DeliveryNotification (please discuss with George or Geoff) in the new work item. 
- If it is an unknown issue, code review the whole process again to detect if having any hole then providing more logs or fixing the issue within this incident.
- Next, confirm if the message is delivered by looking the logs and update it to the final status if it was attempted to update. 
    - If there is no information to follow up, please ask the responsible product manager if it is fine to resubmit the messages. 
    - If there is no responsible product manager or you cannot have the answer, please resubmit the message by providing the SQL script in "Prepare deployment request" task.
- Create the task at the end of the work item for IS team to turn off the alert.

#### Useful Queries 

Used to retrieve the PKs of the messages that are being sent between the Inbox and Outbox tables at any particular moment. 
```
Select * from [eHubTransactions].[dbo].[eHubInboxMessageExp] i WITH(NOLOCK)
JOIN [eHubTransactions].[dbo].[ediProdClient] WITH(NOLOCK) ON CC_ID_Sender = CC_ID AND LD_LicenceType = 'PRD' 
LEFT JOIN [eHubTransactions].[dbo].[eHubOutboxMessage] o ON OI_EI_InboxPK = CAST(EI_PK AS varchar(36))
WHERE EI_Status < 3 AND i.CC_ID_Recipient = 'SCHEDULE_FEED_SERVICE'
```

Retrieve the specific SFS message that is stuck. 
The left JOIN caluse combines data from the Inbox and Outbox tables. 
```
SELECT * FROM eHubInboxMessageExp i
LEFT JOIN eHubOutboxMessageExp o ON OI_EI_InboxPK = CAST(EI_PK AS varchar(36))
WHERE EI_PK = '43269D78-22F8-42E7-A381-BA737A735BAF'
```

The PK could be verified by searching for 'SCHEDULED_FEED_SERVICE' message type from eHubAdmin 
```
http://ehubadmin.wtg.zone/Messages?Role=Any&AnyRole=SCHEDULE_FEED_SERVICE&DateFilterType=WithinPeriod&PeriodType=Day&PeriodValue=1&Status=Not%20Processed&KeyType=EI_PK
```

Solve this problem by setting the EI_Status value = 0. This enables the message send sequence to restart.
``` 
UPDATE eHubInboxMessage
Set EI_Status = 0
WHERE EI_PK = '43269D78-22F8-42E7-A381-BA737A735BAF';
```

### SHIPPING_INSTRUCTION

Time Added: 11/05/2018 10:47:56  
Path: ehubtransactions.db.wisegrid.net  
Alert Id: 545afcd8-acb4-4994-8da8-dc26865d0be5  
Rule Id: a35ddd55-e9cb-3127-4b87-6ee2eb9e40a9  
Priority: 1  
Severity: 1  
Occurrences: 0  
Entity Name: OCEAN messages through eHub are delayed by over 15 mins. Message age from eHub is 4857399 sec, 132 messages in queue.  

#### Summary

There are messages in eHubInbox which are stuck from SHIPPING_INSTRUCTION for more than 15 mins.
Queries to trigger this alert from ehubtransactions.db.wisegrid.net server:  
```
SELECT [eHubTransactions].[dbo].GetInboxMessageAge('SHIPPING_INSTRUCTION') -- Age
SELECT [eHubTransactions].[dbo].GetInboxMessageCount('SHIPPING_INSTRUCTION') -- Queue
```

#### Causes

There are couple of known and unknown reasons for this issue; 
- For known issues, we already that our CargoWise.eHub.Core.PipelineComponents.UpdateStatusEvent BAM activity task could fail to update the status when the server is restarted so there are multiple times that message are sent or failed but failed to be upgraded to final status(3,255). You will need to check log in \\sydwp-sbts-1.wisecloud.zone\Logs using username: sydwp-sbts-1.wisecloud.zone\eHubReader, password: 3hubRock$ if the message was delivered.
- For unknown issues, you will need to reproduce the issue in your local by copying WG1-VSQL-1 product sql data to your local and resubmit stucked messages by reset OutBox status to 0 if having outbox message, otherwise reset Inbox status to 0.

#### Solutions

- If it is a known issue, decide to use DeliveryNotification (please discuss with George or Geoff) in the new work item. 
- If it is an unknown issue, code review the whole process again to detect if having any hole then providing more logs or fixing the issue within this incident.- 
- Next, confirm if the message is delivered by looking at the logs and update it to the final status if it was attempted to update. 
   - If there is no information to follow up, please ask the responsible product manager if it is fine to resubmit the messages. 
   - If there is no responsible product manager or you cannot have the answer, please resubmit the message by providing the SQL script in "Prepare deployment request" task.
- Create the task at the end of the work item for IS team to turn off the alert.

#### Fixed versions:

- 29-11-2018: Update WARN logs to support future investigation. Problem was fixed on 28-11-2018 6:00PM without any notice from eServices.
If the error from Orchestration, you will able to find error log as WARN in \\sydwp-sbts-1.wisecloud.zone\Logs\BizTalk\Orchestrations\CargoWise.eHub.Products.OceanCarrierMessaging.Orchestrations.Carrier_Email.  
If the error from CargoWise.eHub.Products.OceanCarrierMessaging.Orchestrations_1.0.0.0_CargoWise.eHub.Products.OceanCarrierMessaging.Orchestrations.Carrier_Email_SendPort_4f570df270576350, you will see that the message is succesfully sent in the log and the message is failed with 255 status.  

### ZACustoms

Time Added: 11/04/2018 11:58:56  
Path: ehubtransactions.db.wisegrid.net  
Alert Id: 545afcd8-acb4-4994-8da8-dc26865d0be5  
Rule Id: a35ddd55-e9cb-3127-4b87-6ee2eb9e40a9  
Priority: 1  
Severity: 2  
Occurrences: 0
State: Bad  
Entity Name: ZACustoms messages through eHub are delayed by over 10 mins. Message age from eHub is 2330860 sec   

#### Summary 

There are messages in eHub which are stuck for more than 10 mins.
The Message Age in this instance is over 2330000 secs. There are 2 messages in queue. 
Checking this alert from ehubtransactions.db.wisegrid.net
```
SELECT [eHubTransactions].[dbo].GetInboxMessageCount('ZACustoms') -- Queue
SELECT [eHubTransactions].[dbo].GetInboxMessageAge('ZACustoms') -- Age
```

#### Causes 

There are couple of known and unknown reasons for this issue; 
- For known issues, we already that our CargoWise.eHub.Core.PipelineComponents.UpdateStatusEvent BAM activity task could fail to update the status when the server is restarted so there are multiple times that message are sent or failed but failed to be upgraded to final status(3,255). You will need to check log in \\sydwp-sbts-1.wisecloud.zone\Logs using username: sydwp-sbts-1.wisecloud.zone\eHubReader, password: 3hubRock$ if the message was delivered.
- For unknown issues, you will need to reproduce the issue in your local by copying WG1-VSQL-1 product sql data to your local and resubmit stucked messages by reset OutBox status to 0 if having outbox message, otherwise reset Inbox status to 0.

#### Solutions

- If it is a known issue, decide to use DeliveryNotification (please discuss with George or Geoff) in the new work item. 
- If it is an unknown issue, code review the whole process again to detect if having any hole then providing more logs or fixing the issue within this incident.
- Next, confirm if the message is delivered by looking the logs and update it to the final status if it was attempted to update. 
    - If there is no information to follow up, please ask the responsible product manager if it is fine to resubmit the messages. 
    - If there is no responsible product manager or you cannot have the answer, please resubmit the message by providing the SQL script in "Prepare deployment request" task.
- Create the task at the end of the work item for IS team to turn off the alert.

#### Useful Queries 

```
BEGIN TRAN
USE eHubTransactions
GO

DECLARE @CurrentTime DateTime = GETDATE()

EXEC [dbo].[InsertError] @ErrorPK = ['Insert GUID of PK'], @Source = 'BIZ', @ErrorType = 'Fai', @Description = 'Manual Failed Message for ZACustoms', @InboxPK = ['Insert Inbox GUID'], @OutboxPK = ['Insert Outbox GUID'], @CurrentDateTimeUTC = @CurrentTime
EXEC [dbo].[InsertError] @ErrorPK = 'A5BF1DA1-34A8-4983-B752-82740D194151', @Source = 'BIZ', @ErrorType = 'Fai', @Description = 'Manual Failed Message for ZACustoms', @InboxPK = 'A3106A84-145C-4230-A2B6-EF78A29B384A', @OutboxPK = '10042FBC-6BA5-4E50-9361-BC9582A8E2DC', @CurrentDateTimeUTC = @CurrentTime
EXEC [dbo].[InsertError] @ErrorPK = ['Insert GUID of PK'], @Source = 'BIZ', @ErrorType = 'Fai', @Description = 'Manual Failed Message for ZACustoms', @InboxPK = ['Insert Inbox GUID'], @OutboxPK = null, @CurrentDateTimeUTC = @CurrentTime
EXEC [dbo].[InsertError] @ErrorPK = '05061F4C-2D47-40FE-B5F7-2DAFB0FB39C1', @Source = 'BIZ', @ErrorType = 'Fai', @Description = 'Manual Failed Message for ZACustoms', @InboxPK = '9D347475-739D-4AF2-9D0A-D216D3EF8B4C', @OutboxPK = null, @CurrentDateTimeUTC = @CurrentTime
									 
COMMIT
--ROLLBACK
```

### Deployment 

How to Deploy for Testing: Prepare for Deployment Task.

- Attach the SQL query that was developed for the problem as an eDoc. 
- Change the Document Type to 'SCR' (Script) 
- Copy link from eDoc SCR document, right-click -> copy-link ID
- Open 'Confirms Ready to Deploy Task' and past link ID
- Close Prepare Deployment task. 

### AUCustoms

Time Added: 12/07/2019 10:24:06 AM  
Path: ehubtransactions.db.wisegrid.net\WTG.EHUB.AU.eHub\Australian Customs
Alert Id: 7b361b10b9-13ca-46da-89eb-1e03eb8b616b%7d
Priority: 1  
Severity: 1  
Occurrences: 0
Entity Name: In the last sample, there have been 0 response messages received by the tracking system. 4 responses were expected. These responses are an acknowledgement of receipt from the ultimate endpoint of the message.  

#### Summary

In the last sample, there have been 0 response messages received by the tracking system. 4 responses were expected. These responses are an acknowledgement of receipt from the ultimate endpoint of the message.
Queries to trigger this alert from ehubtransactions.db.wisegrid.net server:  

#### Causes

AU Customs stopped to send back responses for WTG tracking messages due to the certificate used for body message encryption has expired. WTG certificate expires in July every 2 years.

#### Solutions

- Request new certificate to WTG Customs team 
- Update project CargoWise.eHub.Tracking.TestMessageService - Handlers - CustomsMessages - AUCustomsMsgCert.pfx certificate
- Deploy changes to production

