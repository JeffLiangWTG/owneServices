using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Freight;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MasterFiles.Business;
using MimeKit;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class BounceBackEmailDetailsTest : TestCaseWithFactory
	{
		public void TestEmailConstructor2()
		{
			string eml = @"--------------------------------------------------------------------------------

MAIN Exception

Enterprise.Client.EDI.ServiceTasks.EndpointMonitoringException: Endpoint [https://ordwebservices.wisegrid.net:443/addresscleansing/v2/GetServiceHealth?runComprehensiveCheck=False] is unreachable.
   at Enterprise.Client.EDI.ServiceTasks.MonitoredEndpoint.QueryServiceHealth(String url)
   at Enterprise.Client.EDI.ServiceTasks.MonitoredEndpoint.QueryServiceHealth()
   at Enterprise.Client.EDI.ServiceTasks.AddressValidationEndpointMonitoringServiceTask.<QueryEndpoints>d__7.MoveNext()

--------------------------------------------------------------------------------

INNER Exception

System.Net.Http.HttpRequestException: An error occurred while sending the request.
No Stack Trace

-----";
			var bounceDetails = new BounceBackEmailDetails(Factory, Encoding.UTF8.GetBytes(eml));
			AssertEquals("Entire email should be the same as input", eml, bounceDetails.GetEml());
			AssertEquals("Body should be eml since IMail.Text is Empty", eml, bounceDetails.Body);
			AssertEquals("From address should be blank", "", bounceDetails.From);
			AssertContainsExactElementsInAnyOrder("There should be no BouncedRecipients", System.Array.Empty<string>(), bounceDetails.BouncedRecipients.Select(x => x.Address));
			AssertEquals("Bounce back campaignItemID should be an empty Guid", ZGuid.Empty, bounceDetails.BusinessEntityID);
			AssertEquals("", bounceDetails.BounceReason);
			AssertEquals("UNV", bounceDetails.BounceReasonCode);
			AssertNullOrEmpty(ErrorReporter.LastKeyReported);
		}

		public void TestEmailConstructor()
		{
			string eml = "a";
			var bounceDetails = new BounceBackEmailDetails(Factory, Encoding.UTF8.GetBytes(eml));

			AssertEquals("Entire email should be the same as input", "a", bounceDetails.GetEml());
			AssertEquals("Body should be eml since IMail.Text is Empty", "a", bounceDetails.Body);
			AssertEquals("From address should be blank", "", bounceDetails.From);
			AssertContainsExactElementsInAnyOrder("There should be no BouncedRecipients", System.Array.Empty<string>(), bounceDetails.BouncedRecipients.Select(x => x.Address));
			AssertEquals("Bounce back campaignItemID should be an empty Guid", ZGuid.Empty, bounceDetails.BusinessEntityID);
			AssertEquals("", bounceDetails.BounceReason);
			AssertEquals("UNV", bounceDetails.BounceReasonCode);
			AssertNullOrEmpty(ErrorReporter.LastKeyReported);
		}

		public void TestSender()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AAA";
			staff.GS_EmailAddress = "AAA@123.com";
			Factory.Save();

			var bounceDetails = new EmailBuilderForTesting().GetNewBounceDetails(Factory);
			AssertEquals("", bounceDetails.SenderAddress);
			AssertEquals("", bounceDetails.SenderName);
			AssertEquals("", bounceDetails.SenderNameAddress);
			AssertEquals(ZGuid.Empty, bounceDetails.SenderStaffID);

			var message = new MimeMessage();
			message.Sender = new MailboxAddress("Name", "address@test.com");
			message.Headers.Add("X-SenderStaffID", staff.PK.ToString());
			bounceDetails = new BounceBackEmailDetails(Factory, message.GetData());
			AssertEquals("address@test.com", bounceDetails.SenderAddress);
			AssertEquals("Name", bounceDetails.SenderName);
			AssertEquals("\"Name\" <address@test.com>", bounceDetails.SenderNameAddress);
			AssertEquals(staff.PK, bounceDetails.SenderStaffID);

			message = new MimeMessage();
			message.Sender = new MailboxAddress("", "address@test.com");
			bounceDetails = new BounceBackEmailDetails(Factory, message.GetData());
			AssertEquals("address@test.com", bounceDetails.SenderAddress);
			AssertEquals("", bounceDetails.SenderName);
			AssertEquals("<address@test.com>", bounceDetails.SenderNameAddress);

			bounceDetails = new EmailBuilderForTesting().From("From", "from@test.com").GetNewBounceDetails(Factory);
			AssertEquals("from@test.com", bounceDetails.SenderAddress);
			AssertEquals("From", bounceDetails.SenderName);
			AssertEquals("\"From\" <from@test.com>", bounceDetails.SenderNameAddress);
		}

		public void TestBusinessEntity()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Bob Powell";
			contact.OC_Email = "powell@live.com";

			var campaignItem = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			campaignItem.G8_TrackingStatus = "UNV";
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_RecipientTableCode = "OC";

			var shipment = (BusinessObject)Factory.New<ICommonShipment>();
			shipment.FillWithValidTestData();
			var job = new JobHeader.Loader((IJobHeaderParent)shipment).TryCreate();
			job.FillWithValidTestData();

			Factory.Save();

			var bounceDetails = new EmailBuilderForTesting().GetNewBounceDetails(Factory);
			AssertNull(bounceDetails.BusinessEntity);
			AssertEquals(ZGuid.Empty, bounceDetails.BusinessEntityID);
			AssertEquals("", bounceDetails.BusinessEntityTableCode);
			AssertEquals("", bounceDetails.BusinessEntityJobNumber);

			var message = new MimeMessage();
			message.Headers.Add("X-BusinessEntityTableCode", campaignItem.TablePrefix);
			message.Headers.Add("X-BusinessEntityID", campaignItem.PK.ToString());
			bounceDetails = new BounceBackEmailDetails(Factory, message.GetData());
			AssertNotNull(bounceDetails.BusinessEntity);
			AssertEquals(campaignItem.PK, bounceDetails.BusinessEntityID);
			AssertEquals(campaignItem.TablePrefix, bounceDetails.BusinessEntityTableCode);
			AssertEquals("", bounceDetails.BusinessEntityJobNumber);

			message = new MimeMessage();
			message.Headers.Add("X-BusinessEntityTableCode", shipment.TablePrefix);
			message.Headers.Add("X-BusinessEntityID", shipment.PK.ToString());
			bounceDetails = new BounceBackEmailDetails(Factory, message.GetData());
			AssertNotNull(bounceDetails.BusinessEntity);
			AssertEquals(shipment.PK, bounceDetails.BusinessEntityID);
			AssertEquals(shipment.TablePrefix, bounceDetails.BusinessEntityTableCode);
			AssertNotEquals("", bounceDetails.BusinessEntityJobNumber);

			message = new MimeMessage();
			message.Headers.Add("X-BusinessEntityTableCode", job.TablePrefix);
			message.Headers.Add("X-BusinessEntityID", job.PK.ToString());
			bounceDetails = new BounceBackEmailDetails(Factory, message.GetData());
			AssertNotNull(bounceDetails.BusinessEntity);
			AssertEquals(job.PK, bounceDetails.BusinessEntityID);
			AssertEquals(job.TablePrefix, bounceDetails.BusinessEntityTableCode);
			AssertNotEquals("", bounceDetails.BusinessEntityJobNumber);
		}

		public void TestBouncedMail()
		{
			BounceBackEmailDetails email = new EmailBuilderForTesting().GetNewBounceDetails(Factory);
			AssertEquals(email.GetEml(), email.Body);
			AssertEquals(ZGuid.Empty, email.BusinessEntityID);
			AssertEquals(ZString.Empty, email.DocumentName);
			AssertEquals("", email.BounceReason);
			AssertEquals("UNV", email.BounceReasonCode);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), email.BouncedRecipients.Select(x => x.Address));

			string messageBody = @"MailEnable: Message could not be delivered to some recipients.
