using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(NHTSAHeader))]
	class NHTSAHeaderPGADataCorrectionTest : PGADataCorrectionlTest<NHTSAHeader>
	{
		protected override NHTSAHeader[] SetupDataForParentFieldsAreSetupForTracking()
		{
			invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Declared;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_NHTSAIndicator = OGAIndicatorList.Codes.Disclaimed;
			return base.SetupDataForParentFieldsAreSetupForTracking();
		}

		protected override Type GetInterfaceType()
		{
			return typeof(INHTSAHeader);
		}

		protected override bool IsTypeToIgnoreAccessCheck(BusinessObject bo)
		{
			return bo is NHTSAHeader || bo is USNHTSAAddInfo;
		}

		protected override NHTSAHeader GetNewPGA()
		{
			invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Declared;
			var result = invoiceLine.NHTSALines.AddNew();
			result.US_NHTProgramCode = "MVS";
			return result;
		}

		protected override void SetPGAData(NHTSAHeader pga)
		{
			pga.US_NHTBoxNumber = "03";
		}

		protected override bool FieldsToExcludeFromTracking(string fieldName)
		{
			return base.FieldsToExcludeFromTracking(fieldName) ||
				fieldName.EndsWith("AddInfoJobDeclaration.US_EntryFilerCode")
				|| fieldName.EndsWith("USNHTSADocumentAddInfo.US_NHTDocumentType")
				|| fieldName.EndsWith("USNHTSADocumentAddInfo.US_NHTDocumentOwner") // Entry Filer Code is only set once.
				|| fieldName.EndsWith("AddInfoJobComInvoiceHeader.US_NHTSASignDate");
		}

		protected override List<ZString> PGARelatedParentColumnUsedInPGABlocksCreator()
		{
			var result = base.PGARelatedParentColumnUsedInPGABlocksCreator();
			result.Add("Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_NHTSAIndicator");
			return result;
		}
	}
}
