using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(Vehicle))]
	class VehiclePGADataCorrectionTest : PGADataCorrectionlTest<Vehicle>
	{
		protected override Vehicle[] SetupDataForParentFieldsAreSetupForTracking()
		{
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_VNEInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine2.US_VNEDisclaimReason = PGADisclaimReasonList.Codes.A;
			return base.SetupDataForParentFieldsAreSetupForTracking();
		}

		protected override Type GetInterfaceType()
		{
			return typeof(IVNEData);
		}

		protected override bool IsTypeToIgnoreAccessCheck(BusinessObject bo)
		{
			return bo is Vehicle || bo is USVehicleAddInfo;
		}

		protected override Vehicle GetNewPGA()
		{
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			var result = invoiceLine.VehicleLines.AddNew();
			result.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			return result;
		}

		protected override void SetPGAData(Vehicle pga)
		{
			pga.US_ImportCode = ImportCodesForm3520_21List.Codes._04;
		}

		protected override bool FieldsToExcludeFromTracking(string fieldName)
		{
			return base.FieldsToExcludeFromTracking(fieldName) ||
				fieldName.EndsWith("AddInfoJobDeclaration.US_EntryFilerCode") || // Entry Filer Code is only set once.
				fieldName.EndsWith("AddInfoJobComInvoiceHeader.US_VNESignDate");
		}

		protected override List<ZString> PGARelatedParentColumnUsedInPGABlocksCreator()
		{
			var result = base.PGARelatedParentColumnUsedInPGABlocksCreator();
			result.Add("Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_VNEInd");
			return result;
		}
	}
}
