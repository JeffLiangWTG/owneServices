using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Web
{
	public class WebReportHolder : NonPersistentBusinessObject, IObsoleteValidation, IDisposable
	{
		public WebReportHolder(BusinessObjectFactory factory, OrgContactWebUser siteUser)
			: base(factory)
		{
			this.SiteUser = siteUser;
		}

		#region Report PK

		public ZGuid ReportPK
		{
			get { return reportPK; }
			set
			{
				if (value != reportPK)
				{
					reportPK = value;
					ResetSelectedReport();
					ReportPKInfo.RefreshBinding();
				}
			}
		}

		void ResetSelectedReport()
		{
			if (selectedReport != null)
			{
				selectedReport.Dispose();
			}

			selectedReport = null;
		}
		ZGuid reportPK;

		public ZPropertyInfo ReportPKInfo
		{
			get { return GetZPropertyInfo(nameof(ReportPK)); }
		}

		#endregion

		#region Selected Report

		public Report SelectedReport
		{
			get
			{
				if (selectedReport == null)
				{
					ReportCommand command = Factory.Load<ReportCommand>(ReportPK);

					if (command != null)
					{
						ReportPrintSet printset = new ReportPrintSet(command);
						DocumentPack pack = printset[0];
						selectedReport = pack.GetFirstReport();

						if (selectedReport != null)
						{
							selectedReport.PrepareForRender(); // populates the collection of filters
						}
					}
				}

				return selectedReport;
			}
		}

		Report selectedReport;

		#endregion

		#region SIteUser

		public OrgContactWebUser SiteUser
		{
			get { return fSiteUser; }
			set { fSiteUser = value; }
		}
		protected OrgContactWebUser fSiteUser;

		#endregion

		#region Web Reports Collection

		public List<ZString> Modes
		{
			get
			{
				if (fMode == null)
				{
					fMode = new List<ZString>();
				}
				return fMode;
			}
			set
			{
				if (value != fMode)
				{
					fMode = value;
					LoadWebReports();
				}
			}
		}
		List<ZString> fMode;

		public WebReportCommandCollection WebReports
		{
			get
			{
				if (webReports == null)
				{
					LoadWebReports();
				}

				return webReports;
			}
		}

		void LoadWebReports()
		{
			webReports = WebReportHelper.GetAvailableWebReports(Factory, Modes, SiteUser.IsLoggedIn, SiteUser.AreSecurityRightsGranted);
		}

		WebReportCommandCollection webReports;

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (selectedReport != null)
			{
				selectedReport.Dispose();
			}
		}

		#endregion
	}
}
