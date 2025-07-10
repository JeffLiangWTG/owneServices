using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(TestAddInfo))]
	sealed class BaseAddInfoTest : PersistentBusinessObjectTestCase
	{
		public void TestGetStringRepresentation()
		{
			var date = new ZDate(2019, 02, 21);
			var result = BaseAddInfo.GetStringRepresentation(date);
			AssertEquals("2019-02-21", result);
			AssertEquals("2021-01-01 02:03:04.0000000 -01:00", BaseAddInfo.GetStringRepresentation(new ZDateTimeOffset(2021, 1, 1, 2, 3, 4, TimeSpan.FromHours(-1))));
			AssertEquals("POINT (-222 22)", BaseAddInfo.GetStringRepresentation(new ZGeography("-222 22")));
		}

		public void TestStarIsSavedAsNonSeparator()
		{
			bO.AddInfo.UZ_String = "Te*st";
			AssertEquals("String=Te¤st", bO.AddInfo.ToString());
			bO.ZY_AddInfo = "String=Te¤st";
			bO.AddInfo.LoadPropertiesFromString(bO.ZY_AddInfo);
			AssertEquals("Te*st", bO.AddInfo.UZ_String);
		}

		public void TestIAddInfoGetEffectiveValue()
		{
			var date1 = ZDateTime.BrettsBirthday;
			var date2 = ZDateTime.BrettsBirthday.AddDays(1);
			bO.Z1_String = "B1";
			bO.Z1_Date = date1;
			bO.AddInfo.UZ_String = "B2";
			bO.AddInfo.UZ_Date = date2;
			bO.AddInfo.UZ_Int = 2;
			IAddInfo addInfo = bO.AddInfo;
			AssertNull(addInfo.GetEffectiveValue(""));
			AssertNull(addInfo.GetEffectiveValue("adfasdf"));
			AssertNull("ZY_Date is not a property of AddInfo", addInfo.GetEffectiveValue(bO.ZY_DateInfo.Name));
			AssertEquals("B2", addInfo.GetEffectiveValue(bO.AddInfo.UZ_StringInfo.Name));
			AssertEquals(date2, addInfo.GetEffectiveValue(bO.AddInfo.UZ_DateInfo.Name));
			AssertEquals(2, addInfo.GetEffectiveValue(bO.AddInfo.UZ_IntInfo.Name));

			bO.AddInfo.UZ_String = ZString.Empty;
			bO.AddInfo.UZ_Date = ZDateTime.Empty;
			bO.AddInfo.UZ_Int = ZInt.Zero;
			AssertEquals("B1", addInfo.GetEffectiveValue(bO.AddInfo.UZ_StringInfo.Name));
			AssertEquals(date1, addInfo.GetEffectiveValue(bO.AddInfo.UZ_DateInfo.Name));
			AssertEquals(0, addInfo.GetEffectiveValue(bO.AddInfo.UZ_IntInfo.Name));
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			// Not applicable to this class
		}

		public void TestIsInDatabase()
		{
			AssertEquals(false, bO.AddInfo.IsInDatabase);
			bO.IsInDatabaseExposed = true;
			AssertEquals(true, bO.AddInfo.IsInDatabase);
		}

		public void TestConvertToZType()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			AssertEquals("HELLO", BaseAddInfo.ConvertToZType(typeof(ZString), "HELLO"));
			AssertEquals(12.32m, BaseAddInfo.ConvertToZType(typeof(ZDecimal), "12.32"));
			AssertEquals(12, BaseAddInfo.ConvertToZType(typeof(ZInt), "12"));
			AssertEquals((short)12, BaseAddInfo.ConvertToZType(typeof(ZShort), "12"));
			AssertEquals((byte)7, BaseAddInfo.ConvertToZType(typeof(ZByte), "7"));
			AssertEquals(long.MaxValue, BaseAddInfo.ConvertToZType(typeof(ZLong), long.MaxValue.ToString()));
			AssertEquals(ZDateTime.BrettsBirthday, BaseAddInfo.ConvertToZType(typeof(ZDateTime), ZDateTime.BrettsBirthday.SqlFormat));
			AssertEquals(ZDate.BrettsBirthday, BaseAddInfo.ConvertToZType(typeof(ZDate), ZDateTime.BrettsBirthday.SqlFormat));
			AssertEquals(new ZDateTimeOffset(2021, 1, 1, 2, 3, 4, TimeSpan.FromHours(-1)), BaseAddInfo.ConvertToZType(typeof(ZDateTimeOffset), new ZDateTimeOffset(2021, 1, 1, 2, 3, 4, TimeSpan.FromHours(-1)).SqlFormat));
			AssertEquals(dummy.PK, BaseAddInfo.ConvertToZType(typeof(ZGuid), dummy.PK.ToString()));
			AssertEquals(ZBool.True, BaseAddInfo.ConvertToZType(typeof(ZBool), "Y"));
			AssertEquals(ZBool.False, BaseAddInfo.ConvertToZType(typeof(ZBool), "76"));
			AssertEquals(new ZGeography("-222 22"), BaseAddInfo.ConvertToZType(typeof(ZGeography), "-222 22"));
		}

		public void TestIsSavedByFactory()
		{
			AssertEquals("Not saved by factory", false, bO.AddInfo.IsSavedByFactory);
		}

		public void TestBooleanToString()
		{
			bO.AddInfo.UZ_Boolean = ZBool.True;
			AssertEquals("ToString()", "Boolean=Y", bO.AddInfo.ToString());
		}

		public void TestDateToString()
		{
			bO.AddInfo.UZ_Date = new ZDateTime(2004, 9, 18, 12, 30, 31);
			AssertEquals("ToString()", "Date=" + bO.AddInfo.UZ_Date.SqlFormat, bO.AddInfo.ToString());
		}

		public void TestDecimalToString()
		{
			bO.AddInfo.UZ_Decimal = 123456.12345m;
			AssertEquals("ToString()", "Decimal=123456.12345", bO.AddInfo.ToString());

			bO.AddInfo.UZ_Decimal = 100000.00000m;
			AssertEquals("Trailing zeros should be truncated", "Decimal=100000", bO.AddInfo.ToString());
		}

		public void TestIntToString()
		{
			bO.AddInfo.UZ_Int = 123456;
			AssertEquals("ToString()", "Int=123456", bO.AddInfo.ToString());
		}

		public void TestShortToString()
		{
			bO.AddInfo.UZ_Short = 12345;
			AssertEquals("ToString()", "Short=12345", bO.AddInfo.ToString());
		}

		public void TestStringToString()
		{
			bO.AddInfo.UZ_String = "123456";
			bO.AddInfo.UZ_NString = "一二三四五六";
			AssertEquals("ToString()", "NString=一二三四五六*String=123456", bO.AddInfo.ToString());
		}

		public void TestLoadBooleanFromString()
		{
			bO.ZY_AddInfo = "Boolean=Y";
			AssertEquals("Value", ZBool.True, bO.AddInfo.UZ_Boolean);//Reference AddInfo should trigger load()
		}

		public void TestLoadDateFromString()
		{
			ZDateTime testDateTime = ZDateTime.Now;
			bO.ZY_AddInfo = "Date=" + testDateTime.SqlFormat;
			AssertEquals("Value", testDateTime, bO.AddInfo.UZ_Date);//Reference AddInfo should trigger load()

			bO.ZY_AddInfo = "Date=2012-01-02 00:00:01";
			AssertEquals("Value", new ZDateTime(2012, 1, 2, 0, 0, 1), bO.AddInfo.UZ_Date);

			bO.ZY_AddInfo = "Date=2012-01-02 AB:AB:01";
			AssertEquals("Value", ZDateTime.Invalid, bO.AddInfo.UZ_Date);
		}

		public void TestLoadDecimalFromString()
		{
			bO.ZY_AddInfo = "Decimal=123.12345";
			AssertEquals("Value", 123.12345m, bO.AddInfo.UZ_Decimal);//Reference AddInfo should trigger load()
		}

		public void TestLoadIntFromString()
		{
			bO.ZY_AddInfo = "Int=123";
			AssertEquals("Value", 123, bO.AddInfo.UZ_Int);//Reference AddInfo should trigger load()
		}

		[TestDate(2014, 12, 16, 12, 30, 35)]
		public void TestHandleAddInfoConcurrencyIssue()
		{
			var bizObj = Factory.New<DummyBusinessObjectWithIAddInfoWithConcurrencyResolverSupporter>();
			bizObj.AddInfo.UZ_Int = 12;
			bizObj.AddInfo.UZ_String = "COMMENT";
			bizObj.AddInfo.UZ_NString = "初始值";
			bizObj.AddInfo.UZ_Boolean = true;
			bizObj.AddInfo.UZ_Date = new ZDateTime(2014, 11, 1);
			bizObj.AddInfo.UZ_Decimal = 10m;
			ConcurrencyInfo.SetConcurrencyPolicy(bizObj, nameof(DummyBusinessObject.Z0_VarCharMax), ConcurrencyPolicy.Observe);
			ConcurrencyInfo.SetConcurrencyPolicy(bizObj, nameof(DummyBusinessObject.Z0_NVarCharMax), ConcurrencyPolicy.Observe);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var bizObjInDiffFactory = newFactory.Load<DummyBusinessObjectWithIAddInfoWithConcurrencyResolverSupporter>(bizObj.PK);
			bizObj.AddInfo.UZ_Decimal = 200m;
			bizObj.AddInfo.UZ_String = "123";
			bizObj.AddInfo.UZ_NString = "一二三";
			bizObj.AddInfo.UZ_Boolean = false;
			bizObj.AddInfo.UZ_Date = new ZDateTime(2014, 10, 10);
			bizObjInDiffFactory.AddInfo.UZ_Decimal = 150m;
			bizObjInDiffFactory.AddInfo.UZ_String = "123";
			bizObjInDiffFactory.AddInfo.UZ_NString = "四五六";
			bizObjInDiffFactory.AddInfo.UZ_Date = new ZDateTime(2014, 12, 10);
			bizObjInDiffFactory.AddInfo.UZ_Short = 9;
			ConcurrencyInfo.SetConcurrencyPolicy(bizObjInDiffFactory, nameof(DummyBusinessObject.Z0_VarCharMax), ConcurrencyPolicy.Observe);
			ConcurrencyInfo.SetConcurrencyPolicy(bizObjInDiffFactory, nameof(DummyBusinessObject.Z0_NVarCharMax), ConcurrencyPolicy.Observe);
			Factory.RefreshEnabled = false;
			Factory.Save();
			var handler = new NotificationHandlerForTest();
			try
			{
				newFactory.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, handler, true);
			}
			AssertConcurrencyValue(bizObj, false, new ZDateTime(2014, 10, 10), 200m, 12, ZShort.Zero, "123", "一二三");
			AssertConcurrencyValue(bizObjInDiffFactory, false, new ZDateTime(2014, 10, 10), 200m, 12, (short)9, "123", "一二三");
			var message = @"Another user () has changed these fields.
