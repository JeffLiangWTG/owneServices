using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.eTail.Business.DeniedPartyScreening.Testing
{
	public class HVLVDpsClearingReasonValidationTest : TestCaseWithFactory
	{
		public void TestValidateClearingReason_WhenValueIsSelected_ShouldNotShowError()
		{
			var model = new HVLVDpsClearingReason(Array.Empty<HVLVDpsMatch>(), Factory);
			model.ClearingReason = "OTH";

			model.Validation.ValidateClearingReason();
			AssertNoErrors(model.ClearingReasonInfo);
		}

		public void TestValidateClearingReason_WhenNoValueIsSelected_ShouldShowError()
		{
			var model = new HVLVDpsClearingReason(Array.Empty<HVLVDpsMatch>(), Factory);
			model.ClearingReason = string.Empty;

			model.Validation.ValidateClearingReason();
			AssertHasError(model.ClearingReasonInfo, "Your organization requires you to enter a reason for clearing this record as it had potential denied party matches. Your reason will be recorded for audit purposes.");
		}

		public void TestValidateClearingReasonText_WhenTextIsValid_ShouldNotShowError()
		{
			var model = new HVLVDpsClearingReason(Array.Empty<HVLVDpsMatch>(), Factory);
			model.ClearingReasonText = "123 456";

			model.Validation.ValidateClearingReasonText();
			AssertNoErrors(model.ClearingReasonTextInfo);
		}

		public void TestValidateClearingReasonText_WhenCodeAndTextAreEmpty_ShouldShowError()
		{
			var model = new HVLVDpsClearingReason(Array.Empty<HVLVDpsMatch>(), Factory);
			model.ClearingReason = string.Empty;
			model.ClearingReasonText = string.Empty;

			model.Validation.ValidateClearingReasonText();
			AssertHasError(model.ClearingReasonTextInfo, "Your organization requires you to enter a reason for clearing this record as it had potential denied party matches. Your reason will be recorded for audit purposes.");
		}

		public void TestValidateClearingReasonText_WhenTextIsEmpty_ShouldShowError()
		{
			var model = new HVLVDpsClearingReason(Array.Empty<HVLVDpsMatch>(), Factory);
			model.ClearingReason = "OTH";
			model.ClearingReasonText = string.Empty;

			model.Validation.ValidateClearingReasonText();
			AssertHasError(model.ClearingReasonTextInfo, "Clearing reason must be minimum two words and six characters long.");
		}

		public void TestValidateClearingReasonText_WhenTextIsOneWord_ShouldShowError()
		{
			var model = new HVLVDpsClearingReason(Array.Empty<HVLVDpsMatch>(), Factory);
			model.ClearingReasonText = "123456";

			model.Validation.ValidateClearingReasonText();
			AssertHasError(model.ClearingReasonTextInfo, "Clearing reason must be minimum two words long.");
		}

		public void TestValidateClearingReasonText_WhenTextIsMoreThanOneWordButLessThanSixChars_ShouldShowError()
		{
			var model = new HVLVDpsClearingReason(Array.Empty<HVLVDpsMatch>(), Factory);
			model.ClearingReasonText = "a word";

			model.Validation.ValidateClearingReasonText();
			AssertHasError(model.ClearingReasonTextInfo, "Clearing reason must be minimum six characters long.");
		}

		public void TestValidateClearingReasonText_WhenTextIsOneWordAndLessThanSixChars_ShouldShowError()
		{
			var model = new HVLVDpsClearingReason(Array.Empty<HVLVDpsMatch>(), Factory);
			model.ClearingReasonText = "one";

			model.Validation.ValidateClearingReasonText();
			AssertHasError(model.ClearingReasonTextInfo, "Clearing reason must be minimum two words and six characters long.");
		}
	}
}
