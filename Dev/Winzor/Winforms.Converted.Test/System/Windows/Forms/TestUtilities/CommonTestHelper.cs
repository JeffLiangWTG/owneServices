// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Linq;
using NUnit.Framework;

namespace System.Windows.Forms.TestUtilities
{
	public static class CommonTestHelper
	{
		public static IEnumerable<TestCaseData> GetEnumTypeTheoryData(Type enumType)
		{
			foreach (Enum item in Enum.GetValues(enumType))
			{
				yield return new TestCaseData(item);
			}
		}

		public static IEnumerable<TestCaseData> GetEnumTypeTheoryDataWitIdentity(Type enumType)
		{
			var counter = 0;
			foreach (Enum item in Enum.GetValues(enumType))
			{
				yield return new TestCaseData(item.ToString(), counter++);
				yield return new TestCaseData(((int)(object)item).ToString(), counter++);
			}
		}

		public static IEnumerable<TestCaseData> GetEnumTypeTheoryDataInvalid(Type enumType)
		{
			var values = Enum.GetValues(enumType).Cast<Enum>().OrderBy(p => p).Distinct().ToArray();

			for (var i = 0; i < values.Length - 2; i++)
			{
				var currentVal = Convert.ToInt32(values[i]);
				var nextVal = Convert.ToInt32(values[i + 1]);
				if (nextVal != currentVal + 1)
				{
					// Not sequential.
					yield return new TestCaseData((Enum)Enum.ToObject(enumType, currentVal + 1));

					if (nextVal - 1 != currentVal)
					{
						yield return new TestCaseData((Enum)Enum.ToObject(enumType, nextVal - 1));
					}
				}
			}

			yield return new TestCaseData((Enum)Enum.ToObject(enumType, Convert.ToInt32(values.Min()) - 1));
			yield return new TestCaseData((Enum)Enum.ToObject(enumType, Convert.ToInt32(values.Max()) + 1));
		}

		public static IEnumerable<TestCaseData> GetEnumTypeTheoryDataInvalidMasked(Type enumType)
		{
			var values = Enum.GetValues(enumType).Cast<Enum>().OrderBy(p => p);

			long allMasked = 0;
			foreach (var value in values)
			{
				allMasked |= Convert.ToInt64(value);
			}

			yield return new TestCaseData((Enum)Enum.ToObject(enumType, int.MaxValue));
			yield return new TestCaseData((Enum)Enum.ToObject(enumType, allMasked + 1));
		}

		#region Primitives

		// helper method to generate theory data for all values of a boolean
		public static IEnumerable<TestCaseData> GetBoolTheoryData()
		{
			yield return new TestCaseData(true);
			yield return new TestCaseData(false);
		}

		// helper method to generate theory data for some values of a int
		public static IEnumerable<TestCaseData> GetIntTheoryData()
		{
			yield return new TestCaseData(int.MinValue);
			yield return new TestCaseData(int.MaxValue);
			yield return new TestCaseData(0);
			yield return new TestCaseData(1);
			yield return new TestCaseData(-1);
			yield return new TestCaseData(int.MaxValue / 2);
		}

		public static IEnumerable<TestCaseData> GetNonNegativeIntTheoryData()
		{
			yield return new TestCaseData(int.MaxValue);
			yield return new TestCaseData(0);
			yield return new TestCaseData(1);
			yield return new TestCaseData(int.MaxValue / 2);
		}

		// helper method to generate theory data for some values of a int
		internal static IEnumerable<TestCaseData> GetUIntTheoryData()
		{
			yield return new TestCaseData(int.MaxValue);
			yield return new TestCaseData(0);
			yield return new TestCaseData(1);
			yield return new TestCaseData(int.MaxValue / 2);
		}

		// helper method to generate theory data for some values of a int
		internal static IEnumerable<TestCaseData> GetNIntTheoryData()
		{
			yield return new TestCaseData(int.MinValue);
			yield return new TestCaseData(-1);
			yield return new TestCaseData(int.MinValue / 2);
		}

		// helper method to generate theory data for some values of a int
		internal static IEnumerable<TestCaseData> GetFloatTheoryData()
		{
			yield return new TestCaseData(float.MaxValue);
			yield return new TestCaseData(float.MinValue);
			yield return new TestCaseData(float.Epsilon);
			yield return new TestCaseData(float.Epsilon * -1);
			yield return new TestCaseData(float.NegativeInfinity); // not sure about these two
			yield return new TestCaseData(float.PositiveInfinity); // 2
			yield return new TestCaseData(0);
			yield return new TestCaseData(-1);
			yield return new TestCaseData(1);
			yield return new TestCaseData(float.MaxValue / 2);
		}

		// helper method to generate theory data for some values of a int
		internal static IEnumerable<TestCaseData> GetUFloatTheoryData()
		{
			yield return new TestCaseData(float.MaxValue);
			yield return new TestCaseData(float.Epsilon);
			yield return new TestCaseData(float.PositiveInfinity); // not sure about this one
			yield return new TestCaseData(0);
			yield return new TestCaseData(1);
			yield return new TestCaseData(float.MaxValue / 2);
		}

		// helper method to generate theory data for a span of string values
		const string reasonable = nameof(reasonable);

		public static IEnumerable<TestCaseData> GetStringTheoryData()
		{
			yield return new TestCaseData(string.Empty);
			yield return new TestCaseData(reasonable);
		}

		public static IEnumerable<TestCaseData> GetStringWithNullTheoryData()
		{
			yield return new TestCaseData(null);
			yield return new TestCaseData(string.Empty);
			yield return new TestCaseData(reasonable);
		}

