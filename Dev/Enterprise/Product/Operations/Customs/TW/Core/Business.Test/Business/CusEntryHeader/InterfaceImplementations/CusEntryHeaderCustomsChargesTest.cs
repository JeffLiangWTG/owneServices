using System;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.InterfaceImplementations.Testing
{
	public sealed class CusEntryHeaderCustomsChargesTest : Customs.Business.Testing.CusEntryHeaderCustomsChargesTest
	{
		public override void AddCountrySpecificCharges(Customs.Business.CusEntryHeader entryHeader)
		{
			var entryPayInfo1 = entryHeader.EntryPayInfos.AddNew();
			entryPayInfo1.C9_TransactionType = EntryChargeTypeList.Codes.A10;
			entryPayInfo1.C9_PaymentAmount = 1000m;

			var entryPayInfo2 = entryHeader.EntryPayInfos.AddNew();
			entryPayInfo2.C9_TransactionType = EntryChargeTypeList.Codes.A20;
			entryPayInfo2.C9_PaymentAmount = 2000m;

			var entryPayInfo3 = entryHeader.EntryPayInfos.AddNew();
			entryPayInfo3.C9_TransactionType = EntryChargeTypeList.Codes.A10;
			entryPayInfo3.C9_PaymentAmount = 3000m;

			var entryPayInfo4 = entryHeader.EntryPayInfos.AddNew();
			entryPayInfo4.C9_TransactionType = EntryChargeTypeList.Codes.C21;
			entryPayInfo4.C9_PaymentAmount = 5000m;
		}

		[ExpectNoExceptions]
		public override void AssertCustomsCharges(CustomsCharge[] charges)
		{
			NUnit.Framework.Assert.That(charges.Length, NUnit.Framework.Is.EqualTo(3), "There should be 3 customs charges");

			var charge1 = charges[0];
			AssertCustomsCharge(charge1, 2000m, DutyTaxFeeCodeList.Descriptions.A20);
			NUnit.Framework.Assert.That(charge1.IsPaidByBroker, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(charge1.CreditorPK, NUnit.Framework.Is.EqualTo(creditor.PK));
			NUnit.Framework.Assert.That(charge1.ChargeCodePK, NUnit.Framework.Is.EqualTo(ZGuid.Empty));
			NUnit.Framework.Assert.That(charge1.OverrideCurrency, NUnit.Framework.Is.EqualTo("TWD").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(charge1.EntryReference, NUnit.Framework.Is.EqualTo("Test1234").Using(CustomComparers.TypeComparison));

			var charge2 = charges[1];
			AssertCustomsCharge(charge2, 4000m, DutyTaxFeeCodeList.Descriptions.A10);
			NUnit.Framework.Assert.That(charge2.IsPaidByBroker, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(charge2.CreditorPK, NUnit.Framework.Is.EqualTo(creditor.PK));
			NUnit.Framework.Assert.That(charge2.ChargeCodePK, NUnit.Framework.Is.EqualTo(chgCode.PK));
			NUnit.Framework.Assert.That(charge2.OverrideCurrency, NUnit.Framework.Is.EqualTo("TWD").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(charge2.EntryReference, NUnit.Framework.Is.EqualTo("Test1234").Using(CustomComparers.TypeComparison));

			var charge3 = charges[2];
			AssertCustomsCharge(charge3, 5000m, DepositTypeCodeList.Descriptions.C21);
			NUnit.Framework.Assert.That(charge3.IsPaidByBroker, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(charge3.CreditorPK, NUnit.Framework.Is.EqualTo(creditor.PK));
			NUnit.Framework.Assert.That(charge3.ChargeCodePK, NUnit.Framework.Is.EqualTo(ZGuid.Empty));
			NUnit.Framework.Assert.That(charge3.OverrideCurrency, NUnit.Framework.Is.EqualTo("TWD").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(charge3.EntryReference, NUnit.Framework.Is.EqualTo("Test1234").Using(CustomComparers.TypeComparison));
		}

		public override Customs.Business.CusEntryHeader GetEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaidBy = Enterprise.MasterFiles.Business.Customs.PaidByCodeList.Codes.CLI;

			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_BGMReference = "Test1234";
			var fee = entryHeader.AllEntryLines.AddNew().Fees.AddNew();
			fee.CF_ChargeAmount = 100m;
			fee.CF_ChargeType = UniversalReferenceConstants.RefCusTaxOrFeeCodes.VAT;
			return entryHeader;
		}

		protected override void SetUp()
		{
			base.SetUp();
			chgCode = Factory.NewWithValidTestData<AccChargeCode>();
			chgCode.AC_ChargeType = "DSB";
			chgCode.AC_Code = "TST2";
			chgCode.AC_GC = GlbCompany.CurrentCompany.PK;

			var chargeTypeSettings = new EntryChargeTypeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
			var chargeTypeSetting = chargeTypeSettings.AddNew();
			chargeTypeSetting.ChargeType = EntryChargeTypeList.Codes.A10;
			chargeTypeSetting.AC_ChargeCode = chgCode.PK;
			RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, chargeTypeSettings);

			creditor = Factory.NewWithValidTestData<OrgHeader>();
			Enterprise.Registry.Business.RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, creditor.PK.ToGuid());
			RatingDataRegistry.Instance.IncludeEntryHeaderReferenceInCustomsDisbursementCharges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Factory.Save();
		}
		AccChargeCode chgCode;
		OrgHeader creditor;
	}
}
