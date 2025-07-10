using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.InterfaceImplementations.Testing
{
	sealed class CusEntryHeaderCustomsChargesTest : TestCaseWithFactory
	{
		public void TestDefaultCreditorPK()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var prpRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.ProvisionalPayment, description: "Provisional Payments");
			var penRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Penalty, description: "Penalties");
			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			var levyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Levy, description: "Levy");
			var vatRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, "VAT", description: "VAT Normal");
			helper.LoadOrCreateNewCusRateCode(Factory, Constants.RateTypes.Duty, dutyRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, Constants.RateTypes.Levy, levyRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, Constants.RateTypes.Penalty, penRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, Constants.RateTypes.ProvisionalPayment, prpRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "VAT", vatRateType.PK);
			prpRateType.ZZR_IsPayable = true;
			prpRateType.ZZR_Description = "Provisional Payments";
			penRateType.ZZR_IsPayable = true;
			penRateType.ZZR_Description = "Penalties";
			dutyRateType.ZZR_IsPayable = true;
			dutyRateType.ZZR_Description = "Duty";
			levyRateType.ZZR_IsPayable = true;
			levyRateType.ZZR_Description = "Levy";
			vatRateType.ZZR_IsPayable = true;
			vatRateType.ZZR_Description = "VAT Normal";
			var helper2 = new ZAUniversalReferenceTestDataHelper(Factory);
			helper2.CreateCustomsOfficeCusCodeEntry("JHB");
			Factory.Save();
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeCodeDisbursementDefault.PK.ToGuid());
			RatingDataRegistry.Instance.CustomDeferredChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeCodeDeferred.PK.ToGuid());
			var entryChargeTypesAndCodes = RatingDataRegistry.Instance.EntryChargeTypesAndCodes.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var entryChargeType1 = entryChargeTypesAndCodes.AddNew();
			entryChargeType1.ChargeType = Constants.RateTypes.Duty;
			entryChargeType1.AC_ChargeCode = ChargeCodeDisbursementDuty.PK;
			var entryChargeType2 = entryChargeTypesAndCodes.AddNew();
			entryChargeType2.ChargeType = Constants.RateTypes.Levy;
			entryChargeType2.AC_ChargeCode = ChargeCodeDisbursementLevy.PK;
			var entryChargeType3 = entryChargeTypesAndCodes.AddNew();
			entryChargeType3.ChargeType = Constants.RateTypes.Penalty;
			entryChargeType3.AC_ChargeCode = ChargeCodeDisbursementPenalty.PK;
			var entryChargeType4 = entryChargeTypesAndCodes.AddNew();
			entryChargeType4.ChargeType = Constants.RateTypes.ProvisionalPayment;
			entryChargeType4.AC_ChargeCode = ChargeCodeDisbursementProvisionalPayment.PK;
			var entryChargeType5 = entryChargeTypesAndCodes.AddNew();
			entryChargeType5.ChargeType = CusEntryHeader.RateTypes.VATNormal;
			entryChargeType5.AC_ChargeCode = ChargeCodeDisbursementDefault.PK;
			using (RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, entryChargeTypesAndCodes))
			{
				helper2.CreateCustomsOfficeCusCodeEntry("BBR");
				helper2.CreateCustomsOfficeCusCodeEntry("BFN");
				helper2.CreateCustomsOfficeCusCodeEntry("JHB");
				var testAgentForFAN = Factory.NewWithValidTestData<OrgHeader>();
				testAgentForFAN.CustomsCodes.AddNew("CDP", "51051342", Core.Constants.CountryCodes.SouthAfrica);
				testAgentForFAN.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "51051342", Core.Constants.CountryCodes.SouthAfrica);
				var testCreditor1 = Factory.NewWithValidTestData<OrgHeader>();
				testCreditor1.OH_IsCreditor = true;
				var testCreditor2 = Factory.NewWithValidTestData<OrgHeader>();
				testCreditor2.OH_IsCreditor = true;
				Factory.Save();
				var agentOfficeToCreditorMappings = new FinancialAccountNumberPortMapCollection();
				var mapping1 = agentOfficeToCreditorMappings.AddNew();
				mapping1.OrganizationPK = testAgentForFAN.PK;
				mapping1.CustomsOfficeCode = "JHB";
				mapping1.FinancialAccountNumber = "1234567890";
				mapping1.CreditorPK = testCreditor1.PK;
				mapping1.ImporterPays = false;
				mapping1.AccountStartDay = 1;
				var mapping2 = agentOfficeToCreditorMappings.AddNew();
				mapping2.OrganizationPK = testAgentForFAN.PK;
				mapping2.Cash = true;
				mapping2.CustomsOfficeCode = "";
				mapping2.FinancialAccountNumber = "1234567899";
				mapping2.CreditorPK = testCreditor2.PK;
				mapping2.ImporterPays = false;
				mapping2.AccountStartDay = 1;
				using (ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, agentOfficeToCreditorMappings))
				{
					var buyer = OrgHeader.New(Factory);
					buyer.FillWithValidTestData();
					var collection = new CustomsDSBCreditorOverrideCollection();
					var creditor = collection.AddNew();
					var orgHeaderQuery = new ZDBOnlyQuery(typeof(OrgHeader));
					var orgCompanyDataQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
					orgCompanyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, true);
					orgHeaderQuery.AddSubQuery(orgCompanyDataQuery, JoinCondition.And);
					var organisation = Factory.LoadTop1<OrgHeader>(orgHeaderQuery);
					creditor.DistrictOfficeCode = "BFN";
					creditor.CreditorPK = organisation.PK;
					var organisation2 = Factory.NewWithValidTestData<OrgHeader>();
					Factory.Save();
					using (ZACustomsRegistry.Instance.DSBCreditors.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
					using (RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, organisation2.PK.ToGuid()))
					using (RatingDataRegistry.Instance.IncludeEntryHeaderReferenceInCustomsDisbursementCharges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
					{
						var declaration = Factory.New<JobDeclaration>();
						declaration.JE_CustomsOffice = "JHB";
						declaration.JE_OH_AgentOverride = testAgentForFAN.PK;
						declaration.ActiveEntryHeaders.AddNew();
						var entry = declaration.CustomsEntryHeaders[0];
						var entryLine1 = entry.MergedLines.AddNew();
						entryLine1.Fees.AddOrUpdate(Constants.RateTypes.Duty, 10m);
						entryLine1.Fees.AddOrUpdate(Constants.RateTypes.Levy, 20m);
						entryLine1.Fees.AddOrUpdate(Constants.RateTypes.Penalty, 30m);
						entryLine1.Fees.AddOrUpdate(Constants.RateTypes.ProvisionalPayment, 40m);
						entryLine1.Fees.AddOrUpdate("VAT", 50m);
						var entryLine2 = entry.MergedLines.AddNew();
						entryLine2.Fees.AddOrUpdate(Constants.RateTypes.Duty, 10m);
						entryLine2.Fees.AddOrUpdate(Constants.RateTypes.Levy, 20m);
						entryLine2.Fees.AddOrUpdate(Constants.RateTypes.Penalty, 30m);
						entryLine2.Fees.AddOrUpdate(Constants.RateTypes.ProvisionalPayment, 40m);
						entryLine2.Fees.AddOrUpdate("VAT", 50m);
						AssertEquals(testCreditor1.PK, entry.CreditorPK);
						ICustomsCharges chargeProvider = new CusEntryHeaderCustomsCharges(entry);
						var customsCharges = chargeProvider.GetCustomsCharges(null);
						AssertEquals(3, customsCharges.Length);
						var duty = customsCharges.FirstOrDefault(x => x.Description == "Duty");
						AssertNotNull(duty);
						AssertEquals(testCreditor1.PK, duty.CreditorPK);
						var levy = customsCharges.FirstOrDefault(x => x.Description == "Levy");
						AssertNotNull(levy);
						AssertEquals(testCreditor1.PK, levy.CreditorPK);
						var vat = customsCharges.FirstOrDefault(x => x.Description == "VAT Normal");
						AssertNotNull(vat);
						AssertEquals(testCreditor1.PK, vat.CreditorPK);
						entry.CH_PaymentMethod = PaymentMethodCodeList.Codes.VATOnly;
						chargeProvider = new CusEntryHeaderCustomsCharges(entry);
						customsCharges = chargeProvider.GetCustomsCharges(null);
						AssertEquals(3, customsCharges.Length);
						duty = customsCharges.FirstOrDefault(x => x.Description == "Duty");
						AssertNotNull(duty);
						AssertEquals(testCreditor2.PK, duty.CreditorPK);
						levy = customsCharges.FirstOrDefault(x => x.Description == "Levy");
						AssertNotNull(levy);
						AssertEquals(testCreditor2.PK, levy.CreditorPK);
						vat = customsCharges.FirstOrDefault(x => x.Description == "VAT Normal");
						AssertNotNull(vat);
						AssertEquals(testCreditor1.PK, vat.CreditorPK);
					}
				}
			}
		}

		AccChargeCode CreateChargeCode(string code, string description, string chargeType)
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "BG~" + code + "~";
			chargeCode.AC_Desc = description;
			chargeCode.AC_ChargeType = chargeType;
			chargeCode.AC_MarginPercentage = 0m;
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			return chargeCode;
		}

		AccChargeCode chargeCodeDisbursementDefault;
		AccChargeCode ChargeCodeDisbursementDefault
		{
			get
			{
				if (chargeCodeDisbursementDefault == null)
				{
					chargeCodeDisbursementDefault = CreateChargeCode("DSB", "Customs Disbursements Default", Core.Constants.ChargeType.Disbursement);
				}

				return chargeCodeDisbursementDefault;
			}
		}

		AccChargeCode chargeCodeDisbursementDuty;
		AccChargeCode ChargeCodeDisbursementDuty
		{
			get
			{
				if (chargeCodeDisbursementDuty == null)
				{
					chargeCodeDisbursementDuty = CreateChargeCode("DUT", "Customs Disbursements Duty", Core.Constants.ChargeType.Disbursement);
				}

				return chargeCodeDisbursementDuty;
			}
		}

		AccChargeCode chargeCodeDisbursementLevy;
		AccChargeCode ChargeCodeDisbursementLevy
		{
			get
			{
				if (chargeCodeDisbursementLevy == null)
				{
					chargeCodeDisbursementLevy = CreateChargeCode("LVY", "Customs Disbursements Levy", Core.Constants.ChargeType.Disbursement);
				}

				return chargeCodeDisbursementLevy;
			}
		}

		AccChargeCode chargeCodeDisbursementPenalty;
		AccChargeCode ChargeCodeDisbursementPenalty
		{
			get
			{
				if (chargeCodeDisbursementPenalty == null)
				{
					chargeCodeDisbursementPenalty = CreateChargeCode("PEN", "Customs Disbursements Penalty", Core.Constants.ChargeType.Disbursement);
				}

				return chargeCodeDisbursementPenalty;
			}
		}

		AccChargeCode chargeCodeDisbursementProvisionalPayment;
		AccChargeCode ChargeCodeDisbursementProvisionalPayment
		{
			get
			{
				if (chargeCodeDisbursementProvisionalPayment == null)
				{
					chargeCodeDisbursementProvisionalPayment = CreateChargeCode("PRP", "Customs Disbursements ProvisionalPayment", Core.Constants.ChargeType.Disbursement);
				}

				return chargeCodeDisbursementProvisionalPayment;
			}
		}

		AccChargeCode chargeCodeDeferred;
		AccChargeCode ChargeCodeDeferred
		{
			get
			{
				if (chargeCodeDeferred == null)
				{
					chargeCodeDeferred = CreateChargeCode("DEF", "Customs Deferred Charge (For information only)", Core.Constants.ChargeType.Comment);
				}

				return chargeCodeDeferred;
			}
		}
	}
}
