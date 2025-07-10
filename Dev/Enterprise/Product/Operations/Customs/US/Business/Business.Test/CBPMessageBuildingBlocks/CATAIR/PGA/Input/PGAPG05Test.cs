using System;
using Enterprise.Customs.US.Business.BIRD.ACS;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class PGAPG05Test : BIRDPGAScientificRecordTest
	{
		protected override IBIRDPGAScientificRecord[] GetPopulatedRecords()
		{
			PGAPG05 pg05 = new PGAPG05();
			pg05.ScientificGenusName = "QUERCUS";
			pg05.ScientificSpeciesName = "RUBRA";

			return new IBIRDPGAScientificRecord[] { pg05 };
		}

		protected override string[] GetFieldNameToExcludeForTesting()
		{
			return new string[]
			{
				//These are not populated at all in message builders
				"ScientificSubSpeciesName",
				"ScientificSpeciesCode",
				"FWSDescriptionCode",
			};
		}

		protected override Type GetTypeOfMessageBlock() => typeof(PGAPG05);
	}
}
