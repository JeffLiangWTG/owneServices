using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FWSHeader))]
	class FWSHeaderPGADataCorrectionTest : PGADataCorrectionlTest<FWSHeader>
	{
		protected override FWSHeader[] SetupDataForParentFieldsAreSetupForTracking()
		{
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_FWSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine2.US_FWSDisclaimReason = PGADisclaimReasonList.Codes.A;
			return base.SetupDataForParentFieldsAreSetupForTracking();
		}

		protected override Type GetInterfaceType()
		{
			return typeof(IFWSHeader);
		}

		protected override bool IsTypeToIgnoreAccessCheck(BusinessObject bo)
		{
			return bo is FWSHeader || bo is USFWSHeaderAddInfo;
		}

		protected override FWSHeader GetNewPGA()
		{
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
			var result = invoiceLine.FWSHeaders.AddNew();
			return result;
		}

		protected override void SetPGAData(FWSHeader pga)
		{
			pga.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
		}

		protected override bool FieldsToExcludeFromTracking(string fieldName)
		{
			return base.FieldsToExcludeFromTracking(fieldName) ||
				fieldName.EndsWith("AddInfoJobDeclaration.US_EntryFilerCode") || // Entry Filer Code is only set once.
				fieldName.EndsWith("AddInfoJobComInvoiceHeader.US_FWSSignDate");
		}

		protected override List<ZString> PGARelatedParentColumnUsedInPGABlocksCreator()
		{
			var result = base.PGARelatedParentColumnUsedInPGABlocksCreator();
			result.Add("Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_FWSInd");
			return result;
		}
	}
}
