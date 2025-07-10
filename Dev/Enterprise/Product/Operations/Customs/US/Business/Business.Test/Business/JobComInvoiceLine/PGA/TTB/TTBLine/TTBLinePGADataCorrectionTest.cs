using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(TTBLine))]
	class TTBLinePGADataCorrectionTest : PGADataCorrectionlTest<TTBLine>
	{
		protected override TTBLine[] SetupDataForParentFieldsAreSetupForTracking()
		{
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_TTBInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine2.US_TTBDisclaimReason = PGADisclaimReasonList.Codes.A;
			return base.SetupDataForParentFieldsAreSetupForTracking();
		}

		protected override Type GetInterfaceType()
		{
			return typeof(ITTBLine);
		}

		protected override bool IsTypeToIgnoreAccessCheck(BusinessObject bo)
		{
			return bo is TTBLine || bo is USTTBLineAddInfo;
		}

		protected override TTBLine GetNewPGA()
		{
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			var result = invoiceLine.TTBLines.AddNew();
			result.US_ProgramCode = TTBProgramCodeList.Codes.Wine;
			return result;
		}

		protected override void SetPGAData(TTBLine pga)
		{
			pga.US_ProcessingCode = TTBTOBProcessingCodeList.Codes.T51;
		}

		protected override bool FieldsToExcludeFromTracking(string fieldName)
		{
			return base.FieldsToExcludeFromTracking(fieldName) ||
				fieldName.EndsWith("AddInfoJobDeclaration.US_EntryFilerCode"); // Entry Filer Code is only set once.
		}

		protected override List<ZString> PGARelatedParentColumnUsedInPGABlocksCreator()
		{
			var result = base.PGARelatedParentColumnUsedInPGABlocksCreator();
			result.Add("Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_TTBInd");
			return result;
		}
	}
}
