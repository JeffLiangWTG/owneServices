using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	[TestedType(typeof(ScreeningStatusWinModel))]
	public class ScreeningStatusWinModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var screeningParty = new ScreeningParty(orgHeader, string.Empty, orgHeader);
			var model = new ScreeningStatusWinModel(ScoreGrades.Low, screeningParty);
			CombineAssertions(() =>
			{
				AssertEquals("Learn about Denied Party Screening", model.LearnDeniedPartyScreeningText);
				AssertEquals("https://myaccount.cargowise.com/Home/CargoWiseLearning.aspx#item=D4B99507-3D62-45CE-B256-65D149DA53EA&amp;video=11582152,91b0f67ddf8574bcbde03ab13ad54bf9", ScreeningStatusWinModel.Uri);
			});
		}

		public void TestScreeningStatusesInOrganization_ShouldHaveMatchedAndClearAndPermanentClear()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var screeningParty = new ScreeningParty(orgHeader, string.Empty, orgHeader);
			var model = new ScreeningStatusWinModel(ScoreGrades.Low, screeningParty);

			CombineAssertions(() =>
			{
				AssertEquals(2, model.ScreeningStatuses.Count);
				AssertNotNull(model.ScreeningStatuses.GetDescriptionFromCode(ScreeningStatusesList.Codes.Matched));
				AssertNotNull(model.ScreeningStatuses.GetDescriptionFromCode(ScreeningStatusesList.Codes.Clear));
			});
		}

		public void TestScreeningStatusesInJob_ShouldHaveMatchedAndClear()
		{
			var docAddress = Factory.New<JobDocAddress>();
			var screeningParty = new ScreeningParty(docAddress, string.Empty, docAddress);
			var model = new ScreeningStatusWinModel(ScoreGrades.Low, screeningParty);
			CombineAssertions(() =>
			{
				AssertEquals(2, model.ScreeningStatuses.Count);
				AssertNotNull(model.ScreeningStatuses.ToArray().First(r => r.Code == ScreeningStatusesList.Codes.Matched));
				AssertNotNull(model.ScreeningStatuses.ToArray().First(r => r.Code == ScreeningStatusesList.Codes.Clear));
			});
		}

		public void TestScreeningStatuses_WhenComboBoxIsHidden()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var screeningParty = new ScreeningParty(orgHeader, string.Empty, orgHeader);
			var model = new ScreeningStatusWinModel(ScoreGrades.Low, screeningParty);

			CombineAssertions(() =>
			{
				AssertEquals(2, model.ScreeningStatuses.Count);
				AssertEquals(false, model.HideScreeningStatusesComboBox);
			});

			model = new ScreeningStatusWinModel(ScoreGrades.Low, screeningParty, null, true);
			model.ScreeningStatuses.Clear();
			CombineAssertions(() =>
			{
				AssertEquals(0, model.ScreeningStatuses.Count);
				AssertEquals(true, model.HideScreeningStatusesComboBox);
			});

			model = new ScreeningStatusWinModel(ScoreGrades.Low, screeningParty, null, true);
			AssertEquals(true, model.HideScreeningStatusesComboBox);

			model = new ScreeningStatusWinModel(ScoreGrades.Low, screeningParty);
			model.ScreeningStatuses.Clear();
			AssertEquals(true, model.HideScreeningStatusesComboBox);
		}

		public void TestScreeningStatusClearingReasonNotAllowInvalidValue()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var screeningParty = new ScreeningParty(orgHeader, string.Empty, orgHeader);
			var model = new ScreeningStatusWinModel(ScoreGrades.Low, screeningParty);
			AssertEquals(ZString.Empty, model.ScreeningStatus);

			model.ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			AssertEquals(ScreeningStatusesList.Codes.Matched, model.ScreeningStatus);

			model.ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			AssertEquals(ScreeningStatusesList.Codes.Clear, model.ScreeningStatus);

			model.ScreeningStatus = "DUM";
			AssertEquals(ScreeningStatusesList.Codes.Clear, model.ScreeningStatus);

			model.ScreeningStatus = "  " + ScreeningStatusesList.Codes.Matched + "  ";
			AssertEquals(ScreeningStatusesList.Codes.Matched, model.ScreeningStatus);

			model.ClearingReason = "DUM";
			AssertEquals(ZString.Empty, model.ClearingReason);

			model.ClearingReason = "  " + RequireReasonForCLRRegistryConstants.Code.Other + "  ";
			AssertEquals(RequireReasonForCLRRegistryConstants.Code.Other, model.ClearingReason);

			model.ClearingReason = "DUM";
			AssertEquals(RequireReasonForCLRRegistryConstants.Code.Other, model.ClearingReason);
		}

		public void TestClearingReason_RegistryNeededForOther()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var screeningParty = new ScreeningParty(orgHeader, string.Empty, orgHeader);
			var tmpSecurityCore = new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK)
			{
				OrgDeniedPartyScreeningAllowOtherReason = { IsAllowed = true }
			};

			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			{
				var model = new ScreeningStatusWinModel(ScoreGrades.Low, screeningParty);
				CombineAssertions(() =>
				{
					AssertEquals("Only OTH option", 1, model.ClearingReasons.Count);
					AssertNotNull(model.ClearingReasons.GetDescriptionFromCode(RequireReasonForCLRRegistryConstants.Code.Other));
				});
			}

			tmpSecurityCore.OrgDeniedPartyScreeningAllowOtherReason.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			{
				var model = new ScreeningStatusWinModel(ScoreGrades.Low, screeningParty);
				CombineAssertions(() =>
				{
					AssertEquals("No OTH option", 0, model.ClearingReasons.Count);
					AssertNull(model.ClearingReasons.GetDescriptionFromCode(RequireReasonForCLRRegistryConstants.Code.Other));
				});
			}
		}

		public void TestClearingReason_ReasonTextFieldEmptyEditableAndVisibleForOther()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var screeningParty = new ScreeningParty(orgHeader, string.Empty, orgHeader);
			var tmpSecurityCore = new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK)
			{
				OrgDeniedPartyScreeningAllowOtherReason = { IsAllowed = true }
			};

			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			{
				var model = new ScreeningStatusWinModel(ScoreGrades.Low, screeningParty);
				model.ScreeningStatus = ScreeningStatusesList.Codes.Clear;

				AssertEquals("Only OTH option", 1, model.ClearingReasons.Count);
				AssertNotNull(model.ClearingReasons.GetDescriptionFromCode(RequireReasonForCLRRegistryConstants.Code.Other));

				model.ClearingReason = RequireReasonForCLRRegistryConstants.Code.Other;

				AssertEquals("Should have empty textfield for input", string.Empty, model.ClearingReasonText);
				AssertEquals("Should have editable textfield", true, model.ClearingReasonTextEnabled);
				AssertEquals(true, model.ClearingReasonsVisibility);
			}
		}

		public void TestClearingReason_UsesReasonsFromRegistry()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var screeningParty = new ScreeningParty(orgHeader, string.Empty, orgHeader);
			var tmpSecurityCore = new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK)
			{
				OrgDeniedPartyScreeningAllowOtherReason = { IsAllowed = false }
			};

			var itemCollection = new RequireReasonForCLRItemCollection
			{
				new RequireReasonForCLRItem { Code = "C01", Title = "Title1", IsMandatory = true },
				new RequireReasonForCLRItem
				{
					Code = "C02", Title = "Title2", ClearingReason = "", IsMandatory = false,
				},
				new RequireReasonForCLRItem { Code = "C03", Title = "Title3", ClearingReason = "Description 123", IsMandatory = true }
			};

			var requireReasonWrapper = new RequireReasonForCLRWrapper(itemCollection)
			{
				RequireReasonForCLR = true
			};

			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForClearing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requireReasonWrapper))
			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			{
				var model = new ScreeningStatusWinModel(ScoreGrades.Low, screeningParty);

				model.ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				model.ClearingReason = "C03";

				CombineAssertions(() =>
				{
					AssertEquals("All options from registry and no OTH", 3, model.ClearingReasons.Count);
					AssertEquals("Should have editable textfield since C03 has mandatory", true, model.ClearingReasonTextEnabled);
					AssertEquals("Should have C03's reason in textfield", "Description 123", model.ClearingReasonText);
					AssertEquals("Should have visible textfield", true, model.ClearingReasonsVisibility);
					AssertNull(model.ClearingReasons.GetDescriptionFromCode(RequireReasonForCLRRegistryConstants.Code.Other));
				});

				model.ClearingReason = "C02";

				CombineAssertions(() =>
				{
					AssertEquals("Should not have editable textfield since C02 does not have mandatory", false, model.ClearingReasonTextEnabled);
					AssertEquals("Should have C02's reason in textfield", "Not Requested", model.ClearingReasonText);
					AssertEquals("Should have visible textfield", true, model.ClearingReasonsVisibility);
				});
			}
		}

		public void TestClearingReasonTextValidation()
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

				model.ClearingReasonText = "123456";
				AssertEquals(false, model.SaveButtonEnabled);
				AssertEquals("Clearing reason must be minimum two words long.", model.ClearingReasonTextValidationText);

				model.ClearingReasonText = string.Empty;
				AssertEquals(false, model.SaveButtonEnabled);
				AssertEquals("Your organization requires you to enter a reason for clearing this record as it had potential denied party matches. Your reason will be recorded for audit purposes.", model.ClearingReasonTextValidationText);

				model.ClearingReasonText = "12 34";
				AssertEquals(false, model.SaveButtonEnabled);
				AssertEquals("Clearing reason must be minimum six characters long.", model.ClearingReasonTextValidationText);

				model.ClearingReasonText = "1234";
				AssertEquals(false, model.SaveButtonEnabled);
				AssertEquals("Clearing reason must be minimum two words and six characters long.", model.ClearingReasonTextValidationText);

				model.ClearingReasonText = "123 456";
				AssertEquals(true, model.SaveButtonEnabled);
				AssertNull(model.ClearingReasonTextValidationText);
			}
		}

		public void TestNotReplaceInvalidCharactersToSpaceForClearingReasonText()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var screeningParty = new ScreeningParty(orgHeader, string.Empty, orgHeader);
			var model = new ScreeningStatusWinModel(ScoreGrades.Medium, screeningParty);
			model.ClearingReasonText = "\r\n123\t456\n789";
			AssertEquals(" 123 456 789", model.ClearingReasonText);
		}

		public void TestNoValidation()
		{
			var requireReasonWrapper = new RequireReasonForCLRWrapper(new RequireReasonForCLRItemCollection())
			{
				RequireReasonForCLR = true
			};

			var orgHeader = Factory.New<OrgHeader>();
			var screeningParty = new ScreeningParty(orgHeader, string.Empty, orgHeader);

			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForClearing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requireReasonWrapper))
			{
				var model = new ScreeningStatusWinModel(ScoreGrades.Low, screeningParty)
				{
					ScreeningStatus = ScreeningStatusesList.Codes.Clear,
				};

				model.ClearingReasonText = "123456";
				AssertEquals(true, model.SaveButtonEnabled);
				AssertEquals(null, model.ClearingReasonTextValidationText);
			}

			requireReasonWrapper.RequireReasonForCLR = false;
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForClearing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requireReasonWrapper))
			{
				var model = new ScreeningStatusWinModel(ScoreGrades.Medium, screeningParty)
				{
					ScreeningStatus = ScreeningStatusesList.Codes.Clear,
				};

				model.ClearingReasonText = "123456";
				AssertEquals(true, model.SaveButtonEnabled);
				AssertEquals(null, model.ClearingReasonTextValidationText);
			}
		}

		public void TestCountryChangeStatusWatermark_ShouldHaveAcceptDropdownList()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			var model = new ScreeningStatusWinModel(ScoreGrades.High, new ScreeningParty(country, country.HumanReadableName, country));

			CombineAssertions(() =>
			{
				AssertEquals(1, model.ScreeningStatuses.Count);
				AssertEquals("Mark as Sanctioned", model.ChangeStatusWaterMark.Caption);
				AssertEquals("Accept", model.ScreeningStatuses[0].Description);
				AssertEquals("MAT", model.ScreeningStatuses[0].Code);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var screeningParty = new ScreeningParty(orgHeader, string.Empty, orgHeader);

			return new ScreeningStatusWinModel(ScoreGrades.Low, screeningParty)
			{
				ScreeningStatus = ScreeningStatusesList.Codes.Clear,
			};
		}
	}
}
