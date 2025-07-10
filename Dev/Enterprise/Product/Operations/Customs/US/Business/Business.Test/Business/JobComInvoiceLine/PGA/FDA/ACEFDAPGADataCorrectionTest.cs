using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ACEFDA))]
	class ACEFDAPGADataCorrectionTest : PGADataCorrectionlTest<ACEFDA>
	{
		protected override ACEFDA[] SetupDataForParentFieldsAreSetupForTracking()
		{
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine2.US_FDADisclaimReason = PGADisclaimReasonList.Codes.A;
			return base.SetupDataForParentFieldsAreSetupForTracking();
		}

		protected override Type GetInterfaceType()
		{
			return typeof(IFDAData);
		}

		protected override bool IsTypeToIgnoreAccessCheck(BusinessObject bo)
		{
			return bo is ACEFDA || bo is USACEFDAAddInfo;
		}

		protected override ACEFDA GetNewPGA()
		{
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			var result = invoiceLine.ACE_FDALines.AddNew();
			result.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			return result;
		}

		protected override void SetPGAData(ACEFDA pga)
		{
			pga.US_FDAForcePN = true;
		}

		protected override bool FieldsToExcludeFromTracking(string fieldName)
		{
			return base.FieldsToExcludeFromTracking(fieldName) ||
				fieldName.EndsWith("AddInfoJobDeclaration.US_EntryFilerCode") ||
				fieldName.EndsWith("JobDeclaration.JE_EntryAuthorisationDate") ||
				fieldName.EndsWith("AddInfoJobDeclaration.US_PreliminaryStatementPrintDate") ||
				fieldName.EndsWith("AddInfoJobDeclaration.US_ITDate") ||
				fieldName.EndsWith("Enterprise.Customs.US.Business.AddInfoCusEntryHeader.US_DutyCalcDate") ||
				fieldName.EndsWith("AddInfoJobDeclaration.US_ImmediateDelivery");
		}

		protected override List<ZString> PGARelatedParentColumnUsedInPGABlocksCreator()
		{
			var result = base.PGARelatedParentColumnUsedInPGABlocksCreator();
			result.Add("Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_FDAIndicator");
			result.Add("Enterprise.Customs.US.Business.AddInfoJobDeclaration.US_F_PNMode");
			return result;
		}
	}
}
