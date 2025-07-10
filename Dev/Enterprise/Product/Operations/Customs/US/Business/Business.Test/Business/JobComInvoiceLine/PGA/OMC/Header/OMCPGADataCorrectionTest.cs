using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(OMCHeader))]
	internal class OMCPGADataCorrectionTest : PGADataCorrectionlTest<OMCHeader>
	{
		protected override OMCHeader[] SetupDataForParentFieldsAreSetupForTracking()
		{
			invoiceLine.US_OMCInd = OGAIndicatorList.Codes.Declared;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_OMCInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine2.US_OMCDisclaimReason = PGADisclaimReasonList.Codes.A;
			return base.SetupDataForParentFieldsAreSetupForTracking();
		}

		protected override Type GetInterfaceType()
		{
			return typeof(IOMCHeader);
		}

		protected override bool IsTypeToIgnoreAccessCheck(BusinessObject bo)
		{
			return bo is OMCHeader || bo is OMCHeaderAddInfo;
		}

		protected override OMCHeader GetNewPGA()
		{
			invoiceLine.US_OMCInd = OGAIndicatorList.Codes.Declared;
			var result = invoiceLine.OMCHeaders.AddNew();
			result.US_NetWeight = 100m;
			return result;
		}

		protected override void SetPGAData(OMCHeader pga)
		{
			invoiceLine.US_OMCInd = OGAIndicatorList.Codes.Declared;
			pga.US_NetWeight = 50m;
		}

		protected override bool FieldsToExcludeFromTracking(string fieldName)
		{
			return base.FieldsToExcludeFromTracking(fieldName) ||
				fieldName.EndsWith("AddInfoJobDeclaration.US_EntryFilerCode"); // Entry Filer Code is only set once.
		}

		protected override List<ZString> PGARelatedParentColumnUsedInPGABlocksCreator()
		{
			var result = base.PGARelatedParentColumnUsedInPGABlocksCreator();
			result.Add("Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_OMCInd");
			return result;
		}
	}
}
