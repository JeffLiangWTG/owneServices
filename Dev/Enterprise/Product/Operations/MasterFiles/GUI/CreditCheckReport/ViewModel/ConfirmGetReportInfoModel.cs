using System.Collections.ObjectModel;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI
{
	public class ConfirmGetReportInfoModel
	{
		public ConfirmGetReportInfoModel(ConfirmGetReportModel model)
		{
			IsGet = model.IsGet;
			ReportType = model.ReportType;
			CompanyName = model.CompanyName;
			CompanyAddress = model.CompanyAddress;
			City = model.City;
			CountryState = model.CountryState;
			PostCode = model.PostCode;
			Identifiers = new ObservableCollection<Identifier>(model.Identifiers);
		}

		#region VM UI Caption String Part

		public string Title => ResourceStringHelper.GetTitleForConfirmGetReport(IsGet, ReportType);
		public string OperationDetail => ResourceStringHelper.GetOperationDetailForConfirmGetReport(ReportType);
		public string Cancel => ResourceStringHelper.Cancel;
		public string ActionName => ResourceStringHelper.GetActionNameForConfirmGetReport(IsGet);
		public string CityStatePostInfo => $"{City} {CountryState} {PostCode}";

		#endregion

		#region VM Data Part

		public string CompanyName { get; }
		public bool IsGet { get; }
		public CreditReportType ReportType { get; }
		public string CompanyAddress { get; }
		public string City { get; }
		public string CountryState { get; }
		public string PostCode { get; }
		public ObservableCollection<Identifier> Identifiers { get; set; }

		#endregion

	}
}
