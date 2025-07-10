using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Accounting.EInvoicing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class MauritiusComplianceInfo : CountryComplianceInfo,
		IComplianceInfoElectronicInvoicing
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Mauritius;

		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;

		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;

		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.VATCode;

		protected override bool? GetIsReciprocal() => true;

		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;

		#endregion

		#region IComplianceInfoElectronicInvoicing

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate()
			=> Env.Instance.IsProductionSystem
				? new ZDate(2024, 5, 1)
				: new ZDate(2024, 4, 1);

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDateForPayables() => ZDate.Empty;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatus() => Constants.EInvoicingPivotState.Queued;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany) => string.Empty;

		MultilingualString IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingPivotPendingStatusDescription() => IEInvoicingTransactionConstants.DefaultEInvoicingPivotPendingStatusDescription;

		string IComplianceInfoElectronicInvoicing.GetGovernmentAllocatedNumberColumnName()
			=> AccTransactionHeader.Schema.AH_GovernmentAllocatedID;

		string IComplianceInfoElectronicInvoicing.GetAccTransactionHeaderAuthorisationRecordType()
			=> AccTransactionHeaderAuthorisationRecordTypes.Mauritius;

		#endregion
	}
}
