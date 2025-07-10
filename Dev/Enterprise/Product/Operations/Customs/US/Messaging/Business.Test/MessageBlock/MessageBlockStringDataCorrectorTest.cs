using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Testing
{
	sealed class MessageBlockStringDataCorrectorTest : TestCase
	{
		public void TestKeepOnlyValidCharacters()
		{
			ZStringBuilder builder = new ZStringBuilder();
			for (int i = 1; i < 256; i++)
			{
				builder.Append(((char)i).ToString());
			}

			ZString value = builder.ToString();
			AssertEquals(" ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ", MessageBlockStringDataCorrector.KeepOnlyValidCharacters(value, MessageBlockStringDataCorrector.CharacterType.Alphabetic, 255));
			AssertEquals(" 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ", MessageBlockStringDataCorrector.KeepOnlyValidCharacters(value, MessageBlockStringDataCorrector.CharacterType.Alphanumeric, 255));
			AssertEquals("0123456789", MessageBlockStringDataCorrector.KeepOnlyValidCharacters(value, MessageBlockStringDataCorrector.CharacterType.Numeric, 255));
			AssertMultilineASCIIEquals("", @" !""#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`ABCDEFGHIJKLMNOPQRSTUVWXYZ{|}~¢", MessageBlockStringDataCorrector.KeepOnlyValidCharacters(value, MessageBlockStringDataCorrector.CharacterType.Special, 255));
			AssertEquals(" ABCD", MessageBlockStringDataCorrector.KeepOnlyValidCharacters(value, MessageBlockStringDataCorrector.CharacterType.Alphabetic, 5));
			AssertEquals(" 0123", MessageBlockStringDataCorrector.KeepOnlyValidCharacters(value, MessageBlockStringDataCorrector.CharacterType.Alphanumeric, 5));
			AssertEquals("01234", MessageBlockStringDataCorrector.KeepOnlyValidCharacters(value, MessageBlockStringDataCorrector.CharacterType.Numeric, 5));
			AssertEquals(@" !""#$", MessageBlockStringDataCorrector.KeepOnlyValidCharacters(value, MessageBlockStringDataCorrector.CharacterType.Special, 5));
			value = "125.4ㅎ!B1#$3";
			AssertEquals("1254B13", MessageBlockStringDataCorrector.KeepOnlyValidCharacters(value, MessageBlockStringDataCorrector.CharacterType.Alphanumeric, 12));
			AssertEquals("125.4B13", MessageBlockStringDataCorrector.KeepOnlyValidCharacters(value, ABICharacterTypeString.Constants.Alphanumeric + ".", 12));
		}

		public void TestKeepOnlyValidCharacters_AMS()
		{
			var value = "125.4!B1#$3";
			AssertEquals("1254B13", MessageBlockStringDataCorrector.KeepOnlyValidCharacters(value, MessageBlockStringDataCorrector.CharacterType.Alphanumeric, 12));
			AssertEquals("125.4B13", MessageBlockStringDataCorrector.KeepOnlyValidCharacters(value, ABICharacterTypeString.Constants.Alphanumeric + ".", 12));
			value = @" !ㅎ""#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`ABCDEFGHIJKLMNOPQRSTUVWXYZ{|}~¢";
			AssertMultilineASCIIEquals("", @" !""#$%&'()+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`ABCDEFGHIJKLMNOPQRSTUVWXYZ{|}~", MessageBlockStringDataCorrector.KeepOnlyValidCharacters(value, AMSCharacterTypeString.Constants.Special, 255));
			AssertEquals(" ABCD", MessageBlockStringDataCorrector.KeepOnlyValidCharacters(value, AMSCharacterTypeString.Constants.Alphabetic, 5));
			AssertEquals(" 0123", MessageBlockStringDataCorrector.KeepOnlyValidCharacters(value, AMSCharacterTypeString.Constants.Alphanumeric, 5));
			AssertEquals("01234", MessageBlockStringDataCorrector.KeepOnlyValidCharacters(value, AMSCharacterTypeString.Constants.Numeric, 5));
			AssertEquals(@" !""#$", MessageBlockStringDataCorrector.KeepOnlyValidCharacters(value, AMSCharacterTypeString.Constants.Special, 5));
		}

		public void TestReplaceInvalidCharacters()
		{
			var value = @" !""#$%&'()*+,-./0123456789:;<=>?@ABㅎCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`ABCDEFGHIJKLMNOPQRSTUVWXYZ{|}~¢";
			CombineAssertions(() =>
			{
				AssertEquals(" ================================AB=CDEFGHIJKLMNOPQRSTUVWXYZ======ABCDEFGHIJKLMNOPQRSTUVWXYZ=====", MessageBlockStringDataCorrector.ReplaceInvalidCharacters(value, AMSCharacterTypeString.Constants.Alphabetic, '='));
				AssertEquals(" ===============0123456789=======AB=CDEFGHIJKLMNOPQRSTUVWXYZ======ABCDEFGHIJKLMNOPQRSTUVWXYZ=====", MessageBlockStringDataCorrector.ReplaceInvalidCharacters(value, AMSCharacterTypeString.Constants.Alphanumeric, '='));
				AssertEquals("================0123456789=======================================================================", MessageBlockStringDataCorrector.ReplaceInvalidCharacters(value, AMSCharacterTypeString.Constants.Numeric, '='));
				AssertEquals(@" !""#$%&'()=+,-./0123456789:;<=>?@AB=CDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`ABCDEFGHIJKLMNOPQRSTUVWXYZ{|}~=", MessageBlockStringDataCorrector.ReplaceInvalidCharacters(value, AMSCharacterTypeString.Constants.Special, '='));
			});
		}
	}
}
