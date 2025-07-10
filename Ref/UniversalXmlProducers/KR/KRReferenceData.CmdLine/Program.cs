using System;
using System.Globalization;
using CargoWise.RefDbRepo.XmlProducer.Common;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using NPOI.SS.Formula.Functions;
using System.IO;

namespace CargoWise.RefDbRepo.KRReferenceData.CmdLine
{
	class Program
	{
		static int Main(string[] args)
		{
			return (int)ProduceXml(args);
		}

#pragma warning disable CA1502
		static ProducerStatus ProduceXml(string[] args)
		{
#pragma warning disable CA1031 // Do not catch general exception types
			var result = ProducerStatus.Success;
			try
			{
				if (args.Length != 0)
				{
					var functionToRun = args[0].ToUpper(CultureInfo.CurrentCulture);
					switch (functionToRun)
					{
						case Constants.ProgramFunctions.ExchangeRates:
							ExchangeRatesProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.Tariffs:
							TariffsProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.ExportNonGAReasonTypes:
							ExportNonGAReasonProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.ImportNonGAReasonTypes:
							ImportNonGAReasonProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.Preferences:
							PreferenceProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.PreferenceRefCusMap:
							PreferenceRefCusMapProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.ExportFTAType:
							ExportFTATypeProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.OGAImport:
							OGAProgram.Import.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.OGAExport:
							OGAProgram.Export.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.TradeGroup:
							TradeGroupProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.WCONomenclatures:
							WCONomenclaturesProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.KRNomenclatures:
							KRNomenclaturesProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.SteelNomenclatures:
							SteelNomenclaturesProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.SimpleDrawback:
							SimpleDrawbackProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.CustomsOffice:
							CustomsOfficeProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.CustomsDepartment:
							CustomsDepartmentProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.ForwarderIDs:
							ForwarderIDsProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.OtherGovernment:
							OtherGovernmentProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.ExpressDeliveryServiceID:
							ExpressDeliveryServiceIDProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.BrandCodes:
							BrandCodesProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.DutyRates:
							DutyRatesProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.DutyReductionExemption:
							DutyReductionExemptionProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.DomesticTaxExemption:
							DomesticTaxExemptionProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.DomesticTaxRates:
							DomesticTaxRatesProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.SpecialUseCodeDutyRates:
							SpecialUseCodeDutyRatesProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.InstalmentCodes:
							InstalmentCodesProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.AdditionalPaymentReasons:
							AdditionalPaymentReasonsProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.TaxOffice:
							TaxOfficeProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.OGARegulationCategory:
							OGARegulationCategoryProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.HSExtensionCodesMain:
							HSExtensionCodesMainProgram.Run(ApplicationConfig.OutputDirectory);
							break;
						case Constants.ProgramFunctions.HSExtensionCodesSub:
							HSExtensionCodesSubProgram.Run(ApplicationConfig.OutputDirectory);
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
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex);
				result = ProducerStatus.Failure;
			}
#pragma warning restore CA1031 // Do not catch general exception types

			return result;
		}
#pragma warning restore CA1502
	}
}
