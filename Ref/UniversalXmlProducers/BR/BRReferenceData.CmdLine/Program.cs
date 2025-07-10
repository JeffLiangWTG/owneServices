using System;
using System.Globalization;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	class Program
	{
		static int Main(string[] args)
		{
			ProduceXml(args);
			return (int)ProducerStatus.Success;
		}

#pragma warning disable CA1502
		static void ProduceXml(string[] args)
#pragma warning restore CA1502
		{
			if (args.Length != 0)
			{
				var functionToRun = args[0].ToUpper(CultureInfo.CurrentCulture);
				switch (functionToRun)
				{
					case Constants.ProgramFunctions.CustomsOfficeCode:
						new CustomsOfficeCodeListProgram().Run();
						break;
					case Constants.ProgramFunctions.CustomsTariffSubitemCode:
						new HSNTariffProgram().Run();
						break;
					case Constants.ProgramFunctions.CustomsTariffUnitOfMeasureCode:
						new HSNTariffUOMProgram().Run();
						break;
					case Constants.ProgramFunctions.TariffBRCharacteristicNcmCode:
						new TariffCharacteristicNCMProgram(true, args.Length == 2 ? args[1] == Constants.TariffCharacteristicNCM.EXP_ONLY : false).Run();
						break;
					case Constants.ProgramFunctions.TariffBRCharacteristicNcmTestCode:
						new TariffCharacteristicNCMProgram(false, args.Length == 2 ? args[1] == Constants.TariffCharacteristicNCM.EXP_ONLY : false).Run();
						break;
					case Constants.ProgramFunctions.ExchangeRateCode:
						new ExchangeRateProgram().Run();
						break;
					case Constants.ProgramFunctions.ExportProcedure:
						new ExportProcedureProgram().Run();
						break;
					case Constants.ProgramFunctions.ImportSiscomexProcedure:
						new ImportSiscomexProcedureProgram().Run();
						break;
					case Constants.ProgramFunctions.ImportSiscomexProcedureManual:
						new ImportSiscomexProcedureManualProgram(args[1]).Run();
						break;
					case Constants.ProgramFunctions.NomenclatureGroupCode:
						new NomenclatureGroupProgram().Run();
						break;
					case Constants.ProgramFunctions.CustomsEnclosureCode:
						new CustomsEnclosureCodeListProgram().Run();
						break;
					case Constants.ProgramFunctions.CustomsTariffWTOCode:
						new WTOIIITariffProgram().Run();
						break;
					case Constants.ProgramFunctions.CustomsTariffRateTIPI:
						new TariffRateIPIProgram().Run();
						break;
					case Constants.ProgramFunctions.CustomsLPCOCode:
						new LPCOProgram().Run();
						break;
					case Constants.ProgramFunctions.CustomsTariffGMCCode:
						new TariffGMCProgram().Run();
						break;
					case Constants.ProgramFunctions.CustomsTariffTec:
						new TariffTecProgram().Run();
						break;
					case Constants.ProgramFunctions.CustomsTariffAutoPartsList:
						new AutoPartsTariffProgram().Run();
						break;
					case Constants.ProgramFunctions.CustomsTariffRateCovidCode:
						new TariffRateCovidProgram().Run();
						break;
					case Constants.ProgramFunctions.CustomsTariffRateLetecCode:
						new TariffRateLetecProgram().Run();
						break;
					case Constants.ProgramFunctions.CustomsTariffIPITableCode:
						new TariffIPITableProgram().Run();
						break;
					case Constants.ProgramFunctions.CustomsTariffLebitCode:
						new LEBITTariffProgram().Run();
						break;
					case Constants.ProgramFunctions.CustomsConsentingBodyCode:
						new ConsentingBodyCodeListProgram().Run();
						break;
					case Constants.ProgramFunctions.CustomsTariffBitAndBkCode:
						new TariffBitAndBKDownloadProgram().Run();
						break;
					case Constants.ProgramFunctions.CustomsDutyGSTPCode:
						new GSTPTariffProgram().Run();
						break;
					case Constants.ProgramFunctions.WarehousingSectorsCode:
						new WarehousingSectorsCodeListProgram().Run();
						break;
					case Constants.ProgramFunctions.DutyLegalBasis:
						new DutyLegalBasisCodeListProgram().Run();
						break;
					case Constants.ProgramFunctions.CustomsTariffRate:
						new TariffRateProgram(args.Length == 3 ? args[1] : string.Empty, args.Length == 3 ? args[2] : string.Empty).Run();
						break;
					case Constants.ProgramFunctions.CustomsTariffDetaches:
						new TariffDetachesProgram().Run();
						break;
					case Constants.ProgramFunctions.CustomsReasonTemporaryAdmission:
						new ReasonTemporaryAdmissionCodeListProgram().Run();
						break;
					case Constants.ProgramFunctions.CustomsMethodOfPayment:
						new ExchangeHedgeMethodofPaymentCodeListProgram().Run();
						break;
					case Constants.ProgramFunctions.CustomsTariffVigentRate:
						new HSNTariffDutyRatePdfProgram().Run();
						break;
					case Constants.ProgramFunctions.CustomsTaxationRegimeDuty:
						new DutyTaxationRegimeCodeListProgram().Run();
						break;
					case Constants.ProgramFunctions.CustomsNaladiNcca:
						new NaladiNccaProgram().Run();
						break;
					case Constants.ProgramFunctions.CustomsNaladiSh:
						new NaladiShProgram().Run();
						break;
					case Constants.ProgramFunctions.PisCofinsLegalBasis:
						new PisCofinsLegalBasisCodeListProgram().Run();
						break;
					case Constants.ProgramFunctions.DutyLaiaAgreements:
						new AgreementsLAIACodeListProgram().Run();
						break;
					case Constants.ProgramFunctions.TariffBRCharacteristicNveCode:
						new TariffCharacteristicNVEProgram().Run();
						break;
					case Constants.ProgramFunctions.SACUTradeGroup:
						new SACUTradeGroupProgram().Run();
						break;
					case Constants.ProgramFunctions.PisCofinsReductionTariff:
						new PisCofinsReductionTariffProgram().Run();
						break;
					case Constants.ProgramFunctions.TariffAgreements:
						new TariffAgreementsCodeListProgram().Run();
						break;
					case Constants.ProgramFunctions.CustomsTariffExTariffIPI:
						new IPIExTariffrogram().Run();
						break;
					case Constants.ProgramFunctions.SpecialClearanceAttributesProcedure:
						new SpecialClearanceAttributesProcedureProgram().Run();
						break;
					case Constants.ProgramFunctions.TariffTributary:
						new TariffTributaryProgram().Run();
						break;
					case Constants.ProgramFunctions.TariffAttributes:
						new TariffProfileNCMProgram(true).Run();
						break;
					case Constants.ProgramFunctions.TariffAttributesTest:
						new TariffProfileNCMProgram(false).Run();
						break;
					case Constants.ProgramFunctions.DispatchInstructionDocument:
						new DispatchInstructionDocumentsProgram(true).Run();
						break;
					case Constants.ProgramFunctions.DispatchInstructionDocumentTest:
						new DispatchInstructionDocumentsProgram(false).Run();
						break;
					case Constants.ProgramFunctions.TariffRates:
						new TariffRatesProgram().Run();
						break;
					default:
						throw new ArgumentException($"Invalid argument entered: {functionToRun}");
				}
			}
			else
			{
				throw new ArgumentException("No arguments entered.");
			}
		}
	}
}
