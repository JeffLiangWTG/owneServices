using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	partial class APHISLicenseTypeList
	{
		public static ICodeDescriptionPairList GetListForProgram(BusinessObjectFactory factory, ZString programCode)
		{
			return factory.GetCachedValue<ICodeDescriptionPairList>("APHISLicenceTypeList_" + programCode, () =>
				{
					var result = new CodeDescriptionPairList();
					switch (programCode)
					{
						case APHISProgramCodeList.Codes.AAC:
							result.AddPair(Codes.LiveAnimalHealthCertificate, Descriptions.LiveAnimalHealthCertificate);
							result.AddPair(Codes.Aphis7040b7040c, Descriptions.Aphis7040b7040c);
							result.AddPair(Codes.AphisRabiesVaccination, Descriptions.AphisRabiesVaccination);
							result.AddPair(Codes.AphisPpq368, Descriptions.AphisPpq368);
							break;
						case APHISProgramCodeList.Codes.ABS:
							result.AddPair(Codes.AphisBrs2000, Descriptions.AphisBrs2000);
							result.AddPair(Codes.AphisBrsAcknowledgementLetter, Descriptions.AphisBrsAcknowledgementLetter);
							result.AddPair(Codes.AphisBrsNotification, Descriptions.AphisBrsNotification);
							result.AddPair(Codes.AphisPpq368, Descriptions.AphisPpq368);
							break;
						case APHISProgramCodeList.Codes.APQ:
							result.AddPair(Codes.PhytosanitaryCertificate, Descriptions.PhytosanitaryCertificate);
							result.AddPair(Codes.ElectronicPhytosanitaryCertificate, Descriptions.ElectronicPhytosanitaryCertificate);
							result.AddPair(Codes.TreatmentCertificate, Descriptions.TreatmentCertificate);
							result.AddPair(Codes.AphisPpq203, Descriptions.AphisPpq203);
							result.AddPair(Codes.AphisPpq525b, Descriptions.AphisPpq525b);
							result.AddPair(Codes.AphisPpq526, Descriptions.AphisPpq526);
							result.AddPair(Codes.AphisPpq546, Descriptions.AphisPpq546);
							result.AddPair(Codes.AphisPpq585, Descriptions.AphisPpq585);
							result.AddPair(Codes.AphisPpq586, Descriptions.AphisPpq586);
							result.AddPair(Codes.AphisPpq5878, Descriptions.AphisPpq5878);
							result.AddPair(Codes.AphisPpq58715, Descriptions.AphisPpq58715);
							result.AddPair(Codes.AphisPpq58737, Descriptions.AphisPpq58737);
							result.AddPair(Codes.AphisPpq58741, Descriptions.AphisPpq58741);
							result.AddPair(Codes.AphisPpq58755, Descriptions.AphisPpq58755);
							result.AddPair(Codes.AphisPpq58756, Descriptions.AphisPpq58756);
							result.AddPair(Codes.AphisPpq58775, Descriptions.AphisPpq58775);
							result.AddPair(Codes.AphisPpq58737can, Descriptions.AphisPpq58737can);
							result.AddPair(Codes.AphisP588, Descriptions.AphisP588);
							result.AddPair(Codes.AphisP621, Descriptions.AphisP621);
							result.AddPair(Codes.AphisSeedAnalysisCertificate, Descriptions.AphisSeedAnalysisCertificate);
							result.AddPair(Codes.AphisPpq368, Descriptions.AphisPpq368);
							result.AddPair(Codes.CertificateOfOrigin, Descriptions.CertificateOfOrigin);
							if (ZZCustomsFunctionality.IsAPHIS2024Effective)
							{
								result.AddPair(Codes.USCanadaGreenhouseGrownPlantExportCertificationLabel, Descriptions.USCanadaGreenhouseGrownPlantExportCertificationLabel);
							}
							break;
						case APHISProgramCodeList.Codes.AVS:
							result.AddPair(Codes.LiveAnimalHealthCertificate, Descriptions.LiveAnimalHealthCertificate);
							result.AddPair(Codes.AnimalProductsCertificate, Descriptions.AnimalProductsCertificate);
							result.AddPair(Codes.TreatmentCertificate, Descriptions.TreatmentCertificate);
							result.AddPair(Codes.Aphis2006ResearchAndEvaluation, Descriptions.Aphis2006ResearchAndEvaluation);
							result.AddPair(Codes.Aphis2006SaleAndDistribution, Descriptions.Aphis2006SaleAndDistribution);
							result.AddPair(Codes.AphisVs166A, Descriptions.AphisVs166A);
							result.AddPair(Codes.AphisVs1729, Descriptions.AphisVs1729);
							result.AddPair(Codes.AphisVs17135, Descriptions.AphisVs17135);
							result.AddPair(Codes.AphisVs1732, Descriptions.AphisVs1732);
							result.AddPair(Codes.CertificateOfOrigin, Descriptions.CertificateOfOrigin);
							result.AddPair(Codes.AphisPpq368, Descriptions.AphisPpq368);
							result.AddPair(Codes.ManufacturersStatementCertificateDeclaration, Descriptions.ManufacturersStatementCertificateDeclaration);
							break;
					}
					result.SortByDescription();
					return result;
				});
		}
	}
}
