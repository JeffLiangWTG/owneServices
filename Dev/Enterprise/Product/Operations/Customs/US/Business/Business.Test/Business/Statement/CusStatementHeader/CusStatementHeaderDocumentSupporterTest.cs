using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusStatementHeaderDocumentSupporter))]
	sealed class CusStatementHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestShowReasonForNotPrinting()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "8804P04001";
			statement.B2_IsMonthlyStatement = true;
			AssertEquals(false, statement.DocumentSupporter.ShowReasonForNotPrinting(Core.Constants.DataContext.None, null));
		}

		public void TestBOProviderForDocumentSupport()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "8804P04001";
			statement.B2_IsMonthlyStatement = true;
			AssertEquals(typeof(CusStatementHeader), statement.DocumentSupporter.GetBODocDataProviders(new DataContextValue(".CusStatementHeader"), null)[0].ParentBusinessObject.GetType());
			var message = Factory.New<MQEDIMessage>();
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement;
			statement.Messages.Add(message);
			AssertEquals(typeof(MonthlyStatementMessageHeader), statement.DocumentSupporter.GetBODocDataProviders(new DataContextValue(".CusStatementHeader"), null)[0].ParentBusinessObject.GetType());
			statement.B2_StatementNumber = "8804104001";
			statement.B2_IsMonthlyStatement = false;
			statement.Messages.RemoveAndDeleteAll();
			message = Factory.New<MQEDIMessage>();
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.DailyStatement;
			statement.Messages.Add(message);
			AssertEquals(typeof(DailyStatementMessageHeader), statement.DocumentSupporter.GetBODocDataProviders(new DataContextValue(".CusStatementHeader"), null)[0].ParentBusinessObject.GetType());
		}

		public void TestCusStatementHeaderIsSupported()
		{
			CusStatementHeader statementHeader = Factory.New<CusStatementHeader>();

			CusStatementHeaderDocumentSupporter supporter = new CusStatementHeaderDocumentSupporter(statementHeader);
			AssertEquals(true, supporter.IsDataContextSupported(new DataContextValue(".CusStatementHeader")));

			IBODocDataProvider[] result = supporter.GetBODocDataProviders(new DataContextValue(".CusStatementHeader"), null);

			AssertEquals(1, result.Length);
			AssertEquals(statementHeader, BODocDataProvider.GetBusinessObject(result[0]));
		}

		public void TestGetContactOrganisation()
		{
			var importer = Factory.New<OrgHeader>();
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_OH_Importer = importer.PK;
			AssertEquals(importer.PK, statement.DocumentSupporter.GetContactOrganisation("", ContactType.Consignee, DocumentDirection.ANY).OrgHeader.PK);

			var newOrg = Factory.New<OrgHeader>();
			newOrg.OH_Code = "TESTORG";
			newOrg.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199000", "US");

			var message = Factory.New<MQEDIMessage>();
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement;
			message.EM_MessageText = "B011101X02MSFS000P00030051607691-013199000                                      ";
			statement.B2_IsMonthlyStatement = true;
			statement.Messages.Add(message);

			var matchedOrg = statement.DocumentSupporter.GetContactOrganisation("", ContactType.Consignee, DocumentDirection.ANY).OrgHeader;
			AssertEquals(importer.PK, matchedOrg.PK);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<CusStatementHeader>();
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			return documentCommand.SU_MenuName == "-";
		}
	}
}
