using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(Pesticide))]
	class PesticidePGADataCorrectionTest : PGADataCorrectionlTest<Pesticide>
	{
		protected override Pesticide[] SetupDataForParentFieldsAreSetupForTracking()
		{
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine2.US_PSTDisclaimReason = PGADisclaimReasonList.Codes.A;
			return base.SetupDataForParentFieldsAreSetupForTracking();
		}

		protected override Type GetInterfaceType()
		{
			return typeof(IPSTData);
		}

		protected override bool IsTypeToIgnoreAccessCheck(BusinessObject bo)
		{
			return bo is Pesticide || bo is USPSTAddInfo;
		}

		protected override Pesticide GetNewPGA()
		{
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			var result = invoiceLine.PSTLines.AddNew();
			result.US_ProductType = PSTProductTypeList.Codes.PS1;
			return result;
		}

		protected override void SetPGAData(Pesticide pga)
		{
			pga.US_IntendedUseCode = PSTIntendedUseCodesList.Codes._130026;
		}

		protected override bool FieldsToExcludeFromTracking(string fieldName)
		{
			return base.FieldsToExcludeFromTracking(fieldName) ||
				fieldName.EndsWith("AddInfoJobDeclaration.US_EntryFilerCode") || // Entry Filer Code is only set once.
				fieldName.EndsWith("AddInfoJobComInvoiceHeader.US_PSTSignDate");
		}

		protected override List<ZString> PGARelatedParentColumnUsedInPGABlocksCreator()
		{
			var result = base.PGARelatedParentColumnUsedInPGABlocksCreator();
			result.Add("Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_PSTIndicator");
			return result;
		}
	}
}
