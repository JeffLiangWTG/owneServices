using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(NMFSLine))]
	class NMFSLinePGADataCorrectionTest : PGADataCorrectionlTest<NMFSLine>
	{
		protected override NMFSLine[] SetupDataForParentFieldsAreSetupForTracking()
		{
			invoiceLine.US_NMFSAMRInd = OGAIndicatorList.Codes.Declared;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_NMFSAMRInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine2.US_NMFSAMRDisclaimReason = PGADisclaimReasonList.Codes.A;
			return base.SetupDataForParentFieldsAreSetupForTracking();
		}

		protected override Type GetInterfaceType()
		{
			return typeof(INMFSLine);
		}

		protected override bool IsTypeToIgnoreAccessCheck(BusinessObject bo)
		{
			return bo is NMFSLine || bo is USNMFSLineAddInfo;
		}

		protected override NMFSLine GetNewPGA()
		{
			invoiceLine.US_NMFSAMRInd = OGAIndicatorList.Codes.Declared;
			var result = invoiceLine.NMFSLines.AddNew();
			result.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			return result;
		}

		protected override void SetPGAData(NMFSLine pga)
		{
			pga.US_AMLRPermitNumber = "AMR1";
		}

		protected override bool FieldsToExcludeFromTracking(string fieldName)
		{
			return base.FieldsToExcludeFromTracking(fieldName) ||
				fieldName.EndsWith("AddInfoJobDeclaration.US_EntryFilerCode"); // Entry Filer Code is only set once.
		}

		protected override List<ZString> PGARelatedParentColumnUsedInPGABlocksCreator()
		{
			var result = base.PGARelatedParentColumnUsedInPGABlocksCreator();
			result.Add("Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_NMFS370Ind");
			result.Add("Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_NMFSAMRInd");
			result.Add("Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_NMFSHMSInd");
			result.Add("Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_NMFSSIMPInd");
			result.Add("Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_NMFS370DisclaimReason");
			result.Add("Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_NMFSAMRDisclaimReason");
			result.Add("Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_NMFSHMSDisclaimReason");
			return result;
		}
	}
}
