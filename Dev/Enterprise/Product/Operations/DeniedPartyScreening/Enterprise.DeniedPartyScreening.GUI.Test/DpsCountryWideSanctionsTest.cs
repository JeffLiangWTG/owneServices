using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Modules;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	public class DpsCountryWideSanctionsTest : TestCaseWithFactory
	{
		public void TestSanctionsFormActionsMenuItems_ShouldExistWithVisibility()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			Factory.Save();

			using (var form = new RefCountryForm(country))
			{
				CombineAssertions(() =>
				{
					var menuItem = GetActionsMenuItems(form);
					AssertEquals("Country Sanctions", menuItem.CountrySanctions.Text);
					AssertEquals("Mark as Sanctioned", menuItem.MarkAsSanctioned.Text);
					AssertEquals("Remove Sanctions", menuItem.RemoveSanctions.Text);
					AssertEquals("Screen", menuItem.Screen.Text);
					AssertEquals(true, menuItem.Screen.Visible);
					AssertEquals(true, menuItem.MarkAsSanctioned.Visible);
					AssertEquals(false, menuItem.RemoveSanctions.Visible);
				});
			}
		}

		public void TestSanctionsModuleActionsMenuItems_ShouldExistWithVisibility()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			Factory.Save();

			using (var form = new RefCountryForm(country))
			using (var module = new RefCountryModuleForTest())
			{
				form.Controls.Add(module.EmbeddedControl);

				var menuItem = module.FormActionMenu.FindByText("Screen", true);

				CombineAssertions(() =>
				{
					AssertNotNull(menuItem);
					AssertEquals("Screen", menuItem.Text);
					AssertEquals(true, menuItem.Visible);
				});
			}
		}

		public void TestSanctionsRecordNotSave_ShouldShowMessage()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();

			using (var form = new RefCountryForm(country))
			{
				var menuItem = GetActionsMenuItems(form);

				UnitTestUserNotification.Instance.ClearMessages();
				menuItem.MarkAsSanctioned.PerformClick();

				AssertEquals("Please save the form before Denied Party Screening or Performing Sanctions", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestUserDecisionsToSanctionsWithInvalidSecurityRights_ShouldShowMessage()
		{
			var securityCore = new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			securityCore.CountriesManageSanctions.IsAllowed = false;

			var country = Factory.NewWithValidTestData<RefCountry>();
			Factory.Save();

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			using (var form = new RefCountryForm(country))
			{
				var menuItem = GetActionsMenuItems(form);

				AssertInvalidSecurityRights(menuItem.MarkAsSanctioned, securityCore.CountriesManageSanctions.DisplayTextPathToSecurityRight);
			}

			void AssertInvalidSecurityRights(MenuItem menuItem, string securityPath)
			{
				UnitTestUserNotification.Instance.ClearMessages();

				menuItem.PerformClick();

				var expectedMessage = string.Format(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{0}", securityPath);

				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestUserDecisionsToMarkAsSanctionedWithComplianceRisk_ShouldShowMessage()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "RF";
			country.RN_Desc = "Russian Federation";
			Factory.Save();

			using (var form = new RefCountryForm(country))
			{
				UnitTestUserNotification.Instance.ClearMessages();

				var menuItem = GetActionsMenuItems(form);
				menuItem.MarkAsSanctioned.PerformClick();

				var expectedMessage = string.Format(@"You are about to apply sanctions to {0}.

Doing so will cause all entities with this country present to be blocked by Denied Party Screening. For more information please refer to the Denied Party Screening Reference Guide.

This operation must only be done by those who understand the impact it will have on their compliance process.", country.RN_Desc);

				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestUserDecisionsToRemoveSanctionsWithComplianceRisk_ShouldShowMessage()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "RF";
			country.RN_Desc = "Russian Federation";
			country.RN_IsSanctioned = true;
			Factory.Save();

			using (var form = new RefCountryForm(country))
			{
				UnitTestUserNotification.Instance.ClearMessages();

				var menuItem = GetActionsMenuItems(form);
				menuItem.RemoveSanctions.PerformClick();

				var expectedMessage = string.Format(@"You are about to remove sanctions applied to {0}.

Doing so will free entities with this country present from being blocked by Denied Party Screening. For more information please refer to the Denied Party Screening Reference Guide.

This operation must only be done by those who understand the impact it will have on their compliance process.", country.RN_Desc);

				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestUserDecisionsToMarkAsSanctioned_ShouldUpdateSanctionStatusWithCreatedLogs()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			Factory.Save();

			using (var form = new RefCountryForm(country))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				var menuItem = GetActionsMenuItems(form);
				menuItem.MarkAsSanctioned.PerformClick();

				AssertEquals(true, country.RN_IsSanctioned);
				AssertEquals(false, menuItem.MarkAsSanctioned.Visible);
				AssertEquals(true, menuItem.RemoveSanctions.Visible);
				AssertDeniedPartyScreeningLogs(country, DeniedPartyConstants.LogsScreeningStatus.UserDecisionsMarkAsSanctioned, "User's Decisions to Mark as Sanctioned");
			}
		}

		public void TestUserDecisionsToRemoveSanctions_ShouldUpdateSanctionStatusWithCreatedLogs()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_IsSanctioned = true;
			Factory.Save();

			using (var form = new RefCountryForm(country))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				var menuItem = GetActionsMenuItems(form);
				menuItem.RemoveSanctions.PerformClick();

				AssertEquals(false, country.RN_IsSanctioned);
				AssertEquals(true, menuItem.MarkAsSanctioned.Visible);
				AssertEquals(false, menuItem.RemoveSanctions.Visible);
				AssertDeniedPartyScreeningLogs(country, DeniedPartyConstants.LogsScreeningStatus.UserDecisionsRemoveSanctions, "User's Decisions to Remove Sanctions");
			}
		}

		public void TestScreenOnForm_WhenCancelledWithNoPotentialMatches_ShouldCreateLogStatusDPD()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_IsSanctioned = true;
			country.RN_Desc = "ZZ";
			Factory.Save();

			using (var form = new RefCountryForm(country))
			using (ObjectFactory.Substitute<IDpsManager>(new DpsManagerForTest()))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				var menuItem = GetActionsMenuItems(form);
				menuItem.Screen.PerformClick();

				Assert(country.RN_IsSanctioned);
				AssertDeniedPartyScreeningLogs(country, DeniedPartyConstants.LogsScreeningStatus.ScreenedCanceled, string.Empty);
			}
		}

		public void TestScreenOnForm_WhenCancelledWithPotentialMatches_ShouldCreateLogStatusDPD()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_IsSanctioned = false;
			Factory.Save();

			using (var form = new RefCountryForm(country))
			using (ObjectFactory.Substitute<IDpsManager>(new DpsManagerResultForTest(country)))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				var menuItem = GetActionsMenuItems(form);
				menuItem.Screen.PerformClick();

				Assert(!country.RN_IsSanctioned);
				AssertDeniedPartyScreeningLogs(country, DeniedPartyConstants.LogsScreeningStatus.ScreenedCanceled, string.Empty);
			}
		}

		public void TestScreenOnModule_WhenCancelledMatches_ShouldCreateLogStatusDPD()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			Factory.Save();
			using (var module = new RefCountryModuleForTestWithBussinessObjects(country))
			using (ObjectFactory.Substitute<IDpsManager>(new DpsManagerResultForTest(country)))
			using (var form = new ZFormWithIMainForm())
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var menuItem = module.FormActionMenu.FindByText("Screen", true);
				menuItem.PerformClick();

				Assert(!country.RN_IsSanctioned);
				AssertDeniedPartyScreeningLogs(country, DeniedPartyConstants.LogsScreeningStatus.ScreenedCanceled, string.Empty);
			}
		}

		public void TestScreen_ShouldNotCreateEdiMessage()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			Factory.Save();

			using (var form = new RefCountryForm(country))
			using (ObjectFactory.Substitute<IDpsManager>(new DpsManagerForTest()))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				var menuItem = GetActionsMenuItems(form);
				menuItem.Screen.PerformClick();

				var filter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.DPSRequestMessage);
				filter.AddToFilter(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.JDC);
				filter.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.ScreeningRequest);

				var ediMessages = Factory.Load<DpsEDIMessage>(filter);
				Assert("Should not create EdiMessage", ediMessages.Length == 0);
			}
		}

		public void TestSanctionsMenuItem_WhenMultipleFormsOpenedIsNotDisplayingProperly_ShouldDisplayMenuPrecisely()
		{
			var country1 = Factory.NewWithValidTestData<RefCountry>();
			country1.RN_Code = "X1";
			country1.RN_Desc = "Test Country For X1";
			var country2 = Factory.NewWithValidTestData<RefCountry>();
			country2.RN_Code = "X2";
			country2.RN_Desc = "Test Country For X2";
			Factory.Save();
			using (var countryForm1 = new RefCountryForm(country1))
			using (var countryForm2 = new RefCountryForm(country2))
			{
				UnitTestUserNotification.Instance.ClearMessages();
				var menuItem = GetActionsMenuItems(countryForm1);
				var menuItem2 = GetActionsMenuItems(countryForm2);
				CombineAssertions(() =>
				{
					menuItem.MarkAsSanctioned.PerformClick();
					AssertEquals(false, menuItem.MarkAsSanctioned.Visible);
					AssertEquals(true, menuItem.RemoveSanctions.Visible);
					AssertEquals(true, menuItem2.MarkAsSanctioned.Visible);
					AssertEquals(false, menuItem2.RemoveSanctions.Visible);
					menuItem2.MarkAsSanctioned.PerformClick();
					AssertEquals(false, menuItem2.MarkAsSanctioned.Visible);
					AssertEquals(true, menuItem2.RemoveSanctions.Visible);
					AssertEquals(false, menuItem.MarkAsSanctioned.Visible);
					AssertEquals(true, menuItem.RemoveSanctions.Visible);
				});
			}
		}

		(MenuItem CountrySanctions, MenuItem MarkAsSanctioned, MenuItem RemoveSanctions, MenuItem Screen) GetActionsMenuItems(ZForm form)
		{
			return (form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("Country Sanctions", true),
				form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("Mark as Sanctioned", true),
				form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("Remove Sanctions", true),
				form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("Screen", true));
		}

		void AssertDeniedPartyScreeningLogs(BusinessObject bizO, string logStatus, string logDescription)
		{
			var dpsLog = new StmEntityScreeningLogCollection(bizO)[0];

			AssertNotNull(dpsLog);
			AssertEquals(logDescription, dpsLog.PJ_ClearedReason);
			AssertEquals(logStatus, dpsLog.PJ_Status);
			AssertEquals(bizO.PK, dpsLog.PJ_ParentID);
			AssertEquals(bizO.TablePrefix, dpsLog.PJ_ParentTableCode);
			AssertEquals(bizO.PK, dpsLog.PJ_SourceID);
			AssertEquals(bizO.TablePrefix, dpsLog.PJ_SourceTableCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Db.Connection.ExecuteNonQuery("Delete dbo.RefComplianceList");
		}
	}

	class RefCountryModuleForTestWithBussinessObjects : RefCountryModuleForTest
	{
		readonly RefCountry country;
		public RefCountryModuleForTestWithBussinessObjects(RefCountry refCountry)
		{
			country = refCountry;
		}

		public override BusinessObject[] GetSelectedBusinessObjects()
		{
			return new BusinessObject[] { country };
		}
	}

	class ZFormWithIMainForm : ZForm, IMainForm
	{
		#region IMainForm Members

		public ZString CaptionSuffix
		{
			get { return ""; }
			set { }
		}

		public void UpdateToolBarDeleteButton(ZEmbeddedModule embeddedModule) { }

		public INamedModule CurrentModule { get { return null; } }

		public string CurrentModuleLicenceCheckPointName { get { return string.Empty; } }

		#endregion
	}

	class DpsManagerResultForTest : IDpsManager
	{
		public DpsManagerResultForTest()
		{
		}

		public DpsManagerResultForTest(RefCountry refCountry)
		{
			country = refCountry;
		}

		readonly RefCountry country;

		public IDpsServiceV4 GetDpsService(string url)
		{
			return new DpsServiceV4(new HttpClient());
		}

		public List<IDpsServiceV4> GetDpsServices(IDpsServiceV4 service)
		{
			var services = new List<IDpsServiceV4>();
			services.Add(new DpsServiceV4(new HttpClient()));
			return services;
		}

		public void DeactivateBillingEntities(Guid[] entityPKs)
		{
			throw new NotImplementedException();
		}

		public async Task<DpsResponse> Screen(DpsRequestHeaderWithAddressMatching dpsRequestHeaderWithAddressMatching, IDpsServiceV4 service)
		{
			if (country == null)
			{
				return await Task.FromResult(new DpsResponse
				{
					CountryMatches = Array.Empty<CountryMatchInfo>(),
					Profiles = Array.Empty<ProfileHeaderInfo>()
				});
			}

			return await Task.FromResult(GetDpsResponseCountryMatched());
		}

		public async Task<DpsResponse> GetScreenResult(DpsRequestHeaderWithAddressMatching dpsRequestHeaderWithAddressMatching, List<IDpsServiceV4> services)
		{
			if (country == null)
			{
				return await Task.FromResult(new DpsResponse
				{
					CountryMatches = Array.Empty<CountryMatchInfo>(),
					Profiles = Array.Empty<ProfileHeaderInfo>()
				});
			}

			return await Task.FromResult(GetDpsResponseCountryMatched());
		}

		DpsResponse GetDpsResponseCountryMatched()
		{
			var matchingCountryId = Guid.NewGuid();
			var sourceProfileId = Guid.NewGuid();
			var countryMatchInfo = new CountryMatchInfo()
			{
				MatchingCountryId = matchingCountryId,
				SourceProfileID = sourceProfileId,
				MatchingCountryCode = country.RN_Code,
				MatchingCountryName = country.RN_Desc,
				MatchingStandardizedValue = country.RN_Code,
				MatchingCountryScore = 100,
				RequestCountry = new DpsCountryCandidate()
				{
					HeaderPk = Guid.Empty,
					CountryCode = country.RN_Code,
					StandardizedValue = country.RN_Code
				}
			};
			var profileHeaderInfo = new ProfileHeaderInfo()
			{
				SourceProfileID = sourceProfileId,
				ProfileRisk = "High",
				ProfileNames = new List<ProfileNameInfo>()
				{
					new ProfileNameInfo()
					{
						ID = Guid.NewGuid(),
						FullName = country.RN_Desc,
						IsPrimaryName = true,
						Language = string.Empty,
						SourceProfileID = sourceProfileId
					}
				},
				ProfileCountries = new List<ProfileCountryInfo>()
				{
					new ProfileCountryInfo()
					{
						ID = matchingCountryId,
						SourceProfileID = sourceProfileId,
						Code = country.RN_Code,
						CountryName = country.RN_Desc
					}
				},
				TypeOfEntity = "COY",
				ProfileNotes = Array.Empty<byte>(),
				SourceListCodes = new[] { "Source List Code" }
			};

			return new DpsResponse()
			{
				ResponseCode = DpsResponseCode.Successful,
				CountryMatches = new List<CountryMatchInfo>()
				{
					countryMatchInfo
				},
				Profiles = new List<ProfileHeaderInfo>()
				{
					profileHeaderInfo
				}
			};
		}
	}
}
