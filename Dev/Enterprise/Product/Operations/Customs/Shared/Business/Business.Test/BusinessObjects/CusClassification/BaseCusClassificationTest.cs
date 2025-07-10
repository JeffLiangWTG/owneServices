using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseCusClassification))]
	public class BaseCusClassificationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestNewCusClassificationCreatedByPRCompanyDefaultToUS()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.PuertoRico);
			var cusClassification = Factory.New<BaseCusClassification>();
			AssertEquals("default to US", Core.Constants.CountryCodes.UnitedStates, cusClassification.CC_RN_NKCountryCode);
		}

		public virtual void TestCC_FormattedTariffNum()
		{
			BaseCusClassification classification = Factory.New<BaseCusClassification>();
			ZString tariff = "1234567890";
			classification.CC_FormattedTariffNum = tariff;
			AssertEquals("CC_FormattedTariffNum", "1234.56.78 90", classification.CC_FormattedTariffNum);
			tariff = "9876.54.32 10";
			classification.CC_FormattedTariffNum = "9876.54.32 10";
			AssertEquals("CC_FormattedTariffNum", tariff, classification.CC_FormattedTariffNum);
		}

		public void TestDescriptionIncludingTariff()
		{
			BaseCusClassification classification = Factory.New<BaseCusClassification>();
			classification.CC_TariffNum = "000000";
			classification.CC_Description = "TEST";

			AssertEquals("(0000.00) TEST", classification.DescriptionIncludingTariff);
		}

		public void TestStaticNewConstructor()
		{
			BaseCusClassification classification = Factory.New<BaseCusClassification>();
			AssertNotNull("Classification Lookup from Static New Constructor", classification);
		}

		public void TestLoadFromLookup()
		{
			Classification.CC_LookupCode = "TestLU";
			Classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			Factory.Save();

			BaseCusClassification classificationLoaded = BaseCusClassification.LoadFromLookupCode(Factory, "TestLU", true, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			AssertEquals("TestLU", classificationLoaded.CC_LookupCode);

			classificationLoaded = BaseCusClassification.LoadFromLookupCode(Factory, "TestLU", false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			AssertNull(classificationLoaded);

			Classification.CC_ClassificationType = BaseCusClassification.ClassificationType.EXP;
			Factory.Save();

			classificationLoaded = BaseCusClassification.LoadFromLookupCode(Factory, "TestLU", false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			AssertEquals("TestLU", classificationLoaded.CC_LookupCode);

			Classification.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
			Factory.Save();

			classificationLoaded = BaseCusClassification.LoadFromLookupCode(Factory, "TestLU", BaseCusClassification.ClassificationType.Both, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			AssertEquals("TestLU", classificationLoaded.CC_LookupCode);

			classificationLoaded = BaseCusClassification.LoadFromLookupCode(Factory, "TestLU", BaseCusClassification.ClassificationType.Both, new ZString());
			AssertNull(classificationLoaded);
		}

		public void TestEmptyLookupCodeIsError()
		{
			Classification.CC_LookupCode = ZString.Empty;
			Assert("Empty lookup code is an error", Classification.CC_LookupCodeInfo.HasErrors());

			Classification.CC_LookupCode = "TestCode";
			Assert("Not error", !Classification.CC_LookupCodeInfo.HasErrors());
		}

		public void TestDuplicateLookupCodeAndClassTypeIsError()
		{
			Classification.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
			Classification.CC_LookupCode = "TestCode";
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			BaseCusClassification newClass = newFactory.New<BaseCusClassification>();
			newClass.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
			newClass.CC_LookupCode = "TestCode";
			Assert("Duplicate lookup code is an error", newClass.CC_LookupCodeInfo.HasErrors());

			newClass.CC_LookupCode = "TestCode2";
			Assert("no more error", !newClass.CC_LookupCodeInfo.HasErrors());
		}

		public void TestEmptyTariffNum()
		{
			Classification.CC_TariffNum = ZString.Empty;
			Assert("Empty Tariff num is an error", Classification.CC_TariffNumInfo.HasErrors());
		}

		public void TestEmptyDescription()
		{
			Classification.CC_Description = ZString.Empty;
			Assert("Empty description is an error", Classification.CC_DescriptionInfo.HasErrors());

			Classification.CC_Description = "test";
			Assert("No more error", !Classification.CC_DescriptionInfo.HasErrors());
		}

		public void TestCC_IsAudited()
		{
			AssertEquals("CC_IsAudited=false by default, when CC_LastAuditedDate not populated", false, Classification.CC_IsAudited);
			Classification.CC_LastAuditedDate = ZDateTime.Now;
			AssertEquals("CC_IsAudited=true when CC_LastAuditedDate populated", true, Classification.CC_IsAudited);
		}

		public void TestChangeOfValuesClearsAuditEntry()
		{
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_LookupCode = "TestLookup";
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.EXP;
			classification.CC_LastAuditedUser = "USR";
			classification.CC_LastAuditedDate = ZDateTime.BrettsBirthday;
			Factory.Save();

			AssertEquals("USR", classification.CC_LastAuditedUser);
			AssertEquals(ZDateTime.BrettsBirthday, classification.CC_LastAuditedDate);

			var timeOfChange = ZDateTime.UtcNow;
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			Factory.Save();

			AssertEquals("", classification.CC_LastAuditedUser);
			AssertEquals(ZDateTime.Empty, classification.CC_LastAuditedDate);

			var logEntry = classification.Logs.MostRecentLogByEventTime(Events.EditedARecord);
			AssertNotNull(logEntry);
			AssertLessThanOrEqualTo(timeOfChange, logEntry.SL_PostedTimeUtc);
		}

		public void TestIsImportIsExportIsBoth()
		{
			BaseCusClassification classification = Factory.New<BaseCusClassification>();
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.EXP;
			AssertEquals("IsExport", true, classification.IsExport);
			AssertEquals("IsImport", false, classification.IsImport);
			AssertEquals("IsBoth", false, classification.IsBoth);

			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			AssertEquals("IsExport", false, classification.IsExport);
			AssertEquals("IsImport", true, classification.IsImport);
			AssertEquals("IsBoth", false, classification.IsBoth);

			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
			AssertEquals("IsExport", false, classification.IsExport);
			AssertEquals("IsImport", false, classification.IsImport);
			AssertEquals("IsBoth", true, classification.IsBoth);
		}

		public void TestDefaultClassificationType()
		{
			var classification = Factory.New<BaseCusClassification>();
			AssertEquals("CC_ClassificationType default value", BaseCusClassification.ClassificationType.Both, classification.CC_ClassificationType);
		}

		public virtual void TestPreventDelete()
		{
			BaseCusClassification classification = Factory.New<BaseCusClassification>();
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(typeof(BaseCusClassification)));
		}

		public virtual void TestITariffFormatProvider()
		{
			var classification = Factory.New<BaseCusClassification>();
			AssertType("TariffFormatter", GetExpectedTariffFormatterType, ((ITariffFormatProvider)classification).TariffFormatter);
		}

		protected virtual Type GetExpectedTariffFormatterType => typeof(TariffFormatter);

		#region Implementation
		protected BaseCusClassification Classification
		{
			get
			{
				if (fTestClassification == null)
				{
					fTestClassification = GetNewCusClassification();
					using (fTestClassification.SuspendSettingHasChanges())
					{
						fTestClassification.CC_LookupCode = "TestLookup";
						fTestClassification.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
					}
				}
				return fTestClassification;
			}
		}
		BaseCusClassification fTestClassification;

		protected virtual BaseCusClassification GetNewCusClassification()
		{
			return Factory.New<BaseCusClassification>();
		}
		#endregion
	}
}
