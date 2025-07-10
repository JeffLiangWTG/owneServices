using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class ScreeningStatusWinModelValidationTest : TestCaseWithFactory
	{
		public void TestValidateClearingReasonText()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var screeningParty = new ScreeningParty(orgHeader, string.Empty, orgHeader);

			var requireReasonWrapper = new RequireReasonForCLRWrapper(new RequireReasonForCLRItemCollection())
			{
				RequireReasonForCLR = true
			};

			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForClearing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requireReasonWrapper))
			{
				var model = new ScreeningStatusWinModel(ScoreGrades.Medium, screeningParty)
				{
					ScreeningStatus = ScreeningStatusesList.Codes.Clear,
				};

				AssertNoErrors(model.ClearingReasonTextInfo);

				model.ClearingReason = "OTH";
				AssertHasError(model.ClearingReasonTextInfo, "Your organization requires you to enter a reason for clearing this record as it had potential denied party matches. Your reason will be recorded for audit purposes.");

				model.ClearingReasonText = "123456";
				AssertHasError(model.ClearingReasonTextInfo, "Clearing reason must be minimum two words long.");

				model.ClearingReasonText = "12 34";
				AssertHasError(model.ClearingReasonTextInfo, "Clearing reason must be minimum six characters long.");

				model.ClearingReasonText = "1234";
				AssertHasError(model.ClearingReasonTextInfo, "Clearing reason must be minimum two words and six characters long.");

				model.ClearingReasonText = "123 456";
				AssertNoErrors(model.ClearingReasonTextInfo);
			}
		}
	}
}
