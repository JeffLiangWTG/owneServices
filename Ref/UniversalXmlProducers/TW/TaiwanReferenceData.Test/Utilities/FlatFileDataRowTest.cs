using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	public class FlatFileDataRowTest
	{
		public class FlatFileDataRowForTest : FlatFileDataRow
		{
			public FlatFileDataRowForTest(int fieldCount) : base(fieldCount, string.Empty) { }

			public new void SetFieldProperties(string lineData)
			{
				originalLine = lineData;
				base.SetFieldProperties(lineData);
			}

			public new List<FlatFileFieldProperty> SchemaList => base.SchemaList;
		}

		[Test]
		public void TestThis()
		{
			Assert.AreEqual("AUD", FlatFileDataRowTestData[0]);
			Assert.AreEqual("2018", FlatFileDataRowTestData[1]);
			Assert.AreEqual("07", FlatFileDataRowTestData[2]);
			Assert.AreEqual("3", FlatFileDataRowTestData[3]);
			Assert.AreEqual("22.45", FlatFileDataRowTestData[4]);
			Assert.AreEqual("22.68", FlatFileDataRowTestData[5]);
		}

		[Test]
		public void TestGetField()
		{
			Assert.AreEqual("AUD", FlatFileDataRowTestData.GetField(0));
			Assert.AreEqual("2018", FlatFileDataRowTestData.GetField(1));
			Assert.AreEqual("07", FlatFileDataRowTestData.GetField(2));
			Assert.AreEqual("3", FlatFileDataRowTestData.GetField(3));
			Assert.AreEqual("22.45", FlatFileDataRowTestData.GetField(4));
			Assert.AreEqual("22.68", FlatFileDataRowTestData.GetField(5));
		}

		[Test]
		public void TestGetFieldAsInt()
		{
			var oldCurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			var oldCurrentCulture = Thread.CurrentThread.CurrentCulture;

			try
			{
				Assert.AreEqual(7, FlatFileDataRowTestData.GetFieldAsInt(2));
				Assert.AreEqual(3, FlatFileDataRowTestData.GetFieldAsInt(3));
				Assert.AreEqual(0, FlatFileDataRowTestData.GetFieldAsInt(4));
				Assert.AreEqual(0, FlatFileDataRowTestData.GetFieldAsInt(0));
				Assert.AreEqual("22.68", FlatFileDataRowTestData.GetField(5));
				Assert.AreEqual(0, FlatFileDataRowTestData.GetFieldAsInt(5));

				SetOtherCurrentThread();

				int result;
				int.TryParse(FlatFileDataRowTestData.GetField(2), out result);
				Assert.AreEqual(7, result);
				int.TryParse(FlatFileDataRowTestData.GetField(3), out result);
				Assert.AreEqual(3, result);
				int.TryParse(FlatFileDataRowTestData.GetField(4), out result);
				Assert.AreEqual(0, result);
				int.TryParse(FlatFileDataRowTestData.GetField(0), out result);
				Assert.AreEqual(0, result);
				int.TryParse(FlatFileDataRowTestData.GetField(5), out result);
				Assert.AreEqual(0, result);

				Assert.AreEqual(7, FlatFileDataRowTestData.GetFieldAsInt(2));
				Assert.AreEqual(3, FlatFileDataRowTestData.GetFieldAsInt(3));
				Assert.AreEqual(0, FlatFileDataRowTestData.GetFieldAsInt(4));
				Assert.AreEqual(0, FlatFileDataRowTestData.GetFieldAsInt(0));
				Assert.AreEqual(0, FlatFileDataRowTestData.GetFieldAsInt(5));
			}
			catch { }
			finally
			{
				Thread.CurrentThread.CurrentUICulture = oldCurrentUICulture;
				Thread.CurrentThread.CurrentCulture = oldCurrentCulture;
			}
		}

		[Test]
		public void TestGetFixedFieldAsDecimal()
		{
			var oldCurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			var oldCurrentCulture = Thread.CurrentThread.CurrentCulture;

			try
			{
				Assert.AreEqual(0.2245m, FlatFileDataRowTestData.GetFixedFieldAsDecimal(4, 2));
				Assert.AreEqual(22.68m, FlatFileDataRowTestData.GetFixedFieldAsDecimal(5, 0));
				Assert.AreEqual(226.8m, FlatFileDataRowTestData.GetFixedFieldAsDecimal(5, -1));

				SetOtherCurrentThread();

				decimal result;
				decimal.TryParse(FlatFileDataRowTestData.GetField(4), out result);
				Assert.AreEqual(0, result);
				decimal.TryParse(FlatFileDataRowTestData.GetField(5), out result);
				Assert.AreEqual(0, result);

				Assert.AreEqual(0.2245m, FlatFileDataRowTestData.GetFixedFieldAsDecimal(4, 2));
				Assert.AreEqual(22.68m, FlatFileDataRowTestData.GetFixedFieldAsDecimal(5, 0));
				Assert.AreEqual(226.8m, FlatFileDataRowTestData.GetFixedFieldAsDecimal(5, -1));
			}
			catch { }
			finally
			{
				Thread.CurrentThread.CurrentUICulture = oldCurrentUICulture;
				Thread.CurrentThread.CurrentCulture = oldCurrentCulture;
			}
		}

		[Test]
		public void TestGetFieldAsDecimal()
		{
			var oldCurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			var oldCurrentCulture = Thread.CurrentThread.CurrentCulture;
			try
			{
				Assert.AreEqual(22.45m, FlatFileDataRowTestData.GetFieldAsDecimal(4));
				Assert.AreEqual(22.68m, FlatFileDataRowTestData.GetFieldAsDecimal(5));
				Assert.AreEqual("07", FlatFileDataRowTestData.GetField(2));
				Assert.AreEqual(7, FlatFileDataRowTestData.GetFieldAsDecimal(2));

				SetOtherCurrentThread();

				decimal result;
				decimal.TryParse(FlatFileDataRowTestData.GetField(4), out result);
				Assert.AreEqual(0, result);
				decimal.TryParse(FlatFileDataRowTestData.GetField(5), out result);
				Assert.AreEqual(0, result);
				decimal.TryParse(FlatFileDataRowTestData.GetField(2), out result);
				Assert.AreEqual(7, result);

				Assert.AreEqual(22.45m, FlatFileDataRowTestData.GetFieldAsDecimal(4));
				Assert.AreEqual(22.68m, FlatFileDataRowTestData.GetFieldAsDecimal(5));
				Assert.AreEqual(7, FlatFileDataRowTestData.GetFieldAsDecimal(2));
			}
			catch { }
			finally
			{
				Thread.CurrentThread.CurrentUICulture = oldCurrentUICulture;
				Thread.CurrentThread.CurrentCulture = oldCurrentCulture;
			}
		}

		void SetOtherCurrentThread()
		{
			var newCulture = CultureInfo.CreateSpecificCulture("fr-FR");
			Thread.CurrentThread.CurrentUICulture = newCulture;
			Thread.CurrentThread.CurrentCulture = newCulture;
		}

		[Test]
		public void TestGetFieldAsDateTime()
		{
			var oldCurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			var oldCurrentCulture = Thread.CurrentThread.CurrentCulture;

			try
			{
				Assert.AreEqual(new DateTime(2018, 07, 19), FlatFileDataRowTestData.GetFieldAsDateTime(6, "yyyyMMdd"));
				Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), FlatFileDataRowTestData.GetFieldAsDateTime(7, "yyyyMMdd"));
				Assert.AreEqual(DateTime.MinValue, FlatFileDataRowTestData.GetFieldAsDateTime(8, "yyyyMMdd"));

				var newCulture = CultureInfo.CreateSpecificCulture("fr-FR");
				Thread.CurrentThread.CurrentUICulture = newCulture;
				Thread.CurrentThread.CurrentCulture = newCulture;
				Assert.AreEqual(new DateTime(2018, 07, 19), FlatFileDataRowTestData.GetFieldAsDateTime(6, "yyyyMMdd"));
				Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), FlatFileDataRowTestData.GetFieldAsDateTime(7, "yyyyMMdd"));
				Assert.AreEqual(DateTime.MinValue, FlatFileDataRowTestData.GetFieldAsDateTime(8, "yyyyMMdd"));
			}
			catch { }
			finally
			{
				Thread.CurrentThread.CurrentUICulture = oldCurrentUICulture;
				Thread.CurrentThread.CurrentCulture = oldCurrentCulture;
			}
		}

		[Test]
		public void TestSetField()
		{
			var flatFileDataRowForTest1 = new FlatFileDataRowForTest(9);
			string value = "milotest";
			flatFileDataRowForTest1.SetField(0, value);
			Assert.AreEqual(value, flatFileDataRowForTest1.GetField(0));

			flatFileDataRowForTest1.SetField(new FlatFileFieldProperty(1, 3), value);
			Assert.AreEqual(value.Substring(0, 3), flatFileDataRowForTest1.GetField(1));

			TestDelegate testDelegate = () => flatFileDataRowForTest1.SetField(new FlatFileFieldProperty(2, 10), value);
			Assert.That(testDelegate, Throws.TypeOf<ArgumentOutOfRangeException>());

			flatFileDataRowForTest1.SetField(new FlatFileFieldProperty(2, 3), 55);
			Assert.AreEqual("055", flatFileDataRowForTest1.GetField(2));
			Assert.AreEqual(55, flatFileDataRowForTest1.GetFieldAsInt(2));

			flatFileDataRowForTest1.SetField(new FlatFileFieldProperty(2, 3), 13.465m);
			Assert.AreEqual("13.465", flatFileDataRowForTest1.GetField(2));
			Assert.AreEqual(13.465m, flatFileDataRowForTest1.GetFieldAsDecimal(2));

			flatFileDataRowForTest1.SetFixedDecimalField(new FlatFileFieldProperty(2, 3), 13.465m, 1);
			Assert.AreEqual(134.65m, flatFileDataRowForTest1.GetFieldAsDecimal(2));
			Assert.AreEqual(13.465m, flatFileDataRowForTest1.GetFixedFieldAsDecimal(2, 1));

			DateTime currentTime = new DateTime(2018, 07, 20, 02, 10, 21);
			flatFileDataRowForTest1.SetField(new FlatFileFieldProperty(2, 3), currentTime, "yyyyMMddHHmmss");
			Assert.AreEqual(currentTime, flatFileDataRowForTest1.GetFieldAsDateTime(2, "yyyyMMddHHmmss"));

			flatFileDataRowForTest1.SetField(2, currentTime, "yyyyMMddHHmmss");
			Assert.AreEqual(currentTime, flatFileDataRowForTest1.GetFieldAsDateTime(2, "yyyyMMddHHmmss"));
		}

		[Test]
		[SetCulture("en-US")]
		public void TestSetFieldWhenUS()
		{
			TestSetField();
		}

		[Test]
		[SetCulture("fr-FR")]
		public void TestSetFieldWhenFR()
		{
			TestSetField();
		}

		[Test]
		public void TestLineData()
		{
			Assert.AreEqual("AUD201807322.45     22.68     201807199999999920781306", FlatFileDataRowTestData.LineData);
		}

		public FlatFileDataRowForTest FlatFileDataRowTestData
		{
			get
			{
				if (flatFileDataRowForTest == null)
				{
					flatFileDataRowForTest = new FlatFileDataRowForTest(9);
					flatFileDataRowForTest.SchemaList.Add(new FlatFileFieldProperty(0, 3));
					flatFileDataRowForTest.SchemaList.Add(new FlatFileFieldProperty(1, 4));
					flatFileDataRowForTest.SchemaList.Add(new FlatFileFieldProperty(2, 2));
					flatFileDataRowForTest.SchemaList.Add(new FlatFileFieldProperty(3, 1));
					flatFileDataRowForTest.SchemaList.Add(new FlatFileFieldProperty(4, 10));
					flatFileDataRowForTest.SchemaList.Add(new FlatFileFieldProperty(5, 10));
					flatFileDataRowForTest.SchemaList.Add(new FlatFileFieldProperty(6, 8));
					flatFileDataRowForTest.SchemaList.Add(new FlatFileFieldProperty(7, 8));
					flatFileDataRowForTest.SchemaList.Add(new FlatFileFieldProperty(8, 8));
					flatFileDataRowForTest.SetFieldProperties("AUD201807322.45     22.68     201807199999999920781306");
				}
				return flatFileDataRowForTest;
			}
		}

		private FlatFileDataRowForTest flatFileDataRowForTest;
	}
}
