using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(TSCADataCorrection))]
	class TSCADataCorrectionTest : PGADataCorrectionlCoreTest<TSCADataCorrection>
	{
		protected override Type GetInterfaceType()
		{
			return typeof(ITSCAData);
		}

		protected override TSCADataCorrection[] SetupDataForParentFieldsAreSetupForTracking()
		{
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_TSCAInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine2.US_TSCADisclaimReason = PGADisclaimReasonList.Codes.A;
			return base.SetupDataForParentFieldsAreSetupForTracking();
		}

		protected override TSCADataCorrection GetNewPGA()
		{
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			return invoiceLine.TSCADataCorrection;
		}

		protected override TSCADataCorrection GetPGAInDiffFactory(BusinessObjectFactory factory, TSCADataCorrection pga)
		{
			var invoiceLine = factory.Load<JobComInvoiceLine>(pga.InvoiceLine.PK);
			return invoiceLine.TSCADataCorrection;
		}

		protected override bool IsTypeToIgnoreAccessCheck(BusinessObject bo)
		{
			return false;
		}

		protected override void SetPGAData(TSCADataCorrection pga)
		{
			pga.InvoiceLine.US_FDAContactName = "BOB";
		}

		protected override bool FieldsToExcludeFromTracking(string fieldName)
		{
			return base.FieldsToExcludeFromTracking(fieldName) ||
				fieldName.EndsWith(".AddInfoJobComInvoiceLine.US_ODSInd") ||
				fieldName.EndsWith(".AddInfoJobComInvoiceLine.US_ODSDisclaimReason") ||
				fieldName.EndsWith(".AddInfoJobComInvoiceLine.US_ODSTrackingStatus") ||
				fieldName.EndsWith(".AddInfoJobComInvoiceLine.US_TSCATrackingStatus") ||
				fieldName.EndsWith("AddInfoJobComInvoiceHeader.US_TSCASignDate");
		}

		protected override List<ZString> PGARelatedParentColumnUsedInPGABlocksCreator()
		{
			var result = base.PGARelatedParentColumnUsedInPGABlocksCreator();
			result.Add("Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_TSCAInd");
			return result;
		}
	}
}
