using System.Reflection;
using CargoWise.EntityFramework;
using static Enterprise.Customs.PL.Business.Constants;
using CusPollingTransaction = Enterprise.Customs.Business.CusPollingTransaction;
using CusPollingTransactionTypes = Enterprise.Core.Constants.Customs.CusPollingTransactionType.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

public static class GetDocumentsResponseDocumentUnpackingTestHelper
{
	public static MessageExchange CreateMessageExchangeForDocument(
		BusinessObjectFactory factory,
		string applicationCode,
		string resourceFilePath,
		string interchangeType)
	{
		var testFileBody = Assembly.GetExecutingAssembly().GetTestFile(resourceFilePath);
		return CreateMessageExchange(factory, testFileBody, applicationCode, interchangeType);
	}

	public static MessageExchange CreateMessageExchange(
		BusinessObjectFactory factory,
		string responseInterchangeContent,
		string applicationCode,
		string interchangeType)
	{
		var requestMessageBody = GetTransmitMessageBody();
		var cusPollingTransaction = CreateCusPollingTransactionWithStaffAndGlbExternalPassword(factory, CusPollingTransactionTypes.PLC);
		var (requestMessage, responseInterchange) = factory.PrepareRequestWithResponse(responseInterchangeContent, applicationCode, interchangeType, cusPollingTransaction, requestMessageBody);
		requestMessage.EM_MessageType = EdiMessageMessageType.CusPollingTransaction;
		return new (requestMessage, responseInterchange) { CusPollingTransaction = cusPollingTransaction };
	}

	public static CusPollingTransaction CreateCusPollingTransactionWithStaffAndGlbExternalPassword(BusinessObjectFactory factory, string transactionType)
	{
		var staff = factory.CreateStaffAndGlbExternalPasswordWithBranch(login: "asd", isActive: true, mailBox: "abc", password: "123");
		var cusPollingTransaction = factory.CreatePollingTransactionForStaff(staff, type: transactionType);

		return cusPollingTransaction;
	}

	public static string GetTransmitMessageBody(string dateFrom = "2024-01-25T10:20:00", string dateTo = "2024-01-25T10:20:00") => $"""
		<GetDocumentsRequest xmlns="http://www.mf.gov.pl/uslugiBiznesowe/WsPull/Usluga/2014/01_v2_0">
			<pobrany>0</pobrany>
			<dataOd>{dateFrom}</dataOd>
			<dataDo>{dateTo}</dataDo>
			<allEmployees>true</allEmployees>
		</GetDocumentsRequest>
		""";
}
