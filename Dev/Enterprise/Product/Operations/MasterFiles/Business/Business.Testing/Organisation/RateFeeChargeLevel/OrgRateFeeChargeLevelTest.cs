using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgRateFeeChargeLevel))]
	sealed class OrgRateFeeChargeLevelTest : EnterpriseBusinessObjectTestCase
	{
		public void TestServiceDescription()
		{
			ChargeLevel.ORF_ServiceType = ServiceTypeFromRegistry.Code;
			AssertEquals(ServiceTypeFromRegistry.Description, ChargeLevel.ServiceDescription);

			ChargeLevel.ORF_ServiceType = ZString.Empty;
			AssertEquals(ZString.Empty, ChargeLevel.ServiceDescription);
		}

		public void TestReadOnlySecurity()
		{
			var level = OrgInDB.CompanyData.RateFeeChargeLevels.AddNew();

			Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed = true;
			Env.Security.OrgDetailsModify.IsAllowed = true;
			AssertProperties(level, false);

			Env.Security.OrgDetailsModify.IsAllowed = false;
			Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed = true;
			AssertProperties(level, true);

			Env.Security.OrgDetailsModify.IsAllowed = true;
			Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed = false;
			AssertProperties(level, true);
		}

		OrgHeader OrgInDB
		{
			get
			{
				if (fOrgInDB == null)
				{
					ResetOrgInDB();
				}

				return fOrgInDB;
			}
		}
		OrgHeader fOrgInDB;

		void ResetOrgInDB()
		{
			fOrgInDB = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
		}

		void AssertProperties(OrgRateFeeChargeLevel level, bool isExpectedReadOnly)
		{
			AssertEquals(isExpectedReadOnly, level.ORF_ServiceTypeInfo.ReadOnly);
			AssertEquals(isExpectedReadOnly, level.ORF_LevelInfo.ReadOnly);
			AssertEquals(isExpectedReadOnly, level.ORF_Amount1TypeInfo.ReadOnly);
			AssertEquals(isExpectedReadOnly, level.ORF_Amount1Info.ReadOnly);
			AssertEquals(isExpectedReadOnly, level.ORF_RX_NKAmount1CurrencyInfo.ReadOnly);
			AssertEquals(isExpectedReadOnly, level.ORF_Amount2TypeInfo.ReadOnly);
			AssertEquals(isExpectedReadOnly, level.ORF_Amount2Info.ReadOnly);
			AssertEquals(isExpectedReadOnly, level.ORF_RX_NKAmount2CurrencyInfo.ReadOnly);
		}

		public void TestORF_Level()
		{
			//default value for registry

			var registry = OrganisationsDataRegistry.Instance.RateFeeChargeLevels.Value;
			var registryChargeType = registry.FeeChargeTypes[0];

			var levelNum = registryChargeType.FeeChargeLevels.Count;
			var level = registryChargeType.FeeChargeLevels.AddNew();

			level.Code = "SIL";
			level.Description = (NoResString)"Test Description";
			level.Amount1Type = OrgConstants.ServiceLevelAmountTypes.Code.Excess;
			level.Amount1 = 300;
			level.Amount1Currency = "AUD";
			level.Amount2Type = OrgConstants.ServiceLevelAmountTypes.Code.Maximum;
			level.Amount2Currency = "CNY";
			level.Amount2 = 600;

			var level1 = registryChargeType.FeeChargeLevels.AddNew();
			level1.Code = "TES";
			level1.Description = (NoResString)"Test Description 1";
			level1.Amount1Type = OrgConstants.ServiceLevelAmountTypes.Code.Minimum;
			level1.Amount1 = 600;
			level1.Amount1Currency = "CNY";
			level1.Amount2Type = OrgConstants.ServiceLevelAmountTypes.Code.None;

			AssertEquals(levelNum + 2, registryChargeType.FeeChargeLevels.Count);

			OrganisationsDataRegistry.Instance.RateFeeChargeLevels.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registry);

			var chargeTest = Factory.New<OrgRateFeeChargeLevel>();
			chargeTest.ORF_ServiceType = registryChargeType.Code;
			chargeTest.ORF_Level = "SIL";

			AssertEquals(level.Amount1Type, chargeTest.ORF_Amount1Type);
			AssertEquals(level.Amount1, chargeTest.ORF_Amount1);
			AssertEquals(level.Amount1Currency, chargeTest.ORF_RX_NKAmount1Currency);

			AssertEquals(level.Amount2Type, chargeTest.ORF_Amount2Type);
			AssertEquals(level.Amount2, chargeTest.ORF_Amount2);
			AssertEquals(level.Amount2Currency, chargeTest.ORF_RX_NKAmount2Currency);

			//no redefault

			chargeTest.ORF_Level = "TES";

			AssertEquals(level.Amount1Type, chargeTest.ORF_Amount1Type);
			AssertEquals(level.Amount1, chargeTest.ORF_Amount1);
			AssertEquals(level.Amount1Currency, chargeTest.ORF_RX_NKAmount1Currency);

			AssertEquals(level.Amount2Type, chargeTest.ORF_Amount2Type);
			AssertEquals(level.Amount2, chargeTest.ORF_Amount2);
			AssertEquals(level.Amount2Currency, chargeTest.ORF_RX_NKAmount2Currency);
		}

		public void TestHtmlProperty()
		{
			AssertEquals(ZBlob.Empty, ChargeLevel.ORF_NoteData);
			AssertEquals(ZBlob.Empty, ChargeLevel.ORF_NoteData_HTML);

			ChargeLevel.ORF_NoteData_HTML = ZBlob.FromUTF8("<p>123</p>");

			AssertEquals(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{{123}\par}}", ORtfTextUtil.GeneratorInfoRegex.Replace(ChargeLevel.ORF_NoteData.ToUTF8(), string.Empty));
			AssertEquals("<p>123</p>", ChargeLevel.ORF_NoteData_HTML.ToUTF8());
		}

		public void TestHtmlFromTextProperty()
		{
			AssertEquals(ZBlob.Empty, ChargeLevel.ORF_NoteData);
			AssertEquals(ZBlob.Empty, ChargeLevel.ORF_NoteData_HTML);

			ChargeLevel.ORF_NoteData = ZBlob.FromUTF8("1234\r\n5678");
			AssertEquals("<p>1234</p><p>5678</p>", ChargeLevel.ORF_NoteData_HTML.ToUTF8());

			ChargeLevel.ORF_NoteData = ZBlob.FromUTF8("{\\rtf1\\test\\ansi\\ansicpg1252\\nouicompat\\deflang3081\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \\pard rtf\\par\r\n}\r\n");
			AssertEquals("<p>rtf</p>", ChargeLevel.ORF_NoteData_HTML.ToUTF8());
		}

		OrgRateFeeChargeLevel ChargeLevel;

		protected override void SetUp()
		{
			base.SetUp();
			ChargeLevel = Factory.NewWithValidTestData<OrgRateFeeChargeLevel>();
		}

		static FeeChargeType ServiceTypeFromRegistry
		{
			get
			{
				var registrySetting = OrganisationsDataRegistry.Instance.RateFeeChargeLevels.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				if (registrySetting != null)
				{
					return registrySetting.FeeChargeTypes.FirstOrDefault() as FeeChargeType;
				}

				return null;
			}
		}
	}
}
