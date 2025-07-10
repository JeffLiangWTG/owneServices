using System;
using CargoWise.RefDbRepo.JPReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.JPReferenceData.CmdLine
{
	public static class NaccsProgram
	{
		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.JPNACCSTariff)]
		public static class JPNACCSImportTariffProgram
		{
			public static void Run()
			{
				TariffProcessor.WriteXml(true);
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.JPNACCSExportTariff)]
		public static class JPNACCSExportTariffProgram
		{
			public static void Run()
			{
				TariffProcessor.WriteXml(false);
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.DutyExemptionCode)]
		public static class DutyExemptionCodeProgram
		{
			public static void Run()
			{
				new DutyExemptionCodeXmlWriter().WriteXml(new HttpClientHelper());
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.BeforePermitApplicationReasonCode)]
		public static class BeforePermitApplicationReasonCodeProgram
		{
			public static void Run()
			{
				new BeforePermitApplicationReasonCodeXmlWriter().WriteXml(new HttpClientHelper());
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.ExportApprovalCertificateType)]
		public static class ExportApprovalCertificateTypeProgram
		{
			public static void Run()
			{
				new ExportApprovalCertificateTypeXmlWriter().WriteXml(new HttpClientHelper());
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.ConsumptionTaxExemptionReductionCode)]
		public static class ConsumptionTaxExemptionReductionCodeProgram
		{
			public static void Run()
			{
				new ConsumptionTaxExemptionReductionCodeXmlWriter().WriteXml(new HttpClientHelper());
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.ImportTradeControlOrdinanceAppendix)]
		public static class ImportTradeControlOrdinanceAppendixProgram
		{
			public static void Run()
			{
				new ImportTradeControlOrdinanceAppendixXmlWriter().WriteXml(new HttpClientHelper());
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.OtherLawCode)]
		public static class OtherLawCodeProgram
		{
			public static void Run()
			{
				new OtherLawCodeXmlWriter().WriteXml(new HttpClientHelper());
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.CustomsOffices)]
		public static class CustomsOfficesProgram
		{
			public static void Run()
			{
				var errors = CustomsOfficesXmlWriter.WriteXml(new HttpClientHelper());
				ErrorWriter.WriteError(errors);
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.CustomsOfficeDepartments)]
		public static class CustomsOfficeDepartmentsProgram
		{
			public static void Run()
			{
				new CustomsOfficeDepartmentsXMLWriter().WriteXml(new HttpClientHelper());
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.CertificateOfOriginTypeCode)]
		public static class CertificateOfOriginTypeCodeProgram
		{
			public static void Run()
			{
				new CertificateOfOriginTypeCodeXMLWriter().WriteXml(new HttpClientHelper());
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.ConsumptionTaxExemptionCodeExport)]
		public static class ConsumptionTaxExemptionCodeExportProgram
		{
			public static void Run()
			{
				new ConsumptionTaxExemptionCodeExportXMLWriter().WriteXml(new HttpClientHelper());
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.BondedAreaCode)]
		public static class BondedAreaCodeProgram
		{
			public static void Run()
			{
				new BondedAreaCodeXmlWriter().WriteXml(new HttpClientHelper());
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.UnitOfMeasurement)]
		public static class UnitOfMeasurementProgram
		{
			public static void Run()
			{
				new UnitOfMeasurementNaccsXmlWriter().WriteXml(new HttpClientHelper());
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.DutyExemptionRefundCode)]
		public static class DutyExemptionRefundCodeProgram
		{
			public static void Run()
			{
				new DutyExemptionRefundCodeXmlWriter().WriteXml(new HttpClientHelper());
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.ImportApprovalCertificateNumber)]
		public static class ImportApprovalCertificateNumberProgram
		{
			public static void Run()
			{
				new ImportApprovalCertificateNumberXmlWriter().WriteXml(new HttpClientHelper());
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.SpecialCargoCode)]
		public static class SpecialCargoCodeProgram
		{
			public static void Run()
			{
				new SpecialCargoCodeXmlWriter().WriteXml(new HttpClientHelper());
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.SpecialCargoCodeLanguage)]
		public static class SpecialCargoCodeProgramLanguage
		{
			public static void Run()
			{
				new SpecialCargoCodeWithLanguageXmlWriter().WriteXml(new HttpClientHelper());
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.ExportTradeControlOrdinanceAppendix)]
		public static class ExportTradeControlOrdinanceAppendixProgram
		{
			public static void Run()
			{
				new ExportTradeControlOrdinanceAppendixXmlWriter().WriteXml(new HttpClientHelper());
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.ResultCode)]
		public static class ResultCodeProgram
		{
			public static void Run()
			{
				new ResultCodeXmlWriter().WriteXml(new HttpClientHelper());
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.MSXDocumentType)]
		public static class MSXDocumentTypeProgram
		{
			public static void Run()
			{
				new MSXDocumentTypeXmlWriter().WriteXml(new HttpClientHelper());
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.IATA)]
		public static class IATAProgram
		{
			public static void Run()
			{
				new IATAXmlWriter().WriteXml(new HttpClientHelper());
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.FSB)]
		public static class FSBProgram
		{
			public static void Run()
			{
				new FSBXmlWriter().WriteXml(new HttpClientHelper());
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.ContainerType)]
		public static class ContainerTypeProgram
		{
			public static void Run()
			{
				new ContainerTypeXmlWriter().WriteXml(new HttpClientHelper());
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.ContainerLength)]
		public static class ContainerLengthProgram
		{
			public static void Run()
			{
				new ContainerLengthXmlWriter().WriteXml(new HttpClientHelper());
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.ContainerHeight)]
		public static class ContainerHeightProgram
		{
			public static void Run()
			{
				new ContainerHeightXmlWriter().WriteXml(new HttpClientHelper());
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.SeaCarrier)]
		public static class SeaCarrierProgram
		{
			public static void Run()
			{
				SeaCarrierXmlWriter.WriteXml(new HttpClientHelper());
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.PackageType)]
		public static class PackageTypeProgram
		{
			public static void Run()
			{
				new PackageTypeXmlWriter().WriteXml(new HttpClientHelper());
			}
		}

		[StartupArgument(Business.Constants.ProgramFunctions.NACCS.UNLOCO)]
		public static class UNLOCOProgram
		{
			public static void Run()
			{
				new UNLOCOXmlWriter().WriteXml(new HttpClientHelper());
			}
		}
	}
}
