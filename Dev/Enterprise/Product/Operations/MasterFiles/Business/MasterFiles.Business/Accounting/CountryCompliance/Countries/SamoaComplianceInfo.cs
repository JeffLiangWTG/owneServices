using System.Globalization;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business.Accounting.EInvoicing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class SamoaComplianceInfo : CountryComplianceInfo,
		ITaxMessagesGroupProvider,
		IComplianceInfoElectronicInvoicing
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.WesternSamoa;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.GSTCode;
		protected override string GetConsumptionTaxCode() => "VAGST"; // TODO: Investigate this code, and how it integrates with OrgCusCodes. It has a 5 character long name that breaks assumptions.
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.CorporationCode;
		protected override bool? GetIsReciprocal() => true;

		#endregion

		protected override string GetComplianceVersionNo()
		{
			var currentVersionNumber = new VersionNumber(2020, 11, 1, 0);
			return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", currentVersionNumber.Major, currentVersionNumber.Minor, currentVersionNumber.Release.ToString("D2"));
		}

		#region IComplianceInfoElectronicInvoicing

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate() => new ZDate(2021, 7, 1);

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDateForPayables() => ZDate.Empty;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatus() => Constants.EInvoicingPivotState.Queued;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany) => string.Empty;

		MultilingualString IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingPivotPendingStatusDescription() => IEInvoicingTransactionConstants.DefaultEInvoicingPivotPendingStatusDescription;

		string IComplianceInfoElectronicInvoicing.GetGovernmentAllocatedNumberColumnName() => AccEInvoicingBatchSchema.Constants.AIB_GovernmentAllocatedNumber;

		public string GetAccTransactionHeaderAuthorisationRecordType() => AccTransactionHeaderAuthorisationRecordTypes.WesternSamoa;

		#endregion

		#region ITaxMessagesGroupProvider
		CodeDescriptionBoolRelatedItemCollection ITaxMessagesGroupProvider.GetTaxMessageGroup()
		{
			return new CodeDescriptionBoolRelatedItemCollection
			{
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.A, Description = TaxMessageGroupDescriptions.A, Bool = true, RelatedItemCode = TaxMessageGroupCodes.A },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.B, Description = TaxMessageGroupDescriptions.B, Bool = true, RelatedItemCode = TaxMessageGroupCodes.B },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.C, Description = TaxMessageGroupDescriptions.C, Bool = true, RelatedItemCode = TaxMessageGroupCodes.C },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.E, Description = TaxMessageGroupDescriptions.E, Bool = false, RelatedItemCode = TaxMessageGroupCodes.E },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.F, Description = TaxMessageGroupDescriptions.F, Bool = false, RelatedItemCode = TaxMessageGroupCodes.F },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N, Description = TaxMessageGroupDescriptions.N, Bool = true, RelatedItemCode = TaxMessageGroupCodes.N },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.P, Description = TaxMessageGroupDescriptions.P, Bool = false, RelatedItemCode = TaxMessageGroupCodes.P },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.NOT_REQ, Description = TaxMessageGroupDescriptions.NOT_REQ, Bool = true, RelatedItemCode = TaxMessageGroupCodes.NOT_REQ }
			};
		}

		public static class TaxMessageGroupCodes
		{
			public const string A = "A";
			public const string B = "B";
			public const string C = "C";
			public const string E = "E";
			public const string F = "F";
			public const string N = "N";
			public const string P = "P";
			public const string NOT_REQ = "0";
		}

		#region SuppressResourceStringsCheckRegion

		static class TaxMessageGroupDescriptions
		{
			public static MultilingualString A => (NoResString)"VAGST";
			public static MultilingualString B => (NoResString)"VAGST-EXPORT";
			public static MultilingualString C => (NoResString)"VAGST-EXCL";
			public static MultilingualString E => (NoResString)"STT";
			public static MultilingualString F => (NoResString)"ECAL";
			public static MultilingualString N => (NoResString)"N-TAX";
			public static MultilingualString P => (NoResString)"PBL";
			public static MultilingualString NOT_REQ => (NoResString)"NOT REQ FOR FISCALIZATION";
		}

		#endregion

		#endregion
	}
}
