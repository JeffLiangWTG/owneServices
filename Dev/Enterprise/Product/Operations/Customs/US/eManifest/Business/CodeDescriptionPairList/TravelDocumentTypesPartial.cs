using System.Linq;
using CargoWise.Integration;
using Enterprise.MasterFiles.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Business
{
	partial class TravelDocumentTypes : IPersonCertificateTypesProvider
	{
		internal static bool IsDriversLicense(string type)
		{
			return new[]
					{
						Codes.CommercialDriversLicense,
						Codes.DrivingLicenseNational,
						Codes.EnhancedDriversLicense
					}.Contains(type);
		}

		internal static bool IsCountryOfIssuanceRequired(string type)
		{
			return IsDriversLicense(type)
				   || new[]
						{
							Codes.Passport,
							Codes.USPassportCard,
							Codes.VisaImmigrant,
							Codes.VisaNonImmigrant,
							Codes.BirthCertificate,
							Codes.CitizenshipDocumentNumber,
							Codes.NativeAmericanIndian
						}.Contains(type);
		}

		internal static bool IsStateOrProvinceOfIssuanceRequired(string type)
		{
			return IsDriversLicense(type)
				   || type == Codes.BirthCertificate;
		}

		#region Implementation of IPersonCertificateTypesProvider

		public ICodeDescriptionPairList GetAdditionalCertificateTypes()
		{
			AddRangeOverwriteIfExists(new CrewACEIdTypes());
			return this;
		}

		#endregion
	}
}
