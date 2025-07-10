using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module.Testing
{
	sealed class ActionsNotificationsHelperTest : TestCaseWithFactory
	{
		public void TestCheckIfValidToSendAuthorizationOrPayment()
		{
			Env.Security.USCustomsImportStatementModify.IsAllowed = false;
			Env.Security.USCustomsImportStatementView.IsAllowed = true;
			Env.Security.USCustomsImportStatementSendAuthMsgImporter.IsAllowed = true;
			Header.B2_StatementNumber = "0914725836";
			var link = new LogControllerLink(Header.B2_StatementNumber, ControllerIDs.Customs.CustomsStatement, Header.PK);
			Header.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			Header.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			Env.Security.USCustomsImportStatementSendAuthMsgBroker.IsAllowed = false;
			var helper = new ActionsNotificationsHelper(link);
			Assert(helper.CanSendMessage);
			AssertEquals(ZString.Empty, helper.NotificationsAsString);
			helper.CheckIfValidToSendAuthorizationOrPayment(Header);
			Assert(!helper.CanSendMessage);
			Assert(helper.NotificationsAsString.Contains("Send Authorization Message (Broker statement)"));
			Env.Security.USCustomsImportStatementSendAuthMsgImporter.IsAllowed = false;
			Header.B2_PaymentParty = ZString.Empty;
			Header.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			helper = new ActionsNotificationsHelper(link);
			helper.CheckIfValidToSendAuthorizationOrPayment(Header);
			Assert(!helper.CanSendMessage);
			Assert(helper.NotificationsAsString.Contains("Send Authorization Message (Importer statement)"));
			Env.Security.USCustomsImportStatementSendAuthMsgImporter.IsAllowed = true;
			USCustomsDataRegistry.Instance.RequireApprovalPriorAuthorizingStatement.SetValue(Header.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			helper = new ActionsNotificationsHelper(link);
			helper.CheckIfValidToSendAuthorizationOrPayment(Header);
			Assert(!helper.CanSendMessage);
			Assert(helper.NotificationsAsString.Contains("Permission to 'Send Payment Authorization' needs to be granted"));
			USCustomsDataRegistry.Instance.RequireApprovalPriorAuthorizingStatement.SetValue(Header.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			helper = new ActionsNotificationsHelper(link);
			helper.CheckIfValidToSendAuthorizationOrPayment(Header);
			Assert(helper.CanSendMessage);
			AssertEquals(ZString.Empty, helper.NotificationsAsString);
		}

		CusStatementHeader header;
		CusStatementHeader Header => header ?? (header = Factory.New<CusStatementHeader>());
	}
}
