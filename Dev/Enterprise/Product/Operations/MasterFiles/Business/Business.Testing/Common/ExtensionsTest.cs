using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ExtensionsTest : TransactionedTestCase
	{
		public void TestNotifyUserIfRequired()
		{
			var isInteractive = Globals.IsUserInteractive;

			var unitTestUserNotification = Globals.Message as UnitTestUserNotification;

			var innerException = new WebException("WebException Message", null, WebExceptionStatus.SecureChannelFailure, null);
			var exception = new HttpRequestException("HttpRequestException Message", innerException);

			AssertContainsExactElementsInAnyOrder("Precondition: No error message", Array.Empty<string>(), unitTestUserNotification.PreviousMessages.Where(x => x.WasError));

			Globals.IsUserInteractive = true;
			exception.NotifyUserIfRequired("http://www.wisetechglobal.com");
			AssertContainsExactElementsInAnyOrder
			(
				"GIVEN IsUserInteractive=TRUE THEN should show error",
				new[] { "Could not establish secure SSL/TLS connection with WiseTech Global service at 'http://www.wisetechglobal.com'. Please contact your system administrator." },
				unitTestUserNotification.PreviousMessages.Where(x => x.WasError).Select(x => x.Text)
			);

			unitTestUserNotification.ClearMessages();
			Globals.IsUserInteractive = false;
			exception.NotifyUserIfRequired("http://www.wisetechglobal.com");
			AssertContainsExactElementsInAnyOrder
			(
				"GIVEN IsUserInteractive=FALSE(ServiceTask) THEN should not show error BECAUSE UnattendedUserNotification tries to send email by saving email record which isn't allowed in ServiceTask",
				Array.Empty<string>(),
				unitTestUserNotification.PreviousMessages.Where(x => x.WasError).Select(x => x.Text)
			);

			unitTestUserNotification.ClearMessages();
			Globals.IsUserInteractive = true;
			var authenticationException = new AuthenticationException("The remote certificate was rejected by the provided RemoteCertificateValidationCallback.");
			exception = new HttpRequestException("HttpRequestException Message", authenticationException);
			exception.NotifyUserIfRequired("http://www.wisetechglobal.com");
			AssertContainsExactElementsInAnyOrder
			(
				"GIVEN IsUserInteractive=TRUE THEN should show error",
				new[] { "Failed to validate the SSL/TLS certificate of WiseTech Global service at 'http://www.wisetechglobal.com'. Please contact your system administrator." },
				unitTestUserNotification.PreviousMessages.Where(x => x.WasError).Select(x => x.Text)
			);

			Globals.IsUserInteractive = isInteractive;
		}

		public void TestGetSingleValueOrManyText_EmptyList()
		{
			var testEnumerable = Enumerable.Empty<Child>();

			var result = testEnumerable.GetSingleValueOrManyText(o => o.Value, "Many");

			AssertEquals(string.Empty, result);

			result = testEnumerable.GetSingleValueOrManyText(o => o.Value);

			AssertEquals(string.Empty, result);
		}

		public void TestGetSingleValueOrManyText_SingleResult()
		{
			var childObject = new Child() { Key = "testKey", Value = "testValue" };
			var testEnumerable = new List<Child>() { childObject };

			var result = testEnumerable.GetSingleValueOrManyText(o => o.Value, "Many");

			AssertEquals("testValue", result);

			// Tests the default many return overload
			result = testEnumerable.GetSingleValueOrManyText(o => o.Value);

			AssertEquals("testValue", result);
		}

		public void TestGetSingleValueOrManyText_ManyResult()
		{
			var childObject1 = new Child() { Key = "testKey1", Value = "testValue1" };
			var childObject2 = new Child() { Key = "testKey2", Value = "testValue2" };
			var testEnumerable = new List<Child>() { childObject1, childObject2 };

			var result = testEnumerable.GetSingleValueOrManyText(o => o.Value, "Many");

			AssertEquals("Many", result);

			// Tests the default many return overload
			result = testEnumerable.GetSingleValueOrManyText(o => o.Value);

			AssertEquals("Many", result);
		}

		public void TestGetSingleValueOrManyText_DeDuplicationForeignKeyEmptyList()
		{
			var testEnumerable = Enumerable.Empty<Parent>();

			var result = testEnumerable.GetSingleValueOrManyText(o => o.Child.Value, o => o.Child.Key, "Many");

			AssertEquals(string.Empty, result);

			result = testEnumerable.GetSingleValueOrManyText(o => o.Child.Value, o => o.Child.Key);

			AssertEquals(string.Empty, result);
		}

		public void TestGetSingleValueOrManyText_DeDuplicationForeignKeySingleResult()
		{
			var childObject = new Child() { Key = "testKey", Value = "testValue" };
			var parentObject = new Parent { Child = childObject };
			var testEnumerable = new List<Parent>() { parentObject };

			var result = testEnumerable.GetSingleValueOrManyText(o => o.Child.Value, o => o.Child.Key, "Many");

			AssertEquals("testValue", result);

			// Tests the default many return overload
			result = testEnumerable.GetSingleValueOrManyText(o => o.Child.Value, o => o.Child.Key);

			AssertEquals("testValue", result);
		}

		public void TestGetSingleValueOrManyText_DeDuplicationForeignKeyManyResult()
		{
			var childObject1 = new Child() { Key = "testKey1", Value = "testValue1" };
			var parentObject1 = new Parent { Child = childObject1 };
			var childObject2 = new Child() { Key = "testKey2", Value = "testValue2" };
			var parentObject2 = new Parent { Child = childObject2 };
			var testEnumerable = new List<Parent>() { parentObject1, parentObject2 };

			var result = testEnumerable.GetSingleValueOrManyText(o => o.Child.Value, o => o.Child.Key, "Many");

			AssertEquals("Many", result);

			// Tests the default many return overload
			result = testEnumerable.GetSingleValueOrManyText(o => o.Child.Value, o => o.Child.Key);

			AssertEquals("Many", result);
		}

		public void TestGetSingleValueOrManyText_DeDuplicationForeignKeyAttributeConstraintEmptyList()
		{
			var testEnumerable = Enumerable.Empty<Parent>();

			var result = testEnumerable.GetSingleValueOrManyText(o => o.Child.Value, o => o.Child.Key, o => o.Child.Flag, "Many");

			AssertEquals(string.Empty, result);

			result = testEnumerable.GetSingleValueOrManyText(o => o.Child.Value, o => o.Child.Key, o => o.Child.Flag);

			AssertEquals(string.Empty, result);
		}

		public void TestGetSingleValueOrManyText_DeDuplicationForeignKeyAttributeConstraintSingleResult()
		{
			var childObject1 = new Child() { Key = "testKey", Value = "testValue", Flag = true };
			var parentObject1 = new Parent { Child = childObject1 };
			var testEnumerable = new List<Parent>() { parentObject1 };

			var result = testEnumerable.GetSingleValueOrManyText(o => o.Child.Value, o => o.Child.Key, o => o.Child.Flag, "Many");

			AssertEquals("testValue", result);

			var childObject2 = new Child() { Key = "testKey", Value = "testValue", Flag = false };
			var parentObject2 = new Parent { Child = childObject2 };
			testEnumerable = new List<Parent>() { parentObject1, parentObject2 };

			result = testEnumerable.GetSingleValueOrManyText(o => o.Child.Value, o => o.Child.Key, o => o.Child.Flag, "Many");

			AssertEquals("testValue", result);

			// Tests the default many return overload
			result = testEnumerable.GetSingleValueOrManyText(o => o.Child.Value, o => o.Child.Key, o => o.Child.Flag);

			AssertEquals("testValue", result);
		}

		public void TestGetSingleValueOrManyText_DeDuplicationForeignKeyAttributeConstraintManyResult()
		{
			var childObject1 = new Child() { Key = "testKey1", Value = "testValue1", Flag = true };
			var parentObject1 = new Parent { Child = childObject1 };
			var childObject2 = new Child() { Key = "testKey2", Value = "testValue2", Flag = true };
			var parentObject2 = new Parent { Child = childObject2 };
			var testEnumerable = new List<Parent>() { parentObject1, parentObject2 };

			var result = testEnumerable.GetSingleValueOrManyText(o => o.Child.Value, o => o.Child.Key, o => o.Child.Flag, "Many");

			AssertEquals("Many", result);

			// Tests the default many return overload
			result = testEnumerable.GetSingleValueOrManyText(o => o.Child.Value, o => o.Child.Key, o => o.Child.Flag);

			AssertEquals("Many", result);
		}

		public void TestLoggerWithPrefix()
		{
			var logger = new DummyLogger();
			var prefixedLogger = logger.WithPrefix("Hello ");
			prefixedLogger.Information("World!");

			AssertEquals("Hello World!", logger.Logs[0].Item2);
		}

		public void TestLoggerDistinct()
		{
			var logger = new DummyLogger();
			var distinctLogger = logger.Distinct();
			distinctLogger.Error("101");        //x
			distinctLogger.Warning("201");      //x
			distinctLogger.Error("201");        //x
			distinctLogger.Error("101");
			distinctLogger.Information("101");  //x
			distinctLogger.Information("101");
			distinctLogger.Warning("201");
			distinctLogger.Error("201");

			var expected = new Tuple<LogType, string>[] {
				Tuple.Create(LogType.Error, "101"),
				Tuple.Create(LogType.Warning, "201"),
				Tuple.Create(LogType.Error, "201"),
				Tuple.Create(LogType.Information, "101")
			};

			AssertContainsExactElementsInAnyOrder(expected, logger.Logs);
		}

		public void MakeDebug()
		{
			var dummyLogger = new DummyLogger();
			var debugLogger = dummyLogger.DebugOnly();
			debugLogger.Error("Critical failure, everyone doomed!");

			AssertEquals("Hello World!", dummyLogger.Logs[0].Item1);
			AssertEquals(LogType.Debug, dummyLogger.Logs[0].Item2);

			dummyLogger.Logs.Clear();
			dummyLogger.Error("Critical failure, everyone doomed!");

			AssertEquals("Hello World!", dummyLogger.Logs[0].Item1);
			AssertEquals(LogType.Error, dummyLogger.Logs[0].Item2);
		}

		public void TestSameOrDefaultWithSelector()
		{
			var list = new List<A>
			{
				new A { i = 1 },
				null,
				new A { i = 1 },
				new A { i = 2 },
				new A { i = 3 },
				null,
				new A { i = 4 },
				new A { i = 5 },
			};

			Func<A, string> selector = x => (x?.i ?? 0).ToString();
			AssertNull(list.SameOrDefault(selector));

			list = new List<A>
			{
				new A { i = 2 },
				new A { i = 2 },
				new A { i = 2 },
				new A { i = 2 },
				new A { i = 2 },
				new A { i = 2 },
			};

			AssertEquals("2", list.SameOrDefault(selector));
		}

		public void TestSameOrDefault()
		{
			var list = new List<A>
			{
				new A { i = 1 },
				null,
				new A { i = 1 },
				new A { i = 2 },
				new A { i = 3 },
				null,
				new A { i = 4 },
				new A { i = 5 },
			};

			AssertNull(list.SameOrDefault());
			var lambdaComparer = new LambdaComparer<A>((x, y) => x != null && y != null && x.i < 10 && y.i < 10, x => x.i.GetHashCode());
			AssertNull(list.SameOrDefault(lambdaComparer));

			list = new List<A>
			{
				new A { i = 1 },
				new A { i = 1 },
				new A { i = 2 },
				new A { i = 3 },
				new A { i = 4 },
				new A { i = 5 },
			};

			Assert(list.SameOrDefault(lambdaComparer).i < 10);
			Assert(list.SameOrDefault() == null);

			var strings = new[] { "bla", "bla", "bla" };

			Assert(strings.SameOrDefault() == "bla");

			strings = new[] { "bla", "bla", "bla", "" };

			AssertNull(strings.SameOrDefault());
		}

		public void TestBatch_BatchSizeLessThanNumberOfElements()
		{
			var numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7 };

			var batch = numbers.Batch(3).ToArray();

			AssertEquals("should batch into 3 groups", 3, batch.Length);

			AssertContainsExactElementsInAnyOrder("frist batch", new[] { 1, 2, 3 }, batch[0]);
			AssertContainsExactElementsInAnyOrder("second batch", new[] { 4, 5, 6 }, batch[1]);
			AssertContainsExactElementsInAnyOrder("third batch", new[] { 7 }, batch[2]);
		}

		public void TestBatch_BatchSizeGreaterThanNumberOfElements()
		{
			var numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7 };

			var batch = numbers.Batch(10).ToArray();

			AssertEquals("should batch into 1 group", 1, batch.Length);

			AssertContainsExactElementsInAnyOrder("frist and last batch", new[] { 1, 2, 3, 4, 5, 6, 7 }, batch[0]);
		}

		public void TestBatch_BatchSizeTooSmall()
		{
			var numbers = new List<int> { 1, 2, 3 };

			var batch = numbers.Batch(0).ToArray();

			AssertEquals("with batch size 0 should batch each element into a separate group", 3, batch.Length);

			AssertContainsExactElementsInAnyOrder("frist batch", new[] { 1 }, batch[0]);
			AssertContainsExactElementsInAnyOrder("second batch", new[] { 2 }, batch[1]);
			AssertContainsExactElementsInAnyOrder("third batch", new[] { 3 }, batch[2]);

			batch = numbers.Batch(-3).ToArray();

			AssertEquals("with batch size -3 should batch each element into a separate group", 3, batch.Length);

			AssertContainsExactElementsInAnyOrder("frist batch", new[] { 1 }, batch[0]);
			AssertContainsExactElementsInAnyOrder("second batch", new[] { 2 }, batch[1]);
			AssertContainsExactElementsInAnyOrder("third batch", new[] { 3 }, batch[2]);
		}

		public void TestToHoursAndMinutesString()
		{
			var timespan = TimeSpan.FromHours(40);
			AssertEquals("40:00", timespan.ToHoursAndMinutesString());
		}

		public void TestToStandardDateTimeString()
		{
			var datetime = new ZDateTime(2000, 1, 1, 1, 1, 1);
			AssertEquals("01-Jan-2000 01:01:01", datetime.ToStandardDateTimeString());

			datetime = new ZDateTime(2000, 2, 1, 13, 1, 1);
			AssertEquals("01-Feb-2000 13:01:01", datetime.ToStandardDateTimeString());
		}

		public void TestFuncComparer()
		{
			var list = new List<A>() {
				new A { i = 1 },
				new A { i = 2 },
				new A { i = 4 },
				new A { i = 8 },
				new A { i = 3 }
			};

			var sortedWithComparer = list.OrderBy(x => x, new FuncComparer<A>((x, y) => x.i.CompareTo(y.i)));
			var sortedByI = list.OrderBy(x => x.i);
			AssertArrayEqualsByElements(sortedByI.ToArray(), sortedWithComparer.ToArray());

			sortedWithComparer = list.OrderBy(x => x, list.CreateFuncComparerForElements((x, y) => x.i.CompareTo(y.i)));
		}

		public void TestGetPropertyInfo()
		{
			AssertEquals("propertyForTest", Business.Extensions.GetPropertyInfo((TestClassForGetPropertyInfo x) => x.propertyForTest).Name);
		}

		public void TestToDateTimeOffset_ForInvalidTime_ShouldBeInvalidDateTimeOffset()
		{
			var offset = ZDateTime.Invalid.ToDateTimeOffset(null);

			AssertEquals(false, offset.IsValid);
		}

		public void TestToDateTimeOffset_ForValidTime_ButNullPort_ShouldBeValidDateTimeOffset()
		{
			var offset = new ZDateTime(2015, 7, 14).ToDateTimeOffset(null);

			AssertEquals(true, offset.IsValid);
			AssertEquals(new ZDateTime(2015, 7, 14), offset.ToZDateTime());
			AssertEquals(new ZDateTime(2015, 7, 13, 14, 0, 0), offset.ToUtcZDateTime()); // Assumes the currently logged-in branch for context.
		}

		public void TestToDateTimeOffset_ForValidTime_ShouldBeValidDateTimeOffset()
		{
			var factory = new BusinessObjectFactory();
			var location = new RefUNLOCO.Loader(factory).Load("AUPER");
			var offset = new ZDateTime(2015, 7, 14).ToDateTimeOffset(location);

			AssertEquals(true, offset.IsValid);
			AssertEquals(new ZDateTime(2015, 7, 14), offset.ToZDateTime());
			AssertEquals(new ZDateTime(2015, 7, 13, 16, 0, 0), offset.ToUtcZDateTime());
		}

		public void TestToLocalBranchTimeOffset_CurrentBranchNull()
		{
			Business.Extensions.ToLocalBranchTimeOffset(ZDateTime.Now);
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Guid.Empty, Guid.Empty))
			{
				try
				{
					Business.Extensions.ToLocalBranchTimeOffset(ZDateTime.Now);
				}
				catch (NullReferenceException nre)
				{
					AssertContains("CurrentBranch is null in ToLocalBranchTimeOffset.", nre.Message);
					AssertContains("at Enterprise.Environment.MultiThreadUserContextManager.SetMasterUserContext(IUserContext userContext)", nre.Message);
				}
			}
		}

		sealed class Parent
		{
			public Child Child { get; set; }
		}

		sealed class Child
		{
			public string Key { get; set; }
			public string Value { get; set; }
			public bool Flag { get; set; }
		}

		sealed class DummyLogger : ILogger
		{
			public readonly List<Tuple<LogType, string>> Logs = new List<Tuple<LogType, string>>();
			public void Log(LogType type, string message) => Log(type, message, null);

			public void Log(LogType type, string message, Exception ex) => Logs.Add(Tuple.Create(type, message));
		}

		sealed class A
		{
			public int i;
		}

		sealed class TestClassForGetPropertyInfo
		{
			public string propertyForTest { get; }
		}
	}
}
