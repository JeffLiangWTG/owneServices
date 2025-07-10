using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ExternalValidationProgressForm))]
	sealed class ExternalValidationProgressFormTest : ZFormBasherTest
	{
		#region Public/Internal Instance Methods

		public void TestProgressResultIsInvalidWhenServiceReturnsInvalid()
		{
			using (ExternalValidationProgressForm testForm = new ExternalValidationProgressForm(Factory.NewWithValidTestData<OrgHeader>(), new MockInvalidExternalValidationClient()))
			{
				testForm.ShowDialog();
				AssertEquals(ExternalValidationProgressResult.Invalid, testForm.ProgressResult);
			}
		}

		public void TestProgressResultIsTimeoutWhenServiceTimeout()
		{
			var mockValidationClient = new MockExceptionThrownExternalValidationClient(
				ErrorSource.Timeout,
				"[_MOCK_MESSAGE_]");

			using (var testForm = new ExternalValidationProgressForm(Factory.NewWithValidTestData<OrgHeader>(), mockValidationClient))
			{
				testForm.ShowDialog();
				AssertEquals(ExternalValidationProgressResult.Timeout, testForm.ProgressResult);
			}
		}

		public void TestShowDialog_WhenGettingUnhandledExceptionFromValidationService_ShouldDisplayErrorMessage()
		{
			var mockValidationClient = new MockExceptionThrownExternalValidationClient(
				ErrorSource.Other,
				"[_MOCK_MESSAGE_]");

			using (var testForm = new ExternalValidationProgressForm(Factory.NewWithValidTestData<OrgHeader>(), mockValidationClient))
			{
				testForm.ShowDialog();
				AssertEquals(ExternalValidationProgressResult.OtherErrors, testForm.ProgressResult);
			}
		}

		public void TestProgressResultIsValidWhenServiceReturnsValid()
		{
			using (ExternalValidationProgressForm testForm = new ExternalValidationProgressForm(Factory.NewWithValidTestData<OrgHeader>(), new MockValidExternalValidationClient()))
			{
				testForm.ShowDialog();
				AssertEquals(ExternalValidationProgressResult.Valid, testForm.ProgressResult);
			}
		}

		public void TestExecuteCloseFormWhenServiceReturnsValid()
		{
			var isExecuteCloseForm = false;
			ExternalValidationProgressForm testForm = null;
			using (testForm = new ExternalValidationProgressForm(Factory.NewWithValidTestData<OrgHeader>(), new MockValidExternalValidationClient(), ExecuteCloseForm))
			{
				testForm.ShowDialog();
				AssertEquals(ExternalValidationProgressResult.Valid, testForm.ProgressResult);
				Assert(isExecuteCloseForm);
			}

			void ExecuteCloseForm()
			{
				isExecuteCloseForm = true;
				testForm?.Close();
			}
		}

		public void TestWhenCancelPressedShouldReturnCancelledStatus()
		{
			using (ExternalValidationProgressForm testForm = new ExternalValidationProgressForm(Factory.NewWithValidTestData<OrgHeader>(), new MockValidExternalValidationClient(1000)))
			{
				testForm.ShowDialog();
				testForm.buttonCancel_Click(null, new EventArgs());
				AssertEquals(ExternalValidationProgressResult.Cancelled, testForm.ProgressResult);
			}
		}

		#endregion

		#region Private/Protected Members

		protected override Form GetFormToBashCore()
		{
			OrgHeader testOrganization = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			Assert(!testOrganization.HasChanges);
			return new ExternalValidationProgressForm(testOrganization, new MockValidExternalValidationClient(), needsValidation: false);
		}

		class MockInvalidExternalValidationClient : IExternalValidationServiceClient
		{
			#region Interface Methods

			/// <summary>
			/// Validates the current organization.
			/// </summary>
			/// <returns>The validation result.</returns>
			public Task<ExternalValidationResult> ValidateAsync(BusinessObjectFactory factory, CancellationToken cancellationToken)
			{
				return Task.FromResult(new ExternalValidationResult
				{
					Value = "Invalid",
					Errors = new string[] { "Error1" },
					Warnings = new string[] { "Warning1" },
					ErrorSource = ErrorSource.External
				});
			}

			#endregion
		}

		class MockExceptionThrownExternalValidationClient : IExternalValidationServiceClient
		{
			readonly ErrorSource errorSource;

			readonly string errorMessage;

			public MockExceptionThrownExternalValidationClient(ErrorSource errorSource, string errorMessage)
			{
				this.errorSource = errorSource;
				this.errorMessage = errorMessage;
			}

			#region Interface Methods

			/// <summary>
			/// Validates the current organization.
			/// </summary>
			/// <returns>The validation result.</returns>
			public Task<ExternalValidationResult> ValidateAsync(BusinessObjectFactory factory, CancellationToken cancellationToken)
			{
				return Task.FromResult(new ExternalValidationResult
				{
					Errors = new[] { errorMessage },
					ErrorSource = errorSource
				});
			}

			#endregion
		}

		internal class MockValidExternalValidationClient : IExternalValidationServiceClient
		{
#pragma warning disable CW1050 // Use System.TimeSpan Type For A Duration
			readonly int timeoutInMilliseconds;

			public MockValidExternalValidationClient(int timeoutInMilliseconds = 0)
			{
				this.timeoutInMilliseconds = timeoutInMilliseconds;
			}

			#region Interface Methods

			/// <summary>
			/// Validates the current organization.
			/// </summary>
			/// <returns>The validation result.</returns>
			public async Task<ExternalValidationResult> ValidateAsync(BusinessObjectFactory factory, CancellationToken cancellationToken)
			{
				if (timeoutInMilliseconds > 0)
				{
					await Task.Delay(timeoutInMilliseconds, cancellationToken);
				}

				return await Task.FromResult(new ExternalValidationResult
				{
					Value = "Valid",
					ErrorSource = ErrorSource.None
				});
			}

			#endregion
		}

		#endregion
	}
}
