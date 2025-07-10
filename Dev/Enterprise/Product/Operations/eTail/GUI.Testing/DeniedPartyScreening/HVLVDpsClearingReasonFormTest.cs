using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.eTail.Business.DeniedPartyScreening;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.DeniedPartyScreening.Common.DeniedPartyConstants;

namespace Enterprise.eTail.GUI.Testing
{
	[TestedType(typeof(HVLVDpsClearingReasonForm))]
	public class HVLVDpsClearingReasonFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			using var form = CreateClearingReasonForm();
			form.Show();
			AssertEquals("New Clearing Reason", form.Text);
		}

		public void TestSaveButtonClick_WhenNoErrors_ShouldReturnsOK()
		{
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForClearing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateRequireReasonWrapper(true)))
			using (var form = CreateClearingReasonForm())
			{
				form.Show();

				var businessEntity = form.BusinessEntity as HVLVDpsClearingReason;
				businessEntity.ClearingReason = "OTH";
				businessEntity.ClearingReasonText = "A clearing reason";

				var saveButton = form.Controls.Find("saveButton", true).OfType<ZButton>().First();
				saveButton.PerformClick();

				AssertEquals(DialogResult.OK, form.DialogResult);
				Assert(form.IsDisposed);
			}
		}

		public void TestSaveButtonClick_WhenHasErrors_ShouldNotClose()
		{
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForClearing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateRequireReasonWrapper(true)))
			using (var form = CreateClearingReasonForm())
			{
				form.Show();

				var saveButton = form.Controls.Find("saveButton", true).OfType<ZButton>().First();
				saveButton.PerformClick();

				AssertEquals(DialogResult.None, form.DialogResult);
				Assert(!form.IsDisposed);
			}
		}

		public void TestCancelButtonClick()
		{
			using var form = CreateClearingReasonForm();
			form.Show();
			form.CancelButton.PerformClick();

			AssertEquals(DialogResult.Cancel, form.DialogResult);
			Assert(form.IsDisposed);
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return CreateClearingReasonForm();
		}

		#endregion

		HVLVDpsClearingReasonForm CreateClearingReasonForm()
		{
			var dpsMatches = new HVLVDpsMatch[] {
				new HVLVDpsMatch(CreateDpsResponseWithScreeningParty(), Factory) { Cleared = true, ClearingReason = "OTH", ClearingReasonText = "A clearing reason" },
			};

			return new HVLVDpsClearingReasonForm(new HVLVDpsClearingReason(dpsMatches, Factory));
		}

		DpsResponseWithScreeningParty CreateDpsResponseWithScreeningParty()
		{
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = Guid.NewGuid(), ProfileNotes = Array.Empty<byte>(), TypeOfEntity = ScreeningNameTypes.Person },
			};

			var header = Factory.NewWithValidTestData<OrgHeader>();
			var party = new ScreeningParty(header, string.Empty, header);
			var responseWithParty = new DpsResponseWithScreeningParty(party, new DpsResponse { Profiles = profiles }, new DpsRequestHeaderWithAddressMatching());
			return responseWithParty;
		}

		RequireReasonForCLRWrapper CreateRequireReasonWrapper(bool required = false)
		{
			var requireReasonWrapper = new RequireReasonForCLRWrapper(new RequireReasonForCLRItemCollection())
			{
				RequireReasonForCLR = required
			};

			return requireReasonWrapper;
		}
	}
}
