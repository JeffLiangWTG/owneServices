using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class GlbPersonAddressControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestBinding()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "Richard Wilson";
			person.PER_HomeAddress1 = Address1;

			using (var form = new PersonFormForTest(person))
			{
				form.Show();
				AssertEquals(form.GlbPersonAddressControl.Person.PER_HomeAddress1, Address1);
			}
		}

		[RequiresSTA]
		public void TestValidateAddressButtonVisibility()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "Richard Wilson";
			person.PER_HomeAddress1 = Address1;

			bool isAllowed = Env.Security.PersonIntelligenceViewHomeAddress.IsAllowed;

			try
			{
				Assert("person is not saved.", !person.IsInDatabase);

				Env.Security.PersonIntelligenceViewHomeAddress.IsAllowed = false;
				using (var form = new PersonFormForTest(person))
				{
					form.Show();
					Assert("Validate address button is visible when security checkpoint PersonIntelligenceViewHomeAddress is not granted and person is not in database.", form.ValidateAddressButton.Visible);
					Assert("Clear fields button is visible when security checkpoint PersonIntelligenceViewHomeAddress is not granted and person is not in database.", form.ClearFieldsButton.Visible);
				}

				Factory.Save();
				Assert("person is saved", person.IsInDatabase);
				using (var form = new PersonFormForTest(person))
				{
					form.Show();
					Assert("Validate address button is invisible when security checkpoint PersonIntelligenceViewHomeAddress is not granted and person is in database.", !form.ValidateAddressButton.Visible);
					Assert("Clear fields button is invisible when security checkpoint PersonIntelligenceViewHomeAddress is not granted and person is in database.", !form.ClearFieldsButton.Visible);
				}

				Env.Security.PersonIntelligenceViewHomeAddress.IsAllowed = true;
				using (var form = new PersonFormForTest(person))
				{
					form.Show();
					Assert("Validate address button is visible when security checkpoint PersonIntelligenceViewHomeAddress is granted.", form.ValidateAddressButton.Visible);
					Assert("Clear fields button is visible when security checkpoint PersonIntelligenceViewHomeAddress is granted.", form.ClearFieldsButton.Visible);
				}
			}
			finally
			{
				Env.Security.PersonIntelligenceViewHomeAddress.IsAllowed = isAllowed;
			}
		}

		[RequiresSTA]
		public void TestNoCreatedChangesNotificationExceptionThrown_WhenOnLoad()
		{
			using (DataRegistry.Instance.RawRegistry.EnableAddressValidationWebService.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var person = Factory.NewWithValidTestData<GlbPersonForTest>();
				person.PER_HomeAddress1 = "xxxx";
				person.PER_RN_NKCountry = "AU";
				person.PER_City = "Sydney";
				person.ValidationStatus = AddressValidationStatus.ToBeVerified;
				person.Address1 = "Test Address1";
				Factory.Save();

				AssertNoExceptionThrown(() =>
				{
					using (TestingState.SuspendIsRunningTests())
					using (var form = new GlbPersonFormForTest(person))
					{
						form.Show();
						Assert("Change should not happen on the person when load", !person.HasChanges);
						AssertNotContains("Should NOT contain DeveloperNotificationException", "Created Changes Before Type (HasChanges: True, HasChangesNotIncludingChildren: False)", ErrorReporter.LastMessageReported);
						AssertEquals("Validation status has been changed to INV.", AddressValidationStatus.Invalid, person.ValidationStatus);
					}
				});
			}
		}

		public ZString Address1 => "234 Gardeners Rd.";

		#region Implementation

		class GlbPersonForTest : GlbPerson, ISupportWebAddressValidation
		{
			public GlbPersonForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new Task<WebAddressValidationResult> ValidateAddressAsync(CancellationTokenSource cancellationToken, CleanseAction cleanseAction = CleanseAction.ValidateAndSuggest)
			{
				ValidationStatus = AddressValidationStatus.Invalid;
				return Task.FromResult(new WebAddressValidationResult());
			}
		}

		class PersonFormForTest : ZForm
		{
			public PersonFormForTest(GlbPerson person) : base(person)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 768);
				Controls.Add(GlbPersonAddressControl);
				BindingSource.SetBindingMember(GlbPersonAddressControl, ".");
				Show();
			}

			public readonly GlbPersonAddressControl GlbPersonAddressControl = new GlbPersonAddressControl();

			public ZButton ValidateAddressButton
			{
				get
				{
					return GlbPersonAddressControl.Controls.Find("ValidateAddressButton", true)[0] as ZButton;
				}
			}

			public ZButton ClearFieldsButton
			{
				get
				{
					return GlbPersonAddressControl.Controls.Find("ClearFieldsButton", true)[0] as ZButton;
				}
			}
		}

		public class GlbPersonFormForTest : GlbPersonForm
		{
			public GlbPersonFormForTest(GlbPerson person) : base(person)
			{
			}
		}

		#endregion
	}
}
