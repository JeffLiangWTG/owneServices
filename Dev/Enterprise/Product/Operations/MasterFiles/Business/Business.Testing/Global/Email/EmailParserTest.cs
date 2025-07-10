using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class EmailParserTest : TestCase
	{
		public void TestProperties()
		{
			string message = @"MailEnable: Message could not be delivered to some recipients.
The following recipient(s) could not be reached:

	Recipient: [SMTP:neddy21010@yahoo.com]
	Reason: 554 delivery error: dd This user doesn't have a yahoo.com account (neddy21010@yahoo.com) [0] - mta1685.mail.gq1.yahoo.com

Message contents follow:

Received: from SYD-WEDW-1 ([10.61.3.58]) by wisetechglobal.com with MailEnable ESMTP; Tue, 11 Mar 2014 11:26:18 +1100
mime-version: 1.0
X-BusinessEntityID: 140305_PRINT_PREAP_Email tester campaign
X-BusinessEntityTableCode: G8
X-BusinessEntityJobNumber: S00000001
X-DocumentName: Arrival Note
X-SenderStaffID: F235825F-9AFB-453C-A7BC-928846DF67DF
content-transfer-encoding: 7bit
content-type: text/html;
	charset='utf-8'
subject: Email tester campaign
date: Tue, 11 Mar 2014 11:24:57 +1100
from: 'Edward Onwodi' <Support@enterprisedevelopment.cargowise.com>
reply-to: <edward.onwodi@cargowise.com>
to: <neddy21010@yahoo.com>

140305_PRINT_PREAP_Email tester campaign
some random text with to: text text";

			BounceEmailParser parser = new BounceEmailParser(message);
			AssertMultilineASCIIEquals("Reason", "554 delivery error: dd This user doesn't have a yahoo.com account (neddy21010@yahoo.com) [0] - mta1685.mail.gq1.yahoo.com", parser.Reason);
			AssertEquals("554", parser.ReasonCode);
			AssertEquals("140305_PRINT_PREAP_Email tester campaign", parser.BusinessEntityID);
			AssertEquals("G8", parser.BusinessEntityTableCode);
			AssertEquals("S00000001", parser.BusinessEntityJobNumber);
			AssertEquals("Arrival Note", parser.DocumentName);
			AssertEquals("F235825F-9AFB-453C-A7BC-928846DF67DF", parser.SenderStaffID);
			AssertEquals("Tue, 11 Mar 2014 11:24:57 +1100", parser.SenderTimeText);
			AssertContainsExactElementsInAnyOrder(new[] { "neddy21010@yahoo.com" }, parser.BouncedRecipients.Select(x => x.Address));

			message = message.Replace("X-DocumentName: Arrival Note", "X-DocumentName:" + Convert.ToBase64String(Encoding.UTF8.GetBytes("Arrival Note")));
			parser = new BounceEmailParser(message);
			AssertEquals("140305_PRINT_PREAP_Email tester campaign", parser.BusinessEntityID);
			AssertEquals("G8", parser.BusinessEntityTableCode);
			AssertEquals("S00000001", parser.BusinessEntityJobNumber);
			AssertEquals("Arrival Note", parser.DocumentName);
		}

		public void TestProperties_WithoutEntityTableCode()
		{
			string message = @"MailEnable: Message could not be delivered to some recipients.
The following recipient(s) could not be reached:

	Recipient: [SMTP:neddy21010@yahoo.com]
	Reason: 554 delivery error: dd This user doesn't have a yahoo.com account (neddy21010@yahoo.com) [0] - mta1685.mail.gq1.yahoo.com

Message contents follow:

Received: from SYD-WEDW-1 ([10.61.3.58]) by wisetechglobal.com with MailEnable ESMTP; Tue, 11 Mar 2014 11:26:18 +1100
mime-version: 1.0
X-BusinessEntityID: 140305_PRINT_PREAP_Email tester campaign
content-transfer-encoding: 7bit
content-type: text/html;
	charset='utf-8'
subject: Email tester campaign
date: Tue, 11 Mar 2014 11:24:57 +1100
from: 'Edward Onwodi' <Support@enterprisedevelopment.cargowise.com>
reply-to: <edward.onwodi@cargowise.com>
to: <neddy21010@yahoo.com>

140305_PRINT_PREAP_Email tester campaign";

			BounceEmailParser parser = new BounceEmailParser(message);
			AssertMultilineASCIIEquals("Reason", parser.Reason, "554 delivery error: dd This user doesn't have a yahoo.com account (neddy21010@yahoo.com) [0] - mta1685.mail.gq1.yahoo.com");
			AssertEquals("140305_PRINT_PREAP_Email tester campaign", parser.BusinessEntityID);
			AssertEquals("", parser.BusinessEntityTableCode);
			AssertEquals("", parser.DocumentName);
			AssertContainsExactElementsInAnyOrder(new[] { "neddy21010@yahoo.com" }, parser.BouncedRecipients.Select(x => x.Address));
			AssertEquals("554", parser.ReasonCode);
		}

		public void TestProperties_LineBreakStyle()
		{
			string message =
@"This is a MIME-encapsulated message.

--33EA34E3.1526896870/test.emailserver.net
Content-Description: Notification
Content-Type: text/plain; charset=utf-8
Content-Transfer-Encoding: 8bit

This is the mail system at host test.emailserver.net.

I'm sorry to have to inform you that your message could not
be delivered to one or more recipients. It's attached below.

For further assistance, please send mail to postmaster.

If you do so, please include this problem report. You can
delete your own text from the attached returned message.

                   The mail system

<user@test.com>: host test.mail.com[44.55.66.88] said: 550
    user@test.com:user not exist (in reply to RCPT TO command)

--33EA34E3.1526896870/test.emailserver.net
Content-Description: Delivery report
Content-Type: message/delivery-status
Content-Transfer-Encoding: 8bit

Reporting-MTA: dns; test.emailserver.net
X-Postfix-Queue-ID: 33EA34E3
X-Postfix-Sender: rfc822; bounce@test.com
Arrival-Date: Mon, 21 May 2018 06:01:04 -0400 (EDT)

Final-Recipient: rfc822; user@test.com
Original-Recipient: rfc822;user@test.com
Action: failed
Status: 5.0.0
Remote-MTA: dns; test.mail.com
Diagnostic-Code: smtp; 550 user@test.com:user not exist

--33EA34E3.1526896870/test.emailserver.net
Content-Description: Undelivered Message Headers
Content-Type: text/rfc822-headers
Content-Transfer-Encoding: 8bit

Return-Path: <bounce@test.com>
Received: from test.emailserver.net (test.emailserver.net [11.22.33.44])
	by test.emailserver.net (Postfix) with ESMTPS id 33EA34E3
	for <user@test.com>; Mon, 21 May 2018 06:01:04 -0400 (EDT)
Received: from emailserver.net (unknown [11.22.33.44])
	by test.emailserver.net (Postfix) with ESMTPA id 1112223
	for <user@test.com>; Mon, 21 May 2018 05:01:17 -0500 (CDT)
Content-Type: multipart/mixed;
 boundary=""----=_NextPart_54347027.237494247765""
MIME-Version: 1.0
Date: Mon, 21 May 2018 05:00:49 -0500
Message-ID: <135bb06f-31ae-40a5-b7c6-f096424eff73@mail.dll>
Subject: Bill of Lading - S00001000
From: ""Sender"" <sender@someorg.com>
To: <user@test.com>
X-SenderStaffID: 0d0fc780-c2d1-4609-a85c-7a8a99bf3c54
X-BusinessEntityID: 94b60495-b50e-4dc7-8150-7d308d7d83e1
X-BusinessEntityTableCode: JS
X-BusinessEntityJobNumber: S00000001
X-DocumentName: Q09QWQ==

--33EA34E3.1526896870/test.emailserver.net--";

			message = message.Replace("\r\n", "\n");

			var parser = new BounceEmailParser(message);

			CombineAssertions(() =>
				{
					AssertEquals("94b60495-b50e-4dc7-8150-7d308d7d83e1", parser.BusinessEntityID);
					AssertEquals("JS", parser.BusinessEntityTableCode);
					AssertEquals("S00000001", parser.BusinessEntityJobNumber);
					AssertEquals("COPY", parser.DocumentName);
					AssertEquals("0d0fc780-c2d1-4609-a85c-7a8a99bf3c54", parser.SenderStaffID);
					AssertEquals("Mon, 21 May 2018 05:00:49 -0500", parser.SenderTimeText);
					AssertContainsExactElementsInAnyOrder(new[] { "user@test.com" }, parser.BouncedRecipients.Select(x => x.Address));
				}
			);
		}

		public void TestBouncedRecipients_MultipleRecipientsIncludingCcAndBcc_WithSuffixReason()
		{
			AssertBouncedRecipients(
				@"This is a MIME-encapsulated message.
--0268B6218A8.1462065459/nobelium.mailguard.com.au
Content-Description: Notification
Content-Type: text/plain; charset=us-ascii

This is the mail system at host nobelium.mailguard.com.au.

I'm sorry to have to inform you that your message could not
be delivered to one or more recipients. It's attached below.

For further assistance, please send mail to postmaster.

If you do so, please include this problem report. You can
delete your own text from the attached returned message.

                   The mail system

<to.ndr@salesforcenational.com.au>: connect to
    149.135.111.209[149.135.111.209]:25: Connection timed out

<cc.ndr@salesforcenational.com.au>: connect to
    149.135.111.209[149.135.111.209]:25: Connection timed out

<bcc.ndr.1@salesforcenational.com.au>: connect to
    149.135.111.209[149.135.111.209]:25: Connection timed out

<bcc.ndr.2@salesforcenational.com.au>: connect to
    149.135.111.209[149.135.111.209]:25: Connection timed out

--0268B6218A8.1462065459/nobelium.mailguard.com.au
Content-Description: Delivery report
Content-Type: message/delivery-status

Reporting-MTA: dns; nobelium.mailguard.com.au
X-Postfix-Queue-ID: 0268B6218A8
X-Postfix-Sender: rfc822; support@enterprisedevelopment.cargowise.com
Arrival-Date: Tue, 26 Apr 2016 10:57:33 +1000 (AEST)

Final-Recipient: rfc822; to.ndr@salesforcenational.com.au
Original-Recipient: rfc822;to.ndr@salesforcenational.com.au
Action: failed
Status: 4.4.1
Diagnostic-Code: X-Postfix; connect to 149.135.111.209[149.135.111.209]:25:
    Connection timed out

--0268B6218A8.1462065459/nobelium.mailguard.com.au
Content-Description: Undelivered Message
Content-Type: message/rfc822

Return-Path: <support@enterprisedevelopment.cargowise.com>
Received: from rubidium.mailguard.com.au (rubidium.mailguard.com.au [103.248.191.194])
	(using TLSv1 with cipher DHE-RSA-AES256-SHA (256/256 bits))
	(No client certificate requested)
	by nobelium.mailguard.com.au (Postfix) with ESMTPS id 0268B6218A8
	for <to.ndr@salesforcenational.com.au>; Tue, 26 Apr 2016 10:57:33 +1000 (AEST)
Received: from rubidium.mailguard.com.au (localhost.localdomain [127.0.0.1])
	by rubidium.mailguard.com.au (Postfix) with ESMTP id BBA224361D3
	for <to.ndr@salesforcenational.com.au>; Tue, 26 Apr 2016 10:57:33 +1000 (AEST)
Received: from enmail.wisetechglobal.com (hmail.edi.net.au [203.62.214.13])
	by rubidium.mailguard.com.au (Postfix) with ESMTP id 1A4EF1126005
	for <to.ndr@salesforcenational.com.au>; Tue, 26 Apr 2016 10:57:27 +1000 (AEST)
Received: from [10.61.164.82] ([10.61.164.82]) by wisetechglobal.com with MailEnable ESMTP; Tue, 26 Apr 2016 10:55:40 +1000
X-BusinessEntityID: 29757055-4201-4a41-a35a-5877392a7c49
Content-Type: multipart/alternative;
MIME-Version: 1.0
Subject: Test Bounce Campaign to DAECOR
Message-ID: <48d3067d-12fb-4170-83e0-6cad73d1550c@mail.dll>
Date: Wed, 20 Apr 2016 17:06:48 +1000
From: ""Tester1"" <andrew.luong@wisetechglobal.com>
Reply-To: <andrew.luong@wisetechglobal.com>
To: <to.ndr@salesforcenational.com.au>
Cc: <cc.valid@salesforcenational.com.au>,
 <cc.ndr@salesforcenational.com.au>
Bcc: <bcc.ndr.1@salesforcenational.com.au>,
 <bcc.ndr.2@salesforcenational.com.au>
X-MailGuard-UID: 571ebcf743cc9ca2
X-SpamGuard-Score: 6.482
X-MailGuard-ID: 571ebcf84e727f
X-Filtered: by MailGuard - visit http://www.mailguard.com.au",
				new[] { "to.ndr@salesforcenational.com.au", "cc.ndr@salesforcenational.com.au", "bcc.ndr.1@salesforcenational.com.au", "bcc.ndr.2@salesforcenational.com.au" });
		}

		public void TestBouncedRecipients_MultipleRecipientsIncludingCcAndBcc_WithRecipientAndStmpPrefix()
		{
			AssertBouncedRecipients(
				@"MailEnable: Message could not be delivered to some recipients.
The following recipient(s) could not be reached:

	Recipient: [SMTP:to.ndr@me.com]
	Reason: Remote SMTP Server Returned: 550-5.1.1 The email account that you tried to reach does not exist.

	Recipient: [SMTP:cc.ndr@me.com]
	Reason: Remote SMTP Server Returned: 550-5.1.1 The email account that you tried to reach does not exist.

	Recipient: [SMTP:bcc.ndr.1@me.com]
	Reason: Remote SMTP Server Returned: 550-5.1.1 The email account that you tried to reach does not exist.

	Recipient: [SMTP:bcc.ndr.2@me.com]
	Reason: Remote SMTP Server Returned: 550-5.1.1 The email account that you tried to reach does not exist.

Message headers follow:

Received: from [10.61.164.82] ([10.61.164.82]) by wisetechglobal.com with MailEnable ESMTP; Mon, 2 May 2016 09:47:52 +1000
X-BusinessEntityID: 2093c8bb-33de-451d-9e5d-88eef0f9b626
X-BusinessEntityTableCode: JS
X-DocumentName: EMAIL COPY
Content-Type: multipart/mixed;
MIME-Version: 1.0
Subject: My Bounce Subject
Message-ID: <a7f72ee2-c315-447d-ad57-723d47c6c046@mail.dll>
Date: Mon, 02 May 2016 09:47:49 +1000
From: ""TEST EDI"" <Support@enterprisedevelopment.cargowise.com>
To: <to.ndr@me.com>
Cc: <cc.valid@me.com>,
 <cc.ndr@me.com>
Bcc: 
Return-Path: <Support@enterprisedevelopment.cargowise.com>",
				new[] { "to.ndr@me.com", "cc.ndr@me.com" });
		}

		public void TestBouncedRecipients_MultipleRecipientsIncludingCcAndBcc_WithDisplayNameAndMailToPrefix()
		{
			AssertBouncedRecipients(
				@"Delivery has failed to these recipients or groups:

to.ndr@wisetechglobal.com <mailto:to.ndr@wisetechglobal.com>
Your message wasn't delivered. Despite repeated attempts to deliver your me=
ssage, the recipient's email system refused to accept a connection from you=
r email system.

cc.ndr@wisetechglobal.com <mailto:cc.ndr@wisetechglobal.com>
Your message wasn't delivered. Despite repeated attempts to deliver your me=
ssage, the recipient's email system refused to accept a connection from you=
r email system.

bcc.ndr.1@wisetechglobal.com <mailto:bcc.ndr.1@wisetechglobal.com>
Your message wasn't delivered. Despite repeated attempts to deliver your me=
ssage, the recipient's email system refused to accept a connection from you=
r email system.

bcc.ndr.2@wisetechglobal.com <mailto:bcc.ndr.2@wisetechglobal.com>
Your message wasn't delivered. Despite repeated attempts to deliver your me=
ssage, the recipient's email system refused to accept a connection from you=
r email system.

Original message headers:

Received: from [10.61.164.82] ([10.61.164.82]) by wisetechglobal.com with MailEnable ESMTP; Mon, 2 May 2016 09:47:52 +1000
X-BusinessEntityID: 2093c8bb-33de-451d-9e5d-88eef0f9b626
X-BusinessEntityTableCode: JS
X-DocumentName: EMAIL COPY
Content-Type: multipart/mixed;
MIME-Version: 1.0
Subject: My Bounce Subject
Message-ID: <a7f72ee2-c315-447d-ad57-723d47c6c046@mail.dll>
Date: Mon, 02 May 2016 09:47:49 +1000
From: ""TEST EDI"" <Support@enterprisedevelopment.cargowise.com>
To: <to.ndr@wisetechglobal.com>
Cc: <cc.valid@wisetechglobal.com>,
 <cc.ndr@wisetechglobal.com>
Bcc: 
Return-Path: <Support@enterprisedevelopment.cargowise.com>",
				new[] { "to.ndr@wisetechglobal.com", "cc.ndr@wisetechglobal.com" });
		}

		public void TestBouncedRecipients_RecipientNotIncludedInHeader()
		{
			AssertBouncedRecipients(
				@"Delivery has failed to these recipients or groups:

egi.lowquota@wisetechglobal.com<mailto:edi.lowquota@wisetechglobal.com>
The recipient's mailbox is full and can't accept messages now. Please try resending your message later, or contact the recipient directly.

Original message headers:

Received: from [10.61.164.82] ([10.61.164.82]) by wisetechglobal.com with MailEnable ESMTP; Thu, 10 Nov 2022 02:23:26 +0000
X-BusinessEntityID: d7627cf3-4256-432f-9cbc-a59653da3f47
X-BusinessEntityTableCode: G8
X-DocumentName: 3D20
Content-Type: text/html; charset=3D3Dutf-8
MIME-Version: 1.0
Subject: My Bounce Subject
Message-ID: <a7f72ee2-c315-447d-ad57-723d47c6c046@mail.dll>
Date: Thu, 10 Nov 2022 02:23:26 +0000
From: ""TEST EDI"" <Support@enterprisedevelopment.cargowise.com>
To: <to.ndr@wisetechglobal.com>
Cc: <cc.valid@wisetechglobal.com>,
 <cc.ndr@wisetechglobal.com>
Bcc: 
Return-Path: <Support@enterprisedevelopment.cargowise.com>",
				Array.Empty<string>());
		}

		public void TestBouncedRecipients_MultipleRecipientsWithRegexQuantifier()
		{
			string email1 = "RNAME-email-adressen-formatausschoepfungstestvariante-mit-verschiedenen-Zeichen-!#$%&'*+/=?^_`{|}~1234567890@TESTDOMAIN-1234567890-by-zerti.de";
			string email2 = "+441326376614@dsvfax.uk";
			AssertBouncedRecipients($@"MailEnable: Message could not be delivered to some recipients.
The following recipient(s) could not be reached:

	Recipient: [SMTP:{email1}]
	Reason: Remote SMTP Server Returned: 550-5.1.1 The email account that you tried to reach does not exist.

	Recipient: [SMTP:{email2}]
	Reason: Remote SMTP Server Returned: 550-5.1.1 The email account that you tried to reach does not exist.

Message headers follow:

Received: from [10.61.164.82] ([10.61.164.82]) by wisetechglobal.com with MailEnable ESMTP; Mon, 2 May 2016 09:47:52 +1000
X-BusinessEntityID: 2093c8bb-33de-451d-9e5d-88eef0f9b626
X-BusinessEntityTableCode: JS
X-DocumentName: EMAIL COPY
Content-Type: multipart/mixed;
MIME-Version: 1.0
Subject: My Bounce Subject
Message-ID: <a7f72ee2-c315-447d-ad57-723d47c6c046@mail.dll>
Date: Mon, 02 May 2016 09:47:49 +1000
From: ""TEST EDI"" <Support@enterprisedevelopment.cargowise.com>
To: <{email1}>
Cc: <{email2}>
Bcc: 
Return-Path: <Support@enterprisedevelopment.cargowise.com>",
	new[] { email1, email2 });
		}

		void AssertBouncedRecipients(string emailBody, IEnumerable<string> expectedBouncedRecipients)
		{
			var parser = new BounceEmailParser(emailBody);
			AssertContainsExactElementsInAnyOrder(expectedBouncedRecipients, parser.BouncedRecipients.Select(x => x.Address));
		}
	}
}
