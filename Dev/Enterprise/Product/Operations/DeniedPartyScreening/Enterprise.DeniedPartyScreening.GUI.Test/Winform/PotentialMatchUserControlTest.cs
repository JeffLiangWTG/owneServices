using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class PotentialMatchUserControlTest : TestCaseWithFactory
	{
		public void TestMatchVisibilityWithEmptyMatchInfo()
		{
			var profileNameInfos = new List<ProfileNameInfo>();
			var nameMatchInfos = new List<NameMatchInfo>();
			var profileHeaderInfo = new ProfileHeaderInfo { SourceProfileID = Guid.NewGuid(), ProfileNotes = Array.Empty<byte>(), ProfileNames = profileNameInfos, ProfileAddresses = new List<ProfileAddressInfo>(), ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(), ProfileCountries = new List<ProfileCountryInfo>(), SourceListCodes = new List<string>(), TypeOfEntity = DeniedPartyConstants.ScreeningNameTypes.Country };
			var potentialMatchModel = new PotentialMatchModel(profileHeaderInfo, new List<AddressMatchInfo>(), nameMatchInfos, new List<RegistrationCodeMatchInfo>(), new List<CountryMatchInfo>(), new BusinessObjectFactory(), DeniedPartyConstants.ScreeningNameTypes.Country);
			var winModel = new PotentialMatchWinModel(potentialMatchModel);

			using (var form = new ZForm())
			using (var userControl = new PotentialMatchUserControl())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(winModel, "");
				form.Show();

				AssertEquals(winModel.NameMatchWinModel.MatchViewVisibility, false);
				AssertEquals(winModel.AddressMatchWinModel.MatchViewVisibility, false);
				AssertEquals(winModel.RegistrationCodeWinModel.MatchViewVisibility, false);

				AssertEquals("SourceListNamesUserControl, GenericMatchUserControl ", 2, userControl.FindAll<ZUserControl>(u => u is ProfileNotesUserControl || u is SourceListNamesUserControl || u is GenericMatchUserControl).Count());
			}
		}

		public void TestMatchVisibilityWithMatchInfo()
		{
			var winModel = PotentialMatchWinModelTest.GetNewPotentialMatchWinModel(DeniedPartyConstants.ScreeningNameTypes.Organization);

			using (var form = new ZForm())
			using (var userControl = new PotentialMatchUserControl())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(winModel, "");
				form.Show();

				AssertEquals(winModel.NameMatchWinModel.MatchViewVisibility, true);
				AssertEquals(winModel.AddressMatchWinModel.MatchViewVisibility, true);
				AssertEquals(winModel.RegistrationCodeWinModel.MatchViewVisibility, true);

				AssertEquals("ProfileNotesUserControl, SourceListNamesUserControl, GenericMatchUserControl", 5, userControl.FindAll<ZUserControl>(u => u is ProfileNotesUserControl || u is SourceListNamesUserControl || u is GenericMatchUserControl).Count());
			}
		}

		public void TestControlHasPartyNameEntityIconMatchRisk()
		{
			var winModel = PotentialMatchWinModelTest.GetNewPotentialMatchWinModel(DeniedPartyConstants.ScreeningNameTypes.Organization);

			using (var form = new ZForm())
			using (var userControl = new PotentialMatchUserControl())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(winModel, "");
				form.Show();

				AssertNotNull(userControl.FindSingleOrDefault<ZLabel>("ProfileNameLabel"));
				AssertNotNull(userControl.FindSingleOrDefault<ZLabel>("MiddleScoreGradeLabel"));
				AssertNotNull(userControl.FindSingleOrDefault<ZLabel>("ScoreGradeReviewLabel"));

				AssertNotNull(userControl.FindSingleOrDefault<ZPictureBox>("ProfilePictureBox"));
				AssertNotNull(userControl.FindSingleOrDefault<ZPictureBox>("MiddleScoreGradePictureBox"));
				AssertNotNull(userControl.FindSingleOrDefault<ZPictureBox>("HeaderScoreGradePictureBox"));
			}
		}

		public void TestHighRiskEntityIconAndText()
		{
			var profileId1 = Guid.NewGuid();
			var regCodeId1 = Guid.NewGuid();
			var profileRegistrationCodeInfos = new List<ProfileRegistrationCodeInfo>()
			{
				new ProfileRegistrationCodeInfo { ID = regCodeId1, IdType = "A", IdNumber = "123", IdCountry = "CN", SourceProfileID = profileId1 },
			};
			var registrationCodeMatchInfos = new List<RegistrationCodeMatchInfo>()
			{
				new RegistrationCodeMatchInfo { RequestRegistrationCode = new DpsRegistrationCodeCandidate { RegCountryCode = "CN", RegCodeType = "A", RegCodeValue = "123" }, MatchingRegistrationCodeID = regCodeId1, MatchingRegistrationCodeScore = 100, SourceProfileID = profileId1 },
			};
			var profileHeaderInfo = new ProfileHeaderInfo
			{
				SourceProfileID = Guid.NewGuid(),
				ProfileNotes = Array.Empty<byte>(),
				ProfileNames = new List<ProfileNameInfo>(),
				ProfileAddresses = new List<ProfileAddressInfo>(),
				ProfileRegistrationCodes = profileRegistrationCodeInfos,
				ProfileCountries = new List<ProfileCountryInfo>(),
				SourceListCodes = new List<string>(),
				TypeOfEntity = DeniedPartyConstants.ScreeningNameTypes.Organization
			};
			var addressMatchInfos = new List<AddressMatchInfo>();
			var nameMatchInfos = new List<NameMatchInfo>();
			var countryMatchInfos = new List<CountryMatchInfo>();
			var potentialMatchModel = new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, Factory, DeniedPartyConstants.ScreeningNameTypes.Organization);
			var winModel = new PotentialMatchWinModel(potentialMatchModel);

			using (var form = new ZForm())
			using (var userControl = new PotentialMatchUserControl())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(winModel, "");
				form.Show();

				var scoreGradeReviewLabel = userControl.FindSingleOrDefault<ZLabel>("ScoreGradeReviewLabel");
				var profileNameLabel = userControl.FindSingleOrDefault<ZLabel>("ProfileNameLabel");
				var middleScoreGradeLabel = userControl.FindSingleOrDefault<ZLabel>("MiddleScoreGradeLabel");

				AssertEquals("High Risk - Review Required", scoreGradeReviewLabel.Text);
				AssertEquals(Color.FromArgb(255, 209, 25, 25), scoreGradeReviewLabel.BackColor);
				AssertEquals(Color.FromArgb(255, 209, 25, 25), userControl.FindSingleOrDefault<ZPanel>("PotentialMatchHeaderPanel").BackColor);

				AssertEquals("Unknown", profileNameLabel.Text);
				AssertEquals(Color.FromArgb(255, 220, 225, 228), profileNameLabel.BackColor);

				AssertEquals("High", middleScoreGradeLabel.Text);
				AssertEquals(Color.FromArgb(255, 220, 225, 228), middleScoreGradeLabel.BackColor);
			}
		}

		public void TestMediumRiskContent()
		{
			var profileId1 = Guid.NewGuid();
			var nameId1 = Guid.NewGuid();
			var profileNameInfos = new List<ProfileNameInfo>()
			{
				new ProfileNameInfo { ID = nameId1, FullName = "Test Full Name 1", Language = "Chinese", IsPrimaryName = true, SourceProfileID = profileId1 },
			};
			var nameMatchInfos = new List<NameMatchInfo>()
			{
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "ORG", FullName = "Test Full Name 3" }, MatchingNameID = nameId1, MatchingNameScore = 66, SourceProfileID = profileId1 },
			};
			using (OrganisationsDataRegistry.Instance.MatchingConfidenceThresholdsForOrganisations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DpsConfidenceThresholdsBusinessObject(65, 90)))
			{
				var factory = new BusinessObjectFactory();
				var profileHeaderInfo = new ProfileHeaderInfo { SourceProfileID = Guid.NewGuid(), ProfileNotes = Array.Empty<byte>(), ProfileNames = profileNameInfos, ProfileAddresses = new List<ProfileAddressInfo>(), ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(), ProfileCountries = new List<ProfileCountryInfo>(), SourceListCodes = new List<string>(), TypeOfEntity = DeniedPartyConstants.ScreeningNameTypes.Organization };
				var addressMatchInfos = new List<AddressMatchInfo>();
				var registrationCodeMatchInfos = new List<RegistrationCodeMatchInfo>();
				var countryMatchInfos = new List<CountryMatchInfo>();
				var winModel = new PotentialMatchWinModel(new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, factory, DeniedPartyConstants.ScreeningNameTypes.Organization));

				using (var form = new ZForm())
				using (var userControl = new PotentialMatchUserControl())
				{
					form.Controls.Add(userControl);
					userControl.SetDataBinding(winModel, "");
					form.Show();

					var scoreGradeReviewLabel = userControl.FindSingleOrDefault<ZLabel>("ScoreGradeReviewLabel");
					var profileNameLabel = userControl.FindSingleOrDefault<ZLabel>("ProfileNameLabel");
					var middleScoreGradeLabel = userControl.FindSingleOrDefault<ZLabel>("MiddleScoreGradeLabel");

					AssertEquals("Medium Risk - Review Required", scoreGradeReviewLabel.Text);
					AssertEquals(Color.FromArgb(255, 226, 105, 0), scoreGradeReviewLabel.BackColor);
					AssertEquals(Color.FromArgb(255, 226, 105, 0), userControl.FindSingleOrDefault<ZPanel>("PotentialMatchHeaderPanel").BackColor);

					AssertEquals("Test Full Name 1", profileNameLabel.Text);
					AssertEquals(Color.FromArgb(255, 220, 225, 228), profileNameLabel.BackColor);

					AssertEquals("Medium", middleScoreGradeLabel.Text);
					AssertEquals(Color.FromArgb(255, 220, 225, 228), middleScoreGradeLabel.BackColor);
				}
			}
		}
	}
}
