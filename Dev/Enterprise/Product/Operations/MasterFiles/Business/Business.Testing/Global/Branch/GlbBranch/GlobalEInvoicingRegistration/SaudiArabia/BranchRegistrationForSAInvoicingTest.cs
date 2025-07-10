using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CargoWise.Common.JSON.Extensions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.Testing;
using Moq;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.MasterFiles.Business.Accounting.GlobalEInvoicingRegistration.SaudiArabia.Testing
{
	public class BranchRegistrationForSAInvoicingTest : TestCaseWithFactory
	{
		public void TestConvertValidationWebResponseToResponseData()
		{
			var responseText = @"{
""validationResults"":
	{
	 ""infoMessages"":[{""type"":""INFO"",""code"":""XSD_ZATCA_VALID"",""category"":""XSD validation"",""message"":""Complied with UBL 2.1 standards in line with ZATCA specifications"",""status"":""PASS""}],
	 ""warningMessages"":[],
	 ""errorMessages"":[],
	 ""status"":""PASS""
	},
""reportingStatus"":null,
""clearanceStatus"":""CLEARED"",
""qrSellertStatus"":null,
""qrBuyertStatus"":null
}";
			register = GetInstance();
			var convertedObject = register.ConvertValidationWebResponseToResponseData_TestOnly(responseText);
			AssertNotNull(convertedObject);
			AssertEquals("CLEARED", convertedObject.ClearanceStatus);
			AssertNotNull(convertedObject.ValidationResults);
			AssertEquals(1, convertedObject.ValidationResults.InfoMessages.Count);
			AssertEquals("INFO", convertedObject.ValidationResults.InfoMessages[0].Type);
			AssertEquals("XSD_ZATCA_VALID", convertedObject.ValidationResults.InfoMessages[0].Code);
			AssertEquals("XSD validation", convertedObject.ValidationResults.InfoMessages[0].Category);
			AssertEquals("Complied with UBL 2.1 standards in line with ZATCA specifications", convertedObject.ValidationResults.InfoMessages[0].Message);
			AssertEquals("PASS", convertedObject.ValidationResults.InfoMessages[0].Status);
			AssertEquals(0, convertedObject.ValidationResults.ErrorMessages.Count);
			AssertEquals(0, convertedObject.ValidationResults.WarningMessages.Count);
		}

		public void TestSendTrialInvoiceAPI()
		{
			var register = GetInstance();
			var expectedContent = @"{""invoiceHash"":""KvbnuACY2zNGZwdextGpItayxHlLFFn6IIrGA1VvnNA="",
""uuid"":""8d487816-70b8-4ade-a618-9d620b73814a"",";
			AssertSendTrialTransactionAPI((r, t, s) => r.SendTrialInvoiceAPI_testOnly(t, s), expectedContent);
		}

		public void TestSendTrialCreditNoteAPI()
		{
			var register = GetInstance();
			var expectedContent = @"{""invoiceHash"":""gOhcV/UkzrPLlyo3mdxLaCYlDoWsOGUF82nDlJud9LE="",
""uuid"":""322efd74-5b1b-41e0-843c-1d13bec4475e"",";
			AssertSendTrialTransactionAPI((r, t, s) => r.SendTrialCreditNoteAPI_testOnly(t, s, "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ=="), expectedContent);
		}

		public void TestSendTrialDebitNoteAPI()
		{
			var register = GetInstance();
			var expectedContent = @"{""invoiceHash"":""V6JuFjcoh1b9HB15p5UpMF4K+TMcBHwn1WP8/jSapag="",
""uuid"":""ad2c3a8b-1cc8-4060-8a35-0d6aa8ff6fcc"",";
			AssertSendTrialTransactionAPI((r, t, s) => r.SendTrialDebitNoteAPI_testOnly(t, s, "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ=="), expectedContent);
		}

		void AssertSendTrialTransactionAPI(Func<BranchRegistrationForSAInvoicing_TestOnly, string, string, Task<(HttpStatusCode ResponseCode, string ResponseContent, string TransactionHash)>> apiCallMethod, string expectedRequestContent)
		{
			var response = new ValidationResponseData()
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
			clientHandlerMock.AddJsonResponse(string.Format("{0}/invoices", SAEInvoicingAPIEndPoints.Testing_CSID_URL), HttpStatusCode.OK, response.ToJSON());
			var register = GetInstance(handlerMock: clientHandlerMock);
			var task = Task.Run(() => apiCallMethod(register, "DummyTokenData", "DummySecretData"));
			task.Wait();

			var taskResult = task.Result;
			var responseCode = taskResult.ResponseCode;
			var responseContent = taskResult.ResponseContent;
			var responseStatus = (int)responseCode;

			AssertStartsWith("request body content", expectedRequestContent, clientHandlerMock.LastHttpRequestContent);

			AssertEquals(HttpStatusCode.OK, responseCode);
			AssertEquals(200, responseStatus);
			AssertEquals("{\"clearanceStatus\":\"CLEARED\",\"qrBuyertStatus\":null,\"qrSellertStatus\":null,\"reportingStatus\":null,\"validationResults\":{\"errorMessages\":null,\"infoMessages\":[{\"category\":\"XSD validation\",\"code\":\"XSD_ZATCA_VALID\",\"message\":\"Complied with UBL 2.1 standards in line with ZATCA specifications\",\"status\":\"PASS\",\"type\":\"INFO\"}],\"status\":\"PASS\",\"warningMessages\":null}}", responseContent);
		}

		public void TestConvertRegistrationWebResponseToGoodResponseData()
		{
			var responseText = "{\"requestID\":1234567890123,\"dispositionMessage\":\"ISSUED\",\"binarySecurityToken\":\"TUlJQjhUQ0NBWmFnQXdJQkFnSUdBWWhtTGxUNU1Bb0dDQ3FHU000OUJBTUNNQlV4RXpBUkJnTlZCQU1NQ21WSmJuWnZhV05wYm1jd0hoY05Nak13TlRJNU1EWXlORFEzV2hjTk1qZ3dOVEk0TWpFd01EQXdXakJQTVFzd0NRWURWUVFHRXdKVFFURVhNQlVHQTFVRUN3d09ZVzF0WVc0Z1FuSmhibU5vWTJneEV6QVJCZ05WQkFvTUNtaGhlV0VnZVdGbklETXhFakFRQmdOVkJBTU1DVEV5Tnk0d0xqQXVNVEJXTUJBR0J5cUdTTTQ5QWdFR0JTdUJCQUFLQTBJQUJOdUt0aWYvSy84NndlRVdVdys4VnhIRWplZTI2VFdMQzJLVTFpNVhiNzNtU2NNQ3lGdms0V3doZ0llaitRdlRRcS9FdXpqNno2dldEeStwU0NtaVR4ZWpnWm93Z1pjd0RBWURWUjBUQVFIL0JBSXdBRENCaGdZRFZSMFJCSDh3ZmFSN01Ia3hHekFaQmdOVkJBUU1FakV0YUdGNVlYd3lMVEl6Tkh3ekxUTTFOREVmTUIwR0NnbVNKb21UOGl4a0FRRU1Eek14TURFM05UTTVOelF3TURBd016RU5NQXNHQTFVRURBd0VNVEV3TURFUU1BNEdBMVVFR2d3SFdtRjBZMkVnTXpFWU1CWUdBMVVFRHd3UFJtOXZaQ0JDZFhOemFXNWxjM016TUFvR0NDcUdTTTQ5QkFNQ0Ewa0FNRVlDSVFEOWZkN0swZXJKdXZid05hRnJLRUVZVTc4VHpidTBYSzhldjFISGZsemF2d0loQU0zSDhzV1ppOUdWK3NWVHFOaUpBZU5BL3JsK2dlY2p5SGhidW9YckIwS3g=\",\"secret\":\"WvUCAV3NKx9ecpzYmg3HWvRbT6Mq0hikQClvJW04kYE=\",\"errors\":null}";
			var register = GetInstance();
			var convertedObject = register.ConvertRegistrationWebResponseToGoodResponseData_TestOnly(responseText);
			AssertNotNull(convertedObject);
			AssertEquals("1234567890123", convertedObject.RequestID);
			AssertEquals("ISSUED", convertedObject.DispositionMessage);
			AssertEquals("TUlJQjhUQ0NBWmFnQXdJQkFnSUdBWWhtTGxUNU1Bb0dDQ3FHU000OUJBTUNNQlV4RXpBUkJnTlZCQU1NQ21WSmJuWnZhV05wYm1jd0hoY05Nak13TlRJNU1EWXlORFEzV2hjTk1qZ3dOVEk0TWpFd01EQXdXakJQTVFzd0NRWURWUVFHRXdKVFFURVhNQlVHQTFVRUN3d09ZVzF0WVc0Z1FuSmhibU5vWTJneEV6QVJCZ05WQkFvTUNtaGhlV0VnZVdGbklETXhFakFRQmdOVkJBTU1DVEV5Tnk0d0xqQXVNVEJXTUJBR0J5cUdTTTQ5QWdFR0JTdUJCQUFLQTBJQUJOdUt0aWYvSy84NndlRVdVdys4VnhIRWplZTI2VFdMQzJLVTFpNVhiNzNtU2NNQ3lGdms0V3doZ0llaitRdlRRcS9FdXpqNno2dldEeStwU0NtaVR4ZWpnWm93Z1pjd0RBWURWUjBUQVFIL0JBSXdBRENCaGdZRFZSMFJCSDh3ZmFSN01Ia3hHekFaQmdOVkJBUU1FakV0YUdGNVlYd3lMVEl6Tkh3ekxUTTFOREVmTUIwR0NnbVNKb21UOGl4a0FRRU1Eek14TURFM05UTTVOelF3TURBd016RU5NQXNHQTFVRURBd0VNVEV3TURFUU1BNEdBMVVFR2d3SFdtRjBZMkVnTXpFWU1CWUdBMVVFRHd3UFJtOXZaQ0JDZFhOemFXNWxjM016TUFvR0NDcUdTTTQ5QkFNQ0Ewa0FNRVlDSVFEOWZkN0swZXJKdXZid05hRnJLRUVZVTc4VHpidTBYSzhldjFISGZsemF2d0loQU0zSDhzV1ppOUdWK3NWVHFOaUpBZU5BL3JsK2dlY2p5SGhidW9YckIwS3g=", convertedObject.BinarySecurityToken);
			AssertEquals("WvUCAV3NKx9ecpzYmg3HWvRbT6Mq0hikQClvJW04kYE=", convertedObject.Secret);
			AssertEquals(null, convertedObject.Errors);
		}

		public void TestConvertRegistrationWebResponseToBadResponseData()
		{
			var responseText = "{\"code\":\"Invalid-OTP\",\"message\":\"The provided OTP is invalid\"}";
			var register = GetInstance();
			var convertedObject = register.ConvertRegistrationWebResponseToBadResponseData_TestOnly(responseText);
			AssertNotNull(convertedObject);
			AssertEquals("Invalid-OTP", convertedObject.Code);
			AssertEquals("The provided OTP is invalid", convertedObject.Message);
		}

		public void TestConvertRegistrationWebResponseToBadResponseDataList()
		{
			var responseText = "{\"errors\":[{\"code\":\"Invalid-OTP\",\"message\":\"The provided OTP is invalid\"}]}";
			var register = GetInstance();
			var convertedObject = register.ConvertRegistrationWebResponseToBadResponseDataList_TestOnly(responseText);
			AssertNotNull(convertedObject);
			AssertEquals(1, convertedObject.Errors.Count);
			AssertEquals("Invalid-OTP", convertedObject.Errors[0].Code);
			AssertEquals("The provided OTP is invalid", convertedObject.Errors[0].Message);
		}

		public void TestSendRegistrationComplianceAPI()
		{
			var response = new RegistrationGoodResponseData()
			{
				RequestID = "123456",
				DispositionMessage = "ISSUED",
				BinarySecurityToken = "TUlJQjhUQ0NBWmFnQXdJQkFnSUdBWWhtTGxUNU1Bb0dDQ3FHU000OUJBTUNNQlV4RXpBUkJnTlZCQU1NQ21WSmJuWnZhV05wYm1jd0hoY05Nak13TlRJNU1EWXlORFEzV2hjTk1qZ3dOVEk0TWpFd01EQXdXakJQTVFzd0NRWURWUVFHRXdKVFFURVhNQlVHQTFVRUN3d09ZVzF0WVc0Z1FuSmhibU5vWTJneEV6QVJCZ05WQkFvTUNtaGhlV0VnZVdGbklETXhFakFRQmdOVkJBTU1DVEV5Tnk0d0xqQXVNVEJXTUJBR0J5cUdTTTQ5QWdFR0JTdUJCQUFLQTBJQUJOdUt0aWYvSy84NndlRVdVdys4VnhIRWplZTI2VFdMQzJLVTFpNVhiNzNtU2NNQ3lGdms0V3doZ0llaitRdlRRcS9FdXpqNno2dldEeStwU0NtaVR4ZWpnWm93Z1pjd0RBWURWUjBUQVFIL0JBSXdBRENCaGdZRFZSMFJCSDh3ZmFSN01Ia3hHekFaQmdOVkJBUU1FakV0YUdGNVlYd3lMVEl6Tkh3ekxUTTFOREVmTUIwR0NnbVNKb21UOGl4a0FRRU1Eek14TURFM05UTTVOelF3TURBd016RU5NQXNHQTFVRURBd0VNVEV3TURFUU1BNEdBMVVFR2d3SFdtRjBZMkVnTXpFWU1CWUdBMVVFRHd3UFJtOXZaQ0JDZFhOemFXNWxjM016TUFvR0NDcUdTTTQ5QkFNQ0Ewa0FNRVlDSVFEOWZkN0swZXJKdXZid05hRnJLRUVZVTc4VHpidTBYSzhldjFISGZsemF2d0loQU0zSDhzV1ppOUdWK3NWVHFOaUpBZU5BL3JsK2dlY2p5SGhidW9YckIwS3g=",
				Secret = "WvUCAV3NKx9ecpzYmg3HWvRbT6Mq0hikQClvJW04kYE=",
				Errors = null
			};

			var clientHandlerMock = EInvoicingBranchRegisterHttpClientHandlerMock.New();
			clientHandlerMock.AddJsonResponse(SAEInvoicingAPIEndPoints.Testing_CSID_URL, HttpStatusCode.OK, response.ToJSON());
			var register = GetInstance(handlerMock: clientHandlerMock);
			var task = Task.Run(() => register.SendRegistrationComplianceAPI_TestOnly("123456", "dummyCSRData"));
			task.Wait();

			var taskResult = task.Result;
			var responseCode = taskResult.ResponseCode;
			var responseContent = taskResult.ResponseContent;
			var responseStatus = (int)responseCode;

			AssertEquals(HttpStatusCode.OK, responseCode);
			AssertEquals(200, responseStatus);
			AssertEquals("{\"binarySecurityToken\":\"TUlJQjhUQ0NBWmFnQXdJQkFnSUdBWWhtTGxUNU1Bb0dDQ3FHU000OUJBTUNNQlV4RXpBUkJnTlZCQU1NQ21WSmJuWnZhV05wYm1jd0hoY05Nak13TlRJNU1EWXlORFEzV2hjTk1qZ3dOVEk0TWpFd01EQXdXakJQTVFzd0NRWURWUVFHRXdKVFFURVhNQlVHQTFVRUN3d09ZVzF0WVc0Z1FuSmhibU5vWTJneEV6QVJCZ05WQkFvTUNtaGhlV0VnZVdGbklETXhFakFRQmdOVkJBTU1DVEV5Tnk0d0xqQXVNVEJXTUJBR0J5cUdTTTQ5QWdFR0JTdUJCQUFLQTBJQUJOdUt0aWYvSy84NndlRVdVdys4VnhIRWplZTI2VFdMQzJLVTFpNVhiNzNtU2NNQ3lGdms0V3doZ0llaitRdlRRcS9FdXpqNno2dldEeStwU0NtaVR4ZWpnWm93Z1pjd0RBWURWUjBUQVFIL0JBSXdBRENCaGdZRFZSMFJCSDh3ZmFSN01Ia3hHekFaQmdOVkJBUU1FakV0YUdGNVlYd3lMVEl6Tkh3ekxUTTFOREVmTUIwR0NnbVNKb21UOGl4a0FRRU1Eek14TURFM05UTTVOelF3TURBd016RU5NQXNHQTFVRURBd0VNVEV3TURFUU1BNEdBMVVFR2d3SFdtRjBZMkVnTXpFWU1CWUdBMVVFRHd3UFJtOXZaQ0JDZFhOemFXNWxjM016TUFvR0NDcUdTTTQ5QkFNQ0Ewa0FNRVlDSVFEOWZkN0swZXJKdXZid05hRnJLRUVZVTc4VHpidTBYSzhldjFISGZsemF2d0loQU0zSDhzV1ppOUdWK3NWVHFOaUpBZU5BL3JsK2dlY2p5SGhidW9YckIwS3g=\",\"dispositionMessage\":\"ISSUED\",\"errors\":null,\"requestID\":\"123456\",\"secret\":\"WvUCAV3NKx9ecpzYmg3HWvRbT6Mq0hikQClvJW04kYE=\",\"tokenType\":null}", responseContent);
		}

		public void TestSendRegistrationOnboardingAPI()
		{
			var response = new RegistrationGoodResponseData()
			{
				RequestID = "45678",
				TokenType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3",
				DispositionMessage = "ISSUED",
				BinarySecurityToken = "TUlJRDJ6Q0NBNENnQXdJQkFnSVRid0FBZHFEbUlocXNqcG01Q3dBQkFBQjJvREFLQmdncWhrak9QUVFEQWpCak1SVXdFd1lLQ1pJbWlaUHlMR1FCR1JZRmJHOWpZV3d4RXpBUkJnb0praWFKay9Jc1pBRVpGZ05uYjNZeEZ6QVZCZ29Ka2lhSmsvSXNaQUVaRmdkbGVIUm5ZWHAwTVJ3d0dnWURWUVFERXhOVVUxcEZTVTVXVDBsRFJTMVRkV0pEUVMweE1CNFhEVEl5TURNeU9ERTFORFl6TWxvWERUSXlNRE16TURFMU5EWXpNbG93VFRFTE1Ba0dBMVVFQmhNQ1UwRXhEakFNQmdOVkJBb1RCVXBoY21seU1Sb3dHQVlEVlFRTEV4RktaV1JrWVdnZ1FuSmhibU5vTVRJek5ERVNNQkFHQTFVRUF4TUpNVEkzTGpBdU1DNHhNRll3RUFZSEtvWkl6ajBDQVFZRks0RUVBQW9EUWdBRUQvd2IybGhCdkJJQzhDbm5adm91bzZPelJ5bXltVTlOV1JoSXlhTWhHUkVCQ0VaQjRFQVZyQnVWMnhYaXhZNHFCWWY5ZGRlcnprVzlEd2RvM0lsSGdxT0NBaW93Z2dJbU1JR0xCZ05WSFJFRWdZTXdnWUNrZmpCOE1Sd3dHZ1lEVlFRRURCTXlNakl5TWpNeU5EUTBNelF6YW1abU5ETXlNUjh3SFFZS0NaSW1pWlB5TEdRQkFRd1BNekV3TVRjMU16azNOREF3TURBek1RMHdDd1lEVlFRTURBUXhNREV4TVJFd0R3WURWUVFhREFoVFlXMXdiR1VnUlRFWk1CY0dBMVVFRHd3UVUyRnRjR3hsSUVKMWMzTnBibVZ6Y3pBZEJnTlZIUTRFRmdRVWhXY3NiYkpoakQ1WldPa3dCSUxDK3dOVmZLWXdId1lEVlIwakJCZ3dGb0FVZG1DTSt3YWdyR2RYTlozUG1xeW5LNWsxdFM4d1RnWURWUjBmQkVjd1JUQkRvRUdnUDRZOWFIUjBjRG92TDNSemRHTnliQzU2WVhSallTNW5iM1l1YzJFdlEyVnlkRVZ1Y205c2JDOVVVMXBGU1U1V1QwbERSUzFUZFdKRFFTMHhMbU55YkRDQnJRWUlLd1lCQlFVSEFRRUVnYUF3Z1owd2JnWUlLd1lCQlFVSE1BR0dZbWgwZEhBNkx5OTBjM1JqY213dWVtRjBZMkV1WjI5MkxuTmhMME5sY25SRmJuSnZiR3d2VkZOYVJXbHVkbTlwWTJWVFEwRXhMbVY0ZEdkaGVuUXVaMjkyTG14dlkyRnNYMVJUV2tWSlRsWlBTVU5GTFZOMVlrTkJMVEVvTVNrdVkzSjBNQ3NHQ0NzR0FRVUZCekFCaGg5b2RIUndPaTh2ZEhOMFkzSnNMbnBoZEdOaExtZHZkaTV6WVM5dlkzTndNQTRHQTFVZER3RUIvd1FFQXdJSGdEQWRCZ05WSFNVRUZqQVVCZ2dyQmdFRkJRY0RBZ1lJS3dZQkJRVUhBd013SndZSkt3WUJCQUdDTnhVS0JCb3dHREFLQmdnckJnRUZCUWNEQWpBS0JnZ3JCZ0VGQlFjREF6QUtCZ2dxaGtqT1BRUURBZ05KQURCR0FpRUF5Tmh5Y1EzYk5sTEZkT1BscVlUNlJWUVRXZ25LMUdoME5IZGNTWTRQZkMwQ0lRQ1NBdGhYdnY3dGV0VUw2OVdqcDhCeG5MTE13ZXJ4WmhCbmV3by9nRjNFSkE9PQ==",
				Secret = "f9YRhopN/G7x0TECOY6nKSCHLNYlb5riAHSFPICo4qw=",
			};

			var clientHandlerMock = EInvoicingBranchRegisterHttpClientHandlerMock.New();
			clientHandlerMock.AddJsonResponse(SAEInvoicingAPIEndPoints.Testing_Onboarding_URL, HttpStatusCode.OK, response.ToJSON());
			var register = GetInstance(handlerMock: clientHandlerMock);
			var task = Task.Run(() => register.SendRegistrationOnboardingAPI_TestOnly("DummyTokenData", "DummySecretData", "123456"));
			task.Wait();

			var taskResult = task.Result;
			var responseCode = taskResult.ResponseCode;
			var responseContent = taskResult.ResponseContent;
			var responseStatus = (int)responseCode;

			AssertEquals(HttpStatusCode.OK, responseCode);
			AssertEquals(200, responseStatus);
			AssertEquals("{\"binarySecurityToken\":\"TUlJRDJ6Q0NBNENnQXdJQkFnSVRid0FBZHFEbUlocXNqcG01Q3dBQkFBQjJvREFLQmdncWhrak9QUVFEQWpCak1SVXdFd1lLQ1pJbWlaUHlMR1FCR1JZRmJHOWpZV3d4RXpBUkJnb0praWFKay9Jc1pBRVpGZ05uYjNZeEZ6QVZCZ29Ka2lhSmsvSXNaQUVaRmdkbGVIUm5ZWHAwTVJ3d0dnWURWUVFERXhOVVUxcEZTVTVXVDBsRFJTMVRkV0pEUVMweE1CNFhEVEl5TURNeU9ERTFORFl6TWxvWERUSXlNRE16TURFMU5EWXpNbG93VFRFTE1Ba0dBMVVFQmhNQ1UwRXhEakFNQmdOVkJBb1RCVXBoY21seU1Sb3dHQVlEVlFRTEV4RktaV1JrWVdnZ1FuSmhibU5vTVRJek5ERVNNQkFHQTFVRUF4TUpNVEkzTGpBdU1DNHhNRll3RUFZSEtvWkl6ajBDQVFZRks0RUVBQW9EUWdBRUQvd2IybGhCdkJJQzhDbm5adm91bzZPelJ5bXltVTlOV1JoSXlhTWhHUkVCQ0VaQjRFQVZyQnVWMnhYaXhZNHFCWWY5ZGRlcnprVzlEd2RvM0lsSGdxT0NBaW93Z2dJbU1JR0xCZ05WSFJFRWdZTXdnWUNrZmpCOE1Sd3dHZ1lEVlFRRURCTXlNakl5TWpNeU5EUTBNelF6YW1abU5ETXlNUjh3SFFZS0NaSW1pWlB5TEdRQkFRd1BNekV3TVRjMU16azNOREF3TURBek1RMHdDd1lEVlFRTURBUXhNREV4TVJFd0R3WURWUVFhREFoVFlXMXdiR1VnUlRFWk1CY0dBMVVFRHd3UVUyRnRjR3hsSUVKMWMzTnBibVZ6Y3pBZEJnTlZIUTRFRmdRVWhXY3NiYkpoakQ1WldPa3dCSUxDK3dOVmZLWXdId1lEVlIwakJCZ3dGb0FVZG1DTSt3YWdyR2RYTlozUG1xeW5LNWsxdFM4d1RnWURWUjBmQkVjd1JUQkRvRUdnUDRZOWFIUjBjRG92TDNSemRHTnliQzU2WVhSallTNW5iM1l1YzJFdlEyVnlkRVZ1Y205c2JDOVVVMXBGU1U1V1QwbERSUzFUZFdKRFFTMHhMbU55YkRDQnJRWUlLd1lCQlFVSEFRRUVnYUF3Z1owd2JnWUlLd1lCQlFVSE1BR0dZbWgwZEhBNkx5OTBjM1JqY213dWVtRjBZMkV1WjI5MkxuTmhMME5sY25SRmJuSnZiR3d2VkZOYVJXbHVkbTlwWTJWVFEwRXhMbVY0ZEdkaGVuUXVaMjkyTG14dlkyRnNYMVJUV2tWSlRsWlBTVU5GTFZOMVlrTkJMVEVvTVNrdVkzSjBNQ3NHQ0NzR0FRVUZCekFCaGg5b2RIUndPaTh2ZEhOMFkzSnNMbnBoZEdOaExtZHZkaTV6WVM5dlkzTndNQTRHQTFVZER3RUIvd1FFQXdJSGdEQWRCZ05WSFNVRUZqQVVCZ2dyQmdFRkJRY0RBZ1lJS3dZQkJRVUhBd013SndZSkt3WUJCQUdDTnhVS0JCb3dHREFLQmdnckJnRUZCUWNEQWpBS0JnZ3JCZ0VGQlFjREF6QUtCZ2dxaGtqT1BRUURBZ05KQURCR0FpRUF5Tmh5Y1EzYk5sTEZkT1BscVlUNlJWUVRXZ25LMUdoME5IZGNTWTRQZkMwQ0lRQ1NBdGhYdnY3dGV0VUw2OVdqcDhCeG5MTE13ZXJ4WmhCbmV3by9nRjNFSkE9PQ==\",\"dispositionMessage\":\"ISSUED\",\"errors\":null,\"requestID\":\"45678\",\"secret\":\"f9YRhopN\\/G7x0TECOY6nKSCHLNYlb5riAHSFPICo4qw=\",\"tokenType\":\"http:\\/\\/docs.oasis-open.org\\/wss\\/2004\\/01\\/oasis-200401-wss-x509-token-profile-1.0#X509v3\"}", responseContent);
		}

		BranchRegistrationForSAInvoicing_TestOnly GetInstance(GlbBranch anotherBranch = null, OrgHeader anotherDebtor = null, EInvoicingBranchRegisterHttpClientHandlerMock handlerMock = null)
		{
			var clientHandlerMock = handlerMock ?? EInvoicingBranchRegisterHttpClientHandlerMock.New();
			var loggerMock = new Mock<ILogger>();
			return new BranchRegistrationForSAInvoicing_TestOnly(anotherBranch ?? branch, anotherDebtor ?? debtor, GetCSRGenerator(), new HttpCleintProviderForTest(clientHandlerMock), loggerMock.Object);
		}

		ICSRGenerator GetCSRGenerator()
		{
			(string, string) csrString = (Base64EncoderDecoder.Decode("LS0tLS1CRUdJTiBFQyBQUklWQVRFIEtFWS0tLS0tCk1JR05BZ0VBTUJBR0J5cUdTTTQ5QWdFR0JTdUJCQUFLQkhZd2RBSUJBUVFnZzJrY1VlWjlTaUc5Y3N6VDRvYTEKN0I5QWc2M2p4N1ZRTGRoeHlMRng5MGVnQndZRks0RUVBQXFoUkFOQ0FBU25jR3hJbzVkTG43UWs4V0Izd1pIRApqLzlnODIrNmxDNCthWURkeVJ1Ym9yZUg1UXh4T2Y2V1QrK1JROHdKZXNWaDIyNEZIaDNBRVh1b3F6aXZ2TVVECi0tLS0tRU5EIEVDIFBSSVZBVEUgS0VZLS0tLS0="),
					Base64EncoderDecoder.Decode("LS0tLS1CRUdJTiBDRVJUSUZJQ0FURSBSRVFVRVNULS0tLS0KTUlJQ1F6Q0NBZWdDQVFBd2FURUxNQWtHQTFVRUJoTUNVMEV4SnpBbEJnTlZCQW9NSGxOQlZTQkRUMDFRUVU1WgpJRTlTUjBGT1NWTkJWRWxQVGlCUVVrOVlXVEVUTUJFR0ExVUVDd3dLTmpZMk56YzNPRGc0T1RFY01Cb0dBMVVFCkF3d1RRMkZ5WjI5M2FYTmxJRVJUUVVOTVNWTkJNakJXTUJBR0J5cUdTTTQ5QWdFR0JTdUJCQUFLQTBJQUJLZHcKYkVpamwwdWZ0Q1R4WUhmQmtjT1AvMkR6YjdxVUxqNXBnTjNKRzV1aXQ0ZmxESEU1L3BaUDc1RkR6QWw2eFdIYgpiZ1VlSGNBUmU2aXJPSys4eFFPZ2dnRWVNSUlCR2dZSktvWklodmNOQVFrT01ZSUJDekNDQVFjd0pBWUpLd1lCCkJBR0NOeFFDQkJjVEZWQlNSVnBCVkVOQkxVTnZaR1V0VTJsbmJtbHVaekNCM2dZRFZSMFJCSUhXTUlIVHBJSFEKTUlITk1UWXdOQVlEVlFRRURDMHhMVmRwYzJWMFpXTm9JRWRzYjJKaGJId3lMVU5oY21kdmQybHpaWHd6TFZkVQpURVJUUVVOTVNTMVRRVEl4SHpBZEJnb0praWFKay9Jc1pBRUJEQTh6TXpFeE5qSTROamswTVRBd01ETXhEVEFMCkJnTlZCQXdNQkRFd01EQXhSakJFQmdOVkJCb01QVXhGVmtWTUlERWdNMEVnTHlBM01pQlBKMUpKVDFKRVFVNGcKVTFSU1JVVlVJRUZNUlZoQlRrUlNTVUVnTURJZ01qQXhOU0JCVlZOVVVrRk1TVUV4R3pBWkJnTlZCQThNRWt4dgpaMmx6ZEdsamN5QlRaWEoyYVdObGN6QUtCZ2dxaGtqT1BRUURBZ05KQURCR0FpRUFtbml4N2F6Snp5SjZ4Z0ZpCjZDeFhmZGVncmF5aTUwNnovdHNXMW9Bb0hyY0NJUUR5L0xhYmdBOUlNQkFmdkNwNjdIU0J6ajBGSHM1NnQ2VkYKK0FxRzlPZ1RTdz09Ci0tLS0tRU5EIENFUlRJRklDQVRFIFJFUVVFU1QtLS0tLQ=="));
			var csrGeneratorMock = new Mock<ICSRGenerator>();
			csrGeneratorMock.Setup(g => g.Generate()).Returns(csrString);
			return csrGeneratorMock.Object;
		}

		BranchRegistrationForSAInvoicing_TestOnly register;
		GlbBranch branch;
		OrgHeader debtor;

		protected override void SetUp()
		{
			base.SetUp();
			branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "GBX";
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.OH_FullName = "My Test Proxy Org";
			orgProxy.MainAddress.OA_Address1 = "Address Line 1";
			orgProxy.MainAddress.OA_Address2 = "Address Line 2";
			orgProxy.MainAddress.OA_City = "Test City";
			orgProxy.MainAddress.OA_State = "02";
			orgProxy.MainAddress.OA_RN_NKCountryCode = "SA";
			orgProxy.OH_RL_NKClosestPort = "SAABT";
			orgProxy.PrimaryRegistrationNumber.NumberTypeForDisplay = "SA:VAT";
			orgProxy.PrimaryRegistrationNumber.Number = "123 123 123 12";
			branch.GB_OH_OrgProxy = orgProxy.PK;

			debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.OH_FullName = "SA Test Debtor";
			debtor.MainAddress.OA_Address1 = "7232 KHALID BIN AL WALID STREET";
			debtor.MainAddress.OA_Address2 = "AL SHARAFEYYAH";
			debtor.MainAddress.OA_City = "JEDDAH";
			debtor.MainAddress.OA_State = "MAKKAH";
			debtor.MainAddress.OA_RN_NKCountryCode = "SA";
			debtor.MainAddress.OA_PostCode = "23218";
			debtor.PrimaryRegistrationNumber.NumberTypeForDisplay = "SA:VAT";
			debtor.PrimaryRegistrationNumber.Number = "456 456 456 45";

			Factory.Save();
		}

		public class BranchRegistrationForSAInvoicing_TestOnly : BranchRegistrationForSAInvoicing
		{
			public BranchRegistrationForSAInvoicing_TestOnly(GlbBranch branch, OrgHeader debtor, ICSRGenerator generator, IHttpClientProvider httpClientProvider, ILogger logger)
				: base(branch, debtor, generator, httpClientProvider, logger)
			{
			}

			public async Task<(HttpStatusCode ResponseCode, string ResponseContent)> SendRegistrationOnboardingAPI_TestOnly(string token, string secret, string requestId) => await SendRegistrationOnboardingAPI(token, secret, requestId);

			public async Task<(HttpStatusCode ResponseCode, string ResponseContent)> SendRegistrationComplianceAPI_TestOnly(string otpCode, string csrData) => await SendRegistrationComplianceAPI(otpCode, csrData);

			public async Task<(HttpStatusCode ResponseCode, string ResponseContent, string TransactionHash)> SendTrialInvoiceAPI_testOnly(string token, string secret) => await SendTrialInvoiceAPI(token, secret);

			public async Task<(HttpStatusCode ResponseCode, string ResponseContent, string TransactionHash)> SendTrialCreditNoteAPI_testOnly(string token, string secret, string previousTransactionHash) => await SendTrialCreditNoteAPI(token, secret, previousTransactionHash);

			public async Task<(HttpStatusCode ResponseCode, string ResponseContent, string TransactionHash)> SendTrialDebitNoteAPI_testOnly(string token, string secret, string previousTransactionHash) => await SendTrialDebitNoteAPI(token, secret, previousTransactionHash);

			public ValidationResponseData ConvertValidationWebResponseToResponseData_TestOnly(string responseText) => ConvertValidationWebResponseToResponseData(responseText);

			public RegistrationGoodResponseData ConvertRegistrationWebResponseToGoodResponseData_TestOnly(string responseText) => ConvertRegistrationWebResponseToGoodResponseData(responseText);

			public RegistrationBadResponseData ConvertRegistrationWebResponseToBadResponseData_TestOnly(string responseText) => ConvertRegistrationWebResponseToBadResponseData(responseText);

			public RegistrationBadResponseDataList ConvertRegistrationWebResponseToBadResponseDataList_TestOnly(string responseText) => ConvertRegistrationWebResponseToBadResponseDataList(responseText);
		}
	}
}
