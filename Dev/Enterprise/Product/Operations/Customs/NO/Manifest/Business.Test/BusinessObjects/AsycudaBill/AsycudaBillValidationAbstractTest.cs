using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

[TestsSubclassesOf(typeof(AsycudaBillValidation))]
abstract class AsycudaBillValidationAbstractTest : BusinessObjectValidationTestCase
{
	public void TestForwarderEmailAndPhone()
	{
		var bill = GetAsycudaBillForTests();
		var expectedMessageError = "You have not entered a Phone Number or an Email address for Representative.";

		AssertNoMessageError("When Forwarder is empty", bill.ABL_OA_ForwarderInfo, expectedMessageError);

		var address = Factory.New<OrgAddress>();
		bill.ABL_OA_Forwarder = address.PK;

		var whenEmailIsEmptyPhoneIsEmpty = AssertEntityValidation(bill)
			.WhenProperty(x => x.ABL_ForwarderEmail, Is.EqualTo(ZString.Empty))
			.WhenProperty(x => x.ABL_ForwarderPhone, Is.EqualTo(ZString.Empty))
			.WhenValidating(bill.Validation.ValidateABL_ForwarderEmail)
			.WhenValidating(bill.Validation.ValidateABL_ForwarderPhone);
		whenEmailIsEmptyPhoneIsEmpty.ShouldCheckThat(x => x.ABL_ForwarderEmailInfo, Has.MessageErrorContaining(expectedMessageError));
		whenEmailIsEmptyPhoneIsEmpty.ShouldCheckThat(x => x.ABL_ForwarderPhoneInfo, Has.MessageErrorContaining(expectedMessageError));

		var whenEmailIsNotEmptyPhoneIsEmpty = AssertEntityValidation(bill)
			.WhenProperty(x => x.ABL_ForwarderEmail, Is.EqualTo("xyz@mail.com"))
			.WhenProperty(x => x.ABL_ForwarderPhone, Is.EqualTo(ZString.Empty))
			.WhenValidating(bill.Validation.ValidateABL_ForwarderEmail)
			.WhenValidating(bill.Validation.ValidateABL_ForwarderPhone);
		whenEmailIsNotEmptyPhoneIsEmpty.ShouldCheckThat(x => x.ABL_ForwarderEmailInfo, Has.NoMessageErrorContaining(expectedMessageError));
		whenEmailIsNotEmptyPhoneIsEmpty.ShouldCheckThat(x => x.ABL_ForwarderPhoneInfo, Has.NoMessageErrorContaining(expectedMessageError));

		var whenEmailIsEmptyPhoneIsNotEmpty = AssertEntityValidation(bill)
			.WhenProperty(x => x.ABL_ForwarderEmail, Is.EqualTo(ZString.Empty))
			.WhenProperty(x => x.ABL_ForwarderPhone, Is.EqualTo("1239876"))
			.WhenValidating(bill.Validation.ValidateABL_ForwarderEmail)
			.WhenValidating(bill.Validation.ValidateABL_ForwarderPhone);
		whenEmailIsEmptyPhoneIsNotEmpty.ShouldCheckThat(x => x.ABL_ForwarderEmailInfo, Has.NoMessageErrorContaining(expectedMessageError));
		whenEmailIsEmptyPhoneIsNotEmpty.ShouldCheckThat(x => x.ABL_ForwarderPhoneInfo, Has.NoMessageErrorContaining(expectedMessageError));
	}

	public void TestForwarder_CustomCodes()
	{
		var expectedMessageError = "The Representative must have a valid EORI or Norwegian “organization number”. (Organization’s Config tab Registration Code EOR, ORG or MVA must exist.)";

		var bill = GetAsycudaBillForTests();
		var validation = bill.Validation;

		var address = Factory.New<OrgAddress>();
		var orgHeader = Factory.New<OrgHeader>();

		CombineAssertions(() =>
		{
			AssertNoMessageError("When Forwarder is empty", bill.ABL_OA_ForwarderInfo, expectedMessageError);

			bill.ABL_OA_Forwarder = address.PK;
			AssertHasMessageError("When Forwarder OrgHeader is empty", bill.ABL_OA_ForwarderInfo, expectedMessageError);

			address.OA_OH = orgHeader.PK;
			validation.ValidateABL_OA_Forwarder();
			AssertHasMessageError("When no custom code added", bill.ABL_OA_ForwarderInfo, expectedMessageError);

			var cusCodeEORI = Factory.New<OrgCusCode>();
			cusCodeEORI.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			orgHeader.CustomsCodes.Add(cusCodeEORI);
			validation.ValidateABL_OA_Forwarder();
			AssertNoMessageError("When EORI custom code added", bill.ABL_OA_ForwarderInfo, expectedMessageError);

			orgHeader.CustomsCodes.RemoveAndDeleteAll();
			var cusCodeMVA = Factory.New<OrgCusCode>();
			cusCodeMVA.OK_CodeType = OrgCusCode.NorwayCodeTypes.MVA;
			orgHeader.CustomsCodes.Add(cusCodeMVA);
			validation.ValidateABL_OA_Forwarder();
			AssertNoMessageError("When MVA custom code added", bill.ABL_OA_ForwarderInfo, expectedMessageError);

			orgHeader.CustomsCodes.RemoveAndDeleteAll();
			var cusCodeORG = Factory.New<OrgCusCode>();
			cusCodeORG.OK_CodeType = OrgCusCode.CodeTypes.OrganizationNumber;
			orgHeader.CustomsCodes.Add(cusCodeORG);
			validation.ValidateABL_OA_Forwarder();
			AssertNoMessageError("When ORG custom code added", bill.ABL_OA_ForwarderInfo, expectedMessageError);
		});
	}

	protected abstract AsycudaBill GetAsycudaBillForTests();
}
