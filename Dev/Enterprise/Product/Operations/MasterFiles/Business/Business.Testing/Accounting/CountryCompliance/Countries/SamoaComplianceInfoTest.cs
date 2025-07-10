using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.CountryCompliance.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing.Accounting
{
	[TestedType(typeof(SamoaComplianceInfo))]
	sealed class SamoaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.WesternSamoa;

		protected override ZDate ExpectedEInvoicingComplianceDate => new ZDate(2021, 7, 1);

		protected override string ExpectedComplianceVersionNo => "2020.11.01";

		protected override IEnumerable<CodeDescriptionBoolRelatedItem> ExpectedTaxMessageGroups =>
			new CodeDescriptionBoolRelatedItem[]
			{
				new CodeDescriptionBoolRelatedItem() { Code = "A", Description = (NoResString)"VAGST", Bool = true, RelatedItemCode = "A" },
				new CodeDescriptionBoolRelatedItem() { Code = "B", Description = (NoResString)"VAGST-EXPORT", Bool = true, RelatedItemCode = "B" },
				new CodeDescriptionBoolRelatedItem() { Code = "C", Description = (NoResString)"VAGST-EXCL", Bool = true, RelatedItemCode = "C" },
				new CodeDescriptionBoolRelatedItem() { Code = "E", Description = (NoResString)"STT", Bool = false, RelatedItemCode = "E" },
				new CodeDescriptionBoolRelatedItem() { Code = "F", Description = (NoResString)"ECAL", Bool = false, RelatedItemCode = "F" },
				new CodeDescriptionBoolRelatedItem() { Code = "N", Description = (NoResString)"N-TAX", Bool = true, RelatedItemCode = "N" },
				new CodeDescriptionBoolRelatedItem() { Code = "P", Description = (NoResString)"PBL", Bool = false, RelatedItemCode = "P" },
				new CodeDescriptionBoolRelatedItem() { Code = "0", Description = (NoResString)"NOT REQ FOR FISCALIZATION", Bool = true, RelatedItemCode = "0" }
			};

		protected override string ExpectedAccTransactionHeaderAuthorisationRecordType => AccTransactionHeaderAuthorisationRecordTypes.WesternSamoa;
	}
}
