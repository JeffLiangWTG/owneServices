using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Testing
{
	sealed class MessageBlockStringAttributeTest : TestCase
	{
		public void TestSerialiseIfMaxLengthExceeded()
		{
			try
			{
				new MessageBlockStringAttribute(2, 1, "M").Serialise(null, new ZString("XYZ"));
				Fail("Should have thrown a MessageBlockSerialisationException");
			}
			catch (MessageBlockSerialisationException ex)
			{
				AssertEquals("Data provided exceeded allowable maximum length:\r\nMaximum Length:2\r\nActual Length:3", ex.Message);
				AssertEquals("**", ex.NewInvalidFormat);
			}

			try
			{
				AssertEquals("**", new MessageBlockStringAttribute(2, 1, "M")
				{
					OnLengthViolation = LengthViolationAction.SetInvalidValue
				}.Serialise(null, new ZString("XYZ")));
			}
			catch (MessageBlockSerialisationException)
			{
				Fail("Should not have thrown a MessageBlockSerialisationException");
			}

			try
			{
				AssertEquals("XY", new MessageBlockStringAttribute(2, 1, "M")
				{
					OnLengthViolation = LengthViolationAction.Substring
				}.Serialise(null, new ZString("XYZ")));
			}
			catch (MessageBlockSerialisationException)
			{
				Fail("Should not have thrown a MessageBlockSerialisationException");
			}
		}

		public void TestNonABICharactersAreReplaceWithASpace()
		{
			AssertEquals("X*Y*Z", new MessageBlockStringAttribute(5, 1, "M").Serialise(null, new ZString("X\rY\nZ")));
			AssertEquals("X*Y*Z", new MessageBlockStringAttribute(5, 1, "M").Serialise(null, new ZString("X÷yÌz")));

			var attribute = new MessageBlockStringAttribute(10, 1, "M");
			var validStrings = new ZString[] {
					@"!@#$%^&*()",
					@"-_=+[{]}\|",
					@";:'"",<.>/?",
					@"`~  ABCDEF",
					@"GHIJKLMNOP",
					@"QRSTUVWXYZ",
					@"0123456789",
					@"abcdefghij",
					@"klmnopqrst",
					@"uvwxyz    "
			};

			foreach (var validString in validStrings)
			{
				AssertEquals(validString.ToUpper(), attribute.Serialise(null, validString));
			}
		}

		public void TestNonAMSCharactersAreReplaced()
		{
			var attribute = new MessageBlockStringAttribute(6, 1, "M");
			attribute.ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			AssertEquals("X?Y\\Z?", attribute.Serialise(null, new ZString(@"X*Y\Z¢")));
			AssertEquals("X?Y?Z ", attribute.Serialise(null, new ZString("X÷yÌz")));
			attribute.IsSpecialReplacingBehaviourOfInvalidCharacterOn = true;
			AssertEquals("X YTZ ", attribute.Serialise(null, new ZString(@"X*YTZ")));
			AssertEquals("X YTZ ", attribute.Serialise(null, new ZString("X÷yTz")));

			attribute = new MessageBlockStringAttribute(10, 1, "M");
			var validStrings = new ZString[] {
					@"!@#$%^& ()",
					@"-_=+[{]} |",
					@";:'"",<.>/?",
					@"`~  ABCDEF",
					@"GHIJKLMNOP",
					@"QRSTUVWXYZ",
					@"0123456789",
					@"abcdefghij",
					@"klmnopqrst",
					@"uvwxyz    "
			};

			foreach (var validString in validStrings)
			{
				AssertEquals(validString.ToUpper(), attribute.Serialise(null, validString));
			}
		}

		public void TestMaskSSNNumberWithAsterisks()
		{
			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = true;
			var attribute = new MessageBlockStringAttribute(11, 1, "M");
			attribute.IsPersonalInformation = false;
			AssertEquals("999-99-5555", attribute.Serialise(null, new ZString(@"999-99-5555")));
			AssertEquals("999-99-5555", attribute.Serialise(null, new ZString(@"999-99-5555"), true));

			attribute.IsPersonalInformation = true;
			AssertEquals("999-99-5555", attribute.DeSerialise(new ZString(@"999-99-5555")));
			AssertEquals("999-99-5555", attribute.Serialise(null, new ZString(@"999-99-5555"), true));

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			attribute.IsPersonalInformation = false;
			AssertEquals("999-99-5555", attribute.Serialise(null, new ZString(@"999-99-5555")));
			AssertEquals("999-99-5555", attribute.Serialise(null, new ZString(@"999-99-5555"), true));

			attribute.IsPersonalInformation = true;
			AssertEquals("***-**-****", attribute.DeSerialise(new ZString(@"999-99-5555")));
			AssertEquals("***-**-****", attribute.Serialise(null, new ZString(@"999-99-5555"), true));

			attribute = new MessageBlockStringAttribute(9, 1, "M");
			attribute.IsPersonalInformation = true;
			attribute.MaskButDoNotCheckFormat = true;
			AssertEquals("*********", attribute.DeSerialise(new ZString(@"999995555")));
			AssertEquals("*********", attribute.Serialise(null, new ZString(@"999995555"), true));
		}

		public void TestSerialiseZString()
		{
			AssertEquals("XYZ  ", new MessageBlockStringAttribute(5, 1, "M").Serialise(null, new ZString("XYZ")));
		}

		public void TestRightJustifiedZString()
		{
			AssertEquals("  XYZ", new MessageBlockStringAttribute(5, 1, "M")
			{
				Justification = Justification.Right
			}.Serialise(null, new ZString("XYZ")));
		}

		[MessageBlockString(51, 3, "C", OnLengthViolation = LengthViolationAction.Substring)]
		public ZString TestString;

		public void TestTruncateLongerMessages()
		{
			TestString = "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz";

			AssertEquals("ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXY", new MessageBlockStringAttribute(51, 1, "M")
			{
				OnLengthViolation = LengthViolationAction.Substring
			}.Serialise(null, new ZString("ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ")));
		}

		public void TestLengthViolationAction_SubstringFromRight()
		{
			var attr = new MessageBlockStringAttribute(5, 1, "M")
			{
				OnLengthViolation = LengthViolationAction.SubstringFromRight
			};
			var str = (ZString)"123456789";
			var serialised = attr.Serialise(null, str);
			AssertEquals("56789", serialised);
		}

		public void TestGetCharacterTypeToReplaceWith()
		{
			var characterTypeToReplaceWith = MessageBlockStringAttribute.GetCharacterTypeToReplaceWith(CBPEDIInterchange.ApplicationCodes.AMS);
			AssertEquals("a question mark '?'", characterTypeToReplaceWith);
			characterTypeToReplaceWith = MessageBlockStringAttribute.GetCharacterTypeToReplaceWith("");
			AssertEquals("an asterisk '*'", characterTypeToReplaceWith);
		}

		public void TestReplaceWithValidCharacters()
		{
			var attribute = new MessageBlockStringAttribute(10, 1, "M");
			attribute.IsSpecialReplacingBehaviourOfInvalidCharacterOn = false;
			AssertEquals("*D%*DR*SS3", attribute.Serialise(null, new ZString(@"àd%*drêss3")));

			attribute.ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			attribute.IsSpecialReplacingBehaviourOfInvalidCharacterOn = false;
			AssertEquals("?D%??DR?S3", attribute.Serialise(null, new ZString(@"àd%*¢drês3")));

			var attribute2 = new MessageBlockStringAttribute(70, 1, "M");
			attribute2.IsSpecialReplacingBehaviourOfInvalidCharacterOn = false;
			attribute2.ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			var result = attribute2.Serialise(null, new ZString(@"àd%*¢drês3^!""#$%&'()+,-./0123456789:; <=>?@ABCDEFGHIjkl[\]^_`{|}~ㅎ$"));
			AssertEquals(@"?D%??DR?S3^!""#$%&'()+,-./0123456789:; <=>?@ABCDEFGHIJKL[\]^_`{|}~?$   ", result);
		}
	}
}
