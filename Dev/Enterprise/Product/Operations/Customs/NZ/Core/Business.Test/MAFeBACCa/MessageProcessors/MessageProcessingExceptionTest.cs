namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageProcessors.Testing
{
	using CargoWise.EntityFramework.Testing;
	public class MessageProcessingExceptionTest : TestCaseWithFactory
	{
		public void TestInstantiation()
		{
			NZMMessage message = Factory.New<NZMMessage>();
			message.EM_MessageText = "Message Text";
			AssertExceptionThrown(typeof(MessageProcessingException), "Message\r\n\r\nMessage Text\r\n", delegate
			{
				throw new MessageProcessingException("Message", message, true, true);
			});
		}
	}
}
