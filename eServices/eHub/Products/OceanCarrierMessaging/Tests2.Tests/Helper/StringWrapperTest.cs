using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests2.Tests.StringWrapperTest
{
	[TestClass]
	public class StringWrapperTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestStringWrapper()
		{
			var helper = new StringWrapper();

			var longText = "TEXT-0001 TEXT-0002 TEXT-0003";

			helper.SetupStringWrapper(10, 10);
			helper.SplitStringInWrapper(longText);

			Assert.AreEqual(3, helper.Count());
			Assert.AreEqual("TEXT-0001 ", helper.GetTextFromCurrentIndex());

			Assert.AreEqual("TEXT-0002 ", helper.GetTextFromNextIndex());
			Assert.AreEqual("TEXT-0002 ", helper.GetTextFromCurrentIndex());

			Assert.AreEqual("TEXT-0003", helper.GetTextFromNextIndex());
			Assert.AreEqual("TEXT-0003", helper.GetTextFromCurrentIndex());

			helper.ResetStringWrapper();
			Assert.AreEqual(0, helper.Count());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestStringWrapper_AddToList()
		{
			var helper = new StringWrapper();
			helper.ResetStringWrapper();
			helper.AddToList("TEXT-0001");
			helper.AddToList("TEXT-0002");

			Assert.AreEqual("TEXT-0001", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("TEXT-0002", helper.GetTextFromCurrentIndex());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestStringWrapper_NoSpaceInText()
		{
			var helper = new StringWrapper();

			var longText = "012345678901234567890123456789_THIS_SHOULD_NOT_IN_TEST_RESULT";

			helper.SetupStringWrapper(15, 2);
			helper.SplitStringInWrapper(longText);

			Assert.AreEqual(2, helper.Count());
			Assert.AreEqual("012345678901234", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("567890123456789", helper.GetTextFromCurrentIndex());

			helper.ResetStringWrapper();
			Assert.AreEqual(0, helper.Count());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestStringWrapper_MultipleLines_WithCR()
		{
			var helper = new StringWrapper();

			var longText = "MICROPHONE\rSPEAKER\rHEADPHONE\rMICROPHONE PARTS\rSPEAKER PARTS";

			helper.SetupStringWrapper(26, 100, true);
			helper.SplitStringInWrapper(longText);

			Assert.AreEqual(5, helper.Count());
			Assert.AreEqual("MICROPHONE", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("SPEAKER", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("HEADPHONE", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("MICROPHONE PARTS", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("SPEAKER PARTS", helper.GetTextFromCurrentIndex());

			helper.ResetStringWrapper();
			Assert.AreEqual(0, helper.Count());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestStringWrapper_MultipleLines_WithLF()
		{
			var helper = new StringWrapper();

			var longText = "MICROPHONE\nSPEAKER\nHEADPHONE\nMICROPHONE PARTS\nSPEAKER PARTS";

			helper.SetupStringWrapper(26, 100, true);
			helper.SplitStringInWrapper(longText);

			Assert.AreEqual(5, helper.Count());
			Assert.AreEqual("MICROPHONE", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("SPEAKER", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("HEADPHONE", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("MICROPHONE PARTS", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("SPEAKER PARTS", helper.GetTextFromCurrentIndex());

			helper.ResetStringWrapper();
			Assert.AreEqual(0, helper.Count());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestStringWrapper_MultipleLines_WithCRLF()
		{
			var helper = new StringWrapper();

			var longText = @"MICROPHONE
SPEAKER
HEADPHONE
MICROPHONE PARTS
SPEAKER PARTS";

			helper.SetupStringWrapper(26, 100, true);
			helper.SplitStringInWrapper(longText);

			Assert.AreEqual(5, helper.Count());
			Assert.AreEqual("MICROPHONE", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("SPEAKER", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("HEADPHONE", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("MICROPHONE PARTS", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("SPEAKER PARTS", helper.GetTextFromCurrentIndex());

			helper.ResetStringWrapper();
			Assert.AreEqual(0, helper.Count());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestStringWrapper_MultipleInputString()
		{
			var helper = new StringWrapper();

			helper.SetupStringWrapper(15, 4);
			helper.SplitStringInWrapper("WISETECH GLOBAL PTY LTD");
			helper.SplitStringInWrapper("Address 1");

			Assert.AreEqual(3, helper.Count());
			Assert.AreEqual("WISETECH GLOBAL", helper.GetTextFromCurrentIndex());
			Assert.AreEqual(" PTY LTD", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("Address 1", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("", helper.GetTextFromCurrentIndex());

			helper.ResetStringWrapper();
			Assert.AreEqual(0, helper.Count());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestStringWrapper_MultipleLines_SplitByBlankCharacters()
		{
			var helper = new StringWrapper();

			helper.SetupStringWrapper(26, 100);
			helper.SplitStringInWrapper(@"Levasil CC301 DRUM 255 KG
HS code 3824999699
NON HAZ CHEMICAL

FREIGHT PREAID
FCL/FCL
PALLETS/WOOD ARE COMPLIEDDD WITH ISPM 15

Emergency no, USA:
US CHEMTRC, +1 8-0 424-9300 (24 hours) Dan Haggarty

Notify 2:
Nouryon - 15115 Park Row, Suite 200");

			Assert.AreEqual(10, helper.Count());
			Assert.AreEqual("Levasil CC301 DRUM 255 KG\n", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("HS code 3824999699\nNON HAZ", helper.GetTextFromCurrentIndex());
			Assert.AreEqual(" CHEMICAL\nFREIGHT PREAID\n", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("FCL/FCL\nPALLETS/WOOD ARE ", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("COMPLIEDDD WITH ISPM 15\n", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("Emergency no, USA:\nUS ", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("CHEMTRC, +1 8-0 424-9300 ", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("(24 hours) Dan Haggarty\n", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("Notify 2:\nNouryon - 15115 ", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("Park Row, Suite 200", helper.GetTextFromCurrentIndex());

			helper.ResetStringWrapper();
			Assert.AreEqual(0, helper.Count());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestStringWrapper_MultipleLines_SplitByCRLF()
		{
			var helper = new StringWrapper();

			helper.SetupStringWrapper(26, 100, true);
			helper.SplitStringInWrapper(@"Levasil CC301 DRUM 255 KG
HS code 3824999699
NON HAZ CHEMICAL

FREIGHT PREAID
FCL/FCL
PALLETS/WOOD ARE COMPLIEDDD WITH ISPM 15

Emergency no, USA:
US CHEMTRC, +1 8-0 424-9300 (24 hours) Dan Haggarty

Notify 2:
Nouryon - 15115 Park Row, Suite 200");

			Assert.AreEqual(14, helper.Count());
			Assert.AreEqual("Levasil CC301 DRUM 255 KG", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("HS code 3824999699", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("NON HAZ CHEMICAL", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("FREIGHT PREAID", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("FCL/FCL", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("PALLETS/WOOD ARE ", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("COMPLIEDDD WITH ISPM 15", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("Emergency no, USA:", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("US CHEMTRC, +1 8-0 ", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("424-9300 (24 hours) Dan ", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("Haggarty", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("Notify 2:", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("Nouryon - 15115 Park Row, ", helper.GetTextFromCurrentIndex());
			Assert.AreEqual("Suite 200", helper.GetTextFromCurrentIndex());

			helper.ResetStringWrapper();
			Assert.AreEqual(0, helper.Count());
		}
	}
}