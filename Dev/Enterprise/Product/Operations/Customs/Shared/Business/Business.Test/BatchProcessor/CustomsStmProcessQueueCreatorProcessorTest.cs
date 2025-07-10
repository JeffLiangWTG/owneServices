using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.Business.BatchProcessor.Testing
{
	sealed class CustomsStmProcessQueueCreatorProcessorTest : TestCaseWithFactory
	{
		public void TestCreateStmProcessQueue()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B00001001";

			var logger = new LoggingInformation();
			IProcessor processor = new CustomsStmProcessQueueCreatorProcessor(declaration, "TST", "ABC");
			processor.Process(logger);
			Factory.Save();

			var logs = new ZStringBuilder();
			var enumerator = logger.UserLogStrings.GetEnumerator();
			while (enumerator.MoveNext())
			{
				logs.Append(enumerator.Current.Trim());
			}
			AssertContains("A record has been generated in StmProcessQueue table for Declaration B00001001, PK=", logs.ToString());
		}

		public void TestConstructor()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			var result = new CustomsStmProcessQueueCreatorProcessor(dummyBO, "ASC", "TRA");
			CombineAssertions(() =>
			{
				AssertType<CustomsStmProcessQueueCreatorProcessor>("Type", result);
				AssertEquals("Parent", result.BizObj, dummyBO);
				AssertEquals("ApplicationCode", result.ApplicationCode, "ASC");
				AssertEquals("TriggerActionCode", result.TriggerActionCode, "TRA");
			});
		}
	}
}
