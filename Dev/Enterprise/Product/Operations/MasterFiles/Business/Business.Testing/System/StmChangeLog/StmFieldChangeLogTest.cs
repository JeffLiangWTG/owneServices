using System;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(StmFieldChangeLog))]
	sealed class StmFieldChangeLogTest : NonPersistentBusinessObjectTestCase
	{
		public void TestParseFormat_ForString()
		{
			TestParseFormat(DummyBizoSchema.Z0_VarCharMax.Name, (ZString)"Old|Value\r\n", (ZString)"\r\nNew Value|");
			TestParseFormat(DummyBizoSchema.Z0_VarCharMax.Name, (ZString)"", (ZString)"");
		}

		public void TestParseFormat_ForDateTime()
		{
			TestParseFormat(DummyBizoSchema.Z0_Date.Name, new ZDateTime(2000, 1, 2), new ZDateTime(2005, 1, 2, 3, 4, 5));
		}

		[ExpectException(typeof(FormatException))]
		public void TestParseInvalidEmptyString()
		{
			StmFieldChangeLog parsedChangeLog = new StmFieldChangeLog(Factory);
			parsedChangeLog.Parse("");
		}

		[ExpectException(typeof(FormatException))]
		public void TestParseInvalidPropertyName()
		{
			StmFieldChangeLog parsedChangeLog = new StmFieldChangeLog(Factory);
			parsedChangeLog.Parse("InvalidPropertyName|x|y");
		}

		[ExpectException(typeof(FormatException))]
		public void TestParseInvalidParamCount()
		{
			StmFieldChangeLog parsedChangeLog = new StmFieldChangeLog(Factory);
			parsedChangeLog.Parse("Z0_Number");
		}

		[ExpectException(typeof(FormatException))]
		public void TestParseInvalidParamCount2()
		{
			StmFieldChangeLog parsedChangeLog = new StmFieldChangeLog(Factory);
			parsedChangeLog.Parse("Z0_Number|2");
		}

		[ExpectException(typeof(FormatException))]
		public void TestParseInvalidValue()
		{
			StmFieldChangeLog parsedChangeLog = new StmFieldChangeLog(Factory);
			parsedChangeLog.Parse("Z0_Number|2|string instead of number");
		}

		void TestParseFormat(string propertyName, IZType oldValue, IZType newValue)
		{
			StmFieldChangeLog changeLog = new StmFieldChangeLog(Factory);
			changeLog.PropertyName = propertyName;
			changeLog.OldValue = oldValue;
			changeLog.NewValue = newValue;
			StringBuilder formattedChangeLog = new StringBuilder();
			changeLog.Format(formattedChangeLog);

			StmFieldChangeLog parsedChangeLog = new StmFieldChangeLog(Factory);
			parsedChangeLog.Parse(formattedChangeLog.ToString());
			AssertEquals("PropertyName", propertyName, parsedChangeLog.PropertyName);
			AssertEquals("OldValue", oldValue, parsedChangeLog.OldValue);
			AssertEquals("NewValue", newValue, parsedChangeLog.NewValue);
		}
	}
}
