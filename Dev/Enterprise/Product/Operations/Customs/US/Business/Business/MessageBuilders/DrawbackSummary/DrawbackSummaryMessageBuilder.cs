using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class DrawbackSummaryMessageBuilder : IUSMessageBuilder
	{
		public DrawbackSummaryMessageBuilder(IDrawbackSummary drawbackData)
		{
			this.drawbackData = drawbackData;
			block = new ABIInputBlockControlGenerator(drawbackData.EntryFilerCode, drawbackData.LicensePort, drawbackData.ProcessingOfficeCode);
		}
		readonly ABIInputBlockControlGenerator block;
		readonly IDrawbackSummary drawbackData;

		ZInt sequenceNumberD12;
		ZInt sequenceNumberD20;
		ZInt sequenceNumberD25;
		ZInt sequenceNumberD30;
		ZInt sequenceNumberD40;
		ZInt sequenceNumberD50;
		ZInt countD50;

		#region IUSMessageBuilder Members

		public void Generate()
		{
			block.B.ApplicationIdentifier = ApplicationIdentifierCodeList.Codes.DrawbackSummary;
			GenerateBlocks();
			GenerateMessage();
		}

		public MQEDIMessage CreateMessage()
		{
			Generate();
			return message;
		}

		#endregion

		void GenerateBlocks()
		{
			block.MessageBlocks.Add(GenerateDRWD10());
			if (drawbackData.DeleteCode != "D")
			{
				block.MessageBlocks.Add(GenerateDRWD11());

				sequenceNumberD12 = ZInt.Zero;
				foreach (IDrawbackContractNumber contractNumber in drawbackData.ExtraContractNumbers)
				{
					sequenceNumberD12++;
					block.MessageBlocks.Add(GenerateDRWD12(contractNumber.ContractNumberCode, sequenceNumberD12));
				}

				sequenceNumberD20 = ZInt.Zero;
				foreach (IDrawbackTrailerTariff tariff in drawbackData.TrailerTariffs)
				{
					sequenceNumberD20++;
					block.MessageBlocks.Add(GenerateDRWD20(tariff, sequenceNumberD20));
				}

				sequenceNumberD25 = ZInt.Zero;
				foreach (IDrawbackTrailerScheduleBNumber schBNumber in drawbackData.TrailerScheduleBNumbers)
				{
					sequenceNumberD25++;
					block.MessageBlocks.Add(GenerateDRWD25(schBNumber, sequenceNumberD25));
				}

				countD50 = ZInt.Zero;
				sequenceNumberD30 = ZInt.Zero;
				foreach (IDrawbackImportClaim importClaim in drawbackData.ImportClaims)
				{
					sequenceNumberD30++;
					block.MessageBlocks.Add(GenerateDRWD30(importClaim, sequenceNumberD30));
				}

				sequenceNumberD40 = ZInt.Zero;
				foreach (IDrawbackManufactureClaim manClaim in drawbackData.ManufactureClaims)
				{
					sequenceNumberD40++;
					block.MessageBlocks.Add(GenerateDRWD40(manClaim, sequenceNumberD40));
					if (!manClaim.DescriptionForBlock41.IsEmpty)
					{
						block.MessageBlocks.Add(GenerateDRWD41(manClaim.DescriptionForBlock41));
					}
				}

				sequenceNumberD50 = ZInt.Zero;
				foreach (IDrawbackNAFTATariff nafta in drawbackData.NAFTATariffs)
				{
					sequenceNumberD50++;
					countD50++;
					block.MessageBlocks.Add(GenerateDRWD50(nafta, sequenceNumberD50));
				}

				block.MessageBlocks.Add(GenerateDRWD90());
			}
		}

		void GenerateMessage()
		{
			message = block.CreateMessage<MQEDIMessage>(drawbackData.Factory);
		}
		MQEDIMessage message;

		#region Generate Message Blocks

		DRWD10 GenerateDRWD10()
		{
			DRWD10 d10 = new DRWD10();
			d10.DeleteCode = drawbackData.DeleteCode;
			d10.ClaimNumber = drawbackData.ClaimNumber;
			d10.ClaimType = drawbackData.ClaimType;
			d10.ClaimPort = drawbackData.ClaimPort;
			d10.EstimatedClaimDate = drawbackData.EstimatedClaimDate;
			d10.ClaimantIdentification = drawbackData.ClaimantIdentification;
			d10.DrawbackTeam = drawbackData.DrawbackTeam;
			d10.BondType = drawbackData.BondType;
			d10.SuretyCode = drawbackData.SuretyCode;
			d10.NAFTAClaimIndicator = drawbackData.NAFTAClaimIndicator;
			d10.GovernmentClaimIndicator = drawbackData.GovernmentClaimIndicator;
			d10.AcceleratedClaimIndicator = drawbackData.AcceleratedClaimIndicator;
			d10.ExporterSummaryProcedureIndicator = drawbackData.ExporterSummaryProcedureIndicator;
			d10.WaiverOfPriorNoticeIndicator = drawbackData.WaiverOfPriorNoticeIndicator;
			d10.PreInspectionIndicator = drawbackData.PreInspectionIndicator;
			d10.BrokerReferenceNumber = drawbackData.BrokerReferenceNumber;
			d10.AgentBroker = drawbackData.AgentBroker;
			d10.LicensePort = drawbackData.LicensePort;
			return d10;
		}

		DRWD11 GenerateDRWD11()
		{
			DRWD11 d11 = new DRWD11();
			d11.ContractNumber = drawbackData.FirstContractNumber;
			d11.Description = drawbackData.Description;
			d11.EarliestExportDate = drawbackData.EarliestExportDate;
			d11.PetroleumClaimIndicator = drawbackData.PetroleumClaimIndicator;
			d11.NAFTADrawbackCountryCode = drawbackData.NAFTADrawbackCountryCode;
			return d11;
		}

		DRWD12 GenerateDRWD12(ZString contractNumber, ZInt sequenceNumber)
		{
			DRWD12 d12 = new DRWD12();
			d12.ContractTrailerNumber = sequenceNumber;
			d12.ContractNumber = contractNumber;
			return d12;
		}

		DRWD20 GenerateDRWD20(IDrawbackTrailerTariff tariff, ZInt sequenceNumber)
		{
			DRWD20 d20 = new DRWD20();
			d20.TrailerSequenceNumber = sequenceNumber;
			d20.FirstTariffNumber = tariff.FirstTariffNumber.Replace(".", "");
			d20.AdditionalTariffNumber = tariff.AdditionalTariffNumber.Replace(".", "");
			d20.AdditionalTariffNumber1 = tariff.AdditionalTariffNumber1.Replace(".", "");
			d20.AdditionalTariffNumber2 = tariff.AdditionalTariffNumber2.Replace(".", "");
			d20.AdditionalTariffNumber3 = tariff.AdditionalTariffNumber3.Replace(".", "");
			return d20;
		}

		DRWD25 GenerateDRWD25(IDrawbackTrailerScheduleBNumber scheduleBNumber, ZInt sequenceNumber)
		{
			DRWD25 d25 = new DRWD25();
			d25.TrailerSequenceNumber = sequenceNumber;
			d25.FirstScheduleBNumber = scheduleBNumber.FirstScheduleBNumber.Replace(".", "");
			d25.AdditionalScheduleBNumber = scheduleBNumber.AdditionalScheduleBNumber.Replace(".", "");
			d25.AdditionalScheduleBNumber1 = scheduleBNumber.AdditionalScheduleBNumber1.Replace(".", "");
			d25.AdditionalScheduleBNumber2 = scheduleBNumber.AdditionalScheduleBNumber2.Replace(".", "");
			d25.AdditionalScheduleBNumber3 = scheduleBNumber.AdditionalScheduleBNumber3.Replace(".", "");
			return d25;
		}

		DRWD30 GenerateDRWD30(IDrawbackImportClaim importClaim, ZInt sequenceNumber)
		{
			DRWD30 d30 = new DRWD30();
			d30.TrailerNumber = sequenceNumber;
			d30.ImportEntry = importClaim.DrawbackImportEntry;
			d30.EntryPort = importClaim.DrawbackImportEntryPort;
			d30.EntryDate = importClaim.DrawbackImportEntryDate;
			d30.CMCDIndicator = importClaim.CMCDIndicator;
			d30.ClaimDuty = importClaim.DrawbackClaimDuty;
			d30.ClaimTax = importClaim.DrawbackClaimTax;
			return d30;
		}

		DRWD40 GenerateDRWD40(IDrawbackManufactureClaim manClaim, ZInt sequenceNumber)
		{
			DRWD40 d40 = new DRWD40();
			d40.TrailerNumber = sequenceNumber;
			d40.CertificateOfManufactureNumber = manClaim.CertificateOfManufactureNumber;
			d40.CertificateOfManufacturePort = manClaim.CertificateOfManufacturePort;
			d40.ClaimDuty = manClaim.DrawbackClaimDuty;
			d40.ClaimTax = manClaim.DrawbackClaimTax;
			d40.Quantity = manClaim.DrawbackManufactureQuantity;
			d40.UnitOfMeasure = manClaim.DrawbackManufactureUnitOfMeasure;
			return d40;
		}

		DRWD41 GenerateDRWD41(ZString desciption)
		{
			DRWD41 d41 = new DRWD41();
			d41.Description = desciption;
			return d41;
		}

		DRWD50 GenerateDRWD50(IDrawbackNAFTATariff nafta, ZInt sequenceNumber)
		{
			DRWD50 d50 = new DRWD50();
			d50.NAFTACountryImportEntryTariffCounter = sequenceNumber;
			d50.NAFTACountryImportEntry = nafta.NAFTACountryImportEntry;
			d50.NAFTACountryImportEntryDate = nafta.NAFTACountryImportEntryDate;
			d50.NAFTACountryTariffNumber = nafta.NAFTACountryTariffNumber;
			d50.NAFTACountryDutyRate = nafta.NAFTACountryDutyRate;
			d50.NAFTACountryImportDuty = nafta.NAFTACountryImportDuty;
			d50.EquivalentUSDollarAmountOfNAFTACountryDuty = nafta.EquivalentUSDollarAmountOfNAFTACountryDuty;
			return d50;
		}

		DRWD90 GenerateDRWD90()
		{
			DRWD90 d90 = new DRWD90();
			d90.TotalClaimDuty = drawbackData.TotalClaimDuty;
			d90.TotalClaimTax = drawbackData.TotalClaimTax;
			d90.ImportTrailerCount = sequenceNumberD30;
			d90.CertificateOfManufactureCount = sequenceNumberD40;
			d90.ContractCount = sequenceNumberD12;
			d90.NAFTACountryImportEntryTariffCount = countD50;
			d90.TotalNAFTACountryImportDuty = drawbackData.TotalNAFTACountryImportDuty;
			d90.TotalUSDollarEquivalentOfNAFTACountryDuty = drawbackData.TotalUSDollarEquivalentOfNAFTACountryDuty;
			d90.ImportTariffNumberRecordTrailerCount = sequenceNumberD20;
			d90.ScheduleBNumberRecordTrailerCount = sequenceNumberD25;
			return d90;
		}

		#endregion

	}
}
