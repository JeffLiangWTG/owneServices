using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Environment;
using MailManager;

namespace Enterprise.Recruitment.Testing.ServiceTasks
{
	static class RecruitmentEmailDataHelpers
	{
		// === EMAIL 1 ===
		// initial email from CW1 in Recruitment Module, obtained after running IMS
		// CW1 -> IMS -> MailDBItems
		public static MailItem CreateEmail_FirstEmailBeforeMMT(BusinessObjectFactory factory, bool factorySave = false)
		{
			var item = factory.New<MailItem>();

			item.MI_Application = "MMT";
			item.MI_Status = "QUE";
			item.MI_Direction = DirectionList.Codes.Receive;
			item.MI_From = "Benjamin Anthony Sutas <Benjamin.Sutas@wisetechglobal.com>";
			item.MI_ReplyTo = string.Empty;
			item.MI_Subject = "hihi";
			item.MI_Body = @"<html xmlns:o=""urn:
			schemas - microsoft - com:office:
			office"" xmlns:w=""urn:
			schemas - microsoft - com:office:
			word"" xmlns:m=""http://schemas.microsoft.com/office/2004/12/omml"" xmlns=""http://www.w3.org/TR/REC-html40"">
< head >
< meta http - equiv = ""Content-Type"" content = ""text/html; charset=us-ascii"" >
	 < meta name = ""Generator"" content = ""Microsoft Word 15 (filtered medium)"" >
		< style >< !--
		/* Font Definitions */
		@font - face
	{
				font - family:""Cambria Math"";
				panose - 1:2 4 5 3 5 4 6 3 2 4;
			}
			@font - face
	{
				font - family:Calibri;
				panose - 1:2 15 5 2 2 2 4 3 2 4;
			}
			/* Style Definitions */
			p.MsoNormal, li.MsoNormal, div.MsoNormal
	{
margin:
				0in;
				margin - bottom:.0001pt;
				font - size:11.0pt;
				font - family:""Calibri"",sans - serif;
			}
.MsoChpDefault
	{ mso - style - type:export - only; }
			@page WordSection1
	{
size:
				8.5in 11.0in;
margin:
				1.0in 1.0in 1.0in 1.0in;
			}
			div.WordSection1
	{ page: WordSection1; }
			--></ style >
			</ head >
			< body lang = ""EN-US"" >
			 < div class=""WordSection1"">
<p class=""MsoNormal"">Hihi</p>
<p class=""MsoNormal"">### Please enter your message above this line. Please do not delete this message. ###irFQsOXQxbpNm-yvTHgb96JKQ0M###</p>
</div>
</body>
</html>
";
			item.MI_Header = @"Received: from SYBPR01MB4426.ausprd01.prod.outlook.com (2603:10c6:10:57::23)
 by ME1PR01MB1123.ausprd01.prod.outlook.com with HTTPS; Thu, 10 Dec 2020
 22:40:21 +0000
Received: from SYXPR01MB2175.ausprd01.prod.outlook.com (2603:10c6:0:26::10) by
 SYBPR01MB4426.ausprd01.prod.outlook.com (2603:10c6:10:57::23) with Microsoft
 SMTP Server (version=TLS1_2, cipher=TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384) id
 15.20.3654.12; Thu, 10 Dec 2020 22:40:19 +0000
Received: from SYXPR01MB2175.ausprd01.prod.outlook.com
 ([fe80::5c77:ca45:a276:6c1a]) by SYXPR01MB2175.ausprd01.prod.outlook.com
 ([fe80::5c77:ca45:a276:6c1a%12]) with mapi id 15.20.3654.012; Thu, 10 Dec
 2020 22:40:19 +0000
From: Benjamin Anthony Sutas <Benjamin.Sutas@wisetechglobal.com>
To: recruiting <recruiting@wisetechglobal.com>
Subject: hihi
Thread-Topic: hihi
Thread-Index: AQHWz0Vsb1xPQqp9IU2/y6dcnpe9Nw==
Date: Thu, 10 Dec 2020 22:40:19 +0000
Message-ID:
	<SYXPR01MB21757FAD40B74299A1CA217FEACB0@SYXPR01MB2175.ausprd01.prod.outlook.com>
Accept-Language: en-AU, en-US
Content-Language: en-US
X-MS-Exchange-Organization-AuthAs: Internal
X-MS-Exchange-Organization-AuthMechanism: 04
X-MS-Exchange-Organization-AuthSource: SYXPR01MB2175.ausprd01.prod.outlook.com
X-MS-Has-Attach:
X-MS-Exchange-Organization-Network-Message-Id:
	070f6c47-dce2-41be-24ad-08d89d5c92bf
X-MS-Exchange-Organization-SCL: -1
X-MS-TNEF-Correlator:
X-MS-Exchange-Organization-RecordReviewCfmType: 0
x-ms-publictraffictype: Email
X-Microsoft-Antispam-Mailbox-Delivery:
	ucf:0;jmr:0;auth:0;dest:I;ENG:(750128)(520011016)(706158)(944506458)(944626604);
X-Microsoft-Antispam-Message-Info:
	p0VvFL7DjLFCdswCjmqEuzzKMiTUZeytkn5vHH8Xrfqc9XI32PWGwMd+EHKwHkDSmXSC0H/e6lfl4t7yw7dBUa9FSk07VSGB8f3YKHaKQlbuBWEOg/GoCMY2d+vaYUYBWhBEs6OJ+8R70QqJlKZMdtBzIs7YcTv7Kc0Pkz4gM78rqlFQIvb+om81umX4dkuvg2uunkOvRabeDg3MgUIAh7B+3Q8WDuXfY25q3SxSNkksL7j+K+jG6hv849XiDYHDhX+xz5AhwUekF/6NGAX73NlQ4dBT+1qdFtlGqPYx+KOT2AwZB1UQk5M0JgiQkojDEY+5wDlynHgipMjKxOtsL4qudTUTxtabT/7l7SGYm5A=
Content-Type: text/html; charset=""us - ascii""
MIME-Version: 1.0
X-UIDL: 279
";
			item.MI_ReceivedDateTime = new ZDateTime(2020, 12, 10, 22, 40, 19);
			item.MI_POP3UIDL = 279.ToString();
			item.MI_Encoding = string.Empty;
			item.MI_ContentType = EmailContentTypes.HTML.ContentTypeCode;

			if (factorySave)
			{
				factory.Save();
			}

			return item;
		}

