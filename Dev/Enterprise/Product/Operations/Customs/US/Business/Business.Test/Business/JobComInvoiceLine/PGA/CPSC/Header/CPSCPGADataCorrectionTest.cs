using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CPSCHeader))]
	internal class CPSCPGADataCorrectionTest : PGADataCorrectionlTest<CPSCHeader>
	{
		protected override void SetPGAData(CPSCHeader pga)
		{
			invoiceLine.US_CPSCInd = OGAIndicatorList.Codes.Declared;
			pga.US_ProcessingCode = CPSCProcessingCodeList.Codes.FCP;
			pga.US_ModelColor = "RAD";
		}

		protected override Type GetInterfaceType()
		{
			return typeof(ICPSCHeader);
		}

		protected override CPSCHeader[] SetupDataForParentFieldsAreSetupForTracking()
		{
			invoiceLine.US_CPSCInd = OGAIndicatorList.Codes.Declared;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_CPSCInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine2.US_CPSCDisclaimReason = PGADisclaimReasonList.Codes.A;
			return base.SetupDataForParentFieldsAreSetupForTracking();
		}

		protected override bool IsTypeToIgnoreAccessCheck(BusinessObject bo)
		{
			return bo is CPSCHeader || bo is CPSCHeaderAddInfo;
		}

		protected override CPSCHeader GetNewPGA()
		{
			invoiceLine.US_CPSCInd = OGAIndicatorList.Codes.Declared;
			var result = invoiceLine.CPSCHeaders.AddNew();
			result.US_ModelColor = "RAD";
			return result;
		}

		protected override bool FieldsToExcludeFromTracking(string fieldName)
		{
			return base.FieldsToExcludeFromTracking(fieldName) ||
				fieldName.EndsWith("AddInfoJobDeclaration.US_EntryFilerCode"); // Entry Filer Code is only set once.
		}

		protected override List<ZString> PGARelatedParentColumnUsedInPGABlocksCreator()
		{
			var result = base.PGARelatedParentColumnUsedInPGABlocksCreator();
			result.Add("Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_CPSCInd");
			return result;
		}
	}
}
