using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ODSDataCorrection))]
	class ODSDataCorrectionTest : PGADataCorrectionlCoreTest<ODSDataCorrection>
	{
		protected override Type GetInterfaceType()
		{
			return typeof(ITSCAData);
		}

		protected override ODSDataCorrection[] SetupDataForParentFieldsAreSetupForTracking()
		{
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_ODSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine2.US_ODSDisclaimReason = PGADisclaimReasonList.Codes.A;
			return base.SetupDataForParentFieldsAreSetupForTracking();
		}

		protected override ODSDataCorrection GetNewPGA()
		{
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;
			return invoiceLine.ODSDataCorrection;
		}

		protected override ODSDataCorrection GetPGAInDiffFactory(BusinessObjectFactory factory, ODSDataCorrection pga)
		{
			var invoiceLine = factory.Load<JobComInvoiceLine>(pga.InvoiceLine.PK);
			return invoiceLine.ODSDataCorrection;
		}

		protected override bool IsTypeToIgnoreAccessCheck(BusinessObject bo)
		{
			return false;
		}

		protected override bool FieldsToExcludeFromTracking(string fieldName)
		{
			return base.FieldsToExcludeFromTracking(fieldName) ||
				   fieldName.EndsWith(".AddInfoJobComInvoiceLine.US_TSCAInd") ||
				   fieldName.EndsWith(".AddInfoJobComInvoiceLine.US_TSCADisclaimReason") ||
				   fieldName.EndsWith(".AddInfoJobComInvoiceLine.US_ODSTrackingStatus") ||
				   fieldName.EndsWith(".AddInfoJobComInvoiceLine.US_TSCATrackingStatus") ||
				   fieldName.EndsWith("AddInfoJobComInvoiceHeader.US_TSCASignDate");
		}

		protected override void SetPGAData(ODSDataCorrection pga)
		{
			pga.InvoiceLine.US_FDAContactName = "BOB";
		}

		protected override List<ZString> PGARelatedParentColumnUsedInPGABlocksCreator()
		{
			var result = base.PGARelatedParentColumnUsedInPGABlocksCreator();
			result.Add("Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_ODSInd");
			return result;
		}
	}
}