		// === EMAIL 2 ===
		// this is email 1 after running MMT
		// MailDBItems -> MMT -> MailDBItems
		public static MailItem CreateEmail_FirstEmailAfterMMT(BusinessObjectFactory factory, bool factorySave = false)
		{
			var item = factory.New<MailItem>();

			item.MI_Application = "MMT";
			item.MI_Status = "QUE";
			item.MI_Direction = DirectionList.Codes.Transmit;
			item.MI_From = "recruiting@wisetechglobal.com";
			item.MI_ReplyTo = "recruiting@wisetechglobal.com";
			item.MI_Subject = "hihi";
			item.MI_Body = @"<html xmlns:o=""urn:
			schemas - microsoft - com:office:
			office"" xmlns:w=""urn:
			schemas - microsoft - com:office:
			word"" xmlns:m=""http://schemas.microsoft.com/office/2004/12/omml"" xmlns=""http://www.w3.org/TR/REC-html40"">
< head >
< meta http - equiv = ""Content-Type"" content = ""text/html; charset=us-ascii"" >
	 < meta name = ""Generator"" content = ""Microsoft Word 15 (filtered medium)"" >
		< style >< !--
		/* Font Definitions */
		@font - face
	{
				font - family:""Cambria Math"";
				panose - 1:2 4 5 3 5 4 6 3 2 4;
			}
			@font - face
	{
				font - family:Calibri;
				panose - 1:2 15 5 2 2 2 4 3 2 4;
			}
			/* Style Definitions */
			p.MsoNormal, li.MsoNormal, div.MsoNormal
	{
margin:
				0in;
				margin - bottom:.0001pt;
				font - size:11.0pt;
				font - family:""Calibri"",sans - serif;
			}
.MsoChpDefault
	{ mso - style - type:export - only; }
			@page WordSection1
	{
size:
				8.5in 11.0in;
margin:
				1.0in 1.0in 1.0in 1.0in;
			}
			div.WordSection1
	{ page: WordSection1; }
			--></ style >
			</ head >
			< body lang = ""EN-US"" >
			 < div class=""WordSection1"">
<p class=""MsoNormal"">Hihi</p>
<p class=""MsoNormal"">### Please enter your message above this line. Please do not delete this message. ###irFQsOXQxbpNm-yvTHgb96JKQ0M###</p>
</div>
</body>
</html>
";
			item.MI_Header = @"Subject: hihi
From: recruiting@wisetechglobal.com
To: test@wisetechglobal.com
Reply-To: recruiting@wisetechglobal.com
MIME-Version: 1.0
Content-Type: text/html; charset=""us - ascii""
Date: Thu, 10 Dec 2020 22:40:19 +0000
Accept-Language: en-AU, en-US
Content-Language: en-US
Message-ID: <SYXPR01MB21757FAD40B74299A1CA217FEACB0@SYXPR01MB2175.ausprd01.prod.outlook.com>
Thread-Topic: hihi
Thread-Index: AQHWz0Vsb1xPQqp9IU2/y6dcnpe9Nw==
";
			item.MI_ReceivedDateTime = new ZDateTime(2020, 12, 10, 22, 40, 19);
			item.MI_POP3UIDL = string.Empty;
			item.MI_Encoding = string.Empty;
			item.MI_ContentType = EmailContentTypes.HTML.ContentTypeCode;

			if (factorySave)
			{
				factory.Save();
			}

			return item;
		}

