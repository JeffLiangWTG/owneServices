using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(SaudiArabiaComplianceInfo))]
	sealed class SaudiArabiaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => CountryCodes.SaudiArabia;
		protected override bool ComplianceDateDependOfIsProductionSystem => true;
		protected override ZDate ExpectedEInvoicingComplianceDate => IsProductionSystem ? new ZDate(2023, 01, 01) : new ZDate(2022, 11, 01);

		protected override string ExpectedAccTransactionHeaderAuthorisationRecordType => AccTransactionHeaderAuthorisationRecordTypes.SaudiArabia;

		protected override IEnumerable<CodeDescriptionBoolRelatedItem> ExpectedTaxMessageGroups =>
			new CodeDescriptionBoolRelatedItem[]
			{
				new CodeDescriptionBoolRelatedItem() { Code = "29", Description = (NoResString)"Financial services mentioned in Article 29 of the VAT Regulations", Bool = true, RelatedItemCode = "29" },
				new CodeDescriptionBoolRelatedItem() { Code = "297", Description = (NoResString)"Life insurance services mentioned in Article 29 of the VAT Regulations", Bool = true, RelatedItemCode = "29-7" },
				new CodeDescriptionBoolRelatedItem() { Code = "30", Description = (NoResString)"Real estate transactions mentioned in Article 30 of the VAT Regulations", Bool = true, RelatedItemCode = "30" },
				new CodeDescriptionBoolRelatedItem() { Code = "32", Description = (NoResString)"Export of goods", Bool = true, RelatedItemCode = "32" },
				new CodeDescriptionBoolRelatedItem() { Code = "33", Description = (NoResString)"Export of services", Bool = true, RelatedItemCode = "33" },
				new CodeDescriptionBoolRelatedItem() { Code = "341", Description = (NoResString)"The international transport of goods", Bool = true, RelatedItemCode = "34-1" },
				new CodeDescriptionBoolRelatedItem() { Code = "342", Description = (NoResString)"International transport of passengers", Bool = true, RelatedItemCode = "34-2" },
				new CodeDescriptionBoolRelatedItem() { Code = "343", Description = (NoResString)"Services directly connected and incidental to a Supply of international passenger transport", Bool = true, RelatedItemCode = "34-3" },
				new CodeDescriptionBoolRelatedItem() { Code = "344", Description = (NoResString)"Supply of a qualifying means of transport", Bool = true, RelatedItemCode = "34-4" },
				new CodeDescriptionBoolRelatedItem() { Code = "345", Description = (NoResString)"Any services relating to Goods or passenger transportation, as defined in article twenty five of these Regulations", Bool = true, RelatedItemCode = "34-5" },
				new CodeDescriptionBoolRelatedItem() { Code = "35", Description = (NoResString)"Medicines and medical equipment", Bool = true, RelatedItemCode = "35" },
				new CodeDescriptionBoolRelatedItem() { Code = "36", Description = (NoResString)"Qualifying metals", Bool = true, RelatedItemCode = "36" },
				new CodeDescriptionBoolRelatedItem() { Code = "EDU", Description = (NoResString)"Private education to citizen", Bool = true, RelatedItemCode = "EDU" },
				new CodeDescriptionBoolRelatedItem() { Code = "HEA", Description = (NoResString)"Private healthcare to citizen", Bool = true, RelatedItemCode = "HEA" },
				new CodeDescriptionBoolRelatedItem() { Code = "NON", Description = (NoResString)"No exemption reason", Bool = true, RelatedItemCode = "" },
				new CodeDescriptionBoolRelatedItem() { Code = "OOS", Description = (NoResString)"Not subject to VAT", Bool = true, RelatedItemCode = "OOS" },
				new CodeDescriptionBoolRelatedItem() { Code = "MIL", Description = (NoResString)"Supply of qualified military goods", Bool = true, RelatedItemCode = "MLTRY" },
			};
	}
}