Boolean: Yours: 'Y', Theirs: 'N'
Date: Yours: '10-Dec-14 00:00:00', Theirs: '10-Oct-14 00:00:00'
Decimal: Yours: '150', Theirs: '200'";
			AssertHasWarningContaining(bizObjInDiffFactory.Z0_CodeInfo, message);
			AssertHasWarningContaining(bizObjInDiffFactory.AddInfo.UZ_BooleanInfo, @"Another user () has changed this field.
Yours: 'Y', Theirs: 'N'");
			AssertHasWarningContaining(bizObjInDiffFactory.AddInfo.UZ_DateInfo, @"Another user () has changed this field.
Yours: '10-Dec-14 00:00:00', Theirs: '10-Oct-14 00:00:00'");
			AssertHasWarningContaining(bizObjInDiffFactory.AddInfo.UZ_DecimalInfo, @"Another user () has changed this field.
Yours: '150', Theirs: '200'");
			AssertEquals("ReportInformationCaption", "WARNING", handler.ReportInformationCaption);
			AssertMultilineASCIIEquals("ReportInformationMessage", @"While you have been working with this form, another user has made changes.

The system will now try to combine your changes with those of the other user.
After you click 'OK', the form will merge your changes with changes made by other user.

However, the fields will have warning messages explaining the other user's changes.
Please review the form carefully before clicking the 'Save' button again.

The following objects have changes and will be merged:
DummyBizo
	N Var Char Max
	Data", handler.ReportInformationMessage);

			var addInfoChildren = ((IBusiness)bizObjInDiffFactory).Children.Where(x => x != null && x.GetType() == typeof(TestAddInfo)).ToArray();
			AssertEquals("Should only be one", 1, addInfoChildren.Length);

			newFactory.Save();
			AssertConcurrencyValue(bizObj, false, new ZDateTime(2014, 10, 10), 200m, 12, ZShort.Zero, "123", "一二三");
			AssertConcurrencyValue(bizObjInDiffFactory, false, new ZDateTime(2014, 10, 10), 200m, 12, (short)9, "123", "一二三");
			bizObj.AddInfo.UZ_Short = 15;
			bizObj.AddInfo.UZ_String = "456";
			bizObj.AddInfo.UZ_NString = "四五六";
			handler = new NotificationHandlerForTest();

			try
			{
				Factory.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, handler, true);
			}
			AssertConcurrencyValue(bizObj, false, new ZDateTime(2014, 10, 10), 200m, 12, (short)9, "456", "四五六");
			AssertConcurrencyValue(bizObjInDiffFactory, false, new ZDateTime(2014, 10, 10), 200m, 12, (short)9, "123", "一二三");
			message = @"Another user () has changed these fields.
Short: Yours: '15', Theirs: '9'";
			AssertHasWarningContaining(bizObj.Z0_CodeInfo, message);
			AssertHasWarningContaining(bizObj.AddInfo.UZ_ShortInfo, @"Another user () has changed this field.
Yours: '15', Theirs: '9'");
			AssertEquals("ReportInformationCaption", "WARNING", handler.ReportInformationCaption);
			AssertMultilineASCIIEquals("ReportInformationMessage", @"While you have been working with this form, another user has made changes.

The system will now try to combine your changes with those of the other user.
After you click 'OK', the form will merge your changes with changes made by other user.

However, the fields will have warning messages explaining the other user's changes.
Please review the form carefully before clicking the 'Save' button again.

The following objects have changes and will be merged:
DummyBizo
	Data", handler.ReportInformationMessage);
		}

		public void TestHandleAddInfoConcurrencyIssue_NAddInfo()
		{
			var bizObj = Factory.New<DummyBusinessObjectWithIAddInfoWithConcurrencyResolverSupporter>();
			bizObj.AddInfo.UZ_NString = "初始值";
			ConcurrencyInfo.SetConcurrencyPolicy(bizObj, nameof(DummyBusinessObject.Z0_NVarCharMax), ConcurrencyPolicy.Observe);
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var bizObjInDiffFactory = newFactory.Load<DummyBusinessObjectWithIAddInfoWithConcurrencyResolverSupporter>(bizObj.PK);
			bizObj.AddInfo.UZ_NString = "一二三";
			bizObjInDiffFactory.AddInfo.UZ_NString = "四五六";
			ConcurrencyInfo.SetConcurrencyPolicy(bizObjInDiffFactory, nameof(DummyBusinessObject.Z0_NVarCharMax), ConcurrencyPolicy.Observe);
			Factory.RefreshEnabled = false;
			Factory.Save();
			var handler = new NotificationHandlerForTest();
			try
			{
				newFactory.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, handler, true);
			}
			AssertEquals("一二三", bizObj.AddInfo.UZ_NString);
			AssertEquals("一二三", bizObjInDiffFactory.AddInfo.UZ_NString);
			var message = @"Another user () has changed these fields.
N String: Yours: '四五六', Theirs: '一二三'";
			AssertHasWarningContaining(bizObjInDiffFactory.Z0_CodeInfo, message);
			AssertHasWarningContaining(bizObjInDiffFactory.AddInfo.UZ_NStringInfo, @"Another user () has changed this field.
Yours: '四五六', Theirs: '一二三'");
			AssertEquals("ReportInformationCaption", "WARNING", handler.ReportInformationCaption);
			AssertMultilineASCIIEquals("ReportInformationMessage", @"While you have been working with this form, another user has made changes.

The system will now try to combine your changes with those of the other user.
After you click 'OK', the form will merge your changes with changes made by other user.

However, the fields will have warning messages explaining the other user's changes.
Please review the form carefully before clicking the 'Save' button again.

The following objects have changes and will be merged:
DummyBizo
	N Var Char Max", handler.ReportInformationMessage);
		}

		void AssertConcurrencyValue(DummyBusinessObjectWithIAddInfoWithConcurrencyResolverSupporter bizObj, ZBool booleanValue, ZDateTime dateValue, ZDecimal decimalValue, ZInt intValue, ZShort shortValue, ZString stringValue, ZString nStringValue)
		{
			AssertEquals("bizObj.AddInfo.UZ_Boolean", booleanValue, bizObj.AddInfo.UZ_Boolean);
			AssertEquals("bizObj.AddInfo.UZ_Date", dateValue, bizObj.AddInfo.UZ_Date);
			AssertEquals("bizObj.AddInfo.UZ_Decimal", decimalValue, bizObj.AddInfo.UZ_Decimal);
			AssertEquals("bizObj.AddInfo.UZ_Int", intValue, bizObj.AddInfo.UZ_Int);
			AssertEquals("bizObj.AddInfo.UZ_Short", shortValue, bizObj.AddInfo.UZ_Short);
			AssertEquals("bizObj.AddInfo.UZ_String", stringValue, bizObj.AddInfo.UZ_String);
			AssertEquals("bizObj.AddInfo.UZ_NString", nStringValue, bizObj.AddInfo.UZ_NString);
		}

		public void TestLoadShortFromString()
		{
			bO.ZY_AddInfo = "Short=123";
			AssertEquals("Value", (short)123, bO.AddInfo.UZ_Short);//Reference AddInfo should trigger load()
		}

		public void TestLoadStringFromString()
		{
			bO.ZY_AddInfo = "String=123456*NString=一二三四五六";
			AssertEquals("UZ_String Value", "123456", bO.AddInfo.UZ_String);
			AssertEquals("UZ_NString Value", "一二三四五六", bO.AddInfo.UZ_NString);
		}

		public void TestUpdateRelatedAddInfoStringWhenSaved()
		{
			bO.AddInfo.UZ_String = "123456";
			bO.AddInfo.UZ_NString = "一二三四五六";
			Factory.Save();

			AssertEquals("ZY_AddInfo", "String=123456*NString=一二三四五六", bO.ZY_AddInfo);
			AssertEquals("ZY_NAddInfo", "NString=一二三四五六", bO.ZY_NAddInfo);
		}

		public void TestIsDeleted()
		{
			AssertEquals("Deleted", bO.IsDeleted, bO.AddInfo.IsDeleted);
			bO.Delete();
			AssertEquals("Deleted", bO.IsDeleted, bO.AddInfo.IsDeleted);
		}

		public void TestSavingNewChild()
		{
			bO.IsInDatabaseExposed = false;
			using (bO.AddInfo.SuspendSettingHasChanges())
			{
				bO.AddInfo.UZ_String = "123456";
				bO.AddInfo.UZ_NString = "一二三四五六";
			}
			AssertEquals("PreCondition: AddInfo.HasChanges", false, bO.AddInfo.HasChanges);
			AssertNotEquals("PreCondition: BO.ZY_AddInfo", "String=123456*NString=一二三四五六", bO.ZY_AddInfo);
			AssertNotEquals("PreCondition: BO.ZY_NAddInfo", "NString=一二三四五六", bO.ZY_NAddInfo);

			bO.Factory.Save();
			AssertEquals("String=123456*NString=一二三四五六", bO.ZY_AddInfo);
			AssertEquals("NString=一二三四五六", bO.ZY_NAddInfo);

			bO.IsInDatabaseExposed = true;
			bO.ZY_AddInfo = "";
			bO.Factory.Save();
			using (bO.AddInfo.SuspendSettingHasChanges())
			{
				bO.AddInfo.UZ_String = "123456";
				bO.AddInfo.UZ_NString = "一二三四五六";
			}
			AssertEquals("PreCondition: AddInfo.HasChanges", false, bO.AddInfo.HasChanges);
			AssertNotEquals("PreCondition: BO.ZY_AddInfo", "String=123456*NString=一二三四五六", bO.ZY_AddInfo);
			AssertNotEquals("PreCondition: BO.ZY_NAddInfo", "NString=一二三四五六", bO.ZY_NAddInfo);

			bO.Factory.Save();
			AssertNotEquals("PreCondition: BO.ZY_AddInfo", "String=123456*NString=一二三四五六", bO.ZY_AddInfo);
			AssertNotEquals("PreCondition: BO.ZY_NAddInfo", "NString=一二三四五六", bO.ZY_NAddInfo);
		}

		public void TestAddInfoFieldNotLostWhenParentIsUpdated()
		{
			var declaration = Factory.New<BaseJobDeclarationWithAddInfo>();
			var addinfo = new TestAddInfo(declaration.JE_AddInfoInfo);
			addinfo.UZ_String = "123456";
			Factory.Save();
			AssertEquals("String=123456", declaration.JE_AddInfo);
			addinfo.UZ_String = "789012";
			var newFactory = new BusinessObjectFactory();
			var log = newFactory.New<ZArchitecture.Business.StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Parent = declaration.PK;
				log.SL_Table = declaration.TableName;
				log.SL_SE_NKEvent = ZArchitecture.Business.Events.ReadRelatedNotes.Code;
				log.SL_GS_NKUser = "TTG";
			}
			newFactory.Save();
			AssertEquals("789012", addinfo.UZ_String);

			newFactory = new BusinessObjectFactory();
			var dec = newFactory.Load<BaseJobDeclarationWithAddInfo>(declaration.PK);
			dec.JE_AddInfo = "String=8976534";
			newFactory.Save();
			AssertEquals("8976534", addinfo.UZ_String);
		}

		public void TestAddInfoIsReloadedWhenParentIsReloaded()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = Factory.New<BaseJobComInvoiceLineWithAddInfo>();
			invoiceLine.JI_JZ = invoice.PK;
			var addinfo = new TestAddInfo(invoiceLine.JI_AddInfoInfo);
			addinfo.UZ_String = "123456";
			addinfo.UZ_NString = "一二三四五六";
			Factory.Save();
			AssertEquals("String=123456*NString=一二三四五六", invoiceLine.JI_AddInfo);
			AssertEquals("NString=一二三四五六", invoiceLine.JI_NAddInfo);
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var invoiceLineReloaded = newFactory.Load<BaseJobComInvoiceLineWithAddInfo>(invoiceLine.PK);
			AssertEquals("String=123456*NString=一二三四五六", invoiceLineReloaded.JI_AddInfo);
			AssertEquals("NString=一二三四五六", invoiceLineReloaded.JI_NAddInfo);
			invoiceLineReloaded.JI_AddInfo = "String=523211*NString=五二三二壹壹";
			newFactory.Save();
			AssertEquals("String=123456*NString=一二三四五六", invoiceLine.JI_AddInfo);
			AssertEquals("NString=一二三四五六", invoiceLine.JI_NAddInfo);
			invoiceLine.Reload();
			AssertEquals("String=523211*NString=五二三二壹壹", invoiceLine.JI_AddInfo);
			AssertEquals("523211", addinfo.UZ_String);
			AssertEquals("NString=五二三二壹壹", invoiceLine.JI_NAddInfo);
			AssertEquals("五二三二壹壹", addinfo.UZ_NString);
		}

		public void TestAddInfoBOIsUpdateWhenAddInfoPropertyIsUpdatedExternally()
		{
			bO.ZY_AddInfo = "String=Hello World*NString=你好，世界";
			AssertEquals("UZ_String", "Hello World", bO.AddInfo.UZ_String);
			AssertEquals("UZ_NString", "你好，世界", bO.AddInfo.UZ_NString);
			bO.AddInfo.UZ_String = "Bye World";
			bO.AddInfo.UZ_NString = "拜拜，世界";
			AssertEquals("ZY_AddInfo", "String=Hello World*NString=你好，世界", bO.ZY_AddInfo);
			AssertEquals("ZY_NAddInfo", "NString=你好，世界", bO.ZY_NAddInfo);
			bO.AddInfo.Factory.Save();
			AssertEquals("ZY_AddInfo", "String=Bye World*NString=拜拜，世界", bO.ZY_AddInfo);
			AssertEquals("ZY_NAddInfo", "NString=拜拜，世界", bO.ZY_NAddInfo);
			bO.ZY_AddInfo = "String=Hi Again World";
			bO.ZY_NAddInfo = "NString=我又来了，世界";
			AssertEquals("UZ_String", "Hi Again World", bO.AddInfo.UZ_String);
			AssertEquals("UZ_NString", "我又来了，世界", bO.AddInfo.UZ_NString);
		}

		[DeveloperOnlyTest]
		public void TestLoadPropertiesFromString_Performance()
		{
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			var info = bO.AddInfo;
			for (var i = 0; i < 1000000; i++)
			{
				info.LoadPropertiesFromString("String=Hello World*NString=你好，世界");
			}
			AssertLessThan(stopWatch.ElapsedMilliseconds, 15000);
		}

		public void TestLoadPropertiesFromString_CallsSuspendMarkingAsNeedingValidation()
		{
			bO.AddInfo.UZ_String = "Value";
			bO.AddInfo.UZ_NString = "值";
			AssertEquals("IsMarkingAsNeedingValidationSuspended false when setting property", false, bO.AddInfo.WasMarkingAsNeedingValidationSuspendedInProperty);
			bO.AddInfo.LoadPropertiesFromString("String=Value1*NString=值一");
			AssertEquals("IsMarkingAsNeedingValidationSuspended true during load of properties", true, bO.AddInfo.WasMarkingAsNeedingValidationSuspendedInProperty);
		}

		public void TestLoadPropertiesFromString_ZStringOversized()
		{
			AssertNoExceptionThrown(() => bO.AddInfo.LoadPropertiesFromString("String=123456789012345678901234567890123456789012345678    3456789012345678  "));
		}

		public void TestLoadPropertiesFromString_ZStringTrimTrailingSpace()
		{
			bO.AddInfo.LoadPropertiesFromString("String=123456789012345678901234567890123456789012345678                       ");
			AssertEquals("123456789012345678901234567890123456789012345678", bO.AddInfo.UZ_String);
		}

		public void TestLoadPropertiesFromString_SilentlyThrowAwayUnrecoginizedAddInfoString()
		{
			bO.AddInfo.LoadPropertiesFromString("String=12345*NEVERFINDME=4321");
			AssertEquals("12345", bO.AddInfo.UZ_String);
		}

		public void TestLoadPropertiesFromString_SilentlyThrowAwayAddInfoThatHappensToBeNonPersistent()
		{
			bO.AddInfo.LoadPropertiesFromString("String=12345*SomeProperty=4321");
			AssertEquals("12345", bO.AddInfo.UZ_String);
			AssertEquals("", bO.AddInfo.UZ_SomeProperty);
		}

		public void TestLoadPropertiesFromString_WhenParentIsDeleted()
		{
			var addInfo = bO.AddInfo;
			addInfo.LoadPropertiesFromString("String=1234");
			AssertEquals("1234", addInfo.UZ_String);

			bO.Delete();
			addInfo.LoadPropertiesFromString("String=5678");
			AssertEquals("change of value is not applied", "1234", addInfo.UZ_String);
		}

		public void TestAddinfoWithNonProductItemsRemoved()
		{
			ZString beforeMethod = "InstrumentCode_Hidden=1234567*DRE=1.0000*TreatmentCode_Hidden=505*DXT=S*AMB=D*InstrumentType_Hidden=TC*DMP=1.00*DCX=FR*ORG=DE*TCI=BL:2345678";
			ZString afterMethod = "InstrumentCode_Hidden=1234567**TreatmentCode_Hidden=505*DXT=S**InstrumentType_Hidden=TC**DCX=FR*ORG=DE*TCI=BL:2345678";
			AssertEquals("AddinfoWithNonProductItemsRemoved", afterMethod, BaseAddInfo.AddinfoWithNonProductItemsRemoved(beforeMethod, itemsThatMayBeAddedToProduct, '*'));
		}

		public void TestAllValidAddinfoWithNonProductItemsRemoved()
		{
			ZString beforeMethod = "DCX=1*DXT=2*GSTE=3*ICN=4*ISC=5*LCTE=6*ORG=7*PRI=8*PRT=9*PST=10*MD2=11*TC2=12*CL2=13*TR2=14*TI2=15*UQ2=16*TAN=17*RNO=18*TRN=19*VAN=20*WETE=21*ELA=22*TCI=23*InstrumentCode_Hidden=24*InstrumentType_Hidden=25*TreatmentCode_Hidden=26";
			ZString afterMethod = beforeMethod;
			AssertEquals("AllValidAddinfoWithNonProductItemsRemoved", afterMethod, BaseAddInfo.AddinfoWithNonProductItemsRemoved(beforeMethod, itemsThatMayBeAddedToProduct, '*'));
		}

		public void TestSetValueConvertNoneWesternEuropeanCharacters()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = Factory.New<BaseJobComInvoiceLineWithAddInfo>();
			invoiceLine.JI_JZ = invoice.PK;
			var addinfo = new TestAddInfo(invoiceLine.JI_AddInfoInfo);
			addinfo.UZ_String = "UZ\u73a8";
			addinfo.UZ_NString = "UZ\u73a8";
			AssertEquals("UZ\u73a8", addinfo.UZ_String);
			AssertEquals("UZ\u73a8", addinfo.UZ_NString);
			((IAddInfo)addinfo).SetValue(TestAddInfo.Schema.UZ_String, "UZ\u73a8");
			((IAddInfo)addinfo).SetValue(TestAddInfo.Schema.UZ_NString, "UZ\u73a8");
			AssertEquals("UZ?", addinfo.UZ_String);
			AssertEquals("UZ\u73a8", addinfo.UZ_NString);
		}

		public void TestValidateCharacterSet()
		{
			bO.AddInfo.UZ_String = "123456";
			bO.AddInfo.UZ_NString = "一二三四五六";
			bO.AddInfo.Validation.ValidateUZ_String();
			bO.AddInfo.Validation.ValidateUZ_NString();
			AssertEquals("UZ_String has no errors.", false, bO.AddInfo.UZ_StringInfo.HasNotifications());
			AssertEquals("UZ_NString has no errors.", false, bO.AddInfo.UZ_NStringInfo.HasNotifications());
			bO.AddInfo.UZ_String = "UZ\u73a8";
			bO.AddInfo.UZ_NString = "UZ\u73a8";
			bO.AddInfo.Validation.ValidateUZ_String();
			bO.AddInfo.Validation.ValidateUZ_NString();
			AssertEquals("UZ_String has non-english caracter error?", true, bO.AddInfo.UZ_StringInfo.HasError("String only accepts Western European languages characters."));
			AssertEquals("UZ_NString has no errors.", false, bO.AddInfo.UZ_NStringInfo.HasNotifications());
		}

		readonly string[] itemsThatMayBeAddedToProduct = new string[]
		{
					AUAddInfoSchema.ZA_DCX.Name.Substring(3),
					AUAddInfoSchema.ZA_DXT.Name.Substring(3),
					AUAddInfoSchema.ZA_GSTE.Name.Substring(3),
					AUAddInfoSchema.ZA_ICN.Name.Substring(3),
					AUAddInfoSchema.ZA_ISC.Name.Substring(3),
					AUAddInfoSchema.ZA_LCTE.Name.Substring(3),
					AUAddInfoSchema.ZA_ORG.Name.Substring(3),
					AUAddInfoSchema.ZA_PRI.Name.Substring(3),
					AUAddInfoSchema.ZA_PRT.Name.Substring(3),
					AUAddInfoSchema.ZA_PST.Name.Substring(3),
					AUAddInfoSchema.ZA_MD2.Name.Substring(3),
					AUAddInfoSchema.ZA_TC2.Name.Substring(3),
					AUAddInfoSchema.ZA_CL2.Name.Substring(3),
					AUAddInfoSchema.ZA_TR2.Name.Substring(3),
					AUAddInfoSchema.ZA_TI2.Name.Substring(3),
					AUAddInfoSchema.ZA_UQ2.Name.Substring(3),
					AUAddInfoSchema.ZA_TAN.Name.Substring(3),
					AUAddInfoSchema.ZA_RNO.Name.Substring(3),
					AUAddInfoSchema.ZA_TRN.Name.Substring(3),
					AUAddInfoSchema.ZA_VAN.Name.Substring(3),
					AUAddInfoSchema.ZA_WETE.Name.Substring(3),
					AUAddInfoSchema.ZA_ELA.Name.Substring(3),
					AUAddInfoSchema.ZA_TCI.Name.Substring(3),
					AUAddInfoSchema.ZA_InstrumentCode_Hidden.Name.Substring(3),
					AUAddInfoSchema.ZA_InstrumentType_Hidden.Name.Substring(3),
					AUAddInfoSchema.ZA_TreatmentCode_Hidden.Name.Substring(3)
		};

		public void TestGetKeysIsCached()
		{
			IAddInfo bO1AddInfo = new TestBO(Factory).AddInfo;
			IAddInfo bO2AddInfo = new TestBO(Factory).AddInfo;
			IAddInfo bO3InDiffFactoryAddInfo = new TestBO(new BusinessObjectFactory()).AddInfo;
			IAddInfo bO4AddInfoDiff = new TestAddInfo2(Factory.New<BaseJobComInvoiceLineWithAddInfo>().JI_AddInfoInfo);
			var keys = bO1AddInfo.GetKeys();
			AssertEquals("Same type", true, Object.ReferenceEquals(keys, bO2AddInfo.GetKeys()));
			AssertEquals("Different Factory", false, Object.ReferenceEquals(keys, bO3InDiffFactoryAddInfo.GetKeys()));
			AssertEquals("Different Type", false, Object.ReferenceEquals(keys, bO4AddInfoDiff.GetKeys()));
		}

		class TestAddInfo2 : TestAddInfo
		{
			public TestAddInfo2(ZPropertyInfo addInfoProperty) : base(addInfoProperty)
			{
			}
		}

		public void TestGetKeys()
		{
			IAddInfo addInfo = bO.AddInfo;
			var keys = addInfo.GetKeys();
			AssertEquals(10, keys.Count);
			AssertNull(keys[TestAddInfo.Schema.UZ_Boolean.Substring(3)].Parser);
			AssertNull(keys[TestAddInfo.Schema.UZ_Date.Substring(3)].Parser);
			AssertNull(keys[TestAddInfo.Schema.UZ_Decimal.Substring(3)].Parser);
			AssertNull(keys[TestAddInfo.Schema.UZ_Guid.Substring(3)].Parser);
			AssertNull(keys[TestAddInfo.Schema.UZ_Int.Substring(3)].Parser);
			AssertNull(keys[TestAddInfo.Schema.UZ_Short.Substring(3)].Parser);
			AssertNull(keys[TestAddInfo.Schema.UZ_DateOnly.Substring(3)].Parser);
			AssertNull(keys[TestAddInfo.Schema.UZ_Long.Substring(3)].Parser);
			var data = keys[TestAddInfo.Schema.UZ_String.Substring(3)];
			AssertEquals(TestAddInfo.Schema.UZ_String, data.PropertyName);
			var function = data.Parser;
			AssertNotNull("String Function", function);
			var logger = new TestErrorLogger();
			var longValue = "HELLO".PadRight(TestAddInfo.Schema.UZ_StringMaxLength + 1, '1');
			var value = function(logger, longValue);
			AssertEquals("HELLO".PadRight(TestAddInfo.Schema.UZ_StringMaxLength, '1'), value);
			AssertEquals("logger.Logs", "Warning - " + BaseAddInfo.GetMaximumLengthTruncateMessage("String", TestAddInfo.Schema.UZ_StringMaxLength, longValue), logger.Logs);

			logger = new TestErrorLogger();
			longValue = "HELLO".PadRight(TestAddInfo.Schema.UZ_StringMaxLength, '1');
			value = function(logger, longValue);
			AssertEquals(longValue, value);
			AssertEquals("logger.Logs", "", logger.Logs);

			data = keys[TestAddInfo.Schema.UZ_NString.Substring(3)];
			AssertEquals(TestAddInfo.Schema.UZ_NString, data.PropertyName);
			function = data.Parser;
			AssertNotNull("String Function", function);
			logger = new TestErrorLogger();
			longValue = "你好".PadRight(TestAddInfo.Schema.UZ_NStringMaxLength + 1, '1');
			value = function(logger, longValue);
			AssertEquals("你好".PadRight(TestAddInfo.Schema.UZ_NStringMaxLength, '1'), value);
			AssertEquals("logger.Logs", "Warning - " + BaseAddInfo.GetMaximumLengthTruncateMessage("NString", TestAddInfo.Schema.UZ_NStringMaxLength, longValue), logger.Logs);

			logger = new TestErrorLogger();
			longValue = "你好".PadRight(TestAddInfo.Schema.UZ_NStringMaxLength, '1');
			value = function(logger, longValue);
			AssertEquals(longValue, value);
			AssertEquals("logger.Logs", "", logger.Logs);
		}

		public void TestIsValidInfo()
		{
			var dummy = Factory.New<DummyBusinessObjectWithIAddInfoManager>();
			var addinfo = dummy.AddInfo;

			var isValid = addinfo.IsValidInfo(addinfo.UZ_StringInfo);
			Assert(isValid);

			isValid = addinfo.IsValidInfo(addinfo.UZ_SomePropertyInfo);
			Assert(!isValid);
		}

		public void TestDbAddInfoShouldNotOverrideChangedData()
		{
			var dummy = Factory.New<DummyBusinessObjectWithIAddInfoWithConcurrencyResolverSupporter>();
			var addInfo = dummy.AddInfo;
			addInfo.UZ_String = "Some Test Value";
			Factory.Save();
			AssertEquals("String=Some Test Value", dummy.Z0_VarCharMax);

			for (var i = 1; i < 11; i++)
			{
				var factory = new BusinessObjectFactory();
				dummy = factory.Load<DummyBusinessObjectWithIAddInfoWithConcurrencyResolverSupporter>(dummy.PK);
				addInfo = dummy.AddInfo;
				var newData = "New Data" + i.ToString();
				addInfo.UZ_String = newData;
				addInfo.UpdateRelatedPropertyInfo();
				AssertEquals(true, addInfo.HasChangesSinceLastSaving(TestAddInfoSchema.UZ_String));
				factory.Save();
				AssertEquals($"String={newData}", dummy.Z0_VarCharMax);
				AssertEquals(false, addInfo.HasChangesSinceLastSaving(TestAddInfoSchema.UZ_String));
			}
		}

		public void TestHasChangesSinceLastSaving()
		{
			var declaration = Factory.New<BaseJobDeclarationWithAddInfo>();
			var addInfo = new TestAddInfo(declaration.JE_AddInfoInfo);
			AssertEquals("[PRE-CONDITION] IsInDatabase", false, addInfo.IsInDatabase);
			AssertEquals("[PRE-CONDITION] HasChangesSinceLastSaving when !IsInDatabase", false, addInfo.HasChangesSinceLastSaving(TestAddInfoSchema.UZ_String));

			Factory.Save();
			AssertEquals($"HasChangesSinceLastSaving just after saving", false, addInfo.HasChangesSinceLastSaving(TestAddInfoSchema.UZ_String));

			addInfo.UZ_String = "Some Test Value";
			AssertEquals($"HasChangesSinceLastSaving with unsaved changes", true, addInfo.HasChangesSinceLastSaving(TestAddInfoSchema.UZ_String));

			Factory.Save();
			AssertEquals($"HasChangesSinceLastSaving after saving again", false, addInfo.HasChangesSinceLastSaving(TestAddInfoSchema.UZ_String));
		}

		public void TestGetOriginalValue()
		{
			var declaration = Factory.New<BaseJobDeclarationWithAddInfo>();
			var addInfo = new TestAddInfo(declaration.JE_AddInfoInfo);
			AssertEquals("[PRE-CONDITION] IsInDatabase", false, addInfo.IsInDatabase);
			AssertNull("[PRE-CONDITION] GetOriginalValue when !IsInDatabase", addInfo.GetOriginalValue(TestAddInfoSchema.UZ_String));

			addInfo.UZ_String = "Some Test Value";
			Factory.Save();
			AssertEquals($"GetOriginalValue after modifying and saving", "Some Test Value", addInfo.GetOriginalValue(TestAddInfoSchema.UZ_String));

			addInfo.UZ_String = "Another Test Value";
			AssertEquals($"GetOriginalValue with unsaved changes", "Some Test Value", addInfo.GetOriginalValue(TestAddInfoSchema.UZ_String));
			AssertEquals($"HasChangesSinceLastSaving with unsaved changes", true, addInfo.HasChangesSinceLastSaving(TestAddInfoSchema.UZ_String));

			Factory.Save();
			AssertEquals($"GetOriginalValue after saving new value", "Another Test Value", addInfo.GetOriginalValue(TestAddInfoSchema.UZ_String));
		}

		public void TestDbAddInfo()
		{
			var declaration = Factory.New<BaseJobDeclarationWithAddInfo>();
			var addInfo = new TestAddInfo(declaration.JE_AddInfoInfo);
			Factory.Save();

			var dbAddInfo = addInfo.DbAddInfoExposed;
			AssertNotNull("DbAddInfo should be created", dbAddInfo);
			Assert("IsValidationSuspended", dbAddInfo.IsValidationSuspended);
			AssertEquals("DbAddInfo.Parent", declaration, dbAddInfo.Parent);
			Assert("DbAddInfo should not be a child of the declaration", !declaration.IsRegisteredEditableChildObject(dbAddInfo));
		}

		public void TestLoadPropertiesFromString_ShouldNotCall_InvalidateCachedProperties()
		{
			var declaration = Factory.New<BaseJobDeclarationWithAddInfo>();
			declaration.JE_AddInfo = "String=123";
			var versionBefore = Factory.CacheVersion;
			var addInfo = new TestAddInfoWithNewConstructor(declaration.JE_AddInfoInfo);
			var versionAfter = Factory.CacheVersion;
			AssertEquals("Cache version should not change when first loading AddInfo properties from String", versionBefore, versionAfter);
		}

		public void TestLoadPropertiesFromString_ResetDefaultValues()
		{
			CombineAssertions(() =>
			{
				bO.AddInfo.UZ_String = "12345";
				bO.AddInfo.LoadPropertiesFromString(ZString.Empty, false);
				AssertEquals("property value not reset when not clearExisting and addInfoString is empty", "12345", bO.AddInfo.UZ_String);

				bO.AddInfo.UZ_String = "12345";
				bO.AddInfo.LoadPropertiesFromString(ZString.Empty, true);
				AssertEquals("property value reset when clearExisting and addInfoString is empty", ZString.Empty, bO.AddInfo.UZ_String);

				bO.AddInfo.UZ_String = "12345";
				bO.AddInfo.LoadPropertiesFromString("SomeProperty=4321", true);
				AssertEquals("property value reset when clearExisting and addInfoString is not empty", ZString.Empty, bO.AddInfo.UZ_String);

				bO.AddInfo.UZ_String = "12345";
				bO.AddInfo.LoadPropertiesFromString("SomeProperty=4321", false);
				AssertEquals("property value not reset when not clearExisting and addInfoString is not empty", "12345", bO.AddInfo.UZ_String);
			});
		}

		public void TestSetupEventsAndLoadValues()
		{
			CombineAssertions(() =>
			{
				bO.AddInfo.UZ_NString = "UZ\u73a8";
				bO.UZ_StringInfo.Value = ZString.Empty;
				bO.AddInfo.SetupEventsAndLoadValuesExposed(bO.UZ_StringInfo, false);
				AssertEquals("property value should not reset when not IsInitialised and addInfoString is empty", "UZ\u73a8", bO.AddInfo.UZ_NString);

				bO.AddInfo.UZ_NString = "UZ\u73a8";
				bO.AddInfo.SetupEventsAndLoadValuesExposed(bO.UZ_StringInfo, true);
				AssertEquals("property value should reset when IsInitialised and addInfoString is empty", ZString.Empty, bO.AddInfo.UZ_NString);
			});
		}

		public void TestOnFactorySavedResetsDbAddInfo()
		{
			var originalDbAddInfo = bO.AddInfo.DbAddInfoExposed;
			Factory.Save();
			AssertEquals(false, ReferenceEquals(originalDbAddInfo, bO.AddInfo.DbAddInfoExposed));
		}

		TestBO bO;
		protected override void SetUp()
		{
			base.SetUp();
			bO = new TestBO(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var dec = factory.New<BaseJobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = new DummyInvoiceLineCollection(invoice).AddNew();
			return invoiceLine.AddInfo;
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest() => GetNewBusinessObjectForDeleteTest(Factory);

		class AutoTestBO : NonPersistentBusinessObject, ILightValidationInternals
		{
			public AutoTestBO(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			ZString fZY_AddInfo;
			public virtual ZString ZY_AddInfo
			{
				get { return fZY_AddInfo; }
				set
				{
					if (fZY_AddInfo != value)
					{
						SetNonPersistentPropertyValue(ZY_AddInfoInfo, ref fZY_AddInfo, value);
					}
				}
			}

			public ZPropertyInfo ZY_AddInfoInfo
			{
				get { return GetZPropertyInfo(nameof(ZY_AddInfo)); }
			}

			ZString fZY_NAddInfo;

			public ZString ZY_NAddInfo
			{
				get { return fZY_NAddInfo; }
				set
				{
					if (fZY_NAddInfo != value)
					{
						SetNonPersistentPropertyValue(ZY_NAddInfoInfo, ref fZY_NAddInfo, value);
					}
				}
			}

			public ZPropertyInfo ZY_NAddInfoInfo
			{
				get { return GetZPropertyInfo(nameof(ZY_NAddInfo)); }
			}

			public bool IsInDatabaseExposed;
			public override bool IsInDatabase
			{
				get { return IsInDatabaseExposed; }
			}

			public BaseAddInfo BaseAddInfo
			{
				get { return AddInfo; }
			}

			TestAddInfo fAddInfo;
			public TestAddInfo AddInfo
			{
				get
				{
					if (fAddInfo == null)
					{
						fAddInfo = new TestAddInfo(ZY_AddInfoInfo);
					}
					return fAddInfo;
				}
			}

			public ZString Z1_String
			{
				get;
				set;
			}

			public ZPropertyInfo Z1_StringInfo
			{
				get { return GetZPropertyInfo(nameof(Z1_String)); }
			}

			public ZString Z1_NString
			{
				get;
				set;
			}

			public ZPropertyInfo Z1_NStringInfo
			{
				get { return GetZPropertyInfo(nameof(Z1_NString)); }
			}

			public ZDateTime Z1_Date
			{
				get;
				set;
			}

			public ZPropertyInfo Z1_DateInfo
			{
				get { return GetZPropertyInfo(nameof(Z1_Date)); }
			}

			public ZString UZ_String // Same prefix as current bizObj
			{
				get
				{
					var result = AddInfo.UZ_String;
					return result.IsEmpty ? Z1_String : result;
				}
				set
				{
					AddInfo.UZ_String = value == Z1_String ? ZString.Empty : value;
				}
			}

			public ZPropertyInfo UZ_StringInfo
			{
				get { return GetWrappedZPropertyInfo(nameof(UZ_String), (x) => AddInfo.UZ_StringInfo); }
			}

			public ZString UZ_NString
			{
				get
				{
					var result = AddInfo.UZ_NString;
					return result.IsEmpty ? Z1_NString : result;
				}
				set
				{
					AddInfo.UZ_NString = value == Z1_NString ? ZString.Empty : value;
				}
			}

			public ZPropertyInfo UZ_NStringInfo
			{
				get { return GetWrappedZPropertyInfo(nameof(UZ_NString), (x) => AddInfo.UZ_NStringInfo); }
			}

			public ZDateTime ZY_Date // Same prefix as current bizObj
			{
				get
				{
					var result = AddInfo.UZ_Date;
					return result.IsEmpty ? Z1_Date : result;
				}
				set
				{
					AddInfo.UZ_Date = value == Z1_Date ? ZDateTime.Empty : value;
				}
			}

			public ZPropertyInfo ZY_DateInfo
			{
				get { return GetWrappedZPropertyInfo(nameof(ZY_Date), (x) => AddInfo.UZ_DateInfo); }
			}

			public ZBool ZY_IsValid
			{
				get { return fZY_IsValid; }
				set { fZY_IsValid = value; }
			}
			ZBool fZY_IsValid;

			SchemaBoolColumn ILightValidationInternals.IsValidSchemaColumn
			{
				get { return new SchemaBoolColumn(null, ZY_IsValidInfo.Name, 0, false, false, false); }
			}

			ZBool ILightValidationInternals.IsValid
			{
				get { return ZY_IsValid; }
				set { ZY_IsValid = value; }
			}

			bool ILightValidationInternals.IsValidHasChanges
			{
				get { return ZY_IsValidInfo.HasChanges; }
			}

			public ZPropertyInfoBool ZY_IsValidInfo
			{
				get { return new ZPropertyInfoBool(this, "ZY_IsValid"); }
			}
		}

		sealed class TestBO : AutoTestBO, IAddInfoManager, INAddInfoSupporter
		{
			public TestBO(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			IAddInfo IAddInfoManager.AddInfo
			{
				get { return AddInfo; }
			}

			public override ZString ZY_AddInfo
			{
				get
				{
					return AddInfoParser.ConcatAddInfoStrings(base.ZY_AddInfo, base.ZY_NAddInfo);
				}
				set
				{
					var addInfoStrings = AddInfo.SplitAddInfoString(value);

					base.ZY_AddInfo = addInfoStrings.Item1;
					base.ZY_NAddInfo = addInfoStrings.Item2;
				}
			}

			ZPropertyInfoString INAddInfoSupporter.NAddInfoProperty => ZY_NAddInfoInfo as ZPropertyInfoString;
		}

		sealed class DummyInvoiceLineCollection : DependentBusinessObjectCollection<BaseJobComInvoiceLineWithAddInfo, BaseJobComInvoiceHeader>
		{
			public DummyInvoiceLineCollection(BaseJobComInvoiceHeader invoice)
				: base(invoice)
			{
			}
		}

		sealed class BaseJobComInvoiceLineWithAddInfo : BaseJobComInvoiceLine, IAddInfoManager, INAddInfoSupporter
		{
			public BaseJobComInvoiceLineWithAddInfo(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				var testheader = factory.NewWithValidTestData<BaseJobComInvoiceHeader>();
				JI_JZ = testheader.PK;
			}

			public TestAddInfo AddInfo
			{
				get
				{
					if (addInfo == null)
					{
						addInfo = new TestAddInfo(this);
						RegisterEditableChildObject(addInfo);
					}
					return addInfo;
				}
			}
			TestAddInfo addInfo;

			IAddInfo IAddInfoManager.AddInfo
			{
				get { return AddInfo; }
			}

			ZPropertyInfoString INAddInfoSupporter.NAddInfoProperty => JI_NAddInfoInfo as ZPropertyInfoString;
		}
	}
}
