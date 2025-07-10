using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(QueryQuotaVisaOption))]
	sealed class QueryVisaQuotaOptionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSendQuery()
		{
			var option = new QueryQuotaVisaOption(Factory, false);
			option.FillWithValidTestData();
			option.US_QueryType = QueryTypeList.Codes.QuotaRecords;
			option.SendQueryWithoutSaving();

			var nextNum = "EDIEDIDAT_" + Env.NumberFountains.EDIFACTNumberFountain("M", "ENT", EDIMessage.ApplicationCodes.USCustomsImport).PeekPreliminaryFormatted(Factory);

			Factory.Save();
			var message = new Messaging.Business.CBPEDIMessage.Loader(Factory).LoadTop1(EDIMessage.ApplicationCodes.USCustomsImport, ApplicationIdentifierCodeList.Codes.QueryQuota, nextNum);
			AssertNotNull(message);
			AssertEquals(EM_MessageSubTypeList.Codes.QuotaVisaQuery, message.EM_MessageSubType);
			var qTAU1s = message.GetMessageBlocks<QTAU1>();
			AssertEquals(1, qTAU1s.Count);
			AssertEquals("", qTAU1s[0].VisaQueryIndicator);

			option = new QueryQuotaVisaOption(Factory, true);
			option.SendQueryWithoutSaving();

			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.USCustomsImport);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, ACEApplicationIdentifierCodeList.Codes.QuotaQuery);
			query.FetchOnlyFromLocalCache = true;

			var messages = Factory.Load<MQEDIMessage>(query);
			AssertEquals(1, messages.Length);
			AssertEquals(EM_MessageSubTypeList.Codes.QuotaVisaQuery, messages[0].EM_MessageSubType);
		}

		public void TestSendACEQuotaQuery()
		{
			AssertEquals("Preconditions: no message on the DB", 0, Factory.Load<EDIMessage>(new ZQuery()).Length);

			var option = new QueryQuotaVisaOption(Factory, true);
			option.FillWithValidTestData();
			option.US_QueryType = QueryTypeList.Codes.TextileCategoryNumber;
			option.US_CategoryNumber = "128";
			option.US_UC_NKCountryOfOrigin = "IT";
			option.SendQueryWithoutSaving();

			Factory.Save();
			var messages = Factory.Load<MQEDIMessage>(new ZQuery());
			AssertEquals(1, messages.Length);

			AssertEquals(EDIMessage.ApplicationCodes.USCustomsImport, messages[0].EM_ApplicationCode);
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.QuotaQuery, messages[0].EM_MessageType);
			AssertEquals(EM_MessageSubTypeList.Codes.QuotaVisaQuery, messages[0].EM_MessageSubType);
			AssertContains("Q1X128                 IT                                                       ", messages[0].EM_MessageText);
		}

		public void TestIHaveAdditionalDataForBorderWise()
		{
			var option = new QueryQuotaVisaOption(Factory, false);
			var data = ((IHaveAdditionalDataForBorderWise)option).GetAdditionalDataForBorderWise("");
			AssertEquals("I", data.ParameterForBorderWise);
			AssertEquals(ZDateTime.Today, data.DateForDutyRate);
			AssertEquals(typeof(USCTariff), ((IHaveAdditionalDataForBorderWise)option).ExpectedBusinessObjectTypeForList);
		}

		public void TestValidateUS_TariffNumber()
		{
			var option = new QueryQuotaVisaOption(Factory, false);
			option.US_QueryType = QueryTypeList.Codes.AllVisaRecordsForAllCountries;
			AssertEquals("PreCondition:Tariff number is readonly", true, option.US_FormattedTariffNumberInfo.ReadOnly);

			option.US_QueryType = QueryTypeList.Codes.AllVisaRecordsForACountry;
			AssertEquals("PreCondition:Tariff number is read/write", false, option.US_FormattedTariffNumberInfo.ReadOnly);

			option.US_FormattedTariffNumber = "1";
			AssertHasMessageErrorContaining(option.US_FormattedTariffNumberInfo, QueryQuotaVisaOption.InvalidTariffNumber);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000111122";
			tariff.UE_QuotaIndicator = false;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;

			option.US_FormattedTariffNumber = "0000111122";
			AssertNoMessageErrorContaining(option.US_FormattedTariffNumberInfo, QueryQuotaVisaOption.TariffDoesNotHaveQuotaIndicator);
			AssertHasWarning(option.US_FormattedTariffNumberInfo, QueryQuotaVisaOption.TariffDoesNotHaveQuotaIndicator);

			tariff.UE_QuotaIndicator = true;
			option.US_FormattedTariffNumber = "0000111122";
			AssertNoWarning(option.US_FormattedTariffNumberInfo, QueryQuotaVisaOption.TariffDoesNotHaveQuotaIndicator);
		}

		public void TestValidateUS_CategoryNumber()
		{
			var option = new QueryQuotaVisaOption(Factory, false);
			option.US_QueryType = QueryTypeList.Codes.AllVisaRecordsForAllCountries;
			AssertEquals("PreCondition:Category number is readonly", true, option.US_CategoryNumberInfo.ReadOnly);

			option.US_QueryType = QueryTypeList.Codes.AllVisaRecordsForCountryCategoryOrCountryTariff;
			AssertEquals("PreCondition:Category number is read/write", false, option.US_CategoryNumberInfo.ReadOnly);

			option.US_FormattedTariffNumber = "1";
			option.US_CategoryNumber = "2";
			AssertHasWarning(option.US_CategoryNumberInfo, QueryQuotaVisaOption.CategoryNumberEnteredWontBeUsed);

			option.US_FormattedTariffNumber = "";
			option.US_CategoryNumber = "2";
			AssertNoWarning(option.US_CategoryNumberInfo, QueryQuotaVisaOption.CategoryNumberEnteredWontBeUsed);
		}

		public void TestValidateUS_VisaNumber()
		{
			var option = new QueryQuotaVisaOption(Factory, false);
			option.US_QueryType = QueryTypeList.Codes.AllVisaRecordsForAllCountries;
			AssertEquals("PreCondition:Visa is readonly", true, option.US_VisaNumberInfo.ReadOnly);

			option.US_QueryType = QueryTypeList.Codes.AllVisaRecordsForCountryCategoryOrCountryTariff;
			AssertEquals("PreCondition:Visa is read/write", false, option.US_VisaNumberInfo.ReadOnly);

			option.US_FormattedTariffNumber = "1";
			option.US_VisaNumber = "2";
			AssertHasWarning(option.US_VisaNumberInfo, QueryQuotaVisaOption.VisaNumberEnteredWontBeUsed);

			option.US_FormattedTariffNumber = "";
			option.US_VisaNumber = "2";
			AssertNoWarning(option.US_VisaNumberInfo, QueryQuotaVisaOption.VisaNumberEnteredWontBeUsed);
		}

		public void TestValidateUS_SecondTariffNumber()
		{
			var option = new QueryQuotaVisaOption(Factory, false);
			option.US_QueryType = QueryTypeList.Codes.AllVisaRecordsForCountryCategoryOrCountryTariff;
			AssertEquals("PreCondition:Country is read/write", false, option.US_UC_NKCountryOfOriginInfo.ReadOnly);
			AssertEquals("PreCondition:Tariff is read/write", false, option.US_FormattedTariffNumberInfo.ReadOnly);
			AssertEquals("PreCondition:2nd Tariff is read/write", false, option.US_SecondTariffFormattedNumberInfo.ReadOnly);

			option.US_SecondTariffFormattedNumber = "";
			AssertNoMessageErrors(option.US_SecondTariffFormattedNumberInfo);

			option.US_UC_NKCountryOfOrigin = "AU";
			option.US_SecondTariffFormattedNumber = "1";
			AssertHasMessageError(option.US_SecondTariffFormattedNumberInfo, QueryQuotaVisaOption.SecondTariffNumberEnteredWithoutFirstTariffNumber);

			option.US_FormattedTariffNumber = "1";
			option.US_SecondTariffFormattedNumber = "1";
			AssertNoMessageError(option.US_SecondTariffFormattedNumberInfo, QueryQuotaVisaOption.SecondTariffNumberEnteredWithoutFirstTariffNumber);
			AssertHasMessageErrorContaining(option.US_SecondTariffFormattedNumberInfo, QueryQuotaVisaOption.InvalidTariffNumber);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;

			option.US_SecondTariffFormattedNumber = USCTariff.FDAPriorNoticeRequiredTariff;
			AssertNoMessageErrorContaining(option.US_SecondTariffFormattedNumberInfo, QueryQuotaVisaOption.InvalidTariffNumber);

			option = new QueryQuotaVisaOption(Factory, true);
			option.US_SecondTariffFormattedNumber = "9856321";
			AssertHasMessageErrorContaining(option.US_SecondTariffFormattedNumberInfo, QueryQuotaVisaOption.SecondTariffNumber98_99);
			option.US_SecondTariffFormattedNumber = "112345001";
			AssertNoMessageErrorContaining(option.US_SecondTariffFormattedNumberInfo, QueryQuotaVisaOption.SecondTariffNumber98_99);
		}

		public void TestValidateUS_QueryType()
		{
			var option = new QueryQuotaVisaOption(Factory, false);
			option.US_QueryType = "";
			AssertHasMessageErrorContaining(option.US_QueryTypeInfo, MandatoryValidation.YouHaveNotEntered);

			option.US_QueryType = "~";
			AssertNoMessageErrorContaining(option.US_QueryTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(option.US_QueryTypeInfo, ListValidation.InvalidCodeMessageError);

			option.US_QueryType = option.US_QueryTypeList[0].Code;
			AssertNoMessageErrorContaining(option.US_QueryTypeInfo, ListValidation.InvalidCodeMessageError);

			option.US_QueryType = QueryTypeList.Codes.TariffNumber;
			AssertHasMessageErrorContaining(option.US_QueryTypeInfo, QueryQuotaVisaOption.TariffNoRequired);

			option.US_FormattedTariffNumber = "0000111122";
			AssertNoMessageErrorContaining(option.US_QueryTypeInfo, QueryQuotaVisaOption.TariffNoRequired);

			option.US_QueryType = QueryTypeList.Codes.TextileCategoryNumber;
			AssertHasMessageErrorContaining(option.US_QueryTypeInfo, QueryQuotaVisaOption.CategoryNoRequired);

			option.US_CategoryNumber = "123";
			AssertNoMessageErrorContaining(option.US_QueryTypeInfo, QueryQuotaVisaOption.CategoryNoRequired);
		}

		public void TestSetterOfUS_QueryType()
		{
			var option = new QueryQuotaVisaOption(Factory, false);

			AssertEquals("Readonly", false, option.US_UC_NKCountryOfOriginInfo.ReadOnly);
			AssertEquals("Readonly", false, option.US_SecondTariffFormattedNumberInfo.ReadOnly);

			option.US_CategoryNumber = "369";
			option.US_UC_NKCountryOfOrigin = "KR";
			option.US_SecondTariffFormattedNumber = "0000";

			option.US_QueryType = QueryTypeList.Codes.AllVisaRecordsForAllCountries;
			AssertEquals("For bulk query, C/O should be cleared", "", option.US_UC_NKCountryOfOrigin);
			AssertEquals("For bulk query, Second tariff should be cleared", "", option.US_SecondTariffFormattedNumber);

			AssertEquals("Readonly", true, option.US_UC_NKCountryOfOriginInfo.ReadOnly);
			AssertEquals("Readonly", true, option.US_SecondTariffFormattedNumberInfo.ReadOnly);
		}

		public void TestValidateUS_UC_NKCountryOfOrigin()
		{
			var option = new QueryQuotaVisaOption(Factory, false);
			option.US_UC_NKCountryOfOrigin = "";
			AssertHasMessageErrorContaining(option.US_UC_NKCountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);

			option.US_UC_NKCountryOfOrigin = "~~";
			AssertNoMessageErrorContaining(option.US_UC_NKCountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(option.US_UC_NKCountryOfOriginInfo, ListValidation.InvalidCodeMessageError);

			option.US_QueryType = QueryTypeList.Codes.AllVisaRecordsForAllCountries;
			AssertEquals("", option.US_UC_NKCountryOfOrigin);
			AssertNoMessageErrorContaining(option.US_UC_NKCountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestRunPreSaveValidation()
		{
			var option = new QueryQuotaVisaOption(Factory, false);
			option.RunPreSaveValidation();

			AssertHasMessageErrorContaining(option.US_QueryTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(option.US_UC_NKCountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
		}

		protected override BusinessObject GetNewBusinessObject() => new QueryQuotaVisaOption(Factory, false);
	}
}
