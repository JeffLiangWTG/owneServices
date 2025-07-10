using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business.Accounting.EInvoicing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	//Note: UK eInvoicing is actually D365 for EDI only.
	public class UnitedKingdomComplianceInfo : CountryComplianceInfo,
		IComplianceInfoElectronicInvoicing
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.UnitedKingdom;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.CompanyNumber;
		protected override bool? GetIsReciprocal() => false;
		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;

		#endregion

		#region IComplianceInfoElectronicInvoicing

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate() => ZDate.Empty;

		public ZDate GetEInvoicingComplianceDateForPayables() => ZDate.Empty;

		public string GetDefaultEInvoicingSubmitPivotStatus() => Constants.EInvoicingPivotState.Queued;
		
		public string GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany = null) => string.Empty;

		public MultilingualString GetDefaultEInvoicingPivotPendingStatusDescription() => IEInvoicingTransactionConstants.DefaultEInvoicingPivotPendingStatusDescription;

		public string GetGovernmentAllocatedNumberColumnName() => AccTransactionHeader.Schema.AH_GovernmentAllocatedID;

		public string GetAccTransactionHeaderAuthorisationRecordType() => string.Empty;
		
		#endregion
	}
}
