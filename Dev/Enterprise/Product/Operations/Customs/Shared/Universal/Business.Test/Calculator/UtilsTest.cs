using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	public class UtilsTest : TestCase
	{
		public void TestSafeGetString()
		{
			var mockDataReader = new Mock<IDataReader>();
			mockDataReader.SetupSequence(x => x.Read()).Returns(true).Returns(false);
			mockDataReader.Setup(x => x.FieldCount).Returns(3);
			mockDataReader.Setup(x => x.GetName(0)).Returns("Test_PK");
			mockDataReader.Setup(x => x.GetValue(0)).Returns(Guid.NewGuid());
			mockDataReader.Setup(x => x.GetName(1)).Returns("Test_Prop1");
			mockDataReader.Setup(x => x.GetValue(1)).Returns(DBNull.Value);
			mockDataReader.Setup(x => x.GetOrdinal("Test_Prop1")).Returns(1);
			mockDataReader.Setup(x => x.IsDBNull(1)).Returns(true);
			mockDataReader.Setup(x => x.GetName(2)).Returns("Test_Prop2");
			mockDataReader.Setup(x => x.GetValue(2)).Returns("Test string");
			mockDataReader.Setup(x => x.IsDBNull(2)).Returns(false);
			mockDataReader.Setup(x => x.GetString(2)).Returns("Test string");
			mockDataReader.Setup(x => x.GetOrdinal("Test_Prop2")).Returns(2);
			var reader = mockDataReader.Object;
			reader.Read();
			CombineAssertions(() =>
			{
				AssertEquals(string.Empty, reader.SafeGetString("Test_Prop1"));
				AssertEquals("Test string", reader.SafeGetString("Test_Prop2"));
			}

			);
		}

		public void TestGetIntegerNumber()
		{
			var errorListener = new FormulaErrorListener();
			AssertEquals(154, Utils.GetIntegerNumber("154", errorListener));
			AssertEquals(0, errorListener.Errors.Count());
			AssertEquals(0, Utils.GetIntegerNumber("154.234", errorListener));
			AssertEquals(1, errorListener.Errors.Count());
			AssertEquals(FormulaVisitErrorType.SyntaxError, errorListener.Errors.FirstOrDefault().Type);
			AssertEquals("Failed to convert \"154.234\" to Integer", errorListener.Errors.FirstOrDefault().ErrorMessage);
		}

		public void TestAddNewKeyOrAccumulateValue()
		{
			var dictionary = new Dictionary<string, decimal>();
			dictionary.AddNewKeyOrAccumulateValue("A", 11);
			AssertEquals("Dictionary[A] initial value", 11M, dictionary["A"]);
			dictionary.AddNewKeyOrAccumulateValue("B", 0);
			AssertEquals("Dictionary[B] initial value", 0M, dictionary["B"]);
			dictionary.AddNewKeyOrAccumulateValue("A", 2);
			AssertEquals("Dictionary[A] accumulated value", 13M, dictionary["A"]);
			dictionary.AddNewKeyOrAccumulateValue("B", 5);
			AssertEquals("Dictionary[B] accumulated value", 5M, dictionary["B"]);
		}

		public void TestToIsoDateOnlyNumericValue()
		{
			CombineAssertions(() =>
			{
				AssertEquals("DateTime.MinValue", 10101, DateTime.MinValue.ToIsoDateOnlyNumericValue());
				AssertEquals("DateTime.MaxValue", 99991231, DateTime.MaxValue.ToIsoDateOnlyNumericValue());
				AssertEquals("2020-03-07 23:59:59.999", 20200307, new DateTime(2020, 3, 7, 23, 59, 59, 999).ToIsoDateOnlyNumericValue());
				AssertEquals("932-02-29", 9320229, new DateTime(932, 2, 29).ToIsoDateOnlyNumericValue());
			}

			);
		}

		public void TestGetXmlForValueList()
		{
			AssertEquals("<v></v><v>&lt;</v><v>A</v><v>B</v><v>C</v>", Utils.GetXmlForValueList(new[] { "A", "B", "A", "<", null, "", "C" }));
		}

		public void TestGetTvpTableForValueList()
		{
			CombineAssertions(() =>
			{
				var tbl = Utils.GetTvpTableForValueList(new[] { "A", "B", "A", "<", null, "", "C" });
				AssertEquals("TVP type", typeof(DataTable), tbl.GetType());
				AssertContainsExactElementsInExactOrder("Ordered DataTable", new[] { "", "<", "A", "B", "C" }, tbl.Rows.Cast<DataRow>().Select(x => x[0].ToString()));

				tbl = Utils.GetTvpTableForValueList(null);
				AssertContainsExactElementsInExactOrder("Empty DataTable when values is null", new[] { "" }, tbl.Rows.Cast<DataRow>().Select(x => x[0].ToString()));
			});
		}
	}
}
