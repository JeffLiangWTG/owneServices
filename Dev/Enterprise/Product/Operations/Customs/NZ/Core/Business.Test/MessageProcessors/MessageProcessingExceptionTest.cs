namespace Enterprise.Customs.NZ.Business.MessageProcessors.Testing
{
	using NUnit.Framework;

	public class MessageProcessingExceptionTest : TestCase
	{
		[ExpectException(typeof(MessageProcessingException))]
		public void TestException()
		{
			throw new MessageProcessingException("Test String");
		}
	}
}
