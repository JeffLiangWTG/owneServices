using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.DeniedPartyScreening;
using Enterprise.eTail.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.DeniedPartyScreening.Common.DeniedPartyConstants;

namespace Enterprise.eTail.GUI.Testing
{
	[TestedType(typeof(HVLVDpsResultForm))]
	public class HVLVDpsResultFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			using (var form = new HVLVDpsResultForm(new HVLVDpsResult(new List<DpsResponseWithScreeningParty> { CreateDpsResponseWithScreeningParty() }, Factory)))
			{
				form.Show();
				AssertEquals("New Denied Party Screening Result", form.Text);
			}
		}

		public void TestGridMatchedNames_ShouldShowDisplayScoreForScore()
		{
			using var form = new HVLVDpsResultForm_ForTest(new List<DpsResponseWithScreeningParty> { CreateDpsResponseWithScreeningParty() }, Factory);
			form.Show();

			var grid = form.GridMatchedNames;
			var scoreColumn = GetColumn(grid, "Score");

			AssertNotNull(scoreColumn);
			AssertEquals(true, scoreColumn is ZCustomZeroDisplayColumnStyleInfo);
		}

		public void TestGridMatchedAddresses_ShouldShowDisplayScoreForScore()
		{
			using var form = new HVLVDpsResultForm_ForTest(new List<DpsResponseWithScreeningParty> { CreateDpsResponseWithScreeningParty() }, Factory);
			form.Show();

			var grid = form.GridMatchedAddresses;
			var scoreColumn = GetColumn(grid, "Score");

			AssertNotNull(scoreColumn);
			AssertEquals(true, scoreColumn is ZCustomZeroDisplayColumnStyleInfo);
		}

		public void TestGridMatchedRegistrationCodes_ShouldShowDisplayScoreForScore()
		{
			using var form = new HVLVDpsResultForm_ForTest(new List<DpsResponseWithScreeningParty> { CreateDpsResponseWithScreeningParty() }, Factory);
			form.Show();

			var grid = form.GridMatchedRegistrationCodes;
			var scoreColumn = GetColumn(grid, "Score");

			AssertNotNull(scoreColumn);
			AssertEquals(true, scoreColumn is ZCustomZeroDisplayColumnStyleInfo);
		}

		public void TestMatchButtonClicked_ShouldUpdateStatusAndClearReason()
		{
			using var form = new HVLVDpsResultForm_ForTest(new List<DpsResponseWithScreeningParty> { CreateDpsResponseWithOrgHeader() }, Factory);
			form.Show();

			var dpsResult = form.BusinessEntity as HVLVDpsResult;
			var selectedParty = dpsResult?.ResponseWithScreeningParties[0];
			selectedParty.Cleared = true;
			selectedParty.ClearingReasonTitle = "Other";
			selectedParty.ClearingReason = "OTH";
			selectedParty.ClearingReasonText = "Other Reason";

			var grid = form.GridScreenedParties;
			grid.SelectAllElements();
			form.ClickMatchButton();

			Assert(selectedParty.Matched);
			AssertNullOrEmpty(selectedParty.ClearingReasonTitle);
			AssertNullOrEmpty(selectedParty.ClearingReason);
			AssertNullOrEmpty(selectedParty.ClearingReasonText);
		}

		public void TestClearButtonClicked_WhenClearingReasonIsNotRequired_ShouldNotShowClearingReasonDialog()
		{
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForClearing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateRequireReasonWrapper(false)))
			using (var form = new HVLVDpsResultForm_ForTest(new List<DpsResponseWithScreeningParty> { CreateDpsResponseWithOrgHeader() }, Factory))
			{
				form.Show();

				var dpsResult = form.BusinessEntity as HVLVDpsResult;
				var party = dpsResult.ResponseWithScreeningParties[0];
				party.Matched = true;

				var grid = form.GridScreenedParties;
				grid.SelectAllElements();
				form.ClickClearButton();

				Assert(party.Cleared);
			}
		}

		public void TestClearButtonClicked_WhenClearingReasonIsRequiredAndSaveButtonIsClicked_ShouldUpdateStatusAndClearingReason()
		{
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForClearing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateRequireReasonWrapper(true)))
			using (var form = new HVLVDpsResultForm_ForTest(new List<DpsResponseWithScreeningParty> { CreateDpsResponseWithOrgHeader() }, Factory))
			{
				form.Show();

				var dpsResult = form.BusinessEntity as HVLVDpsResult;
				var party = dpsResult.ResponseWithScreeningParties[0];
				party.Matched = true;

				var grid = form.GridScreenedParties;
				grid.SelectAllElements();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					var form = (HVLVDpsClearingReasonForm)obj;
					var clearingReason = form.BusinessEntity as HVLVDpsClearingReason;
					clearingReason.ClearingReason = "OTH";
					clearingReason.ClearingReasonText = "Other Reason";
				});

				form.ClickClearButton();

				Assert(party.Cleared);
				AssertEquals("OTH", party.ClearingReason);
				AssertEquals("Other Reason", party.ClearingReasonText);
			}
		}

		public void TestClearButtonClicked_WhenClearingReasonIsRequiredButCancelButtonIsClicked_ShouldNotUpdateClearingStatus()
		{
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForClearing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateRequireReasonWrapper(true)))
			using (var form = new HVLVDpsResultForm_ForTest(new List<DpsResponseWithScreeningParty> { CreateDpsResponseWithOrgHeader() }, Factory))
			{
				form.Show();

				var dpsResult = form.BusinessEntity as HVLVDpsResult;
				var party = dpsResult.ResponseWithScreeningParties[0];
				party.Matched = true;

				var grid = form.GridScreenedParties;
				grid.SelectAllElements();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					var form = (HVLVDpsClearingReasonForm)obj;
					var clearingReason = form.BusinessEntity as HVLVDpsClearingReason;

					clearingReason.ClearingReason = "OTH";
					clearingReason.ClearingReasonText = "Other Reason";
				});

				form.ClickClearButton();

				Assert(party.Matched);
				AssertNullOrEmpty(party.ClearingReason);
				AssertNullOrEmpty(party.ClearingReasonText);
			}
		}

		public void TestClearButtonClicked_WhenClearingReasonIsChangedButCancelButtonIsClicked_ShouldNotUpdateClearingReason()
		{
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForClearing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateRequireReasonWrapper(true)))
			using (var form = new HVLVDpsResultForm_ForTest(new List<DpsResponseWithScreeningParty> { CreateDpsResponseWithOrgHeader() }, Factory))
			{
				form.Show();

				var dpsResult = form.BusinessEntity as HVLVDpsResult;
				var party = dpsResult?.ResponseWithScreeningParties[0];
				party.Cleared = true;
				party.ClearingReason = "CR1";
				party.ClearingReasonText = "Original Clearing Reason";

				var grid = form.GridScreenedParties;
				grid.SelectAllElements();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					var form = (HVLVDpsClearingReasonForm)obj;
					var clearingReason = form.BusinessEntity as HVLVDpsClearingReason;

					clearingReason.ClearingReason = "OTH";
					clearingReason.ClearingReasonText = "Other Reason";
				});

				form.ClickClearButton();

				Assert(party.Cleared);
				AssertEquals("CR1", party.ClearingReason);
				AssertEquals("Original Clearing Reason", party.ClearingReasonText);
			}
		}

		public void TestSelectMultiplePartiesOnSingleHVLVConsignment()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_GoodsDescription = "AA";

			var dpsResponseWithScreeningParty1 = CreateDpsResponseWithScreeningParty(consignment);
			var dpsResponseWithScreeningParty2 = CreateDpsResponseWithScreeningParty(consignment);

			Factory.Save();

			using var form = new HVLVDpsResultForm_ForTest(
					new List<DpsResponseWithScreeningParty>
					{
						dpsResponseWithScreeningParty1, dpsResponseWithScreeningParty2
					}, Factory);

			form.Show();

			CombineAssertions("Clicking on both buttons should throw no exceptions", () =>
			{
				AssertNoExceptionThrown(() => form.ClickMatchButton());
				AssertNoExceptionThrown(() => form.ClickClearButton());
			});
		}

		public void TestColorDecider()
		{
			var orgHeaderNotScreened = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeaderUnknown = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeaderMatched = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeaderCleared = Factory.NewWithValidTestData<OrgHeader>();

			orgHeaderNotScreened.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			orgHeaderUnknown.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			orgHeaderMatched.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			orgHeaderCleared.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var dpsNotScreened = CreateDpsResponseWithOrgHeader(orgHeaderNotScreened);
			var dpsNotUnknown = CreateDpsResponseWithOrgHeader(orgHeaderUnknown);
			var dpsNotMatched = CreateDpsResponseWithOrgHeader(orgHeaderMatched);
			var dpsNotCleared = CreateDpsResponseWithOrgHeader(orgHeaderCleared);

			using var form = new HVLVDpsResultForm(new List<DpsResponseWithScreeningParty> { dpsNotScreened, dpsNotUnknown, dpsNotMatched, dpsNotCleared }, Factory);
			form.Show();

			var grid = form.Controls.Find("gridScreenedParties", true).OfType<ZGrid>().First();

			var hvlvDpsMatchNotScreened = new HVLVDpsMatch(dpsNotScreened, Factory);
			var color = GetGridRowColour(grid, hvlvDpsMatchNotScreened);
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, hvlvDpsMatchNotScreened.Status);
			AssertEquals(DeniedPartyConstants.GridColor.NotScreened, color);

			var hvlvDpsMatchUnknown = new HVLVDpsMatch(dpsNotUnknown, Factory);
			color = GetGridRowColour(grid, hvlvDpsMatchUnknown);
			AssertEquals(ScreeningStatusesList.Codes.Unknown, hvlvDpsMatchUnknown.Status);
			AssertEquals(DeniedPartyConstants.GridColor.Unknown, color);

			var hvlvDpsMatch = new HVLVDpsMatch(dpsNotMatched, Factory);
			color = GetGridRowColour(grid, hvlvDpsMatch);
			AssertEquals(ScreeningStatusesList.Codes.Matched, hvlvDpsMatch.Status);
			AssertEquals(DeniedPartyConstants.GridColor.Matched, color);

			var hvlvDpsMatchCleared = new HVLVDpsMatch(dpsNotCleared, Factory);
			color = GetGridRowColour(grid, hvlvDpsMatchCleared);
			AssertEquals(ScreeningStatusesList.Codes.Clear, hvlvDpsMatchCleared.Status);
			AssertEquals(DeniedPartyConstants.GridColor.Clear, color);
		}

		public void TestMinimumSize()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();

				CombineAssertions("The size of the form should be greater than the minimum value.", () =>
				{
					var minimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1249, 600, true);
					AssertEquals(minimumSize, form.MinimumSize);

					form.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(minimumSize.Width - 1, minimumSize.Height - 1, true);
					AssertGreaterThanOrEqualTo(form.Size.Width, form.MinimumSize.Width);
					AssertGreaterThanOrEqualTo(form.Size.Height, form.MinimumSize.Height);
				});

				CombineAssertions("The size of the groupBoxCountControl should be greater than the minimum value.", () =>
				{
					var groupBoxCountControlMinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1236, 68, true);
					var groupBoxCountControl = form.GetControl<ZGroupBox>("groupBoxCountProperties");
					AssertEquals(groupBoxCountControlMinimumSize, groupBoxCountControl.MinimumSize);

					groupBoxCountControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(groupBoxCountControlMinimumSize.Width - 1, groupBoxCountControlMinimumSize.Height - 1, true);
					AssertGreaterThanOrEqualTo(groupBoxCountControl.Size.Width, groupBoxCountControl.MinimumSize.Width);
					AssertGreaterThanOrEqualTo(groupBoxCountControl.Size.Height, groupBoxCountControl.MinimumSize.Height);
				});
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new HVLVDpsResultForm(new HVLVDpsResult(new List<DpsResponseWithScreeningParty> { CreateDpsResponseWithScreeningParty() }, Factory));
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return (control.Name == "ScreenedPartiesCount" || control.Name == "NotScreenedPartiesCount" ||
				control.Name == "ClearPartiesCount" || control.Name == "MatchedPartiesCount" ||
				control.Name == "UnknownPartiesCount" || base.ShouldIgnoreMissingBindingMember(control));
		}

		#endregion

		DpsResponseWithScreeningParty CreateDpsResponseWithScreeningParty()
		{
			var profiles = new List<ProfileHeaderInfo>()
			{
				new()
				{
					SourceProfileID = Guid.NewGuid(),
					ProfileNotes = Array.Empty<byte>(),
					TypeOfEntity = ScreeningNameTypes.Person
				},
			};

			var header = Factory.NewWithValidTestData<OrgHeader>();
			var party = new ScreeningParty(header, string.Empty, header);
			var responseWithParty = new DpsResponseWithScreeningParty(party, new DpsResponse { Profiles = profiles }, new DpsRequestHeaderWithAddressMatching());
			return responseWithParty;
		}

		DpsResponseWithScreeningParty CreateDpsResponseWithOrgHeader(OrgHeader orgHeader = null)
		{
			if (orgHeader == null)
			{
				orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			}

			var party = new ScreeningParty(orgHeader, "EFG", orgHeader);

			return CreateDpsResponse(party);
		}

		DpsResponseWithScreeningParty CreateDpsResponseWithScreeningParty(HVLVConsignment consignment)
		{
			var sourceProfileID = Guid.NewGuid();
			var nameMatchInfo = GetNameMatchInfo(85, sourceProfileID);
			var profileNameInfo = new ProfileNameInfo { ID = nameMatchInfo.MatchingNameID };

			var profiles = new List<ProfileHeaderInfo>()
			{
				new()
				{
					SourceProfileID = sourceProfileID,
					ProfileNotes = Array.Empty<byte>(),
					ProfileNames = new List<ProfileNameInfo>() { profileNameInfo },
					TypeOfEntity = ScreeningNameTypes.Person,
					SourceListCodes = new List<string>() { "test" }
				},
			};

			var dpsResponse = new DpsResponse
			{
				ResponseCode = DpsResponseCode.Successful,
				ExtraMessage = "",
				AddressMatches = Enumerable.Empty<AddressMatchInfo>(),
				NameMatches = new List<NameMatchInfo>() { nameMatchInfo },
				RegistrationCodeMatches = Enumerable.Empty<RegistrationCodeMatchInfo>(),
				Profiles = profiles
			};

			var header = Factory.NewWithValidTestData<OrgHeader>();
			var party = new ScreeningParty((BusinessObject)consignment ?? header, "", header);
			var responseWithParty = new DpsResponseWithScreeningParty(party, dpsResponse, new DpsRequestHeaderWithAddressMatching());
			return responseWithParty;
		}

		static DpsResponseWithScreeningParty CreateDpsResponse(ScreeningParty party)
		{
			var sourceProfileID = Guid.NewGuid();
			var nameMatches = new NameMatchInfo[] { GetNameMatchInfo(80, sourceProfileID) };
			var profiles = new List<ProfileHeaderInfo>()
			{
				new() {
					SourceProfileID = sourceProfileID,
					ProfileNotes = Compressor.Zip("Test"),
					TypeOfEntity = ScreeningNameTypes.Person,
					ProfileNames = new ProfileNameInfo[] { new() { FullName = "Test5", ID = nameMatches[0].MatchingNameID } },
					SourceListCodes = new string[] { "Test" },
				}
			};

			var dpsResponse = new DpsResponse { Profiles = profiles, NameMatches = nameMatches };

			var responseWithParty = new DpsResponseWithScreeningParty(party, dpsResponse, new DpsRequestHeaderWithAddressMatching());
			return responseWithParty;
		}

		static NameMatchInfo GetNameMatchInfo(int score, Guid sourceProfileID)
		{
			return new NameMatchInfo
			{
				RequestName = new DpsNameCandidate
				{
					NameType = "ORG", FullName = "Test Full Name"
				},
				MatchingStandardizedValue = "Test1",
				MatchingNameID = Guid.NewGuid(),
				MatchingNameScore = score,
				SourceProfileID = sourceProfileID,
			};
		}

		static RequireReasonForCLRWrapper CreateRequireReasonWrapper(bool required = false)
		{
			var requireReasonWrapper = new RequireReasonForCLRWrapper(new RequireReasonForCLRItemCollection())
			{
				RequireReasonForCLR = required
			};

			return requireReasonWrapper;
		}

		static ZGridColumnInfo GetColumn(ZGrid grid, string columnName)
		{
			foreach (var columnStyle in grid.ColumnStyles)
			{
				if (columnStyle is ZGridColumnInfo gridColumnStyle && gridColumnStyle.ColumnName == columnName)
				{
					return gridColumnStyle;
				}
			}

			return null;
		}

		public class HVLVDpsResultForm_ForTest : HVLVDpsResultForm
		{
			public HVLVDpsResultForm_ForTest(List<DpsResponseWithScreeningParty> parties, BusinessObjectFactory factory) : base(parties, factory)
			{
			}

			public HVLVDpsResultForm_ForTest(HVLVDpsResult result) : base(result)
			{
			}

			public ZGrid GridScreenedParties => this.GetControl<ZGrid>("gridScreenedParties");

			public ZDisplayGrid GridMatchedNames => this.GetControl<ZDisplayGrid>("gridMatchedNames");

			public ZDisplayGrid GridMatchedAddresses => this.GetControl<ZDisplayGrid>("gridMatchedAddresses");

			public ZDisplayGrid GridMatchedRegistrationCodes => this.GetControl<ZDisplayGrid>("gridMatchedRegistrationCodes");

			public void ClickMatchButton()
			{
				MatchButton.PerformClick();
			}

			public void ClickClearButton()
			{
				ClearButton.PerformClick();
			}

			ZButton MatchButton => this.GetControl<ZButton>("matchButton");

			ZButton ClearButton => this.GetControl<ZButton>("clearButton");
		}

		Color GetGridRowColour(ZGrid grid, object objectAtRow)
		{
			var handler = (EventHandler<ColourDecidingEventArgs>)typeof(ZGrid).GetField("ColourDeciding", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(grid);
			var e = new ColourDecidingEventArgs(objectAtRow);
			handler(null, e);
			return e.Colour;
		}
	}
}
