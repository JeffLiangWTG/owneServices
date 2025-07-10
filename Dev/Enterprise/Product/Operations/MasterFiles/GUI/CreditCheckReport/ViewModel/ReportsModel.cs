using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WTG.CreditCheck;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI
{
	public class ReportsModel : ModelBase<ReportsModel>
	{
		public ReportsModel(List<ReportItemModel> reportItemModels, CreditReportUserControl supportCreditCheck, ICreditCheckService creditCheckService, MainPageModel parent)
		{
			this.supportCreditCheck = supportCreditCheck;
			this.creditCheckService = creditCheckService;
			this.parent = parent;
			Reports = GetReports(reportItemModels);
		}

		readonly CreditReportUserControl supportCreditCheck;

		readonly ICreditCheckService creditCheckService;

		readonly MainPageModel parent;

		public List<CreditEvent> TotalLatestEvents { get; set; } = new List<CreditEvent>();

		ObservableCollection<ReportItemInfoModel> GetReports(List<ReportItemModel> reportItemModels)
		{
			var result = new ObservableCollection<ReportItemInfoModel>();
			for (var i = 0; i < reportItemModels.Count; i++)
			{
				result.Add(i == reportItemModels.Count - 1
					? new ReportItemInfoModel(reportItemModels[i], supportCreditCheck, creditCheckService, parent) { GetReportLineVisible = false }
					: new ReportItemInfoModel(reportItemModels[i], supportCreditCheck, creditCheckService, parent));
			}

			return result;
		}

		public ObservableCollection<ReportItemInfoModel> Reports { get; }

		public void SetReportItemToolTip()
		{
			if (!parent.EventsBannerModel.ErrorOccur && supportCreditCheck != null && !supportCreditCheck.IsDisposed)
			{
				TotalLatestEvents.Clear();

				var totalCreditEvents = parent.EventsBannerModel.Events.Select(x => x.Model);
				var purchasedReports = supportCreditCheck.PurchasedReports;
				var availableCreditReportTypes = supportCreditCheck.AvailableCreditReportTypes;
				var availableCreditEventTypes = CreditReportEventsMapping.GetCreditEventTypes(availableCreditReportTypes);
				var queryInfos = new List<CreditEventQueryInfo>();

				foreach (var creditEventType in availableCreditEventTypes)
				{
					queryInfos.Add(new CreditEventQueryInfo() { CreditEventType = creditEventType, FromDate = parent.EventsBannerModel.CreditEventsFromDate.ToDateTime() });
				}

				if (purchasedReports != null)
				{
					foreach (var purchaseInfo in purchasedReports)
					{
						var purchaseReportType = purchaseInfo.ReportType;
						var purchaseCreditEventTypes = CreditReportEventsMapping.GetCreditEventTypes(new[] { purchaseReportType });
						var lastGetReportDate = purchaseInfo.LastGetReportDate;

						foreach (var creditEventType in purchaseCreditEventTypes)
						{
							if (queryInfos.Any(x => x.CreditEventType == creditEventType))
							{
								var result = queryInfos.Single(x => x.CreditEventType == creditEventType);
								if (result.FromDate < lastGetReportDate)
								{
									result.FromDate = lastGetReportDate;
								}
							}
						}
					}
				}

				foreach (var creditEvent in totalCreditEvents)
				{
					foreach (var queryInfo in queryInfos)
					{
						if (creditEvent.Type == queryInfo.CreditEventType && creditEvent.EventDate.Date > queryInfo.FromDate.Date)
						{
							TotalLatestEvents.Add(creditEvent);
						}
					}
				}

				foreach (var report in Reports)
				{
					report.RelatedLatestEvents = TotalLatestEvents.Where(x => CreditReportEventsMapping.GetCreditEventTypes(new[] { report.CreditReportType }).Contains(x.Type)).OrderByDescending(x => x.EventDate);
				}

				parent.TopBannerModel.UpdateStatusTypeAndEventsCount(TotalLatestEvents.GroupBy(x => new { x.Type, x.EventDate }).Select(y => y.First()).ToList());
			}
		}

		public void RestoreEvents()
		{
			parent.EventsBannerModel.RestoreEvents();
		}
	}
}
