using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.Business.Testing
{
	class APHISLicenceTypeListTest : CodeDescriptionPairListTest
	{
		public void TestGetListForProgram()
		{
			var fullList = new APHISLicenseTypeList();
			var list1 = APHISLicenseTypeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.AAC);
			var list2 = APHISLicenseTypeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.AAC);
			AssertEquals("Data should be cached", list1, list2);
			var expectedCodes = new[] {
				APHISLicenseTypeList.Codes.LiveAnimalHealthCertificate,
				APHISLicenseTypeList.Codes.Aphis7040b7040c,
				APHISLicenseTypeList.Codes.AphisRabiesVaccination,
				APHISLicenseTypeList.Codes.AphisPpq368
			};
			AssertList<APHISLicenseTypeList>(list1, expectedCodes, fullList);

			list1 = APHISLicenseTypeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.ABS);
			list2 = APHISLicenseTypeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.ABS);
			AssertEquals("Data should be cached", list1, list2);
			expectedCodes = new[] {
				APHISLicenseTypeList.Codes.AphisBrs2000,
				APHISLicenseTypeList.Codes.AphisBrsAcknowledgementLetter,
				APHISLicenseTypeList.Codes.AphisBrsNotification,
				APHISLicenseTypeList.Codes.AphisPpq368
			};
			AssertList<APHISLicenseTypeList>(list1, expectedCodes, fullList);

			list1 = APHISLicenseTypeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.AVS);
			list2 = APHISLicenseTypeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.AVS);
			AssertEquals("Data should be cached", list1, list2);
			expectedCodes = new[] {
				APHISLicenseTypeList.Codes.LiveAnimalHealthCertificate,
				APHISLicenseTypeList.Codes.AnimalProductsCertificate,
				APHISLicenseTypeList.Codes.Aphis2006ResearchAndEvaluation,
				APHISLicenseTypeList.Codes.Aphis2006SaleAndDistribution,
				APHISLicenseTypeList.Codes.AphisVs166A,
				APHISLicenseTypeList.Codes.AphisVs1729,
				APHISLicenseTypeList.Codes.AphisVs17135,
				APHISLicenseTypeList.Codes.AphisVs1732,
				APHISLicenseTypeList.Codes.CertificateOfOrigin,
				APHISLicenseTypeList.Codes.AphisPpq368,
				APHISLicenseTypeList.Codes.TreatmentCertificate,
				APHISLicenseTypeList.Codes.ManufacturersStatementCertificateDeclaration
			};
			AssertList<APHISLicenseTypeList>(list1, expectedCodes, fullList);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.APHIS2024, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				list1 = APHISLicenseTypeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.APQ);
				list2 = APHISLicenseTypeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.APQ);
				AssertEquals("Data should be cached", list1, list2);
				expectedCodes = new[] {
					APHISLicenseTypeList.Codes.ElectronicPhytosanitaryCertificate,
					APHISLicenseTypeList.Codes.PhytosanitaryCertificate,
					APHISLicenseTypeList.Codes.AphisPpq203,
					APHISLicenseTypeList.Codes.AphisPpq525b,
					APHISLicenseTypeList.Codes.AphisPpq526,
					APHISLicenseTypeList.Codes.AphisPpq546,
					APHISLicenseTypeList.Codes.AphisPpq585,
					APHISLicenseTypeList.Codes.AphisPpq586,
					APHISLicenseTypeList.Codes.AphisPpq5878,
					APHISLicenseTypeList.Codes.AphisPpq58715,
					APHISLicenseTypeList.Codes.AphisPpq58737,
					APHISLicenseTypeList.Codes.AphisPpq58741,
					APHISLicenseTypeList.Codes.AphisPpq58755,
					APHISLicenseTypeList.Codes.AphisPpq58756,
					APHISLicenseTypeList.Codes.AphisPpq58775,
					APHISLicenseTypeList.Codes.AphisPpq58737can,
					APHISLicenseTypeList.Codes.AphisP588,
					APHISLicenseTypeList.Codes.AphisP621,
					APHISLicenseTypeList.Codes.AphisSeedAnalysisCertificate,
					APHISLicenseTypeList.Codes.AphisPpq368,
					APHISLicenseTypeList.Codes.CertificateOfOrigin,
					APHISLicenseTypeList.Codes.TreatmentCertificate,
					APHISLicenseTypeList.Codes.USCanadaGreenhouseGrownPlantExportCertificationLabel
				};
				AssertList<APHISLicenseTypeList>(list1, expectedCodes, fullList);
				Assert("The following codes have not been used:\r\n\r\n" + new ZStringBuilder(fullList.ToArray().Select(x => x.Code)).ToStringWithNewLineBetweenAppends(), fullList.Count == 0);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.APHIS2024, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, false))
			{
				Factory.ClearCachedValue<ICodeDescriptionPairList>("APHISLicenceTypeList_" + APHISProgramCodeList.Codes.APQ);
				list1 = APHISLicenseTypeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.APQ);
				list2 = APHISLicenseTypeList.GetListForProgram(Factory, APHISProgramCodeList.Codes.APQ);
				AssertEquals("Data should be cached", list1, list2);
				expectedCodes = new[] {
					APHISLicenseTypeList.Codes.ElectronicPhytosanitaryCertificate,
					APHISLicenseTypeList.Codes.PhytosanitaryCertificate,
					APHISLicenseTypeList.Codes.AphisPpq203,
					APHISLicenseTypeList.Codes.AphisPpq525b,
					APHISLicenseTypeList.Codes.AphisPpq526,
					APHISLicenseTypeList.Codes.AphisPpq546,
					APHISLicenseTypeList.Codes.AphisPpq585,
					APHISLicenseTypeList.Codes.AphisPpq586,
					APHISLicenseTypeList.Codes.AphisPpq5878,
					APHISLicenseTypeList.Codes.AphisPpq58715,
					APHISLicenseTypeList.Codes.AphisPpq58737,
					APHISLicenseTypeList.Codes.AphisPpq58741,
					APHISLicenseTypeList.Codes.AphisPpq58755,
					APHISLicenseTypeList.Codes.AphisPpq58756,
					APHISLicenseTypeList.Codes.AphisPpq58775,
					APHISLicenseTypeList.Codes.AphisPpq58737can,
					APHISLicenseTypeList.Codes.AphisP588,
					APHISLicenseTypeList.Codes.AphisP621,
					APHISLicenseTypeList.Codes.AphisSeedAnalysisCertificate,
					APHISLicenseTypeList.Codes.AphisPpq368,
					APHISLicenseTypeList.Codes.CertificateOfOrigin,
					APHISLicenseTypeList.Codes.TreatmentCertificate,
				};
				AssertList<APHISLicenseTypeList>(list1, expectedCodes, fullList);
				Assert("The following codes have not been used:\r\n\r\n" + new ZStringBuilder(fullList.ToArray().Select(x => x.Code)).ToStringWithNewLineBetweenAppends(), fullList.Count == 0);
			}
		}
	}
}
