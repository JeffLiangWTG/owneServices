using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	public class InvoicingTestHelper
	{
		public InvoicingTestHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		public void SetUpDisbursementCreditorAndChargeCode()
		{
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, DisbursementCreditor.PK.ToGuid());
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, DisbursementChargeCode.PK.ToGuid());
			RatingDataRegistry.Instance.CustomDeferredChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, DeferredChargeCode.PK.ToGuid());
			RatingDataRegistry.Instance.IncludeCustomDeferredChargeInInvoicing.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			new AccountingPeriodTestHelper(factory).SetupPeriods();
		}

		public AccChargeCode DeferredChargeCode
		{
			get
			{
				if (fDeferredChargeCode == null)
				{
					fDeferredChargeCode = CreateOrGetChargeCode("~d~", "Deferred Customs Charges (Information Only)", Constants.ChargeType.Comment);
				}
				return fDeferredChargeCode;
			}
		}
		AccChargeCode fDeferredChargeCode;

		public AccChargeCode DisbursementChargeCode
		{
			get
			{
				if (fDisbursementChargeCode == null)
				{
					fDisbursementChargeCode = CreateOrGetChargeCode("~c~", "Customs Disbursements", Constants.ChargeType.Disbursement);
				}
				return fDisbursementChargeCode;
			}
		}
		AccChargeCode fDisbursementChargeCode;

		AccChargeCode CreateOrGetChargeCode(string code, string description, string chargeType)
		{
			var chargeCode = factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "ZZ" + code));
			if (chargeCode == null)
			{
				chargeCode = factory.New<AccChargeCode>();
				chargeCode.AC_Code = "ZZ" + code;
			}

			chargeCode.AC_Desc = description;
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.CustomsDuty;
			chargeCode.AC_ChargeType = chargeType;
			chargeCode.AC_MarginPercentage = 0m;
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode.SetGLAccountDataForTesting();

			chargeCode.AC_AT_GSTRate = ZeroTaxRate.PK;
			factory.Save();

			return chargeCode;
		}

		AccTaxRate ZeroTaxRate
		{
			get
			{
				if (fZeroTaxRate == null)
				{
					fZeroTaxRate = AccTaxRate.CreateTaxRate_ForTestOnly(factory);
					fZeroTaxRate.SetRateNumerator_ForTestOnly(0);
				}
				return fZeroTaxRate;
			}
		}
		AccTaxRate fZeroTaxRate;

		public OrgHeader DisbursementCreditor
		{
			get
			{
				if (fDisbursementCreditor == null)
				{
					fDisbursementCreditor = CreateOrgHeader("~o~", true, false);
				}
				return fDisbursementCreditor;
			}
		}
		OrgHeader fDisbursementCreditor;

		public OrgHeader Importer
		{
			get
			{
				if (importer == null)
				{
					importer = CreateOrgHeader("~i~", false, true);
				}
				return importer;
			}
		}
		OrgHeader importer;

		protected OrgHeader CreateOrgHeader(string code, bool creditor, bool debtor)
		{
			OrgHeader header = factory.New<OrgHeader>();
			header.OH_FullName = "Test Company Name";
			header.MainAddress.OA_Address1 = "184 Bourke Road";
			header.MainAddress.OA_City = "Alexandria";
			header.MainAddress.OA_State = "NSW";
			header.OH_Code = "Z" + code;
			header.OH_IsDebtor = debtor;
			header.OH_IsCreditor = creditor;

			if (debtor)
			{
				header.CompanyData.SetARTaxApplicable(true);
				header.MiscServ.OM_ARWHTApplicable = true;
			}

			if (creditor)
			{
				header.CompanyData.SetAPTaxApplicable(true);
				header.MiscServ.OM_APWHTApplicable = true;
			}

			factory.Save();
			return header;
		}
	}
}
