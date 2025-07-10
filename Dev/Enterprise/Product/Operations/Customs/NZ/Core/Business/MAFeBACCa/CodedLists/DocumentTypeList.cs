using System.Linq;
using Enterprise.Customs.NZ.Business.MAFeBACCa.XMLSchemas;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists
{
	public class DocumentTypeList : CodeDescriptionEnumList<DocumentTypes>
	{
		public static class Codes
		{
			public const string ACVMImportApproval = "AIA";
			public const string AnimalImportPermitMulti = "AIM";
			public const string AnimalImportPermitSingle = "AIS";
			public const string AQISExportDocument = "AED";
			public const string AQISPermitToImport = "API";
			public const string AQISQuarantineClearance = "AQC";
			public const string BACC = "BAC";
			public const string BillofLading = "BOL";
			public const string BiologicalImportPermit = "BIP";
			public const string CertCaptivityArtificialProp = "CCA";
			public const string CertificateOfAcquisition = "COA";
			public const string CertificateOfOrigin = "COO";
			public const string CertifiedInvoice = "CIV";
			public const string CITESExportPermit = "CEP";
			public const string CITESPermitToImport = "CPI";
			public const string ConsignmentClearanceNotification = "CCN";
			public const string ContainerInspection = "CIS";
			public const string ContainerLogSheet = "CLS";
			public const string DioxinCertificate = "DIC";
			public const string ElectronicMessage = "ELM";
			public const string EX28 = "EX2";
			public const string ExporterDeclaration = "EXD";
			public const string FumigationCertificate = "FMC";
			public const string GMOCertificate = "GMC";
			public const string GMODecoderSheet = "GMD";
			public const string GovernmentCertification = "GVC";
			public const string Invoice = "INV";
			public const string IrradiationCertificate = "IRC";
			public const string IVACertificate = "IVA";
			public const string LABSpecimenImportPermit = "LSI";
			public const string LaboratoryTestResults = "LTR";
			public const string LetterExImportExport = "LIE";
			public const string Manifest = "MAN";
			public const string Other = "OTH";
			public const string PackingList = "PKL";
			public const string LetterExNPPRL = "LEN";
			public const string LetterOther = "LTO";
			public const string LetterFromMAFBA = "LFM";
			public const string ManufacturersDeclaration = "MFD";
			public const string MicrobiologicalPermitMulti = "MPM";
			public const string OwnersDeclaration = "OND";
			public const string PasteurisationCertificate = "PTC";
			public const string PermitToExport = "PTE";
			public const string PermitToImportSeed = "PIS";
			public const string PersonalEffectDeclaration = "PED";
			public const string PhytosanitaryCertificate = "PSC";
			public const string PhytosanitaryReexportCertificate = "PRC";
			public const string PlantImportPermitMulti = "PIM";
			public const string PlantImportPermitSingle = "PIP";
			public const string QDContainer = "QDC";
			public const string QuarantineDeclaration = "QDL";
			public const string RFPCertificate = "RFP";
			public const string SamplingCertificate = "SPC";
			public const string SEACOTFTempApplication = "STA";
			public const string SEACOTFTempApproval = "STP";
			public const string SeedAnalysisCertificate = "SAC";
			public const string ThermographRecord = "THR";
			public const string TreatmentCertificate = "TRC";
			public const string VetCertNoNumber = "VCO";
			public const string VetCertNumbered = "VCN";
			public const string ZooSanitaryCertificate = "ZSC";
		}

		public static class Descriptions
		{
			public const string ACVMImportApproval = "ACVM Import Approval";
			public const string AnimalImportPermitMulti = "Animal Import Permit (Multi)";
			public const string AnimalImportPermitSingle = "Animal Import Permit (Single)";
			public const string AQISExportDocument = "Australian Quarantine Export Document";
			public const string AQISPermitToImport = "Australian Quarantine Permit To Import";
			public const string AQISQuarantineClearance = "Australian Quarantine Clearance";
			public const string BACC = "BACC";
			public const string BillofLading = "Bill of Lading";
			public const string BiologicalImportPermit = "Biological Import Permit";
			public const string CertCaptivityArtificialProp = "Certificate Captivity Artificial Prop";
			public const string CertificateOfAcquisition = "Certificate Of Acquisition";
			public const string CertificateOfOrigin = "Certificate Of Origin";
			public const string CertifiedInvoice = "Certified Invoice";
			public const string CITESExportPermit = "CITES Export Permit";
			public const string CITESPermitToImport = "CITES Permit To Import";
			public const string ConsignmentClearanceNotification = "Consignment Clearance Notification";
			public const string ContainerInspection = "Container Inspection";
			public const string ContainerLogSheet = "Container Log Sheet";
			public const string DioxinCertificate = "Dioxin Certificate";
			public const string ElectronicMessage = "Electronic Message";
			public const string EX28 = "EX28";
			public const string ExporterDeclaration = "Exporter Declaration";
			public const string FumigationCertificate = "Fumigation Certificate";
			public const string GMOCertificate = "GMO Certificate";
			public const string GMODecoderSheet = "GMO Decoder Sheet";
			public const string GovernmentCertification = "Government Certification";
			public const string Invoice = "Invoice";
			public const string IrradiationCertificate = "Irradiation Certificate";
			public const string IVACertificate = "IVA Certificate";
			public const string LABSpecimenImportPermit = "LAB Specimen Import Permit";
			public const string LaboratoryTestResults = "Laboratory Test Results";
			public const string LetterExImportExport = "Letter Ex Import Export";
			public const string Manifest = "Manifest";
			public const string Other = "Other";
			public const string PackingList = "Packing List";
			public const string LetterExNPPRL = "Letter Ex NPPRL";
			public const string LetterOther = "Letter Other";
			public const string LetterFromMAFBA = "Letter From MAFBA";
			public const string ManufacturersDeclaration = "Manufacturer's Declaration";
			public const string MicrobiologicalPermitMulti = "Microbiological Permit (Multi)";
			public const string OwnersDeclaration = "Owner's Declaration";
			public const string PasteurisationCertificate = "Pasteurisation Certificate";
			public const string PermitToExport = "Permit To Export";
			public const string PermitToImportSeed = "Permit To Import Seed";
			public const string PersonalEffectDeclaration = "Personal Effects Declaration";
			public const string PhytosanitaryCertificate = "Phytosanitary Certificate";
			public const string PhytosanitaryReexportCertificate = "Phytosanitary Re-export Certificate";
			public const string PlantImportPermitMulti = "Plant Import Permit Multi";
			public const string PlantImportPermitSingle = "Plant Import Permit Single";
			public const string QDContainer = "QD Container";
			public const string QuarantineDeclaration = "Quarantine Declaration";
			public const string RFPCertificate = "RFP Certificate";
			public const string SamplingCertificate = "Sampling Certificate";
			public const string SEACOTFTempApplication = "SEACOTF Temp Application";
			public const string SEACOTFTempApproval = "SEACOTF Temp Approval";
			public const string SeedAnalysisCertificate = "Seed Analysis Certificate";
			public const string ThermographRecord = "Thermograph Record";
			public const string TreatmentCertificate = "Treatment Certificate";
			public const string VetCertNoNumber = "Vet Certificate - No Number";
			public const string VetCertNumbered = "Vet Certificate - Numbered";
			public const string ZooSanitaryCertificate = "Zoo Sanitary Certificate";
		}

		public DocumentTypeList()
		{
			AddPair(Codes.ACVMImportApproval, Descriptions.ACVMImportApproval, DocumentTypes.ACVMImportApproval);
			AddPair(Codes.AnimalImportPermitMulti, Descriptions.AnimalImportPermitMulti, DocumentTypes.AnimalImportPermitMulti);
			AddPair(Codes.AnimalImportPermitSingle, Descriptions.AnimalImportPermitSingle, DocumentTypes.AnimalImportPermitSingle);
			AddPair(Codes.AQISExportDocument, Descriptions.AQISExportDocument, DocumentTypes.AQISExportDocument);
			AddPair(Codes.AQISPermitToImport, Descriptions.AQISPermitToImport, DocumentTypes.AQISPermitToImport);
			AddPair(Codes.AQISQuarantineClearance, Descriptions.AQISQuarantineClearance, DocumentTypes.AQISQuarantineClearance);
			AddPair(Codes.BACC, Descriptions.BACC, DocumentTypes.BACC);
			AddPair(Codes.BillofLading, Descriptions.BillofLading, DocumentTypes.BillofLading);
			AddPair(Codes.BiologicalImportPermit, Descriptions.BiologicalImportPermit, DocumentTypes.BiologicalImportPermit);
			AddPair(Codes.CertCaptivityArtificialProp, Descriptions.CertCaptivityArtificialProp, DocumentTypes.CertCaptivityArtificialProp);
			AddPair(Codes.CertificateOfAcquisition, Descriptions.CertificateOfAcquisition, DocumentTypes.CertificateOfAcquisition);
			AddPair(Codes.CertificateOfOrigin, Descriptions.CertificateOfOrigin, DocumentTypes.CertificateOfOrigin);
			AddPair(Codes.CertifiedInvoice, Descriptions.CertifiedInvoice, DocumentTypes.CertifiedInvoice);
			AddPair(Codes.CITESExportPermit, Descriptions.CITESExportPermit, DocumentTypes.CITESExportPermit);
			AddPair(Codes.CITESPermitToImport, Descriptions.CITESPermitToImport, DocumentTypes.CITESPermitToImport);
			AddPair(Codes.ConsignmentClearanceNotification, Descriptions.ConsignmentClearanceNotification, DocumentTypes.ConsignmentClearanceNotification);
			AddPair(Codes.ContainerInspection, Descriptions.ContainerInspection, DocumentTypes.ContainerInspection);
			AddPair(Codes.ContainerLogSheet, Descriptions.ContainerLogSheet, DocumentTypes.ContainerLogSheet);
			AddPair(Codes.DioxinCertificate, Descriptions.DioxinCertificate, DocumentTypes.DioxinCertificate);
			AddPair(Codes.ElectronicMessage, Descriptions.ElectronicMessage, DocumentTypes.ElectronicMessage);
			AddPair(Codes.EX28, Descriptions.EX28, DocumentTypes.EX28);
			AddPair(Codes.ExporterDeclaration, Descriptions.ExporterDeclaration, DocumentTypes.ExporterDeclaration);
			AddPair(Codes.FumigationCertificate, Descriptions.FumigationCertificate, DocumentTypes.FumigationCertificate);
			AddPair(Codes.GMOCertificate, Descriptions.GMOCertificate, DocumentTypes.GMOCertificate);
			AddPair(Codes.GMODecoderSheet, Descriptions.GMODecoderSheet, DocumentTypes.GMODecoderSheet);
			AddPair(Codes.GovernmentCertification, Descriptions.GovernmentCertification, DocumentTypes.GovernmentCertification);
			AddPair(Codes.Invoice, Descriptions.Invoice, DocumentTypes.Invoice);
			AddPair(Codes.IrradiationCertificate, Descriptions.IrradiationCertificate, DocumentTypes.IrradiationCertificate);
			AddPair(Codes.IVACertificate, Descriptions.IVACertificate, DocumentTypes.IVACertificate);
			AddPair(Codes.LABSpecimenImportPermit, Descriptions.LABSpecimenImportPermit, DocumentTypes.LABSpecimenImportPermit);
			AddPair(Codes.LaboratoryTestResults, Descriptions.LaboratoryTestResults, DocumentTypes.LaboratoryTestResults);
			AddPair(Codes.LetterExImportExport, Descriptions.LetterExImportExport, DocumentTypes.LetterExImportExport);
			AddPair(Codes.Manifest, Descriptions.Manifest, DocumentTypes.Manifest);
			AddPair(Codes.Other, Descriptions.Other, DocumentTypes.Other);
			AddPair(Codes.PackingList, Descriptions.PackingList, DocumentTypes.PackingList);
			AddPair(Codes.LetterExNPPRL, Descriptions.LetterExNPPRL, DocumentTypes.LetterExNPPRL);
			AddPair(Codes.LetterOther, Descriptions.LetterOther, DocumentTypes.LetterOther);
			AddPair(Codes.LetterFromMAFBA, Descriptions.LetterFromMAFBA, DocumentTypes.LetterFromMAFBA);
			AddPair(Codes.ManufacturersDeclaration, Descriptions.ManufacturersDeclaration, DocumentTypes.ManufacturersDeclaration);
			AddPair(Codes.MicrobiologicalPermitMulti, Descriptions.MicrobiologicalPermitMulti, DocumentTypes.MicrobiologicalPermitMulti);
			AddPair(Codes.OwnersDeclaration, Descriptions.OwnersDeclaration, DocumentTypes.OwnersDeclaration);
			AddPair(Codes.PasteurisationCertificate, Descriptions.PasteurisationCertificate, DocumentTypes.PasteurisationCertificate);
			AddPair(Codes.PermitToExport, Descriptions.PermitToExport, DocumentTypes.PermitToExport);
			AddPair(Codes.PermitToImportSeed, Descriptions.PermitToImportSeed, DocumentTypes.PermitToImportSeed);
			AddPair(Codes.PersonalEffectDeclaration, Descriptions.PersonalEffectDeclaration, DocumentTypes.PersonalEffectDeclaration);
			AddPair(Codes.PhytosanitaryCertificate, Descriptions.PhytosanitaryCertificate, DocumentTypes.PhytosanitaryCertificate);
			AddPair(Codes.PhytosanitaryReexportCertificate, Descriptions.PhytosanitaryReexportCertificate, DocumentTypes.PhytosanitaryReexportCertificate);
			AddPair(Codes.PlantImportPermitMulti, Descriptions.PlantImportPermitMulti, DocumentTypes.PlantImportPermitMulti);
			AddPair(Codes.PlantImportPermitSingle, Descriptions.PlantImportPermitSingle, DocumentTypes.PlantImportPermitSingle);
			AddPair(Codes.QDContainer, Descriptions.QDContainer, DocumentTypes.QDContainer);
			AddPair(Codes.QuarantineDeclaration, Descriptions.QuarantineDeclaration, DocumentTypes.QuarantineDeclaration);
			AddPair(Codes.RFPCertificate, Descriptions.RFPCertificate, DocumentTypes.RFPCertificate);
			AddPair(Codes.SamplingCertificate, Descriptions.SamplingCertificate, DocumentTypes.SamplingCertificate);
			AddPair(Codes.SEACOTFTempApplication, Descriptions.SEACOTFTempApplication, DocumentTypes.SEACOTFTempApplication);
			AddPair(Codes.SEACOTFTempApproval, Descriptions.SEACOTFTempApproval, DocumentTypes.SEACOTFTempApproval);
			AddPair(Codes.SeedAnalysisCertificate, Descriptions.SeedAnalysisCertificate, DocumentTypes.SeedAnalysisCertificate);
			AddPair(Codes.ThermographRecord, Descriptions.ThermographRecord, DocumentTypes.ThermographRecord);
			AddPair(Codes.TreatmentCertificate, Descriptions.TreatmentCertificate, DocumentTypes.TreatmentCertificate);
			AddPair(Codes.VetCertNoNumber, Descriptions.VetCertNoNumber, DocumentTypes.VetCertNoNumber);
			AddPair(Codes.VetCertNumbered, Descriptions.VetCertNumbered, DocumentTypes.VetCertNumbered);
			AddPair(Codes.ZooSanitaryCertificate, Descriptions.ZooSanitaryCertificate, DocumentTypes.ZooSanitaryCertificate);
		}

		internal static bool IsImportPermit(string documentType)
		{
			return new[]
					{
						Codes.ACVMImportApproval,
						Codes.AnimalImportPermitMulti,
						Codes.AnimalImportPermitSingle,
						Codes.AQISPermitToImport,
						Codes.BiologicalImportPermit,
						Codes.CITESPermitToImport,
						Codes.LABSpecimenImportPermit,
						Codes.MicrobiologicalPermitMulti,
						Codes.PermitToImportSeed,
						Codes.PlantImportPermitMulti,
						Codes.PlantImportPermitSingle,
					}.Contains(documentType);
		}

		internal static bool IsRelevantInvoice(string documentType)
		{
			return new[]
					{
						Codes.Invoice,
						Codes.CertifiedInvoice
					}.Contains(documentType);
		}

		internal static bool IsCertificate(string documentType)
		{
			return new[]
					{
						Codes.BACC,
						Codes.CertCaptivityArtificialProp,
						Codes.CertificateOfAcquisition,
						Codes.CertificateOfOrigin,
						Codes.DioxinCertificate,
						Codes.FumigationCertificate,
						Codes.GMOCertificate,
						Codes.GovernmentCertification,
						Codes.IrradiationCertificate,
						Codes.IVACertificate,
						Codes.PasteurisationCertificate,
						Codes.PhytosanitaryCertificate,
						Codes.PhytosanitaryReexportCertificate,
						Codes.RFPCertificate,
						Codes.SamplingCertificate,
						Codes.SeedAnalysisCertificate,
						Codes.TreatmentCertificate,
						Codes.VetCertNoNumber,
						Codes.VetCertNumbered,
						Codes.ZooSanitaryCertificate
					}.Contains(documentType);
		}
	}
}
