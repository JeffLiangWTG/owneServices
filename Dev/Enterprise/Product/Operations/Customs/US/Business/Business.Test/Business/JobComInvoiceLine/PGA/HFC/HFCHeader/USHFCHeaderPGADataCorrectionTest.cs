using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USHFCHeader))]
	public class USHFCHeaderPGADataCorrectionTest : PGADataCorrectionlTest<USHFCHeader>
	{
		protected override Type GetInterfaceType()
		{
			return typeof(IHFCHeader);
		}

		protected override USHFCHeader GetNewPGA()
		{
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Declared;
			var result = invoiceLine.USHFCHeaders.AddNew();
			return result;
		}

		protected override bool IsTypeToIgnoreAccessCheck(BusinessObject bo)
		{
			return bo is USHFCHeader || bo is USHFCHeaderAddInfo;
		}

		protected override void SetPGAData(USHFCHeader pga)
		{
			pga.US_ASHRAENumber = "R-1A";
		}

		protected override USHFCHeader[] SetupDataForParentFieldsAreSetupForTracking()
		{
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Declared;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_HFCInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine2.US_HFCDisclaimReason = PGADisclaimReasonList.Codes.A;
			return base.SetupDataForParentFieldsAreSetupForTracking();
		}

		protected override List<ZString> PGARelatedParentColumnUsedInPGABlocksCreator()
		{
			var result = base.PGARelatedParentColumnUsedInPGABlocksCreator();
			result.Add("Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_HFCInd");
			return result;
		}
	}
}
