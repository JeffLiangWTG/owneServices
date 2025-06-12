using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Core.Transforms.Helper;
using System.Diagnostics;
using Rhino.Mocks;
using CargoWise.eHub.DataAccess.Integration;
using System.Globalization;
using System.Threading;

namespace Tests
{
	[TestClass]
	public class DateMapperTests
	{
		static IReadOnlyList<CultureInfo> testCultures;

		DateMapper mapper;
		List<string> failures;

		[ClassInitialize]
		public static void ClassSetup(TestContext _)
		{
			testCultures = new List<CultureInfo>
			{
				CultureInfo.GetCultureInfo("en-US"),
				CultureInfo.GetCultureInfo("en-AU"),
				CultureInfo.InvariantCulture
			}.AsReadOnly();
		}

		[TestInitialize]
		public void TestSetup()
		{
			mapper = new DateMapper();
			failures = new List<string>();
		}

		[TestCleanup]
		public void TestCleanup()
		{
			if (failures.Any())
			{
				Assert.Fail(string.Join(Environment.NewLine, failures));
			}
		}

		[TestMethod]
		public void TestIsValidDate()
		{
			Assert.IsTrue(mapper.IsValidDate("2010-07-03", "yyyy-M-d"));
			Assert.IsTrue(mapper.IsValidDate("2010-7-3", "yyyy-M-d"));
			Assert.IsTrue(mapper.IsValidDate("2010-7-03", "yyyy-M-d"));
		}

		[TestMethod]
		public void TestConvertXmlDateString()
		{
			TestMapper("20100916", () => mapper.ConvertXmlDateString("2010-09-16T05:51:16.1830000+10:00", "yyyyMMdd"));
			TestMapper("2100916", () => mapper.ConvertXmlDateString("2010-09-16T05:51:16.1830000+10:00", "2yyMMdd"));
			TestMapper("20100916", () => mapper.ConvertXmlDateString("16-09-2010", "yyyyMMdd"));
			TestMapper("20100916", () => mapper.ConvertXmlDateString("16/09/2010", "yyyyMMdd"));
			TestMapper("", () => mapper.ConvertXmlDateString("09-16-2010", "yyyyMMdd"));
			TestMapper("", () => mapper.ConvertXmlDateString("09/16/2010", "yyyyMMdd"));
		}

		[TestMethod]
		public void TestConvertLocalXmlDateTimeStringToUTC()
		{
			TestMapper("2010-09-15", () => mapper.ConvertLocalXmlDateTimeStringToUTC("2010-09-16T05:51:16.1830000+10:00", "yyyy-MM-dd"));
			TestMapper("19:51:16", () => mapper.ConvertLocalXmlDateTimeStringToUTC("2010-09-16T05:51:16.1830000+10:00", "HH:mm:ss"));
			TestMapper("2010-09-16", () => mapper.ConvertLocalXmlDateTimeStringToUTC("2010-09-16T05:51:16.1830000", "yyyy-MM-dd"));
			TestMapper("", () => mapper.ConvertLocalXmlDateTimeStringToUTC("2010-09-16T05:51:16.1830000+AA:00", "yyyy-MM-dd"));
			TestMapper("2100915", () => mapper.ConvertLocalXmlDateTimeStringToUTC("2010-09-16T05:51:16.1830000+10:00", "2yyMMdd"));
			TestMapper("ABC", () => mapper.ConvertLocalXmlDateTimeStringToUTC("2010-09-16T05:51:16.1830000+AA:00", "yyyy-MM-dd", "ABC"));
			TestMapper("2010-09-12", () => mapper.ConvertLocalXmlDateTimeStringToUTC("2010-09-16T05:51:16.1830000", "99:00", "yyyy-MM-dd", ""));
		}

		[TestMethod]
		public void TestConvertToUTC()
		{
			TestMapper("2010-10-10T23:52:00", () => mapper.ConvertToUTCTime("2010-10-11T10:52:00.0000000+11:00"));
		}

