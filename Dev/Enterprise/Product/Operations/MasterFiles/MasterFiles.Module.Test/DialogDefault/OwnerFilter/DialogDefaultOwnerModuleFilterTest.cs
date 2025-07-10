using System;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core.DialogDefault;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.DialogDefault.Testing
{
	[TestedType(typeof(DialogDefaultOwnerModuleFilter))]
	sealed class DialogDefaultOwnerModuleFilterTest : ModuleFilterTestCase<DialogDefaultOwnerModuleFilter>
	{
		StmDialogDefault NewDefault(string level, ZGuid ownerPk)
		{
			var item = Factory.NewWithValidTestData<StmDialogDefault>();
			item.SDD_Level = level;
			item.SDD_Owner = ownerPk;

			return item;
		}

		public void TestOwnerPkIsReadOnly()
		{
			var readOnlyCodes = new[] { DialogDefaultLevel.Codes.Global };
			var notReadOnlyCodes = new DialogDefaultLevel()
				.Cast<ICodeDescription>()
				.Select(pair => pair.Code)
				.Except(readOnlyCodes);

			CombineAssertions(() =>
			{
				foreach (var code in readOnlyCodes)
				{
					Filter.LevelCode = code;
					Assert(code + " should be read only", Filter.OwnerPk_ReadOnly);
				}

				foreach (var code in notReadOnlyCodes)
				{
					Filter.LevelCode = code;
					Assert(code + " should NOT be read only", !Filter.OwnerPk_ReadOnly);
				}
			});
		}

		public void TestGetQuery_BlankOwner()
		{
			NewDefault(DialogDefaultLevel.Codes.User, ZGuid.NewZGuid());
			NewDefault(DialogDefaultLevel.Codes.User, ZGuid.NewZGuid());
			NewDefault(DialogDefaultLevel.Codes.User, ZGuid.NewZGuid());

			NewDefault(DialogDefaultLevel.Codes.Company, ZGuid.NewZGuid());
			NewDefault(DialogDefaultLevel.Codes.Company, ZGuid.NewZGuid());

			NewDefault(DialogDefaultLevel.Codes.Global, ZGuid.Empty);

			Factory.Save();

			Filter.LevelCode = DialogDefaultLevel.Codes.User;
			AssertEquals(3, Factory.GetDatabaseCount(typeof(StmDialogDefault), Filter.Query));

			Filter.LevelCode = DialogDefaultLevel.Codes.Company;
			AssertEquals(2, Factory.GetDatabaseCount(typeof(StmDialogDefault), Filter.Query));

			Filter.LevelCode = DialogDefaultLevel.Codes.Global;
			AssertEquals(1, Factory.GetDatabaseCount(typeof(StmDialogDefault), Filter.Query));
		}

		public void TestGetQuery_Company_Specific()
		{
			NewDefault(DialogDefaultLevel.Codes.User, ZGuid.NewZGuid());
			NewDefault(DialogDefaultLevel.Codes.Global, ZGuid.Empty);

			NewDefault(DialogDefaultLevel.Codes.Company, ZGuid.NewZGuid());
			var companyDefault = NewDefault(DialogDefaultLevel.Codes.Company, ZGuid.NewZGuid());

			Factory.Save();

			Filter.LevelCode = DialogDefaultLevel.Codes.Company;
			Filter.OwnerPk = companyDefault.SDD_Owner;

			AssertEquals(companyDefault.PK, Factory.Load<StmDialogDefault>(Filter.Query).Single().PK);
		}

		public void TestGetQuery_User_Specific()
		{
			NewDefault(DialogDefaultLevel.Codes.Company, ZGuid.NewZGuid());
			NewDefault(DialogDefaultLevel.Codes.Global, ZGuid.Empty);

			NewDefault(DialogDefaultLevel.Codes.User, ZGuid.NewZGuid());
			var userDefault = NewDefault(DialogDefaultLevel.Codes.User, ZGuid.NewZGuid());

			Factory.Save();

			Filter.LevelCode = DialogDefaultLevel.Codes.User;
			Filter.OwnerPk = userDefault.SDD_Owner;

			AssertEquals(userDefault.PK, Factory.Load<StmDialogDefault>(Filter.Query).Single().PK);
		}

		public void TestOwnerList()
		{
			Filter.LevelCode = DialogDefaultLevel.Codes.Company;
			AssertEquals(typeof(GlbCompanyCollection), Filter.OwnerPkList.GetType());

			Filter.LevelCode = DialogDefaultLevel.Codes.User;
			AssertEquals(typeof(GlbStaffCollection), Filter.OwnerPkList.GetType());
		}

		public void TestLevelCodeListHasAllCodes()
		{
			Func<CodeDescriptionPairList, string> format = (pairs) => string.Join(", ", pairs.Cast<ICodeDescription>().Select(pair => pair.Code).OrderBy(c => c));

			var expectedCodes = format(new DialogDefaultLevel());
			var actualCodes = format(Filter.LevelCodeList);
			AssertEquals(expectedCodes, actualCodes);
		}

		public void TestCopyValues()
		{
			var firstFilter = GetNewModuleFilter();
			var secondFilter = GetNewModuleFilter();

			var ownerPk = ZGuid.NewZGuid();

			firstFilter.LevelCode = DialogDefaultLevel.Codes.Company;
			firstFilter.OwnerPk = ownerPk;

			secondFilter.CopyPersistantValuesFromFilter_Exposed(firstFilter);

			AssertEquals(DialogDefaultLevel.Codes.Company, secondFilter.LevelCode);
			AssertEquals(ownerPk, secondFilter.OwnerPk);
		}

		#region Implementation

		public override void TestIsExpensiveQuery()
		{
			Assert(!Filter.IsExpensiveQuery);
		}

		protected override DialogDefaultOwnerModuleFilter GetNewModuleFilter()
		{
			return new DialogDefaultOwnerModuleFilter("Owner");
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Other; }
		}

		protected override ZString ExpectedDescription
		{
			get { return "Owner"; }
		}

		#endregion
	}
}
