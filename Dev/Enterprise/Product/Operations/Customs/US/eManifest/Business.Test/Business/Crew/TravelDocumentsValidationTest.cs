using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class TravelDocumentsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckXZ_Comment()
		{
			ValidationTestHelper.AssertErrorFieldIsNotMandatory(document.XZ_CommentInfo);
		}

		public void TestCheckXZ_ExpiryOrDueDate()
		{
			const string warning = "The expiry date entered is in the past.";
			document.XZ_ExpiryOrDueDate = ZDateTime.Today.AddDays(-1);
			AssertHasWarning(document.XZ_ExpiryOrDueDateInfo, warning);
			document.XZ_ExpiryOrDueDate = ZDateTime.Today.AddYears(2);
			AssertNoWarning(document.XZ_ExpiryOrDueDateInfo, warning);
		}

		public void TestCheckXZ_RefNumber()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(document.XZ_RefNumberInfo);
		}

		public void TestCheckXZ_RN_NKCountryOfIssuance()
		{
			document.XZ_Type = TravelDocumentTypes.Codes.Passport;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(document.XZ_RN_NKCountryOfIssuanceInfo, "??", Constants.CountryCodes.UnitedStates);
			document.XZ_Type = TravelDocumentTypes.Codes.OtherTravelDocument;
			ValidationTestHelper.AssertInvalidCodeMessageError(document.XZ_RN_NKCountryOfIssuanceInfo, "??", Constants.CountryCodes.UnitedStates);
		}

		public void TestCheckXZ_StateOrProvinceOfIssuance()
		{
			document.XZ_RN_NKCountryOfIssuance = Constants.CountryCodes.UnitedStates;
			document.XZ_Type = TravelDocumentTypes.Codes.BirthCertificate;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(document.XZ_StateOrProvinceOfIssuanceInfo, "??", USStatesList.Codes.Illinois);
			document.XZ_Type = TravelDocumentTypes.Codes.OtherTravelDocument;
			ValidationTestHelper.AssertInvalidCodeMessageError(document.XZ_StateOrProvinceOfIssuanceInfo, "??", USStatesList.Codes.Illinois);
			document.XZ_RN_NKCountryOfIssuance = Constants.CountryCodes.Aruba;
			document.XZ_Type = TravelDocumentTypes.Codes.BirthCertificate;
			ValidationTestHelper.AssertFieldIsNotMandatory(document.XZ_StateOrProvinceOfIssuanceInfo);
			document.XZ_StateOrProvinceOfIssuance = "XXX";
			AssertNoMessageErrorContaining(document.XZ_StateOrProvinceOfIssuanceInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckXZ_Type()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(document.XZ_TypeInfo, "??", TravelDocumentTypes.Codes.Passport);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var trip = Factory.New<Trip>();
			var crew = trip.CrewMembers.AddNew();
			document = crew.Certificates.AddNew();
		}

		GenRegCertAccredMaintList document;
	}
}
