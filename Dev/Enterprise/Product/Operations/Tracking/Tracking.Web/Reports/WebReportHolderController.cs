using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Tracking.Web
{
	public class WebReportHolderController
	{
		public WebReportHolderController(WebReportHolder source)
		{
			DataSource = source;
		}

		readonly WebReportHolder DataSource;

		public WebReportModes ReportMode(ZString contentType)
		{
			WebReportModes value;
			if (!Enum.TryParse(contentType, out value))
			{
				value = WebReportModes.All;
			}

			return value;
		}

		public string ReportsCaption(WebReportModes mode)
		{
			string caption = "";
			switch (mode)
			{
				case WebReportModes.Freight:
					caption = Res.GetString("a1564ba9-788a-4953-a129-2cf4f2609473", "Forwarding");
					break;
				case WebReportModes.LinerAgency:
					caption = Res.GetString("082978e9-8d09-4171-9cdb-c8bf7ee92471", "Liner & Agency");
					break;
				case WebReportModes.Customs:
					caption = Res.GetString("3729c53e-6185-4208-8001-0bd641b5715b", "Customs");
					break;
				case WebReportModes.Warehouse:
					caption = Res.GetString("d9ca1202-9b5e-4d85-8b96-c004a0ebabd6", "Warehouse");
					break;
				case WebReportModes.Transport:
					caption = Res.GetString("4c1194ad-a0e0-4b23-a2fc-4171be5bac43", "Transport");
					break;
				default:
					break;
			}
			return Res.GetString("85cfb44c-3692-4226-a7cb-6d4094e3b450", "{0} Reports", caption.Trim()).Trim();
		}

		public void UpdateBindingMembers(WebReportModes mode)
		{
			DataSource.Modes = GetBusinessContextList(mode);
		}

		List<ZString> GetBusinessContextList(WebReportModes mode)
		{
			List<ZString> result = new List<ZString>();
			switch (mode)
			{
				case WebReportModes.Freight:
					result.Add(nameof(ModuleId.FreightReport));
					result.Add(nameof(ModuleId.OrdersReport));
					break;
				case WebReportModes.LinerAgency:
					result.Add(nameof(ModuleId.AgencyReports));
					break;
				case WebReportModes.Customs:
					result.Add(nameof(ModuleId.CustomsReport));
					break;
				case WebReportModes.Warehouse:
					result.Add(nameof(ModuleId.WhsReport));
					break;
				case WebReportModes.Transport:
					result.Add(nameof(ModuleId.TransportReports));
					break;
				default:
					break;
			}
			return result;
		}
	}
}
