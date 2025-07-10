namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageProcessors.Testing
{
	using Enterprise.Customs.NZ.Business.MAFeBACCa.Testing;
	abstract class MessageProcessorTest : TestCaseForMessageTesting
	{
		public abstract void TestCanProcess();
	}
}
