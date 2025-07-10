using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using WTG.CreditCheck;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI
{
	public class EventsBannerModel : ModelBase<EventsBannerModel>
	{
		public EventsBannerModel(CreditReportUserControl creditReportControl, ICreditCheckService service, MainPageModel parent)
		{
			this.creditReportControl = creditReportControl;
			this.service = service;
			Parent = parent;
			ErrorMessage = ResourceStringHelper.EventsUnavailable;
			header = creditReportControl.Header;
		}

		readonly CreditReportUserControl creditReportControl;

		readonly ICreditCheckService service;

		readonly OrgHeader header;

		public MainPageModel Parent { get; }

		public ZDateTime CreditEventsToDate => ZDateTime.Today;

		public ZDateTime CreditEventsFromDate => CreditEventsToDate.AddMonths(-12);

		public string EventsBannerCaption => ResourceStringHelper.LatestCreditEvents;

		public string LoadingEvents => ResourceStringHelper.LoadingEvents;

		List<CreditEvent> OriginalEventItemModels { get; set; } = new List<CreditEvent>();

		public ObservableCollection<EventItemModel> Events { get; } = new ObservableCollection<EventItemModel>();

		public CancellationTokenSource TokenSource { get; set; }

		public bool SilentCompanyLookupCompleted { get; set; }

		public bool ErrorOccur => !string.IsNullOrWhiteSpace(ErrorMessage);

		public string ErrorMessage
		{
			get => errorMessage;
			set
			{
				errorMessage = value;
				IsLoading = false;
				CreateDummyDataForShowErrorMessage();
				NotifyPropertyChanged();
			}
		}
		string errorMessage;

		void CreateDummyDataForShowErrorMessage()
		{
			if (!Events.Any())
			{
				foreach (CreditEventType type in Enum.GetValues(typeof(CreditEventType)))
				{
					Events.Add(new EventItemModel(new CreditEvent { EventDate = CreditEventsFromDate.ToDateTime(), Type = type }));
				}
			}
		}

		public bool IsLoading
		{
			get => isLoading;
			set
			{
				isLoading = value;
				NotifyPropertyChanged();
			}
		}
		bool isLoading;

		public void SetEvents(IEnumerable<CreditEvent> eventItemModels)
		{
			IsLoading = false;
			ErrorMessage = null;
			Events.Clear();
			foreach (var model in eventItemModels)
			{
				Events.Add(new EventItemModel(model));
			}
			NotifyPropertyChanged(nameof(Events));
		}

		public async Task LoadCreditEventsOrSilentCompanyLookup()
		{
			if (creditReportControl.Identifiers.Any(x => x.Type == IdentifierType.DUNS && !string.IsNullOrWhiteSpace(x.ID)))
			{
				await LoadCreditEvents();
			}
			else
			{
				try
				{
					TokenSource = new CancellationTokenSource();
					var silentCompanyLookupResult = await BackgroundCompanyLookup();
					if (TokenSource != null && !TokenSource.IsCancellationRequested && silentCompanyLookupResult.ResultCode == ResultCode.Successful)
					{
						var companyItems = silentCompanyLookupResult.CompanyItems;
						if (companyItems != null && companyItems.Count() == 1)
						{
							if (creditReportControl.SaveIdentifiers(companyItems.First().Identifiers, true))
							{
								if (creditReportControl.Identifiers.Any(x => x.Type == IdentifierType.DUNS && !string.IsNullOrWhiteSpace(x.ID)))
								{
									await LoadCreditEvents();
								}
							}
						}
					}
				}
				catch (Exception)
				{
					// ignore all exceptions when Silent Company Lookup
				}
				finally
				{
					SilentCompanyLookupCompleted = true;
				}
			}
		}

		async Task LoadCreditEvents()
		{
			IsLoading = true;

			try
			{
				var result = await service.GetRetrospectiveMonitorAsync(RetrospectiveMonitorRequest);
				if (result.ResultCode == ResultCode.Successful)
				{
					if (result.Events != null && result.Events.Any())
					{
						SetEvents(result.Events.OrderByDescending(x => x.EventDate).ThenBy(x => x.Type));
						OriginalEventItemModels = result.Events.ToList();
					}
					else
					{
						ErrorMessage = ResourceStringHelper.EventsUnavailable;
					}
				}
				else
				{
					ErrorMessage = result.ErrorInfo.ToLocalizedMessage();
				}
			}
			catch (Exception e)
			{
				ErrorMessage = e.Message;
			}
			finally
			{
				if (isLoading)
				{
					IsLoading = false;
				}

				Parent.ReportsModel.SetReportItemToolTip();
			}
		}

		RetrospectiveMonitorRequest RetrospectiveMonitorRequest => new RetrospectiveMonitorRequest
		{
			Country = header.CountryCode,
			Identifiers = creditReportControl.Identifiers,
			Name = header.OH_FullName,
			LicenseCode = creditReportControl.EnterpriseCode,
			CreditEventQueryInfo = CreditEventQueryInfo
		};

		public IEnumerable<CreditEventQueryInfo> CreditEventQueryInfo
		{
			get
			{
				var availableCreditReportTypes = creditReportControl.AvailableCreditReportTypes;
				var availableCreditEventTypes = CreditReportEventsMapping.GetCreditEventTypes(availableCreditReportTypes).ToList();
				var resultList = new List<CreditEventQueryInfo>();

				foreach (var availableCreditEventType in availableCreditEventTypes)
				{
					resultList.Add(new CreditEventQueryInfo() { CreditEventType = availableCreditEventType, FromDate = CreditEventsFromDate.ToDateTime(), ToDate = CreditEventsToDate.ToDateTime() });
				}

				return resultList.OrderBy(x => (int)x.CreditEventType);
			}
		}

		public void FilterEvents(IEnumerable<CreditEvent> creditEvents)
		{
			if (creditEvents != null && creditEvents.Any())
			{
				SetEvents(creditEvents);
			}
		}

		public void RestoreEvents()
		{
			SetEvents(OriginalEventItemModels);
		}

		public async Task<CompanyLookupResponse> BackgroundCompanyLookup()
		{
			var mainAddress = header.MainAddress;
			var request = new CompanyLookupRequest
			{
				OrgPK = creditReportControl.EntityPk,
				Name = header.OH_FullName,
				Address1 = mainAddress.Address1,
				Address2 = mainAddress.Address2,
				City = mainAddress.City,
				Postcode = mainAddress.Postcode,
				State = mainAddress.StateCode,
				Country = header.Country?.RN_IsoAlpha3Code,
				Identifiers = creditReportControl.Identifiers,
				CallingMode = CallingMode.Background,
				LicenseCode = GlbCompany.CurrentCompany.LicenceKeyIdentifier
			};

			return await service.CompanyLookupAsync(request, TokenSource.Token);
		}
	}
}
