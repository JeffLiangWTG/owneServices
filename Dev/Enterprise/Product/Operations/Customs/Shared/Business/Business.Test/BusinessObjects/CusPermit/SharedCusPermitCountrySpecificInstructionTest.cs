using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class SharedCusPermitCountrySpecificInstructionTest<TInstruction> : TestCaseWithFactory
			where TInstruction : SharedCusPermitCountrySpecificInstruction
	{
		public void TestGetTypeList()
		{
			AssertType<PermitTypeList>(countrySpecificInstruction.GetTypeList());
		}

		public void TestGetSubTypeList()
		{
			AssertType<PermitSubTypeList>(countrySpecificInstruction.GetSubTypeList(""));
		}

		public void TestGetQtyValIndicatorList()
		{
			AssertType<PermitQtyValIndicatorList>(countrySpecificInstruction.GetQtyValIndicatorList("", ""));
		}

		public void TestGetTransactionTypeList()
		{
			AssertType<PermitTransactionTypeList>(countrySpecificInstruction.GetTransactionTypeList("", ""));
		}

		public void TestGetRuleCodeList()
		{
			AssertType<PermitRuleCodeList>(countrySpecificInstruction.GetRuleCodeList("", ""));
		}

		public void TestGetRuleCodeListForModule()
		{
			AssertType<PermitRuleCodeList>(countrySpecificInstruction.GetRuleCodeListForModule());
		}

		public void TestGetLookupList()
		{
			CombineAssertions(() =>
			{
				AssertType<MasterFiles.Business.RefCountryCollection>(countrySpecificInstruction.GetLookupList(null, "COO"));
				AssertType<CodeDescriptionPairList>(countrySpecificInstruction.GetLookupList(null, ""));
			});
		}

		public void TestGetPermitNumberCollection()
		{
			AssertNull(countrySpecificInstruction.GetPermitNumberCollection(null));
		}

		public void TestAppliesToIndicator()
		{
			AssertEquals("Applies To = ForceEmpty for no type", AppliesToIndicator.ForceEmpty, countrySpecificInstruction.GetAppliesToIndicator("", ""));
		}

		public void TestGetMatchingType()
		{
			AssertEquals("Default to Range", PermitMatchingType.Range, countrySpecificInstruction.GetMatchingType(""));
		}

		public void TestGetRangeComparer()
		{
			Assert(countrySpecificInstruction.GetRangeComparer("").GetType().Name == "CultureAwareComparer");
		}

		public void TestGetValueFromFieldType()
		{
			AssertEquals("Text", countrySpecificInstruction.GetValueFromFieldType(SharedCusPermitRule.RuleCodes.Tariff));
			AssertEquals("TextCodeFindBox", countrySpecificInstruction.GetValueFromFieldType(SharedCusPermitRule.RuleCodes.CountryOfOrigin));
		}
		public void TestGetValueToFieldType()
		{
			AssertEquals("Text", countrySpecificInstruction.GetValueToFieldType(SharedCusPermitRule.RuleCodes.Tariff));
			AssertEquals("Text", countrySpecificInstruction.GetValueToFieldType(SharedCusPermitRule.RuleCodes.CountryOfOrigin));
		}
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			countrySpecificInstruction = GetNewCusPermitCountrySpecificInstruction(Factory);
		}
		protected TInstruction countrySpecificInstruction;

		protected abstract TInstruction GetNewCusPermitCountrySpecificInstruction(BusinessObjectFactory factory);

		#endregion
	}
}
