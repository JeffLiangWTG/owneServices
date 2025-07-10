using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using WTG.CreditCheck;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CreditCheckUserControl : UserControl
	{
		public CreditCheckUserControl()
		{
			InitializeComponent();
		}

		public void Initialize(CreditReportUserControl creditReportControl)
		{
			ZUserControl userControl;
			if (creditReportControl.EntityPk == Guid.Empty)
			{
				userControl = new ComingSoonControl();
				userControl.SetDataBinding(new UnavailableModel(), string.Empty);
			}
			else
			{
				if (IsCreditReportAvailable(creditReportControl))
				{
					var mainPageModel = GetMainPageModel(creditReportControl);
					userControl = new MainPageControl();
					userControl.SetDataBinding(mainPageModel, string.Empty);
				}
				else
				{
					userControl = new ComingSoonControl();
					userControl.SetDataBinding(new ComingSoonModel(creditReportControl.Header.CountryCode), string.Empty);
				}
			}

			userControl.Dock = DockStyle.Fill;

			CreditCheckMainPageControlPanel.Controls.RemoveAndDisposeAll();
			CreditCheckMainPageControlPanel.Controls.Add(userControl);
			CreditCheckMainPageControlPanel.Visible = true;
		}

		bool IsCreditReportAvailable(CreditReportUserControl creditReportControl)
		{
			return creditReportControl.AvailableCountries.Contains((string)creditReportControl.Header?.CountryCode);
		}

		MainPageModel GetMainPageModel(CreditReportUserControl creditReportControl)
		{
			var mainPageModel = new MainPageModel
			{
				TopBannerModel = new TopBannerModel(CreditReportStatusType.NoEvent, 0, creditReportControl.Header.OH_FullName)
			};

			InitializeCreditCheckService(creditReportControl);
			mainPageModel.ReportsModel = GetReportModel(creditReportControl, mainPageModel);
			mainPageModel.EventsBannerModel = new EventsBannerModel(creditReportControl, creditCheckService, mainPageModel);
			return mainPageModel;
		}

		ReportsModel GetReportModel(CreditReportUserControl creditReportControl, MainPageModel mainPageModel)
		{
			var reportItemModelList = GetReportItemModelList(creditReportControl);

			return new ReportsModel(reportItemModelList.OrderBy(u => u.CreditReportType).ToList(), creditReportControl, creditCheckService, mainPageModel);
		}

		void InitializeCreditCheckService(CreditReportUserControl creditReportControl)
		{
			if (creditCheckService == null)
			{
				creditCheckService = new CreditCheckService(creditReportControl.ServiceWrapper);
				creditCheckService.OnCertificateMismatched += ClearCreditReportsPublicCertificate;
			}
		}

		void ClearCreditReportsPublicCertificate(object sender, EventArgs args)
		{
			OrganisationsDataRegistry.Instance.CreditReportsPublicCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<byte>());
		}

		List<ReportItemModel> GetReportItemModelList(CreditReportUserControl creditReportControl)
		{
			var reportItemModelList = new List<ReportItemModel>();

			foreach (var reportType in creditReportControl.AvailableCreditReportTypes)
			{
				var purchaseInfo = creditReportControl.PurchasedReports?.FirstOrDefault(u => u.ReportType == reportType) ?? default;
				if (purchaseInfo != default)
				{
					reportItemModelList.Add(new ReportItemModel(reportType)
					{
						LastReportDate = purchaseInfo.LastGetReportDate,
					});
				}
				else
				{
					reportItemModelList.Add(new ReportItemModel(reportType));
				}
			}

			return reportItemModelList;
		}

		ICreditCheckService creditCheckService;
	}
}
