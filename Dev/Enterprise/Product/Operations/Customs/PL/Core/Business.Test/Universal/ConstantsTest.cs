using CargoWise.EntityFramework.Testing;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Testing;

class ConstantsTest : TestCaseWithFactory
{
	public void TestR0031ESupportingDocuments()
	{
		var validationList = ValidationLists.R0031ESupportingDocuments(Factory);

		AssertContainsExactElementsInAnyOrder(nameof(ValidationLists.R0031ESupportingDocuments), [SupportingDocumentCodes._4DK3, SupportingDocumentCodes.C710], validationList);
	}

	public void TestR0039EProcedures()
	{
		var validationList = ValidationLists.R0039EProcedures(Factory);

		AssertContainsExactElementsInAnyOrder(nameof(ValidationLists.R0039EProcedures), ["1040", "2140"], validationList);
	}

	public void TestR425AndR481PrimaryPreferenceCodeList()
	{
		var validationList = ValidationLists.R425AndR481PrimaryPreferenceCodeList(Factory);
		var expectedList = new[] {
			PrimaryPreferenceCodes._120, PrimaryPreferenceCodes._123, PrimaryPreferenceCodes._125
			, PrimaryPreferenceCodes._128, PrimaryPreferenceCodes._220, PrimaryPreferenceCodes._223
			, PrimaryPreferenceCodes._225, PrimaryPreferenceCodes._320, PrimaryPreferenceCodes._323
			, PrimaryPreferenceCodes._325, PrimaryPreferenceCodes._420 };

		AssertContainsExactElementsInAnyOrder(validationList, expectedList);
	}

	public void TestSpecialProcedureCodesForPreviousDocuments()
	{
		var validationList = ValidationLists.SpecialProcedureCodesForPreviousDocuments(Factory);
		var expectedList = new[] { PreviousDocumentCodes.MRN, PreviousDocumentCodes.CLE, PreviousDocumentCodes.SDE
			, PreviousDocumentCodes.OGL, PreviousDocumentCodes.ZZZ };

		AssertContainsExactElementsInAnyOrder(validationList, expectedList);
	}

	public void TestBulkPackageCodeList()
	{
		var validationList = ValidationLists.BulkPackageCodes(Factory);
		var expectedList = new[] { "VQ", "VG", "VL", "VY", "VR", "VO", "VS" };

		AssertContainsExactElementsInAnyOrder(validationList, expectedList);
	}
}
