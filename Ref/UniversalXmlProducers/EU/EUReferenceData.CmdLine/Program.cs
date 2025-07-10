using System;
using System.Globalization;
using CargoWise.RefDbRepo.EUReferenceData.AdditionalInformationCode.Business;
using CargoWise.RefDbRepo.EUReferenceData.AdditionalInformationCodeSubset.Business;
using CargoWise.RefDbRepo.EUReferenceData.AESAdditionalInformation.Business;
using CargoWise.RefDbRepo.EUReferenceData.AESAdditionalReference.Business;
using CargoWise.RefDbRepo.EUReferenceData.AESNationality.Business;
using CargoWise.RefDbRepo.EUReferenceData.AESPreviousDocumentType.Business;
using CargoWise.RefDbRepo.EUReferenceData.AESTransportDocumentType.Business;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.Business.ICS2;
using CargoWise.RefDbRepo.EUReferenceData.CUSNumbers.Business;
using CargoWise.RefDbRepo.EUReferenceData.CustomsMeursing.Business;
using CargoWise.RefDbRepo.EUReferenceData.DocumentTypeCommon.Business;
using CargoWise.RefDbRepo.EUReferenceData.ICS2CountryCodeICS2MS.Business;
using CargoWise.RefDbRepo.EUReferenceData.ICS2FunctionalErrorCodes.Business;
using CargoWise.RefDbRepo.EUReferenceData.ICS2HRCMScreeningMethod.Business;
using CargoWise.RefDbRepo.EUReferenceData.KindOfPackages.Business;
using CargoWise.RefDbRepo.EUReferenceData.MethodOfPayment.Business;
using CargoWise.RefDbRepo.EUReferenceData.OfficeCodes.Business;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using CargoWise.RefDbRepo.EUReferenceData.StateSubset.Business;
using CargoWise.RefDbRepo.EUReferenceData.TypeOfMeansOfTransport.Business;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.EUReferenceData.CmdLine
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
		{
			if (args.Length != 0)
			{
				var httpClientHelper = new HttpClientHelper();
				var outputPath = ApplicationConfig.Instance.OutputDirectory;
				var functionToRun = args[0].ToUpper(CultureInfo.InvariantCulture);
				string errMsg = string.Empty;
				switch (functionToRun)
				{
					case Constants.Functions.OfficeCodes:
						errMsg = OfficeCodeXMLProducer.DownloadAndConvertToRefCusCodeListXML(httpClientHelper, outputPath);
						break;
					case Constants.Functions.CustomsMeursing:
						CustomsMeursingDataParser.DownloadFileAndGenerateXml(httpClientHelper);
						break;
					case Constants.Functions.CUSNumbers:
						errMsg = (new CUSNumberXMLProducer(httpClientHelper).DownloadAndConvertToCUSNumberXML(outputPath, ApplicationConfig.Instance.CUSNumberPagesPerBatch));
						break;
					case Constants.Functions.MethodOfPayment:
						errMsg = (new MethodOfPaymentXMLProducer().DownloadAndConvertToXML(httpClientHelper, outputPath, ApplicationConfig.Instance.TransportChargesMethodOfPaymentUrl));
						break;
					case Constants.Functions.DocumentTypeCommon:
						errMsg = (new DocumentTypeCommonXMLProducer().DownloadAndConvertToXML(httpClientHelper, outputPath, ApplicationConfig.Instance.DocumentTypeCommonDownloadUrl));
						break;
					case Constants.Functions.KindOfPackages:
						errMsg = (new KindOfPackagesXMLProducer().DownloadAndConvertToXML(httpClientHelper, outputPath, ApplicationConfig.Instance.KindOfPackagesDownloadUrl));
						break;
					case Constants.Functions.AdditionalSupplyChainActorRoleCode:
						errMsg = (new AdditionalSupplyChainActorRoleCodeXMLProducer().DownloadAndConvertToXML(httpClientHelper, outputPath, ApplicationConfig.Instance.AdditionalSupplyChainActorRoleCodeUrl));
						break;
					case Constants.Functions.AdditionalInformationCodeSubset:
						errMsg = (new AdditionalInformationCodeSubsetXMLProducer().DownloadAndConvertToXML(httpClientHelper, outputPath, ApplicationConfig.Instance.AdditionalInformationCodeSubsetUrl));
						break;
					case Constants.Functions.ICS2FunctionalErrorCodes:
						errMsg = (new ICS2FunctionalErrorCodesXMLProducer().DownloadAndConvertToXML(httpClientHelper, outputPath, ApplicationConfig.Instance.ICS2FunctionalErrorCodesUrl));
						break;
					case Constants.Functions.ICS2HRCMScreeningMethod:
						errMsg = (new ICS2HRCMScreeningMethodXMLProducer().DownloadAndConvertToXML(httpClientHelper, outputPath, ApplicationConfig.Instance.ICS2HRCMScreeningMethodUrl));
						break;
					case Constants.Functions.ICS2UnLocodeExtended:
						errMsg = new UnLocodeExtendedXMLProducer().DownloadAndConvertToXML(httpClientHelper, outputPath, ApplicationConfig.Instance.ICS2UnLocodeExtendedUrl);
						break;
					case Constants.Functions.CountryCodeICS2MS:
						errMsg = (new CountryCodeICS2MSXMLProducer().DownloadAndConvertToXML(httpClientHelper, outputPath, ApplicationConfig.Instance.ICS2CountryCodeICS2MSUrl));
						break;
					case Constants.Functions.AESAdditionalInformation:
						errMsg = (new AESAdditionalInformationXMLProducer().DownloadAndConvertToXML(httpClientHelper, outputPath, ApplicationConfig.Instance.AESAdditionalInformationUrl));
						break;
					case Constants.Functions.AESAdditionalReference:
						errMsg = (new AESAdditionalReferenceXMLProducer().DownloadAndConvertToXML(httpClientHelper, outputPath, ApplicationConfig.Instance.AESAdditionalReferenceUrl));
						break;
					case Constants.Functions.AESTransportDocumentType:
						errMsg = (new AESTransportDocumentTypeXMLProducer().DownloadAndConvertToXML(httpClientHelper, outputPath, ApplicationConfig.Instance.AESTransportDocumentTypeUrl));
						break;
					case Constants.Functions.AESPreviousDocumentType:
						errMsg = (new AESPreviousDocumentTypeXMLProducer().DownloadAndConvertToXML(httpClientHelper, outputPath, ApplicationConfig.Instance.AESPreviousDocumentTypeUrl));
						break;
					case Constants.Functions.AESNationality:
						errMsg = (new AESNationalityXMLProducer().DownloadAndConvertToXML(httpClientHelper, outputPath, ApplicationConfig.Instance.AESNationalityUrl));
						break;
					case Constants.Functions.EUCodeLists:
						NctsCodeListsProgram.Run(outputPath);
						errMsg = new NctsCountryCustomsSecurityAgreementAreaProducer().DownloadAndConvertToRefCusTradeGroupXML(httpClientHelper, outputPath);
						break;
					case Constants.Functions.TypeOfGoods:
						errMsg = new TypeOfGoodsXMLProducer().DownloadAndConvertToXML(httpClientHelper, outputPath, ApplicationConfig.Instance.TypeOfGoodsUrl);
						break;
					case Constants.Functions.TypeOfMeansOfTransport:
						errMsg = new TypeOfMeansOfTransportXMLProducer().DownloadAndConvertToXML(httpClientHelper, outputPath, ApplicationConfig.Instance.TypeOfMeansOfTransportUrl);
						break;
					case Constants.Functions.AuthorisationType:
						errMsg = new AuthorisationTypeProducer().DownloadAndConvertToXML(httpClientHelper, outputPath, ApplicationConfig.Instance.AuthorisationTypeUrl);
						break;
					case Constants.Functions.StateSubset:
						errMsg = new StateSubsetXMLProducer().DownloadAndConvertToXML(httpClientHelper, outputPath, ApplicationConfig.Instance.StateSubsetUrl);
						break;
					case Constants.Functions.CCICodes:
						CCICodeListsProgram.Run(outputPath);
						break;
					default:
						throw new ArgumentException($"Invalid argument entered: {functionToRun}");
				}
				PrintErrorMessage(errMsg);
			}
			else
			{
				throw new ArgumentException("No arguments entered.");
			}
		}
#pragma warning restore CA1502

		internal static void PrintErrorMessage(string errorString)
		{
			if (!string.IsNullOrEmpty(errorString))
			{
				Console.Error.WriteLine(errorString);
			}
		}
	}
}
