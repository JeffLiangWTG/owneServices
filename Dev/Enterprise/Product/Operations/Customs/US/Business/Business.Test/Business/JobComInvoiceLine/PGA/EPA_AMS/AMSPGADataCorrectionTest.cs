using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AMS))]
	class AMSPGADataCorrectionTest : PGADataCorrectionlTest<AMS>
	{
		protected override AMS[] SetupDataForParentFieldsAreSetupForTracking()
		{
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_NOPInd = OGAIndicatorList.Codes.Declared;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_AMSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine2.US_AMSDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine2.US_NOPInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine2.US_NOPDisclaimReason = PGADisclaimReasonList.Codes.A;
			containerHasBeenAccessed = false;
			return base.SetupDataForParentFieldsAreSetupForTracking();
		}

		protected override Type GetInterfaceType()
		{
			if (!containerHasBeenAccessed)
			{
				containerHasBeenAccessed = !container.CO_ContainerNumber.IsEmpty;
			}
			return typeof(IAMSData);
		}

		bool containerHasBeenAccessed;

		protected override bool IsTypeToIgnoreAccessCheck(BusinessObject bo)
		{
			return bo is AMS || bo is AMSAddInfo;
		}

		protected override AMS GetNewPGA()
		{
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var result = invoiceLine.AMSLines.AddNew();
			result.US_Program = AMSProgramList.Codes.MO1;
			return result;
		}

		protected override void SetPGAData(AMS pga)
		{
			pga.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._025000;
		}

		protected override bool FieldsToExcludeFromTracking(string fieldName)
		{
			return base.FieldsToExcludeFromTracking(fieldName) ||
				fieldName.EndsWith("AddInfoJobDeclaration.US_EntryFilerCode"); // Entry Filer Code is only set once.
		}

		protected override AMS SetupDataForMessageCreationAndProcessingTesting()
		{
			var result = base.SetupDataForMessageCreationAndProcessingTesting();
			var line = result.AMSLines.AddNew();
			line.US_ProductNumber = "1";
			return result;
		}

		protected override List<ZString> PGARelatedParentColumnUsedInPGABlocksCreator()
		{
			var result = base.PGARelatedParentColumnUsedInPGABlocksCreator();
			result.Add("Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_AMSInd");
			result.Add("Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_NOPInd");
			return result;
		}
	}
}
