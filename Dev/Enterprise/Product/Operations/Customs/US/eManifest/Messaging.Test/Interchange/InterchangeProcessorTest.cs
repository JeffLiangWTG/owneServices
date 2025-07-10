using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Customs.US.eManifest.Messaging.Testing;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.eManifest.Messaging.Interchange.Testing
{
	public class InterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestPrecessInterchange()
		{
			MessagingTestHelper.GetReceivedInterchange(Factory, ManifestInterchangeText.Replace("\r\n", "'"));
			Factory.Save();
			new InterchangeProcessor().ExecuteBatch();
			var interchanges = new BusinessObjectFactory().Load<CBPEDIInterchange>(new ZQuery());
			AssertEquals(1, interchanges.Length);
			var interchange = interchanges[0];
			AssertEquals("EI_ApplicationCode", CBPEDIInterchange.ApplicationCodes.USeManifest, interchange.EI_ApplicationCode);
			AssertEquals("EI_InterchangeType", MessageTypes.Codes.eManifest, interchange.EI_InterchangeType);
			AssertEquals("EI_ReceiveTransmit", CBPEDIInterchange.Direction.Receive, interchange.EI_ReceiveTransmit);
			AssertEquals("EI_Status", CBPEDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("EI_From", EDIMessage.ApplicationCodes.USCustoms, interchange.EI_From);
			AssertEquals("EI_To", EDIMessage.ApplicationCodes.USeManifest, interchange.EI_To);
			AssertEquals("EI_InterchangeNum", "1", interchange.EI_InterchangeNum);
			AssertEquals("EI_HeaderText", "UNB+UNOA:4+CBP-ACE-TEST:ZZ+LOCK:02+20041203:1623+1956++ACETEST'", interchange.EI_HeaderText);
			AssertEquals("EI_BodyText", "UNH+1956+CUSRES:D:03B:UN'BGM+132:::ST+LOCKKH04120301'DTM+132:200412301200:203'FTX+INS+++STATE FARM INSURANCE COMPANY:QO123456789TF:2004:100000'FTX+AIQ+++ABO04100000'TDT+11++03+:::BT+LOCK+I++:109::10000324'LOC+60+0152'ERP+1'ERC+511'FTX+AAO+++Man Returned to Preliminary'DOC+950:ZZZ+554002525'RFF+AAM:LOCKKH041203101'DTM+133:20041230:102'GEI+7+135'NAD+CN+0000065427:109++KATHY SMITH++BELTSVILLE+MD+20708+US'CTA+IC'COM+8005551212:TE'NAD+IM+0000001714:109++KATHY SMITH++BELTSVILLE+MD+20708+US'CTA+IC'COM+8005551212:TE'NAD+SH+0000065422:109++KATHY SMITH++BELTSVILLE+MD+20708+US'CTA+IC'COM+8005551212:TE'CST+1'FTX+ZZZ+++158'ERP+2'ERC+060'FTX+AAO+++XXXX Bill Rejected XXXX'ERP+2'ERC+033'FTX+AAO+++Invalid DDPP'ERP+2'ERC+108'FTX+AAO+++Bonded Carrier ID Required'ERP+2'ERC+491'FTX+AAO+++Ship data mxd with rel typs'UNT+38+1956'", interchange.EI_BodyText);
			AssertEquals("EI_FooterText", "UNZ+1+1956", interchange.EI_FooterText);
			AssertEquals(1, interchange.ContainedMessages.Count);
			var message = interchange.ContainedMessages[0];
			AssertEquals("EI_ApplicationCode", CBPEDIInterchange.ApplicationCodes.USeManifest, message.EM_ApplicationCode);
			AssertEquals("EM_ReceiveTransmit", CBPEDIInterchange.Direction.Receive, message.EM_ReceiveTransmit);
			AssertEquals("EM_Status", CBPEDIInterchange.Status.Queued, message.EM_Status);
			AssertEquals("EM_MessageText", interchange.EI_BodyText, message.EM_MessageText);
		}

		public const string ManifestInterchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+LOCK:02+20041203:1623+1956++ACETEST
UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20041203:1623+1956+UN+D:03B
UNH+1956+CUSRES:D:03B:UN
BGM+132:::ST+LOCKKH04120301
DTM+132:200412301200:203
FTX+INS+++STATE FARM INSURANCE COMPANY:QO123456789TF:2004:100000
FTX+AIQ+++ABO04100000
TDT+11++03+:::BT+LOCK+I++:109::10000324
LOC+60+0152
ERP+1
ERC+511
FTX+AAO+++Man Returned to Preliminary
DOC+950:ZZZ+554002525
RFF+AAM:LOCKKH041203101
DTM+133:20041230:102
GEI+7+135
NAD+CN+0000065427:109++KATHY SMITH++BELTSVILLE+MD+20708+US
CTA+IC
COM+8005551212:TE
NAD+IM+0000001714:109++KATHY SMITH++BELTSVILLE+MD+20708+US
CTA+IC
COM+8005551212:TE
NAD+SH+0000065422:109++KATHY SMITH++BELTSVILLE+MD+20708+US
CTA+IC
COM+8005551212:TE
CST+1
FTX+ZZZ+++158
ERP+2
ERC+060
FTX+AAO+++XXXX Bill Rejected XXXX
ERP+2
ERC+033
FTX+AAO+++Invalid DDPP
ERP+2
ERC+108
FTX+AAO+++Bonded Carrier ID Required
ERP+2
ERC+491
FTX+AAO+++Ship data mxd with rel typs
UNT+38+1956
UNE+1+1956
UNZ+1+1956";
	}
}
