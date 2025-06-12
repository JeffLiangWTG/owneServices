using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests2.Tests.StringHelperTest
{
  [TestClass]
  public class StringHelperTest
  {
    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestShrinkSpaces()
    {
      Assert.AreEqual("AB CD", Helper.ShrinkSpaces("   AB   CD   "));
      Assert.AreEqual("A B C D", Helper.ShrinkSpaces("A   B C     D"));
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestSubstringSafe()
    {
      var value = "ABCDEF";
      Assert.AreEqual("A", Helper.SubstringSafe(value, 0, 1));
      Assert.AreEqual("B", Helper.SubstringSafe(value, 1, 1));
      Assert.AreEqual("ABCDEF", Helper.SubstringSafe(value, 0, 6));
      Assert.AreEqual("", Helper.SubstringSafe(value, 0, 0));
      Assert.AreEqual("", Helper.SubstringSafe(value, 1, 0));

      Assert.AreEqual("BCDEF", helper.SubstringSafe(value, 1));
      Assert.AreEqual("ABCDEF", helper.SubstringSafe(value, 0));
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestReplaceCRLFText()
    {
      var value = @"A
B
C";

      Assert.AreEqual("A B C", Helper.ReplaceCRLFText(value));
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestTrim()
    {
      var value = @"                   A
B
C                   ";

      Assert.AreEqual("A\nB\nC", Helper.Trim(value));
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestReplaceCRLFTextWithNewLine()
    {
      var value = "A\r\nB\r\nC";

      Assert.AreEqual("A\nB\nC", Helper.ReplaceCRLFTextWithNewLine(value));
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestShrinkCRLF()
    {
      var value = "A\r\n\r\nB\n\nC\r\n\nD";

      Assert.AreEqual("A\nB\nC\nD", Helper.ShrinkCRLF(value));
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestTrimCRLF()
    {
      var value = "\r\n\nABC\n\n\r\n";

      Assert.AreEqual("ABC", Helper.TrimCRLF(value));
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestReplaceText()
    {
      var value = "A x C";

      Assert.AreEqual("A B C", Helper.ReplaceText(value, "x", "B"));
    }


    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void ToUpper()
    {
      Assert.AreEqual("ABC", Helper.ToUpper("abc"));
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestPadRight()
    {
      var value = "ab";
      Assert.AreEqual("ab ", Helper.PadRight(value, 3, " "));
      Assert.AreEqual("ab-", Helper.PadRight(value, 3, "-"));
      Assert.AreEqual("ab   ", Helper.PadRight(value, 5, " "));
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestMatchPattern()
    {
      Assert.IsTrue(Helper.MatchPattern("ABCABC", "^ABC"));
      Assert.IsTrue(Helper.MatchPattern("ABCABC", "ABC$"));
      Assert.IsFalse(Helper.MatchPattern("AABBCC", "ABC"));
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestIsNewFormMessage()
    {
      Assert.IsFalse(Helper.IsNewFormMessage(null));
      Assert.IsFalse(Helper.IsNewFormMessage(""));
      Assert.IsFalse(Helper.IsNewFormMessage("xx"));
      Assert.IsFalse(Helper.IsNewFormMessage("1"));
      Assert.IsFalse(Helper.IsNewFormMessage("0"));
      Assert.IsFalse(Helper.IsNewFormMessage("0.1"));
      Assert.IsFalse(Helper.IsNewFormMessage("0.9.9"));
      Assert.IsFalse(Helper.IsNewFormMessage("2"));

      Assert.IsTrue(Helper.IsNewFormMessage("1.0"));
      Assert.IsTrue(Helper.IsNewFormMessage("1.0.0"));
      Assert.IsTrue(Helper.IsNewFormMessage("1.0.1"));
      Assert.IsTrue(Helper.IsNewFormMessage("1.0.0.1"));
      Assert.IsTrue(Helper.IsNewFormMessage("2.0"));

      Assert.IsFalse(Helper.IsNewFormMessage("1.9", "2.0.0"));
      Assert.IsFalse(Helper.IsNewFormMessage("1.0", "2.0.0"));
      Assert.IsFalse(Helper.IsNewFormMessage("2.0.1", "2.0.2"));

      Assert.IsTrue(Helper.IsNewFormMessage("2.0", "2.0.0"));
      Assert.IsTrue(Helper.IsNewFormMessage("2.0.2", "2.0.2"));
      Assert.IsTrue(Helper.IsNewFormMessage("2.1", "2.0.2"));
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCharCount()
    {
      var value = "11111_22222_33333";
      Assert.AreEqual(0, Helper.CharCount(null, "_"));
      Assert.AreEqual(0, Helper.CharCount(value, "X"));
      Assert.AreEqual(2, Helper.CharCount(value, "_"));
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestGetIndexOfValue()
    {
      var value = "11111_22222_33333";
      Assert.AreEqual("", Helper.GetIndexOfValue(null, "_", 2));
      Assert.AreEqual("", Helper.GetIndexOfValue(value, "X", 2));
      Assert.AreEqual("", Helper.GetIndexOfValue(value, "_", 3));
      Assert.AreEqual("", Helper.GetIndexOfValue(value, "_", 99));

      Assert.AreEqual("11111", Helper.GetIndexOfValue(value, "_", 1));
      Assert.AreEqual("11111_22222", Helper.GetIndexOfValue(value, "_", 2));
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestGetValueOrDefault()
    {
      Assert.AreEqual("", Helper.GetValueOrDefault(null));
      Assert.AreEqual("123", Helper.GetValueOrDefault("123"));
    }


    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestContainsAny()
    {
      Assert.IsTrue(Helper.ContainsAny("01;02;03;04;05;", "01"));
      Assert.IsTrue(Helper.ContainsAny("01;02;03;04;05;", "02"));
      Assert.IsTrue(Helper.ContainsAny("01;02;03;04;05;", "03"));
      Assert.IsTrue(Helper.ContainsAny("01;02;03;04;05;", "04"));
      Assert.IsTrue(Helper.ContainsAny("01;02;03;04;05;", "05"));
      Assert.IsTrue(Helper.ContainsAny("01;02;03;04;05", "05"));
      Assert.IsFalse(Helper.ContainsAny("01;02;03;04;05;", "99"));
      Assert.IsFalse(Helper.ContainsAny("01;02;03;04;05;", ""));
      Assert.IsFalse(Helper.ContainsAny("01;02;03;04;05;", null));

      Assert.IsTrue(Helper.ContainsAny("0102030405", "01"));
      Assert.IsTrue(Helper.ContainsAny("0102030405", "02"));
      Assert.IsTrue(Helper.ContainsAny("0102030405", "03"));
      Assert.IsTrue(Helper.ContainsAny("0102030405", "04"));
      Assert.IsTrue(Helper.ContainsAny("0102030405", "05"));
      Assert.IsFalse(Helper.ContainsAny("0102030405", "99"));
    }

    StringHelper Helper => helper ?? (helper = new StringHelper());
    StringHelper helper;

  }
}