		[TestMethod]
		public void TestCurrentDateWithTimeZone()
		{
			TestMapper(DateTime.Now.ToString("yyyy-MM-ddT00:00:00zzz"), () => mapper.CurrentDateWithTimeZone());
		}

		[TestMethod]
		public void TestCurrentDateTimeWithTimeZone()
		{
			TestMapper(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz"), () => mapper.CurrentDateTimeWithTimeZone());
		}

		[TestMethod]
		public void TestCurrentDateTimeUTC()
		{
			TestMapper(DateTime.UtcNow.ToString("yyyyMMddHHmmss"), () => mapper.CurrentDateTimeUTC("yyyyMMddHHmmss"));
		}

		[TestMethod]
		public void TestConvertToDateTimeString()
		{
			TestMapper("2011-03-07T00:00:00.0000000+11:00", () => mapper.ConvertToDateTimeString("2011-03-07T00:00:00+11:00"));
			TestMapper("2011-03-07T00:00:00.0000000", () => mapper.ConvertToDateTimeString("20110307", "yyyyMMdd"));
			TestMapper("2011-01-09T01:55:00.0000000", () => mapper.ConvertToDateTimeString("20110109", "yyyyMMdd", "1:55 AM", "h:mmtt"));
			TestMapper("2011-01-09T01:55:00.0000000", () => mapper.ConvertToDateTimeString("20110109", "yyyyMMdd", "1:55", "h:mm"));
			TestMapper("2011-01-19T16:20:00.0000000", () => mapper.ConvertToDateTimeString("20110119", "yyyyMMdd", "16:20", "H:mm"));
			TestMapper("7-Mar-11 12:00 AM", () => mapper.ConvertToDateTimeString("20110307", "yyyyMMdd", "", "", "d-MMM-yy h:mm tt"));
		}

		[TestMethod]
		public void TestFormatXmlDateTime()
		{
			TestMapper("20110221", () => mapper.FormatXmlDateTime("2011-02-21T17:16:56.713", "yyyyMMdd"));
			TestMapper("17165671", () => mapper.FormatXmlDateTime("2011-02-21T17:16:56.713", "HHmmssFF"));
			TestMapper("171656", () => mapper.FormatXmlDateTime("2011-02-21T17:16:56.006", "HHmmssFF"));
			TestMapper("171650", () => mapper.FormatXmlDateTime("2011-02-21T17:16:50", "HHmmssFF"));
			TestMapper("1716", () => mapper.FormatXmlDateTime("2011-02-21T17:16:50", "HHmm"));
			TestMapper("171633", () => mapper.FormatXmlDateTime("2011-02-21T17:16:33", "HHmmss"));

			TestMapper("022111", () => mapper.FormatXmlDateTime("2011-02-21", "MMddyy"));
			TestMapper("022111", () => mapper.FormatXmlDateTime("21/02/2011", "MMddyy"));
			TestMapper("022111", () => mapper.FormatXmlDateTime("21-02-2011", "MMddyy"));

			TestMapperException<FormatException>(() => mapper.FormatXmlDateTime("02/21/2011", "MMddyy"));
			TestMapperException<FormatException>(() => mapper.FormatXmlDateTime("02-21-2011", "MMddyy"));
			TestMapper("", () => mapper.FormatXmlDateTime("", "yyyyMMdd"));
			TestMapper("", () => mapper.FormatXmlDateTime(" ", "yyyyMMdd"));
			TestMapper("", () => mapper.FormatXmlDateTime(null, "yyyyMMdd"));
		}

		[TestMethod]
		public void TestConvertToXmlDatePartOnly()
		{
			TestMapper("1900-01-01", () => mapper.ConvertToXmlDatePartOnly("19000101", "yyyyMMdd"));
			TestMapper("2011-04-04", () => mapper.ConvertToXmlDatePartOnly("20110404", "yyyyMMdd"));
		}

		[TestMethod]
		public void TestConvertUTCToLocalTimeByUNLOCO()
		{
			var mockRepository = new MockRepository();

			var transformAccessor = mockRepository.StrictMock<ITransformAccessor>();
			Expect.Call(transformAccessor.CallActionProcedure("CalculateTimeZoneOffset", "@offset", new string[] { "@UNLOCO", "AUSYD", "@localtime", "2015-10-20T20:00:00" }))
				.Return("+09:00")
				.Repeat.Any();
			Expect.Call(transformAccessor.CallActionProcedure("CalculateTimeZoneOffset", "@offset", new string[] { "@UNLOCO", "USLAX", "@localtime", "2015-10-20T20:00:00" }))
				.Return("-09:00")
				.Repeat.Any();

			var dateMapper = mockRepository.PartialMock<DateMapper>();
			Expect.Call(dateMapper.GetTransformAccessor()).Return(transformAccessor).Repeat.Any();

			mockRepository.ReplayAll();

			TestMapper("2015-10-21T05:00:00", () => dateMapper.ConvertUTCToLocalTimeByUNLOCO("2015-10-20T20:00:00Z", "AUSYD"));
			TestMapper("2015-10-20T11:00:00", () => dateMapper.ConvertUTCToLocalTimeByUNLOCO("2015-10-20T20:00:00+0000", "USLAX"));
			TestMapper("2015-09-16T07:16:00", () => dateMapper.ConvertUTCToLocalTimeByUNLOCO("2015-09-16T07:16+0530", "USLAX"));
			TestMapper("2015-10-20T20:00:00", () => dateMapper.ConvertUTCToLocalTimeByUNLOCO("2015-10-20T20:00:00", "USLAX"));
			TestMapper(string.Empty, () => dateMapper.ConvertUTCToLocalTimeByUNLOCO("BLAHHHH", "USLAX"));
			TestMapper(string.Empty, () => dateMapper.ConvertUTCToLocalTimeByUNLOCO("BLAHHHH+00:00", "USLAX"));
		}

		private void TestMapper(string expectedResult, Func<string> testFunction)
		{
			TestMapperWithCulture(expectedResult, testFunction);

			foreach (var testCulture in testCultures)
			{
				TestMapperWithCulture(expectedResult, testFunction, testCulture);
			}
		}

		private void TestMapperWithCulture(
			string expectedResult,
			Func<string> testFunction,
			CultureInfo cultureInfo = null)
		{
			var savedInfo = CultureInfo.CurrentCulture;
			var testCulture = cultureInfo ?? CultureInfo.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = testCulture;

			try
			{
				Assert.AreEqual(expectedResult, testFunction.Invoke(),
					"Should work as expected under '{0}' culture", testCulture.DisplayName);
			}
			catch (AssertFailedException ex)
			{
				failures.Add(ex.Message);
			}
			catch (Exception ex)
			{
				failures.Add("---- Test Failure ----");
				failures.Add(ex.ToString());
				failures.Add("----------------------");
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = savedInfo;
			}
		}

		private void TestMapperException<TException>(Func<string> testFunction)
			where TException : Exception
		{
			TestMapperExceptionWithCulture<TException>(testFunction);

			foreach (var testCulture in testCultures)
			{
				TestMapperExceptionWithCulture<TException>(testFunction, testCulture);
			}
		}

		private void TestMapperExceptionWithCulture<TException>(
				Func<string> testFunction,
				CultureInfo cultureInfo = null)
			where TException : Exception
		{
			var savedInfo = CultureInfo.CurrentCulture;
			var testCulture = cultureInfo ?? CultureInfo.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = testCulture;

			try
			{
				testFunction.Invoke();
				failures.Add(string.Format(
					"Expected exception of type {0} under '{1}' culture.", 
					typeof(TException), 
					testCulture.DisplayName));
			}
			catch (Exception ex)
			{
				if (ex is TException) 
				{ 
					return; // Ignore expected exception
				}

				failures.Add("---- Test Failure ----");
				failures.Add(ex.ToString());
				failures.Add("----------------------");
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = savedInfo;
			}
		}
	}
}
