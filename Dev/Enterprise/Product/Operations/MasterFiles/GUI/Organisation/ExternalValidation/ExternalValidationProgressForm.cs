using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public enum ExternalValidationProgressResult
	{
		Valid,
		Invalid,
		Cancelled,
		Timeout,
		OtherErrors,
	}

	public partial class ExternalValidationProgressForm : ZChildForm
	{
		readonly IReadOnlyDictionary<ErrorSource, ExternalValidationProgressResult> progressResultLookup =
			new Dictionary<ErrorSource, ExternalValidationProgressResult>
			{
				[ErrorSource.External] = ExternalValidationProgressResult.Invalid,
				[ErrorSource.Cancelled] = ExternalValidationProgressResult.Cancelled,
				[ErrorSource.Timeout] = ExternalValidationProgressResult.Timeout,
				[ErrorSource.Other] = ExternalValidationProgressResult.OtherErrors
			};

		public ExternalValidationProgressForm()
		{
			InitializeComponent();
		}

		readonly Action closeProgressFormAndOrgForm;

		public ExternalValidationProgressForm(OrgHeader organisation, Action closeProgressFormAndOrgForm)
			: base(organisation)
		{
			Organization = organisation;
			externalValidationServiceClient = new ExternalValidationServiceClient(organisation.PK);
			this.closeProgressFormAndOrgForm = closeProgressFormAndOrgForm;
			InitializeComponent();
		}

#if DEBUG
		internal ExternalValidationProgressForm(OrgHeader organisation, IExternalValidationServiceClient externalValidationServiceClient, Action closeProgressFormAndOrgForm = null, bool needsValidation = true)
			: this(organisation, closeProgressFormAndOrgForm)
		{
			this.externalValidationServiceClient = externalValidationServiceClient;
			this.needsValidation = needsValidation;
		}
#endif

		/// <summary>
		/// Gets the target organization.
		/// </summary>
		public OrgHeader Organization { get; private set; }

		/// <summary>
		/// Gets the result of the external validation progress.
		/// </summary>
		public ExternalValidationProgressResult ProgressResult { get; private set; }

		/// <summary>
		/// Gets the result of the external validation.
		/// </summary>
		public ExternalValidationResult Result { get; private set; }

		public string ExceptionMessage { get; private set; } = string.Empty;

		public override string FormCaption
		{
			get
			{
				return Res.GetString("ExternalValidationProgressForm|{34d9ff4-ef58-48e0-b380-d6f04620d2d4", "{0} Validation", Organization.HumanReadableName);
			}
		}

		void ExternalValidationProgressForm_Load(object sender, EventArgs e)
		{
			ValidateAsync();
		}

		void ValidateAsync()
		{
			if (needsValidation)
			{
				RefreshLabelText();
				labelRefreshTimer.Start();
				cancellationTokenSource = new CancellationTokenSource();

				Task.Factory.StartNew(() =>
				{
					var innerTask = Task.Factory.StartNew(() =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							try
							{
								return externalValidationServiceClient.ValidateAsync(new BusinessObjectFactory { NameForDebugging = "FactoryForOrgExternalValidation" }, cancellationTokenSource.Token).Result;
							}
							catch (AggregateException ax)
							{
								if (ax?.InnerException is OperationCanceledException)
								{
									return MakeCancelReturn();
								}
								else
								{
									return MakeFailReturn(ax?.InnerException?.Message);
								}
							}
							catch (OperationCanceledException)
							{
								return MakeCancelReturn();
							}
							catch (Exception ex) when (!ex.IsCriticalException())
							{
								return MakeFailReturn(ex.Message);
							}
						}
					});
					return innerTask.Result;
				}).ContinueWith(t =>
				{
					labelRefreshTimer.Stop();

					if (Result?.ErrorSource != ErrorSource.Cancelled)
					{
						Result = t.Result;
					}

					if (Result.ErrorSource != ErrorSource.None)
					{
						if (!progressResultLookup.TryGetValue(Result.ErrorSource, out var progressResult))
						{
							progressResult = ExternalValidationProgressResult.OtherErrors;
						}

						ProgressResult = progressResult;

						ExceptionMessage = Result.Errors != null && Result.Errors.Any()
							? string.Join(", ", Result.Errors.Select(error => string.Format(CultureInfo.InvariantCulture, "'{0}'", error)))
							: string.Empty;

						Close();
					}
					else
					{
						if (Result.IsValid)
						{
							ProgressResult = ExternalValidationProgressResult.Valid;
							if (Result.HasErrors || Result.HasWarnings)
							{
								Close();
							}
							else
							{
								PassValidation();
							}
						}
						else
						{
							ProgressResult = ExternalValidationProgressResult.Invalid;
							Close();
						}
					}
				}, TaskScheduler.FromCurrentSynchronizationContext());
			}
		}

		ExternalValidationResult MakeCancelReturn()
		{
			return new ExternalValidationResult
			{
				ErrorSource = ErrorSource.Cancelled,
				Warnings = Array.Empty<string>(),
				Errors = new[]
				{
					Res.GetString("8F6B912D-AC46-4042-B7CC-D92A75FBB351", "External validation operation is canceled.")
				}
			};
		}

		ExternalValidationResult MakeFailReturn(string error)
		{
			return new ExternalValidationResult
			{
				ErrorSource = ErrorSource.Other,
				Warnings = Array.Empty<string>(),
				Errors = new[] { error },
			};
		}

		readonly bool needsValidation = true;

		void RefreshLabelText()
		{
			LabelStatus.Text = Res.GetString("ExternalValidationProgressForm|37a45191-af14-488f-a26e-b5740e8a4c86", @"Calling 3rd party web service '{0}' to validate organization details against external rules.
This service is configured by the system administrator and is not part of {1}.
If the service is taking an excessive amount of time, you may cancel and continue.
Waiting for response from the service for {2} seconds...", DataRegistry.Instance.ExternalValidationServiceUrl, BrandingFactory.Instance.ProductName, labelRefreshTicks++);
		}

		void PassValidation()
		{
			LabelStatus.Text = Res.GetString("ExternalValidationProgressForm|B890E5B9-9059-4B3B-9D0C-915C59A13DFB", "External validation passed for {0}.", Organization.HumanReadableName);
			buttonCancel.Visible = false;
			buttonClose.Visible = true;
			closeTimer.Enabled = true;
		}

#if DEBUG
		internal
#endif
		void buttonCancel_Click(object sender, EventArgs e)
		{
			var message = Res.GetString("8F6B912D-AC46-4042-B7CC-D92A75FBB351", "External validation operation is canceled.");

			Result = new ExternalValidationResult
			{
				ErrorSource = ErrorSource.Cancelled,
				Warnings = Array.Empty<string>(),
				Errors = new[] { message }
			};
			ProgressResult = ExternalValidationProgressResult.Cancelled;
			ExceptionMessage = message;

			cancellationTokenSource?.Cancel();
		}

		void buttonClose_Click(object sender, EventArgs e)
		{
			CloseForm();
		}

		void closeTimer_OnTick(object sender, EventArgs eventArgs)
		{
			closeTimer.Enabled = false;
			CloseForm();
		}

		void CloseForm()
		{
			if (closeProgressFormAndOrgForm != null)
			{
				closeProgressFormAndOrgForm();
			}
			else
			{
				Close();
			}
		}

		void labelRefreshTimer_OnTick(object sender, EventArgs eventArgs)
		{
			RefreshLabelText();
		}

		CancellationTokenSource cancellationTokenSource;
		readonly IExternalValidationServiceClient externalValidationServiceClient;

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					if (components != null)
					{
						components.Dispose();
					}
					if (cancellationTokenSource != null)
					{
						cancellationTokenSource.Dispose();
					}
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		#endregion

		#region GUI Setup

		public override string FormVerb
		{
			get
			{
				return "";
			}
		}

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			base.InitialiseForm();
		}

		#endregion

#if DEBUG

#pragma warning disable CW1050 // Use System.TimeSpan Type For A Duration

		public class MockValidExternalValidationClient : IExternalValidationServiceClient
		{
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

#endif

	}
}
