using System;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class OGADT02Test : OGABIRDUpdateTest
	{
		protected override IBIRDOGALineRecord[] GetPopulatedOGARecords()
		{
			OGADT02 dt02 = new OGADT02();
			dt02.MakeOfVehicle = "Hyundai";
			dt02.Model = "Getz";
			dt02.Year = 2009;
			dt02.VehicleIdentificationNumber = "VID123456";
			dt02.NHTSARegisteredImporterRINumber = "87YUIAHU";
			dt02.VehicleEligibilityNumber = "R12345";

			return new IBIRDOGALineRecord[] { dt02 };
		}

		protected override IOGALine CreateOGALine(JobComInvoiceLine invoiceLine) => invoiceLine.DOTs.AddNew();

		protected override void PrepareJob(JobComInvoiceLine invoiceLine, IOGALine ogaLine, IBIRDOGALineRecord ogaRecord)
		{
			base.PrepareJob(invoiceLine, ogaLine, ogaRecord);
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
		}

		protected override Type GetTypeOfMessageBlock() => typeof(OGADT02);
	}
}
