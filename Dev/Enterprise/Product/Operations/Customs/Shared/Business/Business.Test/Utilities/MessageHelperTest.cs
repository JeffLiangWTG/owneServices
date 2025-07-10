using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing;

[TestedType(typeof(MessageHelper))]
sealed class MessageHelperTest : TestCase
{
	public void TestGetHeaderTextDictionary_WithTrailingComma()
	{
		const string headerText = @"{
			""custom.Credentials.UserName"":"""",
			""custom.ErrorType"":""ERR"",
			""custom.NotificationType"":""Failure"",
			}";
		var actual = MessageHelper.GetHeaderTextDictionary(headerText);
		var expected = new Dictionary<string, string>
		{
			{ "custom.ErrorType", "ERR" },
			{ "custom.NotificationType", "Failure" }
		};

		AssertContainsExactElementsInAnyOrder("HeaderTextDictionary contains all valid elements", expected, actual);
	}
}
