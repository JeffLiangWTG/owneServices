using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Business.TariffValidation;

namespace Enterprise.Customs.NZ.Business.MasterFiles.Testing
{
	using Enterprise.Customs.NZ.Business.Testing;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Business.Testing;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	[TestedType(typeof(CusClassification))]
	public class CusClassificationBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
	}

	[TestedType(typeof(CusClassification))]
	public class CusClassificationTest : Customs.Business.Testing.BaseCusClassificationTest
	{
		public void TestCC_TariffNum()
		{
			var tariffQuery = new ZQuery(CusClassificationSchema.CC_TariffNum, "1234.56.78.9");
			AssertNull("Precondition - no existing line with this tariff", Factory.LoadTop1<CusClassification>(tariffQuery));

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Classification.CC_TariffNum = "1234.56.78.9";
				AssertEquals("1234.56.78.9", Classification.CC_TariffNum);
				Factory.Save();
			}

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var newFactory = new BusinessObjectFactory();
				var classificationInNewFactory = newFactory.LoadTop1<CusClassification>(tariffQuery);
				AssertNotNull("line was stored with a dotted tariff", classificationInNewFactory);
				AssertEquals("Dots are removed in getter", "123456789", classificationInNewFactory.CC_TariffNum);

				classificationInNewFactory.CC_TariffNum = "987654321";
				classificationInNewFactory.CC_TariffNum = "123456789";  // force write back of new value
				newFactory.Save();

				var newFactory2 = new BusinessObjectFactory();
				classificationInNewFactory = newFactory2.LoadTop1<CusClassification>(tariffQuery);
				AssertNotNull("line was stored with a dotted tariff", classificationInNewFactory);
				AssertEquals("Dots are removed in getter", "123456789", classificationInNewFactory.CC_TariffNum);
			}
		}

		public void TestChangeAllAddInfoPropertiesToModelViewMigration()
		{
			var classification = Factory.NewWithValidTestData<CusClassification>();
			classification.CC_AddInfo = "ConcessionCode=123456789*" +
				"OtherInfos=otherinfo*" +
				"PartsOfClassification=3333*" +
				"PermitCodes=44444*" +
				"ProhibitedCodes=123456";
			Factory.Save();
			classification = Factory.Load<CusClassification>(classification.PK);
			AssertEquals("123456789", classification.CC_ConcessionCode);
			AssertEquals("otherinfo", classification.CC_OtherInfos);
			AssertEquals("3333", classification.CC_PartsOfClassification);
			AssertEquals("44444", classification.CC_PermitCodes);
			AssertEquals("123456", classification.CC_ProhibitedCodes);

			classification.CC_ConcessionCode = "1";
			classification.CC_OtherInfos = "2";
			classification.CC_PartsOfClassification = "3";
			classification.CC_PermitCodes = "4";
			classification.CC_ProhibitedCodes = "5";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var retrieved = newFactory.Load<CusClassification>(classification.PK);
			AssertEquals("1", classification.CC_ConcessionCode);
			AssertEquals("2", classification.CC_OtherInfos);
			AssertEquals("3", classification.CC_PartsOfClassification);
			AssertEquals("4", classification.CC_PermitCodes);
			AssertEquals("5", classification.CC_ProhibitedCodes);
		}

		public void TestCC_PartsOfClassification()
		{
			AssertEquals("Caption", "Parts Of Classification", DataBoundResourceStrings.GetDataForProperty(typeof(CusClassification), nameof(CusClassification.CC_PartsOfClassification)).Caption);

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Classification.CC_PartsOfClassification = "1234.56.78.9";
				AssertEquals("1234.56.78.9", Classification.CC_PartsOfClassification);
			}

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Classification.CC_PartsOfClassification = "987654321";
				AssertEquals("987654321", Classification.CC_PartsOfClassification);
			}
		}

		public void TestCC_ConcessionCode()
		{
			AssertEquals("Caption", "Concession", DataBoundResourceStrings.GetDataForProperty(typeof(CusClassification), nameof(CusClassification.CC_ConcessionCode)).Caption);
		}

		public void TestConcessionList()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				UniversalTariffHelperTest.SetupTariffData(Factory);
				var classification = Factory.New<CusClassification>();
				classification.CC_TariffNum = "123456789";

				var concessions = classification.ConcessionList;
				AssertEquals("ConcessionList.Count", 2, concessions.Count);
			}
		}

		public void TestTariff()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				UniversalTariffHelperTest.SetupTariffData(Factory);
				var classification = Factory.New<CusClassification>();
				classification.CC_TariffNum = "123456789";

				AssertEquals("123456789", classification.Tariff.Code);
			}
		}

		public override void TestCC_FormattedTariffNum()
		{
			var classification = Factory.New<CusClassification>();
			ZString tariff = "1234567890K";
			classification.CC_FormattedTariffNum = tariff;
			AssertEquals("CC_FormattedTariffNum", "1234.56.78.90K", classification.CC_FormattedTariffNum);
			tariff = "9876.54.32.10K";
			classification.CC_FormattedTariffNum = tariff;
			AssertEquals("CC_FormattedTariffNum", tariff, classification.CC_FormattedTariffNum);
		}

		[TestDate(2008, 6, 6)]
		public void TestDutyRateForCurrentCountry()
		{
			CusClassification classification = Factory.New<CusClassification>();
			classification.CC_TariffNum = "4201.00.00.01B";
			AssertEquals("classification.DutyRateForCurrentCountry", "7.00%", classification.DutyRateForCurrentCountry);
			classification.CC_ConcessionCode = "909090A";
			AssertEquals("classification.DutyRateForCurrentCountry", "FREE", classification.DutyRateForCurrentCountry);
			classification.CC_TariffNum = "8716.90.09.09K";
			classification.CC_ConcessionCode = "";
			AssertEquals("classification.DutyRateForCurrentCountry", "FREE", classification.DutyRateForCurrentCountry);
			classification.CC_PartsOfClassification = "8716.80.09.10K";
			AssertEquals("classification.DutyRateForCurrentCountry", "7.00%", classification.DutyRateForCurrentCountry);
		}

		public void TestCusClassificationValidation()
		{
			AssertEquals(typeof(CusClassificationValidation), Classification.Validation.GetType());
		}

		[TestDate(2006, 5, 18)]
		public void TestITariffValidationData()
		{
			Classification.CC_TariffNum = Tariff1.U0_Tariff;
			Classification.CC_PartsOfClassification = Tariff2.U0_Tariff;
			PermitCode permit1 = Classification.PermitCodes.AddNew();
			PermitCode permit2 = Classification.PermitCodes.AddNew();
			ITariffValidationData tariffValidation = Classification;
			AssertNotNull("Classification as ITariffValidationData", tariffValidation);
			AssertEquals("tariffValidation.AllowableTariffCodeTypes.Import", true, tariffValidation.AllowableTariffCodeTypes.Import);
			AssertEquals("tariffValidation.AllowableTariffCodeTypes.Export", true, tariffValidation.AllowableTariffCodeTypes.Export);
			AssertEquals("tariffValidation.AllowableTariffCodeTypes.Excise", true, tariffValidation.AllowableTariffCodeTypes.Excise);
			AssertEquals("tariffValidation.DateForDutyRate", new ZDateTime(2006, 5, 18), tariffValidation.DateForDutyRate);
			AssertEquals("tariffValidation.EmptyTariffIsFullError", true, tariffValidation.EmptyTariffIsFullError);
			AssertEquals("tariffValidation.EmptyTariffIsAllowed", false, tariffValidation.EmptyTariffIsAllowed);
			AssertEquals("tariffValidation.Factory", Classification.Factory, tariffValidation.Factory);
			AssertEquals("tariffValidation.PartsOfTariffBO", Tariff2, tariffValidation.PartsOfTariffBO);
			AssertEquals("tariffValidation.PartsOfTariffCode", Tariff2.U0_Tariff, tariffValidation.PartsOfTariffCode);
			AssertEquals("tariffValidation.PartsOfTariffCodeInfo", Classification.CC_PartsOfClassificationInfo, tariffValidation.PartsOfTariffCodeInfo);
			AssertEquals("tariffValidation.PermitCodeCount", 2, tariffValidation.PermitCodeCount);
			AssertEquals("tariffValidation.TariffBO", Tariff1, tariffValidation.TariffBO);
			AssertEquals("tariffValidation.TariffCode", Tariff1.U0_Tariff, tariffValidation.TariffCode);
			AssertEquals("tariffValidation.TariffCodeInfo", Classification.CC_TariffNumInfo, tariffValidation.TariffCodeInfo);
		}

		[TestDate(2005, 1, 3)]
		public void TestGetAdditionalDataForBorderWise()
		{
			AdditionalDataForBorderWise additionalData = Classification.GetAdditionalDataForBorderWise("");
			AssertEquals("AdditionalData.ParameterForBorderWise", "I", additionalData.ParameterForBorderWise);
			AssertEquals("AdditionalData.DateForDutyRate", new ZDateTime(2005, 1, 3), additionalData.DateForDutyRate);
		}

		public void TestOnTariffSetUpdatesDescriptionAsAppropriate()
		{
			Classification.CC_TariffNum = Tariff1.U0_Tariff;
			AssertEquals(Tariff1.U0_Description, Classification.CC_Description);
			Classification.CC_TariffNum = Tariff2.U0_Tariff;
			AssertEquals(Tariff2.U0_Description, Classification.CC_Description);
			Classification.CC_TariffNum = "";
			AssertEquals("", Classification.CC_Description);

			Classification.CC_Description = UserDescription;
			Classification.CC_TariffNum = Tariff1.U0_Tariff;
			AssertEquals(UserDescription, Classification.CC_Description);
			Classification.CC_TariffNum = Tariff2.U0_Tariff;
			AssertEquals(UserDescription, Classification.CC_Description);
			Classification.CC_TariffNum = "";
			AssertEquals(UserDescription, Classification.CC_Description);

			Classification.CC_Description = ZString.Empty;
			Classification.CC_TariffNum = Tariff1.U0_Tariff;
			AssertEquals(Tariff1.U0_Description, Classification.CC_Description);
			Classification.CC_TariffNum = "3333.33.33.33K";
			AssertEquals("Setting an invalid code should not clear the current description", Tariff1.U0_Description, Classification.CC_Description);
		}
		const string UserDescription = "USER_DESCRIPTION";

		public void TestRunPreSaveValidation()
		{
			Assert("PreCondition: No errors", !Classification.HasErrors);

			PermitCode code = Classification.PermitCodes.AddNew();
			code.ZO_Code = "XXX";
			Assert("PreCondition: Invalid code", code.ZO_CodeInfo.HasMessageErrors());
			code.ZO_CodeInfo.ClearAllNotifications();
			Classification.RunPreSaveValidation();//should revalidate
			Assert("Should have revalidated", code.ZO_CodeInfo.HasMessageErrors());
		}

		public void TestAggregatedPermitCodes()
		{
			PermitCode code1 = Classification.PermitCodes.AddNew();
			code1.ZO_Code = "ZZZ";
			code1.ZO_Data = "111";

			PermitCode code2 = Classification.PermitCodes.AddNew();
			code2.ZO_Code = "XXX";
			code2.ZO_Data = "222";

			AssertEquals("Aggregated PermitCodes", "ZZZ/XXX", Classification.CC_AggregatedPermitCodes);
		}

		public void TestAggregatedProhibitedCodes()
		{
			ProhibitedCode code1 = Classification.ProhibitedCodes.AddNew();
			code1.ZO_Code = "ZZZ";

			ProhibitedCode code2 = Classification.ProhibitedCodes.AddNew();
			code2.ZO_Code = "XXX";

			AssertEquals("Aggregated ProhibitedCode", "ZZZ/XXX", Classification.CC_AggregatedProhibitedCodes);
		}

		public void TestAggregatedOtherInfoCodes()
		{
			OtherInfo code1 = Classification.OtherInfos.AddNew();
			code1.ZO_Code = "ZZZ";

			OtherInfo code2 = Classification.OtherInfos.AddNew();
			code2.ZO_Code = "XXX";

			AssertEquals("Aggregated OtherInfos", "ZZZ/XXX", Classification.CC_AggregatedOtherInfoCodes);
		}

		public void TestDeleteItemInOtherInfoAffectHasChanges()
		{
			PermitCode permitCode = Classification.PermitCodes.AddNew();
			permitCode.ZO_Code = "CUD";
			permitCode.ZO_Data = "111";

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CusClassification classLoaded = newFactory.Load<CusClassification>(Classification.PK);
			AssertEquals("Classification not changed on loaded", false, classLoaded.HasChanges);

			PermitCode permitCode2 = classLoaded.PermitCodes[0];
			classLoaded.PermitCodes.RemoveAndDelete(permitCode2);
			AssertEquals("Classification changed", true, classLoaded.HasChanges);
		}

		public void TestOtherInfosChangeAffectHasChanges()
		{
			Assert("PreCondition:not changed yet", !Classification.HasChanges);
			OtherInfo otherInfo = Classification.OtherInfos.AddNew();
			otherInfo.ZO_Code = "ZZZ";
			Assert("Has chagned", Classification.HasChanges);
		}

		public void TestAddInfoWithOtherInfosOnFactorySaving()
		{
			AssertEquals("PreCondition:AddInfo", ZString.Empty, Classification.CC_AddInfo);
			Classification.CC_ConcessionCode = "123456";
			PermitCode code = Classification.PermitCodes.AddNew();
			code.ZO_Code = "AF1";
			code.ZO_Data = "123456";
			Factory.Save();
			Assert("AddInfo has Concession", Classification.CC_AddInfo.IndexOf("ConcessionCode=123456") >= 0);
			Assert("AddInfo has Permit", Classification.CC_AddInfo.IndexOf("PermitCodes=AF1=123456") >= 0);
		}

		public void TestAddInfoOnLoaded()
		{
			Classification.CC_ConcessionCode = "12345OP";
			Factory.Save();

			var loadedClass = Factory.Load<CusClassification>(Classification.PK);
			AssertEquals("Concession", Classification.CC_ConcessionCode, loadedClass.CC_ConcessionCode);
		}

		public void TestAddInfoOnFactorySaving()
		{
			Classification.CC_ConcessionCode = "12345OP";
			Factory.Save();

			var loadedClass = Factory.Load<CusClassification>(Classification.PK);
			Assert("AddInfo string has Concession EXP", loadedClass.CC_AddInfo.IndexOf("ConcessionCode=12345OP") >= 0);
		}

		public void TestAddInfoPropertyChangeHasChanges()
		{
			Assert("PreCondition:HasChanges", !Classification.HasChanges);
			Classification.CC_ConcessionCode = "123456";
			Assert("HasChanges", Classification.HasChanges);
		}

		public void TestDefaultValues()
		{
			AssertEquals("Country/Region is set", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Classification.CC_RN_NKCountryCode);
			AssertEquals("Type is set", CusClassification.ClassificationType.Both, Classification.CC_ClassificationType);
		}

		public override void TestITariffFormatProvider()
		{
			var classification = Factory.New<CusClassification>();
			AssertType<NZTariffFormatter>("TariffFormatter", ((ITariffFormatProvider)classification).TariffFormatter);
		}

		#region Implementation
		protected new CusClassification Classification
		{
			get { return (CusClassification)base.Classification; }
		}

		protected override Customs.Business.BaseCusClassification GetNewCusClassification()
		{
			return Factory.New<CusClassification>();
		}

		protected NZCClassification Tariff1
		{
			get
			{
				if (fTariff1 == null)
				{
					fTariff1 = NZCClassification.New(Factory);
					fTariff1.U0_Tariff = "1111.11.11.11K";
					fTariff1.U0_Description = "TARIFF_DESCRIPTION_1";
					fTariff1.U0_DateActiveFrom = new ZDateTime(1980, 1, 1, 0, 0, 0);
					fTariff1.U0_DateActiveTo = new ZDateTime(3000, 12, 31, 0, 0, 0);
				}
				return fTariff1;
			}
		}
		NZCClassification fTariff1;

		protected NZCClassification Tariff2
		{
			get
			{
				if (fTariff2 == null)
				{
					fTariff2 = NZCClassification.New(Factory);
					fTariff2.U0_Tariff = "2222.22.22.22K";
					fTariff2.U0_Description = "TARIFF_DESCRIPTION_2";
					fTariff2.U0_DateActiveFrom = new ZDateTime(1980, 1, 1, 0, 0, 0);
					fTariff2.U0_DateActiveTo = new ZDateTime(3000, 12, 31, 0, 0, 0);
				}
				return fTariff2;
			}
		}
		NZCClassification fTariff2;

		#endregion
	}
}
