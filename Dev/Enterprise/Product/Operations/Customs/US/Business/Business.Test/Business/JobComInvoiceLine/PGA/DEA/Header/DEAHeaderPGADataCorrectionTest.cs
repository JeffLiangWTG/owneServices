using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DEAHeader))]
	class DEAHeaderPGADataCorrectionTest : PGADataCorrectionlTest<DEAHeader>
	{
		protected override DEAHeader[] SetupDataForParentFieldsAreSetupForTracking()
		{
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_DEAInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine2.US_DEADisclaimReason = PGADisclaimReasonList.Codes.A;
			return base.SetupDataForParentFieldsAreSetupForTracking();
		}

		protected override Type GetInterfaceType()
		{
			return typeof(IDEAHeader);
		}

		protected override bool IsTypeToIgnoreAccessCheck(BusinessObject bo)
		{
			return bo is DEAHeader || bo is AutoUSDEAHeaderAddInfo;
		}

		protected override DEAHeader GetNewPGA()
		{
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
			var result = invoiceLine.DEAHeaders.AddNew();
			result.US_RegistrationNumber = "123456789";
			result.US_CountryOfShipment = "CN";
			result.US_FormID = "DEA-35";
			return result;
		}

		protected override void SetPGAData(DEAHeader dea)
		{
			dea.US_PermitNumber = "1234567";
		}

		protected override bool FieldsToExcludeFromTracking(string fieldName)
		{
			return base.FieldsToExcludeFromTracking(fieldName) ||
				fieldName.EndsWith("AddInfoJobDeclaration.US_EntryFilerCode"); // Entry Filer Code is only set once.
		}

		protected override List<ZString> PGARelatedParentColumnUsedInPGABlocksCreator()
		{
			var result = base.PGARelatedParentColumnUsedInPGABlocksCreator();
			result.Add("Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_DEAInd");
			return result;
		}
	}
}
