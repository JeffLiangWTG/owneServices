using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.Universal.ConditionChecker;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusConditionValue))]
	public class RefCusConditionValueTesting : EnterpriseBusinessObjectTestCase
	{
		public void TestIsDocumentConditionValue()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var conditionValueType1 = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.China, Constants.ConditionValueType.Information);
			var conditionValueType2 = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.China, Constants.ConditionValueType.SupportingDocument);
			var conditionValueType3 = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.China, Constants.ConditionValueType.SupportingDocumentNoReferenceNumber);
			Factory.Save();

			var testConditionValue = (RefCusConditionValue)GetNewBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("Not document type", false, testConditionValue.IsDocumentConditionValue);

				testConditionValue.ZX3_ZX4_ValueType = conditionValueType1.PK;
				AssertEquals("INF type", false, testConditionValue.IsDocumentConditionValue);

				testConditionValue.ZX3_ZX4_ValueType = conditionValueType2.PK;
				AssertEquals("SUP type", true, testConditionValue.IsDocumentConditionValue);

				testConditionValue.ZX3_ZX4_ValueType = conditionValueType3.PK;
				AssertEquals("SNR type", true, testConditionValue.IsDocumentConditionValue);
			});
		}

		public void TestIsInformationConditionValue()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var conditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.China, Constants.ConditionValueType.Information);
			Factory.Save();

			var testConditionValue = (RefCusConditionValue)GetNewBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("Not INF type", false, testConditionValue.IsInformationConditionValue);

				testConditionValue.ZX3_ZX4_ValueType = conditionValueType.PK;
				AssertEquals("Is INF type", true, testConditionValue.IsInformationConditionValue);
			});
		}

		public void TestIsConditionMet_IsInformationConditionValue()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var conditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.China, "INF");
			Factory.Save();

			var testConditionValue = (RefCusConditionValue)GetNewBusinessObject();
			EvaluateConditionValue validator = (_, __, input) => false;

			CombineAssertions(() =>
			{
				AssertEquals("Not INF type", false, testConditionValue.IsConditionMet(validator));

				testConditionValue.ZX3_ZX4_ValueType = conditionValueType.PK;
				AssertEquals("Is INF type", true, testConditionValue.IsConditionMet(validator));
			});
		}

		public void TestIsConditionValueMet()
		{
			var testConditionValue = (RefCusConditionValue)GetNewBusinessObject();
			testConditionValue.ZX3_Value = "1A";
			var testConditionValue2 = (RefCusConditionValue)GetNewBusinessObject();
			testConditionValue2.ZX3_Value = "0C";
			var validList = new[] { "1a", "0C" }.ToList();
			EvaluateConditionValue validator = (_, __, input) => validList.Contains(input);
			AssertEquals(false, testConditionValue.IsConditionMet(validator));
			AssertEquals(true, testConditionValue2.IsConditionMet(validator));
		}

		public void TestPropertyNamesFromFriends()
		{
			var refCusConditionCreator = new RefCusConditionCreatorForTest(Factory);
			var cusConditionValue = refCusConditionCreator.CreateRefCusConditionValue();
			Factory.Save();
			AssertEquals("DOC", cusConditionValue.ValueType);
			AssertEquals("DOC DESC", cusConditionValue.ValueTypeDescription);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => new RefCusConditionCreatorForTest(factory).CreateRefCusConditionValue();
	}
}
