using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class InvoiceLineApportionCharge : EU.Business.Declaration.InvoiceLineApportionCharge, Integration.Customs.TR.IInvoiceLineApportionCharge
	{
		public InvoiceLineApportionCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		public override ZString J7_ChargeType
		{
			get => base.J7_ChargeType;
			set
			{
				var oldValue = J7_ChargeType;

				base.J7_ChargeType = value;
				if (!IsCopying && oldValue != J7_ChargeType)
				{
					if (J7_RX_NKCurrency.IsEmpty && value == TRIncotermChargeCodeList.Codes.LocalTotalCharges)
					{
						J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Turkey;
					}
				}
			}
		}

		protected override JobComInvHeaderChargeValidation GetNewValidation() => new InvoiceLineApportionChargeValidation(this);

		protected override JobComInvHeaderChargeLookups GetNewLookups() => new InvoiceLineApportionChargeLookups(this);

		protected override bool GetJ7_IsStatisticalValueApplicable_ReadOnly() => true;

		protected override bool GetIsJ7_ExchangeRateUserEnterableReadOnly() => true;
	}
}
