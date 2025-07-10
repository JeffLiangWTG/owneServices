using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using WTG.CreditCheck;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI
{
	public class ReportItemInfoModel : ModelBase<ReportItemInfoModel>
	{
		public ReportItemInfoModel(ReportItemModel model, CreditReportUserControl supportCreditCheck, ICreditCheckService creditCheckService, MainPageModel parent)
		{
			this.model = model;
			this.creditCheckService = creditCheckService;
			this.parent = parent;
			this.supportCreditCheck = supportCreditCheck;
			LastReportDate = model.LastReportDate;
			header = supportCreditCheck?.Header;
		}

		readonly ReportItemModel model;

		readonly CreditReportUserControl supportCreditCheck;

		readonly ICreditCheckService creditCheckService;

		readonly MainPageModel parent;

		readonly OrgHeader header;

		public CreditReportType CreditReportType => model.CreditReportType;

		public (bool IsAllowed, string ErrorMessageForNotAllowed) SecurityCheckpoint
		{
			get
			{
				var result = (true, string.Empty);

				if (supportCreditCheck.SecurityCheckpoints != null && supportCreditCheck.SecurityCheckpoints.TryGetValue((CreditReportType, IsGet), out var securityCheckpointForSpecifiedReport))
				{
					result = securityCheckpointForSpecifiedReport;
				}

				return result;
			}
		}

		public bool IsGet => model.LastReportDate == null;

		public string ReportCaption => ResourceStringHelper.GetReportCaption(model.CreditReportType);

		public string ButtonCaption => IsGet ? ResourceStringHelper.GetReport : ResourceStringHelper.Renew;

		public Bitmap ToolTipIcon => ImageBitmapHelper.GetCreditEventIconInfo(CreditReportEventsMapping.GetWorstCreditEventType(relatedLatestEvents)).Icon;

		public string ToolTipBackgroundColor => ImageBitmapHelper.GetCreditEventIconInfo(CreditReportEventsMapping.GetWorstCreditEventType(relatedLatestEvents)).Color;

		public bool ToolTipIconVisible => RelatedLatestEvents.Any();

		public string ToolTipCaption => ResourceStringHelper.ToolTipCaption(RelatedLatestEvents.Count());

		public bool GetReportLineVisible { get; set; } = true;

		public IEnumerable<CreditEvent> RelatedLatestEvents
		{
			get => relatedLatestEvents;
			set
			{
				relatedLatestEvents = value;
				NotifyPropertyChanged();
			}
		}
		IEnumerable<CreditEvent> relatedLatestEvents = new List<CreditEvent>();

		public DateTime? LastReportDate
		{
			get => model.LastReportDate;
			set
			{
				model.LastReportDate = value;
				LastReportDateOrMoreInfo = value == null
					? ResourceStringHelper.MoreInfo
					: ResourceStringHelper.LastReportDate(model.LastReportDate.Value);
				NotifyPropertyChanged(nameof(ButtonCaption));
			}
		}

		public string LastReportDateOrMoreInfo
		{
			get => lastReportDateOrMoreInfo;
			set
			{
				lastReportDateOrMoreInfo = value;
				NotifyPropertyChanged();
			}
		}
		string lastReportDateOrMoreInfo;

		public async Task OpenMoreInfoForm(Form parentForm)
		{
			if (IsGet)
			{
				var getSpecifiedReportModel = new GetSpecifiedReportModel(CreditReportType, supportCreditCheck);
				var reportForm = new GetSpecifiedReportForm(getSpecifiedReportModel);
				ZFormModaliser.ShowDialogAndDispose(reportForm, supportCreditCheck.ParentForm);

				if (reportForm.NeedToGetReport)
				{
					await CompanyLookupAndGetReportAsync();
				}
			}
			else
			{
				supportCreditCheck.ShowLatestReport(CreditReportType);
			}
		}

		internal async Task CompanyLookupAndGetReportAsync()
		{
			var eventBannerModel = parent.EventsBannerModel;
			if (!eventBannerModel.SilentCompanyLookupCompleted)
			{
				eventBannerModel.TokenSource?.Cancel();
				eventBannerModel.TokenSource?.Dispose();
				eventBannerModel.SilentCompanyLookupCompleted = true;
			}

			if (header.HasChanges)
			{
				Globals.Message.ShowWarning(ResourceStringHelper.SaveBeforeGettingReport);
			}
			else if (!SecurityCheckpoint.IsAllowed)
			{
				Globals.Message.ShowError(SecurityCheckpoint.ErrorMessageForNotAllowed);
			}
			else if (supportCreditCheck.Identifiers.Any(x => x.Type == IdentifierType.DUNS && !string.IsNullOrWhiteSpace(x.ID)))
			{
				await GetReport();
			}
			else
			{
				var identifiers = await CompanyLookupAsync();
				if (identifiers != null && identifiers.Any())
				{
					if (supportCreditCheck.SaveIdentifiers(identifiers, false))
					{
						await GetReport(identifiers);
					}
				}
			}
		}

		async Task<IEnumerable<Identifier>> CompanyLookupAsync()
		{
			IEnumerable<Identifier> result = null;

			var mainAddress = header.MainAddress;
			var companyLookupRequest = new CompanyLookupRequest()
			{
				OrgPK = supportCreditCheck.EntityPk,
				Name = header.OH_FullName,
				Country = CountryIsoAlpha3Code,
				Postcode = mainAddress.Postcode,
				State = mainAddress.StateCode,
				City = mainAddress.City,
				Address1 = mainAddress.Address1,
				Address2 = mainAddress.Address2,
				LicenseCode = supportCreditCheck.EnterpriseCode,
				Identifiers = supportCreditCheck.Identifiers,
				CallingMode = CallingMode.UI
			};

			var tokenSource = new CancellationTokenSource();

			var loadingResult = new LoadingWindowResult<CompanyLookupResponse>();

			using (var form = new LoadingForm(
					   new LoadingFormModel<object>(
						   ResourceStringHelper.CompanyLookup,
						   new Task<object>(() => creditCheckService.CompanyLookupAsync(companyLookupRequest, tokenSource.Token).Result))))
			{
				await form.ShowAndRunningTaskAsync(supportCreditCheck.ParentForm);
				loadingResult.Exception = form.Model.LoadingResult.Exception;
				loadingResult.Result = (CompanyLookupResponse)form.Model.LoadingResult.Result;
				form.Close();
			}

			if (loadingResult.Exception == null && loadingResult.Result.ResultCode == ResultCode.Successful)
			{
				var companyItems = loadingResult.Result.CompanyItems.ToList();
				if (companyItems.Any())
				{
					var companyLookupModel = new CompanyLookupModel(companyItems);
					var confirmOrganizationForm = new ConfirmOrganizationForm(companyLookupModel);
					ZFormModaliser.ShowDialogAndDispose(confirmOrganizationForm, supportCreditCheck.ParentForm);

					if (confirmOrganizationForm.NeedToGetReport)
					{
						result = companyLookupModel.CompanyLookupItemModels.Single(x => x.Selected).Identifiers;
					}
				}
				else
				{
					Globals.Message.ShowWarning(ResourceStringHelper.NoMatchedCompany);
				}
			}
			else
			{
				if (loadingResult.Result != null)
				{
					Globals.Message.ShowError(loadingResult.Result.ErrorInfo.ToLocalizedMessage(CreditReportType.ToString(), header.OH_FullName));
				}
				else
				{
					Globals.Message.ShowError(ResourceStringHelper.GeneralError);
				}
			}
			return result;
		}

		protected async Task GetReport(IEnumerable<Identifier> identifiers = null)
		{
			if (await ShowTermsAndAgreement())
			{
				var availableIdentifiers = identifiers != null && identifiers.Any() ? identifiers : supportCreditCheck.Identifiers?.ToList();
				var address1 = (string)header.MainAddress.Address1;
				var address2 = (string)header.MainAddress.Address2;
				var confirmModel = new ConfirmGetReportModel
				{
					ReportType = model.CreditReportType,
					Identifiers = availableIdentifiers,
					City = header.MainAddress.City,
					CompanyAddress = string.IsNullOrWhiteSpace(address2) ? address1 : string.Join(" ", address1, address2),
					CompanyName = header.OH_FullName,
					CountryState = header.MainAddress.StateCode,
					PostCode = header.MainAddress.Postcode,
					IsGet = IsGet,
				};

				if (IsConfirmed(confirmModel))
				{
					var loadingResult = await GetLoadingResult(availableIdentifiers);

					if (loadingResult.Exception == null && loadingResult.Result.ResultCode == ResultCode.Successful)
					{
						var extractedInfo = loadingResult.Result.ExtractedInfo;
						if (Env.Security.OrganisationCreditReportsImportCompanyData.IsAllowed &&
							OrganisationRegistry.Instance.EnableImportFromCreditReports.Value &&
							(
								!string.IsNullOrEmpty(extractedInfo.WebsiteUrl) ||
								extractedInfo.RelatedBrandOrCompanyNames != null && extractedInfo.RelatedBrandOrCompanyNames.Any() ||
								extractedInfo.AddressInfos != null && extractedInfo.AddressInfos.Any() ||
								extractedInfo.RegistrationNums != null && extractedInfo.RegistrationNums.Any()
							))
						{
						}

						supportCreditCheck.SaveAndShowReport(model.CreditReportType, loadingResult.Result.Payload, loadingResult.Result.PayloadExtension, IsGet, loadingResult.Result.DunsRating, loadingResult.Result.FailureRiskScore, loadingResult.Result.LatePaymentScore);

						AddMonitorAndSavePurchaseInfoSilently();
						LastReportDate = ZDateTime.Today.ToDateTime();
						parent.ReportsModel.SetReportItemToolTip();
					}
					else
					{
						Globals.Message.ShowError(loadingResult.Result.ErrorInfo.ToLocalizedMessage(model.CreditReportType.ToString(), header.OH_FullName));
					}
				}
			}
		}

		protected virtual async Task<LoadingWindowResult<CreditReportResponse>> GetLoadingResult(IEnumerable<Identifier> availableIdentifiers)
		{
			var tokenSource = new CancellationTokenSource();
			var loadingResult = new LoadingWindowResult<CreditReportResponse>();

			using (var form = new LoadingForm(new LoadingFormModel<object>(
					   ResourceStringHelper.GetLoadingFormTitleForReport(model.CreditReportType),
					   new Task<object>(() => creditCheckService.GetReportAsync(
						   new CreditReportRequest
						   {
							   Country = CountryIsoAlpha3Code,
							   Identifiers = availableIdentifiers,
							   LicenseCode = supportCreditCheck.EnterpriseCode,
							   Name = header.OH_FullName,
							   ReportType = model.CreditReportType,
						   },
						   tokenSource.Token).Result))))
			{
				await form.ShowAndRunningTaskAsync(supportCreditCheck.ParentForm);
				loadingResult.Exception = form.Model.LoadingResult.Exception;
				loadingResult.Result = (CreditReportResponse)form.Model.LoadingResult.Result;
			}

			return loadingResult;
		}

		protected virtual async Task<bool> ShowTermsAndAgreement()
		{
			var term = new CreditReportTermsAndCondition();
			var acknowledgeChecker = new TermsAcknowledgementChecker(term, supportCreditCheck.ParentForm);
			return await acknowledgeChecker.CheckTermAcknowledged();
		}

		protected virtual bool IsConfirmed(ConfirmGetReportModel confirmModel)
		{
			var confirmGetReportForm = new ConfirmGetReportForm(confirmModel);
			ZFormModaliser.ShowDialogAndDispose(confirmGetReportForm, supportCreditCheck.ParentForm);
			return confirmGetReportForm.ConfirmButtonClicked;
		}

		public void FilterEvents()
		{
			parent.EventsBannerModel.FilterEvents(relatedLatestEvents);
		}

		void AddMonitorAndSavePurchaseInfoSilently()
		{
			var countryIsoAlpha3Code = CountryIsoAlpha3Code;
			var identifiers = supportCreditCheck.Identifiers;
			var enterpriseCode = supportCreditCheck.EnterpriseCode;
			var organizationName = header.OH_FullName;
			var entityPk = supportCreditCheck.EntityPk;

			Task.Run(async () =>
			{
				var retryCount = 3;
				var successful = false;
				while (retryCount > 0 && !successful)
				{
					try
					{
						var result = await creditCheckService.SubscribeAsync(new SubscriptionRequest()
						{
							Country = countryIsoAlpha3Code,
							Identifiers = identifiers,
							LicenseCode = enterpriseCode,
							Name = organizationName,
							OrgPK = entityPk,
							AddPurchaseInfo = true,
							PurchaseReportType = model.CreditReportType,
						});

						successful = result.ResultCode == ResultCode.Successful;
					}
					catch (Exception)
					{
						// ignored
					}
					finally
					{
						retryCount--;
					}
				}
			});
		}

		string CountryIsoAlpha3Code => header.Country?.RN_IsoAlpha3Code;
	}
}
