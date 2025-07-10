using System.Collections.Generic;
using System.Net;
using System.Windows.Forms;
using CargoWise.Common.JSON.Extensions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.Accounting.GlobalEInvoicingRegistration.SaudiArabia;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.Accounting.GlobalEInvoicingRegistration.SaudiArabia.Testing.BranchRegistrationForSAInvoicingTest;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(EInvoicingBranchRegisterForm))]
	public class EInvoicingBranchRegisterFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var branchRegister = new EInvoicingBranchRegister(branch);

			var form = new EInvoicingBranchRegisterForm(branchRegister, null);
			return form;
		}

		BranchRegistrationForSAInvoicing_TestOnly GetRequestorInstance(GlbBranch branch, OrgHeader debtor, Enterprise.Integration.ILogger logger, EInvoicingBranchRegisterHttpClientHandlerMock handlerMock = null)
		{
			var clientHandlerMock = handlerMock ?? EInvoicingBranchRegisterHttpClientHandlerMock.New();
			return new BranchRegistrationForSAInvoicing_TestOnly(branch, debtor, GetCSRGenerator(), new HttpCleintProviderForTest(clientHandlerMock), logger);
		}

		ICSRGenerator GetCSRGenerator()
		{
			(string, string) csrString = (Base64EncoderDecoder.Decode("LS0tLS1CRUdJTiBFQyBQUklWQVRFIEtFWS0tLS0tCk1JR05BZ0VBTUJBR0J5cUdTTTQ5QWdFR0JTdUJCQUFLQkhZd2RBSUJBUVFnZzJrY1VlWjlTaUc5Y3N6VDRvYTEKN0I5QWc2M2p4N1ZRTGRoeHlMRng5MGVnQndZRks0RUVBQXFoUkFOQ0FBU25jR3hJbzVkTG43UWs4V0Izd1pIRApqLzlnODIrNmxDNCthWURkeVJ1Ym9yZUg1UXh4T2Y2V1QrK1JROHdKZXNWaDIyNEZIaDNBRVh1b3F6aXZ2TVVECi0tLS0tRU5EIEVDIFBSSVZBVEUgS0VZLS0tLS0="),
					Base64EncoderDecoder.Decode("LS0tLS1CRUdJTiBDRVJUSUZJQ0FURSBSRVFVRVNULS0tLS0KTUlJQ1F6Q0NBZWdDQVFBd2FURUxNQWtHQTFVRUJoTUNVMEV4SnpBbEJnTlZCQW9NSGxOQlZTQkRUMDFRUVU1WgpJRTlTUjBGT1NWTkJWRWxQVGlCUVVrOVlXVEVUTUJFR0ExVUVDd3dLTmpZMk56YzNPRGc0T1RFY01Cb0dBMVVFCkF3d1RRMkZ5WjI5M2FYTmxJRVJUUVVOTVNWTkJNakJXTUJBR0J5cUdTTTQ5QWdFR0JTdUJCQUFLQTBJQUJLZHcKYkVpamwwdWZ0Q1R4WUhmQmtjT1AvMkR6YjdxVUxqNXBnTjNKRzV1aXQ0ZmxESEU1L3BaUDc1RkR6QWw2eFdIYgpiZ1VlSGNBUmU2aXJPSys4eFFPZ2dnRWVNSUlCR2dZSktvWklodmNOQVFrT01ZSUJDekNDQVFjd0pBWUpLd1lCCkJBR0NOeFFDQkJjVEZWQlNSVnBCVkVOQkxVTnZaR1V0VTJsbmJtbHVaekNCM2dZRFZSMFJCSUhXTUlIVHBJSFEKTUlITk1UWXdOQVlEVlFRRURDMHhMVmRwYzJWMFpXTm9JRWRzYjJKaGJId3lMVU5oY21kdmQybHpaWHd6TFZkVQpURVJUUVVOTVNTMVRRVEl4SHpBZEJnb0praWFKay9Jc1pBRUJEQTh6TXpFeE5qSTROamswTVRBd01ETXhEVEFMCkJnTlZCQXdNQkRFd01EQXhSakJFQmdOVkJCb01QVXhGVmtWTUlERWdNMEVnTHlBM01pQlBKMUpKVDFKRVFVNGcKVTFSU1JVVlVJRUZNUlZoQlRrUlNTVUVnTURJZ01qQXhOU0JCVlZOVVVrRk1TVUV4R3pBWkJnTlZCQThNRWt4dgpaMmx6ZEdsamN5QlRaWEoyYVdObGN6QUtCZ2dxaGtqT1BRUURBZ05KQURCR0FpRUFtbml4N2F6Snp5SjZ4Z0ZpCjZDeFhmZGVncmF5aTUwNnovdHNXMW9Bb0hyY0NJUUR5L0xhYmdBOUlNQkFmdkNwNjdIU0J6ajBGSHM1NnQ2VkYKK0FxRzlPZ1RTdz09Ci0tLS0tRU5EIENFUlRJRklDQVRFIFJFUVVFU1QtLS0tLQ=="));
			var csrGeneratorMock = new Mock<ICSRGenerator>();
			csrGeneratorMock.Setup(g => g.Generate()).Returns(csrString);
			return csrGeneratorMock.Object;
		}

		public void TestSendRequestWithGoodResponse()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var saOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			branch.GB_OH_OrgProxy = saOrgProxy.PK;
			branch.GB_RN_NKCountryCode = "SA";

			var debtor = GetDebtorForTests();

			Factory.Save();

			var branchRegister = new EInvoicingBranchRegister(branch);

			var response1 = new RegistrationGoodResponseData()
			{
				RequestID = "123456",
				DispositionMessage = "ISSUED",
				BinarySecurityToken = "TUlJQjhUQ0NBWmFnQXdJQkFnSUdBWWhtTGxUNU1Bb0dDQ3FHU000OUJBTUNNQlV4RXpBUkJnTlZCQU1NQ21WSmJuWnZhV05wYm1jd0hoY05Nak13TlRJNU1EWXlORFEzV2hjTk1qZ3dOVEk0TWpFd01EQXdXakJQTVFzd0NRWURWUVFHRXdKVFFURVhNQlVHQTFVRUN3d09ZVzF0WVc0Z1FuSmhibU5vWTJneEV6QVJCZ05WQkFvTUNtaGhlV0VnZVdGbklETXhFakFRQmdOVkJBTU1DVEV5Tnk0d0xqQXVNVEJXTUJBR0J5cUdTTTQ5QWdFR0JTdUJCQUFLQTBJQUJOdUt0aWYvSy84NndlRVdVdys4VnhIRWplZTI2VFdMQzJLVTFpNVhiNzNtU2NNQ3lGdms0V3doZ0llaitRdlRRcS9FdXpqNno2dldEeStwU0NtaVR4ZWpnWm93Z1pjd0RBWURWUjBUQVFIL0JBSXdBRENCaGdZRFZSMFJCSDh3ZmFSN01Ia3hHekFaQmdOVkJBUU1FakV0YUdGNVlYd3lMVEl6Tkh3ekxUTTFOREVmTUIwR0NnbVNKb21UOGl4a0FRRU1Eek14TURFM05UTTVOelF3TURBd016RU5NQXNHQTFVRURBd0VNVEV3TURFUU1BNEdBMVVFR2d3SFdtRjBZMkVnTXpFWU1CWUdBMVVFRHd3UFJtOXZaQ0JDZFhOemFXNWxjM016TUFvR0NDcUdTTTQ5QkFNQ0Ewa0FNRVlDSVFEOWZkN0swZXJKdXZid05hRnJLRUVZVTc4VHpidTBYSzhldjFISGZsemF2d0loQU0zSDhzV1ppOUdWK3NWVHFOaUpBZU5BL3JsK2dlY2p5SGhidW9YckIwS3g=",
				Secret = "WvUCAV3NKx9ecpzYmg3HWvRbT6Mq0hikQClvJW04kYE=",
				Errors = null
			};

			var response2 = new ValidationResponseData()
			{
				ValidationResults = new ValidationResults()
				{
					InfoMessages = new List<ValidationMessage>
														{ new ValidationMessage()
																	{ Type = "INFO",
																	Code = "XSD_ZATCA_VALID",
																	Category = "XSD validation",
																	Message = "Complied with UBL 2.1 standards in line with ZATCA specifications",
																	Status = "PASS" } },
					WarningMessages = null,
					ErrorMessages = null,
					Status = "PASS"
				},
				ReportingStatus = null,
				ClearanceStatus = "CLEARED",
				QRSellertStatus = null,
				QRBuyertStatus = null
			};

			var response3 = new RegistrationGoodResponseData()
			{
				RequestID = "45678",
				TokenType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3",
				DispositionMessage = "ISSUED",
				BinarySecurityToken = "TUlJRDJ6Q0NBNENnQXdJQkFnSVRid0FBZHFEbUlocXNqcG01Q3dBQkFBQjJvREFLQmdncWhrak9QUVFEQWpCak1SVXdFd1lLQ1pJbWlaUHlMR1FCR1JZRmJHOWpZV3d4RXpBUkJnb0praWFKay9Jc1pBRVpGZ05uYjNZeEZ6QVZCZ29Ka2lhSmsvSXNaQUVaRmdkbGVIUm5ZWHAwTVJ3d0dnWURWUVFERXhOVVUxcEZTVTVXVDBsRFJTMVRkV0pEUVMweE1CNFhEVEl5TURNeU9ERTFORFl6TWxvWERUSXlNRE16TURFMU5EWXpNbG93VFRFTE1Ba0dBMVVFQmhNQ1UwRXhEakFNQmdOVkJBb1RCVXBoY21seU1Sb3dHQVlEVlFRTEV4RktaV1JrWVdnZ1FuSmhibU5vTVRJek5ERVNNQkFHQTFVRUF4TUpNVEkzTGpBdU1DNHhNRll3RUFZSEtvWkl6ajBDQVFZRks0RUVBQW9EUWdBRUQvd2IybGhCdkJJQzhDbm5adm91bzZPelJ5bXltVTlOV1JoSXlhTWhHUkVCQ0VaQjRFQVZyQnVWMnhYaXhZNHFCWWY5ZGRlcnprVzlEd2RvM0lsSGdxT0NBaW93Z2dJbU1JR0xCZ05WSFJFRWdZTXdnWUNrZmpCOE1Sd3dHZ1lEVlFRRURCTXlNakl5TWpNeU5EUTBNelF6YW1abU5ETXlNUjh3SFFZS0NaSW1pWlB5TEdRQkFRd1BNekV3TVRjMU16azNOREF3TURBek1RMHdDd1lEVlFRTURBUXhNREV4TVJFd0R3WURWUVFhREFoVFlXMXdiR1VnUlRFWk1CY0dBMVVFRHd3UVUyRnRjR3hsSUVKMWMzTnBibVZ6Y3pBZEJnTlZIUTRFRmdRVWhXY3NiYkpoakQ1WldPa3dCSUxDK3dOVmZLWXdId1lEVlIwakJCZ3dGb0FVZG1DTSt3YWdyR2RYTlozUG1xeW5LNWsxdFM4d1RnWURWUjBmQkVjd1JUQkRvRUdnUDRZOWFIUjBjRG92TDNSemRHTnliQzU2WVhSallTNW5iM1l1YzJFdlEyVnlkRVZ1Y205c2JDOVVVMXBGU1U1V1QwbERSUzFUZFdKRFFTMHhMbU55YkRDQnJRWUlLd1lCQlFVSEFRRUVnYUF3Z1owd2JnWUlLd1lCQlFVSE1BR0dZbWgwZEhBNkx5OTBjM1JqY213dWVtRjBZMkV1WjI5MkxuTmhMME5sY25SRmJuSnZiR3d2VkZOYVJXbHVkbTlwWTJWVFEwRXhMbVY0ZEdkaGVuUXVaMjkyTG14dlkyRnNYMVJUV2tWSlRsWlBTVU5GTFZOMVlrTkJMVEVvTVNrdVkzSjBNQ3NHQ0NzR0FRVUZCekFCaGg5b2RIUndPaTh2ZEhOMFkzSnNMbnBoZEdOaExtZHZkaTV6WVM5dlkzTndNQTRHQTFVZER3RUIvd1FFQXdJSGdEQWRCZ05WSFNVRUZqQVVCZ2dyQmdFRkJRY0RBZ1lJS3dZQkJRVUhBd013SndZSkt3WUJCQUdDTnhVS0JCb3dHREFLQmdnckJnRUZCUWNEQWpBS0JnZ3JCZ0VGQlFjREF6QUtCZ2dxaGtqT1BRUURBZ05KQURCR0FpRUF5Tmh5Y1EzYk5sTEZkT1BscVlUNlJWUVRXZ25LMUdoME5IZGNTWTRQZkMwQ0lRQ1NBdGhYdnY3dGV0VUw2OVdqcDhCeG5MTE13ZXJ4WmhCbmV3by9nRjNFSkE9PQ==",
				Secret = "f9YRhopN/G7x0TECOY6nKSCHLNYlb5riAHSFPICo4qw=",
			};

			var clientHandlerMock = EInvoicingBranchRegisterHttpClientHandlerMock.New();
			clientHandlerMock.AddJsonResponse(SAEInvoicingAPIEndPoints.Testing_CSID_URL, HttpStatusCode.OK, response1.ToJSON());
			clientHandlerMock.AddJsonResponse(string.Format("{0}/invoices", SAEInvoicingAPIEndPoints.Testing_CSID_URL), HttpStatusCode.OK, response2.ToJSON());
			clientHandlerMock.AddJsonResponse(SAEInvoicingAPIEndPoints.Testing_Onboarding_URL, HttpStatusCode.OK, response3.ToJSON());
			var requestor = GetRequestorInstance(branch, debtor, branchRegister, clientHandlerMock);
			branchRegister.SubstitueCountrySpecificRegistrationRequestor_TestOnly(requestor);

			using (var form = new EInvoicingBranchRegisterForm(branchRegister, null))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.OKButton_Click(null, null);
				AssertEquals("Please enter an One Time Password.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				branchRegister.OTP = "123321";
				requestor.OTP = branchRegister.OTP;
				form.OKButton_Click(null, null);
				AssertEquals("Please enter a Debtor.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				branchRegister.DebtorPK = debtor.PK;
				form.OKButton_Click(null, null);
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(@"-----BEGIN EC PRIVATE KEY-----
MIGNAgEAMBAGByqGSM49AgEGBSuBBAAKBHYwdAIBAQQgg2kcUeZ9SiG9cszT4oa1
7B9Ag63jx7VQLdhxyLFx90egBwYFK4EEAAqhRANCAASncGxIo5dLn7Qk8WB3wZHD
j/9g82+6lC4+aYDdyRuboreH5QxxOf6WT++RQ8wJesVh224FHh3AEXuoqzivvMUD
-----END EC PRIVATE KEY-----
Submitting to https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/compliance
OTP: 123321
-----BEGIN CERTIFICATE REQUEST-----
MIICQzCCAegCAQAwaTELMAkGA1UEBhMCU0ExJzAlBgNVBAoMHlNBVSBDT01QQU5Z
IE9SR0FOSVNBVElPTiBQUk9YWTETMBEGA1UECwwKNjY2Nzc3ODg4OTEcMBoGA1UE
AwwTQ2FyZ293aXNlIERTQUNMSVNBMjBWMBAGByqGSM49AgEGBSuBBAAKA0IABKdw
bEijl0uftCTxYHfBkcOP/2Dzb7qULj5pgN3JG5uit4flDHE5/pZP75FDzAl6xWHb
bgUeHcARe6irOK+8xQOgggEeMIIBGgYJKoZIhvcNAQkOMYIBCzCCAQcwJAYJKwYB
BAGCNxQCBBcTFVBSRVpBVENBLUNvZGUtU2lnbmluZzCB3gYDVR0RBIHWMIHTpIHQ
MIHNMTYwNAYDVQQEDC0xLVdpc2V0ZWNoIEdsb2JhbHwyLUNhcmdvd2lzZXwzLVdU
TERTQUNMSS1TQTIxHzAdBgoJkiaJk/IsZAEBDA8zMzExNjI4Njk0MTAwMDMxDTAL
BgNVBAwMBDEwMDAxRjBEBgNVBBoMPUxFVkVMIDEgM0EgLyA3MiBPJ1JJT1JEQU4g
U1RSRUVUIEFMRVhBTkRSSUEgMDIgMjAxNSBBVVNUUkFMSUExGzAZBgNVBA8MEkxv
Z2lzdGljcyBTZXJ2aWNlczAKBggqhkjOPQQDAgNJADBGAiEAmnix7azJzyJ6xgFi
6CxXfdegrayi506z/tsW1oAoHrcCIQDy/LabgA9IMBAfvCp67HSBzj0FHs56t6VF
+AqG9OgTSw==
-----END CERTIFICATE REQUEST-----
Received response: 200 OK
requestID: 123456
secret: WvUCAV3NKx9ecpzYmg3HWvRbT6Mq0hikQClvJW04kYE=
binarySecurityToken:
-----BEGIN CERTIFICATE-----
MIIB8TCCAZagAwIBAgIGAYhmLlT5MAoGCCqGSM49BAMCMBUxEzARBgNVBAMMCmVJ
bnZvaWNpbmcwHhcNMjMwNTI5MDYyNDQ3WhcNMjgwNTI4MjEwMDAwWjBPMQswCQYD
VQQGEwJTQTEXMBUGA1UECwwOYW1tYW4gQnJhbmNoY2gxEzARBgNVBAoMCmhheWEg
eWFnIDMxEjAQBgNVBAMMCTEyNy4wLjAuMTBWMBAGByqGSM49AgEGBSuBBAAKA0IA
BNuKtif/K/86weEWUw+8VxHEjee26TWLC2KU1i5Xb73mScMCyFvk4WwhgIej+QvT
Qq/Euzj6z6vWDy+pSCmiTxejgZowgZcwDAYDVR0TAQH/BAIwADCBhgYDVR0RBH8w
faR7MHkxGzAZBgNVBAQMEjEtaGF5YXwyLTIzNHwzLTM1NDEfMB0GCgmSJomT8ixk
AQEMDzMxMDE3NTM5NzQwMDAwMzENMAsGA1UEDAwEMTEwMDEQMA4GA1UEGgwHWmF0
Y2EgMzEYMBYGA1UEDwwPRm9vZCBCdXNzaW5lc3MzMAoGCCqGSM49BAMCA0kAMEYC
IQD9fd7K0erJuvbwNaFrKEEYU78Tzbu0XK8ev1HHflzavwIhAM3H8sWZi9GV+sVT
qNiJAeNA/rl+gecjyHhbuoXrB0Kx
-----END CERTIFICATE-----

Submitting test Standard Invoice
Debtor SADEBTOR using address KING FAHD STREET
https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/compliance/invoices
Received response: 200 OK

Submitting test Standard Credit Note
Debtor SADEBTOR using address KING FAHD STREET
https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/compliance/invoices
Received response: 200 OK

Submitting test Standard Debit Note
Debtor SADEBTOR using address KING FAHD STREET
https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/compliance/invoices
Received response: 200 OK


Submitting to https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/production/csids

Received response: 200 OK
requestID: 45678
secret: f9YRhopN/G7x0TECOY6nKSCHLNYlb5riAHSFPICo4qw=
binarySecurityToken:
-----BEGIN CERTIFICATE-----
MIID2zCCA4CgAwIBAgITbwAAdqDmIhqsjpm5CwABAAB2oDAKBggqhkjOPQQDAjBj
MRUwEwYKCZImiZPyLGQBGRYFbG9jYWwxEzARBgoJkiaJk/IsZAEZFgNnb3YxFzAV
BgoJkiaJk/IsZAEZFgdleHRnYXp0MRwwGgYDVQQDExNUU1pFSU5WT0lDRS1TdWJD
QS0xMB4XDTIyMDMyODE1NDYzMloXDTIyMDMzMDE1NDYzMlowTTELMAkGA1UEBhMC
U0ExDjAMBgNVBAoTBUphcmlyMRowGAYDVQQLExFKZWRkYWggQnJhbmNoMTIzNDES
MBAGA1UEAxMJMTI3LjAuMC4xMFYwEAYHKoZIzj0CAQYFK4EEAAoDQgAED/wb2lhB
vBIC8CnnZvouo6OzRymymU9NWRhIyaMhGREBCEZB4EAVrBuV2xXixY4qBYf9dder
zkW9Dwdo3IlHgqOCAiowggImMIGLBgNVHREEgYMwgYCkfjB8MRwwGgYDVQQEDBMy
MjIyMjMyNDQ0MzQzamZmNDMyMR8wHQYKCZImiZPyLGQBAQwPMzEwMTc1Mzk3NDAw
MDAzMQ0wCwYDVQQMDAQxMDExMREwDwYDVQQaDAhTYW1wbGUgRTEZMBcGA1UEDwwQ
U2FtcGxlIEJ1c3NpbmVzczAdBgNVHQ4EFgQUhWcsbbJhjD5ZWOkwBILC+wNVfKYw
HwYDVR0jBBgwFoAUdmCM+wagrGdXNZ3PmqynK5k1tS8wTgYDVR0fBEcwRTBDoEGg
P4Y9aHR0cDovL3RzdGNybC56YXRjYS5nb3Yuc2EvQ2VydEVucm9sbC9UU1pFSU5W
T0lDRS1TdWJDQS0xLmNybDCBrQYIKwYBBQUHAQEEgaAwgZ0wbgYIKwYBBQUHMAGG
Ymh0dHA6Ly90c3RjcmwuemF0Y2EuZ292LnNhL0NlcnRFbnJvbGwvVFNaRWludm9p
Y2VTQ0ExLmV4dGdhenQuZ292LmxvY2FsX1RTWkVJTlZPSUNFLVN1YkNBLTEoMSku
Y3J0MCsGCCsGAQUFBzABhh9odHRwOi8vdHN0Y3JsLnphdGNhLmdvdi5zYS9vY3Nw
MA4GA1UdDwEB/wQEAwIHgDAdBgNVHSUEFjAUBggrBgEFBQcDAgYIKwYBBQUHAwMw
JwYJKwYBBAGCNxUKBBowGDAKBggrBgEFBQcDAjAKBggrBgEFBQcDAzAKBggqhkjO
PQQDAgNJADBGAiEAyNhycQ3bNlLFdOPlqYT6RVQTWgnK1Gh0NHdcSY4PfC0CIQCS
AthXvv7tetUL69Wjp8BxnLLMwerxZhBnewo/gF3EJA==
-----END CERTIFICATE-----", branchRegister.ProgressLog);
			}
		}

		[RequiresSTA]
		public void TestSendRequestWithValidationErrorInTrailTransactionResponse()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var saOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			branch.GB_OH_OrgProxy = saOrgProxy.PK;
			branch.GB_RN_NKCountryCode = "SA";

			var debtor = GetDebtorForTests();

			Factory.Save();

			var branchRegister = new EInvoicingBranchRegister(branch);
			var response1 = new RegistrationGoodResponseData()
			{
				RequestID = "123456",
				DispositionMessage = "ISSUED",
				BinarySecurityToken = "TUlJQjhUQ0NBWmFnQXdJQkFnSUdBWWhtTGxUNU1Bb0dDQ3FHU000OUJBTUNNQlV4RXpBUkJnTlZCQU1NQ21WSmJuWnZhV05wYm1jd0hoY05Nak13TlRJNU1EWXlORFEzV2hjTk1qZ3dOVEk0TWpFd01EQXdXakJQTVFzd0NRWURWUVFHRXdKVFFURVhNQlVHQTFVRUN3d09ZVzF0WVc0Z1FuSmhibU5vWTJneEV6QVJCZ05WQkFvTUNtaGhlV0VnZVdGbklETXhFakFRQmdOVkJBTU1DVEV5Tnk0d0xqQXVNVEJXTUJBR0J5cUdTTTQ5QWdFR0JTdUJCQUFLQTBJQUJOdUt0aWYvSy84NndlRVdVdys4VnhIRWplZTI2VFdMQzJLVTFpNVhiNzNtU2NNQ3lGdms0V3doZ0llaitRdlRRcS9FdXpqNno2dldEeStwU0NtaVR4ZWpnWm93Z1pjd0RBWURWUjBUQVFIL0JBSXdBRENCaGdZRFZSMFJCSDh3ZmFSN01Ia3hHekFaQmdOVkJBUU1FakV0YUdGNVlYd3lMVEl6Tkh3ekxUTTFOREVmTUIwR0NnbVNKb21UOGl4a0FRRU1Eek14TURFM05UTTVOelF3TURBd016RU5NQXNHQTFVRURBd0VNVEV3TURFUU1BNEdBMVVFR2d3SFdtRjBZMkVnTXpFWU1CWUdBMVVFRHd3UFJtOXZaQ0JDZFhOemFXNWxjM016TUFvR0NDcUdTTTQ5QkFNQ0Ewa0FNRVlDSVFEOWZkN0swZXJKdXZid05hRnJLRUVZVTc4VHpidTBYSzhldjFISGZsemF2d0loQU0zSDhzV1ppOUdWK3NWVHFOaUpBZU5BL3JsK2dlY2p5SGhidW9YckIwS3g=",
				Secret = "WvUCAV3NKx9ecpzYmg3HWvRbT6Mq0hikQClvJW04kYE=",
				Errors = null
			};

			var response2 = new ValidationResponseData()
			{
				ValidationResults = new ValidationResults()
				{
					InfoMessages = null,
					WarningMessages = null,
					ErrorMessages = new List<ValidationMessage>
														{ new ValidationMessage()
																	{ Type = "ERROR",
																	Code = "INVOICE_UUID_VALIDATION",
																	Category = "MISSING_UUID_IN_INVOICE",
																	Message = "UUID provided in the invoice doesn't match UUID in the provided Request",
																	Status = "ERROR" } },
					Status = "ERROR"
				},
				ReportingStatus = null,
				ClearanceStatus = "NOT_CLEARED",
				QRSellertStatus = null,
				QRBuyertStatus = null
			};

			var clientHandlerMock = EInvoicingBranchRegisterHttpClientHandlerMock.New();
			clientHandlerMock.AddJsonResponse(SAEInvoicingAPIEndPoints.Testing_CSID_URL, HttpStatusCode.OK, response1.ToJSON());
			clientHandlerMock.AddJsonResponse(string.Format("{0}/invoices", SAEInvoicingAPIEndPoints.Testing_CSID_URL), HttpStatusCode.BadRequest, response2.ToJSON());
			var requestor = GetRequestorInstance(branch, debtor, branchRegister, clientHandlerMock);
			branchRegister.SubstitueCountrySpecificRegistrationRequestor_TestOnly(requestor);

			using (var form = new EInvoicingBranchRegisterForm(branchRegister, null))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.OKButton_Click(null, null);
				AssertEquals("Please enter an One Time Password.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				branchRegister.OTP = "123321";
				requestor.OTP = branchRegister.OTP;
				form.OKButton_Click(null, null);
				AssertEquals("Please enter a Debtor.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				branchRegister.DebtorPK = debtor.PK;
				form.OKButton_Click(null, null);
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(@"-----BEGIN EC PRIVATE KEY-----
MIGNAgEAMBAGByqGSM49AgEGBSuBBAAKBHYwdAIBAQQgg2kcUeZ9SiG9cszT4oa1
7B9Ag63jx7VQLdhxyLFx90egBwYFK4EEAAqhRANCAASncGxIo5dLn7Qk8WB3wZHD
j/9g82+6lC4+aYDdyRuboreH5QxxOf6WT++RQ8wJesVh224FHh3AEXuoqzivvMUD
-----END EC PRIVATE KEY-----
Submitting to https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/compliance
OTP: 123321
-----BEGIN CERTIFICATE REQUEST-----
MIICQzCCAegCAQAwaTELMAkGA1UEBhMCU0ExJzAlBgNVBAoMHlNBVSBDT01QQU5Z
IE9SR0FOSVNBVElPTiBQUk9YWTETMBEGA1UECwwKNjY2Nzc3ODg4OTEcMBoGA1UE
AwwTQ2FyZ293aXNlIERTQUNMSVNBMjBWMBAGByqGSM49AgEGBSuBBAAKA0IABKdw
bEijl0uftCTxYHfBkcOP/2Dzb7qULj5pgN3JG5uit4flDHE5/pZP75FDzAl6xWHb
bgUeHcARe6irOK+8xQOgggEeMIIBGgYJKoZIhvcNAQkOMYIBCzCCAQcwJAYJKwYB
BAGCNxQCBBcTFVBSRVpBVENBLUNvZGUtU2lnbmluZzCB3gYDVR0RBIHWMIHTpIHQ
MIHNMTYwNAYDVQQEDC0xLVdpc2V0ZWNoIEdsb2JhbHwyLUNhcmdvd2lzZXwzLVdU
TERTQUNMSS1TQTIxHzAdBgoJkiaJk/IsZAEBDA8zMzExNjI4Njk0MTAwMDMxDTAL
BgNVBAwMBDEwMDAxRjBEBgNVBBoMPUxFVkVMIDEgM0EgLyA3MiBPJ1JJT1JEQU4g
U1RSRUVUIEFMRVhBTkRSSUEgMDIgMjAxNSBBVVNUUkFMSUExGzAZBgNVBA8MEkxv
Z2lzdGljcyBTZXJ2aWNlczAKBggqhkjOPQQDAgNJADBGAiEAmnix7azJzyJ6xgFi
6CxXfdegrayi506z/tsW1oAoHrcCIQDy/LabgA9IMBAfvCp67HSBzj0FHs56t6VF
+AqG9OgTSw==
-----END CERTIFICATE REQUEST-----
Received response: 200 OK
requestID: 123456
secret: WvUCAV3NKx9ecpzYmg3HWvRbT6Mq0hikQClvJW04kYE=
binarySecurityToken:
-----BEGIN CERTIFICATE-----
MIIB8TCCAZagAwIBAgIGAYhmLlT5MAoGCCqGSM49BAMCMBUxEzARBgNVBAMMCmVJ
bnZvaWNpbmcwHhcNMjMwNTI5MDYyNDQ3WhcNMjgwNTI4MjEwMDAwWjBPMQswCQYD
VQQGEwJTQTEXMBUGA1UECwwOYW1tYW4gQnJhbmNoY2gxEzARBgNVBAoMCmhheWEg
eWFnIDMxEjAQBgNVBAMMCTEyNy4wLjAuMTBWMBAGByqGSM49AgEGBSuBBAAKA0IA
BNuKtif/K/86weEWUw+8VxHEjee26TWLC2KU1i5Xb73mScMCyFvk4WwhgIej+QvT
Qq/Euzj6z6vWDy+pSCmiTxejgZowgZcwDAYDVR0TAQH/BAIwADCBhgYDVR0RBH8w
faR7MHkxGzAZBgNVBAQMEjEtaGF5YXwyLTIzNHwzLTM1NDEfMB0GCgmSJomT8ixk
AQEMDzMxMDE3NTM5NzQwMDAwMzENMAsGA1UEDAwEMTEwMDEQMA4GA1UEGgwHWmF0
Y2EgMzEYMBYGA1UEDwwPRm9vZCBCdXNzaW5lc3MzMAoGCCqGSM49BAMCA0kAMEYC
IQD9fd7K0erJuvbwNaFrKEEYU78Tzbu0XK8ev1HHflzavwIhAM3H8sWZi9GV+sVT
qNiJAeNA/rl+gecjyHhbuoXrB0Kx
-----END CERTIFICATE-----

Submitting test Standard Invoice
Debtor SADEBTOR using address KING FAHD STREET
https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/compliance/invoices
Received response: 400 BadRequest
code: INVOICE_UUID_VALIDATION
message: UUID provided in the invoice doesn't match UUID in the provided Request
", branchRegister.ProgressLog);
			}
		}

		public void TestSendRequestWithValidationWarningInTrailTransactionResponse()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var saOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			branch.GB_OH_OrgProxy = saOrgProxy.PK;
			branch.GB_RN_NKCountryCode = "SA";

			var debtor = GetDebtorForTests();

			Factory.Save();

			var branchRegister = new EInvoicingBranchRegister(branch);
			var response1 = new RegistrationGoodResponseData()
			{
				RequestID = "123456",
				DispositionMessage = "ISSUED",
				BinarySecurityToken = "TUlJQjhUQ0NBWmFnQXdJQkFnSUdBWWhtTGxUNU1Bb0dDQ3FHU000OUJBTUNNQlV4RXpBUkJnTlZCQU1NQ21WSmJuWnZhV05wYm1jd0hoY05Nak13TlRJNU1EWXlORFEzV2hjTk1qZ3dOVEk0TWpFd01EQXdXakJQTVFzd0NRWURWUVFHRXdKVFFURVhNQlVHQTFVRUN3d09ZVzF0WVc0Z1FuSmhibU5vWTJneEV6QVJCZ05WQkFvTUNtaGhlV0VnZVdGbklETXhFakFRQmdOVkJBTU1DVEV5Tnk0d0xqQXVNVEJXTUJBR0J5cUdTTTQ5QWdFR0JTdUJCQUFLQTBJQUJOdUt0aWYvSy84NndlRVdVdys4VnhIRWplZTI2VFdMQzJLVTFpNVhiNzNtU2NNQ3lGdms0V3doZ0llaitRdlRRcS9FdXpqNno2dldEeStwU0NtaVR4ZWpnWm93Z1pjd0RBWURWUjBUQVFIL0JBSXdBRENCaGdZRFZSMFJCSDh3ZmFSN01Ia3hHekFaQmdOVkJBUU1FakV0YUdGNVlYd3lMVEl6Tkh3ekxUTTFOREVmTUIwR0NnbVNKb21UOGl4a0FRRU1Eek14TURFM05UTTVOelF3TURBd016RU5NQXNHQTFVRURBd0VNVEV3TURFUU1BNEdBMVVFR2d3SFdtRjBZMkVnTXpFWU1CWUdBMVVFRHd3UFJtOXZaQ0JDZFhOemFXNWxjM016TUFvR0NDcUdTTTQ5QkFNQ0Ewa0FNRVlDSVFEOWZkN0swZXJKdXZid05hRnJLRUVZVTc4VHpidTBYSzhldjFISGZsemF2d0loQU0zSDhzV1ppOUdWK3NWVHFOaUpBZU5BL3JsK2dlY2p5SGhidW9YckIwS3g=",
				Secret = "WvUCAV3NKx9ecpzYmg3HWvRbT6Mq0hikQClvJW04kYE=",
				Errors = null
			};

			var response2 = new ValidationResponseData()
			{
				ValidationResults = new ValidationResults()
				{
					InfoMessages = null,
					WarningMessages = new List<ValidationMessage>
														{ new ValidationMessage()
																	{ Type = "WARNING",
																	Code = "BR-KSA-81",
																	Category = "KSA",
																	Message = "The other Buyer ID must present in the tax invoice",
																	Status = "WARNING" } },
					ErrorMessages = null,
					Status = "WARNING"
				},
				ReportingStatus = null,
				ClearanceStatus = "CLEARED",
				QRSellertStatus = null,
				QRBuyertStatus = null
			};

			var response3 = new RegistrationGoodResponseData()
			{
				RequestID = "45678",
				TokenType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3",
				DispositionMessage = "ISSUED",
				BinarySecurityToken = "TUlJRDJ6Q0NBNENnQXdJQkFnSVRid0FBZHFEbUlocXNqcG01Q3dBQkFBQjJvREFLQmdncWhrak9QUVFEQWpCak1SVXdFd1lLQ1pJbWlaUHlMR1FCR1JZRmJHOWpZV3d4RXpBUkJnb0praWFKay9Jc1pBRVpGZ05uYjNZeEZ6QVZCZ29Ka2lhSmsvSXNaQUVaRmdkbGVIUm5ZWHAwTVJ3d0dnWURWUVFERXhOVVUxcEZTVTVXVDBsRFJTMVRkV0pEUVMweE1CNFhEVEl5TURNeU9ERTFORFl6TWxvWERUSXlNRE16TURFMU5EWXpNbG93VFRFTE1Ba0dBMVVFQmhNQ1UwRXhEakFNQmdOVkJBb1RCVXBoY21seU1Sb3dHQVlEVlFRTEV4RktaV1JrWVdnZ1FuSmhibU5vTVRJek5ERVNNQkFHQTFVRUF4TUpNVEkzTGpBdU1DNHhNRll3RUFZSEtvWkl6ajBDQVFZRks0RUVBQW9EUWdBRUQvd2IybGhCdkJJQzhDbm5adm91bzZPelJ5bXltVTlOV1JoSXlhTWhHUkVCQ0VaQjRFQVZyQnVWMnhYaXhZNHFCWWY5ZGRlcnprVzlEd2RvM0lsSGdxT0NBaW93Z2dJbU1JR0xCZ05WSFJFRWdZTXdnWUNrZmpCOE1Sd3dHZ1lEVlFRRURCTXlNakl5TWpNeU5EUTBNelF6YW1abU5ETXlNUjh3SFFZS0NaSW1pWlB5TEdRQkFRd1BNekV3TVRjMU16azNOREF3TURBek1RMHdDd1lEVlFRTURBUXhNREV4TVJFd0R3WURWUVFhREFoVFlXMXdiR1VnUlRFWk1CY0dBMVVFRHd3UVUyRnRjR3hsSUVKMWMzTnBibVZ6Y3pBZEJnTlZIUTRFRmdRVWhXY3NiYkpoakQ1WldPa3dCSUxDK3dOVmZLWXdId1lEVlIwakJCZ3dGb0FVZG1DTSt3YWdyR2RYTlozUG1xeW5LNWsxdFM4d1RnWURWUjBmQkVjd1JUQkRvRUdnUDRZOWFIUjBjRG92TDNSemRHTnliQzU2WVhSallTNW5iM1l1YzJFdlEyVnlkRVZ1Y205c2JDOVVVMXBGU1U1V1QwbERSUzFUZFdKRFFTMHhMbU55YkRDQnJRWUlLd1lCQlFVSEFRRUVnYUF3Z1owd2JnWUlLd1lCQlFVSE1BR0dZbWgwZEhBNkx5OTBjM1JqY213dWVtRjBZMkV1WjI5MkxuTmhMME5sY25SRmJuSnZiR3d2VkZOYVJXbHVkbTlwWTJWVFEwRXhMbVY0ZEdkaGVuUXVaMjkyTG14dlkyRnNYMVJUV2tWSlRsWlBTVU5GTFZOMVlrTkJMVEVvTVNrdVkzSjBNQ3NHQ0NzR0FRVUZCekFCaGg5b2RIUndPaTh2ZEhOMFkzSnNMbnBoZEdOaExtZHZkaTV6WVM5dlkzTndNQTRHQTFVZER3RUIvd1FFQXdJSGdEQWRCZ05WSFNVRUZqQVVCZ2dyQmdFRkJRY0RBZ1lJS3dZQkJRVUhBd013SndZSkt3WUJCQUdDTnhVS0JCb3dHREFLQmdnckJnRUZCUWNEQWpBS0JnZ3JCZ0VGQlFjREF6QUtCZ2dxaGtqT1BRUURBZ05KQURCR0FpRUF5Tmh5Y1EzYk5sTEZkT1BscVlUNlJWUVRXZ25LMUdoME5IZGNTWTRQZkMwQ0lRQ1NBdGhYdnY3dGV0VUw2OVdqcDhCeG5MTE13ZXJ4WmhCbmV3by9nRjNFSkE9PQ==",
				Secret = "f9YRhopN/G7x0TECOY6nKSCHLNYlb5riAHSFPICo4qw=",
			};

			var clientHandlerMock = EInvoicingBranchRegisterHttpClientHandlerMock.New();
			clientHandlerMock.AddJsonResponse(SAEInvoicingAPIEndPoints.Testing_CSID_URL, HttpStatusCode.OK, response1.ToJSON());
			clientHandlerMock.AddJsonResponse(string.Format("{0}/invoices", SAEInvoicingAPIEndPoints.Testing_CSID_URL), HttpStatusCode.Accepted, response2.ToJSON());
			clientHandlerMock.AddJsonResponse(SAEInvoicingAPIEndPoints.Testing_Onboarding_URL, HttpStatusCode.OK, response3.ToJSON());
			var requestor = GetRequestorInstance(branch, debtor, branchRegister, clientHandlerMock);
			branchRegister.SubstitueCountrySpecificRegistrationRequestor_TestOnly(requestor);

			using (var form = new EInvoicingBranchRegisterForm(branchRegister, null))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.OKButton_Click(null, null);
				AssertEquals("Please enter an One Time Password.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				branchRegister.OTP = "123321";
				requestor.OTP = branchRegister.OTP;
				form.OKButton_Click(null, null);
				AssertEquals("Please enter a Debtor.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				branchRegister.DebtorPK = debtor.PK;
				form.OKButton_Click(null, null);
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(@"-----BEGIN EC PRIVATE KEY-----
MIGNAgEAMBAGByqGSM49AgEGBSuBBAAKBHYwdAIBAQQgg2kcUeZ9SiG9cszT4oa1
7B9Ag63jx7VQLdhxyLFx90egBwYFK4EEAAqhRANCAASncGxIo5dLn7Qk8WB3wZHD
j/9g82+6lC4+aYDdyRuboreH5QxxOf6WT++RQ8wJesVh224FHh3AEXuoqzivvMUD
-----END EC PRIVATE KEY-----
Submitting to https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/compliance
OTP: 123321
-----BEGIN CERTIFICATE REQUEST-----
MIICQzCCAegCAQAwaTELMAkGA1UEBhMCU0ExJzAlBgNVBAoMHlNBVSBDT01QQU5Z
IE9SR0FOSVNBVElPTiBQUk9YWTETMBEGA1UECwwKNjY2Nzc3ODg4OTEcMBoGA1UE
AwwTQ2FyZ293aXNlIERTQUNMSVNBMjBWMBAGByqGSM49AgEGBSuBBAAKA0IABKdw
bEijl0uftCTxYHfBkcOP/2Dzb7qULj5pgN3JG5uit4flDHE5/pZP75FDzAl6xWHb
bgUeHcARe6irOK+8xQOgggEeMIIBGgYJKoZIhvcNAQkOMYIBCzCCAQcwJAYJKwYB
BAGCNxQCBBcTFVBSRVpBVENBLUNvZGUtU2lnbmluZzCB3gYDVR0RBIHWMIHTpIHQ
MIHNMTYwNAYDVQQEDC0xLVdpc2V0ZWNoIEdsb2JhbHwyLUNhcmdvd2lzZXwzLVdU
TERTQUNMSS1TQTIxHzAdBgoJkiaJk/IsZAEBDA8zMzExNjI4Njk0MTAwMDMxDTAL
BgNVBAwMBDEwMDAxRjBEBgNVBBoMPUxFVkVMIDEgM0EgLyA3MiBPJ1JJT1JEQU4g
U1RSRUVUIEFMRVhBTkRSSUEgMDIgMjAxNSBBVVNUUkFMSUExGzAZBgNVBA8MEkxv
Z2lzdGljcyBTZXJ2aWNlczAKBggqhkjOPQQDAgNJADBGAiEAmnix7azJzyJ6xgFi
6CxXfdegrayi506z/tsW1oAoHrcCIQDy/LabgA9IMBAfvCp67HSBzj0FHs56t6VF
+AqG9OgTSw==
-----END CERTIFICATE REQUEST-----
Received response: 200 OK
requestID: 123456
secret: WvUCAV3NKx9ecpzYmg3HWvRbT6Mq0hikQClvJW04kYE=
binarySecurityToken:
-----BEGIN CERTIFICATE-----
MIIB8TCCAZagAwIBAgIGAYhmLlT5MAoGCCqGSM49BAMCMBUxEzARBgNVBAMMCmVJ
bnZvaWNpbmcwHhcNMjMwNTI5MDYyNDQ3WhcNMjgwNTI4MjEwMDAwWjBPMQswCQYD
VQQGEwJTQTEXMBUGA1UECwwOYW1tYW4gQnJhbmNoY2gxEzARBgNVBAoMCmhheWEg
eWFnIDMxEjAQBgNVBAMMCTEyNy4wLjAuMTBWMBAGByqGSM49AgEGBSuBBAAKA0IA
BNuKtif/K/86weEWUw+8VxHEjee26TWLC2KU1i5Xb73mScMCyFvk4WwhgIej+QvT
Qq/Euzj6z6vWDy+pSCmiTxejgZowgZcwDAYDVR0TAQH/BAIwADCBhgYDVR0RBH8w
faR7MHkxGzAZBgNVBAQMEjEtaGF5YXwyLTIzNHwzLTM1NDEfMB0GCgmSJomT8ixk
AQEMDzMxMDE3NTM5NzQwMDAwMzENMAsGA1UEDAwEMTEwMDEQMA4GA1UEGgwHWmF0
Y2EgMzEYMBYGA1UEDwwPRm9vZCBCdXNzaW5lc3MzMAoGCCqGSM49BAMCA0kAMEYC
IQD9fd7K0erJuvbwNaFrKEEYU78Tzbu0XK8ev1HHflzavwIhAM3H8sWZi9GV+sVT
qNiJAeNA/rl+gecjyHhbuoXrB0Kx
-----END CERTIFICATE-----

Submitting test Standard Invoice
Debtor SADEBTOR using address KING FAHD STREET
https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/compliance/invoices
Received response: 202 Accepted
code: BR-KSA-81
message: The other Buyer ID must present in the tax invoice

Submitting test Standard Credit Note
Debtor SADEBTOR using address KING FAHD STREET
https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/compliance/invoices
Received response: 202 Accepted
code: BR-KSA-81
message: The other Buyer ID must present in the tax invoice

Submitting test Standard Debit Note
Debtor SADEBTOR using address KING FAHD STREET
https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/compliance/invoices
Received response: 202 Accepted
code: BR-KSA-81
message: The other Buyer ID must present in the tax invoice


Submitting to https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/production/csids

Received response: 200 OK
requestID: 45678
secret: f9YRhopN/G7x0TECOY6nKSCHLNYlb5riAHSFPICo4qw=
binarySecurityToken:
-----BEGIN CERTIFICATE-----
MIID2zCCA4CgAwIBAgITbwAAdqDmIhqsjpm5CwABAAB2oDAKBggqhkjOPQQDAjBj
MRUwEwYKCZImiZPyLGQBGRYFbG9jYWwxEzARBgoJkiaJk/IsZAEZFgNnb3YxFzAV
BgoJkiaJk/IsZAEZFgdleHRnYXp0MRwwGgYDVQQDExNUU1pFSU5WT0lDRS1TdWJD
QS0xMB4XDTIyMDMyODE1NDYzMloXDTIyMDMzMDE1NDYzMlowTTELMAkGA1UEBhMC
U0ExDjAMBgNVBAoTBUphcmlyMRowGAYDVQQLExFKZWRkYWggQnJhbmNoMTIzNDES
MBAGA1UEAxMJMTI3LjAuMC4xMFYwEAYHKoZIzj0CAQYFK4EEAAoDQgAED/wb2lhB
vBIC8CnnZvouo6OzRymymU9NWRhIyaMhGREBCEZB4EAVrBuV2xXixY4qBYf9dder
zkW9Dwdo3IlHgqOCAiowggImMIGLBgNVHREEgYMwgYCkfjB8MRwwGgYDVQQEDBMy
MjIyMjMyNDQ0MzQzamZmNDMyMR8wHQYKCZImiZPyLGQBAQwPMzEwMTc1Mzk3NDAw
MDAzMQ0wCwYDVQQMDAQxMDExMREwDwYDVQQaDAhTYW1wbGUgRTEZMBcGA1UEDwwQ
U2FtcGxlIEJ1c3NpbmVzczAdBgNVHQ4EFgQUhWcsbbJhjD5ZWOkwBILC+wNVfKYw
HwYDVR0jBBgwFoAUdmCM+wagrGdXNZ3PmqynK5k1tS8wTgYDVR0fBEcwRTBDoEGg
P4Y9aHR0cDovL3RzdGNybC56YXRjYS5nb3Yuc2EvQ2VydEVucm9sbC9UU1pFSU5W
T0lDRS1TdWJDQS0xLmNybDCBrQYIKwYBBQUHAQEEgaAwgZ0wbgYIKwYBBQUHMAGG
Ymh0dHA6Ly90c3RjcmwuemF0Y2EuZ292LnNhL0NlcnRFbnJvbGwvVFNaRWludm9p
Y2VTQ0ExLmV4dGdhenQuZ292LmxvY2FsX1RTWkVJTlZPSUNFLVN1YkNBLTEoMSku
Y3J0MCsGCCsGAQUFBzABhh9odHRwOi8vdHN0Y3JsLnphdGNhLmdvdi5zYS9vY3Nw
MA4GA1UdDwEB/wQEAwIHgDAdBgNVHSUEFjAUBggrBgEFBQcDAgYIKwYBBQUHAwMw
JwYJKwYBBAGCNxUKBBowGDAKBggrBgEFBQcDAjAKBggrBgEFBQcDAzAKBggqhkjO
PQQDAgNJADBGAiEAyNhycQ3bNlLFdOPlqYT6RVQTWgnK1Gh0NHdcSY4PfC0CIQCS
AthXvv7tetUL69Wjp8BxnLLMwerxZhBnewo/gF3EJA==
-----END CERTIFICATE-----", branchRegister.ProgressLog);
			}
		}

		public void TestSendRequestWithBadResponseInOnboardingAPI()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var saOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			branch.GB_OH_OrgProxy = saOrgProxy.PK;
			branch.GB_RN_NKCountryCode = "SA";

			var debtor = GetDebtorForTests();

			Factory.Save();

			var branchRegister = new EInvoicingBranchRegister(branch);
			var response1 = new RegistrationGoodResponseData()
			{
				RequestID = "123456",
				DispositionMessage = "ISSUED",
				BinarySecurityToken = "TUlJQjhUQ0NBWmFnQXdJQkFnSUdBWWhtTGxUNU1Bb0dDQ3FHU000OUJBTUNNQlV4RXpBUkJnTlZCQU1NQ21WSmJuWnZhV05wYm1jd0hoY05Nak13TlRJNU1EWXlORFEzV2hjTk1qZ3dOVEk0TWpFd01EQXdXakJQTVFzd0NRWURWUVFHRXdKVFFURVhNQlVHQTFVRUN3d09ZVzF0WVc0Z1FuSmhibU5vWTJneEV6QVJCZ05WQkFvTUNtaGhlV0VnZVdGbklETXhFakFRQmdOVkJBTU1DVEV5Tnk0d0xqQXVNVEJXTUJBR0J5cUdTTTQ5QWdFR0JTdUJCQUFLQTBJQUJOdUt0aWYvSy84NndlRVdVdys4VnhIRWplZTI2VFdMQzJLVTFpNVhiNzNtU2NNQ3lGdms0V3doZ0llaitRdlRRcS9FdXpqNno2dldEeStwU0NtaVR4ZWpnWm93Z1pjd0RBWURWUjBUQVFIL0JBSXdBRENCaGdZRFZSMFJCSDh3ZmFSN01Ia3hHekFaQmdOVkJBUU1FakV0YUdGNVlYd3lMVEl6Tkh3ekxUTTFOREVmTUIwR0NnbVNKb21UOGl4a0FRRU1Eek14TURFM05UTTVOelF3TURBd016RU5NQXNHQTFVRURBd0VNVEV3TURFUU1BNEdBMVVFR2d3SFdtRjBZMkVnTXpFWU1CWUdBMVVFRHd3UFJtOXZaQ0JDZFhOemFXNWxjM016TUFvR0NDcUdTTTQ5QkFNQ0Ewa0FNRVlDSVFEOWZkN0swZXJKdXZid05hRnJLRUVZVTc4VHpidTBYSzhldjFISGZsemF2d0loQU0zSDhzV1ppOUdWK3NWVHFOaUpBZU5BL3JsK2dlY2p5SGhidW9YckIwS3g=",
				Secret = "WvUCAV3NKx9ecpzYmg3HWvRbT6Mq0hikQClvJW04kYE=",
				Errors = null
			};

			var response2 = new ValidationResponseData()
			{
				ValidationResults = new ValidationResults()
				{
					InfoMessages = new List<ValidationMessage>
														{ new ValidationMessage()
																	{ Type = "INFO",
																	Code = "XSD_ZATCA_VALID",
																	Category = "XSD validation",
																	Message = "Complied with UBL 2.1 standards in line with ZATCA specifications",
																	Status = "PASS" } },
					WarningMessages = null,
					ErrorMessages = null,
					Status = "PASS"
				},
				ReportingStatus = null,
				ClearanceStatus = "CLEARED",
				QRSellertStatus = null,
				QRBuyertStatus = null
			};

			var clientHandlerMock = EInvoicingBranchRegisterHttpClientHandlerMock.New();
			clientHandlerMock.AddJsonResponse(SAEInvoicingAPIEndPoints.Testing_CSID_URL, HttpStatusCode.OK, response1.ToJSON());
			clientHandlerMock.AddJsonResponse(string.Format("{0}/invoices", SAEInvoicingAPIEndPoints.Testing_CSID_URL), HttpStatusCode.OK, response2.ToJSON());
			clientHandlerMock.AddJsonResponse(SAEInvoicingAPIEndPoints.Testing_Onboarding_URL, HttpStatusCode.InternalServerError, "");
			var requestor = GetRequestorInstance(branch, debtor, branchRegister, clientHandlerMock);
			branchRegister.SubstitueCountrySpecificRegistrationRequestor_TestOnly(requestor);

			using (var form = new EInvoicingBranchRegisterForm(branchRegister, null))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.OKButton_Click(null, null);
				AssertEquals("Please enter an One Time Password.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				branchRegister.OTP = "123321";
				requestor.OTP = branchRegister.OTP;
				form.OKButton_Click(null, null);
				AssertEquals("Please enter a Debtor.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				branchRegister.DebtorPK = debtor.PK;
				form.OKButton_Click(null, null);
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(@"-----BEGIN EC PRIVATE KEY-----
MIGNAgEAMBAGByqGSM49AgEGBSuBBAAKBHYwdAIBAQQgg2kcUeZ9SiG9cszT4oa1
7B9Ag63jx7VQLdhxyLFx90egBwYFK4EEAAqhRANCAASncGxIo5dLn7Qk8WB3wZHD
j/9g82+6lC4+aYDdyRuboreH5QxxOf6WT++RQ8wJesVh224FHh3AEXuoqzivvMUD
-----END EC PRIVATE KEY-----
Submitting to https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/compliance
OTP: 123321
-----BEGIN CERTIFICATE REQUEST-----
MIICQzCCAegCAQAwaTELMAkGA1UEBhMCU0ExJzAlBgNVBAoMHlNBVSBDT01QQU5Z
IE9SR0FOSVNBVElPTiBQUk9YWTETMBEGA1UECwwKNjY2Nzc3ODg4OTEcMBoGA1UE
AwwTQ2FyZ293aXNlIERTQUNMSVNBMjBWMBAGByqGSM49AgEGBSuBBAAKA0IABKdw
bEijl0uftCTxYHfBkcOP/2Dzb7qULj5pgN3JG5uit4flDHE5/pZP75FDzAl6xWHb
bgUeHcARe6irOK+8xQOgggEeMIIBGgYJKoZIhvcNAQkOMYIBCzCCAQcwJAYJKwYB
BAGCNxQCBBcTFVBSRVpBVENBLUNvZGUtU2lnbmluZzCB3gYDVR0RBIHWMIHTpIHQ
MIHNMTYwNAYDVQQEDC0xLVdpc2V0ZWNoIEdsb2JhbHwyLUNhcmdvd2lzZXwzLVdU
TERTQUNMSS1TQTIxHzAdBgoJkiaJk/IsZAEBDA8zMzExNjI4Njk0MTAwMDMxDTAL
BgNVBAwMBDEwMDAxRjBEBgNVBBoMPUxFVkVMIDEgM0EgLyA3MiBPJ1JJT1JEQU4g
U1RSRUVUIEFMRVhBTkRSSUEgMDIgMjAxNSBBVVNUUkFMSUExGzAZBgNVBA8MEkxv
Z2lzdGljcyBTZXJ2aWNlczAKBggqhkjOPQQDAgNJADBGAiEAmnix7azJzyJ6xgFi
6CxXfdegrayi506z/tsW1oAoHrcCIQDy/LabgA9IMBAfvCp67HSBzj0FHs56t6VF
+AqG9OgTSw==
-----END CERTIFICATE REQUEST-----
Received response: 200 OK
requestID: 123456
secret: WvUCAV3NKx9ecpzYmg3HWvRbT6Mq0hikQClvJW04kYE=
binarySecurityToken:
-----BEGIN CERTIFICATE-----
MIIB8TCCAZagAwIBAgIGAYhmLlT5MAoGCCqGSM49BAMCMBUxEzARBgNVBAMMCmVJ
bnZvaWNpbmcwHhcNMjMwNTI5MDYyNDQ3WhcNMjgwNTI4MjEwMDAwWjBPMQswCQYD
VQQGEwJTQTEXMBUGA1UECwwOYW1tYW4gQnJhbmNoY2gxEzARBgNVBAoMCmhheWEg
eWFnIDMxEjAQBgNVBAMMCTEyNy4wLjAuMTBWMBAGByqGSM49AgEGBSuBBAAKA0IA
BNuKtif/K/86weEWUw+8VxHEjee26TWLC2KU1i5Xb73mScMCyFvk4WwhgIej+QvT
Qq/Euzj6z6vWDy+pSCmiTxejgZowgZcwDAYDVR0TAQH/BAIwADCBhgYDVR0RBH8w
faR7MHkxGzAZBgNVBAQMEjEtaGF5YXwyLTIzNHwzLTM1NDEfMB0GCgmSJomT8ixk
AQEMDzMxMDE3NTM5NzQwMDAwMzENMAsGA1UEDAwEMTEwMDEQMA4GA1UEGgwHWmF0
Y2EgMzEYMBYGA1UEDwwPRm9vZCBCdXNzaW5lc3MzMAoGCCqGSM49BAMCA0kAMEYC
IQD9fd7K0erJuvbwNaFrKEEYU78Tzbu0XK8ev1HHflzavwIhAM3H8sWZi9GV+sVT
qNiJAeNA/rl+gecjyHhbuoXrB0Kx
-----END CERTIFICATE-----

Submitting test Standard Invoice
Debtor SADEBTOR using address KING FAHD STREET
https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/compliance/invoices
Received response: 200 OK

Submitting test Standard Credit Note
Debtor SADEBTOR using address KING FAHD STREET
https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/compliance/invoices
Received response: 200 OK

Submitting test Standard Debit Note
Debtor SADEBTOR using address KING FAHD STREET
https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/compliance/invoices
Received response: 200 OK


Submitting to https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/production/csids

Received response: 500 InternalServerError

Response: 
", branchRegister.ProgressLog);
			}
		}

		public void TestSendRequestWithGoodResponse_WithEmptyContent()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var saOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			branch.GB_OH_OrgProxy = saOrgProxy.PK;
			branch.GB_RN_NKCountryCode = "SA";

			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.OH_IsDebtor = true;
			debtor.MainAddress.ClosestPort = "SAABT";
			debtor.CompanyData.OB_ARVATConfig = OrganisationTaxConfiguartionTypes.Default.Code;

			Factory.Save();

			var branchRegister = new EInvoicingBranchRegister(branch);

			var clientHandlerMock = EInvoicingBranchRegisterHttpClientHandlerMock.New();
			clientHandlerMock.AddJsonResponse(SAEInvoicingAPIEndPoints.Testing_CSID_URL, HttpStatusCode.OK, "");
			var requestor = GetRequestorInstance(branch, debtor, branchRegister, clientHandlerMock);
			branchRegister.SubstitueCountrySpecificRegistrationRequestor_TestOnly(requestor);

			using (var form = new EInvoicingBranchRegisterForm(branchRegister, null))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.OKButton_Click(null, null);
				AssertEquals("Please enter an One Time Password.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				branchRegister.OTP = "123321";
				requestor.OTP = branchRegister.OTP;
				form.OKButton_Click(null, null);
				AssertEquals("Please enter a Debtor.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				branchRegister.DebtorPK = debtor.PK;
				form.OKButton_Click(null, null);
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertMultilineASCIIEquals(@"-----BEGIN EC PRIVATE KEY-----
MIGNAgEAMBAGByqGSM49AgEGBSuBBAAKBHYwdAIBAQQgg2kcUeZ9SiG9cszT4oa1
7B9Ag63jx7VQLdhxyLFx90egBwYFK4EEAAqhRANCAASncGxIo5dLn7Qk8WB3wZHD
j/9g82+6lC4+aYDdyRuboreH5QxxOf6WT++RQ8wJesVh224FHh3AEXuoqzivvMUD
-----END EC PRIVATE KEY-----
Submitting to https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/compliance
OTP: 123321
-----BEGIN CERTIFICATE REQUEST-----
MIICQzCCAegCAQAwaTELMAkGA1UEBhMCU0ExJzAlBgNVBAoMHlNBVSBDT01QQU5Z
IE9SR0FOSVNBVElPTiBQUk9YWTETMBEGA1UECwwKNjY2Nzc3ODg4OTEcMBoGA1UE
AwwTQ2FyZ293aXNlIERTQUNMSVNBMjBWMBAGByqGSM49AgEGBSuBBAAKA0IABKdw
bEijl0uftCTxYHfBkcOP/2Dzb7qULj5pgN3JG5uit4flDHE5/pZP75FDzAl6xWHb
bgUeHcARe6irOK+8xQOgggEeMIIBGgYJKoZIhvcNAQkOMYIBCzCCAQcwJAYJKwYB
BAGCNxQCBBcTFVBSRVpBVENBLUNvZGUtU2lnbmluZzCB3gYDVR0RBIHWMIHTpIHQ
MIHNMTYwNAYDVQQEDC0xLVdpc2V0ZWNoIEdsb2JhbHwyLUNhcmdvd2lzZXwzLVdU
TERTQUNMSS1TQTIxHzAdBgoJkiaJk/IsZAEBDA8zMzExNjI4Njk0MTAwMDMxDTAL
BgNVBAwMBDEwMDAxRjBEBgNVBBoMPUxFVkVMIDEgM0EgLyA3MiBPJ1JJT1JEQU4g
U1RSRUVUIEFMRVhBTkRSSUEgMDIgMjAxNSBBVVNUUkFMSUExGzAZBgNVBA8MEkxv
Z2lzdGljcyBTZXJ2aWNlczAKBggqhkjOPQQDAgNJADBGAiEAmnix7azJzyJ6xgFi
6CxXfdegrayi506z/tsW1oAoHrcCIQDy/LabgA9IMBAfvCp67HSBzj0FHs56t6VF
+AqG9OgTSw==
-----END CERTIFICATE REQUEST-----
Received response: 200 OK

Unable to retrieve response RegistrationGoodResponseData - object is null.
Response:
", branchRegister.ProgressLog);
			}
		}

		public void TestSendRequestWithBadResponse_ListFormat()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var saOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			branch.GB_OH_OrgProxy = saOrgProxy.PK;
			branch.GB_RN_NKCountryCode = "SA";

			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.OH_IsDebtor = true;
			debtor.MainAddress.ClosestPort = "SAABT";
			debtor.CompanyData.OB_ARVATConfig = OrganisationTaxConfiguartionTypes.Default.Code;

			Factory.Save();

			var branchRegister = new EInvoicingBranchRegister(branch);
			var response = new RegistrationBadResponseDataList()
			{
				Errors = new List<RegistrationBadResponseData>()
				 {
					 new RegistrationBadResponseData()
					{
						Code = "Missing-OTP",
						Message = "OTP is required field"
					}
				 }
			};

			var clientHandlerMock = EInvoicingBranchRegisterHttpClientHandlerMock.New();
			clientHandlerMock.ClearAllResponses();
			clientHandlerMock.AddJsonResponse(SAEInvoicingAPIEndPoints.Testing_CSID_URL, HttpStatusCode.BadRequest, response.ToJSON());
			var requestor = GetRequestorInstance(branch, debtor, branchRegister, clientHandlerMock);
			branchRegister.SubstitueCountrySpecificRegistrationRequestor_TestOnly(requestor);

			using (var form = new EInvoicingBranchRegisterForm(branchRegister, null))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.OKButton_Click(null, null);
				AssertEquals("Please enter an One Time Password.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				branchRegister.OTP = "123321";
				requestor.OTP = branchRegister.OTP;
				form.OKButton_Click(null, null);
				AssertEquals("Please enter a Debtor.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				branchRegister.DebtorPK = debtor.PK;
				form.OKButton_Click(null, null);
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(@"-----BEGIN EC PRIVATE KEY-----
MIGNAgEAMBAGByqGSM49AgEGBSuBBAAKBHYwdAIBAQQgg2kcUeZ9SiG9cszT4oa1
7B9Ag63jx7VQLdhxyLFx90egBwYFK4EEAAqhRANCAASncGxIo5dLn7Qk8WB3wZHD
j/9g82+6lC4+aYDdyRuboreH5QxxOf6WT++RQ8wJesVh224FHh3AEXuoqzivvMUD
-----END EC PRIVATE KEY-----
Submitting to https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/compliance
OTP: 123321
-----BEGIN CERTIFICATE REQUEST-----
MIICQzCCAegCAQAwaTELMAkGA1UEBhMCU0ExJzAlBgNVBAoMHlNBVSBDT01QQU5Z
IE9SR0FOSVNBVElPTiBQUk9YWTETMBEGA1UECwwKNjY2Nzc3ODg4OTEcMBoGA1UE
AwwTQ2FyZ293aXNlIERTQUNMSVNBMjBWMBAGByqGSM49AgEGBSuBBAAKA0IABKdw
bEijl0uftCTxYHfBkcOP/2Dzb7qULj5pgN3JG5uit4flDHE5/pZP75FDzAl6xWHb
bgUeHcARe6irOK+8xQOgggEeMIIBGgYJKoZIhvcNAQkOMYIBCzCCAQcwJAYJKwYB
BAGCNxQCBBcTFVBSRVpBVENBLUNvZGUtU2lnbmluZzCB3gYDVR0RBIHWMIHTpIHQ
MIHNMTYwNAYDVQQEDC0xLVdpc2V0ZWNoIEdsb2JhbHwyLUNhcmdvd2lzZXwzLVdU
TERTQUNMSS1TQTIxHzAdBgoJkiaJk/IsZAEBDA8zMzExNjI4Njk0MTAwMDMxDTAL
BgNVBAwMBDEwMDAxRjBEBgNVBBoMPUxFVkVMIDEgM0EgLyA3MiBPJ1JJT1JEQU4g
U1RSRUVUIEFMRVhBTkRSSUEgMDIgMjAxNSBBVVNUUkFMSUExGzAZBgNVBA8MEkxv
Z2lzdGljcyBTZXJ2aWNlczAKBggqhkjOPQQDAgNJADBGAiEAmnix7azJzyJ6xgFi
6CxXfdegrayi506z/tsW1oAoHrcCIQDy/LabgA9IMBAfvCp67HSBzj0FHs56t6VF
+AqG9OgTSw==
-----END CERTIFICATE REQUEST-----
Received response: 400 BadRequest
code: Missing-OTP
message: OTP is required field
", branchRegister.ProgressLog);
			}
		}

		public void TestSendRequestWithBadResponse_NonListFormat()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var saOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			branch.GB_OH_OrgProxy = saOrgProxy.PK;
			branch.GB_RN_NKCountryCode = "SA";

			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.OH_IsDebtor = true;
			debtor.MainAddress.ClosestPort = "SAABT";
			debtor.CompanyData.OB_ARVATConfig = OrganisationTaxConfiguartionTypes.Default.Code;

			Factory.Save();

			var branchRegister = new EInvoicingBranchRegister(branch);
			var response = new RegistrationBadResponseData()
			{
				Code = "Missing-OTP",
				Message = "OTP is required field"
			};

			var clientHandlerMock = EInvoicingBranchRegisterHttpClientHandlerMock.New();
			clientHandlerMock.AddJsonResponse(SAEInvoicingAPIEndPoints.Testing_CSID_URL, HttpStatusCode.BadRequest, response.ToJSON());
			var requestor = GetRequestorInstance(branch, debtor, branchRegister, clientHandlerMock);
			branchRegister.SubstitueCountrySpecificRegistrationRequestor_TestOnly(requestor);

			using (var form = new EInvoicingBranchRegisterForm(branchRegister, null))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.OKButton_Click(null, null);
				AssertEquals("Please enter an One Time Password.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				branchRegister.OTP = "123321";
				requestor.OTP = branchRegister.OTP;
				form.OKButton_Click(null, null);
				AssertEquals("Please enter a Debtor.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				branchRegister.DebtorPK = debtor.PK;
				form.OKButton_Click(null, null);
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(@"-----BEGIN EC PRIVATE KEY-----
MIGNAgEAMBAGByqGSM49AgEGBSuBBAAKBHYwdAIBAQQgg2kcUeZ9SiG9cszT4oa1
7B9Ag63jx7VQLdhxyLFx90egBwYFK4EEAAqhRANCAASncGxIo5dLn7Qk8WB3wZHD
j/9g82+6lC4+aYDdyRuboreH5QxxOf6WT++RQ8wJesVh224FHh3AEXuoqzivvMUD
-----END EC PRIVATE KEY-----
Submitting to https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/compliance
OTP: 123321
-----BEGIN CERTIFICATE REQUEST-----
MIICQzCCAegCAQAwaTELMAkGA1UEBhMCU0ExJzAlBgNVBAoMHlNBVSBDT01QQU5Z
IE9SR0FOSVNBVElPTiBQUk9YWTETMBEGA1UECwwKNjY2Nzc3ODg4OTEcMBoGA1UE
AwwTQ2FyZ293aXNlIERTQUNMSVNBMjBWMBAGByqGSM49AgEGBSuBBAAKA0IABKdw
bEijl0uftCTxYHfBkcOP/2Dzb7qULj5pgN3JG5uit4flDHE5/pZP75FDzAl6xWHb
bgUeHcARe6irOK+8xQOgggEeMIIBGgYJKoZIhvcNAQkOMYIBCzCCAQcwJAYJKwYB
BAGCNxQCBBcTFVBSRVpBVENBLUNvZGUtU2lnbmluZzCB3gYDVR0RBIHWMIHTpIHQ
MIHNMTYwNAYDVQQEDC0xLVdpc2V0ZWNoIEdsb2JhbHwyLUNhcmdvd2lzZXwzLVdU
TERTQUNMSS1TQTIxHzAdBgoJkiaJk/IsZAEBDA8zMzExNjI4Njk0MTAwMDMxDTAL
BgNVBAwMBDEwMDAxRjBEBgNVBBoMPUxFVkVMIDEgM0EgLyA3MiBPJ1JJT1JEQU4g
U1RSRUVUIEFMRVhBTkRSSUEgMDIgMjAxNSBBVVNUUkFMSUExGzAZBgNVBA8MEkxv
Z2lzdGljcyBTZXJ2aWNlczAKBggqhkjOPQQDAgNJADBGAiEAmnix7azJzyJ6xgFi
6CxXfdegrayi506z/tsW1oAoHrcCIQDy/LabgA9IMBAfvCp67HSBzj0FHs56t6VF
+AqG9OgTSw==
-----END CERTIFICATE REQUEST-----
Received response: 400 BadRequest
code: Missing-OTP
message: OTP is required field
", branchRegister.ProgressLog);
			}
		}

		public void TestSendRequestWithBadResponse_NonJSonContent()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var saOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			branch.GB_OH_OrgProxy = saOrgProxy.PK;
			branch.GB_RN_NKCountryCode = "SA";

			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.OH_IsDebtor = true;
			debtor.MainAddress.ClosestPort = "SAABT";
			debtor.CompanyData.OB_ARVATConfig = OrganisationTaxConfiguartionTypes.Default.Code;

			Factory.Save();

			var branchRegister = new EInvoicingBranchRegister(branch);
			var response = "This version is not supported or not provided in the header";

			var clientHandlerMock = EInvoicingBranchRegisterHttpClientHandlerMock.New();
			clientHandlerMock.AddJsonResponse(SAEInvoicingAPIEndPoints.Testing_CSID_URL, HttpStatusCode.NotAcceptable, response);
			var requestor = GetRequestorInstance(branch, debtor, branchRegister, clientHandlerMock);
			branchRegister.SubstitueCountrySpecificRegistrationRequestor_TestOnly(requestor);

			using (var form = new EInvoicingBranchRegisterForm(branchRegister, null))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.OKButton_Click(null, null);
				AssertEquals("Please enter an One Time Password.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				branchRegister.OTP = "123321";
				requestor.OTP = branchRegister.OTP;
				form.OKButton_Click(null, null);
				AssertEquals("Please enter a Debtor.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				branchRegister.DebtorPK = debtor.PK;
				form.OKButton_Click(null, null);
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(@"-----BEGIN EC PRIVATE KEY-----
MIGNAgEAMBAGByqGSM49AgEGBSuBBAAKBHYwdAIBAQQgg2kcUeZ9SiG9cszT4oa1
7B9Ag63jx7VQLdhxyLFx90egBwYFK4EEAAqhRANCAASncGxIo5dLn7Qk8WB3wZHD
j/9g82+6lC4+aYDdyRuboreH5QxxOf6WT++RQ8wJesVh224FHh3AEXuoqzivvMUD
-----END EC PRIVATE KEY-----
Submitting to https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/compliance
OTP: 123321
-----BEGIN CERTIFICATE REQUEST-----
MIICQzCCAegCAQAwaTELMAkGA1UEBhMCU0ExJzAlBgNVBAoMHlNBVSBDT01QQU5Z
IE9SR0FOSVNBVElPTiBQUk9YWTETMBEGA1UECwwKNjY2Nzc3ODg4OTEcMBoGA1UE
AwwTQ2FyZ293aXNlIERTQUNMSVNBMjBWMBAGByqGSM49AgEGBSuBBAAKA0IABKdw
bEijl0uftCTxYHfBkcOP/2Dzb7qULj5pgN3JG5uit4flDHE5/pZP75FDzAl6xWHb
bgUeHcARe6irOK+8xQOgggEeMIIBGgYJKoZIhvcNAQkOMYIBCzCCAQcwJAYJKwYB
BAGCNxQCBBcTFVBSRVpBVENBLUNvZGUtU2lnbmluZzCB3gYDVR0RBIHWMIHTpIHQ
MIHNMTYwNAYDVQQEDC0xLVdpc2V0ZWNoIEdsb2JhbHwyLUNhcmdvd2lzZXwzLVdU
TERTQUNMSS1TQTIxHzAdBgoJkiaJk/IsZAEBDA8zMzExNjI4Njk0MTAwMDMxDTAL
BgNVBAwMBDEwMDAxRjBEBgNVBBoMPUxFVkVMIDEgM0EgLyA3MiBPJ1JJT1JEQU4g
U1RSRUVUIEFMRVhBTkRSSUEgMDIgMjAxNSBBVVNUUkFMSUExGzAZBgNVBA8MEkxv
Z2lzdGljcyBTZXJ2aWNlczAKBggqhkjOPQQDAgNJADBGAiEAmnix7azJzyJ6xgFi
6CxXfdegrayi506z/tsW1oAoHrcCIQDy/LabgA9IMBAfvCp67HSBzj0FHs56t6VF
+AqG9OgTSw==
-----END CERTIFICATE REQUEST-----
Received response: 406 NotAcceptable

Response: This version is not supported or not provided in the header
", branchRegister.ProgressLog);
			}
		}

		public void TestSendRequestWithBadResponse_NoResponseContent()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var saOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			branch.GB_OH_OrgProxy = saOrgProxy.PK;
			branch.GB_RN_NKCountryCode = "SA";

			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.OH_IsDebtor = true;
			debtor.MainAddress.ClosestPort = "SAABT";
			debtor.CompanyData.OB_ARVATConfig = OrganisationTaxConfiguartionTypes.Default.Code;

			Factory.Save();

			var branchRegister = new EInvoicingBranchRegister(branch);

			var clientHandlerMock = EInvoicingBranchRegisterHttpClientHandlerMock.New();
			clientHandlerMock.AddJsonResponse(SAEInvoicingAPIEndPoints.Testing_CSID_URL, HttpStatusCode.InternalServerError, "");
			var requestor = GetRequestorInstance(branch, debtor, branchRegister, clientHandlerMock);
			branchRegister.SubstitueCountrySpecificRegistrationRequestor_TestOnly(requestor);

			using (var form = new EInvoicingBranchRegisterForm(branchRegister, null))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.OKButton_Click(null, null);
				AssertEquals("Please enter an One Time Password.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				branchRegister.OTP = "123321";
				requestor.OTP = branchRegister.OTP;
				form.OKButton_Click(null, null);
				AssertEquals("Please enter a Debtor.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				branchRegister.DebtorPK = debtor.PK;
				form.OKButton_Click(null, null);
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(@"-----BEGIN EC PRIVATE KEY-----
MIGNAgEAMBAGByqGSM49AgEGBSuBBAAKBHYwdAIBAQQgg2kcUeZ9SiG9cszT4oa1
7B9Ag63jx7VQLdhxyLFx90egBwYFK4EEAAqhRANCAASncGxIo5dLn7Qk8WB3wZHD
j/9g82+6lC4+aYDdyRuboreH5QxxOf6WT++RQ8wJesVh224FHh3AEXuoqzivvMUD
-----END EC PRIVATE KEY-----
Submitting to https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/compliance
OTP: 123321
-----BEGIN CERTIFICATE REQUEST-----
MIICQzCCAegCAQAwaTELMAkGA1UEBhMCU0ExJzAlBgNVBAoMHlNBVSBDT01QQU5Z
IE9SR0FOSVNBVElPTiBQUk9YWTETMBEGA1UECwwKNjY2Nzc3ODg4OTEcMBoGA1UE
AwwTQ2FyZ293aXNlIERTQUNMSVNBMjBWMBAGByqGSM49AgEGBSuBBAAKA0IABKdw
bEijl0uftCTxYHfBkcOP/2Dzb7qULj5pgN3JG5uit4flDHE5/pZP75FDzAl6xWHb
bgUeHcARe6irOK+8xQOgggEeMIIBGgYJKoZIhvcNAQkOMYIBCzCCAQcwJAYJKwYB
BAGCNxQCBBcTFVBSRVpBVENBLUNvZGUtU2lnbmluZzCB3gYDVR0RBIHWMIHTpIHQ
MIHNMTYwNAYDVQQEDC0xLVdpc2V0ZWNoIEdsb2JhbHwyLUNhcmdvd2lzZXwzLVdU
TERTQUNMSS1TQTIxHzAdBgoJkiaJk/IsZAEBDA8zMzExNjI4Njk0MTAwMDMxDTAL
BgNVBAwMBDEwMDAxRjBEBgNVBBoMPUxFVkVMIDEgM0EgLyA3MiBPJ1JJT1JEQU4g
U1RSRUVUIEFMRVhBTkRSSUEgMDIgMjAxNSBBVVNUUkFMSUExGzAZBgNVBA8MEkxv
Z2lzdGljcyBTZXJ2aWNlczAKBggqhkjOPQQDAgNJADBGAiEAmnix7azJzyJ6xgFi
6CxXfdegrayi506z/tsW1oAoHrcCIQDy/LabgA9IMBAfvCp67HSBzj0FHs56t6VF
+AqG9OgTSw==
-----END CERTIFICATE REQUEST-----
Received response: 500 InternalServerError

Response: 
", branchRegister.ProgressLog);
			}
		}

		public void TestCredentialCreatedWhenDataRegisteredSuccessfully()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			company.GC_RN_NKCountryCode = "SA";
			company.GC_Name = "Name";
			company.GC_Address1 = "Address 1";
			company.GC_City = "City";
			company.GC_PostCode = "1234";
			var branch = company.Branches.AddNew();
			branch.GB_RN_NKCountryCode = "SA";

			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.OH_IsDebtor = true;
			debtor.MainAddress.ClosestPort = "SAABT";
			debtor.CompanyData.OB_ARVATConfig = OrganisationTaxConfiguartionTypes.Default.Code;

			Factory.Save();

			var branchRegister = new EInvoicingBranchRegister(branch);
			var response1 = new RegistrationGoodResponseData()
			{
				RequestID = "123456",
				DispositionMessage = "ISSUED",
				BinarySecurityToken = "TUlJQjhUQ0NBWmFnQXdJQkFnSUdBWWhtTGxUNU1Bb0dDQ3FHU000OUJBTUNNQlV4RXpBUkJnTlZCQU1NQ21WSmJuWnZhV05wYm1jd0hoY05Nak13TlRJNU1EWXlORFEzV2hjTk1qZ3dOVEk0TWpFd01EQXdXakJQTVFzd0NRWURWUVFHRXdKVFFURVhNQlVHQTFVRUN3d09ZVzF0WVc0Z1FuSmhibU5vWTJneEV6QVJCZ05WQkFvTUNtaGhlV0VnZVdGbklETXhFakFRQmdOVkJBTU1DVEV5Tnk0d0xqQXVNVEJXTUJBR0J5cUdTTTQ5QWdFR0JTdUJCQUFLQTBJQUJOdUt0aWYvSy84NndlRVdVdys4VnhIRWplZTI2VFdMQzJLVTFpNVhiNzNtU2NNQ3lGdms0V3doZ0llaitRdlRRcS9FdXpqNno2dldEeStwU0NtaVR4ZWpnWm93Z1pjd0RBWURWUjBUQVFIL0JBSXdBRENCaGdZRFZSMFJCSDh3ZmFSN01Ia3hHekFaQmdOVkJBUU1FakV0YUdGNVlYd3lMVEl6Tkh3ekxUTTFOREVmTUIwR0NnbVNKb21UOGl4a0FRRU1Eek14TURFM05UTTVOelF3TURBd016RU5NQXNHQTFVRURBd0VNVEV3TURFUU1BNEdBMVVFR2d3SFdtRjBZMkVnTXpFWU1CWUdBMVVFRHd3UFJtOXZaQ0JDZFhOemFXNWxjM016TUFvR0NDcUdTTTQ5QkFNQ0Ewa0FNRVlDSVFEOWZkN0swZXJKdXZid05hRnJLRUVZVTc4VHpidTBYSzhldjFISGZsemF2d0loQU0zSDhzV1ppOUdWK3NWVHFOaUpBZU5BL3JsK2dlY2p5SGhidW9YckIwS3g=",
				Secret = "WvUCAV3NKx9ecpzYmg3HWvRbT6Mq0hikQClvJW04kYE=",
				Errors = null
			};

			var response2 = new ValidationResponseData()
			{
				ValidationResults = new ValidationResults()
				{
					InfoMessages = new List<ValidationMessage>
														{ new ValidationMessage()
																	{ Type = "INFO",
																	Code = "XSD_ZATCA_VALID",
																	Category = "XSD validation",
																	Message = "Complied with UBL 2.1 standards in line with ZATCA specifications",
																	Status = "PASS" } },
					WarningMessages = null,
					ErrorMessages = null,
					Status = "PASS"
				},
				ReportingStatus = null,
				ClearanceStatus = "CLEARED",
				QRSellertStatus = null,
				QRBuyertStatus = null
			};

			var response3 = new RegistrationGoodResponseData()
			{
				RequestID = "45678",
				TokenType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3",
				DispositionMessage = "ISSUED",
				BinarySecurityToken = "TUlJRDJ6Q0NBNENnQXdJQkFnSVRid0FBZHFEbUlocXNqcG01Q3dBQkFBQjJvREFLQmdncWhrak9QUVFEQWpCak1SVXdFd1lLQ1pJbWlaUHlMR1FCR1JZRmJHOWpZV3d4RXpBUkJnb0praWFKay9Jc1pBRVpGZ05uYjNZeEZ6QVZCZ29Ka2lhSmsvSXNaQUVaRmdkbGVIUm5ZWHAwTVJ3d0dnWURWUVFERXhOVVUxcEZTVTVXVDBsRFJTMVRkV0pEUVMweE1CNFhEVEl5TURNeU9ERTFORFl6TWxvWERUSXlNRE16TURFMU5EWXpNbG93VFRFTE1Ba0dBMVVFQmhNQ1UwRXhEakFNQmdOVkJBb1RCVXBoY21seU1Sb3dHQVlEVlFRTEV4RktaV1JrWVdnZ1FuSmhibU5vTVRJek5ERVNNQkFHQTFVRUF4TUpNVEkzTGpBdU1DNHhNRll3RUFZSEtvWkl6ajBDQVFZRks0RUVBQW9EUWdBRUQvd2IybGhCdkJJQzhDbm5adm91bzZPelJ5bXltVTlOV1JoSXlhTWhHUkVCQ0VaQjRFQVZyQnVWMnhYaXhZNHFCWWY5ZGRlcnprVzlEd2RvM0lsSGdxT0NBaW93Z2dJbU1JR0xCZ05WSFJFRWdZTXdnWUNrZmpCOE1Sd3dHZ1lEVlFRRURCTXlNakl5TWpNeU5EUTBNelF6YW1abU5ETXlNUjh3SFFZS0NaSW1pWlB5TEdRQkFRd1BNekV3TVRjMU16azNOREF3TURBek1RMHdDd1lEVlFRTURBUXhNREV4TVJFd0R3WURWUVFhREFoVFlXMXdiR1VnUlRFWk1CY0dBMVVFRHd3UVUyRnRjR3hsSUVKMWMzTnBibVZ6Y3pBZEJnTlZIUTRFRmdRVWhXY3NiYkpoakQ1WldPa3dCSUxDK3dOVmZLWXdId1lEVlIwakJCZ3dGb0FVZG1DTSt3YWdyR2RYTlozUG1xeW5LNWsxdFM4d1RnWURWUjBmQkVjd1JUQkRvRUdnUDRZOWFIUjBjRG92TDNSemRHTnliQzU2WVhSallTNW5iM1l1YzJFdlEyVnlkRVZ1Y205c2JDOVVVMXBGU1U1V1QwbERSUzFUZFdKRFFTMHhMbU55YkRDQnJRWUlLd1lCQlFVSEFRRUVnYUF3Z1owd2JnWUlLd1lCQlFVSE1BR0dZbWgwZEhBNkx5OTBjM1JqY213dWVtRjBZMkV1WjI5MkxuTmhMME5sY25SRmJuSnZiR3d2VkZOYVJXbHVkbTlwWTJWVFEwRXhMbVY0ZEdkaGVuUXVaMjkyTG14dlkyRnNYMVJUV2tWSlRsWlBTVU5GTFZOMVlrTkJMVEVvTVNrdVkzSjBNQ3NHQ0NzR0FRVUZCekFCaGg5b2RIUndPaTh2ZEhOMFkzSnNMbnBoZEdOaExtZHZkaTV6WVM5dlkzTndNQTRHQTFVZER3RUIvd1FFQXdJSGdEQWRCZ05WSFNVRUZqQVVCZ2dyQmdFRkJRY0RBZ1lJS3dZQkJRVUhBd013SndZSkt3WUJCQUdDTnhVS0JCb3dHREFLQmdnckJnRUZCUWNEQWpBS0JnZ3JCZ0VGQlFjREF6QUtCZ2dxaGtqT1BRUURBZ05KQURCR0FpRUF5Tmh5Y1EzYk5sTEZkT1BscVlUNlJWUVRXZ25LMUdoME5IZGNTWTRQZkMwQ0lRQ1NBdGhYdnY3dGV0VUw2OVdqcDhCeG5MTE13ZXJ4WmhCbmV3by9nRjNFSkE9PQ==",
				Secret = "f9YRhopN/G7x0TECOY6nKSCHLNYlb5riAHSFPICo4qw=",
			};

			var clientHandlerMock = EInvoicingBranchRegisterHttpClientHandlerMock.New();
			clientHandlerMock.AddJsonResponse(SAEInvoicingAPIEndPoints.Testing_CSID_URL, HttpStatusCode.OK, response1.ToJSON());
			clientHandlerMock.AddJsonResponse(string.Format("{0}/invoices", SAEInvoicingAPIEndPoints.Testing_CSID_URL), HttpStatusCode.OK, response2.ToJSON());
			clientHandlerMock.AddJsonResponse(SAEInvoicingAPIEndPoints.Testing_Onboarding_URL, HttpStatusCode.OK, response3.ToJSON());
			var requestor = GetRequestorInstance(branch, debtor, branchRegister, clientHandlerMock);
			branchRegister.SubstitueCountrySpecificRegistrationRequestor_TestOnly(requestor);

			using (var branchForm = new GlbBranchForm(branch))
			{
				branchForm.Show();
				branchForm.BranchTabControl_TestOnly.SelectTab("EInvoicingCredentialTabPage");
				//remove the ghost row in the grid
				branchForm.CertificateCredentialControl_TestOnly.X509CertificatesGrid_TestOnly.ListManager.RemoveAt(0);
				using (var registrationForm = new EInvoicingBranchRegisterForm(branchRegister, branchForm.CertificateCredentialControl_TestOnly.CertificateLoaderUserControl_TestOnly))
				{
					registrationForm.Show();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					branchRegister.OTP = "123321";
					requestor.OTP = branchRegister.OTP;
					branchRegister.DebtorPK = debtor.PK;
					registrationForm.OKButton_Click(null, null);
					Assert(branchRegister.ProgressLog.EndsWith("\r\nAdded to Branch record\r\n"));
					AssertEquals("Before factory save", 1, branch.EInvoicingCertificateCredentials.Count);
					Factory.Save();
					var newObjFactory = new BusinessObjectFactory();
					var loadedBranch = newObjFactory.Load<GlbBranch>(branch.PK);
					AssertEquals(1, loadedBranch.EInvoicingCertificateCredentials.Count);
					var credential = branch.EInvoicingCertificateCredentials[0];
					AssertEquals("127.0.0.1", credential.SubjectNameCommonName);
					AssertEquals("TSZEINVOICE-SubCA-1", credential.IssuerNameCommonName);
					AssertEquals("29-Mar-22", credential.GP_IssueDate.ToShortDateString());
					AssertEquals("31-Mar-22", credential.GP_ExpiryDate.ToShortDateString());
					AssertEquals("BABF8156CCF1DB06AA65AFD26E0EEA7C60AA5598", credential.GP_UserID);
					AssertEquals("f9YRhopN/G7x0TECOY6nKSCHLNYlb5riAHSFPICo4qw=", credential.CurrentDecryptedCertificatePassphrase);
				}
			}
		}

		OrgHeader GetDebtorForTests()
		{
			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.OH_IsDebtor = true;
			debtor.OH_Code = "SADEBTOR";
			debtor.MainAddress.OA_Address1 = "7232 KHALID BIN AL WALID STREET";
			debtor.MainAddress.OA_Code = "KHALID BIN WALID STREET";
			debtor.MainAddress.OA_Address2 = "AL SHARAFEYYAH";
			debtor.MainAddress.OA_City = "JEDDAH";
			debtor.MainAddress.OA_State = "MAKKAH";
			debtor.MainAddress.OA_PostCode = "23218";
			debtor.MainAddress.OA_RN_NKCountryCode = "SA";
			debtor.OH_RL_NKClosestPort = "SAABT";
			debtor.CompanyData.OB_ARVATConfig = OrganisationTaxConfiguartionTypes.Default.Code;

			var arAddress = debtor.Addresses.AddNew(OrgAddressType.Receivables, ZBool.True);
			arAddress.OA_Address1 = "1548 KING FAHD STREET";
			arAddress.OA_Code = "KING FAHD STREET";
			arAddress.OA_Address2 = "AL SHUMAISI DIST";
			arAddress.OA_City = "RIYADH";
			arAddress.OA_State = "RIYADH PROVINCE";
			arAddress.OA_PostCode = "12633";
			arAddress.OA_RN_NKCountryCode = "SA";
			return debtor;
		}
	}
}
