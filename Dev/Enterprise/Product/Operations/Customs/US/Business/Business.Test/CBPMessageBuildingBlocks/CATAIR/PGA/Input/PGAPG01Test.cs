using System;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class PGAPG01Test : OGABIRDUpdateTest
	{
		public void TestIBIRDOGALineIDRecord()
		{
			var pg01 = new PGAPG01();

			AssertEquals(OGAType.PGA, ((IBIRDOGALineIDRecord)pg01).OGAType);
		}

		protected override Type GetTypeOfMessageBlock() => typeof(PGAPG01);

		protected override IBIRDOGALineRecord[] GetPopulatedOGARecords()
		{
			var pg01 = new PGAPG01();
			pg01.PGALineItemNumber = 1;
			pg01.AgencyQualifier1 = "AP";
			return new IBIRDOGALineRecord[] { pg01 };
		}

		protected override IOGALine CreateOGALine(JobComInvoiceLine invoiceLine) => invoiceLine.LaceyActLines.AddNew();

		protected override string[] GetFieldNameToExcludeForTesting()
		{
			return new string[]
			{
				//not populated at all
				"AgencyQualifier2",
				"AgencyQualifier3",
				"AgencyQualifier4",
				"AgencyQualifier5",
				"AgencyQualifier6",
				"ProductCode",
				"ChemicalAbstractsServiceCASNumber",
				"IntendedUseCode",
				"IntendedUseDescription"
			};
		}
	}
}
