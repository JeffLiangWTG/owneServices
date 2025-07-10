using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public abstract class WhsBusinessObjectValidationTestCase : BusinessObjectValidationTestCase
	{
		#region ErrorCheckType

		public enum ErrorCheckType
		{
			HasErrors,
			HasWarnings,
			HasNotifications
		}

		public ErrorCheckType SetErrorCheckType(ErrorCheckType type)
		{
			ErrorCheckType old = fErrorCheck;
			fErrorCheck = type;
			return old;
		}

		ErrorCheckType fErrorCheck = ErrorCheckType.HasErrors;

		#endregion

		#region AssertType

		protected bool AssertType(ZPropertyInfo prop)
		{
			if (fErrorCheck == ErrorCheckType.HasErrors)
			{
				return prop.HasErrors();
			}

			if (fErrorCheck == ErrorCheckType.HasWarnings)
			{
				return prop.HasWarnings();
			}

			if (fErrorCheck == ErrorCheckType.HasNotifications)
			{
				return prop.HasNotifications();
			}

			return true;
		}

		protected bool AssertType(ZPropertyInfo prop, string message)
		{
			if (fErrorCheck == ErrorCheckType.HasErrors)
			{
				return prop.HasError(message);
			}

			if (fErrorCheck == ErrorCheckType.HasWarnings)
			{
				return prop.HasWarning(message);
			}

			if (fErrorCheck == ErrorCheckType.HasNotifications)
			{
				return prop.HasNotification(message);
			}

			return true;
		}

		#endregion

		#region ErrTypeDesc

		protected ZString ErrTypeDesc()
		{
			if (fErrorCheck == ErrorCheckType.HasErrors)
			{
				return "an error";
			}

			if (fErrorCheck == ErrorCheckType.HasWarnings)
			{
				return "a warning";
			}

			if (fErrorCheck == ErrorCheckType.HasNotifications)
			{
				return "a notification";
			}

			return "an undefined notification";
		}

		#endregion

		#region TestMandatory ZString, ZDateTime, ZDateTimeOffset, ZGuid

		public void TestMandatoryString(ZPropertyInfo prop, ErrorCheckType errType)
		{
			TestMandatoryProperty(prop, errType, new ZString("AAA"), ZString.Empty);
		}

		public void TestMandatoryDate(ZPropertyInfo prop, ErrorCheckType errType)
		{
			TestMandatoryProperty(prop, errType, ZDateTime.Now, ZDateTime.Empty);
		}

		public void TestMandatoryDateTimeOffset(ZPropertyInfo prop, ErrorCheckType errType)
		{
			TestMandatoryProperty(prop, errType, ZDateTimeOffset.Now, ZDateTimeOffset.Empty);
		}

		public void TestMandatoryGuid(ZPropertyInfo prop, ErrorCheckType errType)
		{
			TestMandatoryProperty(prop, errType, ZGuid.NewZGuid(), ZGuid.Empty);
		}

		public void TestMandatoryProperty(ZPropertyInfo prop, ErrorCheckType errType, IZType validValue, IZType emptyValue)
		{
			SetErrorCheckType(errType);

			prop.Value = validValue;
			AssertEquals(prop.Name + " = " + prop.Value + ". A valid " + validValue.GetType().Name + " has caused " + ErrTypeDesc() + ".", false, AssertType(prop));
			prop.Value = emptyValue;
			AssertEquals(prop.Name + " = blank. An empty " + validValue.GetType().Name + " has NOT caused " + ErrTypeDesc() + ".", true, AssertType(prop));
		}

		#endregion

		#region TestCodePairList

		public void TestCodePairList(ZPropertyInfo prop, ErrorCheckType errType, bool allowEmpty, ICodeDescriptionPairList list)
		{
			SetErrorCheckType(errType);

			prop.Value = ZString.Empty;
			AssertEquals(prop.Name + " = blank" + ". A blank code has NOT caused " + ErrTypeDesc() + ".", !allowEmpty, AssertType(prop));

			string errorField = prop.HasUserDescription ? prop.Description : "selection";
			string errorMessage = String.Format("Enter a valid {0}.", errorField);

			for (int i = 0; i < list.Count; i++)
			{
				prop.Value = (ZString)((ICodeDescription)list[i]).Code;
				AssertEquals(prop.Name + " = " + prop.Value + ". A valid code has caused " + ErrTypeDesc() + ".", false, AssertType(prop, errorMessage));
			}

			ZString junk = ZString.Replicate(';', prop.MaxLength);
			if (!list.ContainsCode(junk))
			{
				prop.Value = junk;
				AssertEquals(prop.Name + " = " + prop.Value + ". A junk code has NOT caused " + ErrTypeDesc() + ".", true, AssertType(prop, errorMessage));
			}
		}

		#endregion

		#region TestMinShort, TestMinInt,TestMinDecimal,TestMinMaxShort,TestMinMaxInt,TestMinMaxDecimal

		public void TestMinShort(ZPropertyInfo prop, ErrorCheckType errType, ZShort min)
		{
			SetErrorCheckType(errType);

			ZShort testVal = min;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". A valid value (minimum) has caused " + ErrTypeDesc() + ". ", false, AssertType(prop));

			testVal = min - (ZShort)1;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". An invalid value (< minimum) has NOT caused " + ErrTypeDesc() + ".", true, AssertType(prop));

			testVal = min + (ZShort)10000;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". A valid value (> minimum) has caused " + ErrTypeDesc() + ".", false, AssertType(prop));
		}

		public void TestMinInt(ZPropertyInfo prop, ErrorCheckType errType, ZInt min)
		{
			SetErrorCheckType(errType);

			ZInt testVal = min;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". A valid value (minimum) has caused " + ErrTypeDesc() + ". ", false, AssertType(prop));

			testVal = min - 1;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". An invalid value (< minimum) has NOT caused " + ErrTypeDesc() + ".", true, AssertType(prop));

			testVal = min + 10000;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". A valid value (> minimum) has caused " + ErrTypeDesc() + ".", false, AssertType(prop));
		}

		public void TestMinDecimal(ZPropertyInfo prop, ErrorCheckType errType, ZDecimal min)
		{
			SetErrorCheckType(errType);

			ZDecimal testVal = min;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". A valid value (minimum) has caused " + ErrTypeDesc() + ".", false, AssertType(prop));

			testVal = min - 1;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". An invalid value (< minimum) has NOT caused " + ErrTypeDesc() + ".", true, AssertType(prop));

			testVal = min + 10000;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". A valid value (> minimum) has caused " + ErrTypeDesc() + ".", false, AssertType(prop));
		}

		public void TestMinMaxShort(ZPropertyInfo prop, ErrorCheckType errType, ZShort min, ZShort max)
		{
			SetErrorCheckType(errType);

			ZShort testVal = min;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". A valid value (minimum) has caused " + ErrTypeDesc() + ".", false, AssertType(prop));

			testVal = min - 1;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". An invalid value (< minimum) has NOT caused " + ErrTypeDesc() + ".", true, AssertType(prop));

			testVal = (ZShort)((max - min) / 2) + min;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". A valid value (range medium) has caused " + ErrTypeDesc() + ".", false, AssertType(prop));

			testVal = max + 1;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". A invalid value (> maximum) has NOT caused " + ErrTypeDesc() + ".", true, AssertType(prop));

			testVal = max;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". A valid value (maximum) has caused " + ErrTypeDesc() + ".", false, AssertType(prop));
		}

		public void TestMinMaxInt(ZPropertyInfo prop, ErrorCheckType errType, ZInt min, ZInt max)
		{
			SetErrorCheckType(errType);

			ZInt testVal = min;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". A valid value (minimum) has caused " + ErrTypeDesc() + ".", false, AssertType(prop));

			testVal = min - 1;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". An invalid value (< minimum) has NOT caused " + ErrTypeDesc() + ".", true, AssertType(prop));

			testVal = ((max - min) / 2) + min;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". A valid value (range medium) has caused " + ErrTypeDesc() + ".", false, AssertType(prop));

			testVal = max + 1;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". A invalid value (> maximum) has NOT caused " + ErrTypeDesc() + ".", true, AssertType(prop));

			testVal = max;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". A valid value (maximum) has caused " + ErrTypeDesc() + ".", false, AssertType(prop));
		}

		public void TestMinMaxDecimal(ZPropertyInfo prop, ErrorCheckType errType, ZDecimal min, ZDecimal max)
		{
			SetErrorCheckType(errType);

			ZDecimal testVal = min;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". A valid value (minimum) has caused " + ErrTypeDesc() + ".", false, AssertType(prop));

			testVal = min - 1;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". An invalid value (< minimum) has NOT caused " + ErrTypeDesc() + ".", true, AssertType(prop));

			testVal = ((max - min) / 2) + min;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". A valid value (range medium) has caused " + ErrTypeDesc() + ".", false, AssertType(prop));

			testVal = max + 1;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". A invalid value (> maximum) has NOT caused " + ErrTypeDesc() + ".", true, AssertType(prop));

			testVal = max;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". A valid value (maximum) has caused " + ErrTypeDesc() + ".", false, AssertType(prop));
		}

		#endregion

		#region TestMinMaxDate, TestMinMaxDate

		public void TestMinMaxDate(ZPropertyInfo prop, ErrorCheckType errType)
		{
			TestMinMaxDate(prop, errType, System.DateTime.MinValue, System.DateTime.MaxValue);
		}

		public void TestMinMaxDate(ZPropertyInfo prop, ErrorCheckType errType, ZDateTime min, ZDateTime max)
		{
			SetErrorCheckType(errType);

			ZDateTime testVal = min;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". A valid date (minimum) has caused " + ErrTypeDesc() + ".", false, AssertType(prop));

			if (min != System.DateTime.MinValue)
			{
				testVal = min.AddDays(-1);
				prop.Value = testVal;
				AssertEquals(prop.Name + " = " + testVal + ". An invalid date (< minimum) has NOT caused " + ErrTypeDesc() + ".", true, AssertType(prop));
			}

			if (max != System.DateTime.MaxValue)
			{
				testVal = max.AddDays(1);
				prop.Value = testVal;
				AssertEquals(prop.Name + " = " + testVal + ". A invalid date (> maximum) has NOT caused " + ErrTypeDesc() + ".", true, AssertType(prop));
			}

			testVal = max;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". A valid date (maximum) has caused " + ErrTypeDesc() + ".", false, AssertType(prop));
		}

		public void TestMinMaxDateTimeOffset(ZPropertyInfo prop, ErrorCheckType errType)
		{
			TestMinMaxDateTimeOffset(prop, errType, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
		}

		public void TestMinMaxDateTimeOffset(ZPropertyInfo prop, ErrorCheckType errType, ZDateTimeOffset min, ZDateTimeOffset max)
		{
			SetErrorCheckType(errType);

			var testVal = min;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". A valid DateTimeOffset (minimum) has caused " + ErrTypeDesc() + ".", false, AssertType(prop));

			if (min != DateTimeOffset.MinValue)
			{
				testVal = min.AddDays(-1);
				prop.Value = testVal;
				AssertEquals(prop.Name + " = " + testVal + ". An invalid DateTimeOffset (< minimum) has NOT caused " + ErrTypeDesc() + ".", true, AssertType(prop));
			}

			if (max != DateTimeOffset.MaxValue)
			{
				testVal = max.AddDays(1);
				prop.Value = testVal;
				AssertEquals(prop.Name + " = " + testVal + ". A invalid DateTimeOffset (> maximum) has NOT caused " + ErrTypeDesc() + ".", true, AssertType(prop));
			}

			testVal = max;
			prop.Value = testVal;
			AssertEquals(prop.Name + " = " + testVal + ". A valid DateTimeOffset (maximum) has caused " + ErrTypeDesc() + ".", false, AssertType(prop));
		}

		#endregion

		#region Implementation

		#region helper

		WhsTestHelperFunctionsEnv helper;
		protected WhsTestHelperFunctionsEnv Helper
		{
			get
			{
				if (helper == null)
				{
					helper = GetNewTestHelperFunctions();
				}
				return helper;
			}
		}

		#endregion

		#region notify

		TestNotificationBuffer notify;
		protected TestNotificationBuffer Notify
		{
			get
			{
				if (notify == null)
				{
					notify = new TestNotificationBuffer();
				}
				return notify;
			}
		}

		#endregion

		#region GetNewTestHelperFunctions

		protected virtual WhsTestHelperFunctionsEnv GetNewTestHelperFunctions()
		{
			return new WhsTestHelperFunctionsEnv(Factory);
		}

		#endregion

		#endregion
	}
}
