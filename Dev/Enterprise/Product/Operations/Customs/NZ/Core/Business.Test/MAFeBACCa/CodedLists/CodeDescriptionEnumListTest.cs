using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists.Testing
{
	sealed class CodeDescriptionEnumListTest : TestCaseWithFactory
	{
		public void TestGetEnumValue()
		{
			var codeDescriptionEnum = new CodeDescriptionEnumListForTest();
			AssertEquals(EnumForTest.Bill, codeDescriptionEnum.GetEnumValue(CodeDescriptionEnumListForTest.Codes.Bill));
			AssertEquals(EnumForTest.Ted, codeDescriptionEnum.GetEnumValue(CodeDescriptionEnumListForTest.Codes.Ted));
			AssertEquals(null, codeDescriptionEnum.GetEnumValue(""));
			AssertEquals(null, codeDescriptionEnum.GetEnumValue("WHATEVA!!!"));

			AssertEquals(EnumForTest.Bill, codeDescriptionEnum.GetEnumValueOrDefault(CodeDescriptionEnumListForTest.Codes.Bill, EnumForTest.Ted));
			AssertEquals(EnumForTest.Ted, codeDescriptionEnum.GetEnumValueOrDefault(CodeDescriptionEnumListForTest.Codes.Ted, EnumForTest.Bill));
			AssertEquals(EnumForTest.Ted, codeDescriptionEnum.GetEnumValueOrDefault("", EnumForTest.Ted));
			AssertEquals(EnumForTest.Ted, codeDescriptionEnum.GetEnumValueOrDefault("WHATEVA!!!", EnumForTest.Ted));
		}

		enum EnumForTest { Bill, Ted }

		class CodeDescriptionEnumListForTest : CodeDescriptionEnumList<EnumForTest>
		{
			public static class Codes
			{
				public const string Bill = "Bill";
				public const string Ted = "Ted";
			}
			public CodeDescriptionEnumListForTest()
			{
				AddPair(Codes.Bill, "Bill Gates", EnumForTest.Bill);
				AddPair(Codes.Ted, "Ted Danson", EnumForTest.Ted);
			}
		}
	}

	abstract class CodeDescriptionPairListTestCase : TestCaseWithFactory
	{
		public void TestCodesAreAll3Character()
		{
			var list = GetNewList();
			foreach (ICodeDescription element in list)
			{
				AssertEquals("[" + element.Code + "] - Invalid Length.", 3, element.Code.Length);
			}
		}

		public void TestNoDuplicateCodes()
		{
			var elements = new Dictionary<string, ICodeDescription>();
			var list = GetNewList();
			foreach (ICodeDescription element in list)
			{
				if (elements.ContainsKey(element.Code))
				{
					Fail("Code [" + element.Code + "] found twice with descriptions of [" + elements[element.Code].Description + "] and [" + element.Description + "].");
				}
				elements.Add(element.Code, element);
			}
			Assert(true);
		}

		protected abstract CodeDescriptionPairList GetNewList();
	}

	abstract class CodeDescriptionEnumListTestCase : CodeDescriptionPairListTestCase
	{
		public void TestAllEnumValuesAreCoveredAndNotDuplicated()
		{
			var list = GetNewList();

			var getEnumTypeMethod = list.GetType().GetMethod("GetEnumTypeForTesting");
			var enumType = (Type)getEnumTypeMethod.Invoke(list, null);
			var elementNamesInEnum = Enum.GetNames(enumType);

			var getEnumValuesInListMethod = list.GetType().GetMethod("GetEnumValuesInListForTesting");
			var enumValuesInList = (string[])getEnumValuesInListMethod.Invoke(list, null);

			var enumValues = new List<string>();
			foreach (var enumValue in enumValuesInList)
			{
				if (enumValues.Contains(enumValue))
				{
					Fail("[" + enumType.Name + "." + enumValue + "] has been added to the List at least twice.");
				}
				enumValues.Add(enumValue);
			}

			foreach (var enumValue in elementNamesInEnum)
			{
				if (!enumValues.Contains(enumValue))
				{
					Fail("[" + enumType.Name + "." + enumValue + "] is missing from the list.");
				}
			}

			Assert(true);
		}
	}
}
