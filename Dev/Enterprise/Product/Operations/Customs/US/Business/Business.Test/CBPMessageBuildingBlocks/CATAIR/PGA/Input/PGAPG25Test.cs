using System;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class PGAPG25Test : OGABIRDUpdateTest
	{
		protected override Type GetTypeOfMessageBlock() => typeof(PGAPG25);

		protected override IBIRDOGALineRecord[] GetPopulatedOGARecords()
		{
			var pg25 = new PGAPG25();
			pg25.PGALineValue = 1000m;
			return new IBIRDOGALineRecord[] { pg25 };
		}

		protected override IOGALine CreateOGALine(JobComInvoiceLine invoiceLine) => invoiceLine.LaceyActLines.AddNew();

		protected override string[] GetFieldNameToExcludeForTesting()
		{
			return new string[]
			{
				//There are not populated at all
				"StorageTemperatureQualifier",
				"DegreeType",
				"NegativeNumber",
				"ActualTemperature",
				"StorageTypeLocationOfTemperatureRecording",
				"LotNumber",
				"ProductionDateRangeOfTheProduct",
				"PGAUnitValue",
			};
		}
	}
}
