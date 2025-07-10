using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static NUnit.Framework.Assertion;

namespace Enterprise.Customs.PL.Business.Testing;

public static class TestHelper
{
	internal static IDisposable TemporarilySetIsAESTransitionPeriod(this JobDeclaration declaration, bool enabled)
	{
		return ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Poland, ZDate.Today, enabled);
	}

	internal static void AddCodeToCusMap_EUNAU(this BusinessObjectFactory factory, string cw1Code, string customsCode = "")
	{
		AddCodeToCusMap_EUNAU(new UniversalReferenceTestDataHelper(factory), cw1Code, customsCode);

		factory.Save();
	}

	internal static void AddCodeToCusMap_EUNAU(this BusinessObjectFactory factory, params (string cw1Code, string customsCode)[] pairs)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);

		foreach (var pair in pairs)
		{
			AddCodeToCusMap_EUNAU(helper, pair.cw1Code, pair.customsCode);
		}

		factory.Save();
	}

	static void AddCodeToCusMap_EUNAU(UniversalReferenceTestDataHelper helper, string cw1Code, string customsCode = "")
	{
		if (string.IsNullOrEmpty(customsCode))
		{
			customsCode = cw1Code;
		}

		var eunau = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU;
		var startDate = ZDateTime.Today.AddDays(-1);
		var endDate = ZDateTime.Today.AddDays(1);
		helper.CreateCusMapType(eunau, MapDirectionList.Codes.OUT, eunau, true);
		helper.CreateCusMap(eunau, cw1Code, customsCode, startDate, endDate, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
	}

	public static void GenerateGlbExternalPasswordPL(GlbStaff staff, ZString mailBoxID, ZString currentPassword, ZGuid? branchPK = null)
	{
		var password = GlbStaffWrapper.Get(staff).GlbExternalPassword;
		password.GP_MailBoxID = mailBoxID;
		password.CurrentDecryptedPassword = currentPassword;
		password.GP_GS = staff.PK;
		password.GP_GB = branchPK ?? GlbBranch.CurrentBranch.PK;
	}

	public static IEnumerable<string> GetPublicStringConstantValues(Type classType)
		=> classType.GetFields(BindingFlags.Public | BindingFlags.Static)
			.Where(field => field.IsLiteral && !field.IsInitOnly)
			.Select(field => field.GetValue(null)?.ToString())
			.WhereNotNull();

	public static IReadOnlyList<RefCusCodeList> CreateCustomOfficesForTest(this BusinessObjectFactory factory,
		params (string code, string description)[] offices)
		=> factory.CreateCustomOfficesForTest(startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTimeValue, offices);

	public static IReadOnlyList<RefCusCodeList> CreateCustomOfficesForTest(this BusinessObjectFactory factory,
		ZDateTime startDate,
		ZDateTime endDate,
		params (string code, string description)[] offices)
	{
		if (offices.Length == 0)
		{
			return Array.Empty<RefCusCodeList>();
		}

		const string officeCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
		var helper = new UniversalReferenceTestDataHelper(factory);

		var oldGrouping = string.Empty;
		return offices.Select(x => CreateOffice(x.code, x.description)).ToList();

		RefCusCodeList CreateOffice(string code, string description)
		{
			if (code.Length < 2)
			{
				throw new ArgumentException("Office code must include the country code!", nameof(code));
			}
			var grouping = code.Substring(0, 2);
			if (!string.Equals(oldGrouping, grouping))
			{
				helper.CreateNewOrGetExistingDataGrouping(grouping);
				helper.CreateNewOrGetExistingCusCodeType(officeCodeType, $"Test code type {officeCodeType}", grouping);
				oldGrouping = grouping;
			}
			return helper.CreateCusCodeList(grouping, officeCodeType, code, description, startDate, endDate);
		}
	}

	public static string ToSingleLineHtml(this string str)
	{
		var sb = new StringBuilder(str);
		sb.Replace(System.Environment.NewLine, string.Empty);
		sb.Replace("\t", string.Empty);
		return sb.ToString();
	}

	public static string GetTextBetween(this string source, string start, string end)
	{
		if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end))
		{
			return string.Empty;
		}

		var pattern = $"{Regex.Escape(start)}(.*?){Regex.Escape(end)}";
		var match = Regex.Match(source, pattern);

		if (match.Success)
		{
			return match.Groups[1].Value.Trim();
		}

		return string.Empty;
	}

	public static void TestCollectionHasCorrectIndexOrder<TObj>(IReadOnlyCollection<TObj> collection,
		Func<TObj, int> getIndex,
		int expectedStartingSequenceNumber = 1)
		where TObj : class
	{
		var itemsCount = collection.Count;
		AssertEquals("Please use at least 2 items for it to be valid test case", true, itemsCount >= 2);
		for (var i = 0; i < itemsCount; i++)
		{
			var index = getIndex(collection.ElementAt(i));
			var expected = expectedStartingSequenceNumber + i;
			AssertEquals($"{typeof(TObj)} - got sequence number:[{index}] - expected sequence number:[{expected}]", expected, index);
		}
	}

	public static PropertyInfo GetPropertyInfo<TSource, TProperty>(this Expression<Func<TSource, TProperty>> propertyLambda)
	{
		if (propertyLambda == null)
		{
			throw new ArgumentNullException(nameof(propertyLambda));
		}

		var type = typeof(TSource);

		if (propertyLambda.Body is not MemberExpression member)
		{
			throw new ArgumentException($"Expression '{propertyLambda}' refers to a method, not a property.");
		}

		if (member.Member is not PropertyInfo propInfo)
		{
			throw new ArgumentException($"Expression '{propertyLambda}' refers to a field, not a property.");
		}

		if (type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType))
		{
			throw new ArgumentException($"Expression '{propertyLambda}' refers to a property that is not from type {type}.");
		}

		return propInfo;
	}
}
