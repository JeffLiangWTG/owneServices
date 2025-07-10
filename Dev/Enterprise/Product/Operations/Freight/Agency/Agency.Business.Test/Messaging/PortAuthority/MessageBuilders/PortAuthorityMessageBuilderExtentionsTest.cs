using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	sealed class PortAuthorityMessageBuilderExtentionsTest : TestCase
	{
		public void TestGetInterchangeText()
		{
			const string messageText =
				"SEG+Random:<<MSGNO PLACEHOLDER>>:Crap'" +
				"";

			const string interchangeText =
				"UNA:+.? '" +
				"UNB+UNOA:1+SENDER+RECIPIENT+090517:1245+090517124500'" +
				"SEG+Random:090517124500:Crap'" +
				"UNZ+1+090517124500'" +
				"";

			MockRepository repository = new MockRepository(MockBehavior.Strict);
			var builder = repository.Create<IPortAuthorityMessageBuilder>();
			var data = repository.Create<IPortAuthorityMessagingData>();

			builder.Setup(m => m.GenerateMessageText(data.Object)).Returns(messageText);

			string result;

			result = builder.Object.GetInterchangeText(data.Object, new ZDateTime(2009, 5, 17, 12, 45, 00), "sender", "recipient");

			AssertMultilineASCIIEquals("", interchangeText.Replace("'", "'\r\n"), result.Replace("'", "'\r\n"));
		}
	}
}
