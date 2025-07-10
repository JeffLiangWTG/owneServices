using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(EgyptComplianceInfo))]
	sealed class EgyptComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Egypt;

		protected override bool ComplianceDateDependOfIsProductionSystem => true;

		protected override ZDate ExpectedEInvoicingComplianceDate => IsProductionSystem ? new ZDate(2021, 5, 15) : new ZDate(2021, 4, 1);

		protected override ZString ExpectedDefaultEInvoicingSubmitPivotStatus => Constants.EInvoicingPivotState.Pending;

		protected override ZString ExpectedDefaultEInvoicingPivotPendingStatusDescription => "Pending Digital Signature";

		protected override IEnumerable<CodeDescriptionBoolRelatedItem> ExpectedTaxMessageGroups =>
			new CodeDescriptionBoolRelatedItem[]
			{
				new CodeDescriptionBoolRelatedItem() { Code = "E00", Description = (NoResString)"Not applicable / غير قابل للتطبيق", Bool = true, RelatedItemCode = "" },
				new CodeDescriptionBoolRelatedItem() { Code = "E01", Description = (NoResString)"Export / تصدير للخارج", Bool = true, RelatedItemCode = "V001" },
				new CodeDescriptionBoolRelatedItem() { Code = "E02", Description = (NoResString)"Export to free areas and other areas / تصدير مناطق حرة وأخرى", Bool = true, RelatedItemCode = "V002" },
				new CodeDescriptionBoolRelatedItem() { Code = "E03", Description = (NoResString)"Exempted good or service / سلعة أو خدمة معفاة", Bool = true, RelatedItemCode = "V003" },
				new CodeDescriptionBoolRelatedItem() { Code = "E04", Description = (NoResString)"A non-taxable good or service / سلعة أو خدمة غير خاضعة للضريبة", Bool = true, RelatedItemCode = "V004" },
				new CodeDescriptionBoolRelatedItem() { Code = "E05", Description = (NoResString)"Exemptions for diplomats, consulates and embassies / إعفاءات دبلوماسين والقنصليات والسفارات", Bool = true, RelatedItemCode = "V005" },
				new CodeDescriptionBoolRelatedItem() { Code = "E06", Description = (NoResString)"Defence and National Security exemptions / إعفاءات الدفاع والأمن القومى", Bool = true, RelatedItemCode = "V006" },
				new CodeDescriptionBoolRelatedItem() { Code = "E07", Description = (NoResString)"Agreements exemptions / إعفاءات اتفاقيات", Bool = true, RelatedItemCode = "V007" },
				new CodeDescriptionBoolRelatedItem() { Code = "E08", Description = (NoResString)"Special exemptions and other reasons / إعفاءات خاصة و أخرى", Bool = true, RelatedItemCode = "V008" },
				new CodeDescriptionBoolRelatedItem() { Code = "E09", Description = (NoResString)"General item sales / سلع عامة", Bool = true, RelatedItemCode = "V009" },
				new CodeDescriptionBoolRelatedItem() { Code = "E10", Description = (NoResString)"Other rates / نسب ضريبة أخرى", Bool = true, RelatedItemCode = "V010" },
				new CodeDescriptionBoolRelatedItem() { Code = "N01", Description = (NoResString)"Other fees (rate) / رسوم أخرى (نسبة)", Bool = true, RelatedItemCode = "OF03" },
				new CodeDescriptionBoolRelatedItem() { Code = "N02", Description = (NoResString)"Other fees (amount) / رسوم أخرى (قطعية)", Bool = true, RelatedItemCode = "OF04" },
				new CodeDescriptionBoolRelatedItem() { Code = "T2", Description = (NoResString)"Table tax (percentage) / ضريبه الجدول (نسبيه)", Bool = true, RelatedItemCode = "Tbl01" },
			};

		protected override string ExpectedGovernmentAllocatedNumberColumnName => AccTransactionHeaderAuthorisationRecordSchema.Constants.AHF_Number;

		protected override string ExpectedAccTransactionHeaderAuthorisationRecordType => AccTransactionHeaderAuthorisationRecordTypes.Egypt;
	}
}
