using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Testing
{
	class RefCusCodeListAttributeTypesTest : TestCaseWithFactory
	{
		public void TestGetAttributeValuesFor()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType1 = helper.CreateCusCodeType("TT1", "TT1 DESC");
			var codeType2 = helper.CreateCusCodeType("TT2", "TT2 DESC");
			var code1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ethiopia, codeType1.ZZK_CodeType, "JOE", ZDateTime.BrettsBirthday, ZDateTime.Today);
			var attributeName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT1", "Desc.", codeType1.ZZK_CodeType, Core.Constants.CountryCodes.Ethiopia, codeType1.ZZK_CodeType);
			var attributeName3 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT2", "Desc.", codeType1.ZZK_CodeType, Core.Constants.CountryCodes.Ethiopia, codeType1.ZZK_CodeType);
			var code1Attribute1 = helper.CreateCusCodeListAttribute(code1.PK, "ATT1", "HI");
			var code1Attribute2 = helper.CreateCusCodeListAttribute(code1.PK, "ATT1", "HELLO");
			var code1Attribute3 = helper.CreateCusCodeListAttribute(code1.PK, "ATT2", "YO");
			var code2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ethiopia, codeType1.ZZK_CodeType, "BOB", ZDateTime.BrettsBirthday, ZDateTime.Today);
			var code2Attribute1 = helper.CreateCusCodeListAttribute(code2.PK, "ATT1", "GDay");
			var code2Attribute2 = helper.CreateCusCodeListAttribute(code2.PK, "ATT1", "YAH");
			var code2Attribute3 = helper.CreateCusCodeListAttribute(code2.PK, "ATT2", "YEH");
			var code3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, codeType1.ZZK_CodeType, "JAY", ZDateTime.BrettsBirthday, ZDateTime.Today);
			var code4 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ethiopia, codeType2.ZZK_CodeType, "JOY", ZDateTime.BrettsBirthday, ZDateTime.Today);
			Factory.Save();
			var date = ZDateTime.Today.AddMonths(-1);
			var dateKey = "_" + date.Date.ToString("yyMMdd");
			AssertEquals(0, RefCusCodeListAttributeTypes.GetAttributeValuesFor(null, Core.Constants.CountryCodes.Ethiopia, "TT1", date, "JOE", "ATT1").Length);
			AssertEquals(0, RefCusCodeListAttributeTypes.GetAttributeValuesFor(Factory, ZString.Empty, "TT1", date, "JOE", "ATT1").Length);
			AssertEquals(0, RefCusCodeListAttributeTypes.GetAttributeValuesFor(Factory, Core.Constants.CountryCodes.Ethiopia, ZString.Empty, date, "JOE", "ATT1").Length);
			AssertEquals(0, RefCusCodeListAttributeTypes.GetAttributeValuesFor(Factory, Core.Constants.CountryCodes.Ethiopia, "TT1", date, ZString.Empty, "ATT1").Length);
			AssertEquals(0, RefCusCodeListAttributeTypes.GetAttributeValuesFor(Factory, Core.Constants.CountryCodes.Ethiopia, "TT1", date, "JOE", ZString.Empty).Length);
			var values = RefCusCodeListAttributeTypes.GetAttributeValuesFor(Factory, Core.Constants.CountryCodes.Ethiopia, "TT1", date, "JOE", "ATT1");
			AssertEquals(2, values.Length);
			AssertEquals("HELLO", values[0]);
			AssertEquals("HI", values[1]);
			var dictionary1 = Factory.GetCachedValue(string.Format(CultureInfo.InvariantCulture, "RefCusCodeListAttributeValuesFor{0}_TT1_JOE" + dateKey, Core.Constants.CountryCodes.Ethiopia), () => new Dictionary<ZString, ZString[]>());
			var dictionary2 = Factory.GetCachedValue(string.Format(CultureInfo.InvariantCulture, "RefCusCodeListAttributeValuesFor{0}_TT1_JOE" + dateKey, Core.Constants.CountryCodes.Ethiopia), () => new Dictionary<ZString, ZString[]>());
			AssertEquals("IsCached", true, object.ReferenceEquals(dictionary1, dictionary2));
			AssertEquals(2, dictionary1.Count);
			AssertArrayEqualsByElements(new ZString[] { "HELLO", "HI", }, dictionary1["ATT1"].ToArray());
			AssertArrayEqualsByElements(new ZString[] { "YO", }, dictionary1["ATT2"].ToArray());
			values = RefCusCodeListAttributeTypes.GetAttributeValuesFor(Factory, Core.Constants.CountryCodes.Ethiopia, "TT1", date, "BOB", "ATT2");
			AssertEquals(1, values.Length);
			AssertEquals("YEH", values[0]);
			var dictionary3 = Factory.GetCachedValue(string.Format(CultureInfo.InvariantCulture, "RefCusCodeListAttributeValuesFor{0}_TT1_BOB" + dateKey, Core.Constants.CountryCodes.Ethiopia), () => new Dictionary<ZString, ZString[]>());
			var dictionary4 = Factory.GetCachedValue(string.Format(CultureInfo.InvariantCulture, "RefCusCodeListAttributeValuesFor{0}_TT1_BOB" + dateKey, Core.Constants.CountryCodes.Ethiopia), () => new Dictionary<ZString, ZString[]>());
			AssertEquals("Not Cached", false, object.ReferenceEquals(dictionary3, dictionary1));
			AssertEquals("IsCached", true, object.ReferenceEquals(dictionary3, dictionary4));
			AssertEquals(2, dictionary3.Count);
			AssertArrayEqualsByElements(new ZString[] { "GDay", "YAH", }, dictionary3["ATT1"].ToArray());
			AssertArrayEqualsByElements(new ZString[] { "YEH", }, dictionary4["ATT2"].ToArray());
			values = RefCusCodeListAttributeTypes.GetAttributeValuesFor(Factory, Core.Constants.CountryCodes.Ethiopia, "tt1", date, "joe", "att1");
			AssertEquals(2, values.Length);
			values = RefCusCodeListAttributeTypes.GetAttributeValuesFor(Factory, Core.Constants.CountryCodes.Ethiopia, "tt1", date, "bob", "att2");
			AssertEquals(1, values.Length);
		}
	}
}
