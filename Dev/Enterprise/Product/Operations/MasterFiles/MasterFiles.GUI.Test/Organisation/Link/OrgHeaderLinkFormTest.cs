using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(OrgHeaderLinkForm))]
	sealed class OrgHeaderLinkFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ABC";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "XYZ";

			var mockUserNotification = new Mock<IUserNotification>();

			return new OrgHeaderLinkForm(
				new OrgHeaderLink(Factory, mockUserNotification.Object, org1, org2));
		}

		[RequiresSTA]
		public void TestFailingSecurityCheck()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TESTPARENT";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TESTCHILD";
			Factory.Save();

			var previousModifyRelatedPartiesValue = Env.Security.OrgDetailsModifyRelatedParties.IsAllowed;
			var previousNewModifyRelatedPartiesValue = Env.Security.OrgDetailsNewModifyRelatedParties.IsAllowed;

			var mockUserNotification = new Mock<IUserNotification>();

			try
			{
				var link = new OrgHeaderLink(Factory, mockUserNotification.Object, org1, org2);
				using (var form = new OrgHeaderLinkForm(link))
				{
					form.Show();
					form.Organisation1ButtonForTest.Checked = true;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

					UnitTestUserNotification.Instance.ClearMessages();
					Env.Security.OrgDetailsModifyRelatedParties.IsAllowed = false;
					form.SaveAndCloseButton.PerformClick();
					var query = new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, org1.PK);
					query.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, org2.PK);
					var result = Factory.Load<OrgRelatedParty>(query);
					AssertEquals("Relationship should not be added to table", 0, result.Length);
					AssertEquals(Env.Security.OrgDetailsModifyRelatedParties.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					Env.Security.OrgDetailsModifyRelatedParties.IsAllowed = true;
					form.SaveAndCloseButton.PerformClick();
					query = new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, org1.PK);
					query.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, org2.PK);
					result = Factory.Load<OrgRelatedParty>(query);
					AssertEquals("Relationship should added to table", 1, result.Length);
					AssertEquals("Organizations successfully linked.", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				var org3 = Factory.NewWithValidTestData<OrgHeader>();
				org3.OH_Code = "TESTORG3";
				link = new OrgHeaderLink(Factory, mockUserNotification.Object, org3, org1);
				using (var form = new OrgHeaderLinkForm(link))
				{
					form.Show();
					form.Organisation1ButtonForTest.Checked = true;

					UnitTestUserNotification.Instance.ClearMessages();
					Env.Security.OrgDetailsNewModifyRelatedParties.IsAllowed = false;
					form.SaveAndCloseButton.PerformClick();
					var query = new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, org3.PK);
					query.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, org1.PK);
					var result = Factory.Load<OrgRelatedParty>(query);
					AssertEquals("Relationship should not be added to table", 0, result.Length);
					AssertEquals(Env.Security.OrgDetailsNewModifyRelatedParties.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					Env.Security.OrgDetailsNewModifyRelatedParties.IsAllowed = true;
					form.SaveAndCloseButton.PerformClick();
					query = new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, org3.PK);
					query.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, org1.PK);
					result = Factory.Load<OrgRelatedParty>(query);
					AssertEquals("Relationship should added to table", 1, result.Length);
					AssertEquals("Organizations successfully linked.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				Env.Security.OrgDetailsModifyRelatedParties.IsAllowed = previousModifyRelatedPartiesValue;
				Env.Security.OrgDetailsNewModifyRelatedParties.IsAllowed = previousNewModifyRelatedPartiesValue;
			}
		}

		public void TestOtherOrgIsNull()
		{
			var previousModifyRelatedPartiesValue = Env.Security.OrgDetailsModifyRelatedParties.IsAllowed;

			try
			{
				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				org1.OH_Code = "TESTPARENT";
				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				org2.OH_Code = "TESTCHILD";
				Factory.Save();

				Env.Security.OrgDetailsModifyRelatedParties.IsAllowed = true;

				var mockUserNotification = new Mock<IUserNotification>();

				var link = new OrgHeaderLink(Factory, mockUserNotification.Object, org1, org2);

				using (var form = new OrgHeaderLinkForm(link))
				{
					form.Show();
					form.OtherButtonForTest.Select();
					var result = form.TryAddRelatedParty(org1, org2);

					Assert("Expect an error", !result);
					mockUserNotification.Verify(mock => mock.ShowError(It.IsAny<string>()), Times.Once);
					mockUserNotification.Verify(mock => mock.ShowError("Please choose an organization from the search box."), Times.Once);
				}
			}
			finally
			{
				Env.Security.OrgDetailsModifyRelatedParties.IsAllowed = previousModifyRelatedPartiesValue;
			}
		}

		[RequiresSTA]
		public void TestDialogResult()
		{
			var previousModifyRelatedPartiesValue = Env.Security.OrgDetailsModifyRelatedParties.IsAllowed;

			try
			{
				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				org1.OH_Code = "TESTPARENT";
				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				org2.OH_Code = "TESTCHILD";
				Factory.Save();

				Env.Security.OrgDetailsModifyRelatedParties.IsAllowed = true;
				var mockUserNotification = new Mock<IUserNotification>();
				var link = new OrgHeaderLink(Factory, mockUserNotification.Object, org1, org2);
				using (var form = new OrgHeaderLinkForm(link))
				{
					form.Show();
					form.OtherButtonForTest.Select();
					form.SaveAndCloseButton.PerformClick();
					AssertEquals("Should return DialogResult.None when not save successfully.", form.DialogResult, DialogResult.None);

					form.Organisation1ButtonForTest.Select();
					form.SaveAndCloseButton.PerformClick();
					AssertEquals("Should return DialogResult.OK when save successfully.", form.DialogResult, DialogResult.OK);
				}
			}
			finally
			{
				Env.Security.OrgDetailsModifyRelatedParties.IsAllowed = previousModifyRelatedPartiesValue;
			}
		}

		public void TestNotSaveAnyRelatedPartyWhenAnyVerificationFailed()
		{
			var previousModifyRelatedPartiesValue = Env.Security.OrgDetailsModifyRelatedParties.IsAllowed;

			try
			{
				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				org1.OH_Code = "TESTPARENT";
				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				org2.OH_Code = "TESTCHILD";
				var org3 = Factory.NewWithValidTestData<OrgHeader>();
				org3.OH_Code = "TESTOTHER";
				Factory.Save();

				Env.Security.OrgDetailsModifyRelatedParties.IsAllowed = true;
				var mockUserNotification = new Mock<IUserNotification>();
				var link = new OrgHeaderLink(Factory, mockUserNotification.Object, org1, org2);
				using (var form = new OrgHeaderLinkForm(link))
				{
					form.Show();

					form.Organisation1ButtonForTest.Select();
					var org1RelatedParty = CreateRelatedParty(org2.PK, org1.PK);
					form.SaveAndCloseButton.PerformClick();
					AssertNoRelatedPartyCreated(org1.PK);

					org1RelatedParty.Delete();
					form.Organisation2ButtonForTest.Select();
					CreateRelatedParty(org1.PK, org2.PK);
					form.SaveAndCloseButton.PerformClick();
					AssertNoRelatedPartyCreated(org2.PK);

					form.OtherButtonForTest.Select();
					form.OrganisationFindBoxForTest.CodeBox.Text = org3.OH_Code;
					form.SaveAndCloseButton.PerformClick();
					AssertNoRelatedPartyCreated(org3.PK);
				}
			}
			finally
			{
				Env.Security.OrgDetailsModifyRelatedParties.IsAllowed = previousModifyRelatedPartiesValue;
			}
		}

		OrgRelatedParty CreateRelatedParty(ZGuid pKRelatedParty, ZGuid pKParent)
		{
			var orgRelatedParty = Factory.New<OrgRelatedParty>();
			orgRelatedParty.PR_OH_Parent = pKParent;
			orgRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			orgRelatedParty.PR_OH_RelatedParty = pKRelatedParty;

			return orgRelatedParty;
		}

		[RequiresSTA]
		public void TestConcurrencyErrorWhenSaving()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TESTPARENT";
			org1.MainAddress.ValidationStatus = AddressValidationStatus.ToBeVerified;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TESTCHILD";
			org2.MainAddress.ValidationStatus = AddressValidationStatus.ToBeVerified;
			Factory.Save();

			var query = new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, org1.PK);
			query.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, org2.PK);

			var previousModifyRelatedPartiesValue = Env.Security.OrgDetailsModifyRelatedParties.IsAllowed;
			var previousNewModifyRelatedPartiesValue = Env.Security.OrgDetailsNewModifyRelatedParties.IsAllowed;
			var mockUserNotification = new Mock<IUserNotification>();

			try
			{
				var link = new OrgHeaderLink(Factory, mockUserNotification.Object, org1, org2);

				using (var form = new OrgHeaderLinkForm(link))
				{
					form.Show();
					form.Organisation1ButtonForTest.Checked = true;
					Env.Security.OrgDetailsModifyRelatedParties.IsAllowed = true;
					Env.Security.OrgDetailsNewModifyRelatedParties.IsAllowed = true;

					var anohterFactory = new BusinessObjectFactory();
					anohterFactory.RefreshEnabled = false;
					var org1MainAddress = anohterFactory.Load<OrgAddress>(org1.MainAddress.PK);
					org1MainAddress.ValidationStatus = AddressValidationStatus.Invalid;
					var org2MainAddress = anohterFactory.Load<OrgAddress>(org2.MainAddress.PK);
					org2MainAddress.ValidationStatus = AddressValidationStatus.Invalid;
					anohterFactory.Save();

					org1.MainAddress.ValidationStatus = AddressValidationStatus.ManuallyVerified;
					org2.MainAddress.ValidationStatus = AddressValidationStatus.ManuallyVerified;

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.ClearMessages();
					form.SaveAndCloseButton.PerformClick();

					Assert(form.SaveAndCloseButton.Enabled);
					Assert(form.CancelLinkingButton.Enabled);
					AssertEquals("Relationship should not be added to table", 0, Factory.Load<OrgRelatedParty>(query).Length);
					AssertContains("Concurrency error happend.", "While you have been working with this form", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					form.SaveAndCloseButton.PerformClick();
					AssertEquals("Relationship should be added to table", 1, Factory.Load<OrgRelatedParty>(query).Length);
					AssertEquals("Organizations successfully linked.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				Env.Security.OrgDetailsModifyRelatedParties.IsAllowed = previousModifyRelatedPartiesValue;
				Env.Security.OrgDetailsNewModifyRelatedParties.IsAllowed = previousNewModifyRelatedPartiesValue;
			}
		}

		void AssertNoRelatedPartyCreated(ZGuid pKRelatedParty)
		{
			var query = new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, pKRelatedParty);
			var relatedParties = Factory.Load<OrgRelatedParty>(query);
			Assert("Should not related", !relatedParties.Any());
		}
	}
}
