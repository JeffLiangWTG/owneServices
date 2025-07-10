using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business.Accounting.EInvoicing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public sealed class HungaryComplianceInfo : CountryComplianceInfo, ITaxMessagesGroupProvider, IComplianceInfoElectronicInvoicing
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Hungary;

		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;

		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.CorporationCode;

		protected override string GetRecipientLocalBusinessRegNumberCodeType() => OrgCusCode.CodeTypes.TaxFileCode;

		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;

		protected override bool? GetIsReciprocal() => true;

		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;

		protected override string GetRecipientLocalBusinessRegHeading() => (NoResString)"CLIENT TAX #";

		#endregion

		#region IComplianceInfoElectronicInvoicing

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate() => new ZDate(2020, 7, 1);

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDateForPayables() => ZDate.Empty;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatus() => Constants.EInvoicingPivotState.Queued;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany) => string.Empty;

		MultilingualString IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingPivotPendingStatusDescription() => IEInvoicingTransactionConstants.DefaultEInvoicingPivotPendingStatusDescription;

		string IComplianceInfoElectronicInvoicing.GetGovernmentAllocatedNumberColumnName() => AccEInvoicingBatchSchema.Constants.AIB_GovernmentAllocatedNumber;

		string IComplianceInfoElectronicInvoicing.GetAccTransactionHeaderAuthorisationRecordType() => string.Empty;

		#endregion

		#region ITaxMessagesGroupProvider

		CodeDescriptionBoolRelatedItemCollection ITaxMessagesGroupProvider.GetTaxMessageGroup()
		{
			return new CodeDescriptionBoolRelatedItemCollection
			{
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.H01, Description = TaxMessageGroupDescriptions.H01, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.H01 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.H02, Description = TaxMessageGroupDescriptions.H02, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.H02 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.H03, Description = TaxMessageGroupDescriptions.H03, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.H03 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.H04, Description = TaxMessageGroupDescriptions.H04, Bool = false, RelatedItemCode = GovernmentTaxGroupCodes.H04 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.H05, Description = TaxMessageGroupDescriptions.H05, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.H05 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.H06, Description = TaxMessageGroupDescriptions.H06, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.H06 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.H21, Description = TaxMessageGroupDescriptions.H21, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.H21 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.H22, Description = TaxMessageGroupDescriptions.H22, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.H22 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.H23, Description = TaxMessageGroupDescriptions.H23, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.H23 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.H24, Description = TaxMessageGroupDescriptions.H24, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.H24 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.H25, Description = TaxMessageGroupDescriptions.H25, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.H25 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.H26, Description = TaxMessageGroupDescriptions.H26, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.H26 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.H00, Description = TaxMessageGroupDescriptions.H00, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.H00 },
			};
		}

		public static class TaxMessageGroupCodes
		{
			public const string H01 = "H01";
			public const string H02 = "H02";
			public const string H03 = "H03";
			public const string H04 = "H04";
			public const string H05 = "H05";
			public const string H06 = "H06";
			public const string H21 = "H21";
			public const string H22 = "H22";
			public const string H23 = "H23";
			public const string H24 = "H24";
			public const string H25 = "H25";
			public const string H26 = "H26";
			public const string H00 = "H00";
			public const string UKN = "UNKNOWN";
		}

		public static class GovernmentTaxGroupCodes
		{
			public const string H01 = "AAM";
			public const string H02 = "TAM";
			public const string H03 = "KBAET";
			public const string H04 = "KBAUK";
			public const string H05 = "EAM";
			public const string H06 = "NAM";
			public const string H21 = "ATK";
			public const string H22 = "THK";
			public const string H23 = "EUFAD37";
			public const string H24 = "EUFADE";
			public const string H25 = "EUE";
			public const string H26 = "HO";
			public const string H00 = "";
			public const string UKN = "UNKNOWN";
		}

		#region SuppressResourceStringsCheckRegion

		public static class TaxMessageGroupDescriptions
		{
			public static MultilingualString H01 => (NoResString)"Alanyi adómentes";
			public static MultilingualString H02 => (NoResString)"„tárgyi adómentes” ill. a tevékenység közérdekű vagy speciális jellegére tekintettel adómentes";
			public static MultilingualString H03 => (NoResString)"adómentes Közösségen belüli termékértékesítés, új közlekedési eszköz nélkül";
			public static MultilingualString H04 => (NoResString)"adómentes Közösségen belüli új közlekedési eszköz értékesítés";
			public static MultilingualString H05 => (NoResString)"adómentes termékértékesítés a Közösség területén kívülre (termékexport harmadik országba)";
			public static MultilingualString H06 => (NoResString)"egyéb nemzetközi ügyletekhez kapcsolódó jogcímen megállapított adómentesség";
			public static MultilingualString H21 => (NoResString)"Áfa tárgyi hatályán kívül/Outside the scope of VAT";
			public static MultilingualString H22 => (NoResString)"területi hatályon kívül";
			public static MultilingualString H23 => (NoResString)"Áfa tv. 37. §-a alapján másik tagállamban teljesített, fordítottan adózó ügylet";
			public static MultilingualString H24 => (NoResString)"Másik tagállamban teljesített, nem az Áfa tv. 37. §-a alá tartozó, fordítottan adózó ügylet";
			public static MultilingualString H25 => (NoResString)"Másik tagállamban teljesített, nem fordítottan adózó ügylet";
			public static MultilingualString H26 => (NoResString)"Harmadik országban teljesített ügylet";
			public static MultilingualString H00 => (NoResString)"Nem jelenthető";
			public static MultilingualString UKN => (NoResString)"3.0 előtti számlára hivatkozó, illetve előzmény nélküli módosító és sztornó számlák esetén használható, ha nem megállapítható az érték";
		}

		#endregion

		#endregion
	}
}
