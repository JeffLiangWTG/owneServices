using CargoWise.Common;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class MessageInterpretationGeneratorTest : TestCaseWithFactory
	{
		public void TestGetFormattedMessageText()
		{
			var messageText = "<Root><tag1>TestTestTest</tag1><tag2>TestTestTest</tag2></Root>";
			var expectedXML = @"<Root>
  <tag1>TestTestTest</tag1>
  <tag2>TestTestTest</tag2>
</Root>";
			if (XmlUtils.IsValidXml(messageText, out var xmlDocument))
			{
				var formatedXML = MessageInterpretationGenerator.GetFormattedMessageText(xmlDocument);
				var nonFormatedXML = xmlDocument.ToFormattedString();
				AssertEquals(expectedXML, formatedXML);
				AssertNotEquals(expectedXML, nonFormatedXML);
			}
		}
	}
}


