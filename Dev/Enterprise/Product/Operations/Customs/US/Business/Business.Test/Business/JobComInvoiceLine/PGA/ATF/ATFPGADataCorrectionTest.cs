using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ATF))]
	class ATFPGADataCorrectionTest : PGADataCorrectionlTest<ATF>
	{
		protected override ATF[] SetupDataForParentFieldsAreSetupForTracking()
		{
			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_ATFInd = OGAIndicatorList.Codes.Disclaimed;
			return base.SetupDataForParentFieldsAreSetupForTracking();
		}

		protected override Type GetInterfaceType()
		{
			return typeof(IATFData);
		}

		protected override bool IsTypeToIgnoreAccessCheck(BusinessObject bo)
		{
			return bo is ATF || bo is ATFAddInfo;
		}

		protected override ATF GetNewPGA()
		{
			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;
			var result = invoiceLine.ATFLines.AddNew();
			result.US_CategoryCode = ATFCategoryCodeList.Codes.ESP;
			result.US_AECANumber = "1111";
			return result;
		}

		protected override void SetPGAData(ATF pga)
		{
			pga.US_CategoryCode = ATFCategoryCodeList.Codes.AMP;
		}

		protected override bool FieldsToExcludeFromTracking(string fieldName)
		{
			return base.FieldsToExcludeFromTracking(fieldName) ||
				fieldName.EndsWith("AddInfoJobDeclaration.US_EntryFilerCode"); // Entry Filer Code is only set once.
		}

		protected override List<ZString> PGARelatedParentColumnUsedInPGABlocksCreator()
		{
			var result = base.PGARelatedParentColumnUsedInPGABlocksCreator();
			result.Add("Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_ATFInd");
			return result;
		}
	}
}