		public static IEnumerable<TestCaseData> GetNullOrEmptyStringTheoryData()
		{
			yield return new TestCaseData(null);
			yield return new TestCaseData(string.Empty);
		}

		public static IEnumerable<TestCaseData> GetStringNormalizedTheoryData()
		{
			yield return new TestCaseData(null, string.Empty);
			yield return new TestCaseData(string.Empty, string.Empty);
			yield return new TestCaseData(reasonable, reasonable);
		}

		public static IEnumerable<TestCaseData> GetCtrlBackspaceData()
		{
			yield return new TestCaseData("aaa", "", 0);
			yield return new TestCaseData("---", "", 0);
			yield return new TestCaseData(" aaa", "", 0);
			yield return new TestCaseData(" ---", "", 0);
			yield return new TestCaseData("aaa---", "", 0);
			yield return new TestCaseData("---aaa", "---", 0);
			yield return new TestCaseData("aaa---aaa", "aaa---", 0);
			yield return new TestCaseData("---aaa---", "---", 0);
			yield return new TestCaseData("a-a", "a-", 0);
			yield return new TestCaseData("-a-", "", 0);
			yield return new TestCaseData("--a-", "--", 0);
			yield return new TestCaseData("abc", "c", -1);
			yield return new TestCaseData("a,1-b", "a,b", -1);
		}

		public static IEnumerable<TestCaseData> GetCtrlBackspaceRepeatedData()
		{
			yield return new TestCaseData("aaa", "", 2);
			yield return new TestCaseData("---", "", 2);
			yield return new TestCaseData("aaa---aaa", "", 2);
			yield return new TestCaseData("---aaa---", "", 2);
			yield return new TestCaseData("aaa bbb", "", 2);
			yield return new TestCaseData("aaa bbb ccc", "aaa ", 2);
			yield return new TestCaseData("aaa --- ccc", "", 2);
			yield return new TestCaseData("1 2 3 4 5 6 7 8 9 0", "1 ", 9);
		}

		public static IEnumerable<TestCaseData> GetCharTheoryData()
		{
			yield return new TestCaseData('\0');
			yield return new TestCaseData('a');
		}

		public static IEnumerable<TestCaseData> GetIntPtrTheoryData()
		{
			yield return new TestCaseData((IntPtr)(-1));
			yield return new TestCaseData(IntPtr.Zero);
			yield return new TestCaseData((IntPtr)1);
		}

		public static IEnumerable<TestCaseData> GetGuidTheoryData()
		{
			yield return new TestCaseData(Guid.Empty);
			yield return new TestCaseData(Guid.NewGuid());
		}

		public static IEnumerable<TestCaseData> GetColorTheoryData()
		{
			yield return new TestCaseData(Color.Red);
			yield return new TestCaseData(Color.Blue);
			yield return new TestCaseData(Color.Black);
		}

		public static IEnumerable<TestCaseData> GetColorWithEmptyTheoryData()
		{
			yield return new TestCaseData(Color.Red);
			yield return new TestCaseData(Color.Empty);
		}

		public static IEnumerable<TestCaseData> GetPointTheoryData() => GetPointTheoryData(TestIncludeType.All);

		public static IEnumerable<TestCaseData> GetPointTheoryData(TestIncludeType includeType)
		{
			if (!includeType.HasFlag(TestIncludeType.NoPositives))
			{
				yield return new TestCaseData(new Point());
				yield return new TestCaseData(new Point(10));
				yield return new TestCaseData(new Point(1, 2));
			}

			if (!includeType.HasFlag(TestIncludeType.NoNegatives))
			{
				yield return new TestCaseData(new Point(int.MaxValue, int.MinValue));
				yield return new TestCaseData(new Point(-1, -2));
			}
		}

		public static IEnumerable<TestCaseData> GetSizeTheoryData() => GetSizeTheoryData(TestIncludeType.All);

		public static IEnumerable<TestCaseData> GetSizeTheoryData(TestIncludeType includeType)
		{
			if (!includeType.HasFlag(TestIncludeType.NoPositives))
			{
				yield return new TestCaseData(new Size());
				yield return new TestCaseData(new Size(new Point(1, 1)));
				yield return new TestCaseData(new Size(1, 2));
			}

			if (!includeType.HasFlag(TestIncludeType.NoNegatives))
			{
				yield return new TestCaseData(new Size(-1, 1));
				yield return new TestCaseData(new Size(1, -1));
			}
		}

		public static IEnumerable<TestCaseData> GetPositiveSizeTheoryData()
		{
			yield return new TestCaseData(new Size());
			yield return new TestCaseData(new Size(1, 2));
		}

		public static IEnumerable<TestCaseData> GetRectangleTheoryData()
		{
			yield return new TestCaseData(new Rectangle());
			yield return new TestCaseData(new Rectangle(1, 2, 3, 4));
			yield return new TestCaseData(new Rectangle(-1, -2, -3, -4));
		}

		public static IEnumerable<TestCaseData> GetConvertFromTheoryData()
		{
			yield return new TestCaseData(typeof(bool), false);
			yield return new TestCaseData(typeof(InstanceDescriptor), true);
			yield return new TestCaseData(typeof(int), false);
			yield return new TestCaseData(typeof(double), false);
			yield return new TestCaseData(null, false);
		}

		public static IEnumerable<TestCaseData> GetEventArgsTheoryData()
		{
			yield return new TestCaseData(null);
			yield return new TestCaseData(new EventArgs());
		}

		#endregion
	}

	[Flags]
	public enum TestIncludeType
	{
		All,
		NoPositives,
		NoNegatives
	}
}
