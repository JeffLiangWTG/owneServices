using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USInvoiceLineFSISLine))]
	class USInvoiceLineFSISLinePGADataCorrectionTest : PGADataCorrectionlTest<USInvoiceLineFSISLine>
	{
		protected override USInvoiceLineFSISLine[] SetupDataForParentFieldsAreSetupForTracking()
		{
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_FSISInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine2.US_FSISDisclaimReason = PGADisclaimReasonList.Codes.A;
			return base.SetupDataForParentFieldsAreSetupForTracking();
		}

		protected override Type GetInterfaceType()
		{
			return typeof(IFSISLine);
		}

		protected override bool IsTypeToIgnoreAccessCheck(BusinessObject bo)
		{
			return bo is USInvoiceLineFSISLine || bo is USFSISLineAddInfo;
		}

		protected override USInvoiceLineFSISLine GetNewPGA()
		{
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
			var result = invoiceLine.FSISLines.AddNew();
			result.US_HealthCertificateNumber = "ABC";
			return result;
		}

		protected override void SetPGAData(USInvoiceLineFSISLine pga)
		{
			pga.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.Australia;
		}

		protected override bool FieldsToExcludeFromTracking(string fieldName)
		{
			return base.FieldsToExcludeFromTracking(fieldName) ||
				fieldName.EndsWith("AddInfoJobDeclaration.US_EntryFilerCode") || // Entry Filer Code is only set once.
				fieldName.EndsWith("AddInfoJobComInvoiceHeader.US_FSISSignDate");
		}

		protected override List<ZString> PGARelatedParentColumnUsedInPGABlocksCreator()
		{
			var result = base.PGARelatedParentColumnUsedInPGABlocksCreator();
			result.Add("Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_FSISInd");
			return result;
		}
	}
}
