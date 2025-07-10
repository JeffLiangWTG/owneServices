using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ACEPGATestForm<VNEUserControl>))]
	sealed class VNEUserControlTest : ZPGAFormBasherAbstractTest<VNEUserControl>
	{
		protected override CargoWise.EntityFramework.BusinessObject GetPGABusinessObject(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			var vehicle = invoiceLine.VehicleLines.AddNew();
			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			vehicle.US_VehicleModel = "2012";
			vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._04;
			vehicle.US_IndustryCode = IndustryCodesList.Codes.D;
			vehicle.US_ModelYear = "2012";
			return vehicle;
		}

		protected override string BindMember => "FilteredInvoiceLines.VehicleLines";
	}

	sealed class VNEGridPGADataCorrectionSupporterTest : ZGridPGADataCorrectionSupporterTest<VNEUserControl>
	{
		protected override IPGADataCorrectionCollection GetPGACollection(JobComInvoiceLine invoiceLine) => invoiceLine.VehicleLines;

		protected override ZGrid GetGrid(VNEUserControl control) => control.VNEGrid;

		protected override IPGADataCorrection AddNewItemToCollection(IPGADataCorrectionCollection collection) => ((VehicleCollection)collection).AddNew();
	}
}
