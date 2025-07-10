using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;
using CoreConstants = Enterprise.Core.Constants;

namespace Enterprise.Customs.Universal.Testing
{
	class RefCusConditionCreatorForTest
	{
		[ThreadSafe]
		static int tariffNumBasic = 1;
		readonly UniversalReferenceTestDataHelper helper;
		readonly BusinessObjectFactory factory;
		public RefCusConditionCreatorForTest(BusinessObjectFactory factory)
		{
			this.factory = factory;
			helper = new UniversalReferenceTestDataHelper(factory);
		}

		public RefCusConditionType CreateRefCusConditionType()
		{
			var result = helper.CreateOrGetExistingRefCusConditionType(CoreConstants.CountryCodes.China, CoreConstants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "CIQCI", "CIQCI DESC");
			factory.Save();
			return result;
		}

		public RefCusConditionValueType CreateRefCusConditionValueType()
		{
			var result = helper.CreateOrGetExistingRefCusConditionValueType(CoreConstants.CountryCodes.China, "DOC", "DOC DESC");
			factory.Save();
			return result;
		}

		public RefCusCondition CreateRefCusCondition()
		{
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariffType = helper.CreateNewOrGetExistingTariffType(CoreConstants.CountryCodes.China, "SHN");
			factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, tariffNumBasic++.ToString(), minDate, maxDate);
			var conditionType = CreateRefCusConditionType();
			var result = helper.CreateOrGetExistingRefCusCondition(CoreConstants.CountryCodes.China, conditionType.PK, tariff.PK, "COD2", true, true, minDate, maxDate);
			factory.Save();
			return result;
		}

		public RefCusConditionCode CreateRefCusConditionCode()
		{
			var result = helper.CreateOrGetExistingRefCusConditionCode(CoreConstants.CountryCodes.China, "ABC");
			factory.Save();
			return result;
		}

		public RefCusConditionValue CreateRefCusConditionValue()
		{
			var result = helper.CreateOrGetExistingRefCusConditionValue(CreateRefCusConditionValueType().PK, CreateRefCusCondition().PK, "1L");
			factory.Save();
			return result;
		}

		public RefCusConditionLanguage CreateRefCusConditionLanguage(ZString languageType, ZString source, ZString comment)
		{
			var result = helper.CreateOrGetExistingRefCusConditionLanguage(CreateRefCusCondition().PK, languageType, source, comment);
			factory.Save();
			return result;
		}
	}
}
