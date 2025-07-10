using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class TravelDocumentTypesTest : TestCaseWithFactory
	{
		public void TestGetAdditionalCertificateTypes()
		{
			var travelDocumentAndCrewACEIdTypes = new TravelDocumentTypes();
			travelDocumentAndCrewACEIdTypes.AddRangeOverwriteIfExists(new CrewACEIdTypes());
			var staffCertificateTypes = Factory.New<GlbStaff>().Certificates.AddNew().Lookups.CertificateTypes;
			var contactCertificateTypes = Factory.New<OrgContact>().Certificates.AddNew().Lookups.CertificateTypes;
			foreach (ICodeDescription type in travelDocumentAndCrewACEIdTypes)
			{
				AssertEquals(string.Format("GlbStaff.CertificateTypes should contain type '{0}'.", type.Code), true, staffCertificateTypes.ContainsCode(type.Code));
				AssertEquals(string.Format("OrgContact.CertificateTypes should contain type '{0}'.", type.Code), true, contactCertificateTypes.ContainsCode(type.Code));
			}
		}

		public void TestIsDriversLicense()
		{
			AssertTravelDocumentTypes(
				TravelDocumentTypes.IsDriversLicense,
				TravelDocumentTypes.Codes.CommercialDriversLicense,
				TravelDocumentTypes.Codes.DrivingLicenseNational,
				TravelDocumentTypes.Codes.EnhancedDriversLicense);
		}

		public void TestIsCountryOfIssuanceRequired()
		{
			AssertTravelDocumentTypes(
				TravelDocumentTypes.IsCountryOfIssuanceRequired,
				TravelDocumentTypes.Codes.CommercialDriversLicense,
				TravelDocumentTypes.Codes.DrivingLicenseNational,
				TravelDocumentTypes.Codes.EnhancedDriversLicense,
				TravelDocumentTypes.Codes.Passport,
				TravelDocumentTypes.Codes.USPassportCard,
				TravelDocumentTypes.Codes.VisaImmigrant,
				TravelDocumentTypes.Codes.VisaNonImmigrant,
				TravelDocumentTypes.Codes.BirthCertificate,
				TravelDocumentTypes.Codes.CitizenshipDocumentNumber,
				TravelDocumentTypes.Codes.NativeAmericanIndian);
		}

		public void TestIsStateOrProvinceOfIssuanceRequired()
		{
			AssertTravelDocumentTypes(
				TravelDocumentTypes.IsStateOrProvinceOfIssuanceRequired,
				TravelDocumentTypes.Codes.CommercialDriversLicense,
				TravelDocumentTypes.Codes.DrivingLicenseNational,
				TravelDocumentTypes.Codes.EnhancedDriversLicense,
				TravelDocumentTypes.Codes.BirthCertificate);
		}

		static void AssertTravelDocumentTypes(Func<string, bool> isSomethingMethod, params string[] types)
		{
			var otherTypes = new TravelDocumentTypes();
			foreach (var type in types)
			{
				AssertEquals(isSomethingMethod.Method.Name + " for document type: " + type, true, isSomethingMethod(type));
				otherTypes.RemoveCode(type);
			}

			foreach (ICodeDescription type in otherTypes)
			{
				AssertEquals(isSomethingMethod.Method.Name + " for document type: " + type, false, isSomethingMethod(type.Code));
			}
		}
	}
}
