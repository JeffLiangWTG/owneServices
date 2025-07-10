using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Messaging.Business.AWB;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;
using ValueType = Enterprise.Messaging.Business.AWB.ValueType;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.Testing
{
	sealed class AWBMessageBlockTest : TestCase
	{
		public void TestColumnIdentifierDeserialise()
		{
			var data = "MUS/JOHN A-PAUL B-RINGO C-GEORGE\r\n";
			var messageBlock = new AWBMessageBlockDummyWithColumns();
			var result = messageBlock.Deserialise(data);

			CombineAssertions(() =>
			{
				AssertEquals("success", true, result.success);
				AssertEquals("StringField1", "JOHN", messageBlock.StringField1);
				AssertEquals("StringField2", "PAUL", messageBlock.StringField2);
				AssertEquals("StringField3", "RINGO", messageBlock.StringField3);
				AssertEquals("StringField4", "GEORGE", messageBlock.StringField4);
			});
		}

		public void TestColumnIdentifierDeserialise_OptionalColumnMissing()
		{
			var data = "MUS/JOHN A-PAUL C-GEORGE\r\n";
			var messageBlock = new AWBMessageBlockDummyWithColumns();
			var result = messageBlock.Deserialise(data);

			CombineAssertions(() =>
			{
				AssertEquals("success", true, result.success);
				AssertEquals("StringField1", "JOHN", messageBlock.StringField1);
				AssertEquals("StringField2", "PAUL", messageBlock.StringField2);
				AssertEquals("StringField3", "", messageBlock.StringField3);
				AssertEquals("StringField4", "GEORGE", messageBlock.StringField4);
			});
		}

		public void TestColumnIdentifierDeserialise_MandatoryColumnsOnly()
		{
			var data = "MUS/JOHN A-PAUL\r\n";
			var messageBlock = new AWBMessageBlockDummyWithColumns();
			var result = messageBlock.Deserialise(data);

			CombineAssertions(() =>
			{
				AssertEquals("success", true, result.success);
				AssertEquals("StringField1", "JOHN", messageBlock.StringField1);
				AssertEquals("StringField2", "PAUL", messageBlock.StringField2);
				AssertEquals("StringField3", "", messageBlock.StringField3);
				AssertEquals("StringField4", "", messageBlock.StringField4);
			});
		}

		public void TestIsEmpty()
		{
			var dummy = new AWBMessageBlockDummy();
			AssertEquals("IsEmpty", true, dummy.IsEmpty);

			dummy.StringField1 = "HI";
			AssertEquals("IsEmpty", false, dummy.IsEmpty);

			dummy.StringField1 = ZString.Empty;
			dummy.DecimalField1 = 100.20m;
			AssertEquals("IsEmpty", false, dummy.IsEmpty);

			dummy.DecimalField1 = ZDecimal.Zero;
			dummy.CargoDesc = "HELLO";
			AssertEquals("IsEmpty", false, dummy.IsEmpty);

			var dummy2 = new AWBMessageBlockDummy2MandatoryOnly();
			AssertEquals("IsEmpty", true, dummy2.IsEmpty);
		}

		public void TestGetAttributeFieldInfos()
		{
			AWBMessageBlock block = new AWBMessageBlockDummy2MandatoryOnly();
			AssertEquals(0, block.GetAttributeFieldInfos().Count());

			block = new AWBMessageBlockDummy();
			AssertEquals("StringField1;DateField1;DecimalField1;StringField2;DecimalField2;DateField2;StringField3;DecimalField3;DateField3", new ZStringBuilder(block.GetAttributeFieldInfos().Select(x => x.FieldInfo.Name)).ToStringWithDelimiterBetweenAppends(";"));
		}

		[ExpectNoExceptions]
		public void TestPositionIsNotDuplicated()
		{
			var failures = new ZStringBuilder();
			var retriever = new SubClassRetriever(GetType().Assembly, typeof(AWBMessageBlock));
			retriever.IncludeAbstractClasses = false;
			foreach (var type in retriever.Retrieve())
			{
				var block = (AWBMessageBlock)Activator.CreateInstance(type);
				var positionChecker = new Dictionary<int, string>();
				foreach (var fieldInfo in block.GetFieldInfos())
				{
					var key = 0;
					var value = "";
					try
					{
						if (fieldInfo is AWBMessageBlock.SpecialFieldInfo mandatoryFieldInfo)
						{
							key = fieldInfo.Position;
							value = mandatoryFieldInfo.Value;
						}
						else if (fieldInfo is AWBMessageBlock.AttributeFieldInfo attributeFieldInfo)
						{
							key = fieldInfo.Position;
							value = attributeFieldInfo.FieldInfo.Name;
						}
						positionChecker.Add(key, value);
					}
					catch (ArgumentException ex)
					{
						failures.AppendLine($"{type.FullName} - Position '{key}' is duplicated, Value '{value}'.\r\n{ex.Message}");
					}
				}
			}
			if (!failures.IsEmpty)
			{
				Fail(failures.ToString());
			}
		}

		public void TestSerialise()
		{
			CombineAssertions(() =>
			{
				var messageBlock = new AWBMessageBlockDummy();
				AssertMultilineASCIIEquals("data 1", @"----------AWBMessageBlockDummy----------
StringField1 (1-5A)  :?
DecimalField1 (1-8N) :0
DateField2 (5-5AN)   :?????
DecimalField3 (4-4N) :???0
", messageBlock.Serialise());

				messageBlock.StringField1 = "JOE";
				messageBlock.DateField1 = new ZDate(2018, 9, 15);
				messageBlock.DecimalField1 = 8343m;
				messageBlock.StringField2 = "JOHN1";
				messageBlock.DecimalField2 = 1293.56m;
				messageBlock.DateField2 = new ZDate(2017, 5, 25);
				messageBlock.StringField3 = "MAR-Y";
				messageBlock.DecimalField3 = 4832.00m;
				messageBlock.DateField3 = new ZDate(2016, 7, 2);
				messageBlock.CargoDesc = "123456780ABCDE-FG/HI J*KL.MNOPQRSTUVWXYZ";
				AssertMultilineASCIIEquals("data 2", @"----------AWBMessageBlockDummy----------
StringField1 (1-5A)   :JOE
DateField1 (8-8N)     :09152018
DecimalField1 (1-8N)  :8343
StringField2 (2-10AN) :JOHN1
DecimalField2 (1-8N)  :1293.56
DateField2 (5-5AN)    :25MAY
StringField3 (1-5T)   :MAR-Y
DecimalField3 (4-4N)  :4832
DateField3 (8-8T)     :02-07-16
CargoDesc (1-30T)     :123456780ABCDE-FG HI J KL.MNOP
", messageBlock.Serialise());

				messageBlock.DateField1 = ZDate.Empty;
				messageBlock.StringField2 = ZString.Empty;
				messageBlock.DecimalField2 = ZDecimal.Zero;
				messageBlock.StringField3 = ZString.Empty;
				messageBlock.DateField3 = ZDate.Empty;
				messageBlock.CargoDesc = ZString.Empty;
				AssertMultilineASCIIEquals("data 3", @"----------AWBMessageBlockDummy----------
StringField1 (1-5A)  :JOE
DecimalField1 (1-8N) :8343
DateField2 (5-5AN)   :25MAY
DecimalField3 (4-4N) :4832
", messageBlock.Serialise());
			});
		}

		public void TestCreateElements()
		{
			CombineAssertions(() =>
			{
				var messageBlock = new AWBMessageBlockDummy();
				AssertMultilineASCIIEquals("data 1", @"BOB?/-0C
/?????
???0", messageBlock.CreateElements().ToString());

				messageBlock.StringField1 = "JOE";
				messageBlock.DateField1 = new ZDate(2018, 9, 15);
				messageBlock.DecimalField1 = 8343m;
				messageBlock.StringField2 = "JOHN1";
				messageBlock.DecimalField2 = 1293.56m;
				messageBlock.DateField2 = new ZDate(2017, 5, 25);
				messageBlock.StringField3 = "MAR-Y";
				messageBlock.DecimalField3 = 4832.00m;
				messageBlock.DateField3 = new ZDate(2016, 7, 2);
				messageBlock.CargoDesc = "123456780ABCDE-FG/HI J*KL.MNOPQRSTUVWXYZ";
				AssertMultilineASCIIEquals("data 2", @"BOBJOE/09152018-8343CJOHN1/1293.56
/25MAY-MAR-Y
483202-07-16
/123456780A
/BCDE-FG HI
/ J KL.MNOP", messageBlock.CreateElements().ToString());

				messageBlock.DateField1 = ZDate.Empty;
				messageBlock.StringField2 = ZString.Empty;
				messageBlock.DecimalField2 = ZDecimal.Zero;
				messageBlock.StringField3 = ZString.Empty;
				messageBlock.DateField3 = ZDate.Empty;
				messageBlock.CargoDesc = ZString.Empty;
				AssertMultilineASCIIEquals("data 3", @"BOBJOE/-8343C
/25MAY
4832", messageBlock.CreateElements().ToString());
			});
		}

		public void TestInvalidDataDeserialise()
		{
			var messageBlock = new AWBMessageBlockDummy();
			AssertExceptionThrown<InvalidMessageFormatException>("NULL", "Data Mismatch in Message.\r\nError reading CRLF #1 from block AWBMessageBlockDummy.", () => messageBlock.Deserialise(null));
			AssertExceptionThrown<InvalidMessageFormatException>("Missing CRLF", "Data Mismatch in Message.\r\nError reading CRLF #1 from block AWBMessageBlockDummy.", () => messageBlock.Deserialise("Brett"));
		}

		public void TestValidDeSerialise()
		{
			CombineAssertions(() =>
			{
				var messageBlock = new AWBMessageBlockDummy();
				messageBlock.StringField1 = "JOE";
				messageBlock.DateField1 = new ZDate(2018, 9, 15);
				messageBlock.DecimalField1 = 8343m;
				messageBlock.StringField2 = "JOHN1";
				messageBlock.DecimalField2 = 1293.56m;
				messageBlock.DateField2 = new ZDate(ZDate.Today.Year, 5, 25);
				messageBlock.StringField3 = "MAR-Y";
				messageBlock.DecimalField3 = 4832.00m;
				messageBlock.DateField3 = new ZDate(2016, 7, 2);
				messageBlock.CargoDesc = "123456780ABCDE-FG/HI J*KL.MNOPQRSTUVWXYZ";
				var data = messageBlock.CreateElements().ToString() + "HELLO";
				messageBlock = new AWBMessageBlockDummy();
				var result = messageBlock.Deserialise(data);
				AssertEquals("success", true, result.success);
				AssertEquals("StringField1", "JOE", messageBlock.StringField1);
				AssertEquals("DateField1", new ZDate(2018, 9, 15), messageBlock.DateField1);
				AssertEquals("DecimalField1", 8343m, messageBlock.DecimalField1);
				AssertEquals("StringField2", "JOHN1", messageBlock.StringField2);
				AssertEquals("DateField2", new ZDate(ZDate.Today.Year, 5, 25), messageBlock.DateField2);
				AssertEquals("DecimalField2", 1293.56m, messageBlock.DecimalField2);
				AssertEquals("StringField3", "MAR-Y", messageBlock.StringField3);
				AssertEquals("DateField3", new ZDate(2016, 7, 2), messageBlock.DateField3);
				AssertEquals("DecimalField3", 4832m, messageBlock.DecimalField3);
				AssertEquals("CargoDesc", "123456780ABCDE-FG HI J KL.MNOP", messageBlock.CargoDesc);
				AssertEquals("leftoverLineData", "HELLO", result.leftoverLineData);
			});
		}

		public void TestEquals()
		{
			var dummy1 = new AWBMessageBlockDummy();
			var dummy2 = new AWBMessageBlockDummy();
			AssertEquals(true, dummy1.Equals(dummy2));
			dummy1.StringField1 = "A";
			AssertEquals(false, dummy1.Equals(dummy2));
			dummy2.StringField1 = "A";
			AssertEquals(true, dummy1.Equals(dummy2));
			dummy1.DecimalField1 = 12m;
			AssertEquals(false, dummy1.Equals(dummy2));
			dummy2.DecimalField1 = 12m;
			AssertEquals(true, dummy1.Equals(dummy2));
		}

		public void TestGetHashCode()
		{
			var dummy1 = new AWBMessageBlockDummy();
			var dummy2 = new AWBMessageBlockDummy();
			AssertEquals(dummy1.GetHashCode(), dummy2.GetHashCode());
			dummy1.StringField1 = "A";
			AssertNotEquals(dummy1.GetHashCode(), dummy2.GetHashCode());
			dummy2.StringField1 = "A";
			AssertEquals(dummy1.GetHashCode(), dummy2.GetHashCode());
			dummy1.DecimalField1 = 12m;
			AssertNotEquals(dummy1.GetHashCode(), dummy2.GetHashCode());
			dummy2.DecimalField1 = 12m;
			AssertEquals(dummy1.GetHashCode(), dummy2.GetHashCode());
		}

#if DEBUG
		[WTG.StaticAnalysis.Annotation.TypeFactoryAnnotationMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method called via Reflection from Enterprise.ReflectionTestDeadCodeTest.TestNoDeadCode()")]
		static IEnumerable<string> TypeFactoryAnnotation()
		{
			return new SubClassRetriever(typeof(AWBMessageBlock).Assembly, typeof(AWBMessageBlock)).Retrieve().Select(x => x.AssemblyQualifiedName);
		}
#endif

		sealed class AWBMessageBlockDummy : AWBMessageBlock
		{
			public AWBMessageBlockDummy()
			{
				SetupAdditionalMultiLineData("CargoDesc", 10, 3, CharType.Text);
			}

			/// <summary>
			/// 1  - LineIdentifier - BOB
			/// 2  - StringField1 - Mandatory
			/// 3  - Slant - Mandatory
			/// 4  - DateField1 - Optional
			/// 5  - Hyphen - Mandatory
			/// 6  - DecimalField1 - Mandatory
			/// 7  - ColumnIdentifier - Mandatory - C
			/// 8  - StringField2 - Conditional
			/// 9  - Slant - Conditional
			/// 10 - DecimalField2 - Optional
			/// 11 - CRLF - Mandatory
			/// 12 - Slant - Mandatory
			/// 13 - DateField2 - Mandatory
			/// 14 - Hyphen - Conditional
			/// 15 - StringField3 - Optional
			/// 16 - CRLF - Mandatory
			/// 17 - DecimalField3 - Mandatory
			/// 18 - DateField3 - Optional
			/// 19 - CRLF - Mandatory
			/// 20 - Additional StringDesc
			/// </summary>
			protected override void SetupSpecialFields()
			{
				AddSpecialField(1, StatusType.Mandatory, CharType.Alpha, ValueType.LineIdentifier, "BOB");
				AddSpecialField(3, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.Slant);
				AddSpecialField(5, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.Hyphen);
				AddSpecialField(7, StatusType.Mandatory, CharType.Alpha, ValueType.ColumnIdentifier, "C");
				AddSpecialField(9, StatusType.Conditional, CharType.Special, ValueType.Special, SpecialChars.Slant);
				AddSpecialField(11, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.CRLF);
				AddSpecialField(12, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.Slant);
				AddSpecialField(14, StatusType.Conditional, CharType.Special, ValueType.Special, SpecialChars.Hyphen);
				AddSpecialField(16, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.CRLF);
				AddSpecialField(19, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.CRLF);
			}

			/// 1  - LineIdentifier - BOB
			[AWBMessageBlockString(2, 1, 5, StatusType.Mandatory, CharType.Alpha, OnLengthViolation = LengthViolationAction.SetInvalidValue)]
			public ZString StringField1;
			/// 3  - Slant - Mandatory
			[AWBMessageBlockDate(4, StatusType.Optional, CharType.Numeric, "MMddyyyy")]
			public ZDate DateField1;
			/// 5  - Hyphen - Mandatory
			[AWBMessageBlockDecimal(6, 1, 8, StatusType.Mandatory, CharType.Numeric)]
			public ZDecimal DecimalField1;
			/// 7  - ColumnIdentifier - Mandatory - C
			[AWBMessageBlockString(8, 2, 10, StatusType.Conditional, CharType.AlphaNumeric, OnLengthViolation = LengthViolationAction.Substring)]
			public ZString StringField2;
			/// 9  - Slant - Conditional
			[AWBMessageBlockDecimal(10, 1, 8, StatusType.Optional, CharType.NumericWithDecimal)]
			public ZDecimal DecimalField2;
			/// 11 - CRLF - Mandatory
			/// 12 - Slant - Mandatory
			[AWBMessageBlockDate(13, StatusType.Mandatory, CharType.AlphaNumeric, "ddMMM")]
			public ZDate DateField2;
			/// 14 - Hyphen - Conditional
			[AWBMessageBlockString(15, 1, 5, StatusType.Optional, CharType.Text, OnLengthViolation = LengthViolationAction.SetInvalidValue)]
			public ZString StringField3;
			/// 16 - CRLF - Mandatory
			[AWBMessageBlockDecimal(17, 4, 4, StatusType.Mandatory, CharType.Numeric)]
			public ZDecimal DecimalField3;
			[AWBMessageBlockDate(18, StatusType.Optional, CharType.Text, "dd-MM-yy")]
			public ZDate DateField3;
			/// 19 - CRLF - Mandatory
			public ZString CargoDesc { get => AdditionalMultiLineDataValue; set => AdditionalMultiLineDataValue = value; }
		}

		sealed class AWBMessageBlockDummy2MandatoryOnly : AWBMessageBlock
		{
			protected override void SetupSpecialFields()
			{
				AddSpecialField(1, StatusType.Mandatory, CharType.Alpha, ValueType.LineIdentifier, "BOB");
				AddSpecialField(2, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.CRLF);
			}
		}

		sealed class AWBMessageBlockDummyWithColumns : AWBMessageBlock
		{
			public AWBMessageBlockDummyWithColumns()
			{
				StringField1 = StringField2 = StringField3 = StringField4 = ZString.Empty;
			}

			/// <summary>
			/// 1  - LineIdentifier - MUS
			/// 2  - Slant - Mandatory
			/// 3  - StringField1 - Mandatory
			/// 4  - Space - Mandatory
			/// 5  - ColumnIdentifier - Mandatory - A
			/// 6  - Hyphen - Mandatory
			/// 7  - StringField2 - Mandatory
			/// 8  - Space - Conditional
			/// 9  - ColumnIdentifier - Conditional - B
			/// 10 - Hyphen - Conditional
			/// 11 - StringField3 - Conditional
			/// 12 - Space - Conditional
			/// 13 - ColumnIdentifier - Conditional - C
			/// 14 - Hyphen - Conditional
			/// 15 - StringField4 - Conditional
			/// 16 - CRLF - Mandatory
			/// </summary>
			protected override void SetupSpecialFields()
			{
				AddSpecialField(1, StatusType.Mandatory, CharType.Alpha, ValueType.LineIdentifier, "MUS");
				AddSpecialField(2, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.Slant);
				AddSpecialField(4, StatusType.Mandatory, CharType.Special, ValueType.Special, " ");
				AddSpecialField(5, StatusType.Mandatory, CharType.Alpha, ValueType.ColumnIdentifier, "A");
				AddSpecialField(6, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.Hyphen);
				AddSpecialField(8, StatusType.Conditional, CharType.Special, ValueType.Special, " ");
				AddSpecialField(9, StatusType.Optional, CharType.Alpha, ValueType.ColumnIdentifier, "B");
				AddSpecialField(10, StatusType.Conditional, CharType.Special, ValueType.Special, SpecialChars.Hyphen);
				AddSpecialField(12, StatusType.Conditional, CharType.Special, ValueType.Special, " ");
				AddSpecialField(13, StatusType.Optional, CharType.Alpha, ValueType.ColumnIdentifier, "C");
				AddSpecialField(15, StatusType.Conditional, CharType.Special, ValueType.Special, SpecialChars.Hyphen);
				AddSpecialField(16, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.CRLF);
			}

			/// 1  - LineIdentifier - MUS
			/// 2  - Slant - Mandatory
			[AWBMessageBlockString(3, 4, 6, StatusType.Mandatory, CharType.Alpha)]
			public ZString StringField1;
			/// 4  - Space - Mandatory
			/// 5  - ColumnIdentifier - Mandatory - A
			/// 6  - Hyphen - Mandatory
			[AWBMessageBlockString(7, 4, 6, StatusType.Conditional, CharType.Alpha)]
			public ZString StringField2;
			/// 8  - Space - Conditional
			/// 9  - ColumnIdentifier - Conditional - B
			/// 10 - Hyphen - Conditional
			[AWBMessageBlockString(11, 4, 6, StatusType.Conditional, CharType.Alpha)]
			public ZString StringField3;
			/// 12 - Space - Conditional
			/// 13 - ColumnIdentifier - Conditional - C
			/// 14 - Hyphen - Conditional
			[AWBMessageBlockString(15, 4, 6, StatusType.Conditional, CharType.Alpha)]
			public ZString StringField4;
			/// 16 - CRLF - Mandatory
		}
	}
}
