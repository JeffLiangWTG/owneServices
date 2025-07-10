using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class DrawbackSummaryMessageBuilderTest : TestCaseWithFactory
	{
		public void TestBuildDrawbackSummaryMessage()
		{
			Mock<IDrawbackSummary> mock = GetMock();
			IDrawbackSummary drawbackSummary = mock.Object;

			DrawbackSummaryMessageBuilder builder = new DrawbackSummaryMessageBuilder(drawbackSummary);
			builder.Generate();

			ZQuery query = new ZQuery();
			EDIMessage msg = Factory.LoadTop1<EDIMessage>(query);
			ZString expectedResult = @"B019900SV9JJ                                12             <<MSGNO PLACEHOLDER>>
D10 12365245211413901080324GFTR12365245                                  9900   
D1133-33333-333                                   080324  1CA                   
D120133-33333-333                                                               
D20000155555555558888888888                                                     
D2500019999999999                                                               
D300001752125475218888080324E000000000000000000000000                           
D400001CM5632560401000000000000000000000000000000000156KG                       
D41DESCRIPTION                                                                  
D500000112348765      08032480110011  000000000000000000000000000000000000      
D9000000000000000000000000000010001010000100000000000000000000000000010001      
Y  9900SV9JJ00010";
			AssertMultilineASCIIEquals("Message text", expectedResult, msg.EM_FormattedMessageText);
		}

		Mock<IDrawbackSummary> GetMock()
		{
			var mock = new Mock<IDrawbackSummary>();
			mock.Setup(m => m.Factory).Returns(Factory);
			mock.Setup(m => m.EntryFilerCode).Returns("SV9");
			mock.Setup(m => m.ProcessingOfficeCode).Returns("12");

			mock.Setup(m => m.DeleteCode).Returns(" ");
			mock.Setup(m => m.ClaimNumber).Returns("12365245211");
			mock.Setup(m => m.ClaimType).Returns(41);
			mock.Setup(m => m.ClaimPort).Returns("3901");
			mock.Setup(m => m.EstimatedClaimDate).Returns(new ZDate(2008, 03, 24));
			mock.Setup(m => m.ClaimantIdentification).Returns("GFTR12365245");
			mock.Setup(m => m.DrawbackTeam).Returns(" ");
			mock.Setup(m => m.BondType).Returns(" ");
			mock.Setup(m => m.SuretyCode).Returns(" ");
			mock.Setup(m => m.NAFTAClaimIndicator).Returns(" ");
			mock.Setup(m => m.GovernmentClaimIndicator).Returns(" ");
			mock.Setup(m => m.AcceleratedClaimIndicator).Returns(" ");
			mock.Setup(m => m.ExporterSummaryProcedureIndicator).Returns(" ");
			mock.Setup(m => m.WaiverOfPriorNoticeIndicator).Returns(" ");
			mock.Setup(m => m.PreInspectionIndicator).Returns(" ");
			mock.Setup(m => m.AgentBroker).Returns(" ");
			mock.Setup(m => m.BrokerReferenceNumber).Returns(" ");
			mock.Setup(m => m.LicensePort).Returns("9900");

			mock.Setup(m => m.FirstContractNumber).Returns("33-33333-333");
			mock.Setup(m => m.Description).Returns(" ");
			mock.Setup(m => m.EarliestExportDate).Returns(new ZDate(2008, 03, 24));
			mock.Setup(m => m.PetroleumClaimIndicator).Returns("1");
			mock.Setup(m => m.NAFTADrawbackCountryCode).Returns("CA");

			//D12
			List<IDrawbackContractNumber> contracts = new List<IDrawbackContractNumber>();
			var mockContract = new Mock<IDrawbackContractNumber>();
			mockContract.Setup(m => m.ContractNumberCode).Returns("33-33333-333");
			IDrawbackContractNumber contr = mockContract.Object;
			contracts.Add(contr);
			mock.Setup(m => m.ExtraContractNumbers).Returns(contracts);

			//D20
			List<IDrawbackTrailerTariff> tariffs = new List<IDrawbackTrailerTariff>();
			var mockTariff = new Mock<IDrawbackTrailerTariff>();
			mockTariff.Setup(m => m.FirstTariffNumber).Returns("5555555555");
			mockTariff.Setup(m => m.AdditionalTariffNumber).Returns("8888888888");
			mockTariff.Setup(m => m.AdditionalTariffNumber1).Returns(" ");
			mockTariff.Setup(m => m.AdditionalTariffNumber2).Returns(" ");
			mockTariff.Setup(m => m.AdditionalTariffNumber3).Returns(" ");
			IDrawbackTrailerTariff tar = mockTariff.Object;
			tariffs.Add(tar);
			mock.Setup(m => m.TrailerTariffs).Returns(tariffs);

			//D25
			List<IDrawbackTrailerScheduleBNumber> scheduleBNumbers = new List<IDrawbackTrailerScheduleBNumber>();
			var mockscheduleB = new Mock<IDrawbackTrailerScheduleBNumber>();
			mockscheduleB.Setup(m => m.FirstScheduleBNumber).Returns("9999999999");
			mockscheduleB.Setup(m => m.AdditionalScheduleBNumber).Returns(" ");
			mockscheduleB.Setup(m => m.AdditionalScheduleBNumber1).Returns(" ");
			mockscheduleB.Setup(m => m.AdditionalScheduleBNumber2).Returns(" ");
			mockscheduleB.Setup(m => m.AdditionalScheduleBNumber3).Returns(" ");
			IDrawbackTrailerScheduleBNumber sch = mockscheduleB.Object;
			scheduleBNumbers.Add(sch);
			mock.Setup(m => m.TrailerScheduleBNumbers).Returns(scheduleBNumbers);

			//D30
			List<IDrawbackImportClaim> importClaims = new List<IDrawbackImportClaim>();
			var mocksClaim = new Mock<IDrawbackImportClaim>();
			mocksClaim.Setup(m => m.DrawbackImportEntry).Returns("75212547521");
			mocksClaim.Setup(m => m.DrawbackImportEntryPort).Returns("8888");
			mocksClaim.Setup(m => m.DrawbackImportEntryDate).Returns(new ZDate(2008, 03, 24));
			mocksClaim.Setup(m => m.CMCDIndicator).Returns("E");
			mocksClaim.Setup(m => m.DrawbackClaimDuty).Returns(0m);
			mocksClaim.Setup(m => m.DrawbackClaimTax).Returns(0m);
			IDrawbackImportClaim claim = mocksClaim.Object;
			importClaims.Add(claim);
			mock.Setup(m => m.ImportClaims).Returns(importClaims);

			//D40
			List<IDrawbackManufactureClaim> manClaims = new List<IDrawbackManufactureClaim>();
			var mocksManufClaim = new Mock<IDrawbackManufactureClaim>();
			mocksManufClaim.Setup(m => m.CertificateOfManufactureNumber).Returns("CM563256");
			mocksManufClaim.Setup(m => m.CertificateOfManufacturePort).Returns("0401");
			mocksManufClaim.Setup(m => m.DrawbackClaimDuty).Returns(0m);
			mocksManufClaim.Setup(m => m.DrawbackClaimTax).Returns(0m);
			mocksManufClaim.Setup(m => m.DrawbackManufactureQuantity).Returns(156m);
			mocksManufClaim.Setup(m => m.DrawbackManufactureUnitOfMeasure).Returns("KG");

			//D41
			mocksManufClaim.Setup(m => m.DescriptionForBlock41).Returns("DESCRIPTION");

			IDrawbackManufactureClaim manClaim = mocksManufClaim.Object;
			manClaims.Add(manClaim);
			mock.Setup(m => m.ManufactureClaims).Returns(manClaims);

			//D50
			List<IDrawbackNAFTATariff> naftas = new List<IDrawbackNAFTATariff>();
			var mocksNAFTA = new Mock<IDrawbackNAFTATariff>();
			mocksNAFTA.Setup(m => m.NAFTACountryImportEntry).Returns("12348765");
			mocksNAFTA.Setup(m => m.NAFTACountryImportEntryDate).Returns(new ZDate(2008, 03, 24));
			mocksNAFTA.Setup(m => m.NAFTACountryTariffNumber).Returns("80110011");
			mocksNAFTA.Setup(m => m.NAFTACountryDutyRate).Returns(0m);
			mocksNAFTA.Setup(m => m.NAFTACountryImportDuty).Returns(0m);
			mocksNAFTA.Setup(m => m.EquivalentUSDollarAmountOfNAFTACountryDuty).Returns(0m);
			IDrawbackNAFTATariff naf = mocksNAFTA.Object;
			naftas.Add(naf);
			mock.Setup(m => m.NAFTATariffs).Returns(naftas);

			//D90
			mock.Setup(m => m.TotalClaimDuty).Returns(0m);
			mock.Setup(m => m.TotalClaimTax).Returns(0m);
			mock.Setup(m => m.TotalNAFTACountryImportDuty).Returns(0m);
			mock.Setup(m => m.TotalUSDollarEquivalentOfNAFTACountryDuty).Returns(0m);

			return mock;
		}
	}
}
