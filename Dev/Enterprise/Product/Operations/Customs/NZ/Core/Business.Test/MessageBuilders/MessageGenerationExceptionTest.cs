namespace Enterprise.Customs.NZ.Business.MessageBuilders.Testing
{
	using NUnit.Framework;

	public class MessageGenerationExceptionTest : TestCase
	{
		[ExpectException(typeof(MessageGenerationException))]
		public void TestException()
		{
			throw new MessageGenerationException("Test String");
		}
	}
}
