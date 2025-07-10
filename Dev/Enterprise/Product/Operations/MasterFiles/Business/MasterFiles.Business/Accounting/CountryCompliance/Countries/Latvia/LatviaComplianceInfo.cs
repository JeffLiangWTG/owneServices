using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Accounting.EInvoicing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	class LatviaComplianceInfo : CountryComplianceInfo, IComplianceInfoElectronicInvoicing
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Latvia;

		protected override string GetConsumptionTaxCode() => LatviaOrgCusCodeInfo.OrgCusCodes.PVN;

		protected override string GetConsumptionTaxRegistrationCode() => LatviaOrgCusCodeInfo.OrgCusCodes.PVN;

		protected override string GetLocalBusinessRegNoCodeType() => LatviaOrgCusCodeInfo.OrgCusCodes.PVN;

		protected override bool? GetIsReciprocal() => true;

		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;

		#endregion

		#region EInvoicing
		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate()
			=> Env.Instance.IsProductionSystem
				? ZDate.Empty
				: new ZDate(2040, 1, 1);

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDateForPayables() => ZDate.Empty;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatus() => Constants.EInvoicingPivotState.Queued;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany) => string.Empty;

		MultilingualString IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingPivotPendingStatusDescription() => IEInvoicingTransactionConstants.DefaultEInvoicingPivotPendingStatusDescription;

		string IComplianceInfoElectronicInvoicing.GetGovernmentAllocatedNumberColumnName()
			=> AccTransactionHeader.Schema.AH_GovernmentAllocatedID;

		string IComplianceInfoElectronicInvoicing.GetAccTransactionHeaderAuthorisationRecordType()
			=> AccTransactionHeaderAuthorisationRecordTypes.Latvia;

		#endregion
	}
}
