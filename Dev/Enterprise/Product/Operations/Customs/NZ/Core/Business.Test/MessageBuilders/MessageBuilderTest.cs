using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Edifact;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.Testing
{
	using System.Reflection;
	using CargoWise.Types;

	public abstract class MessageBuilderTest : TestCaseWithFactory
	{
		public abstract void TestGenerateTestMessage();
		public abstract void TestGenerateLiveMessage();

		protected MessageBuilder messageBuilder;

		protected void TestMessageBuilderResultAgainstString(string expectedMessage)
		{
			string generatedMessage = messageBuilder.GetMessageText().Replace(new UNOACharacterSet().SegmentDelimiter[0].ToString(), "\r\n");
			Assert2MessagesAreTheSame(expectedMessage, generatedMessage);
		}

		protected void Assert2MessagesAreTheSame(string expectedMessage, string actualMessage)
		{
			AssertMultilineASCIIEquals("Message", expectedMessage, actualMessage.Replace(new UNOACharacterSet().SegmentDelimiter[0].ToString(), "\r\n"));
		}

		protected void SetAllZStringPropertiesToLowerCase(BusinessObject bo)
		{
			foreach (PropertyInfo propertyInfo in bo.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				if (propertyInfo.PropertyType == typeof(ZString) && propertyInfo.CanRead && propertyInfo.CanWrite)
				{
					ZString value = (ZString)propertyInfo.GetValue(bo, null);
					propertyInfo.SetValue(bo, value.ToLower(), null);
				}
			}
		}
	}
}
