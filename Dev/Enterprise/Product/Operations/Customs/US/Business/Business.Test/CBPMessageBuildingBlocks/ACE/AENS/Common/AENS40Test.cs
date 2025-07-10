using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Testing
{
	sealed class AENS40Test : TestCaseWithFactory
	{
		public void TestENS40ChargesZeroFills()
		{
			var aens40 = new AENS40();
			aens40.LineItemIdentifier = "001";
			aens40.CountryOfOriginCode = "CH";
			aens40.CountryOfExportCode = "CH";
			aens40.DateOfExportation = new ZDate(2014, 6, 11);
			aens40.GrossShippingWeight = 100;
			aens40.RelatedPartyIndicator = "N";

			AssertEquals("40  001 CHCH061114        0000000000     0000000100    N                        ", aens40.Serialise());
			aens40.ChargesAmount = 0;
			AssertEquals("40  001 CHCH061114        0000000000     0000000100    N                        ", aens40.Serialise());
		}

		public void TestUpdateInvoiceLine()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var notifications = new NotificationBuffer();
			var aens40 = new AENS40()
			{
				ArticleSetIndicator = SecondarySpecProgIndicatorList.Codes.X,
				CountryOfOriginCode = "CA",
				CountryOfExportCode = "US",
				TradeAgreementSpecialProgramClaimCode = SpecialProgramList.Codes.MX,
				GrossShippingWeight = 1200000m,
				CategoryCodeforTextiles = "123"
			};

			((IACEBIRDLineRecord)aens40).Update(invoiceLine, notifications);
			AssertEquals(SecondarySpecProgIndicatorList.Codes.X, invoiceLine.US_SetInd);
			AssertEquals("CA", invoiceLine.US_UC_NKCountryOfOrigin);
			AssertEquals("US", invoiceLine.US_UC_NKCountryOfExport);
			AssertEquals(SpecialProgramList.Codes.MX, invoiceLine.US_SPI);
			AssertEquals(1200m, invoiceLine.JI_Weight);
			AssertEquals(Enterprise.Core.Constants.Weight.Tonnes, invoiceLine.JI_WeightUQ);
			AssertEquals("123", invoiceLine.US_TextileCategoryNo);
		}
	}
}
