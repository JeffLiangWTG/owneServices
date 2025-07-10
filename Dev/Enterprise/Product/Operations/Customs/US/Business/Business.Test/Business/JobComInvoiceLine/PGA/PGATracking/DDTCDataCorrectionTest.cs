using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DDTCDataCorrection))]
	class DDTCDataCorrectionTest : PGADataCorrectionlCoreTest<DDTCDataCorrection>
	{
		protected override Type GetInterfaceType()
		{
			return typeof(IDDTCData);
		}

		protected override DDTCDataCorrection GetNewPGA()
		{
			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			return invoiceLine.DDTCDataCorrection;
		}

		protected override DDTCDataCorrection GetPGAInDiffFactory(BusinessObjectFactory factory, DDTCDataCorrection pga)
		{
			var invoiceLine = factory.Load<JobComInvoiceLine>(pga.InvoiceLine.PK);
			return invoiceLine.DDTCDataCorrection;
		}

		protected override bool IsTypeToIgnoreAccessCheck(BusinessObject bo)
		{
			return false;
		}

		protected override void SetPGAData(DDTCDataCorrection pga)
		{
			pga.InvoiceLine.US_DDTCLicenseNo = "1";
		}

		protected override bool FieldsToExcludeFromTracking(string fieldName)
		{
			return base.FieldsToExcludeFromTracking(fieldName)
				|| fieldName.EndsWith("AddInfoJobComInvoiceLine.US_DDTCTrackingStatus");
		}

		protected override List<ZString> PGARelatedParentColumnUsedInPGABlocksCreator()
		{
			var result = base.PGARelatedParentColumnUsedInPGABlocksCreator();
			result.Add("Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_DDTCInd");
			return result;
		}
	}
}
