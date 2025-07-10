using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(APHISHeader))]
	class APHISHeaderPGADataCorrectionTest : PGADataCorrectionlTest<APHISHeader>
	{
		protected override APHISHeader[] SetupDataForParentFieldsAreSetupForTracking()
		{
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_APHISInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine2.US_APHISDisclaimReason = PGADisclaimReasonList.Codes.A;
			return base.SetupDataForParentFieldsAreSetupForTracking();
		}

		protected override Type GetInterfaceType()
		{
			return typeof(IAPHISHeader);
		}

		protected override bool IsTypeToIgnoreAccessCheck(BusinessObject bo)
		{
			return bo is APHISHeader || bo is USAPHISHeaderAddInfo;
		}

		protected override APHISHeader GetNewPGA()
		{
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
			var result = invoiceLine.APHISHeaders.AddNew();
			result.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.APHISVSPortVeterinarian;
			result.Inspections.RemoveAndDeleteAll();
			return result;
		}

		protected override void SetPGAData(APHISHeader pga)
		{
			pga.US_CategoryType = APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery;
		}

		protected override bool FieldsToExcludeFromTracking(string fieldName)
		{
			return base.FieldsToExcludeFromTracking(fieldName) ||
				fieldName.EndsWith("AddInfoJobDeclaration.US_EntryFilerCode"); // Entry Filer Code is only set once.
		}

		protected override List<ZString> PGARelatedParentColumnUsedInPGABlocksCreator()
		{
			var result = base.PGARelatedParentColumnUsedInPGABlocksCreator();
			result.Add("Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_APHISInd");
			return result;
		}
	}
}
