using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CustomsNumberViewStmNumsCompanyProviderForTest))]
	sealed class CustomsNumberViewStmNumsCompanyProviderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetProvider()
		{
			AssertNull("IT Provider", CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(Factory, ZString.Empty, ZGuid.Empty));
			CustomsNumberViewStmNumsBusinessProvider provider = null;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				provider = CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(Factory, Core.Constants.CountryCodes.Italy, GlbCompany.CurrentCompany.PK);
				AssertEquals("IT Provider", "Enterprise.Customs.IT.GUI.ITCustomsNumberViewStmNumsCompanyProvider", provider.GetType().FullName);
				AssertEquals(true, object.ReferenceEquals(provider, CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(Factory, Core.Constants.CountryCodes.Italy, GlbCompany.CurrentCompany.PK)));

				var itCompany2 = Factory.NewWithValidTestData<GlbCompany>();
				itCompany2.GC_Code = "IT2";
				itCompany2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;

				var provider2 = CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(Factory, Core.Constants.CountryCodes.Italy, itCompany2.PK);
				AssertEquals("IT Provider for another IT Company", "Enterprise.Customs.IT.GUI.ITCustomsNumberViewStmNumsCompanyProvider", provider2.GetType().FullName);
				AssertEquals(false, object.ReferenceEquals(provider, provider2));
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Cambodia))
			{
				AssertNull("KH Provider", CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(Factory, Core.Constants.CountryCodes.Cambodia, GlbCompany.CurrentCompany.PK));
			}
		}

		public void TestTryGetNextCustomsNumber_CanRollOver()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			using (new CustomsNumberViewStmNumsCompanyProviderForTestSetUp())
			{
				CombineAssertions(() =>
				{
					var provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(new BusinessObjectFactory());
					provider.UsedNumbersForTesting = new Dictionary<ZString, List<ZString>>();
					var stmNums1 = provider.NewCustomsNumber();
					stmNums1.SN_MinimumValue = 2001L;
					stmNums1.SN_MaximumValue = 2002L;
					stmNums1.SN_CanRollover = true;
					var stmNums2 = provider.NewCustomsNumber();
					stmNums2.SN_MinimumValue = 1001L;
					stmNums2.SN_MaximumValue = 1002L;
					stmNums2.SN_CanRollover = true;
					var stmNums3 = provider.NewCustomsNumber();
					stmNums3.SN_MinimumValue = 3001L;
					stmNums3.SN_MaximumValue = 3003L;
					stmNums3.SN_CanRollover = true;
					provider.Factory.Save();
					var list = new List<ZString>();
					list.Add("00002001");
					list.Add("00003002");
					list.Add("00003003");
					provider.UsedNumbersForTesting.Add(stmNums1.SN_Type, list);
					var wrappers = provider.CustomsNumberWrappers.OfType<CustomsNumberViewStmNumsWrapper>().OrderBy(x => x.SN_SystemCreateTimeUtc + x.StmNums.Sequence.ToString());
					Assert("1", provider.TryGetNextCustomsNumber(Factory, wrappers, out var customNumber));
					AssertEquals("2", "00002002", customNumber);
					list.Add(customNumber);
					Assert("3", provider.TryGetNextCustomsNumber(Factory, wrappers, out customNumber));
					AssertEquals("4", "00001001", customNumber);
					list.Add(customNumber);
					Assert("5", provider.TryGetNextCustomsNumber(Factory, wrappers, out customNumber));
					AssertEquals("6", "00001002", customNumber);
					list.Add(customNumber);
					Assert("7", provider.TryGetNextCustomsNumber(Factory, wrappers, out customNumber));
					AssertEquals("8", "00003001", customNumber);
					list.Add(customNumber);
					Assert("9", !provider.TryGetNextCustomsNumber(Factory, wrappers, out customNumber));
					AssertEquals("10", "", customNumber);
				});
			}
		}

		public void TestTryGetNextCustomsNumber_CannotRollOver()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			using (new CustomsNumberViewStmNumsCompanyProviderForTestSetUp())
			{
				CombineAssertions(() =>
				{
					var provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(new BusinessObjectFactory());
					provider.UsedNumbersForTesting = new Dictionary<ZString, List<ZString>>();
					var stmNums1 = provider.NewCustomsNumber();
					stmNums1.SN_MinimumValue = 2001L;
					stmNums1.SN_MaximumValue = 2002L;
					stmNums1.SN_CanRollover = false;
					var stmNums2 = provider.NewCustomsNumber();
					stmNums2.SN_MinimumValue = 1001L;
					stmNums2.SN_MaximumValue = 1002L;
					stmNums2.SN_CanRollover = false;
					var stmNums3 = provider.NewCustomsNumber();
					stmNums3.SN_MinimumValue = 3001L;
					stmNums3.SN_MaximumValue = 3003L;
					stmNums3.SN_CanRollover = false;
					provider.Factory.Save();
					var list = new List<ZString>();
					list.Add("00002001");
					list.Add("00003002");
					list.Add("00003003");
					provider.UsedNumbersForTesting.Add(stmNums1.SN_Type, list);
					var wrappers = provider.CustomsNumberWrappers.OfType<CustomsNumberViewStmNumsWrapper>().OrderBy(x => x.SN_SystemCreateTimeUtc + x.StmNums.Sequence.ToString());
					Assert("1", provider.TryGetNextCustomsNumber(Factory, wrappers, out var customNumber));
					AssertEquals("2", "00002002", customNumber);
					list.Add(customNumber);
					Assert("3", provider.TryGetNextCustomsNumber(Factory, wrappers, out customNumber));
					AssertEquals("4", "00001001", customNumber);
					list.Add(customNumber);
					Assert("5", provider.TryGetNextCustomsNumber(Factory, wrappers, out customNumber));
					AssertEquals("6", "00001002", customNumber);
					list.Add(customNumber);
					Assert("7", provider.TryGetNextCustomsNumber(Factory, wrappers, out customNumber));
					AssertEquals("8", "00003001", customNumber);
					list.Add(customNumber);
					Assert("9", !provider.TryGetNextCustomsNumber(Factory, wrappers, out customNumber));
					AssertEquals("10", "", customNumber);
				});
			}
		}

		public void TestGetSetting()
		{
			var provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(Factory);
			var setting = provider.GetSetting("CEN");
			AssertEquals(true, object.ReferenceEquals(setting, provider.GetSetting("CEN")));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CustomsNumberViewStmNumsCompanyProviderForTest.New(Factory);
		}
	}
}