		// === EMAIL 3 ===
		// we don't care about email after OMS sends it so this is the reply we receive from candidate
		// MailDBItems -> OMS -> <external_recipient>
		public static MailItem CreateEmail_SecondEmailBeforeMMT(BusinessObjectFactory factory, bool factorySave = false)
		{
			var item = factory.New<MailItem>();

			item.MI_Application = "MMT";
			item.MI_Status = "QUE";
			item.MI_Direction = DirectionList.Codes.Receive;
			item.MI_From = "Benjamin Sutas <bigbadbennybrother@gmail.com>";
			item.MI_ReplyTo = string.Empty;
			item.MI_Subject = "Re: hihi";
			item.MI_Body = @"--000000000000d1022405b62428b6
Content-Type: text/plain; charset=""UTF - 8""

hello there

On Fri, 11 Dec 2020 at 09:59, < genericwtgtestaddress@gmail.com > wrote:

> Hihi
>
> ###Please enter your message above this
> line###irFQsOXQxbpNm-yvTHgb96JKQ0M###
>

--000000000000d1022405b62428b6
Content - Type: text / html;
			charset = ""UTF-8""
Content - Transfer - Encoding: quoted - printable

	< meta http - equiv = 3D""Content-Type"" content = 3D""text/html; charset=3Dutf-8"" >< d =
		   iv dir = 3D""ltr"" > hello there </ div >< br >< div class=3D""gmail_quote""><div dir = 3D""=
ltr"" class=3D""gmail_attr"">On Fri, 11 Dec 2020 at 09:59, &lt;<a href=3D""mail=
to:genericwtgtestaddress @gmail.com"">genericwtgtestaddress@gmail.com</a>&gt;=
 wrote:<br></div><blockquote class=3D""gmail_quote"" style=3D""margin:0px 0px =
0px 0.8ex;border-left:1px solid rgb(204,204,204); padding-left:1ex"">





<div lang = 3D""EN-US"">
<div class=3D""gmail-m_1633258544960887674WordSection1"">
<p class=3D""MsoNormal"">Hihi</p>
<p class=3D""MsoNormal"">### Please enter your message above this line. Please do not delete this message. ###irFQs=
OXQxbpNm-yvTHgb96JKQ0M###</p>
</div>
</div>

</blockquote></div>

--000000000000d1022405b62428b6--

";
			item.MI_Header = @"Received: from ME1PR01MB0644.ausprd01.prod.outlook.com (2603:10c6:200:f::19)
 by ME1PR01MB1123.ausprd01.prod.outlook.com with HTTPS; Thu, 10 Dec 2020
 23:01:49 +0000
Received: from SYXPR01CA0095.ausprd01.prod.outlook.com (2603:10c6:0:2e::28) by
 ME1PR01MB0644.ausprd01.prod.outlook.com (2603:10c6:200:f::19) with Microsoft
 SMTP Server (version=TLS1_2, cipher=TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384) id
 15.20.3632.21; Thu, 10 Dec 2020 23:01:48 +0000
Received: from SY4AUS01FT008.eop-AUS01.prod.protection.outlook.com
 (2603:10c6:0:2e:cafe::d8) by SYXPR01CA0095.outlook.office365.com
 (2603:10c6:0:2e::28) with Microsoft SMTP Server (version=TLS1_2,
 cipher=TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384) id 15.20.3654.12 via Frontend
 Transport; Thu, 10 Dec 2020 23:01:48 +0000
Authentication-Results: spf=softfail (sender IP is 148.163.150.14)
 smtp.mailfrom=gmail.com; wisetechglobal.com; dkim=pass (signature was
 verified) header.d=gmail.com;wisetechglobal.com; dmarc=pass action=none
 header.from=gmail.com;compauth=pass reason=100
Received-SPF: SoftFail (protection.outlook.com: domain of transitioning
 gmail.com discourages use of 148.163.150.14 as permitted sender)
Received: from mx0a-00288b01.pphosted.com (148.163.150.14) by
 SY4AUS01FT008.mail.protection.outlook.com (10.114.156.126) with Microsoft
 SMTP Server (version=TLS1_2, cipher=TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384) id
 15.20.3654.12 via Frontend Transport; Thu, 10 Dec 2020 23:01:46 +0000
Received: from pps.filterd (m0112518.ppops.net [127.0.0.1])
	by mx0a-00288b01.pphosted.com (8.16.0.43/8.16.0.43) with SMTP id 0BAMxdDW020485
	for <recruiting@wisetechglobal.com>; Fri, 11 Dec 2020 10:01:45 +1100
Authentication-Results-Original: ppops.net;	spf=pass
 smtp.mailfrom=bigbadbennybrother@gmail.com;	dkim=pass header.s=20161025
 header.d=gmail.com;	dmarc=pass header.from=gmail.com
Received: from mail-vs1-f50.google.com (mail-vs1-f50.google.com [209.85.217.50])
	by mx0a-00288b01.pphosted.com with ESMTP id 35amx5ak4r-1
	(version=TLSv1.2 cipher=ECDHE-RSA-AES128-GCM-SHA256 bits=128 verify=NOT)
	for <recruiting@wisetechglobal.com>; Fri, 11 Dec 2020 10:01:44 +1100
Received: by mail-vs1-f50.google.com with SMTP id x4so3792735vsp.7
        for <recruiting@wisetechglobal.com>; Thu, 10 Dec 2020 15:01:44 -0800 (PST)
DKIM-Signature: v=1; a=rsa-sha256; c=relaxed/relaxed;
        d=gmail.com; s=20161025;
        h=mime-version:references:in-reply-to:from:date:message-id:subject:to;
        bh=r2CxvOJG03FODgxhkr7hZXMSycpx8gTxSBVoHgmaaq4=;
        b=WfBwf9qTBOx0c5e3Cfjxzuu0ukZ2Wi+89TGO7agKFMfLDXNvzbFfNVgdWR/VmGscUe
         o0iDE7ky915xLPZLqXDtr+pQDAu+S8sWnRUbYoemoTP1HA3BlBIERXe8CrpM3eScJE6I
         nC4MmWMm1vx7Sy+/sOWoT93x2F8sJUOMGpXWG2S6QeORJhJ2YT3aYWh3mESu6Ux0G1lS
         T55iPs48i6pt0m2I184ODmAzXeClDo4fuU9NSGjyDAKQ0NmvJ/UVBajTB3C8Xy1wZNdi
         HObVZqtgCjlzsYrHJSTeYfBMazb8201fIMpJTIBlHfo4vRQNh7Cb2Te8izRqFbIKe8Ld
         AOmQ==
X-Google-DKIM-Signature: v=1; a=rsa-sha256; c=relaxed/relaxed;
        d=1e100.net; s=20161025;
        h=x-gm-message-state:mime-version:references:in-reply-to:from:date
         :message-id:subject:to;
        bh=r2CxvOJG03FODgxhkr7hZXMSycpx8gTxSBVoHgmaaq4=;
        b=alJWScqj2MKLaGsLPY5sq92/4AJr8uzYmwthaodR0olLrmOooPI9NLPcO/5EEeFnC1
         c8LWTdjFEY+6OhTWnq7BgnFydT8ueyCPe/I1tSf8Bkua9u2+qmg+OpoZupVRCm13eS+v
         750/hkRH3n71WiQXjcR+sDb89DkpncjTyXeBhDbWtyZS1iQ+3hgu3zhup2s1v0m4WN/Y
         7oa7b7vmXogyhP8tLlz4YmaSKUyWAlaOgErG+Ksz5yCvnlXc0SvePc2iXXnBx8LbObFn
         LcPH0VXt8tRU2dFWvfTByM67BGwrGW3r0PJaC8+1/J1pdyFMIIP5Sz8IFUqcGHXB/LYC
         n28w==
X-Gm-Message-State: AOAM531jTCqO9060oR/hppX8HVxxuxLCtA2YzVNqrqAht+HpeKUb6ckg
	u2anhvaZ3OD0+TOWoGdGsm286bWVYbuOraiBuwW2Ex+CSjY=
X-Google-Smtp-Source: ABdhPJztC78Egh8Yfm5ZsGySbuJ7v56HYGOaT5alseEHDqxDoH5x6mlMu/FLLZMV7oYFj/uoLyp/AUWxA+xnolgQzvs=
X-Received: by 2002:a05:6102:3195:: with SMTP id c21mr3346059vsh.19.1607641303416;
 Thu, 10 Dec 2020 15:01:43 -0800 (PST)
References: <SYXPR01MB21757FAD40B74299A1CA217FEACB0@SYXPR01MB2175.ausprd01.prod.outlook.com>
In-Reply-To: <SYXPR01MB21757FAD40B74299A1CA217FEACB0@SYXPR01MB2175.ausprd01.prod.outlook.com>
From: Benjamin Sutas <bigbadbennybrother@gmail.com>
Date: Fri, 11 Dec 2020 10:01:32 +1100
Message-ID: <CALYT05XzafpyVhBhKJK-102mK84v_o8HyEmYuvFziySC63TWCw@mail.gmail.com>
Subject: Re: hihi
To: recruiting@wisetechglobal.com
Content-Type: multipart/alternative; boundary=""000000000000d1022405b62428b6""
X-CLX-Response: 1TFkXGxwcEQpMehcZGB8RCllEF2trExwdU2l6bBhsEQpYWBdlZxITcBhIUEx dWhEKeE4XY1Nja3sTWH4aX14RCnlMF2hjG1x/aUtfGXBCEQpDSBcHGxgZEQpDWRcZGREKQ0kXGg QaGhoRCllNF2dmchEKWUkXGnEaEBp3BhseHHEYEhAadwYYGgYaEQpZXhdsbHkRCklGF11DWU9eT
 0lCTUZFSEtGdUJFWV5PThEKQ04XZ0RwQx1vGmgTXF5rc2NmfVJpR2RFU0tSXlp6W3tHUEcRClhc Fx8EGgQYGBoFGxoEGxoaBB4SBBgZEBseGh8aEQpeWRdyaW8bUBEKTVwXGxseEQpMWhdvaU1NaxE KTEYXb2trbGtrEQpCTxdiGUJnHEVzY1tBYBEKQ1oXGBoTBBIfBBgbHQQfGhEKQl4XGxEKQlwXGx
 EKXk4XGxEKQksXY1Nja3sTWH4aX14RCkJJF2NTY2t7E1h+Gl9eEQpCRRdlTRN6WFhHZH9+aBEKQ k4XY1Nja3sTWH4aX14RCkJMF2VnEhNwGEhQTF1aEQpCbBdnH1lZX21iH2VZZBEKQkAXentkZgEa blNGfk0RCkJYF28bQ25iHW9cWEFZEQpNXhcbEQpaWBcYEQp5QxdgeWUTTmNDBWR7GhEKWUsXExg
 fGxEKcGcXZRoTTExBS0NBcm8QGRoRCnBoF2x5GR1Qc0toYkUTEBkaEQpwaBdmHFxwU2FgZWVdXx AZGhEKcGgXZhlnaR1QWHheG1wQGRoRCnBoF2drRXNlelMdUHoBEBkaEQpwaBdnRH56fVAdQxwcY xAZGhEKcGwXelp+Q0luWmJtQFgQGRoRCm1+FxsRClhNF0sRIA==
X-CLX-Shades: MLX
X-Proofpoint-Virus-Version: vendor=fsecure engine=2.50.10434:6.0.343,18.0.737
 definitions=2020-12-10_10:2020-12-09,2020-12-10 signatures=0
X-Proofpoint-Spam-Details: rule=notspam policy=default score=4 lowpriorityscore=0 suspectscore=3
 mlxlogscore=143 spamscore=4 adultscore=0 malwarescore=0 bulkscore=0
 phishscore=0 impostorscore=0 mlxscore=4 priorityscore=325 clxscore=166
 classifier=spam adjust=0 reason=mlx scancount=1 engine=8.12.0-2009150000
 definitions=main-2012100148 domainage_hfrom=9251
Return-Path: bigbadbennybrother@gmail.com
X-MS-Exchange-Organization-ExpirationStartTime: 10 Dec 2020 23:01:47.1648
 (UTC)
X-MS-Exchange-Organization-ExpirationStartTimeReason: OriginalSubmit
X-MS-Exchange-Organization-ExpirationInterval: 1:00:00:00.0000000
X-MS-Exchange-Organization-ExpirationIntervalReason: OriginalSubmit
X-MS-Exchange-Organization-Network-Message-Id:
 9bb2ca41-bb47-4060-6508-08d89d5f921b
X-EOPAttributedMessage: 0
X-EOPTenantAttributedMessage: 8b493985-e1b4-4b95-ade6-98acafdbdb01:0
X-MS-Exchange-Organization-MessageDirectionality: Incoming
X-MS-PublicTrafficType: Email
X-MS-Exchange-Organization-AuthSource:
 SY4AUS01FT008.eop-AUS01.prod.protection.outlook.com
X-MS-Exchange-Organization-AuthAs: Anonymous
X-MS-Office365-Filtering-Correlation-Id: 9bb2ca41-bb47-4060-6508-08d89d5f921b
X-MS-TrafficTypeDiagnostic: ME1PR01MB0644:
X-MS-Oob-TLC-OOBClassifiers: OLM:3968;
X-MS-Exchange-Organization-SCL: 1
X-Microsoft-Antispam: BCL:0;
X-Forefront-Antispam-Report:
 CIP:148.163.150.14;CTRY:US;LANG:en;SCL:1;SRV:;IPV:NLI;SFV:NSPM;H:mx0a-00288b01.pphosted.com;PTR:mx0a-00288b01.pphosted.com;CAT:NONE;SFS:(4636009)(5660300002)(82202003)(33964004)(19627235002)(42186006)(6916009)(7596003)(55446002)(83380400001)(7116003)(3480700007)(356005)(73392003)(8676002)(7636003)(26005)(2160300002)(336012)(86362001)(1096003)(82310400003)(76482006)(6666004)(579124003);DIR:INB;
X-MS-Exchange-CrossTenant-OriginalArrivalTime: 10 Dec 2020 23:01:46.5331
 (UTC)
X-MS-Exchange-CrossTenant-Network-Message-Id: 9bb2ca41-bb47-4060-6508-08d89d5f921b
X-MS-Exchange-CrossTenant-Id: 8b493985-e1b4-4b95-ade6-98acafdbdb01
X-MS-Exchange-CrossTenant-AuthSource:
 SY4AUS01FT008.eop-AUS01.prod.protection.outlook.com
X-MS-Exchange-CrossTenant-AuthAs: Anonymous
X-MS-Exchange-CrossTenant-FromEntityHeader: Internet
X-MS-Exchange-Transport-CrossTenantHeadersStamped: ME1PR01MB0644
X-MS-Exchange-Transport-EndToEndLatency: 00:00:02.8640870
X-MS-Exchange-Processed-By-BccFoldering: 15.20.3632.021
X-Microsoft-Antispam-Mailbox-Delivery:
	ucf:0;jmr:0;auth:0;dest:I;ENG:(20160514016)(750128)(520011016)(944506458)(944626604);
X-Microsoft-Antispam-Message-Info:
	=?utf-8?B?TitEMzdFdndFZE13YU9uaVIvemg5bU9Kb2luN202QUdlQ24velh4dytMRDM3?=
 =?utf-8?B?Q053WkxHZFpyM1RXcjVmR1hTZmkvUFgyRDY2T1V0VmRhTTYzZ2p3dW1ZRG9o?=
 =?utf-8?B?c3dwWlg5aHduaXBKLzhWaEFoMFAvMHZiMmlDNkczUklzYVZSS0VXN01LQzYz?=
 =?utf-8?B?MWNDTVE0REFjdW1iUWxVMHpGNUxIZWh3NzhXUm90V3QvTDBhUncydGUzNVpj?=
 =?utf-8?B?QkVHRzZhUXFZdG5HdlJuWkhJazRyMWxBNjhIdmVFMVByUEptUVl5NUVSUzBS?=
 =?utf-8?B?S3BoNHlOWldKUlMzZGp1NXQzZUtLaC9neXZpT3dHdTU4eDFyWTE3bmN2N3RL?=
 =?utf-8?B?VlpJd3lBQVBnT3RTdUtDVGtjd3ZBUE41SnB3WVhYMEVXems4Y3Y1S3BjTDNJ?=
 =?utf-8?B?SE9Ha2VidDhGK2paL3JZNmlIcmtvQVVCRC9SeVFkOGJCWUZ2bE9NRVhvdEhk?=
 =?utf-8?B?WXhnWTB5eG9zemtwc1FXVlQ2a2VYeHQwRW9jdnEvRWRSRnpsTHZMUVdNVkRO?=
 =?utf-8?B?YVk1Q1FBVXRpZlMyL0RJLzR4a0lTUy8rU0dtVE9TUUMzV083WlpIdW1Fd04v?=
 =?utf-8?B?aEY1bHYwMjJhb3RYQTRuMi9CWHVKRGZuZ1UvZmo1bUpodFFrbW1NdUZxMEcy?=
 =?utf-8?B?dGMvdEF3OCtyeDYzVm9MeWU5TlVPWldmQjFKOVRjeVhSNDBvc3FRektmdGN2?=
 =?utf-8?B?TlBGaElBVWFFaWxXTzdPdVFUWFhTWUpIR0N5d3hiRTB3VE5uSDFDL1BDMGxp?=
 =?utf-8?B?emVKZEhnZXNRUHE3K3htSGpZRnVhNWs3WHBjMmVFV2JZVjkrTVB6c3VRVWZF?=
 =?utf-8?B?VlpXUWhwY3djdVJaSEh2VzBKRlZuZmFQRXJ1QXMrUVk1ZTYvcUdhMHZCU2ph?=
 =?utf-8?B?c09GNHc1aWd6cUwyUWJRTGJTRjdpaGcwczlqbFpxeFNmdThYdjRtK0lpMmEw?=
 =?utf-8?B?UVAxblFra1VPajVCeGJuM1BlTU9tOEhlU01wMjEzUWdqUTdaQmxDTkpaQ29s?=
 =?utf-8?B?dGpOdU1LYU1CNVBZbnFQaE1WUjdYMnRaYy9CeUYwOUREVzE2SnNMM3dUMFVG?=
 =?utf-8?B?YUdyUHJyTUl1ZERYM0d1dzBCTHhTRDYzN3lJY25oOVo2VmwydDU3d0dqSHJh?=
 =?utf-8?B?Zi9kNWt2U2tHKzA2Y21UK3JJUURKVWZ2c2lqQXRxRHo1VTFiK3lRcWxwa2RI?=
 =?utf-8?B?cGFOdnE3eXdaMlFNckZsOERhRXZ0cTQzZGMyS3krRHI2YlFnYkpUMFZZTWRI?=
 =?utf-8?B?ak5YbENrUWpsQ29yQnJGc01rUnpLOVFlcVpCRlpBWFk3RGw1NTZpR29zOVNL?=
 =?utf-8?Q?ArZCqYxOKvxNsd/tkrvjSHGeRJcmUB3Rx0?=
MIME-Version: 1.0
X-UIDL: 281
";
			item.MI_ReceivedDateTime = new ZDateTime(2020, 12, 10, 23, 1, 48);
			item.MI_POP3UIDL = 281.ToString();
			item.MI_Encoding = string.Empty;
			item.MI_ContentType = string.Empty;

			if (factorySave)
			{
				factory.Save();
			}

			return item;
		}

