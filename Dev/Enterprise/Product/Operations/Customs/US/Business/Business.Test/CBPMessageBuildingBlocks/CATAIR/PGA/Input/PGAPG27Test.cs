using System;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class PGAPG27Test : OGABIRDUpdateTest
	{
		protected override Type GetTypeOfMessageBlock() => typeof(PGAPG27);

		protected override IBIRDOGALineRecord[] GetPopulatedOGARecords()
		{
			var pg27 = new PGAPG27();
			pg27.ContainerEquipmentID = "MAEUXXXX";
			pg27.ContainerEquipmentID1 = "MAEUTTTTTT";
			pg27.ContainerEquipmentID2 = "MAEUZZZ";

			return new IBIRDOGALineRecord[] { pg27 };
		}

		protected override IOGALine CreateOGALine(JobComInvoiceLine invoiceLine) => invoiceLine.LaceyActLines.AddNew();
	}
}
