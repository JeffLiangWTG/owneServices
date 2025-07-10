using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class CaribbeanBasinTradePartnershipActCertificateValidator : PermitValidator
	{
		public CaribbeanBasinTradePartnershipActCertificateValidator(JobComInvoiceLine invoiceLine)
			: base(LicencePermitTypeList.Codes._18, invoiceLine)
		{
		}

		protected override bool IsPermitNoRequired(ZString entryType)
		{
			var result = false;
			var tariff = InvoiceLine.ImportSupTariff ?? InvoiceLine.ImportTariff;
			if (tariff != null)
			{
				result = tariff.UE_Tariff.StartsWith("98201115") && entryType != EntryTypeList.Codes.Warehouse && entryType != EntryTypeList.Codes.ReWarehouse;
			}

			return result;
		}

		protected override ZString GetErrorTextForExtraCondition(ZString permit)
		{
			var result = ZString.Empty;
			var tariff = InvoiceLine.ImportSupTariff ?? InvoiceLine.ImportTariff;
			if (!permit.IsEmpty && tariff == null)
			{
				result = CBTPACertificateNoMayOnlyBeEnteredForTariff9820115;
			}

			return result;
		}

		internal const string CBTPACertificateNoMayOnlyBeEnteredForTariff9820115 = "CBTPA Certificate No may only be entered for tariff no '9820.11.15'.";
	}
}
