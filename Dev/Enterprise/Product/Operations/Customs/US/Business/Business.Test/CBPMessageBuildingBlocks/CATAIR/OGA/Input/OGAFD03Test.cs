using System;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class OGAFD03Test : OGABIRDUpdateTest
	{
		// WI00034410
		public void TestFDAValueByFDALineShouldAlwaysBeZeroFilled()
		{
			var ogafd03 = new OGAFD03();
			ogafd03.FDAValueByFDALine = 0;
			AssertEquals("It should be zero filled", "FD030000000000                                                                  ", ogafd03.Serialise());
		}

		protected override IBIRDOGALineRecord[] GetPopulatedOGARecords()
		{
			var fd03 = new OGAFD03();
			fd03.FDAValueByFDALine = 10009m;
			fd03.FDAConsigneeFDAEstablishmentIndicatorFEI = "890344980";
			fd03.TradeOrBrandName = "Audi A4";
			fd03.ContainerDimension1 = "0214";
			fd03.ContainerDimensions2 = "3008";
			fd03.ContainerDimensions3 = "4412";

			return new IBIRDOGALineRecord[] { fd03 };
		}

		protected override IOGALine CreateOGALine(JobComInvoiceLine invoiceLine) => invoiceLine.FDAs.AddNew();

		protected override void PrepareJob(JobComInvoiceLine invoiceLine, IOGALine ogaLine, IBIRDOGALineRecord ogaRecord)
		{
			base.PrepareJob(invoiceLine, ogaLine, ogaRecord);
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;

			var fdaConsignee = Factory.NewWithValidTestData<OrgHeader>();
			fdaConsignee.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "890344980");

			((FDA)ogaLine).US_DimUQ = FDAMeasurementUnitList.Codes.InchesWithOneTenthDecimals;

			Factory.Save();
		}

		protected override Type GetTypeOfMessageBlock() => typeof(OGAFD03);
	}
}
