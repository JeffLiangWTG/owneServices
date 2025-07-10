using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.EntityFramework;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	[TestedType(typeof(StandAloneScreeningForm))]
	public class StandAloneScreeningFormTest : ZFormBasherTest
	{
		protected override bool ShouldTestFormIsFullyTranslatable => false;

		[StressTest]
		public override void TestBashingForm()
		{
			base.TestBashingForm();
		}

		protected override Form GetFormToBashCore()
		{
			return new StandAloneScreeningForm(true);
		}

		public void TestShowScreenMessage_OnOrgStandAloneForm()
		{
			var dpsManager = new DpsManagerForTest();
			UnitTestUserNotification.Instance.ClearMessages();
			using (ObjectFactory.Substitute<IDpsManager>(dpsManager))
			using (var dummyForm = new StandAloneScreeningForm_ForTest(true))
			{
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
				AsyncTaskSynchronizer.Run(dummyForm.ScreenButton_Click_ForTest);
				AssertNotNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No denied party matching info.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestNoExceptionDuringScreening()
		{
			var dpsManager = new DpsManagerForTest();
			using (ObjectFactory.Substitute<IDpsManager>(dpsManager))
			using (var dummyForm = new StandAloneScreeningForm_ForTest(true))
			{
				AssertNoExceptionThrown(() => AsyncTaskSynchronizer.Run(dummyForm.ScreenButton_Click_ForTest));
			}
		}

		[StressTest]
		public void TestStandAloneScreening_OrganizationWithCountry()
		{
			var dpsManager = new DpsManagerForTest();
			UnitTestUserNotification.Instance.ClearMessages();
			using (ObjectFactory.Substitute<IDpsManager>(dpsManager))
			using (var dummyForm = new StandAloneScreeningForm_ForTest(true, ""))
			{
				dummyForm.Show();
				dummyForm.OrgRadioButton.PerformClick();
				dummyForm.FullNameTextBox.Text = "WiseTech Global";
				dummyForm.Address1TextBox.Text = "72 O'Riordan";
				dummyForm.Address2TextBox.Text = "Street";
				dummyForm.PostCodeTextBox.Text = "2015";
				dummyForm.CountryFindBox.CodeBox.Text = "AU";
				dummyForm.StateTextBox.Text = "New South Wales";
				dummyForm.IdIssuingCountryFindBox.CodeBox.Text = "US";
				AsyncTaskSynchronizer.Run(dummyForm.ScreenButton_Click_ForTest);
				AssertNotNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No denied party matching info.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("AU", dpsRequestHeaderWithAddressMatching_Exposed.DpsCountryCandidates.Single().CountryCode);
				AssertEquals(0,dpsRequestHeaderWithAddressMatching_Exposed.DpsRegistrationCodeCandidates.Count());
			}
		}

		public void TestStandAloneScreening_OrganizationWithIDNumber()
		{
			var dpsManager = new DpsManagerForTest();
			UnitTestUserNotification.Instance.ClearMessages();
			using (ObjectFactory.Substitute<IDpsManager>(dpsManager))
			using (var dummyForm = new StandAloneScreeningForm_ForTest(true, "750922515"))
			{
				dummyForm.Show();
				dummyForm.OrgRadioButton.PerformClick();
				dummyForm.FullNameTextBox.Text = "Huawei";
				dummyForm.Address1TextBox.Text = "L6 799 Pacific Hwy";
				dummyForm.StateTextBox.Text = "Chatswood";
				dummyForm.PostCodeTextBox.Text = "2067";
				dummyForm.CountryFindBox.CodeBox.Text = "AU";
				dummyForm.StateTextBox.Text = "New South Wales";
				dummyForm.IdIssuingCountryFindBox.CodeBox.Text = "AU";
				AsyncTaskSynchronizer.Run(dummyForm.ScreenButton_Click_ForTest);
				AssertEquals("AU", dpsRequestHeaderWithAddressMatching_Exposed.DpsCountryCandidates.Single().CountryCode);
				AssertEquals("DUN",dpsRequestHeaderWithAddressMatching_Exposed.DpsRegistrationCodeCandidates.Single().RegCodeType);
			}
		}

		public void TestStandAloneScreening_PersonWithIDNumber()
		{
			var dpsManager = new DpsManagerForTest();
			UnitTestUserNotification.Instance.ClearMessages();
			using (ObjectFactory.Substitute<IDpsManager>(dpsManager))
			using (var dummyForm = new StandAloneScreeningForm_ForTest(true, "D1593574"))
			{
				dummyForm.Show();
				dummyForm.PersonRadioButton.PerformClick();
				dummyForm.FullNameTextBox.Text = "Osama";
				AsyncTaskSynchronizer.Run(dummyForm.ScreenButton_Click_ForTest);
				AssertEquals("PAS",dpsRequestHeaderWithAddressMatching_Exposed.DpsRegistrationCodeCandidates.Single().RegCodeType);
			}
		}

		public void TestStandAloneScreening_VesselWithCountyOfRegistration()
		{
			var dpsManager = new DpsManagerForTest();
			UnitTestUserNotification.Instance.ClearMessages();
			using (ObjectFactory.Substitute<IDpsManager>(dpsManager))
			using (var dummyForm = new StandAloneScreeningForm_ForTest(true, initialSelection: StandaloneScreeningSupportedCandidate.Vessel))
			{
				dummyForm.Show();
				dummyForm.VesselRadioButton.PerformClick();
				dummyForm.VesselCountyOfRegFindBox.CodeBox.Text = "AU";
				dummyForm.FullNameTextBox.Text = "Titanic";
				AsyncTaskSynchronizer.Run(dummyForm.ScreenButton_Click_ForTest);
				AssertNotNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No denied party matching info.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("AU", dpsRequestHeaderWithAddressMatching_Exposed.DpsCountryCandidates.Single().CountryCode);
			}
		}

		public void TestScreeningOrganizationNameSeparators()
		{
			using (OrganisationsDataRegistry.Instance.NameSeparators.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { "C/O" }))
			using (ObjectFactory.Substitute<IDpsManager>(new DpsManagerForTest()))
			using (var dummyForm = new StandAloneScreeningForm_ForTest(true))
			{
				dummyForm.Show();
				dummyForm.OrgRadioButton.PerformClick();
				dummyForm.FullNameTextBox.Text = "CHINA AIRLINES C/O CASS";
				AsyncTaskSynchronizer.Run(dummyForm.ScreenButton_Click_ForTest);

				AssertEquals(2, dpsRequestHeaderWithAddressMatching_Exposed.DpsNameCandidates.Count());
				AssertContainsExactElementsInAnyOrder(new[] { "CHINA AIRLINES", "CASS" }, dpsRequestHeaderWithAddressMatching_Exposed.DpsNameCandidates.Select(u => u.FullName).ToArray());
			}
		}

		public void TestScreeningPersonName()
		{
			using (OrganisationsDataRegistry.Instance.NameSeparators.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { "C/O" }))
			using (ObjectFactory.Substitute<IDpsManager>(new DpsManagerForTest()))
			using (var dummyForm = new StandAloneScreeningForm_ForTest(true, ""))
			{
				dummyForm.Show();
				dummyForm.PersonRadioButton.PerformClick();
				dummyForm.FullNameTextBox.Text = "ABC C/O John Doe";
				AsyncTaskSynchronizer.Run(dummyForm.ScreenButton_Click_ForTest);

				AssertEquals(1, dpsRequestHeaderWithAddressMatching_Exposed.DpsNameCandidates.Count());
				AssertEquals("ABC C/O John Doe", dpsRequestHeaderWithAddressMatching_Exposed.DpsNameCandidates.FirstOrDefault().FullName);

				dummyForm.FullNameTextBox.Text = "John Doe";
				AsyncTaskSynchronizer.Run(dummyForm.ScreenButton_Click_ForTest);

				AssertEquals(1, dpsRequestHeaderWithAddressMatching_Exposed.DpsNameCandidates.Count());
				AssertEquals("John Doe", dpsRequestHeaderWithAddressMatching_Exposed.DpsNameCandidates.FirstOrDefault().FullName);
				AssertEquals(0,dpsRequestHeaderWithAddressMatching_Exposed.DpsRegistrationCodeCandidates.Count());
			}
		}

		#region Implementation
		class StandAloneScreeningForm_ForTest : StandAloneScreeningForm
		{
			public StandAloneScreeningForm_ForTest(bool forceFullListRatherThanCutDownList, string idNumberTextBoxText = "123", StandaloneScreeningSupportedCandidate initialSelection = StandaloneScreeningSupportedCandidate.Org)
				: base(forceFullListRatherThanCutDownList, initialSelection)
			{
				IdNumberTextBox.Text = idNumberTextBoxText;
			}

			public Task ScreenButton_Click_ForTest() => Screen();
		}

		class DpsManagerForTest : IDpsManager
		{
			public void DeactivateBillingEntities(Guid[] entityPKs) => throw new NotImplementedException();

			public async Task<DpsResponse> Screen(DpsRequestHeaderWithAddressMatching dpsRequestHeaderWithAddressMatching, IDpsServiceV4 service)
			{
				dpsRequestHeaderWithAddressMatching_Exposed = dpsRequestHeaderWithAddressMatching;
				return await Task.FromResult(new DpsResponse());
			}

			public async Task<DpsResponse> GetScreenResult(DpsRequestHeaderWithAddressMatching dpsRequestHeaderWithAddressMatching, List<IDpsServiceV4> services)
			{
				dpsRequestHeaderWithAddressMatching_Exposed = dpsRequestHeaderWithAddressMatching;
				return await Task.FromResult(new DpsResponse());
			}

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
		}

		static DpsRequestHeaderWithAddressMatching dpsRequestHeaderWithAddressMatching_Exposed;

		#endregion
	}
}
