using System;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ZOrganisationControlTest : ZControlBaseTestCase<ZOrganisationControlForTesting>
	{
		[RequiresSTA]
		public void TestAddressShowCustomsAddress()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_Code = "ORGCODE";
			header.OH_RL_NKClosestPort = "CNNJ";
			var customsAddress = header.Addresses.AddNew();
			customsAddress.OA_Address1 = "KNZ Test";
			customsAddress.OA_Address2 = "nj yuhua";
			customsAddress.OA_City = "nj";
			customsAddress.OA_State = "js";
			customsAddress.OA_PostCode = "2100";
			customsAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			customsAddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);

			var mainAddress = header.Addresses.AddNew();
			mainAddress.OA_Address1 = "main address";
			mainAddress.OA_RL_NKRelatedPortCode = "NZAKL";
			mainAddress.AddAddressType(OrgAddressType.Office);

			Control.AddressType = AddressType.CustomsAddress;
			Control.OrganisationForBinding = header;
			var expectedOrgAddressFormatted = string.Join(System.Environment.NewLine, "KNZ Test", "nj yuhua", "nj js 2100", "Australia").ToUpper();
			AssertMultilineASCIIEquals(expectedOrgAddressFormatted, Control.OrgAddressFormatted);
			AssertEquals(Control.OrgAddressFormatted, Control.AddressLabel.Text);
		}

		[RequiresSTA]
		public void TestAddressShowMain()
		{
			BusinessObject org = (BusinessObject)new BusinessObjectFactory().New<IOrgHeader>();
			org[OrgHeaderSchema.OH_RL_NKClosestPort] = "AUSYD";
			org[OrgHeaderSchema.OH_FullName] = "Some Company Name";

			BusinessObject address = (BusinessObject)org.GetType().GetProperty("MainAddress").GetValue(org, null);
			address[OrgAddressSchema.OA_OH] = org.PK;
			address[OrgAddressSchema.OA_Address1] = "Bourton  Hello  noone   ";
			address[OrgAddressSchema.OA_Address2] = "26 Myrtle   Street";
			address[OrgAddressSchema.OA_City] = "Prospect    Blacktown";
			address[OrgAddressSchema.OA_State] = "NSW";
			address[OrgAddressSchema.OA_PostCode] = "2149";

			Control.OrganisationForBinding = (IOrgHeader)org;

			var expectedOrgAddressFormatted = string.Join(System.Environment.NewLine, "Bourton Hello noone", "26 Myrtle Street", "Prospect Blacktown NSW 2149", "Australia").ToUpper();
			AssertMultilineASCIIEquals(expectedOrgAddressFormatted, Control.OrgAddressFormatted);
			AssertEquals(Control.OrgAddressFormatted, Control.AddressLabel.Text);
		}

		[RequiresSTA]
		public void TestAddressWithNoAddressLine2()
		{
			BusinessObject org = (BusinessObject)new BusinessObjectFactory().New<IOrgHeader>();
			org[OrgHeaderSchema.OH_RL_NKClosestPort] = "AUSYD";
			org[OrgHeaderSchema.OH_FullName] = "Some Company Name";

			BusinessObject address = (BusinessObject)org.GetType().GetProperty("MainAddress").GetValue(org, null);
			address[OrgAddressSchema.OA_OH] = org.PK;
			address[OrgAddressSchema.OA_Address1] = "26 Myrtle   Street";
			address[OrgAddressSchema.OA_Address2] = "";
			address[OrgAddressSchema.OA_City] = "Prospect    Blacktown";
			address[OrgAddressSchema.OA_State] = "NSW";
			address[OrgAddressSchema.OA_PostCode] = "2149";

			Control.OrganisationForBinding = (IOrgHeader)org;

			var expectedOrgAddressFormatted = string.Join(System.Environment.NewLine, "26 Myrtle Street", "Prospect Blacktown NSW 2149", "Australia").ToUpper();
			AssertMultilineASCIIEquals(expectedOrgAddressFormatted, Control.OrgAddressFormatted);
			AssertEquals(Control.OrgAddressFormatted, Control.AddressLabel.Text);
		}

		public void TestOrgAddressLabelIsTruncated_WhenAddressFormattedTooLong()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "RN";
			country.RN_Desc = "Country Country Country Country";

			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = "RW";
			state.RW_RN_NKCountryCode = "RN";
			state.RW_Description = "State";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = org.MainAddress;
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "Additional info Additional info Additional info";
			orgAddress.OA_Address1 = "Address1 Address1 Address1 Address1 Address1";
			orgAddress.OA_Address2 = "Address2 Address2 Address2 Address2 Address2";
			orgAddress.OA_City = "City City City City City City City";
			orgAddress.OA_State = "RW";
			orgAddress.OA_PostCode = "2222";
			orgAddress.OA_RN_NKCountryCode = "RN";

			Control.OrganisationForBinding = org;

			var fullOrgAddressLabelLength = string.Join(System.Environment.NewLine,
				"Additional info Additional info Additional info",
				"Address1 Address1 Address1 Address1 Address1",
				"Address2 Address2 Address2 Address2 Address2",
				"City City City City City City City RW 2222",
				"Country Country Country Country").Length;

			Assert("Address Label is truncated", fullOrgAddressLabelLength > Control.AddressLabel.Text.Length);
		}

		public void TestOrgAddressLabelToolTipShouldAppear_WhenAddressFormattedTooLong()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "RN";
			country.RN_Desc = "Country Country Country Country";

			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = "RW";
			state.RW_RN_NKCountryCode = "RN";
			state.RW_Description = "State";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = org.MainAddress;
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "Additional info Additional info Additional info";
			orgAddress.OA_Address1 = "Address1 Address1 Address1 Address1 Address1";
			orgAddress.OA_Address2 = "Address2 Address2 Address2 Address2 Address2";
			orgAddress.OA_City = "City City City City City City City";
			orgAddress.OA_State = "RW";
			orgAddress.OA_PostCode = "2222";
			orgAddress.OA_RN_NKCountryCode = "RN";
			Control.OrganisationForBinding = org;

			AssertEquals(true, Control.IsAddressLabelToolTipRequired);
		}

		public void TestOrgAddressLabelToolTipShouldNotAppear_WhenAddressFormattedFits()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "RN";
			country.RN_Desc = "Country";

			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = "RW";
			state.RW_RN_NKCountryCode = "RN";
			state.RW_Description = "State";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = org.MainAddress;
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "Additional info";
			orgAddress.OA_Address1 = "Address1 Address1";
			orgAddress.OA_Address2 = "Address2 Address2";
			orgAddress.OA_City = "City City";
			orgAddress.OA_State = "RW";
			orgAddress.OA_PostCode = "2222";
			orgAddress.OA_RN_NKCountryCode = "RN";
			Control.OrganisationForBinding = org;

			AssertEquals(false, Control.IsAddressLabelToolTipRequired);
		}

		[RequiresSTA]
		public void TestOrgAddressLabelToolTipCaptionIsCorrect_WhenAddressFormattedTooLong()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "RN";
			country.RN_Desc = "Country Country Country Country";

			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = "RW";
			state.RW_RN_NKCountryCode = "RN";
			state.RW_Description = "State";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = org.MainAddress;
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "Additional info Additional info Additional info";
			orgAddress.OA_Address1 = "Address1 Address1 Address1 Address1 Address1";
			orgAddress.OA_Address2 = "Address2 Address2 Address2 Address2 Address2";
			orgAddress.OA_City = "City City City City City City City";
			orgAddress.OA_State = "RW";
			orgAddress.OA_PostCode = "2222";
			orgAddress.OA_RN_NKCountryCode = "RN";
			Control.OrganisationForBinding = org;

			var expectedOrgAddressLabelTooltip = string.Join(System.Environment.NewLine,
				"Additional info Additional info Additional info",
				"Address1 Address1 Address1 Address1 Address1",
				"Address2 Address2 Address2 Address2 Address2",
				"City City City City City City City RW 2222",
				"Country Country Country Country").ToUpper();

			Control.AddressLabel_MouseHover(this, EventArgs.Empty);

			AssertMultilineASCIIEquals("Address Label tooltip is correct", expectedOrgAddressLabelTooltip, ToolTipService.GetToolTip(Control.AddressLabel));
		}

		public void TestOrgAddressLabelToolTipRefreshesCorrectly_WhenAddressChanges()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "RN";
			country.RN_Desc = "Country Country Country Country";

			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = "RW";
			state.RW_RN_NKCountryCode = "RN";
			state.RW_Description = "State";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = org.MainAddress;
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "Additional info Additional info Additional info";
			orgAddress.OA_Address1 = "Address1 Address1 Address1 Address1 Address1";
			orgAddress.OA_Address2 = "Address2 Address2 Address2 Address2 Address2";
			orgAddress.OA_City = "City City City City City City City";
			orgAddress.OA_State = "RW";
			orgAddress.OA_PostCode = "2222";
			orgAddress.OA_RN_NKCountryCode = "RN";
			Control.OrganisationForBinding = org;

			var expectedOrgAddressLabelTooltip = string.Join(System.Environment.NewLine,
				"Additional info Additional info Additional info",
				"Address1 Address1 Address1 Address1 Address1",
				"Address2 Address2 Address2 Address2 Address2",
				"City City City City City City City RW 2222",
				"Country Country Country Country").ToUpper();

			Control.AddressLabel_MouseHover(this, EventArgs.Empty);
			AssertMultilineASCIIEquals("Prerequisite", expectedOrgAddressLabelTooltip, ToolTipService.GetToolTip(Control.AddressLabel));

			orgAddress.PrimaryOrgAddressAdditionalInfo.OAI_AdditionalInfo = "some additional info here";
			orgAddress.OA_Address1 = "Unit 37 / 1 address street, Hydepark";
			orgAddress.OA_Address2 = "";
			orgAddress.OA_City = "City of angels";
			orgAddress.OA_State = "RW";
			orgAddress.OA_PostCode = "4444";
			orgAddress.OA_RN_NKCountryCode = "RN";

			var expectedOrgAddressLabelTooltipAfterChanging = string.Join(System.Environment.NewLine,
				"some additional info here",
				"Unit 37 / 1 address street, Hydepark",
				"City of angels RW 4444",
				"Country Country Country Country").ToUpper();

			Control.AddressLabel_MouseHover(this, EventArgs.Empty);
			AssertMultilineASCIIEquals("Address Label tooltip caption refreshes when address changes", expectedOrgAddressLabelTooltipAfterChanging, ToolTipService.GetToolTip(Control.AddressLabel));
		}

		[RequiresSTA]
		public void TestImagesNotAssignedUntilUserIdle()
		{
			AssertNull("Images not created or assigned until user idle, for performance", Control.ContactsLink.Image);
			AssertNull("Images not created or assigned until user idle, for performance", Control.AddressesLink.Image);
			UserIdleWorker.Flush();
			AssertNotNull("Images created and assigned after the user is idle", Control.ContactsLink.Image);
			AssertNotNull("Images created and assigned after the user is idle", Control.AddressesLink.Image);
		}

		public void TestFetchHints()
		{
			DummyBusinessObject bizO = DummyBusinessObject.New(Factory);
			Control.BindTo = "PK";
			Control.BindToOrganisations = "ABC";
			AssertEquals(0, Factory.ActiveFetchHintsForTable(OrgHeaderSchema.Constants.TableName));
			((IFetchHintGenerator)Control).AddFetchHint(bizO, "");
			AssertEquals(1, Factory.ActiveFetchHintsForTable(OrgHeaderSchema.Constants.TableName));
		}

		[RequiresSTA]
		public void TestCaptionRenderingSupport()
		{
			AssertEquals("Caption rendering enabled", true, Control.CaptionRenderingEnabled);
		}

		[RequiresSTA]
		public void TestExtensions()
		{
			Assert(typeof(IExtendedControl).IsAssignableFrom(Control.GetType()));

			AssertEquals(1, Control.OrganisationFindBox.Extensions.Count());
			AssertNotNull(Control.OrganisationFindBox.GetExtension<INotificationExtension>());
		}

		[RequiresSTA]
		public void TestNoExceptionThrowWhenOrganizationIsNullAndCallOrgAddressFormatted()
		{
			Control.OrganisationForBinding = null;
			AssertNoExceptionThrown(() =>
			{
				AssertEquals("*NO ADDRESS FOUND*", Control.OrgAddressFormatted);
			});
		}

		public void TestSetAdditionalAddressTab()
		{
			Control.OrganisationForBinding = null;
			AssertEquals("* NO ORGANIZATION IS SELECTED", Control.AdditionalAddressInfoControl.AdditionalInfoLabel.Text);
			AssertEquals(false, Control.AdditionalAddressInfoControl.AdditionalInfoLabel.Enabled);

			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_Code = "ORGCODE";
			header.OH_RL_NKClosestPort = "CNNJ";
			Control.OrganisationForBinding = header;
			AssertEquals(string.Empty, Control.AdditionalAddressInfoControl.AdditionalInfoLabel.Text);
			AssertEquals(true, Control.AdditionalAddressInfoControl.AdditionalInfoLabel.Enabled);
		}

		#region IDataBoundControl

		[RequiresSTA]
		public void TestDataSourceType()
		{
			using (ZOrganisationControl control = new ZOrganisationControl())
			{
				AssertEquals(typeof(ZGuid), control.DataSourceType);
			}
		}

		#endregion

		public void TestShouldSerializeCaptionResourceStringForDesigner()
		{
			var methodName = $"ShouldSerialize{nameof(ZOrganisationControl.CaptionResourceString)}";
			AssertNotNull($"{nameof(ZOrganisationControl)} should have the method '{methodName}' defined for visual studio designer.", typeof(ZOrganisationControl).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic));
		}

		public void TestCorrectBindToOrgList()
		{
			Control.BindToOrganisations = "BindToOrganisations";
			Control.CorrectBindToOrgList("Prefix.");
			AssertEquals("Prefix.BindToOrganisations", Control.BindToOrganisations);
		}

		public void TestCorrectBindToOrgListDoesNotAppendPrefixIfAlreadyAppended()
		{
			Control.BindToOrganisations = "Prefix.BindToOrganisations";
			Control.CorrectBindToOrgList("Prefix.");
			AssertEquals("Prefix.BindToOrganisations", Control.BindToOrganisations);
		}

		#region Implementation

		protected override bool UsesControlDataBindings
		{
			get { return false; }
		}

		#endregion
	}
}
