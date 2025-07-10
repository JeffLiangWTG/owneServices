using System.Collections.Generic;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI
{
	public class ConfirmGetReportModel
	{
		public bool IsGet { get; set; }
		public CreditReportType ReportType { get; set; }
		public string CompanyName { get; set; }
		public string CompanyAddress { get; set; }
		public string City { get; set; }
		public string CountryState { get; set; }
		public string PostCode { get; set; }
		public IEnumerable<Identifier> Identifiers { get; set; }
	}
}
