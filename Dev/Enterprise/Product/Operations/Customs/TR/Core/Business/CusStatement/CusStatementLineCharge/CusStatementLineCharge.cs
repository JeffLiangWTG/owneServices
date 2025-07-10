using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business
{
	[DependentBusinessObject(typeof(CusStatementLine), "Charges")]
	public class CusStatementLineCharge : BaseCusStatementLineCharge, Integration.Customs.TR.ICusStatementLineCharge
	{
		public CusStatementLineCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusStatementLine StatementLine => (CusStatementLine)base.StatementLine;
		public new CusStatementLineChargeLookups Lookups => (CusStatementLineChargeLookups)base.Lookups;
		protected override Customs.Business.CusStatementLineChargeLookups GetNewLookups() => new CusStatementLineChargeLookups(this);

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateNumberPropertyIfRequired(B4_ReferenceNumberInfo, GetNewStatementNumber);
		}

		ZString GetNewStatementNumber(BusinessObjectFactory factory)
		{
			var companyCode = this.StatementLine?.StatementHeader.Company?.GC_Code ?? GlbCompany.CurrentCompany.GC_Code;
			return StampDutyLedgerNumberFountainHelper.GetNextStampDutyLedgerNumber(factory, companyCode);
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded && !IsInDatabase)
			{
				B4_ReferenceNumber = ZString.Empty;
			}

			base.OnSaved(saveSucceeded);
		}

		[ResourceStringData("A5BE018D-BD52-4E9A-B716-05BDB79B342E", Caption = "Stamp Duty Ledger No", ShortCaption = "S.D.Ledger No")]
		public override ZString B4_ReferenceNumber { get => base.B4_ReferenceNumber; set => base.B4_ReferenceNumber = value; }

		[ResourceStringData("F9FCA3A7-1513-4390-B6B7-37A2A24A9864", Caption = "Charge Type")]
		[List(nameof(Lookups) + "." + nameof(CusStatementLineChargeLookups.TaxOrFeeCodeList))]
		public override ZString B4_ChargeType
		{
			get => base.B4_ChargeType;
			set
			{
				var oldValue = B4_ChargeType;
				base.B4_ChargeType = value;

				if (B4_ChargeAmount.IsEmpty && oldValue != B4_ChargeType && !IsCopying)
				{
					var stampDutyTax = new Universal.RefCusTaxOrFee.Loader(Factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Turkey, value, ZDateTime.Today);

					if (stampDutyTax != null)
					{
						B4_ChargeAmount = stampDutyTax.ZZF_Value;
					}
				}
			}
		}

		[ResourceStringData("118605A7-FBD2-4049-AECA-90BBF379937D", Caption = "Charge Amount")]
		public override ZDecimal B4_ChargeAmount
		{
			get => base.B4_ChargeAmount;
			set
			{
				var oldValue = B4_ChargeAmount;
				base.B4_ChargeAmount = value;
				if (oldValue != B4_ChargeAmount && !IsCopying)
				{
					StatementLine?.StatementHeader?.ReCalcTotalChargeAmount();
				}
			}
		}

		public override void Delete()
		{
			var header = StatementLine?.StatementHeader;
			base.Delete();
			header?.ReCalcTotalChargeAmount();
		}
	}
}