		// === EMAIL 4 ===
		// we don't care about email after OMS sends it so this is the reply we receive from candidate
		// MailDBItems -> OMS -> <external_recipient>
		//public static MailItem CreateEmail_SecondEmailAfterMMT(BusinessObjectFactory factory, bool factorySave = false)
		//{

		//}

		public const string MultiPartQuotedPrintableHeader = @"Subject: Its da final countdown
From: testrecruitmentmail@gmail.com
To: jacobdun.k@gmail.com
Reply-To: testrecruitmentmail@gmail.com
MIME-Version: 1.0
Content-Type: multipart/alternative;    boundary=""_000_SYCPR01MB4400DBAA323DF07A57E1D8B3F38E9SYCPR01MB4400ausp_""
Date: Tue, 9 Feb 2021 10:31:40 +0000
Accept-Language: en-AU, en-US
Content-Language: en-US
Message-ID: <SYCPR01MB4400DBAA323DF07A57E1D8B3F38E9 @SYCPR01MB4400.ausprd01.prod.outlook.com>
Thread-Topic: Its da final countdown
Thread-Index: Adb+zr1+OtBwYL1gTFS0klGCLCrxuA==";

		public const string MultiPartQuotedPrintableBody = @"--_000_SYCPR01MB4400DBAA323DF07A57E1D8B3F38E9SYCPR01MB4400ausp_
Content-Type: text/plain; charset=""us-ascii""
Content-Transfer-Encoding: quoted-printable
Doo doo doo doo
### Please enter your message above this line. Please do not delete this message. ###kUAhv8RLXX1KtIJ_jNzzj1lKQ0M#=
##
--_000_SYCPR01MB4400DBAA323DF07A57E1D8B3F38E9SYCPR01MB4400ausp_
Content-Type: text/html; charset=""us-ascii""
Content-Transfer-Encoding: quoted-printable
<html xmlns:v=3D""urn:schemas-microsoft-com:vml"" xmlns:o=3D""urn:schemas-micr=
osoft-com:office:office"" xmlns:w=3D""urn:schemas-microsoft-com:office:word"" =
xmlns:m=3D""http://schemas.microsoft.com/office/2004/12/omml"" xmlns=3D""http:=
//www.w3.org/TR/REC-html40"">
<head>
<meta http-equiv=3D""Content-Type"" content=3D""text/html; charset=3Dus-ascii""=
>
<meta name = 3D""Generator"" content=3D""Microsoft Word 15 (filtered medium)"">
<style><!--
/* Font Definitions */
@font-face
    {font-family:""Cambria Math"";
    panose-1:2 4 5 3 5 4 6 3 2 4;}
	@font-face
    {font-family:DengXian;
    panose-1:3 0 5 9 0 0 0 0 0 0;}
@font - face

