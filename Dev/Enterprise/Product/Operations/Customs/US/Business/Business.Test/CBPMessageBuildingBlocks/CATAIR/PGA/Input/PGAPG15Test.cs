using System;
using Enterprise.Customs.US.Business.BIRD.ACS;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class PGAPG15Test : BIRDPGAScientificRecordTest
	{
		protected override IBIRDPGAScientificRecord[] GetPopulatedRecords()
		{
			var pg15 = new PGAPG15();
			pg15.ScientificGenusName = "QUERCUS";
			pg15.ScientificSpeciesName = "RUBRA";

			return new IBIRDPGAScientificRecord[] { pg15 };
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

		protected override void PrepareData(JobDeclaration declaration, JobComInvoiceLine invoiceLine, ConstituentElement constituentElement, IBIRDPGAScientificRecord lineRecord)
		{
			base.PrepareData(declaration, invoiceLine, constituentElement, lineRecord);

			var pga = constituentElement.Parent as PGA;

			pga.PG04ConstituentElements.AddNew();
		}

		protected override Type GetTypeOfMessageBlock() => typeof(PGAPG15);
	}
}
