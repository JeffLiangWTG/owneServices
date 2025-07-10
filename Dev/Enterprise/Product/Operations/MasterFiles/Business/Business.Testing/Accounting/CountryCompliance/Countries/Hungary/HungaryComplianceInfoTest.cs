using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(HungaryComplianceInfo))]
	sealed class HungaryComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Hungary;

		protected override string ExpectedRecipientLocalBusinessRegNumberCodeType => "GTX";

		protected override string ExpectedRecipientLocalBusinessRegHeading => "CLIENT TAX #";

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		public void TestGetConsumptionTaxRegistrationOrgCusCode()
		{
			var result = Country.GetConsumptionTaxRegistrationOrgCusCode(CountryCode);
			var expected = "VAT";

			AssertEquals(expected, result);
		}

		public void TestGetConsumptionTaxDescription()
		{
			var result = Country.GetConsumptionTaxDescription(CountryCode);
			var expected = "VAT";

			AssertEquals(expected, result);
		}

		public void TestLocalBusinessRegNoCodeType()
		{
			var hungary = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Hungary);
			AssertEquals(OrgCusCode.CodeTypes.CorporationCode, hungary.LocalBusinessRegNoCodeType);
		}

		protected override ZDate ExpectedEInvoicingComplianceDate => new ZDate(2020, 7, 1);

		protected override IEnumerable<CodeDescriptionBoolRelatedItem> ExpectedTaxMessageGroups =>
			new CodeDescriptionBoolRelatedItem[]
			{
				new CodeDescriptionBoolRelatedItem() { Code = "H01", Description = (NoResString)"Alanyi adómentes", Bool = true, RelatedItemCode = "AAM" },
				new CodeDescriptionBoolRelatedItem() { Code = "H02", Description = (NoResString)"„tárgyi adómentes” ill. a tevékenység közérdekű vagy speciális jellegére tekintettel adómentes", Bool = true, RelatedItemCode = "TAM" },
				new CodeDescriptionBoolRelatedItem() { Code = "H03", Description = (NoResString)"adómentes Közösségen belüli termékértékesítés, új közlekedési eszköz nélkül", Bool = true, RelatedItemCode = "KBAET" },
				new CodeDescriptionBoolRelatedItem() { Code = "H04", Description = (NoResString)"adómentes Közösségen belüli új közlekedési eszköz értékesítés", Bool = false, RelatedItemCode = "KBAUK" },
				new CodeDescriptionBoolRelatedItem() { Code = "H05", Description = (NoResString)"adómentes termékértékesítés a Közösség területén kívülre (termékexport harmadik országba)", Bool = true, RelatedItemCode = "EAM" },
				new CodeDescriptionBoolRelatedItem() { Code = "H06", Description = (NoResString)"egyéb nemzetközi ügyletekhez kapcsolódó jogcímen megállapított adómentesség", Bool = true, RelatedItemCode = "NAM" },
				new CodeDescriptionBoolRelatedItem() { Code = "H21", Description = (NoResString)"Áfa tárgyi hatályán kívül/Outside the scope of VAT", Bool = true, RelatedItemCode = "ATK" },
				new CodeDescriptionBoolRelatedItem() { Code = "H22", Description = (NoResString)"területi hatályon kívül", Bool = true, RelatedItemCode = "THK" },
				new CodeDescriptionBoolRelatedItem() { Code = "H23", Description = (NoResString)"Áfa tv. 37. §-a alapján másik tagállamban teljesített, fordítottan adózó ügylet", Bool = true, RelatedItemCode = "EUFAD37" },
				new CodeDescriptionBoolRelatedItem() { Code = "H24", Description = (NoResString)"Másik tagállamban teljesített, nem az Áfa tv. 37. §-a alá tartozó, fordítottan adózó ügylet", Bool = true, RelatedItemCode = "EUFADE" },
				new CodeDescriptionBoolRelatedItem() { Code = "H25", Description = (NoResString)"Másik tagállamban teljesített, nem fordítottan adózó ügylet", Bool = true, RelatedItemCode = "EUE" },
				new CodeDescriptionBoolRelatedItem() { Code = "H26", Description = (NoResString)"Harmadik országban teljesített ügylet", Bool = true, RelatedItemCode = "HO" },
				new CodeDescriptionBoolRelatedItem() { Code = "H00", Description = (NoResString)"Nem jelenthető", Bool = true, RelatedItemCode = "" }
			};
	}
}
