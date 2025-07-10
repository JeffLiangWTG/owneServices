using System;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Test
{
	[TestFixture]
	public class ParserHelperFixture
	{
		[TestCase("10 kg G", 10, "kg", Constants.AmtTypes.GrossWeightLimitAmtType)]
		[TestCase("Forbidden", 0, "", Constants.AmtTypes.ForbiddenAmtType)]
		[TestCase("15 L", 15, "L", Constants.AmtTypes.NetWeightLimitAmtType)]
		[TestCase("Not Restricted", 0, "", Constants.AmtTypes.NotRestrictedAmtType)]
		[TestCase("No Limit", 0, "", Constants.AmtTypes.NoLimitAmtType)]
		[TestCase("", 0, "", Constants.AmtTypes.NonApplicableAmtType)]
		[TestCase("See 10.3", 0, "", Constants.AmtTypes.NetWeightLimitAmtType)]
		public void SeparateAmtAndUqAndType(string amountText, decimal expectedAmount, string expectedUq, string expectedType)
		{
			var result = ParserHelper.SeparateAmtAndUqAndType(amountText);
			Assert.That(result.amount, Is.EqualTo(expectedAmount));
			Assert.That(result.uq, Is.EqualTo(expectedUq));
			Assert.That(result.type, Is.EqualTo(expectedType));
		}

		[TestCase("","10","CPV")]
		[TestCase("AA", "A1", "SPP")]
		public void CreateUNDGAttribute(string descriptor, string index, string type)
		{
			var attribute = ParserHelper.CreateUNDGAttribute(descriptor, index, type);
			Assert.That(attribute.DA_Descriptor, Is.EqualTo(descriptor));
			Assert.That(attribute.DA_Index, Is.EqualTo(index));
			Assert.That(attribute.DA_Language, Is.EqualTo(string.IsNullOrEmpty(descriptor) ? string.Empty : "EN"));
			Assert.That(attribute.DA_Type, Is.EqualTo(type));
		}

		[Test]
		public void LoadAttributesZZ()
		{
			var attrs = ParserHelper.LoadAttributesZZ("QDT");
			Assert.That(attrs.Length, Is.EqualTo(1));
			Assert.That(attrs.First().DAZ_Type, Is.EqualTo(Constants.AttributeTypes.QualifyingDescriptive));
			Assert.That(attrs.First().DAZ_Index, Is.EqualTo("0"));
			Assert.That(attrs.First().DAZ_Language, Is.EqualTo("EN"));
			Assert.That(attrs.First().DAZ_Descriptor, Is.EqualTo("QDT"));
		}

		[Test]
		public void CheckFieldLength()
		{
			var value = ParserHelper.CheckFieldLength("TextWithinRange", "", 20);
			Assert.That(value, Is.EqualTo("TextWithinRange"));

			value = ParserHelper.CheckFieldLength("TextExceedRange", "", 10);
			Assert.That(value, Is.EqualTo("TextExceed"));
		}

		[Test]
		public void TransformLQMaxAmt()
		{
			var value = ParserHelper.TransformLQMaxAmt("see SP251");
			Assert.That(value, Is.EqualTo(0));
			value = ParserHelper.TransformLQMaxAmt("5 kg");
			Assert.That(value, Is.EqualTo(5));
			value = ParserHelper.TransformLQMaxAmt("1 L or 2 kg");
			Assert.That(value, Is.EqualTo(1));
		}

		[Test]
		public void TransformLQMaxAmtUQ()
		{
			var value = ParserHelper.TransformLQMaxAmtUQ("see SP251");
			Assert.That(value, Is.EqualTo(string.Empty));
			value = ParserHelper.TransformLQMaxAmtUQ("5 kg");
			Assert.That(value, Is.EqualTo("kg"));
			value = ParserHelper.TransformLQMaxAmtUQ("1 L or 2 kg");
			Assert.That(value, Is.EqualTo("L"));
		}

		[Test]
		public void TransformLQMaxAmts()
		{
			var value = ParserHelper.TransformLQMaxAmts("500mL or 400g");
			var expected1 = new Tuple<decimal, string>(500, "mL");
			var expected2 = new Tuple<decimal, string>(400, "g");
			Assert.That(value[0], Is.EqualTo(expected1));
			Assert.That(value[1], Is.EqualTo(expected2));

			value = ParserHelper.TransformLQMaxAmts("500mL");
			expected1 = new Tuple<decimal, string>(500, "mL");
			Assert.That(value[0], Is.EqualTo(expected1));
			Assert.That(value.Length, Is.EqualTo(1));

			value = ParserHelper.TransformLQMaxAmts("500mL or");
			expected1 = new Tuple<decimal, string>(500, "mL");
			expected2 = new Tuple<decimal, string>(0, string.Empty);
			Assert.That(value[0], Is.EqualTo(expected1));
			Assert.That(value[1], Is.EqualTo(expected2));
			Assert.That(value.Length, Is.EqualTo(2));

			value = ParserHelper.TransformLQMaxAmts("500mL OR 400g");
			expected1 = new Tuple<decimal, string>(500, "mL");
			expected2 = new Tuple<decimal, string>(400, "g");
			Assert.That(value[0], Is.EqualTo(expected1));
			Assert.That(value[1], Is.EqualTo(expected2));
			Assert.That(value.Length, Is.EqualTo(2));

			value = ParserHelper.TransformLQMaxAmts(string.Empty);
			expected1 = new Tuple<decimal, string>(0, string.Empty);
			Assert.That(value[0], Is.EqualTo(expected1));
			Assert.That(value.Length, Is.EqualTo(1));
		}

		[Test]
		public void SeeContentCleanUp()
		{
			var value = ParserHelper.SeeContentCleanUp("see SP251");
			Assert.That(value, Is.EqualTo(string.Empty));
			value = ParserHelper.SeeContentCleanUp("SEE SP251");
			Assert.That(value, Is.EqualTo(string.Empty));
			value = ParserHelper.SeeContentCleanUp("T5 see 4.1.9.2.4");
			Assert.That(value, Is.EqualTo("T5"));
			value = ParserHelper.SeeContentCleanUp("CW33 (see 1.7.1.5.1)");
			Assert.That(value, Is.EqualTo("CW33"));
			value = ParserHelper.SeeContentCleanUp("S2.65AN(+) L2.65CN(+)");
			Assert.That(value, Is.EqualTo("S2.65AN(+) L2.65CN(+)"));
		}

		[Test]
		public void TransformPackIns()
		{
			var value = ParserHelper.TransformPackIns("P001 IBC* LP01 R001");
			Assert.That(value, Is.EqualTo("P001 LP01 R001"));
			value = ParserHelper.TransformPackIns("P801 P801a");
			Assert.That(value, Is.EqualTo("P801 P801a"));
		}

		[Test]
		public void TransformIBCIns()
		{
			var value = ParserHelper.TransformIBCIns("P001 IBC* LP01 R001");
			Assert.That(value, Is.EqualTo("IBC*"));
			value = ParserHelper.TransformIBCIns("P801 P801a");
			Assert.That(value, Is.EqualTo(string.Empty));
		}

		[Test]
		public void TransformClassificationCode()
		{
			var value = ParserHelper.TransformClassificationCode("1.1F");
			Assert.That(value, Is.EqualTo("1.1F"));
			value = ParserHelper.TransformClassificationCode("EMPTY");
			Assert.That(value, Is.EqualTo(string.Empty));
			value = ParserHelper.TransformClassificationCode("BLANK");
			Assert.That(value, Is.EqualTo(string.Empty));
		}

		[TestCase("1 (a)(", "1 (A)")]
		[TestCase("2 (D/E", "2 (D/E)")]
		[TestCase("", "")]
		public void TransformTransportCategoryTest(string value, string expected)
		{
			Assert.That(ParserHelper.TransformTransportCategory(value, "0"), Is.EqualTo(expected));
		}

		[TestCase("1234", ExpectedResult = "1234")]
		[TestCase("23", ExpectedResult = "0023")]
		[TestCase("5", ExpectedResult = "0005")]
		[TestCase("111", ExpectedResult = "0111")]
		public string ParseUNNO(string rawString)
		{
			return ParserHelper.ParseUNNO(rawString);
		}

		[TestCase("04a", ExpectedResult = "0004a")]
		[TestCase("51", ExpectedResult = "0051")]
		[TestCase("5", ExpectedResult = "0005")]
		[TestCase("9b", ExpectedResult = "0009b")]
		public string ParseUNNOAndVariant(string rawString)
		{
			return ParserHelper.ParseUNNOAndVariant(rawString);
		}
	}
}