The following recipient(s) could not be reached:

	Recipient: [SMTP:neddy21010@yahoo.com]
	Reason: 554 delivery error: dd This user doesn't have a yahoo.com account (neddy21010@yahoo.com) [0] - mta1685.mail.gq1.yahoo.com

Message contents follow:

Received: from SYD-WEDW-1 ([10.61.3.58]) by wisetechglobal.com with MailEnable ESMTP; Tue, 11 Mar 2014 11:26:18 +1100
mime-version: 1.0
X-BusinessEntityID: 01a37f2d-4146-4c55-bea2-aea782aea6df
X-BusinessEntityTableCode: G8
X-DocumentName: Document Arrival
X-SenderStaffID: E05624C0-45BF-47BA-AC62-1458052FE9CB
content-transfer-encoding: 7bit
content-type: text/html;
	charset='utf-8'
subject: Email tester campaign
date: Tue, 11 Mar 2014 11:24:57 +1100
from: 'Edward Onwodi' <Support@enterprisedevelopment.cargowise.com>
reply-to: <edward.onwodi@cargowise.com>
to: <neddy21010@yahoo.com>

140305_PRINT_PREAP_Email tester campaign";

			EmailBuilderForTesting builder = new EmailBuilderForTesting();
			builder.To("Support", "support@wisetechglobal.com");
			builder.Body(messageBody);
			email = builder.GetNewBounceDetails(Factory);
			AssertEquals(messageBody, email.Body);
			AssertEquals("01a37f2d-4146-4c55-bea2-aea782aea6df", email.BusinessEntityID.ToString());
			AssertEquals("G8", email.BusinessEntityTableCode);
			AssertEquals("Document Arrival", email.DocumentName);
			AssertEquals("E05624C0-45BF-47BA-AC62-1458052FE9CB", email.SenderStaffID.ToString().ToUpperInvariant());
			AssertEquals("Tue, 11 Mar 2014 11:24:57 +1100", email.SentTimeText);
			AssertMultilineASCIIEquals("BounceReason", "554 delivery error: dd This user doesn't have a yahoo.com account (neddy21010@yahoo.com) [0] - mta1685.mail.gq1.yahoo.com", email.BounceReason);
			AssertEquals("554", email.BounceReasonCode);
			AssertContainsExactElementsInAnyOrder(new[] { "neddy21010@yahoo.com" }, email.BouncedRecipients.Select(x => x.Address));
		}

		public void TestBouncedRecipients_ShouldUseXFailedRecipientsHeaderIfAvailable()
		{
			var message = new MimeMessage();
			var bodyBuilder = new BodyBuilder();
			message.Headers.Add("X-Failed-Recipients", "Andrew <andrew.luong@wisetechglobal.com>, richard.smith@wisetechglobal.com");
			bodyBuilder.TextBody = "Failed to send! But i don't include original headers in my response.";
			message.Body = bodyBuilder.ToMessageBody();
			var eml = message.GetData();
			var bounceDetails = new BounceBackEmailDetails(Factory, eml, eml);
			AssertContainsExactElementsInAnyOrder("There should be two BouncedRecipients", new[] { "andrew.luong@wisetechglobal.com", "richard.smith@wisetechglobal.com" }, bounceDetails.BouncedRecipients.Select(x => x.Address));
		}

		public void TestBouncedRecipients_ShouldUseRecipientsFromBounceResultIfAvailable()
		{
			var message = new MimeMessage();
			var bodyBuilder = new BodyBuilder();
			bodyBuilder.TextBody = @"This is a MIME-encapsulated message.

--D7AEF2B03.1522088361/ordmailedge.wisegrid.net
Content-Description: Notification
Content-Type: text/plain; charset=utf-8
Content-Transfer-Encoding: 8bit

This is the mail system at host ordmailedge.wisegrid.net.

I am sorry to have to inform you that your message could not
be delivered to one or more recipients. It is attached below.

For further assistance, please send mail to postmaster.

If you do so, please include this problem report. You can
delete your own text from the attached returned message.

                   The mail system

<albert@westernoverseas.com>: host
    d109325b.ess.barracudanetworks.com[64.235.150.252] said: 550 No such user
    (albert@westernoverseas.com) (in reply to RCPT TO command)

--D7AEF2B03.1522088361/ordmailedge.wisegrid.net
Content-Description: Delivery report
Content-Type: message/delivery-status
Content-Transfer-Encoding: 8bit

Reporting-MTA: dns; ordmailedge.wisegrid.net
X-Postfix-Queue-ID: D7AEF2B03
X-Postfix-Sender: rfc822; woclax@us.americas.wisegrid.net
Arrival-Date: Mon, 26 Mar 2018 14:19:20 -0400 (EDT)

Final-Recipient: rfc822; albert@westernoverseas.com
Original-Recipient: rfc822;albert@westernoverseas.com
Action: failed
Status: 5.0.0
Remote-MTA: dns; d109325b.ess.barracudanetworks.com
Diagnostic-Code: smtp; 550 No such user (albert@westernoverseas.com)

--D7AEF2B03.1522088361/ordmailedge.wisegrid.net
Content-Description: Undelivered Message Headers
Content-Type: text/rfc822-headers
Content-Transfer-Encoding: 8bit

Return-Path: <woclax@us.americas.wisegrid.net>
Received: from ordmail.wisecloud.zone (ordwp-smai-1.wisecloud.zone [10.1.146.51])
	by ordmailedge.wisegrid.net (Postfix) with ESMTPS id D7AEF2B03
	for <albert@westernoverseas.com>; Mon, 26 Mar 2018 14:19:20 -0400 (EDT)
Received: by ordmail.wisecloud.zone (Postfix, from userid 1002)
	id 51C5795; Mon, 26 Mar 2018 13:19:20 -0500 (CDT)
X-Spam-Checker-Version: SpamAssassin 3.4.0 (2014-02-07) on
	ordwp-smai-1.wisecloud.zone
X-Spam-Level: 
X-Spam-Status: No, score=-0.3 required=5.0 tests=ALL_TRUSTED,
	HTML_IMAGE_ONLY_28,HTML_MESSAGE autolearn=no autolearn_force=no version=3.4.0
Received: from wisetechglobal.com (unknown [10.1.132.43])
	by ordmail.wisecloud.zone (Postfix) with ESMTPSA id 2992392
	for <albert@westernoverseas.com>; Mon, 26 Mar 2018 13:19:20 -0500 (CDT)
Content-Type: multipart/related;
 boundary='----=_NextPart_1323507.092707303015'
MIME-Version: 1.0
Date: Mon, 26 Mar 2018 13:21:50 -0500
Message-ID: <208ba6ae-c70a-47b3-89d8-f5475db1870f@mail.dll>
Subject: Importer Security Filing Response for ISF0084359
 (Importer=SNOMONWMZ, HouseBill=SHCWQD1803SS0030)
From: 'Western Overseas Corp' <woclax@us.americas.wisegrid.net>
To: <albert@westernoverseas.com>

--D7AEF2B03.1522088361/ordmailedge.wisegrid.net--";

			message.Body = bodyBuilder.ToMessageBody();
			var eml = message.GetData();
			var bounceDetails = new BounceBackEmailDetails(Factory, eml, eml);
			AssertEquals("albert@westernoverseas.com", bounceDetails.BouncedRecipients.Single().Address);
		}

		public void TestGetShortEml()
		{
			EmailBuilderForTesting builder = new EmailBuilderForTesting();
			builder.To("Support", "support@wisetechglobal.com");
			builder.Body(new ZString('a', 5000));
			var email = builder.GetNewBounceDetails(Factory);
			Assert(email.GetEml().Length > 4000);
			var actual = email.GetShortEml();
			AssertEquals(4000, actual.Length);
			AssertEquals(email.GetEml().Substring(0, 4000), actual);
		}

		public void TestParserWithMultipartEML()
		{
			var header = @"MIME-Version: 1.0 
Content-Type: multipart/alternative; boundary===GKLAJ34KJGQKL34098ADLFJKK35

";

			string body = @"

--==GKLAJ34KJGQKL34098ADLFJKK35
Content-Type: text/plain; charset=UTF-8
Content-Transfer-Encoding: base64

WW91ciBtZXNzYWdlDQoNCiAgU3ViamVjdDogQUJDIExMQyAtIEJvb2tpbmcgQ2FydGFnZSBBZHZpY2UgV2l0aCBSZWNlaXB0
IC0gUzEyMzQ1DQoNCndhcyBub3QgZGVsaXZlcmVkIHRvOg0KDQogIGN3MUB1cy5jdzEuY29tDQoNCmJlY2F1c2U6DQoNCiAg
RXJyb3IgdHJhbnNmZXJyaW5nIHRvIG14Yi0zNDVzZGYzLmdzbGIuZ2EzNDY0Lm5ldDsgU01UUCBQcm90b2NvbCBSZXR1cm5l
ZCBhIFBlcm1hbmVudCBFcnJvciA1NTAgNS4xLjEgVW5rbm93biBSZWNpcGllbnQNCg0K

--==GKLAJ34KJGQKL34098ADLFJKK35
Content-Type: message/delivery-status

Reporting-MTA: dns;cw1-smtp.cw1-us.com

Final-Recipient: rfc822;test.email@us.cw1.com
Action: failed
Status: 5.0.0
Remote-MTA: smtp;mxb-cw1.cw1.cw1.COM
Diagnostic-Code: X-Notes; Error transferring to mxb-cw1.cw1.ppho
 sted.COM; SMTP Protocol Returned a Permanent Error 550 5.1.1 Unknown R
 ecipient

--==GKLAJ34KJGQKL34098ADLFJKK35
Content-Type: message/rfc822

Received: from wisetechglobal.com ([5.153.83.4])
          by cw1-smtp.cw1-us.com (IBM Domino Release 9.0.1FP6)
          with ESMTP id 2016082321415803-111703 ;
          Wed, 24 Aug 2016 21:41:58 +0200 
X-BusinessEntityID: b21c55bc-41e1-4c1e-a0ad-3752b89a59da
X-BusinessEntityTableCode: JS
X-DocumentName: Booking Cartage Advice With Receipt
MIME-Version: 1.0
Subject: ABC - Booking Cartage Advice
 With Receipt - e16SfTL0a49749
From: 'MR A' <A.B@cw1-us.Com>
To: <test.email@us.cw1.com>
Return-Path: <abc@cw1-us.com>
Date: Wed, 24 Aug 2016 20:41:56 +0100
Message-ID: <OFBB3BEfE3.613896Ae-ONC1258019.006C365D-C1258019.001C3742@cw1-us.com>
X-MIMETrack: Itemize by SMTP Server on cw1-SMTP/DE/cw1(Release 9.0.1FP6|April  20, 2016) at
 24.08.2016 21:41:58,
	Serialize by POP3 Server on cw1-SMTP/DE/cw1(Release 9.0.1FP6|April
  20, 2016) at 24.08.2016 21:42:17";

			var bounceDetails = new BounceBackEmailDetails(Factory, Encoding.UTF8.GetBytes(body), Encoding.UTF8.GetBytes(header));
			var businessEntityID = bounceDetails.BusinessEntityID;
			var recipients = bounceDetails.BouncedRecipients.ToArray();

			AssertEquals(new ZGuid("b21c55bc-41e1-4c1e-a0ad-3752b89a59da"), businessEntityID);
			AssertEquals(1, recipients.Length);
			AssertEquals("test.email@us.cw1.com", recipients[0].Address);

			AssertEquals(@"Your message

  Subject: ABC LLC - Booking Cartage Advice With Receipt - S12345

was not delivered to:

  cw1@us.cw1.com

because:

  Error transferring to mxb-345sdf3.gslb.ga3464.net; SMTP Protocol Returned a Permanent Error 550 5.1.1 Unknown Recipient


Content-Type: message/delivery-status

Reporting-MTA: dns;cw1-smtp.cw1-us.com

Final-Recipient: rfc822;test.email@us.cw1.com
Action: failed
Status: 5.0.0
Remote-MTA: smtp;mxb-cw1.cw1.cw1.COM
Diagnostic-Code: X-Notes; Error transferring to mxb-cw1.cw1.ppho
 sted.COM; SMTP Protocol Returned a Permanent Error 550 5.1.1 Unknown R
 ecipient

Content-Type: message/rfc822

Received: from wisetechglobal.com ([5.153.83.4])
          by cw1-smtp.cw1-us.com (IBM Domino Release 9.0.1FP6)
          with ESMTP id 2016082321415803-111703 ;
          Wed, 24 Aug 2016 21:41:58 +0200 
X-BusinessEntityID: b21c55bc-41e1-4c1e-a0ad-3752b89a59da
X-BusinessEntityTableCode: JS
X-DocumentName: Booking Cartage Advice With Receipt
MIME-Version: 1.0
Subject: ABC - Booking Cartage Advice
 With Receipt - e16SfTL0a49749
From: 'MR A' <A.B@cw1-us.Com>
To: <test.email@us.cw1.com>
Return-Path: <abc@cw1-us.com>
Date: Wed, 24 Aug 2016 20:41:56 +0100
Message-ID: <OFBB3BEfE3.613896Ae-ONC1258019.006C365D-C1258019.001C3742@cw1-us.com>
X-MIMETrack: Itemize by SMTP Server on cw1-SMTP/DE/cw1(Release 9.0.1FP6|April  20, 2016) at
 24.08.2016 21:41:58,
	Serialize by POP3 Server on cw1-SMTP/DE/cw1(Release 9.0.1FP6|April
  20, 2016) at 24.08.2016 21:42:17
", bounceDetails.Body);
		}

		public void TestParserWithMultipartAlternative_GetBodyText()
		{
			string body = @"Content-Type: multipart/mixed; boundary===GKLAJ34KJGQKL34098ADLFJKK35

--==GKLAJ34KJGQKL34098ADLFJKK35
Content-Type: multipart/alternative; boundary=bcaec54eecc63acce604a3050f77

--bcaec54eecc63acce604a3050f77
Content-Type: text/plain; charset=ISO-8859-1

--
Test Email Text

--bcaec54eecc63acce604a3050f77
Content-Type: text/html; charset=ISO-8859-1

<br clear=""all"">--<br>Test Email Text<br>

--bcaec54eecc63acce604a3050f77--
--==GKLAJ34KJGQKL34098ADLFJKK35
Content -Type: text/plain; charset=UTF-8

Test Email Text not in Multipart Alternative
";

			var bounceDetails = new BounceBackEmailDetails(Factory, Encoding.UTF8.GetBytes(body));

			var expectedBody = @"--
Test Email Text

Test Email Text not in Multipart Alternative
";

			var actualBody = bounceDetails.Body;
			AssertEquals(expectedBody, actualBody);
		}

		public void TestGetBodyText_MultiSections()
		{
			string body = @"Content-Type: multipart/mixed; boundary=D7AEF2B03.1522088361/ordmailedge.wisegrid.net

--D7AEF2B03.1522088361/ordmailedge.wisegrid.net
Content-Description: Notification
Content-Type: text/plain; charset=utf-8
Content-Transfer-Encoding: 8bit

This is the mail system at host ordmailedge.wisegrid.net.

I am sorry to have to inform you that your message could not
be delivered to one or more recipients. It is attached below.

For further assistance, please send mail to postmaster.

If you do so, please include this problem report. You can
delete your own text from the attached returned message.

                   The mail system

<albert@westernoverseas.com>: host
    d109325b.ess.barracudanetworks.com[64.235.150.252] said: 550 No such user
    (albert@westernoverseas.com) (in reply to RCPT TO command)

--D7AEF2B03.1522088361/ordmailedge.wisegrid.net
Content-Description: Delivery report
Content-Type: message/delivery-status
Content-Transfer-Encoding: 8bit

Reporting-MTA: dns; ordmailedge.wisegrid.net
X-Postfix-Queue-ID: D7AEF2B03
X-Postfix-Sender: rfc822; woclax@us.americas.wisegrid.net
Arrival-Date: Mon, 26 Mar 2018 14:19:20 -0400 (EDT)

Final-Recipient: rfc822; albert@westernoverseas.com
Original-Recipient: rfc822;albert@westernoverseas.com
Action: failed
Status: 5.0.0
Remote-MTA: dns; d109325b.ess.barracudanetworks.com
Diagnostic-Code: smtp; 550 No such user (albert@westernoverseas.com)

--D7AEF2B03.1522088361/ordmailedge.wisegrid.net
Content-Description: Undelivered Message Headers
Content-Type: text/rfc822-headers
Content-Transfer-Encoding: 8bit

Return-Path: <woclax@us.americas.wisegrid.net>
Received: from ordmail.wisecloud.zone (ordwp-smai-1.wisecloud.zone [10.1.146.51])
	by ordmailedge.wisegrid.net (Postfix) with ESMTPS id D7AEF2B03
	for <albert@westernoverseas.com>; Mon, 26 Mar 2018 14:19:20 -0400 (EDT)
Received: by ordmail.wisecloud.zone (Postfix, from userid 1002)
	id 51C5795; Mon, 26 Mar 2018 13:19:20 -0500 (CDT)
X-Spam-Checker-Version: SpamAssassin 3.4.0 (2014-02-07) on
	ordwp-smai-1.wisecloud.zone
X-Spam-Level: 
X-Spam-Status: No, score=-0.3 required=5.0 tests=ALL_TRUSTED,
	HTML_IMAGE_ONLY_28,HTML_MESSAGE autolearn=no autolearn_force=no version=3.4.0
Received: from wisetechglobal.com (unknown [10.1.132.43])
	by ordmail.wisecloud.zone (Postfix) with ESMTPSA id 2992392
	for <albert@westernoverseas.com>; Mon, 26 Mar 2018 13:19:20 -0500 (CDT)
Content-Type: multipart/related;
 boundary='----=_NextPart_1323507.092707303015'
MIME-Version: 1.0
Date: Mon, 26 Mar 2018 13:21:50 -0500
Message-ID: <208ba6ae-c70a-47b3-89d8-f5475db1870f@mail.dll>
Subject: Importer Security Filing Response for ISF0084359
 (Importer=SNOMONWMZ, HouseBill=SHCWQD1803SS0030)
From: 'Western Overseas Corp' <woclax@us.americas.wisegrid.net>
To: <albert@westernoverseas.com>
X-SenderStaffID: abe1d8d8-a709-4bfa-88e3-53997aa925e2
X-BusinessEntityID: ec87f10b-57ad-45ad-9bec-a6d49a40b790
X-BusinessEntityTableCode: BF

--D7AEF2B03.1522088361/ordmailedge.wisegrid.net--
";

			var bounceDetails = new BounceBackEmailDetails(Factory, Encoding.UTF8.GetBytes(body));

			var expectedBody = @"This is the mail system at host ordmailedge.wisegrid.net.

I am sorry to have to inform you that your message could not
be delivered to one or more recipients. It is attached below.

For further assistance, please send mail to postmaster.

If you do so, please include this problem report. You can
delete your own text from the attached returned message.

                   The mail system

<albert@westernoverseas.com>: host
    d109325b.ess.barracudanetworks.com[64.235.150.252] said: 550 No such user
    (albert@westernoverseas.com) (in reply to RCPT TO command)

Content-Description: Delivery report
Content-Type: message/delivery-status
Content-Transfer-Encoding: 8bit

Reporting-MTA: dns; ordmailedge.wisegrid.net
X-Postfix-Queue-ID: D7AEF2B03
X-Postfix-Sender: rfc822; woclax@us.americas.wisegrid.net
Arrival-Date: Mon, 26 Mar 2018 14:19:20 -0400 (EDT)

Final-Recipient: rfc822; albert@westernoverseas.com
Original-Recipient: rfc822;albert@westernoverseas.com
Action: failed
Status: 5.0.0
Remote-MTA: dns; d109325b.ess.barracudanetworks.com
Diagnostic-Code: smtp; 550 No such user (albert@westernoverseas.com)

Content-Description: Undelivered Message Headers
Content-Type: text/rfc822-headers
Content-Transfer-Encoding: 8bit

Return-Path: <woclax@us.americas.wisegrid.net>
Received: from ordmail.wisecloud.zone (ordwp-smai-1.wisecloud.zone [10.1.146.51])
	by ordmailedge.wisegrid.net (Postfix) with ESMTPS id D7AEF2B03
	for <albert@westernoverseas.com>; Mon, 26 Mar 2018 14:19:20 -0400 (EDT)
Received: by ordmail.wisecloud.zone (Postfix, from userid 1002)
	id 51C5795; Mon, 26 Mar 2018 13:19:20 -0500 (CDT)
X-Spam-Checker-Version: SpamAssassin 3.4.0 (2014-02-07) on
	ordwp-smai-1.wisecloud.zone
X-Spam-Level: 
X-Spam-Status: No, score=-0.3 required=5.0 tests=ALL_TRUSTED,
	HTML_IMAGE_ONLY_28,HTML_MESSAGE autolearn=no autolearn_force=no version=3.4.0
Received: from wisetechglobal.com (unknown [10.1.132.43])
	by ordmail.wisecloud.zone (Postfix) with ESMTPSA id 2992392
	for <albert@westernoverseas.com>; Mon, 26 Mar 2018 13:19:20 -0500 (CDT)
Content-Type: multipart/related;
 boundary='----=_NextPart_1323507.092707303015'
MIME-Version: 1.0
Date: Mon, 26 Mar 2018 13:21:50 -0500
Message-ID: <208ba6ae-c70a-47b3-89d8-f5475db1870f@mail.dll>
Subject: Importer Security Filing Response for ISF0084359
 (Importer=SNOMONWMZ, HouseBill=SHCWQD1803SS0030)
From: 'Western Overseas Corp' <woclax@us.americas.wisegrid.net>
To: <albert@westernoverseas.com>
X-SenderStaffID: abe1d8d8-a709-4bfa-88e3-53997aa925e2
X-BusinessEntityID: ec87f10b-57ad-45ad-9bec-a6d49a40b790
X-BusinessEntityTableCode: BF

";
			var actualBody = bounceDetails.Body;
			AssertEquals(expectedBody, actualBody);
			AssertEquals("smtp; 550 No such user (albert@westernoverseas.com)", bounceDetails.BounceReason);
			AssertEquals("550", bounceDetails.BounceReasonCode);
		}
	}
}