	{
	font - family:Calibri;
	panose - 1:2 15 5 2 2 2 4 3 2 4;
}
@font - face

	{
	font - family:""\@DengXian"";
	panose - 1:2 1 6 0 3 1 1 1 1 1;
}
/* Style Definitions */
span.EmailStyle19

	{ mso - style - type:personal - compose; }
.MsoChpDefault
{
	mso - style - type:export - only;
	font - size:10.0pt;
}
@page WordSection1
{
	size:612.0pt 792.0pt;
	margin:72.0pt 72.0pt 72.0pt 72.0pt;
}
div.WordSection1

	{ page: WordSection1; }
--></ style >< !--[if gte mso 9]>< xml >
   < o:shapedefaults v:ext = 3D""edit"" spidmax = 3D""1026"" />
		</ xml >< ![endif]-- >< !--[if gte mso 9]>< xml >
			 < o:shapelayout v:ext = 3D""edit"" >
				< o:idmap v:ext = 3D""edit"" data = 3D""1"" />
					 </ o:shapelayout ></ xml >< ![endif]-- >
					  </ head >
					  < body lang = 3D""EN-AU"" link = 3D""#0563C1"" vlink = 3D""#954F72"" style = 3D""word-wrap:=
break-word"">
< div class= 3D""WordSection1"" >
  < p > Doo doo doo doo<o:p ></ o:p ></ p >
	   < p >### Please enter your message above this line. Please do not delete this message. ###kUAhv8RLXX1KtIJ_jNzzj1lKQ=
0M### <o:p>
</ o:p ></ p >
 </ div >
 </ body >
 </ html >
 --_000_SYCPR01MB4400DBAA323DF07A57E1D8B3F38E9SYCPR01MB4400ausp_--";
	}
}
