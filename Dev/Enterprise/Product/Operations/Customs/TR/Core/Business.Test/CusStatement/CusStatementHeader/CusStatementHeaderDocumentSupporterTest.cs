using CargoWise.Application;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(CusStatementHeaderDocumentSupporter))]
	public class CusStatementHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestStampDutyLedgerDocumentWrapperIsSupported()
		{
			var cusStatementHeader = Factory.New<CusStatementHeader>();
			var supporter = new CusStatementHeaderDocumentSupporter(cusStatementHeader);
			var wrappers = supporter.GetBODocDataProviders(new DataContextValue(".CusStatementHeader"), null);
			AssertEquals(1, wrappers.Length);
			var wrapperType = wrappers[0].ParentBusinessObject.GetType();
			AssertEquals(typeof(StampDutyLedgerDocumentWrapper), wrapperType);
		}

		public void TestShowShowReasonForNotPrinting()
		{
			var cusStatementHeader = Factory.New<CusStatementHeader>();
			var docSupporter = ((IDocumentSupportable)cusStatementHeader).DocumentSupporter;
			AssertEquals("ShowReasonForNotPrinting", false, docSupporter.ShowReasonForNotPrinting(Core.Constants.DataContext.None, null));
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			var cusStatementHeader = Factory.New<CusStatementHeader>();
			var docSupporter = ((IDocumentSupportable)cusStatementHeader).DocumentSupporter;
			AssertEquals(Env.Security.CustomsDeclarationCustomiseDocument, docSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestSupportedDataContext()
		{
			var cusStatementHeader = Factory.New<CusStatementHeader>();
			var docSupporter = ((IDocumentSupportable)cusStatementHeader).DocumentSupporter;
			AssertEquals(true, docSupporter.IsDataContextSupported(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.Statement)));
		}

		public void TestBusinessContext()
		{
			var cusStatementHeader = Factory.New<CusStatementHeader>();
			var docSupporter = ((IDocumentSupportable)cusStatementHeader).DocumentSupporter;
			AssertEquals(CargoWise.Definitions.BusinessContext.Statement, docSupporter.BusinessContext);
		}

		public override void TestRunningDocumentsShouldNotCauseException()
		{
			//We have no Report Of Miscellaneous Charges Documents yet
			Assert(true);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<CusStatementHeader>();
		}

		public void TestGetDataStateBeforeRun()
		{
			var dummyConfigurator = new DummyCusStatmentHeaderDocumentSupporterConfigurator();
			ObjectFactory.Substitute<ITRCusStatmentHeaderDocumentSupporterConfigurator>(dummyConfigurator);

			var cusStatementHeader = Factory.New<CusStatementHeader>();
			var documentSupporter = ((IDocumentSupportable)cusStatementHeader).DocumentSupporter;
			var menuItemName = Factory.New<IStmMenuItem>();
			menuItemName.SU_MenuName = "Print Stamp Duty Ledger";
			var documentSupporterDataState = documentSupporter.GetDataStateBeforeRun(menuItemName);
			AssertEquals("IsValid", true, documentSupporterDataState.IsValid);
		}
	}

	class DummyCusStatmentHeaderDocumentSupporterConfigurator : ITRCusStatmentHeaderDocumentSupporterConfigurator
	{
		public DocumentSupporterDataState PrintStampDutyLedgerConfig(CusStatementHeader statementHeader, DocumentSupporterDataState dataState)
		{
			return new DocumentSupporterDataState(true, "");
		}
	}
}
